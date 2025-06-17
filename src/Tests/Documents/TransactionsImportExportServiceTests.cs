using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Services.ImportExport;

namespace Sivar.Erp.Tests.Documents
{
    /// <summary>
    /// Unit tests for the TransactionImportHelper class
    /// </summary>
    [TestFixture]
    public class TransactionsImportExportServiceTests
    {
        private TransactionsImportExportService _importHelper;
        private List<AccountDto> _accounts;
        private List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> _testTransactions;

        [SetUp]
        public void Setup()
        {
            // Set up test accounts
            _accounts = new List<AccountDto>
            {
                new AccountDto
                {
                   
                    OfficialCode = "1000",
                    AccountName = "Cash",
                    AccountType = AccountType.Asset
                },
                new AccountDto
                {
                   
                    OfficialCode = "2000",
                    AccountName = "Accounts Payable",
                    AccountType = AccountType.Liability
                },
                new AccountDto
                {
                   
                    OfficialCode = "4000",
                    AccountName = "Sales Revenue",
                    AccountType = AccountType.Revenue
                },
                new AccountDto
                {
                   
                    OfficialCode = "5000",
                    AccountName = "Office Expenses",
                    AccountType = AccountType.Expense
                }
            };

            _importHelper = new TransactionsImportExportService(_accounts);

            // Create test transactions
            var transaction1Id = Guid.Parse("b0000000-0000-0000-0000-000000000001");
            var transaction2Id = Guid.Parse("b0000000-0000-0000-0000-000000000002");
            var documentId = Guid.Parse("c0000000-0000-0000-0000-000000000001");

            var transaction1 = new TransactionDto
            {
                Oid = transaction1Id,
                DocumentId = documentId,
                TransactionDate = new DateOnly(2025, 6, 11),
                Description = "Office Supplies Purchase"
            };

            var transaction1Entries = new List<LedgerEntryDto>
            {
                new LedgerEntryDto
                {
                    Oid = Guid.Parse("d0000000-0000-0000-0000-000000000001"),
                    TransactionId = transaction1Id,
                 
                    EntryType = EntryType.Debit,
                    Amount = 150.00m,
                    AccountName = _accounts[3].AccountName,
                    OfficialCode = _accounts[3].OfficialCode
                },
                new LedgerEntryDto
                {
                    Oid = Guid.Parse("d0000000-0000-0000-0000-000000000002"),
                    TransactionId = transaction1Id,
                  
                    EntryType = EntryType.Credit,
                    Amount = 150.00m,
                    AccountName = _accounts[0].AccountName,
                    OfficialCode = _accounts[0].OfficialCode
                }
            };

            var transaction2 = new TransactionDto
            {
                Oid = transaction2Id,
                DocumentId = documentId,
                TransactionDate = new DateOnly(2025, 6, 11),
                Description = "Client Invoice"
            };

            var transaction2Entries = new List<LedgerEntryDto>
            {
                new LedgerEntryDto
                {
                    Oid = Guid.Parse("d0000000-0000-0000-0000-000000000003"),
                    TransactionId = transaction2Id,

                    EntryType = EntryType.Debit,
                    Amount = 500.00m,
                    AccountName = _accounts[0].AccountName,
                    OfficialCode = _accounts[0].OfficialCode
                },
                new LedgerEntryDto
                {
                    Oid = Guid.Parse("d0000000-0000-0000-0000-000000000004"),
                    TransactionId = transaction2Id,

                    EntryType = EntryType.Credit,
                    Amount = 500.00m,
                    AccountName = _accounts[2].AccountName,
                    OfficialCode = _accounts[2].OfficialCode
                }
            };

            _testTransactions = new List<(TransactionDto, List<LedgerEntryDto>)>
            {
                (transaction1, transaction1Entries),
                (transaction2, transaction2Entries)
            };
        }

        [Test]
        public void ExportTransactionsToCsv_WithValidTransactions_GeneratesCorrectCsvText()
        {
            // Arrange
            // (Setup in the [SetUp] method)

            // Act
            string csvText = _importHelper.ExportTransactionsToCsv(_testTransactions);

            // Assert
            Assert.That(csvText, Is.Not.Null);
            Assert.That(csvText, Does.Contain("# TRANSACTIONS"));
            Assert.That(csvText, Does.Contain("# LEDGER ENTRIES"));

            // Check transaction data
            Assert.That(csvText, Does.Contain("b0000000-0000-0000-0000-000000000001"));
            Assert.That(csvText, Does.Contain("Office Supplies Purchase"));
            Assert.That(csvText, Does.Contain("b0000000-0000-0000-0000-000000000002"));
            Assert.That(csvText, Does.Contain("Client Invoice"));

            // Check ledger entry data
            Assert.That(csvText, Does.Contain("d0000000-0000-0000-0000-000000000001"));
            Assert.That(csvText, Does.Contain("Office Expenses"));
            Assert.That(csvText, Does.Contain("5000"));
            Assert.That(csvText, Does.Contain("d0000000-0000-0000-0000-000000000003"));
            Assert.That(csvText, Does.Contain("Cash"));
            Assert.That(csvText, Does.Contain("1000"));

            // Check amounts and entry types
            Assert.That(csvText, Does.Contain("150.00"));
            Assert.That(csvText, Does.Contain("500.00"));
            Assert.That(csvText, Does.Contain("Debit"));
            Assert.That(csvText, Does.Contain("Credit"));
        }

        [Test]
        public void ImportFromCsv_WithValidCsvText_ReturnsCorrectTransactions()
        {
            // Arrange
            string csvText = _importHelper.ExportTransactionsToCsv(_testTransactions);

            // Act
            var importedTransactions = _importHelper.ImportFromCsv(csvText);

            // Assert
            Assert.That(importedTransactions, Is.Not.Null);
            Assert.That(importedTransactions.Count, Is.EqualTo(_testTransactions.Count));

            // Check first transaction
            var firstTransaction = importedTransactions.FirstOrDefault(t => t.Transaction.Oid == _testTransactions[0].Transaction.Oid);
            Assert.That(firstTransaction.Transaction, Is.Not.Null);
            Assert.That(firstTransaction.Transaction.Description, Is.EqualTo("Office Supplies Purchase"));
            Assert.That(firstTransaction.Transaction.TransactionDate, Is.EqualTo(new DateOnly(2025, 6, 11)));
            Assert.That(firstTransaction.Entries.Count, Is.EqualTo(2));

            // Check first transaction entries
            var expenseEntry = firstTransaction.Entries.FirstOrDefault(e => e.EntryType == EntryType.Debit);
            Assert.That(expenseEntry, Is.Not.Null);
            Assert.That(expenseEntry.Amount, Is.EqualTo(150.00m));
            Assert.That(expenseEntry.OfficialCode, Is.EqualTo("5000"));
            Assert.That(expenseEntry.AccountName, Is.EqualTo("Office Expenses"));

            var cashEntry = firstTransaction.Entries.FirstOrDefault(e => e.EntryType == EntryType.Credit);
            Assert.That(cashEntry, Is.Not.Null);
            Assert.That(cashEntry.Amount, Is.EqualTo(150.00m));
            Assert.That(cashEntry.OfficialCode, Is.EqualTo("1000"));
            Assert.That(cashEntry.AccountName, Is.EqualTo("Cash"));

            // Check second transaction
            var secondTransaction = importedTransactions.FirstOrDefault(t => t.Transaction.Oid == _testTransactions[1].Transaction.Oid);
            Assert.That(secondTransaction.Transaction, Is.Not.Null);
            Assert.That(secondTransaction.Transaction.Description, Is.EqualTo("Client Invoice"));
            Assert.That(secondTransaction.Entries.Count, Is.EqualTo(2));
        }

        [Test]
        public void ImportFromCsv_ExportTransactions_Roundtrip_PreservesAllData()
        {
            // Arrange & Act - Export then import
            string exportedCsv = _importHelper.ExportTransactionsToCsv(_testTransactions);
            var reimportedTransactions = _importHelper.ImportFromCsv(exportedCsv);

            // Export the reimported transactions
            string reexportedCsv = _importHelper.ExportTransactionsToCsv(reimportedTransactions);

            // Assert - Both CSV strings should be identical (or contain same data)
            Assert.That(reexportedCsv, Is.EqualTo(exportedCsv));

            // Verify all transaction IDs are preserved
            var originalIds = _testTransactions.Select(t => t.Transaction.Oid).ToList();
            var reimportedIds = reimportedTransactions.Select(t => t.Transaction.Oid).ToList();

            Assert.That(reimportedIds, Is.EquivalentTo(originalIds));

            // Verify all entry IDs are preserved
            var originalEntryIds = _testTransactions.SelectMany(t => t.Entries.Select(e => e.Oid)).ToList();
            var reimportedEntryIds = reimportedTransactions.SelectMany(t => t.Entries.Select(e => e.Oid)).ToList();

            Assert.That(reimportedEntryIds, Is.EquivalentTo(originalEntryIds));

            // Verify amounts are correctly preserved
            foreach (var txn in _testTransactions)
            {
                var reimportedTxn = reimportedTransactions.FirstOrDefault(t => t.Transaction.Oid == txn.Transaction.Oid);
                Assert.That(reimportedTxn.Transaction, Is.Not.Null, $"Transaction {txn.Transaction.Oid} was not reimported");

                foreach (var entry in txn.Entries)
                {
                    var reimportedEntry = reimportedTxn.Entries.FirstOrDefault(e => e.Oid == entry.Oid);
                    Assert.That(reimportedEntry, Is.Not.Null, $"Entry {entry.Oid} was not reimported");
                    Assert.That(reimportedEntry.Amount, Is.EqualTo(entry.Amount), $"Amount mismatch for entry {entry.Oid}");
                    Assert.That(reimportedEntry.EntryType, Is.EqualTo(entry.EntryType), $"EntryType mismatch for entry {entry.Oid}");
                }
            }
        }

        [Test]
        public void ExportTransactions_WithEmptyList_ReturnsHeadersOnly()
        {
            // Arrange
            var emptyList = new List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)>();

            // Act
            var (transactionsCsv, entriesCsv) = _importHelper.ExportTransactions(emptyList);

            // Assert
            Assert.That(transactionsCsv, Is.Not.Null);
            Assert.That(entriesCsv, Is.Not.Null);
            Assert.That(transactionsCsv, Does.Contain("TransactionId,Date,Description,DocumentId"));
            Assert.That(entriesCsv, Does.Contain("EntryId,TransactionId,AccountId,OfficialCode,AccountName,EntryType,Amount"));

            // Check that each CSV has only the header line
            var transactionLines = transactionsCsv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var entryLines = entriesCsv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            Assert.That(transactionLines.Length, Is.EqualTo(1));
            Assert.That(entryLines.Length, Is.EqualTo(1));
        }

        [Test]
        public void ExportTransactionsToCsv_WithEmptyList_ReturnsHeadersOnly()
        {
            // Arrange
            var emptyList = new List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)>();

            // Act
            string csvText = _importHelper.ExportTransactionsToCsv(emptyList);

            // Assert
            Assert.That(csvText, Is.Not.Null);
            Assert.That(csvText, Does.Contain("# TRANSACTIONS"));
            Assert.That(csvText, Does.Contain("TransactionId,Date,Description,DocumentId"));
            Assert.That(csvText, Does.Contain("# LEDGER ENTRIES"));
            Assert.That(csvText, Does.Contain("EntryId,TransactionId,AccountId,OfficialCode,AccountName,EntryType,Amount"));

            // Should have only headers and section markers, no data
            string[] lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines.Length, Is.EqualTo(4)); // Two headers and two section markers
        }

        [Test]
        public void ImportFromCsv_WithMalformedCsv_HandlesErrorsGracefully()
        {
            // Arrange
            string badCsv = @"# TRANSACTIONS
TransactionId,Date,Description,DocumentId
bad-guid,not-a-date,Description,another-bad-guid

# LEDGER ENTRIES
EntryId,TransactionId,AccountId,OfficialCode,AccountName,EntryType,Amount
also-bad-guid,bad-guid,not-guid,Code,Name,NotAnEntryType,not-decimal";

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                var result = _importHelper.ImportFromCsv(badCsv);
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Count, Is.EqualTo(0));
            });
        }
    }
}
