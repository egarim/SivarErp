using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Enhanced implementation of accounting service with comprehensive business logic and validation
    /// </summary>
    [Description("Enhanced implementation of accounting service")]
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
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _logger.LogInformation("Creating transaction from document {DocumentNumber}", document.DocumentNumber);

            // Validate the document first
            var documentValidation = await ValidateDocumentForTransactionAsync(document);
            if (!documentValidation.IsValid)
            {
                throw new InvalidOperationException($"Cannot create transaction from invalid document: {string.Join(", ", documentValidation.Errors)}");
            }

            // Create transaction entity using repository
            var transaction = _repository.CreateObject<TransactionDto>();
            transaction.TransactionNumber = await GenerateTransactionNumberAsync();
            transaction.TransactionDate = document.Date;
            transaction.Description = description ?? GenerateTransactionDescription(document);
            transaction.DocumentNumber = document.DocumentNumber;
            transaction.IsPosted = false;

            // Apply accounting profiles logic
            await ApplyAccountingProfilesAsync(transaction, document);

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
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            _logger.LogInformation("Posting transaction {TransactionNumber}", transaction.TransactionNumber);

            // Validate transaction before posting
            var validationResult = await ValidateTransactionAsync(transaction);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Cannot post invalid transaction: {string.Join(", ", validationResult.Errors)}");
            }

            // Check if already posted
            if (transaction.IsPosted)
            {
                _logger.LogWarning("Transaction {TransactionNumber} is already posted", transaction.TransactionNumber);
                return;
            }

            // Validate fiscal period is open for the transaction date
            await ValidateFiscalPeriodAsync(transaction.TransactionDate);

            // Mark as posted
            if (transaction is TransactionDto transactionDto)
            {
                transactionDto.IsPosted = true;
                _repository.MarkAsModified(transactionDto);

                // Mark all ledger entries as modified to ensure they're committed
                foreach (var entry in transaction.LedgerEntries)
                {
                    if (entry is LedgerEntryDto entryDto)
                    {
                        _repository.MarkAsModified(entryDto);
                    }
                }
            }

            await _repository.CommitChanges();

            _logger.LogInformation("Successfully posted transaction {TransactionNumber} with amount {Amount}", 
                transaction.TransactionNumber, transaction.TotalAmount);
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
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reversal reason is required", nameof(reason));

            _logger.LogInformation("Reversing transaction {TransactionNumber} - Reason: {Reason}", 
                transaction.TransactionNumber, reason);

            if (!transaction.IsPosted)
            {
                throw new InvalidOperationException("Cannot reverse a transaction that has not been posted");
            }

            // Validate fiscal period is open for reversal (use current date)
            var reversalDate = DateOnly.FromDateTime(DateTime.Today);
            await ValidateFiscalPeriodAsync(reversalDate);

            // Create reversal transaction
            var reversalTransaction = _repository.CreateObject<TransactionDto>();
            reversalTransaction.TransactionNumber = await GenerateTransactionNumberAsync();
            reversalTransaction.TransactionDate = reversalDate;
            reversalTransaction.Description = $"REVERSAL: {reason} (Original: {transaction.TransactionNumber})";
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
                reversalEntry.Description = $"Reversal: {originalEntry.Description ?? "N/A"}";
                
                reversalTransaction.LedgerEntries.Add(reversalEntry);
            }

            _logger.LogInformation("Created reversal transaction {TransactionNumber} for original {OriginalTransactionNumber}", 
                reversalTransaction.TransactionNumber, transaction.TransactionNumber);

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
            if (string.IsNullOrWhiteSpace(accountCode))
                throw new ArgumentException("Account code cannot be null or empty", nameof(accountCode));

            var targetDate = asOfDate ?? DateOnly.FromDateTime(DateTime.Today);

            _logger.LogDebug("Calculating balance for account {AccountCode} as of {AsOfDate}", accountCode, targetDate);

            var entries = await GetAccountLedgerEntriesAsync(accountCode, DateOnly.MinValue, targetDate);
            
            decimal balance = 0;
            int entryCount = 0;

            foreach (var entry in entries)
            {
                // Only include posted transactions
                var transaction = _repository.GetObjects<TransactionDto>()
                    .FirstOrDefault(t => t.TransactionNumber == entry.TransactionNumber);

                if (transaction?.IsPosted == true)
                {
                    if (entry.EntryType == EntryType.Debit)
                        balance += entry.Amount;
                    else
                        balance -= entry.Amount;
                    
                    entryCount++;
                }
            }

            _logger.LogDebug("Calculated balance {Balance} for account {AccountCode} from {EntryCount} entries", 
                balance, accountCode, entryCount);

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
            if (string.IsNullOrWhiteSpace(accountCode))
                throw new ArgumentException("Account code cannot be null or empty", nameof(accountCode));

            // Get all transactions that have entries for this account in the date range
            var transactions = _repository.GetObjects<TransactionDto>()
                .Where(t => t.TransactionDate >= fromDate && 
                           t.TransactionDate <= toDate &&
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
            if (string.IsNullOrWhiteSpace(accountCode))
                throw new ArgumentException("Account code cannot be null or empty", nameof(accountCode));

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
        public async Task<Sivar.Erp.Core.Modules.Domain.ValidationResult> ValidateTransactionAsync(ITransaction transaction)
        {
            var result = new Sivar.Erp.Core.Modules.Domain.ValidationResult();

            if (transaction == null)
            {
                result.Errors.Add("Transaction cannot be null");
                result.IsValid = false;
                return result;
            }

            // Check if transaction is balanced
            if (!transaction.IsBalanced)
            {
                result.Errors.Add("Transaction is not balanced - total debits must equal total credits");
                result.IsValid = false;
            }

            // Check for empty entries
            if (!transaction.LedgerEntries.Any())
            {
                result.Errors.Add("Transaction must have at least one ledger entry");
                result.IsValid = false;
            }

            // Check for valid account codes
            var invalidEntries = transaction.LedgerEntries
                .Where(le => string.IsNullOrWhiteSpace(le.AccountCode))
                .ToList();

            if (invalidEntries.Any())
            {
                result.Errors.Add("All ledger entries must have valid account codes");
                result.IsValid = false;
            }

            // Check for zero amounts
            var zeroAmountEntries = transaction.LedgerEntries
                .Where(le => le.Amount <= 0)
                .ToList();

            if (zeroAmountEntries.Any())
            {
                result.Errors.Add("All ledger entries must have positive amounts");
                result.IsValid = false;
            }

            // Check for duplicate transaction number (for new transactions)
            if (!transaction.IsPosted && !string.IsNullOrEmpty(transaction.TransactionNumber))
            {
                var existingTransaction = _repository.FindObject<TransactionDto>(t => 
                    t.TransactionNumber == transaction.TransactionNumber && t.Id != transaction.Id);

                if (existingTransaction != null)
                {
                    result.Errors.Add($"Transaction number {transaction.TransactionNumber} already exists");
                    result.IsValid = false;
                }
            }

            // Validate that accounts exist (would normally check against chart of accounts)
            await ValidateAccountsExistAsync(transaction, result);

            return result;
        }

        /// <summary>
        /// Gets the trial balance for all accounts as of a specific date
        /// </summary>
        /// <param name="asOfDate">Date for which to get the trial balance</param>
        /// <returns>Collection of account balances</returns>
        [Description("Gets the trial balance for all accounts")]
        public async Task<IEnumerable<AccountBalance>> GetTrialBalanceAsync(DateOnly asOfDate)
        {
            _logger.LogInformation("Generating trial balance as of {AsOfDate}", asOfDate);

            // Get all account codes that have entries
            var accountCodes = _repository.GetObjects<LedgerEntryDto>()
                .Select(le => le.AccountCode)
                .Distinct()
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .ToList();

            var balances = new List<AccountBalance>();

            foreach (var accountCode in accountCodes)
            {
                var entries = await GetAccountLedgerEntriesAsync(accountCode, DateOnly.MinValue, asOfDate);
                
                decimal debitTotal = 0;
                decimal creditTotal = 0;

                foreach (var entry in entries)
                {
                    // Only include posted transactions
                    var transaction = _repository.GetObjects<TransactionDto>()
                        .FirstOrDefault(t => t.TransactionNumber == entry.TransactionNumber);

                    if (transaction?.IsPosted == true)
                    {
                        if (entry.EntryType == EntryType.Debit)
                            debitTotal += entry.Amount;
                        else
                            creditTotal += entry.Amount;
                    }
                }

                // Only include accounts with activity
                if (debitTotal > 0 || creditTotal > 0)
                {
                    balances.Add(new AccountBalance
                    {
                        AccountCode = accountCode,
                        AccountName = await GetAccountNameAsync(accountCode),
                        DebitBalance = debitTotal,
                        CreditBalance = creditTotal,
                        AsOfDate = asOfDate
                    });
                }
            }

            var sortedBalances = balances.OrderBy(b => b.AccountCode).ToList();

            _logger.LogInformation("Generated trial balance with {AccountCount} accounts as of {AsOfDate}", 
                sortedBalances.Count, asOfDate);

            return sortedBalances;
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
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be null or empty", nameof(description));

            if (entries == null || !entries.Any())
                throw new ArgumentException("Entries cannot be null or empty", nameof(entries));

            _logger.LogInformation("Creating manual journal entry: {Description}", description);

            var transaction = _repository.CreateObject<TransactionDto>();
            transaction.TransactionNumber = await GenerateTransactionNumberAsync();
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

            _logger.LogInformation("Created manual journal entry {TransactionNumber} with {EntryCount} entries", 
                transaction.TransactionNumber, transaction.LedgerEntries.Count);

            return transaction;
        }

        #region Private Helper Methods

        /// <summary>
        /// Generates a unique transaction number
        /// </summary>
        /// <returns>Transaction number</returns>
        private async Task<string> GenerateTransactionNumberAsync()
        {
            // Enhanced implementation with date-based numbering
            var count = _repository.GetObjects<TransactionDto>().Count();
            var transactionNumber = $"TXN{DateTime.Now:yyyyMM}{(count + 1):D6}";
            
            // Ensure uniqueness
            while (_repository.FindObject<TransactionDto>(t => t.TransactionNumber == transactionNumber) != null)
            {
                count++;
                transactionNumber = $"TXN{DateTime.Now:yyyyMM}{(count + 1):D6}";
            }
            
            return await Task.FromResult(transactionNumber);
        }

        /// <summary>
        /// Generates a meaningful transaction description from a document
        /// </summary>
        /// <param name="document">Source document</param>
        /// <returns>Generated description</returns>
        private string GenerateTransactionDescription(IDocument document)
        {
            var docTypeName = document.DocumentType?.Name ?? "Document";
            var entityName = document.BusinessEntity?.Name ?? "Unknown Entity";
            return $"{docTypeName} #{document.DocumentNumber} - {entityName}";
        }

        /// <summary>
        /// Applies accounting profiles logic to generate ledger entries from document totals
        /// </summary>
        /// <param name="transaction">Transaction to populate</param>
        /// <param name="document">Source document</param>
        private async Task ApplyAccountingProfilesAsync(ITransaction transaction, IDocument document)
        {
            // Apply accounting profiles logic based on document totals
            foreach (var total in document.DocumentTotals.Where(dt => dt.IncludeInTransaction))
            {
                // Create debit entry if debit account is specified
                if (!string.IsNullOrEmpty(total.DebitAccountCode) && total.Total > 0)
                {
                    var debitEntry = _repository.CreateObject<LedgerEntryDto>();
                    debitEntry.TransactionNumber = transaction.TransactionNumber;
                    debitEntry.AccountCode = total.DebitAccountCode;
                    debitEntry.EntryType = EntryType.Debit;
                    debitEntry.Amount = total.Total;
                    debitEntry.Description = $"{total.Concept} (Dr)";
                    
                    transaction.LedgerEntries.Add(debitEntry);
                }

                // Create credit entry if credit account is specified
                if (!string.IsNullOrEmpty(total.CreditAccountCode) && total.Total > 0)
                {
                    var creditEntry = _repository.CreateObject<LedgerEntryDto>();
                    creditEntry.TransactionNumber = transaction.TransactionNumber;
                    creditEntry.AccountCode = total.CreditAccountCode;
                    creditEntry.EntryType = EntryType.Credit;
                    creditEntry.Amount = total.Total;
                    creditEntry.Description = $"{total.Concept} (Cr)";
                    
                    transaction.LedgerEntries.Add(creditEntry);
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Validates that the document is suitable for transaction creation
        /// </summary>
        /// <param name="document">Document to validate</param>
        /// <returns>Validation result</returns>
        private async Task<Sivar.Erp.Core.Modules.Domain.ValidationResult> ValidateDocumentForTransactionAsync(IDocument document)
        {
            var result = new Sivar.Erp.Core.Modules.Domain.ValidationResult { IsValid = true };

            // Check document type allows transaction generation
            if (document.DocumentType?.GeneratesTransaction != true)
            {
                result.Errors.Add($"Document type '{document.DocumentType?.Name}' does not generate transactions");
                result.IsValid = false;
            }

            // Check for document totals
            if (!document.DocumentTotals.Any(dt => dt.IncludeInTransaction))
            {
                result.Errors.Add("Document has no totals marked for transaction inclusion");
                result.IsValid = false;
            }

            // Validate that at least one total has both debit and credit accounts
            var validTotals = document.DocumentTotals.Where(dt => 
                dt.IncludeInTransaction && 
                !string.IsNullOrEmpty(dt.DebitAccountCode) && 
                !string.IsNullOrEmpty(dt.CreditAccountCode));

            if (!validTotals.Any())
            {
                result.Warnings.Add("No document totals have both debit and credit account codes specified");
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Validates that the fiscal period is open for the given date
        /// </summary>
        /// <param name="transactionDate">Date to validate</param>
        private async Task ValidateFiscalPeriodAsync(DateOnly transactionDate)
        {
            // For now, just validate that the date is not in the future
            // In a full implementation, this would check against actual fiscal periods
            if (transactionDate > DateOnly.FromDateTime(DateTime.Today))
            {
                throw new InvalidOperationException($"Cannot post transactions in the future. Transaction date: {transactionDate}");
            }

            // Add warning for old dates
            var daysDiff = DateOnly.FromDateTime(DateTime.Today).DayNumber - transactionDate.DayNumber;
            if (daysDiff > 365)
            {
                _logger.LogWarning("Transaction date {TransactionDate} is more than a year old", transactionDate);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Validates that the accounts referenced in the transaction exist
        /// </summary>
        /// <param name="transaction">Transaction to validate</param>
        /// <param name="result">Validation result to update</param>
        private async Task ValidateAccountsExistAsync(ITransaction transaction, Sivar.Erp.Core.Modules.Domain.ValidationResult result)
        {
            // In a full implementation, this would check against the chart of accounts
            // For now, we'll just ensure account codes are not empty and follow basic patterns
            foreach (var entry in transaction.LedgerEntries)
            {
                if (string.IsNullOrWhiteSpace(entry.AccountCode))
                {
                    result.Errors.Add("Account code cannot be empty");
                    result.IsValid = false;
                }
                else if (entry.AccountCode.Length < 2)
                {
                    result.Errors.Add($"Account code '{entry.AccountCode}' is too short (minimum 2 characters)");
                    result.IsValid = false;
                }
                else if (entry.AccountCode.Length > 20)
                {
                    result.Errors.Add($"Account code '{entry.AccountCode}' is too long (maximum 20 characters)");
                    result.IsValid = false;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets the account name for a given account code
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <returns>Account name or the code if not found</returns>
        private async Task<string> GetAccountNameAsync(string accountCode)
        {
            // In a full implementation, this would look up the account name from the chart of accounts
            // For now, return the code itself
            return await Task.FromResult(accountCode);
        }

        #endregion
    }
}