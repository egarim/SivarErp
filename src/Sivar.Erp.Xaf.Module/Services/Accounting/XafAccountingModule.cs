using DevExpress.ExpressApp;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Enums;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.Modules;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Modules.Accounting;
using Sivar.Erp.Modules.Accounting.BalanceCalculators;
using Sivar.Erp.Modules.Accounting.FiscalPeriods;
using Sivar.Erp.Modules.Accounting.JournalEntries;
using Sivar.Erp.Modules.Accounting.Reports;
using Sivar.Erp.Modules.Accounting.Transactions;
using Sivar.Erp.Modules.Documents.Core.Interfaces;
using Sivar.Erp.Xaf.Module.Services.BalanceCalculators;
using Sivar.Erp.Xaf.Module.Services.FiscalPeriods;
using Sivar.Erp.Xaf.Module.Services.JournalEntries;
using Sivar.Erp.Xaf.Module.Services.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.Accounting
{
    /// <summary>
    /// XAF implementation of accounting module using IObjectSpace and DevExpress XAF services
    /// </summary>
    public class XafAccountingModule : ErpModuleBase, IAccountingModule
    {
        private readonly IObjectSpace _objectSpace;
        private readonly XafFiscalPeriodService _fiscalPeriodService;
        private readonly XafAccountBalanceCalculatorService _accountBalanceCalculator;
        private readonly XafJournalEntryService _journalEntryService;
        private readonly XafJournalEntryReportService _reportService;
        private readonly ILogger<XafAccountingModule>? _logger;

        private const string TRANSACTION_SEQUENCE_CODE = "TRANS";
        private const string BATCH_SEQUENCE_CODE = "BATCH";
        private const string FISCAL_SEQUENCE_CODE = "FISCAL";
        private const string LEDGERENTRY_SEQUENCE_CODE = "LEDGERENTRY";

        /// <summary>
        /// Gets the account balance calculator used by this module
        /// </summary>
        public IAccountBalanceCalculator AccountBalanceCalculator => (IAccountBalanceCalculator)_accountBalanceCalculator;

        /// <summary>
        /// Initializes a new instance of the XafAccountingModule class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="optionService">Option service</param>
        /// <param name="activityStreamService">Activity stream service</param>
        /// <param name="dateTimeZoneService">Date/time zone service</param>
        /// <param name="sequencerService">Sequencer service</param>
        /// <param name="logger">Optional logger</param>
        public XafAccountingModule(
            IObjectSpace objectSpace,
            IOptionService optionService,
            IActivityStreamService activityStreamService,
            IDateTimeZoneService dateTimeZoneService,
            ISequencerService sequencerService,
            ILogger<XafAccountingModule>? logger = null)
            : base(optionService, activityStreamService, dateTimeZoneService, sequencerService)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _logger = logger;

            // Initialize XAF services with null loggers since they need specific types
            _fiscalPeriodService = new XafFiscalPeriodService(_objectSpace, null);
            _accountBalanceCalculator = new XafAccountBalanceCalculatorService(_objectSpace, null);
            _journalEntryService = new XafJournalEntryService(_objectSpace, null);
            _reportService = new XafJournalEntryReportService(_objectSpace, _journalEntryService, null);
        }

        /// <summary>
        /// Registers accounting sequences in the system
        /// </summary>
        /// <param name="sequenceDtos">Collection of sequence DTOs to register</param>
        public override void RegisterSequence(IEnumerable<SequenceDto> sequenceDtos)
        {
            try
            {
                // Transaction sequence
                var transactionSequence = new SequenceDto
                {
                    Code = TRANSACTION_SEQUENCE_CODE,
                    CurrentNumber = 1,
                    Name = "Accounting Transactions",
                    Prefix = "T",
                    Suffix = "S"
                };

                // Ledger entry sequence
                var ledgerEntrySequence = new SequenceDto
                {
                    Code = LEDGERENTRY_SEQUENCE_CODE,
                    CurrentNumber = 1,
                    Name = "Ledger Entries",
                    Prefix = "LE",
                    Suffix = "S"
                };

                // Batch sequence
                var batchSequence = new SequenceDto
                {
                    Code = BATCH_SEQUENCE_CODE,
                    CurrentNumber = 1,
                    Name = "Batch",
                    Prefix = "B",
                    Suffix = "S"
                };

                // Fiscal period sequence
                var fiscalPeriodSequence = new SequenceDto
                {
                    Code = FISCAL_SEQUENCE_CODE,
                    CurrentNumber = 1,
                    Name = "Fiscal Period",
                    Prefix = "FP",
                    Suffix = "S"
                };

                // Register sequences using the sequencer service
                sequencerService.CreateSequenceAsync(transactionSequence);
                sequencerService.CreateSequenceAsync(ledgerEntrySequence);
                sequencerService.CreateSequenceAsync(batchSequence);
                sequencerService.CreateSequenceAsync(fiscalPeriodSequence);

                _logger?.LogInformation("Accounting sequences registered successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error registering accounting sequences");
                throw;
            }
        }

        /// <summary>
        /// Creates a transaction from a document with accounting entries based on document totals
        /// </summary>
        /// <param name="document">The source document for the transaction</param>
        /// <param name="description">Optional description for the transaction</param>
        /// <returns>A transaction ready for posting</returns>
        public async Task<ITransaction> CreateTransactionFromDocumentAsync(IDocument document, string? description = null)
        {
            try
            {
                _logger?.LogDebug("Creating transaction from document: {DocumentNumber}", document?.DocumentNumber);

                if (document == null)
                    throw new ArgumentNullException(nameof(document));

                // Create a new transaction
                var transaction = new TransactionDto
                {
                    TransactionDate = document.Date,
                    Description = description ?? $"Document {document.DocumentType?.Name} #{document.DocumentNumber}",
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
                                    AccountName = total.Concept ?? string.Empty
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
                                    AccountName = total.Concept ?? string.Empty
                                };
                                ((List<ILedgerEntry>)transaction.LedgerEntries).Add(creditEntry);
                            }
                        }
                    }
                }

                _logger?.LogDebug("Created transaction with {Count} ledger entries", transaction.LedgerEntries?.Count() ?? 0);

                return transaction;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating transaction from document");
                throw;
            }
        }

        /// <summary>
        /// Posts a transaction after validating fiscal period is open
        /// </summary>
        /// <param name="transaction">Transaction to post</param>
        /// <returns>True if posted successfully, false otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period is closed</exception>
        public async Task<bool> PostTransactionAsync(ITransaction transaction)
        {
            try
            {
                _logger?.LogDebug("Posting transaction: {TransactionNumber}", transaction?.TransactionNumber);

                if (transaction == null)
                    throw new ArgumentNullException(nameof(transaction));

                if (transaction.IsPosted)
                {
                    _logger?.LogDebug("Transaction {TransactionNumber} is already posted", transaction.TransactionNumber);
                    return true; // Already posted
                }

                // Validate that transaction is in an open fiscal period
                var fiscalPeriod = await _fiscalPeriodService.GetFiscalPeriodForDateAsync(transaction.TransactionDate);

                if (fiscalPeriod == null)
                    throw new InvalidOperationException($"No fiscal period found for date {transaction.TransactionDate}");

                if (fiscalPeriod.Status == FiscalPeriodStatus.Closed)
                    throw new InvalidOperationException($"Cannot post transaction: Fiscal period '{fiscalPeriod.Name}' is closed");

                // Validate transaction balance (debits = credits)
                bool isValid = await ValidateTransactionAsync(transaction);

                if (!isValid)
                    throw new InvalidOperationException("Transaction has unbalanced debits and credits");

                // Generate transaction number if not already set
                if (string.IsNullOrEmpty(transaction.TransactionNumber))
                {
                    transaction.TransactionNumber = await sequencerService.GetNextNumberAsync(TRANSACTION_SEQUENCE_CODE);
                }

                // Generate ledger entry numbers
                if (transaction.LedgerEntries != null)
                {
                    foreach (var ledgerEntry in transaction.LedgerEntries)
                    {
                        if (string.IsNullOrEmpty(ledgerEntry.LedgerEntryNumber))
                        {
                            ledgerEntry.LedgerEntryNumber = await sequencerService.GetNextNumberAsync(LEDGERENTRY_SEQUENCE_CODE);
                        }
                        
                        // Set the transaction number for each ledger entry
                        ledgerEntry.TransactionNumber = transaction.TransactionNumber;
                    }
                }

                // Post the transaction using XAF ObjectSpace
                await PostTransactionToXafAsync(transaction);

                // Log the activity
                var systemActor = CreateSystemStreamObject();
                var transactionTarget = CreateStreamObject(
                    "Transaction",
                    transaction.TransactionNumber,
                    $"Transaction {transaction.TransactionNumber} on {transaction.TransactionDate}");

                await RecordActivityAsync(
                    systemActor,
                    "Posted",
                    transactionTarget);

                _logger?.LogDebug("Successfully posted transaction: {TransactionNumber}", transaction.TransactionNumber);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error posting transaction: {TransactionNumber}", transaction?.TransactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Unposts a previously posted transaction if fiscal period is still open
        /// </summary>
        /// <param name="transaction">Transaction to unpost</param>
        /// <returns>True if unposted successfully</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period is closed</exception>
        public async Task<bool> UnPostTransactionAsync(ITransaction transaction)
        {
            try
            {
                _logger?.LogDebug("Unposting transaction: {TransactionNumber}", transaction?.TransactionNumber);

                if (transaction == null)
                    throw new ArgumentNullException(nameof(transaction));

                if (!transaction.IsPosted)
                {
                    _logger?.LogDebug("Transaction {TransactionNumber} is already unposted", transaction.TransactionNumber);
                    return true; // Already unposted
                }

                // Validate that transaction is in an open fiscal period
                var fiscalPeriod = await _fiscalPeriodService.GetFiscalPeriodForDateAsync(transaction.TransactionDate);

                if (fiscalPeriod == null)
                    throw new InvalidOperationException($"No fiscal period found for date {transaction.TransactionDate}");

                if (fiscalPeriod.Status == FiscalPeriodStatus.Closed)
                    throw new InvalidOperationException($"Cannot unpost transaction: Fiscal period '{fiscalPeriod.Name}' is closed");

                // Unpost the transaction using XAF ObjectSpace
                await UnpostTransactionFromXafAsync(transaction);

                // Log the activity
                var systemActor = CreateSystemStreamObject();
                var transactionTarget = CreateStreamObject(
                    "Transaction",
                    transaction.TransactionNumber,
                    $"Transaction {transaction.TransactionNumber} on {transaction.TransactionDate}");

                await RecordActivityAsync(
                    systemActor,
                    "Unposted",
                    transactionTarget);

                _logger?.LogDebug("Successfully unposted transaction: {TransactionNumber}", transaction.TransactionNumber);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error unposting transaction: {TransactionNumber}", transaction?.TransactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Checks if a transaction is balanced (total debits = total credits) and valid for posting
        /// </summary>
        /// <param name="transaction">Transaction to validate</param>
        /// <returns>True if the transaction is valid</returns>
        public async Task<bool> ValidateTransactionAsync(ITransaction transaction)
        {
            try
            {
                if (transaction == null)
                    throw new ArgumentNullException(nameof(transaction));

                if (transaction.LedgerEntries == null || !transaction.LedgerEntries.Any())
                    return false;

                // Calculate total debits and credits
                var totalDebits = transaction.LedgerEntries
                    .Where(e => e.EntryType == EntryType.Debit)
                    .Sum(e => e.Amount);

                var totalCredits = transaction.LedgerEntries
                    .Where(e => e.EntryType == EntryType.Credit)
                    .Sum(e => e.Amount);

                // Allow for minor rounding differences
                var isBalanced = Math.Abs(totalDebits - totalCredits) < 0.01m;

                _logger?.LogDebug("Transaction {TransactionNumber} validation: Debits={TotalDebits}, Credits={TotalCredits}, Balanced={IsBalanced}",
                    transaction.TransactionNumber, totalDebits, totalCredits, isBalanced);

                return await Task.FromResult(isBalanced);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating transaction: {TransactionNumber}", transaction?.TransactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Gets the balance of an account as of a specific date
        /// </summary>
        /// <param name="accountCode">Account code to query</param>
        /// <param name="asOfDate">Date for which to get the balance</param>
        /// <returns>The account balance</returns>
        public async Task<decimal> GetAccountBalanceAsync(string accountCode, DateOnly asOfDate)
        {
            try
            {
                _logger?.LogDebug("Getting account balance for {AccountCode} as of {AsOfDate}", accountCode, asOfDate);

                if (string.IsNullOrEmpty(accountCode))
                    throw new ArgumentException("Account code cannot be null or empty", nameof(accountCode));

                return await _accountBalanceCalculator.GetAccountBalanceAsync(accountCode, asOfDate);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting account balance for {AccountCode}", accountCode);
                throw;
            }
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
            try
            {
                _logger?.LogDebug("Opening fiscal period {PeriodCode} by user {UserId}", periodCode, userId);

                if (string.IsNullOrEmpty(periodCode))
                    throw new ArgumentNullException(nameof(periodCode));

                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentNullException(nameof(userId));

                // Get fiscal period by ID/code
                var fiscalPeriod = await _fiscalPeriodService.GetFiscalPeriodByIdAsync(periodCode);
                if (fiscalPeriod == null)
                {
                    throw new InvalidOperationException($"Fiscal period with code {periodCode} not found");
                }

                // If already open, return true
                if (fiscalPeriod.Status == FiscalPeriodStatus.Open)
                {
                    _logger?.LogDebug("Fiscal period {PeriodCode} is already open", periodCode);
                    return true;
                }

                // Open the fiscal period by updating its status
                fiscalPeriod.Status = FiscalPeriodStatus.Open;
                fiscalPeriod.UpdatedBy = userId;
                fiscalPeriod.UpdatedAt = DateTime.UtcNow;

                _objectSpace.CommitChanges();

                _logger?.LogInformation("Fiscal period {PeriodCode} opened successfully by user {UserId}", periodCode, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening fiscal period {PeriodCode}", periodCode);
                throw;
            }
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
            try
            {
                _logger?.LogDebug("Closing fiscal period {PeriodCode} by user {UserId}", periodCode, userId);

                if (string.IsNullOrEmpty(periodCode))
                    throw new ArgumentNullException(nameof(periodCode));

                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentNullException(nameof(userId));

                // Get fiscal period by ID/code
                var fiscalPeriod = await _fiscalPeriodService.GetFiscalPeriodByIdAsync(periodCode);
                if (fiscalPeriod == null)
                {
                    throw new InvalidOperationException($"Fiscal period with code {periodCode} not found");
                }

                // If already closed, return true
                if (fiscalPeriod.Status == FiscalPeriodStatus.Closed)
                {
                    _logger?.LogDebug("Fiscal period {PeriodCode} is already closed", periodCode);
                    return true;
                }

                // Close the fiscal period by updating its status
                fiscalPeriod.Status = FiscalPeriodStatus.Closed;
                fiscalPeriod.UpdatedBy = userId;
                fiscalPeriod.UpdatedAt = DateTime.UtcNow;

                _objectSpace.CommitChanges();

                _logger?.LogInformation("Fiscal period {PeriodCode} closed successfully by user {UserId}", periodCode, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error closing fiscal period {PeriodCode}", periodCode);
                throw;
            }
        }

        /// <summary>
        /// Checks if a date falls within an open fiscal period
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if date is in an open fiscal period, false otherwise</returns>
        public async Task<bool> IsDateInOpenFiscalPeriodAsync(DateOnly date)
        {
            try
            {
                var fiscalPeriod = await _fiscalPeriodService.GetFiscalPeriodForDateAsync(date);
                return fiscalPeriod?.Status == FiscalPeriodStatus.Open;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if date {Date} is in open fiscal period", date);
                throw;
            }
        }

        /// <summary>
        /// Gets the fiscal period service for advanced operations
        /// </summary>
        /// <returns>The fiscal period service</returns>
        public IFiscalPeriodService GetFiscalPeriodService()
        {
            return _fiscalPeriodService;
        }

        // Journal Entry Operations

        /// <summary>
        /// Gets journal entries for a specific transaction
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>Collection of journal entries</returns>
        public async Task<IEnumerable<ILedgerEntry>> GetTransactionJournalEntriesAsync(string transactionNumber)
        {
            try
            {
                return await _journalEntryService.GetJournalEntriesByTransactionAsync(transactionNumber);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting journal entries for transaction {TransactionNumber}", transactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Validates if a transaction is balanced (total debits = total credits)
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>True if transaction is balanced</returns>
        public async Task<bool> ValidateTransactionBalanceAsync(string transactionNumber)
        {
            try
            {
                return await _journalEntryService.IsTransactionBalancedAsync(transactionNumber);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating transaction balance for {TransactionNumber}", transactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Gets journal entries based on query criteria
        /// </summary>
        /// <param name="options">Query options for filtering</param>
        /// <returns>Collection of journal entries</returns>
        public async Task<IEnumerable<ILedgerEntry>> GetJournalEntriesAsync(JournalEntryQueryOptions options)
        {
            try
            {
                return await _journalEntryService.GetJournalEntriesAsync(options);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting journal entries with options");
                throw;
            }
        }

        /// <summary>
        /// Generates a journal entry report
        /// </summary>
        /// <param name="options">Query options for the report</param>
        /// <returns>Journal entry report data</returns>
        public async Task<JournalEntryReportDto> GenerateJournalReportAsync(JournalEntryQueryOptions options)
        {
            try
            {
                return await _reportService.GenerateJournalEntryReportAsync(options);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating journal report");
                throw;
            }
        }

        /// <summary>
        /// Generates a transaction audit trail
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>Transaction audit trail data</returns>
        public async Task<TransactionAuditTrailDto> GenerateTransactionAuditTrailAsync(string transactionNumber)
        {
            try
            {
                return await _reportService.GenerateTransactionAuditTrailAsync(transactionNumber);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating transaction audit trail for {TransactionNumber}", transactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Initializes the accounting module by creating required sequences
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _logger?.LogDebug("Initializing XAF Accounting Module");

                // Create required sequences for accounting operations
                await CreateAccountingSequencesAsync();

                _logger?.LogDebug("XAF Accounting Module initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing XAF Accounting Module");
                throw;
            }
        }

        /// <summary>
        /// Posts a transaction to XAF ObjectSpace
        /// </summary>
        /// <param name="transaction">Transaction to post</param>
        private async Task PostTransactionToXafAsync(ITransaction transaction)
        {
            await Task.CompletedTask; // Make method async

            // Create XAF Transaction entity
            var xafTransaction = _objectSpace.CreateObject<Transaction>();
            xafTransaction.TransactionNumber = transaction.TransactionNumber;
            xafTransaction.TransactionDate = transaction.TransactionDate;
            xafTransaction.Description = transaction.Description;
            xafTransaction.DocumentNumber = transaction.DocumentNumber;
            xafTransaction.IsPosted = true;

            // Create XAF LedgerEntry entities
            if (transaction.LedgerEntries != null)
            {
                foreach (var ledgerEntry in transaction.LedgerEntries)
                {
                    var xafLedgerEntry = _objectSpace.CreateObject<LedgerEntry>();
                    xafLedgerEntry.LedgerEntryNumber = ledgerEntry.LedgerEntryNumber;
                    xafLedgerEntry.TransactionNumber = transaction.TransactionNumber;
                    xafLedgerEntry.OfficialCode = ledgerEntry.OfficialCode;
                    xafLedgerEntry.AccountName = ledgerEntry.AccountName;
                    xafLedgerEntry.EntryType = ledgerEntry.EntryType;
                    xafLedgerEntry.Amount = ledgerEntry.Amount;
                }
            }

            // Commit changes to XAF ObjectSpace
            _objectSpace.CommitChanges();

            // Mark transaction as posted
            transaction.IsPosted = true;
        }

        /// <summary>
        /// Unposts a transaction from XAF ObjectSpace
        /// </summary>
        /// <param name="transaction">Transaction to unpost</param>
        private async Task UnpostTransactionFromXafAsync(ITransaction transaction)
        {
            await Task.CompletedTask; // Make method async

            // Find and update XAF Transaction entity
            var criteria = new BinaryOperator("TransactionNumber", transaction.TransactionNumber);
            var xafTransaction = _objectSpace.FindObject<Transaction>(criteria);
            
            if (xafTransaction != null)
            {
                xafTransaction.IsPosted = false;
                _objectSpace.CommitChanges();
            }

            // Mark transaction as unposted
            transaction.IsPosted = false;
        }

        /// <summary>
        /// Creates required sequences for accounting operations
        /// </summary>
        private async Task CreateAccountingSequencesAsync()
        {
            var sequences = new[]
            {
                new { Code = TRANSACTION_SEQUENCE_CODE, Name = "Transactions", Prefix = "T", Suffix = "S" },
                new { Code = LEDGERENTRY_SEQUENCE_CODE, Name = "LedgerEntries", Prefix = "LE", Suffix = "S" },
                new { Code = BATCH_SEQUENCE_CODE, Name = "Batch", Prefix = "B", Suffix = "S" },
                new { Code = FISCAL_SEQUENCE_CODE, Name = "Fiscal Period", Prefix = "FP", Suffix = "S" }
            };

            foreach (var seq in sequences)
            {
                var sequence = new SequenceDto
                {
                    Code = seq.Code,
                    CurrentNumber = 1,
                    Name = seq.Name,
                    Prefix = seq.Prefix,
                    Suffix = seq.Suffix
                };

                await sequencerService.CreateSequenceAsync(sequence);
            }
        }
    }
}
