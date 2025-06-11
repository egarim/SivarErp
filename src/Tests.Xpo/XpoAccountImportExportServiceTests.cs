using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;
using DevExpress.Xpo;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.Xpo.ChartOfAccounts;
using Sivar.Erp.Xpo.Core;
using Sivar.Erp.Xpo.Services;

namespace Sivar.Erp.Tests.Xpo.ChartOfAccounts
{
    /// <summary>
    /// Tests for the XPO account import/export service with ParentOfficialCode support
    /// </summary>
    [TestFixture]
    public class XpoAccountImportExportServiceTests
    {
        private IAuditService _auditService;
        private XpoAccountImportExportService _importExportService;
        private UnitOfWork _unitOfWork;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Initialize the XPO data layer with an in-memory database for testing
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(
                "XpoProvider=InMemoryDataStore", DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
        }

        [SetUp]
        public void Setup()
        {
            _unitOfWork = new UnitOfWork();
            _auditService = new XpoAuditService();
            _importExportService = new XpoAccountImportExportService(_auditService);
        }

        [TearDown]
        public void TearDown()
        {
            _unitOfWork?.Dispose();
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
            string csvContent = "AccountName,OfficialCode,AccountType,ParentOfficialCode,BalanceAndIncomeLineId";

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
            string csvContent = "AccountName,OfficialCode,AccountType,ParentOfficialCode\nCash,11000,Asset,\nPetty Cash,11010,Asset,11000";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedAccounts.Count(), Is.EqualTo(2));

            var accountsList = importedAccounts.ToList();
            var cashAccount = accountsList.FirstOrDefault(a => a.AccountName == "Cash");
            var pettyCashAccount = accountsList.FirstOrDefault(a => a.AccountName == "Petty Cash");

            Assert.That(cashAccount, Is.Not.Null);
            Assert.That(cashAccount.OfficialCode, Is.EqualTo("11000"));
            Assert.That(cashAccount.AccountType, Is.EqualTo(AccountType.Asset));
            Assert.That(cashAccount.ParentOfficialCode, Is.Null.Or.Empty);

            Assert.That(pettyCashAccount, Is.Not.Null);
            Assert.That(pettyCashAccount.ParentOfficialCode, Is.EqualTo("11000"));
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidCsvWithQuotes_ImportsAccounts()
        {
            // Arrange
            string csvContent = "\"AccountName\",\"OfficialCode\",\"AccountType\",\"ParentOfficialCode\"\n\"Cash, on hand\",\"11000\",\"Asset\",\"\"\n\"Accounts Receivable\",\"12000\",\"Asset\",\"\"";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedAccounts.Count(), Is.EqualTo(2));

            var accountsList = importedAccounts.ToList();
            var cashAccount = accountsList.FirstOrDefault(a => a.AccountName == "Cash, on hand");

            Assert.That(cashAccount, Is.Not.Null);
            Assert.That(cashAccount.OfficialCode, Is.EqualTo("11000"));
            Assert.That(cashAccount.ParentOfficialCode, Is.Null.Or.Empty);
        }

        [Test]
        public async Task ImportFromCsvAsync_InvalidAccountType_UsesDefault()
        {
            // Arrange
            string csvContent = "AccountName,OfficialCode,AccountType,ParentOfficialCode\nCash,11000,InvalidType,";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedAccounts.Count(), Is.EqualTo(1));
            Assert.That(importedAccounts.First().AccountType, Is.EqualTo(AccountType.Asset));
            Assert.That(importedAccounts.First().ParentOfficialCode, Is.Null.Or.Empty);
        }

        [Test]
        public async Task ImportFromCsvAsync_WithParentOfficialCode_ImportsCorrectly()
        {
            // Arrange
            string csvContent = "AccountName,OfficialCode,AccountType,ParentOfficialCode\nCurrent Assets,11000,Asset,\nCash,11100,Asset,11000\nBank Account,11110,Asset,11100";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedAccounts.Count(), Is.EqualTo(3));

            var accountsList = importedAccounts.ToList();

            var currentAssets = accountsList.FirstOrDefault(a => a.AccountName == "Current Assets");
            Assert.That(currentAssets, Is.Not.Null);
            Assert.That(currentAssets.ParentOfficialCode, Is.Null.Or.Empty);

            var cash = accountsList.FirstOrDefault(a => a.AccountName == "Cash");
            Assert.That(cash, Is.Not.Null);
            Assert.That(cash.ParentOfficialCode, Is.EqualTo("11000"));

            var bankAccount = accountsList.FirstOrDefault(a => a.AccountName == "Bank Account");
            Assert.That(bankAccount, Is.Not.Null);
            Assert.That(bankAccount.ParentOfficialCode, Is.EqualTo("11100"));
        }

        [Test]
        public async Task ImportFromCsvAsync_SetsAuditFields()
        {
            // Arrange
            string csvContent = "AccountName,OfficialCode,AccountType,ParentOfficialCode\nCash,11000,Asset,";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            var account = importedAccounts.First();
            Assert.That(account.InsertedBy, Is.EqualTo("Test User"));
            Assert.That(account.UpdatedBy, Is.EqualTo("Test User"));
            Assert.That(account.InsertedAt, Is.GreaterThan(DateTime.MinValue));
            Assert.That(account.UpdatedAt, Is.GreaterThan(DateTime.MinValue));
            Assert.That(account.ParentOfficialCode, Is.Null.Or.Empty);
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
            Assert.That(csv, Does.Contain("ParentOfficialCode"));
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
            Assert.That(csv, Does.Contain("ParentOfficialCode"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(1));
        }

        [Test]
        public async Task ExportToCsvAsync_WithAccounts_ReturnsCsvContent()
        {
            // Arrange
            var accounts = new List<IAccount>
            {
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Cash",
                    OfficialCode = "11000",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = null,
                    BalanceAndIncomeLineId = null,
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Test User",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    IsArchived = false
                },
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Accounts Receivable",
                    OfficialCode = "12000",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = null,
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
            Assert.That(csv, Does.Contain("ParentOfficialCode"));
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
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Cash, on hand",
                    OfficialCode = "11000",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = null,
                    BalanceAndIncomeLineId = null
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(accounts);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("\"Cash, on hand\""));
        }

        [Test]
        public async Task ExportToCsvAsync_WithParentChildAccounts_IncludesParentOfficialCode()
        {
            // Arrange
            var accounts = new List<IAccount>
            {
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Current Assets",
                    OfficialCode = "11000",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = null,
                    BalanceAndIncomeLineId = null,
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Test User",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    IsArchived = false
                },
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Cash",
                    OfficialCode = "11100",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = "11000",
                    BalanceAndIncomeLineId = null,
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Test User",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    IsArchived = false
                },
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Bank Account",
                    OfficialCode = "11110",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = "11100",
                    BalanceAndIncomeLineId = null,
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
            Assert.That(csv, Does.Contain("ParentOfficialCode"));
            Assert.That(csv, Does.Contain("Current Assets"));
            Assert.That(csv, Does.Contain("Cash"));
            Assert.That(csv, Does.Contain("Bank Account"));
            Assert.That(csv, Does.Contain("11000"));
            Assert.That(csv, Does.Contain("11100"));
            Assert.That(csv, Does.Contain("11110"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(4)); // Header + 3 data rows
        }

        #endregion

        #region Round Trip Tests

        [Test]
        public async Task RoundTrip_ExportThenImport_PreservesAccounts()
        {
            // Arrange
            var originalAccounts = new List<IAccount>
            {
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Cash",
                    OfficialCode = "11000",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = null,
                    BalanceAndIncomeLineId = null,
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Test User",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    IsArchived = false
                },
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Petty Cash",
                    OfficialCode = "11010",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = "11000",
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

            var importedList = importedAccounts.ToList();

            // Check first account
            var firstOriginal = originalAccounts[0];
            var firstImported = importedList.FirstOrDefault(a => a.OfficialCode == firstOriginal.OfficialCode);
            Assert.That(firstImported, Is.Not.Null);
            Assert.That(firstImported.AccountName, Is.EqualTo(firstOriginal.AccountName));
            Assert.That(firstImported.AccountType, Is.EqualTo(firstOriginal.AccountType));
            Assert.That(firstImported.ParentOfficialCode, Is.EqualTo(firstOriginal.ParentOfficialCode));

            // Check second account
            var secondOriginal = originalAccounts[1];
            var secondImported = importedList.FirstOrDefault(a => a.OfficialCode == secondOriginal.OfficialCode);
            Assert.That(secondImported, Is.Not.Null);
            Assert.That(secondImported.AccountName, Is.EqualTo(secondOriginal.AccountName));
            Assert.That(secondImported.AccountType, Is.EqualTo(secondOriginal.AccountType));
            Assert.That(secondImported.ParentOfficialCode, Is.EqualTo(secondOriginal.ParentOfficialCode));
        }

        [Test]
        public async Task RoundTrip_ExportThenImport_PreservesParentChildRelationships()
        {
            // Arrange
            var originalAccounts = new List<IAccount>
            {
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Assets",
                    OfficialCode = "10000",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = null,
                    BalanceAndIncomeLineId = null,
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Test User",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    IsArchived = false
                },
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Current Assets",
                    OfficialCode = "11000",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = "10000",
                    BalanceAndIncomeLineId = null,
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Test User",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    IsArchived = false
                },
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Cash",
                    OfficialCode = "11100",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = "11000",
                    BalanceAndIncomeLineId = Guid.NewGuid(),
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Test User",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    IsArchived = false
                },
                new XpoAccount(_unitOfWork)
                {
                    Id = Guid.NewGuid(),
                    AccountName = "Bank Account",
                    OfficialCode = "11110",
                    AccountType = AccountType.Asset,
                    ParentOfficialCode = "11100",
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

            // Check that all parent-child relationships are preserved
            var importedList = importedAccounts.ToList();

            var assets = importedList.FirstOrDefault(a => a.OfficialCode == "10000");
            Assert.That(assets, Is.Not.Null);
            Assert.That(assets.ParentOfficialCode, Is.Null.Or.Empty);

            var currentAssets = importedList.FirstOrDefault(a => a.OfficialCode == "11000");
            Assert.That(currentAssets, Is.Not.Null);
            Assert.That(currentAssets.ParentOfficialCode, Is.EqualTo("10000"));

            var cash = importedList.FirstOrDefault(a => a.OfficialCode == "11100");
            Assert.That(cash, Is.Not.Null);
            Assert.That(cash.ParentOfficialCode, Is.EqualTo("11000"));

            var bankAccount = importedList.FirstOrDefault(a => a.OfficialCode == "11110");
            Assert.That(bankAccount, Is.Not.Null);
            Assert.That(bankAccount.ParentOfficialCode, Is.EqualTo("11100"));
        }

        #endregion

        #region Parent-Child Relationship Tests

        [Test]
        public async Task ImportFromCsvAsync_WithComplexHierarchy_CreatesCorrectRelationships()
        {
            // Arrange - Create a complex 4-level hierarchy
            string csvContent = @"AccountName,OfficialCode,AccountType,ParentOfficialCode
Assets,10000,Asset,
Current Assets,11000,Asset,10000
Cash,11100,Asset,11000
Petty Cash,11110,Asset,11100
Bank Account - Main,11120,Asset,11100
Bank Account - Payroll,11130,Asset,11100
Accounts Receivable,11200,Asset,11000
Trade Receivables,11210,Asset,11200
Employee Advances,11220,Asset,11200
Fixed Assets,12000,Asset,10000
Equipment,12100,Asset,12000
Office Equipment,12110,Asset,12100
Computer Equipment,12120,Asset,12100";

            // Act
            var (importedAccounts, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty, $"Import errors: {string.Join(", ", errors)}");
            Assert.That(importedAccounts.Count(), Is.EqualTo(13));

            var accountsList = importedAccounts.ToList();

            // Verify root level
            var assets = accountsList.FirstOrDefault(a => a.OfficialCode == "10000");
            Assert.That(assets, Is.Not.Null);
            Assert.That(assets.ParentOfficialCode, Is.Null.Or.Empty);

            // Verify level 2
            var currentAssets = accountsList.FirstOrDefault(a => a.OfficialCode == "11000");
            Assert.That(currentAssets, Is.Not.Null);
            Assert.That(currentAssets.ParentOfficialCode, Is.EqualTo("10000"));

            var fixedAssets = accountsList.FirstOrDefault(a => a.OfficialCode == "12000");
            Assert.That(fixedAssets, Is.Not.Null);
            Assert.That(fixedAssets.ParentOfficialCode, Is.EqualTo("10000"));

            // Verify level 3
            var cash = accountsList.FirstOrDefault(a => a.OfficialCode == "11100");
            Assert.That(cash, Is.Not.Null);
            Assert.That(cash.ParentOfficialCode, Is.EqualTo("11000"));

            var equipment = accountsList.FirstOrDefault(a => a.OfficialCode == "12100");
            Assert.That(equipment, Is.Not.Null);
            Assert.That(equipment.ParentOfficialCode, Is.EqualTo("12000"));

            // Verify level 4
            var pettyCash = accountsList.FirstOrDefault(a => a.OfficialCode == "11110");
            Assert.That(pettyCash, Is.Not.Null);
            Assert.That(pettyCash.ParentOfficialCode, Is.EqualTo("11100"));

            var computerEquipment = accountsList.FirstOrDefault(a => a.OfficialCode == "12120");
            Assert.That(computerEquipment, Is.Not.Null);
            Assert.That(computerEquipment.ParentOfficialCode, Is.EqualTo("12100"));
        }

        #endregion
    }
}
