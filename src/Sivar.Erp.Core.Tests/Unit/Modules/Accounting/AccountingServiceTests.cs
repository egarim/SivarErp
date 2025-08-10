using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Modules.Accounting;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;

namespace Sivar.Erp.Core.Tests.Unit.Modules.Accounting
{
    /// <summary>
    /// Comprehensive unit tests for the AccountingService implementation
    /// </summary>
    [Description("Unit tests for AccountingService")]
    public class AccountingServiceTests : IDisposable
    {
        private readonly IRepository _repository;
        private readonly ILogger<AccountingService> _logger;
        private readonly AccountingService _accountingService;

        public AccountingServiceTests()
        {
            _repository = new InMemoryRepository();
            _logger = NullLogger<AccountingService>.Instance;
            _accountingService = new AccountingService(_repository, _logger);
        }

        public void Dispose()
        {
            _repository?.Dispose();
        }

        #region CreateTransactionAsync Tests

        [Fact]
        [Description("CreateTransactionAsync should create transaction from document with valid totals")]
        public async Task CreateTransactionAsync_ShouldCreateTransactionFromValidDocument()
        {
            // Arrange
            var document = CreateTestDocument();
            var description = "Test transaction";

            // Act
            var transaction = await _accountingService.CreateTransactionAsync(document, description);

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(description, transaction.Description);
            Assert.Equal(document.DocumentNumber, transaction.TransactionNumber);
            Assert.Equal(document.Date, transaction.TransactionDate);
            Assert.False(transaction.IsPosted);
            Assert.NotEmpty(transaction.TransactionNumber);
            Assert.True(transaction.LedgerEntries.Count > 0);
        }

        [Fact]
        [Description("CreateTransactionAsync should generate meaningful description when none provided")]
        public async Task CreateTransactionAsync_ShouldGenerateDescription()
        {
            // Arrange
            var document = CreateTestDocument();

            // Act
            var transaction = await _accountingService.CreateTransactionAsync(document);

            // Assert
            Assert.NotNull(transaction.Description);
            Assert.Contains(document.DocumentNumber, transaction.Description);
            Assert.Contains(document.BusinessEntity.Name, transaction.Description);
        }

        [Fact]
        [Description("CreateTransactionAsync should throw ArgumentNullException for null document")]
        public async Task CreateTransactionAsync_ShouldThrowForNullDocument()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _accountingService.CreateTransactionAsync(null));
        }

        [Fact]
        [Description("CreateTransactionAsync should create balanced transaction")]
        public async Task CreateTransactionAsync_ShouldCreateBalancedTransaction()
        {
            // Arrange
            var document = CreateTestDocument();

            // Act
            var transaction = await _accountingService.CreateTransactionAsync(document);

            // Assert
            Assert.True(transaction.IsBalanced);
            
            var totalDebits = transaction.LedgerEntries
                .Where(le => le.EntryType == EntryType.Debit)
                .Sum(le => le.Amount);
            var totalCredits = transaction.LedgerEntries
                .Where(le => le.EntryType == EntryType.Credit)
                .Sum(le => le.Amount);
            
            Assert.Equal(totalDebits, totalCredits);
        }

        [Fact]
        [Description("CreateTransactionAsync should handle document with no transaction-eligible totals")]
        public async Task CreateTransactionAsync_ShouldHandleDocumentWithoutEligibleTotals()
        {
            // Arrange
            var document = CreateTestDocument();
            // Remove transaction eligibility from all totals
            foreach (var total in document.DocumentTotals)
            {
                if (total is DocumentTotalDto dto)
                    dto.IncludeInTransaction = false;
            }

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _accountingService.CreateTransactionAsync(document));
            
            Assert.Contains("no totals marked for transaction inclusion", exception.Message);
        }

        #endregion

        #region PostTransactionAsync Tests

        [Fact]
        [Description("PostTransactionAsync should post valid transaction")]
        public async Task PostTransactionAsync_ShouldPostValidTransaction()
        {
            // Arrange
            var document = CreateTestDocument();
            var transaction = await _accountingService.CreateTransactionAsync(document);

            // Act
            await _accountingService.PostTransactionAsync(transaction);

            // Assert
            Assert.True(transaction.IsPosted);
        }

        [Fact]
        [Description("PostTransactionAsync should throw for null transaction")]
        public async Task PostTransactionAsync_ShouldThrowForNullTransaction()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _accountingService.PostTransactionAsync(null));
        }

        [Fact]
        [Description("PostTransactionAsync should handle already posted transaction gracefully")]
        public async Task PostTransactionAsync_ShouldHandleAlreadyPostedTransaction()
        {
            // Arrange
            var document = CreateTestDocument();
            var transaction = await _accountingService.CreateTransactionAsync(document);
            await _accountingService.PostTransactionAsync(transaction);

            // Act - Post again
            await _accountingService.PostTransactionAsync(transaction);

            // Assert
            Assert.True(transaction.IsPosted);
        }

        [Fact]
        [Description("PostTransactionAsync should validate transaction before posting")]
        public async Task PostTransactionAsync_ShouldValidateBeforePosting()
        {
            // Arrange
            var transaction = _repository.CreateObject<TransactionDto>();
            transaction.TransactionNumber = "TEST001";
            transaction.TransactionDate = DateOnly.FromDateTime(DateTime.Today);
            transaction.Description = "Invalid transaction";
            // No ledger entries - should be invalid

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _accountingService.PostTransactionAsync(transaction));
            
            Assert.Contains("Cannot post invalid transaction", exception.Message);
        }

        [Fact]
        [Description("PostTransactionAsync should prevent posting future-dated transactions")]
        public async Task PostTransactionAsync_ShouldPreventFutureDatedTransactions()
        {
            // Arrange
            var document = CreateTestDocument();
            var transaction = await _accountingService.CreateTransactionAsync(document);
            
            // Set future date
            if (transaction is TransactionDto dto)
                dto.TransactionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _accountingService.PostTransactionAsync(transaction));
            
            Assert.Contains("Cannot post transactions in the future", exception.Message);
        }

        #endregion

        #region ReverseTransactionAsync Tests

        [Fact]
        [Description("ReverseTransactionAsync should create reversal transaction")]
        public async Task ReverseTransactionAsync_ShouldCreateReversalTransaction()
        {
            // Arrange
            var document = CreateTestDocument();
            var originalTransaction = await _accountingService.CreateTransactionAsync(document);
            await _accountingService.PostTransactionAsync(originalTransaction);
            var reason = "Correction needed";

            // Act
            var reversalTransaction = await _accountingService.ReverseTransactionAsync(originalTransaction, reason);

            // Assert
            Assert.NotNull(reversalTransaction);
            Assert.Contains("REVERSAL", reversalTransaction.Description);
            Assert.Contains(reason, reversalTransaction.Description);
            Assert.Contains(originalTransaction.TransactionNumber, reversalTransaction.Description);
            Assert.Equal(originalTransaction.DocumentNumber, reversalTransaction.DocumentNumber);
            Assert.False(reversalTransaction.IsPosted);
            Assert.Equal(originalTransaction.LedgerEntries.Count, reversalTransaction.LedgerEntries.Count);
        }

        [Fact]
        [Description("ReverseTransactionAsync should flip debit/credit entries")]
        public async Task ReverseTransactionAsync_ShouldFlipDebitCreditEntries()
        {
            // Arrange
            var document = CreateTestDocument();
            var originalTransaction = await _accountingService.CreateTransactionAsync(document);
            await _accountingService.PostTransactionAsync(originalTransaction);

            // Act
            var reversalTransaction = await _accountingService.ReverseTransactionAsync(originalTransaction, "Test reversal");

            // Assert
            foreach (var originalEntry in originalTransaction.LedgerEntries)
            {
                var reversalEntry = reversalTransaction.LedgerEntries
                    .FirstOrDefault(re => re.AccountCode == originalEntry.AccountCode && re.Amount == originalEntry.Amount);
                
                Assert.NotNull(reversalEntry);
                Assert.NotEqual(originalEntry.EntryType, reversalEntry.EntryType);
            }
        }

        [Fact]
        [Description("ReverseTransactionAsync should throw for unposted transaction")]
        public async Task ReverseTransactionAsync_ShouldThrowForUnpostedTransaction()
        {
            // Arrange
            var document = CreateTestDocument();
            var transaction = await _accountingService.CreateTransactionAsync(document);
            // Don't post the transaction

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _accountingService.ReverseTransactionAsync(transaction, "Test"));
            
            Assert.Contains("Cannot reverse a transaction that has not been posted", exception.Message);
        }

        [Fact]
        [Description("ReverseTransactionAsync should throw for null or empty reason")]
        public async Task ReverseTransactionAsync_ShouldThrowForEmptyReason()
        {
            // Arrange
            var document = CreateTestDocument();
            var transaction = await _accountingService.CreateTransactionAsync(document);
            await _accountingService.PostTransactionAsync(transaction);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountingService.ReverseTransactionAsync(transaction, ""));
            
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountingService.ReverseTransactionAsync(transaction, null));
        }

        #endregion

        #region CalculateAccountBalanceAsync Tests

        [Fact]
        [Description("CalculateAccountBalanceAsync should calculate correct balance")]
        public async Task CalculateAccountBalanceAsync_ShouldCalculateCorrectBalance()
        {
            // Arrange
            var accountCode = "1000";
            await CreateAndPostTestTransaction(accountCode, 100m, 0m);
            await CreateAndPostTestTransaction(accountCode, 50m, 25m);

            // Act
            var balance = await _accountingService.CalculateAccountBalanceAsync(accountCode);

            // Assert
            Assert.Equal(125m, balance); // (100 + 50) - (0 + 25) = 125
        }

        [Fact]
        [Description("CalculateAccountBalanceAsync should only include posted transactions")]
        public async Task CalculateAccountBalanceAsync_ShouldOnlyIncludePostedTransactions()
        {
            // Arrange
            var accountCode = "1000";
            await CreateAndPostTestTransaction(accountCode, 100m, 0m);
            
            // Create unposted transaction
            var document = CreateTestDocument();
            var unpostedTransaction = await _accountingService.CreateTransactionAsync(document);
            // Don't post it

            // Act
            var balance = await _accountingService.CalculateAccountBalanceAsync(accountCode);

            // Assert - Should only include the posted transaction
            Assert.Equal(100m, balance);
        }

        [Fact]
        [Description("CalculateAccountBalanceAsync should throw for null account code")]
        public async Task CalculateAccountBalanceAsync_ShouldThrowForNullAccountCode()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountingService.CalculateAccountBalanceAsync(null));
            
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountingService.CalculateAccountBalanceAsync(""));
            
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountingService.CalculateAccountBalanceAsync("   "));
        }

        [Fact]
        [Description("CalculateAccountBalanceAsync should handle date filtering")]
        public async Task CalculateAccountBalanceAsync_ShouldHandleDateFiltering()
        {
            // Arrange
            var accountCode = "1000";
            await CreateAndPostTestTransaction(accountCode, 100m, 0m, DateOnly.FromDateTime(DateTime.Today.AddDays(-10)));
            await CreateAndPostTestTransaction(accountCode, 50m, 0m, DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));

            // Act - Get balance as of 7 days ago (should only include first transaction)
            var balance = await _accountingService.CalculateAccountBalanceAsync(accountCode, DateOnly.FromDateTime(DateTime.Today.AddDays(-7)));

            // Assert
            Assert.Equal(100m, balance);
        }

        #endregion

        #region ValidateTransactionAsync Tests

        [Fact]
        [Description("ValidateTransactionAsync should validate balanced transaction")]
        public async Task ValidateTransactionAsync_ShouldValidateBalancedTransaction()
        {
            // Arrange
            var document = CreateTestDocument();
            var transaction = await _accountingService.CreateTransactionAsync(document);

            // Act
            var result = await _accountingService.ValidateTransactionAsync(transaction);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        [Description("ValidateTransactionAsync should detect unbalanced transaction")]
        public async Task ValidateTransactionAsync_ShouldDetectUnbalancedTransaction()
        {
            // Arrange
            var transaction = _repository.CreateObject<TransactionDto>();
            transaction.TransactionNumber = "TEST001";
            transaction.TransactionDate = DateOnly.FromDateTime(DateTime.Today);
            transaction.Description = "Unbalanced transaction";
            
            // Add only debit entry (unbalanced)
            var entry = _repository.CreateObject<LedgerEntryDto>();
            entry.AccountCode = "1000";
            entry.EntryType = EntryType.Debit;
            entry.Amount = 100m;
            transaction.LedgerEntries.Add(entry);

            // Act
            var result = await _accountingService.ValidateTransactionAsync(transaction);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("not balanced", result.Errors.First());
        }

        [Fact]
        [Description("ValidateTransactionAsync should detect empty transaction")]
        public async Task ValidateTransactionAsync_ShouldDetectEmptyTransaction()
        {
            // Arrange
            var transaction = _repository.CreateObject<TransactionDto>();
            transaction.TransactionNumber = "TEST001";
            transaction.TransactionDate = DateOnly.FromDateTime(DateTime.Today);
            transaction.Description = "Empty transaction";
            // No ledger entries

            // Act
            var result = await _accountingService.ValidateTransactionAsync(transaction);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("at least one ledger entry", result.Errors.First());
        }

        [Fact]
        [Description("ValidateTransactionAsync should detect invalid account codes")]
        public async Task ValidateTransactionAsync_ShouldDetectInvalidAccountCodes()
        {
            // Arrange
            var transaction = _repository.CreateObject<TransactionDto>();
            transaction.TransactionNumber = "TEST001";
            transaction.TransactionDate = DateOnly.FromDateTime(DateTime.Today);
            transaction.Description = "Invalid accounts";
            
            // Add entries with invalid account codes
            var entry1 = _repository.CreateObject<LedgerEntryDto>();
            entry1.AccountCode = ""; // Empty
            entry1.EntryType = EntryType.Debit;
            entry1.Amount = 100m;
            transaction.LedgerEntries.Add(entry1);

            var entry2 = _repository.CreateObject<LedgerEntryDto>();
            entry2.AccountCode = "A"; // Too short
            entry2.EntryType = EntryType.Credit;
            entry2.Amount = 100m;
            transaction.LedgerEntries.Add(entry2);

            // Act
            var result = await _accountingService.ValidateTransactionAsync(transaction);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Count >= 2);
        }

        [Fact]
        [Description("ValidateTransactionAsync should detect zero amounts")]
        public async Task ValidateTransactionAsync_ShouldDetectZeroAmounts()
        {
            // Arrange
            var transaction = _repository.CreateObject<TransactionDto>();
            transaction.TransactionNumber = "TEST001";
            transaction.TransactionDate = DateOnly.FromDateTime(DateTime.Today);
            transaction.Description = "Zero amounts";
            
            var entry1 = _repository.CreateObject<LedgerEntryDto>();
            entry1.AccountCode = "1000";
            entry1.EntryType = EntryType.Debit;
            entry1.Amount = 0m; // Zero amount
            transaction.LedgerEntries.Add(entry1);

            var entry2 = _repository.CreateObject<LedgerEntryDto>();
            entry2.AccountCode = "2000";
            entry2.EntryType = EntryType.Credit;
            entry2.Amount = -10m; // Negative amount
            transaction.LedgerEntries.Add(entry2);

            // Act
            var result = await _accountingService.ValidateTransactionAsync(transaction);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("positive amounts", result.Errors.First());
        }

        [Fact]
        [Description("ValidateTransactionAsync should handle null transaction")]
        public async Task ValidateTransactionAsync_ShouldHandleNullTransaction()
        {
            // Act
            var result = await _accountingService.ValidateTransactionAsync(null);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("cannot be null", result.Errors.First());
        }

        #endregion

        #region GetTrialBalanceAsync Tests

        [Fact]
        [Description("GetTrialBalanceAsync should generate trial balance")]
        public async Task GetTrialBalanceAsync_ShouldGenerateTrialBalance()
        {
            // Arrange
            await CreateAndPostTestTransaction("1000", 100m, 0m);
            await CreateAndPostTestTransaction("2000", 0m, 50m);
            await CreateAndPostTestTransaction("3000", 75m, 25m);

            // Act
            var trialBalance = await _accountingService.GetTrialBalanceAsync(DateOnly.FromDateTime(DateTime.Today));

            // Assert
            var balanceList = trialBalance.ToList();
            Assert.Equal(3, balanceList.Count);
            
            var account1000 = balanceList.First(b => b.AccountCode == "1000");
            Assert.Equal(100m, account1000.DebitBalance);
            Assert.Equal(0m, account1000.CreditBalance);
            
            var account2000 = balanceList.First(b => b.AccountCode == "2000");
            Assert.Equal(0m, account2000.DebitBalance);
            Assert.Equal(50m, account2000.CreditBalance);
        }

        [Fact]
        [Description("GetTrialBalanceAsync should only include posted transactions")]
        public async Task GetTrialBalanceAsync_ShouldOnlyIncludePostedTransactions()
        {
            // Arrange
            await CreateAndPostTestTransaction("1000", 100m, 0m);
            
            // Create unposted transaction
            var document = CreateTestDocument();
            await _accountingService.CreateTransactionAsync(document);

            // Act
            var trialBalance = await _accountingService.GetTrialBalanceAsync(DateOnly.FromDateTime(DateTime.Today));

            // Assert
            var balanceList = trialBalance.ToList();
            var account1000 = balanceList.FirstOrDefault(b => b.AccountCode == "1000");
            Assert.NotNull(account1000);
            Assert.Equal(100m, account1000.DebitBalance);
        }

        [Fact]
        [Description("GetTrialBalanceAsync should exclude accounts with no activity")]
        public async Task GetTrialBalanceAsync_ShouldExcludeAccountsWithNoActivity()
        {
            // Arrange
            await CreateAndPostTestTransaction("1000", 100m, 0m);
            // Account "2000" has no transactions

            // Act
            var trialBalance = await _accountingService.GetTrialBalanceAsync(DateOnly.FromDateTime(DateTime.Today));

            // Assert
            var balanceList = trialBalance.ToList();
            Assert.Single(balanceList);
            Assert.Equal("1000", balanceList.First().AccountCode);
        }

        #endregion

        #region CreateJournalEntryAsync Tests

        [Fact]
        [Description("CreateJournalEntryAsync should create manual journal entry")]
        public async Task CreateJournalEntryAsync_ShouldCreateManualJournalEntry()
        {
            // Arrange
            var description = "Manual adjustment";
            var entries = new List<ILedgerEntry>
            {
                CreateLedgerEntry("1000", EntryType.Debit, 100m, "Adjustment debit"),
                CreateLedgerEntry("2000", EntryType.Credit, 100m, "Adjustment credit")
            };
            var transactionDate = DateOnly.FromDateTime(DateTime.Today);

            // Act
            var transaction = await _accountingService.CreateJournalEntryAsync(description, entries, transactionDate);

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(description, transaction.Description);
            Assert.Equal(transactionDate, transaction.TransactionDate);
            Assert.False(transaction.IsPosted);
            Assert.Equal(2, transaction.LedgerEntries.Count);
            Assert.True(transaction.IsBalanced);
        }

        [Fact]
        [Description("CreateJournalEntryAsync should throw for null description")]
        public async Task CreateJournalEntryAsync_ShouldThrowForNullDescription()
        {
            // Arrange
            var entries = new List<ILedgerEntry> { CreateLedgerEntry("1000", EntryType.Debit, 100m) };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountingService.CreateJournalEntryAsync(null, entries, DateOnly.FromDateTime(DateTime.Today)));
            
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountingService.CreateJournalEntryAsync("", entries, DateOnly.FromDateTime(DateTime.Today)));
        }

        [Fact]
        [Description("CreateJournalEntryAsync should throw for null or empty entries")]
        public async Task CreateJournalEntryAsync_ShouldThrowForNullOrEmptyEntries()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountingService.CreateJournalEntryAsync("Test", null, DateOnly.FromDateTime(DateTime.Today)));
            
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountingService.CreateJournalEntryAsync("Test", new List<ILedgerEntry>(), DateOnly.FromDateTime(DateTime.Today)));
        }

        #endregion

        #region GetAccountTransactionsAsync Tests

        [Fact]
        [Description("GetAccountTransactionsAsync should return transactions for account in date range")]
        public async Task GetAccountTransactionsAsync_ShouldReturnTransactionsInDateRange()
        {
            // Arrange
            var accountCode = "1000";
            var baseDate = DateOnly.FromDateTime(DateTime.Today);
            
            await CreateAndPostTestTransaction(accountCode, 100m, 0m, baseDate.AddDays(-10));
            await CreateAndPostTestTransaction(accountCode, 50m, 0m, baseDate.AddDays(-5));
            await CreateAndPostTestTransaction(accountCode, 25m, 0m, baseDate.AddDays(-1));

            // Act
            var transactions = await _accountingService.GetAccountTransactionsAsync(
                accountCode, 
                baseDate.AddDays(-7), 
                baseDate);

            // Assert
            var transactionList = transactions.ToList();
            Assert.Equal(2, transactionList.Count); // Should exclude the transaction from 10 days ago
        }

        [Fact]
        [Description("GetAccountTransactionsAsync should throw for null account code")]
        public async Task GetAccountTransactionsAsync_ShouldThrowForNullAccountCode()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountingService.GetAccountTransactionsAsync(
                    null, 
                    DateOnly.FromDateTime(DateTime.Today), 
                    DateOnly.FromDateTime(DateTime.Today)));
        }

        #endregion

        #region GetAccountLedgerEntriesAsync Tests

        [Fact]
        [Description("GetAccountLedgerEntriesAsync should return ledger entries for account")]
        public async Task GetAccountLedgerEntriesAsync_ShouldReturnLedgerEntriesForAccount()
        {
            // Arrange
            var accountCode = "1000";
            await CreateAndPostTestTransaction(accountCode, 100m, 0m);
            await CreateAndPostTestTransaction(accountCode, 50m, 25m);

            // Act
            var entries = await _accountingService.GetAccountLedgerEntriesAsync(
                accountCode, 
                DateOnly.MinValue, 
                DateOnly.MaxValue);

            // Assert
            var entryList = entries.ToList();
            Assert.Equal(2, entryList.Count); // 2 debit entries for account 1000
            Assert.All(entryList, e => Assert.Equal(accountCode, e.AccountCode));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a test document with valid totals for transaction generation
        /// </summary>
        private IDocument CreateTestDocument()
        {
            var documentType = new DocumentTypeDto
            {
                Code = "INV",
                Name = "Invoice",
                GeneratesTransaction = true
            };

            var businessEntity = new BusinessEntityDto
            {
                Code = "CUST001",
                Name = "Test Customer",
                EntityType = BusinessEntityType.Customer
            };

            var document = new DocumentDto
            {
                DocumentNumber = "INV-001",
                Date = DateOnly.FromDateTime(DateTime.Today),
                DocumentType = documentType,
                BusinessEntity = businessEntity,
                Status = DocumentStatus.Approved
            };

            // Add document totals
            var total1 = new DocumentTotalDto
            {
                Concept = "Merchandise",
                Total = 100m,
                DebitAccountCode = "1200", // Accounts Receivable
                CreditAccountCode = "4000", // Sales Revenue
                IncludeInTransaction = true
            };

            document.DocumentTotals.Add(total1);

            return document;
        }

        /// <summary>
        /// Creates a ledger entry for testing
        /// </summary>
        private ILedgerEntry CreateLedgerEntry(string accountCode, EntryType entryType, decimal amount, string description = null)
        {
            return new LedgerEntryDto
            {
                AccountCode = accountCode,
                EntryType = entryType,
                Amount = amount,
                Description = description ?? $"{entryType} entry"
            };
        }

        /// <summary>
        /// Creates and posts a test transaction with specified amounts
        /// </summary>
        private async Task CreateAndPostTestTransaction(string accountCode, decimal debitAmount, decimal creditAmount, DateOnly? date = null)
        {
            var transactionDate = date ?? DateOnly.FromDateTime(DateTime.Today);
            
            var entries = new List<ILedgerEntry>();
            
            if (debitAmount > 0)
            {
                entries.Add(CreateLedgerEntry(accountCode, EntryType.Debit, debitAmount));
            }
            
            if (creditAmount > 0)
            {
                entries.Add(CreateLedgerEntry(accountCode, EntryType.Credit, creditAmount));
            }

            // Balance the transaction with opposite entries
            if (debitAmount > creditAmount)
            {
                entries.Add(CreateLedgerEntry("9999", EntryType.Credit, debitAmount - creditAmount));
            }
            else if (creditAmount > debitAmount)
            {
                entries.Add(CreateLedgerEntry("9999", EntryType.Debit, creditAmount - debitAmount));
            }

            var transaction = await _accountingService.CreateJournalEntryAsync(
                "Test transaction", 
                entries, 
                transactionDate);
            
            await _accountingService.PostTransactionAsync(transaction);
        }

        #endregion
    }
}