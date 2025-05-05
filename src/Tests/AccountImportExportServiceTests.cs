using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Sivar.Erp.ChartOfAccounts;

namespace Sivar.Erp.Tests.ChartOfAccounts
{
    /// <summary>
    /// Tests for the account import/export service
    /// </summary>
    [TestFixture]
    public class AccountImportExportServiceTests
    {
        private IAuditService _auditService;
        private IAccountImportExportService _importExportService;

        [SetUp]
        public void Setup()
        {
            _auditService = new AuditService();
            _importExportService = new AccountImportExportService(_auditService);
        }

        #region Import Tests

        [Test]
        public async Task ImportFromCsvAsync_EmptyContent_ReturnsError()
        {
            // Arrange
            string csvContent = string.Empty;

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedAccounts, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("empty"));
        }

        [Test]
        public async Task ImportFromCsvAsync_HeaderOnly_ReturnsError()
        {
            // Arrange
            string csvContent = "AccountName,OfficialCode,AccountType,BalanceAndIncomeLineId";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedAccounts, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("no data"));
        }

        [Test]
        public async Task ImportFromCsvAsync_MissingRequiredHeader_ReturnsError()
        {
            // Arrange
            string csvContent = "Name,OfficialCode,Type\nCash,11000,Asset";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedAccounts, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("AccountName"));
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidContent_ImportsAccounts()
        {
            // Arrange
            string csvContent = "AccountName,OfficialCode,AccountType\nCash,11000,Asset\nAccounts Receivable,12000,Asset";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedAccounts.Count(), Is.EqualTo(2));
            Assert.That(importedAccounts.First().AccountName, Is.EqualTo("Cash"));
            Assert.That(importedAccounts.First().OfficialCode, Is.EqualTo("11000"));
            Assert.That(importedAccounts.First().AccountType, Is.EqualTo(AccountType.Asset));
            Assert.That(importedAccounts.Last().AccountName, Is.EqualTo("Accounts Receivable"));
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidCsvWithQuotes_ImportsAccounts()
        {
            // Arrange
            string csvContent = "\"AccountName\",\"OfficialCode\",\"AccountType\"\n\"Cash, on hand\",\"11000\",\"Asset\"\n\"Accounts Receivable\",\"12000\",\"Asset\"";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedAccounts.Count(), Is.EqualTo(2));
            Assert.That(importedAccounts.First().AccountName, Is.EqualTo("Cash, on hand"));
            Assert.That(importedAccounts.First().OfficialCode, Is.EqualTo("11000"));
        }

        [Test]
        public async Task ImportFromCsvAsync_InvalidAccountType_UsesDefault()
        {
            // Arrange
            string csvContent = "AccountName,OfficialCode,AccountType\nCash,11000,InvalidType";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedAccounts.Count(), Is.EqualTo(1));
            Assert.That(importedAccounts.First().AccountType, Is.EqualTo(AccountType.Asset));
        }

        [Test]
        public async Task ImportFromCsvAsync_SetsAuditFields()
        {
            // Arrange
            string csvContent = "AccountName,OfficialCode,AccountType\nCash,11000,Asset";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedAccounts.First().InsertedBy, Is.EqualTo("Test User"));
            Assert.That(importedAccounts.First().UpdatedBy, Is.EqualTo("Test User"));
            Assert.That(importedAccounts.First().InsertedAt, Is.GreaterThan(DateTime.MinValue));
            Assert.That(importedAccounts.First().UpdatedAt, Is.GreaterThan(DateTime.MinValue));
        }

        #endregion

        #region Export Tests

        [Test]
        public async Task ExportToCsvAsync_NullAccounts_ReturnsHeaderOnly()
        {
            // Act
            string csv = await _importExportService.ExportToCsvAsync(null);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("AccountName"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(1));
        }

        [Test]
        public async Task ExportToCsvAsync_EmptyAccounts_ReturnsHeaderOnly()
        {
            // Act
            string csv = await _importExportService.ExportToCsvAsync(Array.Empty<IAccount>());

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("AccountName"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(1));
        }

        [Test]
        public async Task ExportToCsvAsync_WithAccounts_ReturnsCsvContent()
        {
            // Arrange
            var accounts = new List<IAccount>
            {
                new AccountDto
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Cash",
                    OfficialCode = "11000",
                    AccountType = AccountType.Asset,
                    BalanceAndIncomeLineId = null,
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Test User",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    IsArchived = false
                },
                new AccountDto
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Accounts Receivable",
                    OfficialCode = "12000",
                    AccountType = AccountType.Asset,
                    BalanceAndIncomeLineId = Guid.NewGuid(),
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Test User",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    IsArchived = false
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(accounts);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("AccountName"));
            Assert.That(csv, Does.Contain("Cash"));
            Assert.That(csv, Does.Contain("Accounts Receivable"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(3)); // Header + 2 data rows
        }

        [Test]
        public async Task ExportToCsvAsync_WithCommasInData_QuotesValues()
        {
            // Arrange
            var accounts = new List<IAccount>
            {
                new AccountDto
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Cash, on hand",
                    OfficialCode = "11000",
                    AccountType = AccountType.Asset,
                    BalanceAndIncomeLineId = null
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(accounts);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("\"Cash, on hand\""));
        }

        #endregion

        #region Round Trip Tests

        [Test]
        public async Task RoundTrip_ExportThenImport_PreservesAccounts()
        {
            // Arrange
            var originalAccounts = new List<IAccount>
            {
                new AccountDto
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Cash",
                    OfficialCode = "11000",
                    AccountType = AccountType.Asset,
                    BalanceAndIncomeLineId = null,
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Test User",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    IsArchived = false
                },
                new AccountDto
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Accounts Receivable",
                    OfficialCode = "12000",
                    AccountType = AccountType.Asset,
                    BalanceAndIncomeLineId = Guid.NewGuid(),
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Test User",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    IsArchived = false
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(originalAccounts);
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csv, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedAccounts.Count(), Is.EqualTo(originalAccounts.Count));

            // Check first account
            var firstOriginal = originalAccounts[0];
            var firstImported = importedAccounts.First();
            Assert.That(firstImported.AccountName, Is.EqualTo(firstOriginal.AccountName));
            Assert.That(firstImported.OfficialCode, Is.EqualTo(firstOriginal.OfficialCode));
            Assert.That(firstImported.AccountType, Is.EqualTo(firstOriginal.AccountType));

            // Check second account
            var secondOriginal = originalAccounts[1];
            var secondImported = importedAccounts.Skip(1).First();
            Assert.That(secondImported.AccountName, Is.EqualTo(secondOriginal.AccountName));
            Assert.That(secondImported.OfficialCode, Is.EqualTo(secondOriginal.OfficialCode));
            Assert.That(secondImported.AccountType, Is.EqualTo(secondOriginal.AccountType));
        }

        #endregion
    }
}