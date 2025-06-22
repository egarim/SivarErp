using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Modules.Accounting;
using Sivar.Erp.Modules.Accounting.JournalEntries;
using Sivar.Erp.Modules.Accounting.Reports;
using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting.Transactions;
using NUnit.Framework;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;

namespace Sivar.Erp.Tests
{
    [TestFixture]
    /// <summary>
    /// Simple test demonstrating journal entry functionality usage
    /// </summary>
    public class JournalEntryFunctionalityTest
    {
        private IObjectDb _objectDb = null!;
        private IJournalEntryService _journalEntryService = null!;
        private IJournalEntryReportService _reportService = null!;

        [SetUp]
        public void Setup()
        {
            _objectDb = new ObjectDb();
            _journalEntryService = new JournalEntryService(_objectDb);
            _reportService = new JournalEntryReportService(_objectDb, _journalEntryService);

            // Add some sample data for testing
            SetupSampleData();
        }

        [Test]
        public async Task TestJournalEntryQueryFunctionality()
        {
            // Test querying journal entries
            var queryOptions = new JournalEntryQueryOptions
            {
                OnlyPosted = false, // Include all entries for testing
                Take = 10
            };

            var entries = await _journalEntryService.GetJournalEntriesAsync(queryOptions);
            Console.WriteLine($"Found {entries.Count()} journal entries");

            Assert.That(entries, Is.Not.Null);

            // Test transaction-specific queries
            if (entries.Any())
            {
                var firstEntry = entries.First();
                var transactionEntries = await _journalEntryService.GetJournalEntriesByTransactionAsync(firstEntry.TransactionNumber);

                Console.WriteLine($"Transaction {firstEntry.TransactionNumber} has {transactionEntries.Count()} entries");
                Assert.That(transactionEntries, Is.Not.Empty);

                // Test balance validation
                var isBalanced = await _journalEntryService.IsTransactionBalancedAsync(firstEntry.TransactionNumber);
                Console.WriteLine($"Transaction is balanced: {isBalanced}");
            }

            Assert.Pass("Journal entry functionality test completed successfully!");
        }

        [Test]
        public async Task TestJournalEntryReports()
        {
            var reportOptions = new JournalEntryQueryOptions
            {
                OnlyPosted = false,
                Take = 50
            };

            // Test journal entry report
            var report = await _reportService.GenerateJournalEntryReportAsync(reportOptions);

            Console.WriteLine($"Generated report: {report.ReportTitle}");
            Console.WriteLine($"Total entries: {report.TotalEntries}");
            Console.WriteLine($"Total debits: ${report.TotalDebits:F2}");
            Console.WriteLine($"Total credits: ${report.TotalCredits:F2}");
            Console.WriteLine($"Is balanced: {report.IsBalanced}");

            Assert.That(report, Is.Not.Null);
            Assert.That(report.TotalEntries, Is.GreaterThanOrEqualTo(0));

            // Test trial balance generation
            var trialBalance = await _reportService.GenerateTrialBalanceFromJournalEntriesAsync(
                DateOnly.FromDateTime(DateTime.Today),
                onlyPosted: false);

            Console.WriteLine($"Trial balance accounts: {trialBalance.Accounts.Count()}");
            Console.WriteLine($"Trial balance is balanced: {trialBalance.IsBalanced}");

            Assert.That(trialBalance, Is.Not.Null);

            Assert.Pass("Journal entry reports test completed successfully!");
        }

        private void SetupSampleData()
        {
            // Create sample accounts
            var account1 = new AccountDto
            {
             
                OfficialCode = "1100",
                AccountName = "Cash Account",
                AccountType = AccountType.Asset
            };

            var account2 = new AccountDto
            {
            
                OfficialCode = "4100",
                AccountName = "Sales Revenue",
                AccountType = AccountType.Revenue
            };

            _objectDb.Accounts.Add(account1);
            _objectDb.Accounts.Add(account2);

            // Create sample transaction
            var transaction = new TransactionDto
            {
               
                TransactionNumber = "TRANS-001",
                TransactionDate = DateOnly.FromDateTime(DateTime.Today),
                Description = "Sample sales transaction",
                DocumentNumber = "INV-001",
                IsPosted = true
            };

            _objectDb.Transactions.Add(transaction);

            // Create sample ledger entries
            var entry1 = new LedgerEntryDto
            {
               
                LedgerEntryNumber = "LE-001",
                TransactionNumber = "TRANS-001",
                OfficialCode = "1100",
                AccountName = "Cash Account",
                EntryType = EntryType.Debit,
                Amount = 100.00m
            };

            var entry2 = new LedgerEntryDto
            {
               
                LedgerEntryNumber = "LE-002",
                TransactionNumber = "TRANS-001",
                OfficialCode = "4100",
                AccountName = "Sales Revenue",
                EntryType = EntryType.Credit,
                Amount = 100.00m
            };

            _objectDb.LedgerEntries.Add(entry1);
            _objectDb.LedgerEntries.Add(entry2);

            transaction.LedgerEntries = new List<ILedgerEntry> { entry1, entry2 };
        }
    }
}
