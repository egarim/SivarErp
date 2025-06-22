using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.Services;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting.BalanceCalculators;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Modules.Accounting.JournalEntries;
using Sivar.Erp.Modules.Accounting.Reports;

namespace Sivar.Erp.Modules.Accounting
{
    public class AccountingModule : ErpModuleBase, IAccountingModule
    {
        protected IFiscalPeriodService FiscalPeriodService;
        private IAccountBalanceCalculator accountBalanceCalculator;
        private readonly PerformanceLogger<AccountingModule> _performanceLogger;
        private readonly IObjectDb? _objectDb;
        private readonly IJournalEntryService _journalEntryService;
        private readonly IJournalEntryReportService _reportService;

        private const string TRANSACTION_SEQUENCE_CODE = "TRANS";
        private const string BATCH_SEQUENCE_CODE = "BATCH";
        private const string FISCAL_SEQUENCE_CODE = "FISCAL";
        private const string LEDGERENTRY_SEQUENCE_CODE = "LEDGERENTRY";

        public IAccountBalanceCalculator AccountBalanceCalculator { get => accountBalanceCalculator; set => accountBalanceCalculator = value; }
        public AccountingModule(
            IOptionService optionService,
            IActivityStreamService activityStreamService,
            IDateTimeZoneService dateTimeZoneService,
            IFiscalPeriodService fiscalPeriodService,
            IAccountBalanceCalculator accountBalanceCalculator,
            ISequencerService sequencerService,
            ILogger<AccountingModule> logger,
            IJournalEntryService journalEntryService,
            IJournalEntryReportService reportService,
            IObjectDb? objectDb = null)
            : base(optionService, activityStreamService, dateTimeZoneService, sequencerService)
        {
            FiscalPeriodService = fiscalPeriodService;
            this.accountBalanceCalculator = accountBalanceCalculator;
            _objectDb = objectDb;
            _journalEntryService = journalEntryService;
            _reportService = reportService;
            _performanceLogger = new PerformanceLogger<AccountingModule>(logger, PerformanceLogMode.All, 100, 10_000_000, _objectDb);
        }

        /// <summary>
        /// Creates a transaction from a document with accounting entries based on document totals
        /// </summary>
        /// <param name="document">The source document for the transaction</param>
        /// <param name="description">Optional description for the transaction</param>
        /// <returns>A transaction ready for posting</returns>
        public async Task<ITransaction> CreateTransactionFromDocumentAsync(IDocument document, string? description = null)
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
        /// Posts a transaction after validating fiscal period is open
        /// </summary>
        /// <param name="transaction">Transaction to post</param>
        /// <returns>True if posted successfully, false otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period is closed</exception>
        public async Task<bool> PostTransactionAsync(ITransaction transaction)
        {
            return await _performanceLogger.Track(nameof(PostTransactionAsync), async () =>
            {
                if (transaction == null)
                    throw new ArgumentNullException(nameof(transaction));

                if (transaction.IsPosted)
                    return true; // Already posted

                // Validate that transaction is in an open fiscal period
                var fiscalPeriod = await FiscalPeriodService.GetFiscalPeriodForDateAsync(transaction.TransactionDate);

                if (fiscalPeriod == null)
                    throw new InvalidOperationException($"No fiscal period found for date {transaction.TransactionDate}");

                if (fiscalPeriod.Status == FiscalPeriodStatus.Closed)
                    throw new InvalidOperationException($"Cannot post transaction: Fiscal period '{fiscalPeriod.Name}' is closed");

                // Validate transaction balance (debits = credits)
                bool isValid = await transaction.ValidateTransactionAsync();

                if (!isValid)
                    throw new InvalidOperationException("Transaction has unbalanced debits and credits");                // Generate transaction number if not already set
                if (string.IsNullOrEmpty(transaction.TransactionNumber))
                {
                    transaction.TransactionNumber = await sequencerService.GetNextNumberAsync(TRANSACTION_SEQUENCE_CODE);
                }
                foreach (ILedgerEntry ledgerEntry in transaction.LedgerEntries)
                {
                    ledgerEntry.LedgerEntryNumber = await sequencerService.GetNextNumberAsync(LEDGERENTRY_SEQUENCE_CODE);
                    // Set the transaction number for each ledger entry
                    ledgerEntry.TransactionNumber = transaction.TransactionNumber;
                }

                // Post the transaction
                transaction.Post();

                // Store the transaction in ObjectDb if available
                if (_objectDb != null)
                {
                    // Add transaction to ObjectDb
                    _objectDb.Transactions.Add(transaction);

                    // Add all ledger entries to ObjectDb
                    foreach (var ledgerEntry in transaction.LedgerEntries)
                    {
                        _objectDb.LedgerEntries.Add(ledgerEntry);
                    }
                }

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

                return true;
            });
        }

        /// <summary>
        /// Unposts a transaction if possible
        /// </summary>
        /// <param name="transaction">Transaction to unpost</param>
        /// <returns>True if unposted successfully, false otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period is closed</exception>
        public async Task<bool> UnPostTransactionAsync(ITransaction transaction)
        {
            return await _performanceLogger.Track(nameof(UnPostTransactionAsync), async () =>
            {
                if (transaction == null)
                    throw new ArgumentNullException(nameof(transaction));

                if (!transaction.IsPosted)
                    return true; // Already unposted

                // Validate that transaction is in an open fiscal period
                var fiscalPeriod = await FiscalPeriodService.GetFiscalPeriodForDateAsync(transaction.TransactionDate);

                if (fiscalPeriod == null)
                    throw new InvalidOperationException($"No fiscal period found for date {transaction.TransactionDate}");

                if (fiscalPeriod.Status == FiscalPeriodStatus.Closed)
                    throw new InvalidOperationException($"Cannot unpost transaction: Fiscal period '{fiscalPeriod.Name}' is closed");

                // Unpost the transaction
                transaction.UnPost();

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

                return true;
            });
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
                return AccountBalanceCalculator.CalculateAccountBalance(accountCode, asOfDate);
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
                var closedFiscalPeriods = await FiscalPeriodService.GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Closed);
                var fiscalPeriod = closedFiscalPeriods.FirstOrDefault(fp => string.Equals(fp.Code, periodCode, StringComparison.OrdinalIgnoreCase));

                if (fiscalPeriod == null)
                {
                    // If not found in closed periods, check open periods as well (maybe it's already open)
                    var openFiscalPeriods = await FiscalPeriodService.GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Open);
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
                var openFiscalPeriods = await FiscalPeriodService.GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Open);
                var fiscalPeriod = openFiscalPeriods.FirstOrDefault(fp => string.Equals(fp.Code, periodCode, StringComparison.OrdinalIgnoreCase));

                if (fiscalPeriod == null)
                {
                    // If not found in open periods, check closed periods as well (maybe it's already closed)
                    var closedFiscalPeriods = await FiscalPeriodService.GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Closed);
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
            return await _performanceLogger.Track(nameof(IsDateInOpenFiscalPeriodAsync), async () =>
            {
                var fiscalPeriod = await FiscalPeriodService.GetFiscalPeriodForDateAsync(date);
                return fiscalPeriod != null && fiscalPeriod.Status == FiscalPeriodStatus.Open;
            });
        }

        public IFiscalPeriodService GetFiscalPeriodService()
        {
            return FiscalPeriodService;
        }

        public override void RegisterSequence(IEnumerable<SequenceDto> sequenceDtos)
        {
            _performanceLogger.Track(nameof(RegisterSequence), () =>
            {
                SequenceDto sequence = new SequenceDto();
                sequence.Code = TRANSACTION_SEQUENCE_CODE;
                sequence.CurrentNumber = 1;
                sequence.Name = "Transactions";
                sequence.Prefix = "T";
                sequence.Suffix = "S";

                SequenceDto ledgerEntry = new SequenceDto();
                ledgerEntry.Code = LEDGERENTRY_SEQUENCE_CODE;
                ledgerEntry.CurrentNumber = 1;
                ledgerEntry.Name = "LedgerEntries";
                ledgerEntry.Prefix = "LE";
                ledgerEntry.Suffix = "S";

                SequenceDto BatchSequence = new SequenceDto();
                BatchSequence.Code = BATCH_SEQUENCE_CODE;
                BatchSequence.CurrentNumber = 1;
                BatchSequence.Name = "Batch";
                BatchSequence.Prefix = "B";
                BatchSequence.Suffix = "S";

                SequenceDto FiscalPeriod = new SequenceDto();
                FiscalPeriod.Code = FISCAL_SEQUENCE_CODE;
                FiscalPeriod.CurrentNumber = 1;
                FiscalPeriod.Name = "Fiscal Period";
                FiscalPeriod.Prefix = "FP";
                FiscalPeriod.Suffix = "S";

                this.sequencerService.CreateSequenceAsync(FiscalPeriod);
                this.sequencerService.CreateSequenceAsync(sequence);
                this.sequencerService.CreateSequenceAsync(BatchSequence);
                this.sequencerService.CreateSequenceAsync(ledgerEntry);
            });
        }

        // Journal Entry Operations

        /// <summary>
        /// Gets journal entries for a specific transaction
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>Collection of journal entries</returns>
        public async Task<IEnumerable<ILedgerEntry>> GetTransactionJournalEntriesAsync(string transactionNumber)
        {
            return await _journalEntryService.GetJournalEntriesByTransactionAsync(transactionNumber);
        }

        /// <summary>
        /// Validates if a transaction is balanced (total debits = total credits)
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>True if transaction is balanced</returns>
        public async Task<bool> ValidateTransactionBalanceAsync(string transactionNumber)
        {
            return await _journalEntryService.IsTransactionBalancedAsync(transactionNumber);
        }

        /// <summary>
        /// Gets journal entries based on query criteria
        /// </summary>
        /// <param name="options">Query options for filtering</param>
        /// <returns>Collection of journal entries</returns>
        public async Task<IEnumerable<ILedgerEntry>> GetJournalEntriesAsync(JournalEntryQueryOptions options)
        {
            return await _journalEntryService.GetJournalEntriesAsync(options);
        }

        /// <summary>
        /// Generates a journal entry report
        /// </summary>
        /// <param name="options">Query options for the report</param>
        /// <returns>Journal entry report data</returns>
        public async Task<JournalEntryReportDto> GenerateJournalReportAsync(JournalEntryQueryOptions options)
        {
            return await _reportService.GenerateJournalEntryReportAsync(options);
        }

        /// <summary>
        /// Generates a transaction audit trail
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>Transaction audit trail data</returns>
        public async Task<TransactionAuditTrailDto> GenerateTransactionAuditTrailAsync(string transactionNumber)
        {
            return await _reportService.GenerateTransactionAuditTrailAsync(transactionNumber);
        }
    }
}