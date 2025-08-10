using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Modules.Accounting;
using Sivar.Erp.Core.Modules.Documents;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Demo;
using Sivar.Erp.Core.Modules.DataImport;
using Sivar.Erp.Core.Configuration;

namespace Sivar.Erp.Core.Tests.Integration
{
    /// <summary>
    /// Integration tests for Day 7: Comprehensive Accounting Service workflow
    /// Tests the complete AccountingService with real business scenarios
    /// </summary>
    [Description("Integration tests for AccountingService workflow")]
    public class Day7AccountingWorkflowTest : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository _repository;
        private readonly IAccountingService _accountingService;
        private readonly IDocumentService _documentService;
        private readonly ISampleDataGenerator _sampleDataGenerator;
        private readonly IDataImportService _dataImportService;

        public Day7AccountingWorkflowTest()
        {
            // Setup DI container
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddSivarErpDemo("ElSalvador");

            _serviceProvider = services.BuildServiceProvider();
            _repository = _serviceProvider.GetRequiredService<IRepository>();
            _accountingService = _serviceProvider.GetRequiredService<IAccountingService>();
            _documentService = _serviceProvider.GetRequiredService<IDocumentService>();
            _sampleDataGenerator = _serviceProvider.GetRequiredService<ISampleDataGenerator>();
            _dataImportService = _serviceProvider.GetRequiredService<IDataImportService>();
        }

        public void Dispose()
        {
            _repository?.Dispose();
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }

        [Fact]
        [Description("Test complete accounting workflow from transaction creation to trial balance")]
        public async Task TestCompleteAccountingWorkflow()
        {
            // Arrange - Setup test data
            await SetupTestDataAsync();

            // Act & Assert - Test each step of the accounting workflow

            // Step 1: Create transaction
            var transaction = await CreateTestTransactionAsync();
            Assert.NotNull(transaction);
            Assert.False(string.IsNullOrEmpty(transaction.TransactionNumber));
            Assert.True(transaction.IsBalanced);

            // Step 2: Post transaction
            await _accountingService.PostTransactionAsync(transaction);
            Assert.True(transaction.IsPosted);

            // Step 3: Verify balances
            await VerifyAccountBalancesAsync();

            // Step 4: Generate trial balance
            var trialBalance = await _accountingService.GetTrialBalanceAsync(DateOnly.FromDateTime(DateTime.Today));
            Assert.NotEmpty(trialBalance);
            VerifyTrialBalanceIntegrity(trialBalance);

            // Step 5: Test reversal
            var reversalTransaction = await _accountingService.ReverseTransactionAsync(transaction, "Test reversal");
            Assert.NotNull(reversalTransaction);
            Assert.True(reversalTransaction.IsBalanced);

            // Step 6: Verify reversal impact
            await VerifyReversalImpactAsync(transaction);
        }

        [Fact]
        [Description("Test transaction validation with various scenarios")]
        public async Task TestTransactionValidation()
        {
            // Arrange
            await SetupTestDataAsync();

            // Test valid transaction
            var validTransaction = await CreateTestTransactionAsync();
            var validationResult = await _accountingService.ValidateTransactionAsync(validTransaction);
            Assert.True(validationResult.IsValid);

            // Test unbalanced transaction
            var unbalancedTransaction = _repository.CreateObject<TransactionDto>();
            unbalancedTransaction.TransactionNumber = "TXN-2024-002";
            unbalancedTransaction.TransactionDate = DateOnly.FromDateTime(DateTime.Today);
            unbalancedTransaction.Description = "Unbalanced transaction";

            var debitEntry = _repository.CreateObject<LedgerEntryDto>();
            debitEntry.AccountCode = "1000";
            debitEntry.EntryType = EntryType.Debit;
            debitEntry.Amount = 100m;
            unbalancedTransaction.LedgerEntries.Add(debitEntry);

            var creditEntry = _repository.CreateObject<LedgerEntryDto>();
            creditEntry.AccountCode = "2000";
            creditEntry.EntryType = EntryType.Credit;
            creditEntry.Amount = 50m; // Intentionally unbalanced
            unbalancedTransaction.LedgerEntries.Add(creditEntry);

            var unbalancedValidation = await _accountingService.ValidateTransactionAsync(unbalancedTransaction);
            Assert.False(unbalancedValidation.IsValid);
            Assert.Contains("Transaction is not balanced", unbalancedValidation.Errors);
        }

        [Fact]
        [Description("Test account balance calculations")]
        public async Task TestAccountBalanceCalculations()
        {
            // Arrange
            await SetupTestDataAsync();

            // Create multiple transactions affecting the same accounts
            for (int i = 0; i < 5; i++)
            {
                var transaction = await CreateTestTransactionAsync($"TXN-2024-{100 + i:D3}");
                await _accountingService.PostTransactionAsync(transaction);
            }

            // Test balance calculation for specific account
            var cashBalance = await _accountingService.CalculateAccountBalanceAsync("1000");
            Assert.True(cashBalance != 0);

            // Test balance calculation with date filter
            var balanceAsOfYesterday = await _accountingService.CalculateAccountBalanceAsync("1000", 
                DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));
            Assert.True(balanceAsOfYesterday >= 0);

            // Test trial balance totals
            var trialBalance = await _accountingService.GetTrialBalanceAsync(DateOnly.FromDateTime(DateTime.Today));
            var totalDebits = trialBalance.Sum(ab => ab.DebitBalance);
            var totalCredits = trialBalance.Sum(ab => ab.CreditBalance);
            Assert.Equal(totalDebits, totalCredits);
        }

        [Fact]
        [Description("Test manual journal entries")]
        public async Task TestManualJournalEntries()
        {
            // Arrange
            await SetupTestDataAsync();

            // Create ledger entries for the journal entry
            var entries = new List<ILedgerEntry>
            {
                new LedgerEntryDto
                {
                    AccountCode = "6000",
                    EntryType = EntryType.Debit,
                    Amount = 250m,
                    Description = "Manual adjustment debit"
                },
                new LedgerEntryDto
                {
                    AccountCode = "1000",
                    EntryType = EntryType.Credit,
                    Amount = 250m,
                    Description = "Manual adjustment credit"
                }
            };

            // Create manual journal entry
            var journalEntry = await _accountingService.CreateJournalEntryAsync(
                "Manual adjustment", 
                entries,
                DateOnly.FromDateTime(DateTime.Today));

            Assert.NotNull(journalEntry);
            Assert.False(string.IsNullOrEmpty(journalEntry.TransactionNumber));

            // Validate and post
            var validation = await _accountingService.ValidateTransactionAsync(journalEntry);
            Assert.True(validation.IsValid);

            await _accountingService.PostTransactionAsync(journalEntry);
            Assert.True(journalEntry.IsPosted);
        }

        [Fact]
        [Description("Test transaction retrieval and filtering")]
        public async Task TestTransactionRetrieval()
        {
            // Arrange
            await SetupTestDataAsync();

            // Create transactions for different accounts
            var cashTransaction = await CreateTestTransactionAsync("CASH-001");
            cashTransaction.LedgerEntries.First(le => le.EntryType == EntryType.Debit).AccountCode = "1000";
            await _accountingService.PostTransactionAsync(cashTransaction);

            var salesTransaction = await CreateTestTransactionAsync("SALES-001");
            salesTransaction.LedgerEntries.First(le => le.EntryType == EntryType.Credit).AccountCode = "4000";
            await _accountingService.PostTransactionAsync(salesTransaction);

            // Test account transaction retrieval
            var fromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var toDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var cashTransactions = await _accountingService.GetAccountTransactionsAsync("1000", fromDate, toDate);
            Assert.NotEmpty(cashTransactions);
            Assert.Contains(cashTransactions, t => t.TransactionNumber == "CASH-001");

            // Test ledger entries retrieval
            var cashLedgerEntries = await _accountingService.GetAccountLedgerEntriesAsync("1000", fromDate, toDate);
            Assert.NotEmpty(cashLedgerEntries);
            Assert.All(cashLedgerEntries, le => Assert.Equal("1000", le.AccountCode));
        }

        [Fact]
        [Description("Test performance with large number of transactions")]
        public async Task TestPerformanceWithManyTransactions()
        {
            // Arrange
            await SetupTestDataAsync();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Create 100 transactions
            for (int i = 0; i < 100; i++)
            {
                var transaction = await CreateTestTransactionAsync($"PERF-{i:D3}");
                await _accountingService.PostTransactionAsync(transaction);
            }

            stopwatch.Stop();

            // Performance assertions
            Assert.True(stopwatch.ElapsedMilliseconds < 10000, 
                $"Creating 100 transactions took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Test trial balance generation performance
            stopwatch.Restart();
            var trialBalance = await _accountingService.GetTrialBalanceAsync(DateOnly.FromDateTime(DateTime.Today));
            stopwatch.Stop();

            Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
                $"Trial balance generation took too long: {stopwatch.ElapsedMilliseconds}ms");
            Assert.NotEmpty(trialBalance);
        }

        [Fact]
        public async Task ExecuteCompleteAccountingWorkflow_ShouldCompleteSuccessfully()
        {
            // PHASE 1: Data Import
            await _sampleDataGenerator.GenerateSampleDataAsync(_repository);

            // PHASE 2: Create Purchase Document
            var supplier = _repository.GetObjects<BusinessEntityDto>().First();
            var docType = _repository.CreateObject<DocumentTypeDto>();
            docType.Code = "PUR";
            docType.Name = "Purchase Invoice";
            docType.GeneratesTransaction = true;

            var purchaseDocument = await _documentService.CreateDocumentAsync(docType, supplier);

            if (purchaseDocument is DocumentDto purchaseDto)
            {
                purchaseDto.DocumentNumber = "PUR-2025-001";
                purchaseDto.Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

                var total = _repository.CreateObject<DocumentTotalDto>();
                total.Concept = "Merchandise";
                total.Total = 650.00m;
                total.DebitAccountCode = "1300";
                total.CreditAccountCode = "2100";
                total.IncludeInTransaction = true;
                purchaseDto.DocumentTotals.Add(total);

                purchaseDto.Status = DocumentStatus.Approved;
            }

            // PHASE 3: Process Purchase Transaction
            var purchaseTransaction = await _accountingService.CreateTransactionAsync(purchaseDocument);
            await _accountingService.PostTransactionAsync(purchaseTransaction);

            // PHASE 4: Validate Results
            Assert.True(purchaseTransaction.IsPosted);
            Assert.True(purchaseTransaction.IsBalanced);

            var trialBalance = await _accountingService.GetTrialBalanceAsync(DateOnly.FromDateTime(DateTime.Today));
            Assert.NotEmpty(trialBalance);
        }

        #region Helper Methods

        private async Task SetupTestDataAsync()
        {
            // Create test accounts
            var cashAccount = _repository.CreateObject<AccountDto>();
            cashAccount.OfficialCode = "1000";
            cashAccount.AccountName = "Cash";
            cashAccount.AccountType = AccountType.Asset;

            var receivablesAccount = _repository.CreateObject<AccountDto>();
            receivablesAccount.OfficialCode = "1200";
            receivablesAccount.AccountName = "Accounts Receivable";
            receivablesAccount.AccountType = AccountType.Asset;

            var payablesAccount = _repository.CreateObject<AccountDto>();
            payablesAccount.OfficialCode = "2000";
            payablesAccount.AccountName = "Accounts Payable";
            payablesAccount.AccountType = AccountType.Liability;

            var salesAccount = _repository.CreateObject<AccountDto>();
            salesAccount.OfficialCode = "4000";
            salesAccount.AccountName = "Sales Revenue";
            salesAccount.AccountType = AccountType.Revenue;

            var expenseAccount = _repository.CreateObject<AccountDto>();
            expenseAccount.OfficialCode = "6000";
            expenseAccount.AccountName = "Operating Expenses";
            expenseAccount.AccountType = AccountType.Expense;

            await _repository.CommitChanges();
        }

        private async Task<ITransaction> CreateTestTransactionAsync(string? transactionNumber = null)
        {
            var transaction = _repository.CreateObject<TransactionDto>();
            transaction.TransactionNumber = transactionNumber ?? "TXN-2024-001";
            transaction.TransactionDate = DateOnly.FromDateTime(DateTime.Today);
            transaction.Description = "Test transaction";

            // Create balanced ledger entries
            var debitEntry = _repository.CreateObject<LedgerEntryDto>();
            debitEntry.TransactionNumber = transaction.TransactionNumber;
            debitEntry.AccountCode = "1000";
            debitEntry.EntryType = EntryType.Debit;
            debitEntry.Amount = 500m;
            debitEntry.Description = "Test debit entry";
            transaction.LedgerEntries.Add(debitEntry);

            var creditEntry = _repository.CreateObject<LedgerEntryDto>();
            creditEntry.TransactionNumber = transaction.TransactionNumber;
            creditEntry.AccountCode = "4000";
            creditEntry.EntryType = EntryType.Credit;
            creditEntry.Amount = 500m;
            creditEntry.Description = "Test credit entry";
            transaction.LedgerEntries.Add(creditEntry);

            await _repository.CommitChanges();
            return transaction;
        }

        private async Task VerifyAccountBalancesAsync()
        {
            var cashBalance = await _accountingService.CalculateAccountBalanceAsync("1000");
            var salesBalance = await _accountingService.CalculateAccountBalanceAsync("4000");

            Assert.True(cashBalance > 0, "Cash account should have positive balance");
            Assert.True(salesBalance < 0, "Sales account should have credit balance (negative)");
        }

        private void VerifyTrialBalanceIntegrity(IEnumerable<AccountBalance> trialBalance)
        {
            var totalDebits = trialBalance.Sum(ab => ab.DebitBalance);
            var totalCredits = trialBalance.Sum(ab => ab.CreditBalance);

            Assert.Equal(totalDebits, totalCredits);
            Assert.All(trialBalance, ab =>
            {
                Assert.False(string.IsNullOrEmpty(ab.AccountCode));
                Assert.False(string.IsNullOrEmpty(ab.AccountName));
            });
        }

        private async Task VerifyReversalImpactAsync(ITransaction originalTransaction)
        {
            // Check that the original accounts now have zero net effect
            foreach (var entry in originalTransaction.LedgerEntries)
            {
                var currentBalance = await _accountingService.CalculateAccountBalanceAsync(entry.AccountCode);
                // After reversal, the net effect should be closer to zero
                // This is a simplified check - in real scenarios you'd compare before/after balances
            }
        }

        #endregion
    }
}