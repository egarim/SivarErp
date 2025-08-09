using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Implementation of accounting service that handles transaction creation, posting, and balance calculations
    /// </summary>
    [Description("Implementation of accounting service")]
    public class AccountingService : IAccountingService
    {
        private readonly IRepository _repository;
        private readonly ILogger<AccountingService> _logger;

        /// <summary>
        /// Initializes a new instance of the AccountingService
        /// </summary>
        /// <param name="repository">Repository for data access</param>
        /// <param name="logger">Logger for the service</param>
        public AccountingService(IRepository repository, ILogger<AccountingService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a transaction from a document with accounting entries based on document totals
        /// </summary>
        /// <param name="document">The source document for the transaction</param>
        /// <param name="description">Optional description for the transaction</param>
        /// <returns>A transaction ready for posting</returns>
        [Description("Creates a transaction from a document")]
        public async Task<ITransaction> CreateTransactionAsync(IDocument document, string? description = null)
        {
            _logger.LogInformation("Creating transaction from document {DocumentNumber}", document.DocumentNumber);

            // Create transaction entity using repository
            var transaction = _repository.CreateObject<TransactionDto>();
            transaction.TransactionNumber = await GenerateTransactionNumber();
            transaction.TransactionDate = document.Date;
            transaction.Description = description ?? $"Transaction for {document.DocumentNumber}";
            transaction.DocumentNumber = document.DocumentNumber;
            transaction.IsPosted = false;

            // Create ledger entries from document totals
            foreach (var total in document.DocumentTotals.Where(dt => dt.IncludeInTransaction))
            {
                // Create debit entry if debit account is specified
                if (!string.IsNullOrEmpty(total.DebitAccountCode))
                {
                    var debitEntry = _repository.CreateObject<LedgerEntryDto>();
                    debitEntry.TransactionNumber = transaction.TransactionNumber;
                    debitEntry.AccountCode = total.DebitAccountCode;
                    debitEntry.EntryType = EntryType.Debit;
                    debitEntry.Amount = total.Total;
                    debitEntry.Description = total.Concept;
                    
                    transaction.LedgerEntries.Add(debitEntry);
                }

                // Create credit entry if credit account is specified
                if (!string.IsNullOrEmpty(total.CreditAccountCode))
                {
                    var creditEntry = _repository.CreateObject<LedgerEntryDto>();
                    creditEntry.TransactionNumber = transaction.TransactionNumber;
                    creditEntry.AccountCode = total.CreditAccountCode;
                    creditEntry.EntryType = EntryType.Credit;
                    creditEntry.Amount = total.Total;
                    creditEntry.Description = total.Concept;
                    
                    transaction.LedgerEntries.Add(creditEntry);
                }
            }

            _logger.LogInformation("Created transaction {TransactionNumber} with {EntryCount} entries", 
                transaction.TransactionNumber, transaction.LedgerEntries.Count);

            return transaction;
        }

        /// <summary>
        /// Posts a transaction to the ledger, making it permanent
        /// </summary>
        /// <param name="transaction">Transaction to post</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Posts a transaction to the ledger")]
        public async Task PostTransactionAsync(ITransaction transaction)
        {
            _logger.LogInformation("Posting transaction {TransactionNumber}", transaction.TransactionNumber);

            // Validate transaction before posting
            var validationResult = await ValidateTransactionAsync(transaction);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Cannot post invalid transaction: {string.Join(", ", validationResult.Errors)}");
            }

            // Mark as posted
            if (transaction is TransactionDto transactionDto)
            {
                transactionDto.IsPosted = true;
                _repository.MarkAsModified(transactionDto);
            }

            await _repository.CommitChanges();

            _logger.LogInformation("Successfully posted transaction {TransactionNumber}", transaction.TransactionNumber);
        }

        /// <summary>
        /// Reverses a previously posted transaction
        /// </summary>
        /// <param name="transaction">Transaction to reverse</param>
        /// <param name="reason">Reason for reversal</param>
        /// <returns>The reversal transaction</returns>
        [Description("Reverses a previously posted transaction")]
        public async Task<ITransaction> ReverseTransactionAsync(ITransaction transaction, string reason)
        {
            _logger.LogInformation("Reversing transaction {TransactionNumber}", transaction.TransactionNumber);

            if (!transaction.IsPosted)
            {
                throw new InvalidOperationException("Cannot reverse a transaction that has not been posted");
            }

            // Create reversal transaction
            var reversalTransaction = _repository.CreateObject<TransactionDto>();
            reversalTransaction.TransactionNumber = await GenerateTransactionNumber();
            reversalTransaction.TransactionDate = DateOnly.FromDateTime(DateTime.Today);
            reversalTransaction.Description = $"Reversal of {transaction.TransactionNumber}: {reason}";
            reversalTransaction.DocumentNumber = transaction.DocumentNumber;
            reversalTransaction.IsPosted = false;

            // Create reversal entries (flip debit/credit)
            foreach (var originalEntry in transaction.LedgerEntries)
            {
                var reversalEntry = _repository.CreateObject<LedgerEntryDto>();
                reversalEntry.TransactionNumber = reversalTransaction.TransactionNumber;
                reversalEntry.AccountCode = originalEntry.AccountCode;
                reversalEntry.EntryType = originalEntry.EntryType == EntryType.Debit ? EntryType.Credit : EntryType.Debit;
                reversalEntry.Amount = originalEntry.Amount;
                reversalEntry.Description = $"Reversal: {originalEntry.Description}";
                
                reversalTransaction.LedgerEntries.Add(reversalEntry);
            }

            _logger.LogInformation("Created reversal transaction {TransactionNumber}", reversalTransaction.TransactionNumber);

            return reversalTransaction;
        }

        /// <summary>
        /// Calculates account balance as of a specific date
        /// </summary>
        /// <param name="accountCode">Account code to query</param>
        /// <param name="asOfDate">Date for which to calculate the balance (optional, defaults to today)</param>
        /// <returns>The account balance</returns>
        [Description("Calculates account balances")]
        public async Task<decimal> CalculateAccountBalanceAsync(string accountCode, DateOnly? asOfDate = null)
        {
            var targetDate = asOfDate ?? DateOnly.FromDateTime(DateTime.Today);

            var entries = await GetAccountLedgerEntriesAsync(accountCode, DateOnly.MinValue, targetDate);
            
            decimal balance = 0;
            foreach (var entry in entries)
            {
                if (entry.EntryType == EntryType.Debit)
                    balance += entry.Amount;
                else
                    balance -= entry.Amount;
            }

            return balance;
        }

        /// <summary>
        /// Gets all transactions for a specific account within a date range
        /// </summary>
        /// <param name="accountCode">Account code to query</param>
        /// <param name="fromDate">Start date for the query</param>
        /// <param name="toDate">End date for the query</param>
        /// <returns>Collection of transactions</returns>
        [Description("Gets all transactions for a specific account within a date range")]
        public async Task<IEnumerable<ITransaction>> GetAccountTransactionsAsync(string accountCode, DateOnly fromDate, DateOnly toDate)
        {
            // Get all transactions that have entries for this account in the date range
            var transactions = _repository.GetObjects<TransactionDto>()
                .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate &&
                           t.LedgerEntries.Any(le => le.AccountCode == accountCode))
                .OrderBy(t => t.TransactionDate)
                .ThenBy(t => t.TransactionNumber);

            return await Task.FromResult(transactions.ToList());
        }

        /// <summary>
        /// Gets all ledger entries for a specific account within a date range
        /// </summary>
        /// <param name="accountCode">Account code to query</param>
        /// <param name="fromDate">Start date for the query</param>
        /// <param name="toDate">End date for the query</param>
        /// <returns>Collection of ledger entries</returns>
        [Description("Gets all ledger entries for a specific account within a date range")]
        public async Task<IEnumerable<ILedgerEntry>> GetAccountLedgerEntriesAsync(string accountCode, DateOnly fromDate, DateOnly toDate)
        {
            // Get transactions in date range that have entries for this account
            var transactions = _repository.GetObjects<TransactionDto>()
                .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate)
                .OrderBy(t => t.TransactionDate)
                .ThenBy(t => t.TransactionNumber);

            var entries = new List<ILedgerEntry>();
            foreach (var transaction in transactions)
            {
                var accountEntries = transaction.LedgerEntries.Where(le => le.AccountCode == accountCode);
                entries.AddRange(accountEntries);
            }

            return await Task.FromResult(entries);
        }

        /// <summary>
        /// Validates a transaction before posting
        /// </summary>
        /// <param name="transaction">Transaction to validate</param>
        /// <returns>Validation result</returns>
        [Description("Validates a transaction before posting")]
        public async Task<ValidationResult> ValidateTransactionAsync(ITransaction transaction)
        {
            var result = new ValidationResult { IsValid = true };

            // Check if transaction is balanced
            if (!transaction.IsBalanced)
            {
                result.IsValid = false;
                result.Errors.Add("Transaction is not balanced - total debits must equal total credits");
            }

            // Check for empty entries
            if (!transaction.LedgerEntries.Any())
            {
                result.IsValid = false;
                result.Errors.Add("Transaction must have at least one ledger entry");
            }

            // Check for valid accounts (this would normally check against chart of accounts)
            var invalidAccounts = transaction.LedgerEntries
                .Where(le => string.IsNullOrEmpty(le.AccountCode))
                .ToList();

            if (invalidAccounts.Any())
            {
                result.IsValid = false;
                result.Errors.Add("All ledger entries must have valid account codes");
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Gets the trial balance for all accounts as of a specific date
        /// </summary>
        /// <param name="asOfDate">Date for which to get the trial balance</param>
        /// <returns>Collection of account balances</returns>
        [Description("Gets the trial balance for all accounts")]
        public async Task<IEnumerable<AccountBalance>> GetTrialBalanceAsync(DateOnly asOfDate)
        {
            // Get all account codes that have entries
            var accountCodes = _repository.GetObjects<LedgerEntryDto>()
                .Select(le => le.AccountCode)
                .Distinct()
                .ToList();

            var balances = new List<AccountBalance>();

            foreach (var accountCode in accountCodes)
            {
                var entries = await GetAccountLedgerEntriesAsync(accountCode, DateOnly.MinValue, asOfDate);
                
                decimal debitTotal = 0;
                decimal creditTotal = 0;

                foreach (var entry in entries)
                {
                    if (entry.EntryType == EntryType.Debit)
                        debitTotal += entry.Amount;
                    else
                        creditTotal += entry.Amount;
                }

                balances.Add(new AccountBalance
                {
                    AccountCode = accountCode,
                    AccountName = accountCode, // Would normally lookup account name
                    DebitBalance = debitTotal,
                    CreditBalance = creditTotal,
                    AsOfDate = asOfDate
                });
            }

            return balances.OrderBy(b => b.AccountCode);
        }

        /// <summary>
        /// Creates a journal entry manually (not from a document)
        /// </summary>
        /// <param name="description">Description of the journal entry</param>
        /// <param name="entries">Collection of ledger entries</param>
        /// <param name="transactionDate">Date of the transaction</param>
        /// <returns>The created transaction</returns>
        [Description("Creates a journal entry manually")]
        public async Task<ITransaction> CreateJournalEntryAsync(string description, IEnumerable<ILedgerEntry> entries, DateOnly transactionDate)
        {
            var transaction = _repository.CreateObject<TransactionDto>();
            transaction.TransactionNumber = await GenerateTransactionNumber();
            transaction.TransactionDate = transactionDate;
            transaction.Description = description;
            transaction.IsPosted = false;

            foreach (var entry in entries)
            {
                var newEntry = _repository.CreateObject<LedgerEntryDto>();
                newEntry.TransactionNumber = transaction.TransactionNumber;
                newEntry.AccountCode = entry.AccountCode;
                newEntry.EntryType = entry.EntryType;
                newEntry.Amount = entry.Amount;
                newEntry.Description = entry.Description;
                
                transaction.LedgerEntries.Add(newEntry);
            }

            return transaction;
        }

        /// <summary>
        /// Generates a unique transaction number
        /// </summary>
        /// <returns>Transaction number</returns>
        private async Task<string> GenerateTransactionNumber()
        {
            // Simple implementation - would normally use a sequence or counter
            var count = _repository.GetObjects<TransactionDto>().Count();
            var transactionNumber = $"TXN{DateTime.Now:yyyyMM}{(count + 1):D6}";
            
            return await Task.FromResult(transactionNumber);
        }
    }
}