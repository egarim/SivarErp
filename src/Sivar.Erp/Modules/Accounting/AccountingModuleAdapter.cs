using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting.BalanceCalculators;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Services.Accounting.Transactions;

namespace Sivar.Erp.Modules.Accounting
{
    /// <summary>
    /// Adapter that implements IAccountingModule using the existing AccountingModule class
    /// This allows other modules to use the accounting functionality through a well-defined interface
    /// without changing the original AccountingModule implementation.
    /// </summary>
    public class AccountingModuleAdapter : IAccountingModule
    {
        private readonly AccountingModule _accountingModule;
        private readonly ILogger<AccountingModuleAdapter> _logger;
        private readonly PerformanceLogger<AccountingModuleAdapter> _performanceLogger;
        private readonly IObjectDb _objectDb;

        public AccountingModuleAdapter(
            AccountingModule accountingModule,
            ILogger<AccountingModuleAdapter> logger,
            IObjectDb objectDb = null)
        {
            _accountingModule = accountingModule ?? throw new ArgumentNullException(nameof(accountingModule));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _objectDb = objectDb;
            _performanceLogger = new PerformanceLogger<AccountingModuleAdapter>(_logger, PerformanceLogMode.All, 100, 10_000_000, _objectDb);
        }

        /// <summary>
        /// Creates a transaction from a document with accounting entries based on document totals
        /// </summary>
        /// <param name="document">The source document for the transaction</param>
        /// <param name="description">Optional description for the transaction</param>
        /// <returns>A transaction ready for posting</returns>
        public async Task<ITransaction> CreateTransactionFromDocumentAsync(IDocument document, string description = null)
        {
            return await _performanceLogger.Track(nameof(CreateTransactionFromDocumentAsync), async () =>
            {
                if (document == null)
                    throw new ArgumentNullException(nameof(document));

                // Create a new transaction
                var transaction = new TransactionDto
                {
                    TransactionDate = document.Date,
                    Description = description ?? $"Document {document.DocumentType.Name} #{document.DocumentNumber}",
                    DocumentNumber = document.DocumentNumber,
                    LedgerEntries = new List<ILedgerEntry>()
                };

                // Generate ledger entries from document totals
                if (document.DocumentTotals != null)
                {
                    foreach (var total in document.DocumentTotals)
                    {
                        // Only create entries for totals marked for inclusion in transactions
                        if (total.IncludeInTransaction)
                        {
                            // Create debit entry if applicable
                            if (!string.IsNullOrEmpty(total.DebitAccountCode) && total.Total > 0)
                            {
                                var debitEntry = new LedgerEntryDto
                                {
                                    OfficialCode = total.DebitAccountCode,
                                    EntryType = EntryType.Debit,
                                    Amount = total.Total,
                                    AccountName = total.Concept
                                };
                                ((List<ILedgerEntry>)transaction.LedgerEntries).Add(debitEntry);
                            }

                            // Create credit entry if applicable
                            if (!string.IsNullOrEmpty(total.CreditAccountCode) && total.Total > 0)
                            {
                                var creditEntry = new LedgerEntryDto
                                {
                                    OfficialCode = total.CreditAccountCode,
                                    EntryType = EntryType.Credit,
                                    Amount = total.Total,
                                    AccountName = total.Concept
                                };
                                ((List<ILedgerEntry>)transaction.LedgerEntries).Add(creditEntry);
                            }
                        }
                    }
                }

                return transaction;
            });
        }

        /// <summary>
        /// Posts a transaction after validating fiscal period is open and transaction is balanced
        /// </summary>
        /// <param name="transaction">Transaction to post</param>
        /// <returns>True if posted successfully</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period is closed or transaction is invalid</exception>
        public async Task<bool> PostTransactionAsync(ITransaction transaction)
        {
            return await _accountingModule.PostTransactionAsync(transaction);
        }

        /// <summary>
        /// Unposts a previously posted transaction if fiscal period is still open
        /// </summary>
        /// <param name="transaction">Transaction to unpost</param>
        /// <returns>True if unposted successfully</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period is closed</exception>
        public async Task<bool> UnPostTransactionAsync(ITransaction transaction)
        {
            return await _accountingModule.UnPostTransactionAsync(transaction);
        }

        /// <summary>
        /// Checks if a transaction is balanced (total debits = total credits) and valid for posting
        /// </summary>
        /// <param name="transaction">Transaction to validate</param>
        /// <returns>True if the transaction is valid</returns>
        public async Task<bool> ValidateTransactionAsync(ITransaction transaction)
        {
            return await _performanceLogger.Track(nameof(ValidateTransactionAsync), async () =>
            {
                if (transaction == null)
                    throw new ArgumentNullException(nameof(transaction));

                // Delegate to the transaction's validation method
                return await transaction.ValidateTransactionAsync();
            });
        }

        /// <summary>
        /// Gets the balance of an account as of a specific date
        /// </summary>
        /// <param name="accountCode">Account code to query</param>
        /// <param name="asOfDate">Date for which to get the balance</param>
        /// <returns>The account balance</returns>
        public async Task<decimal> GetAccountBalanceAsync(string accountCode, DateOnly asOfDate)
        {
            return await _performanceLogger.Track(nameof(GetAccountBalanceAsync), async () =>
            {
                if (string.IsNullOrEmpty(accountCode))
                    throw new ArgumentException("Account code cannot be null or empty", nameof(accountCode));

                // Use the account balance calculator to get the balance
                return _accountingModule.AccountBalanceCalculator.CalculateAccountBalance(accountCode, asOfDate);
            });
        }

        /// <summary>
        /// Opens a fiscal period to allow transaction posting
        /// </summary>
        /// <param name="periodCode">Code of the fiscal period to open</param>
        /// <param name="userId">User opening the period</param>
        /// <returns>True if opened successfully</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period doesn't exist</exception>
        public async Task<bool> OpenFiscalPeriodAsync(string periodCode, string userId)
        {
            return await _performanceLogger.Track(nameof(OpenFiscalPeriodAsync), async () =>
            {
                if (string.IsNullOrEmpty(periodCode))
                    throw new ArgumentNullException(nameof(periodCode));
                    
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentNullException(nameof(userId));

                // We need to search all fiscal periods as the period could be in either state
                // First try closed periods since that's what we're looking to open
                var closedFiscalPeriods = await _accountingModule.GetFiscalPeriodService().GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Closed);
                var fiscalPeriod = closedFiscalPeriods.FirstOrDefault(fp => string.Equals(fp.Code, periodCode, StringComparison.OrdinalIgnoreCase));
                
                if (fiscalPeriod == null)
                {
                    // If not found in closed periods, check open periods as well (maybe it's already open)
                    var openFiscalPeriods = await _accountingModule.GetFiscalPeriodService().GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Open);
                    fiscalPeriod = openFiscalPeriods.FirstOrDefault(fp => string.Equals(fp.Code, periodCode, StringComparison.OrdinalIgnoreCase));
                }
                
                if (fiscalPeriod == null)
                    throw new InvalidOperationException($"Fiscal period with code {periodCode} not found");
                    
                // Only update if needed
                if (fiscalPeriod.Status == FiscalPeriodStatus.Closed)
                {
                    // Open the fiscal period
                    fiscalPeriod.Status = FiscalPeriodStatus.Open;
                    fiscalPeriod.UpdatedBy = userId;
                    fiscalPeriod.UpdatedAt = DateTime.UtcNow;
                }
                
                return true;
            });
        }

        /// <summary>
        /// Closes a fiscal period to prevent further transaction posting
        /// </summary>
        /// <param name="periodCode">Code of the fiscal period to close</param>
        /// <param name="userId">User closing the period</param>
        /// <returns>True if closed successfully</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period doesn't exist</exception>
        public async Task<bool> CloseFiscalPeriodAsync(string periodCode, string userId)
        {
            return await _performanceLogger.Track(nameof(CloseFiscalPeriodAsync), async () =>
            {
                if (string.IsNullOrEmpty(periodCode))
                    throw new ArgumentNullException(nameof(periodCode));
                    
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentNullException(nameof(userId));

                // We need to search all fiscal periods as the period could be in either state
                // First try open periods since that's what we're looking to close
                var openFiscalPeriods = await _accountingModule.GetFiscalPeriodService().GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Open);
                var fiscalPeriod = openFiscalPeriods.FirstOrDefault(fp => string.Equals(fp.Code, periodCode, StringComparison.OrdinalIgnoreCase));
                
                if (fiscalPeriod == null)
                {
                    // If not found in open periods, check closed periods as well (maybe it's already closed)
                    var closedFiscalPeriods = await _accountingModule.GetFiscalPeriodService().GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Closed);
                    fiscalPeriod = closedFiscalPeriods.FirstOrDefault(fp => string.Equals(fp.Code, periodCode, StringComparison.OrdinalIgnoreCase));
                }
                
                if (fiscalPeriod == null)
                    throw new InvalidOperationException($"Fiscal period with code {periodCode} not found");
                    
                // Only update if needed
                if (fiscalPeriod.Status == FiscalPeriodStatus.Open)
                {
                    // Close the fiscal period
                    fiscalPeriod.Status = FiscalPeriodStatus.Closed;
                    fiscalPeriod.UpdatedBy = userId;
                    fiscalPeriod.UpdatedAt = DateTime.UtcNow;
                }
                
                return true;
            });
        }

        /// <summary>
        /// Checks if a date falls within an open fiscal period
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if date is within an open fiscal period, false otherwise</returns>
        public async Task<bool> IsDateInOpenFiscalPeriodAsync(DateOnly date)
        {
            return await _accountingModule.IsDateInOpenFiscalPeriodAsync(date);
        }

        /// <summary>
        /// Gets the fiscal period service for advanced operations
        /// </summary>
        /// <returns>The fiscal period service</returns>
        public IFiscalPeriodService GetFiscalPeriodService()
        {
            return _accountingModule.GetFiscalPeriodService();
        }
    }
}