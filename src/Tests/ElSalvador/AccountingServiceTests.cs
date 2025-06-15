using NUnit.Framework;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting;
using Sivar.Erp.Services.Accounting.BalanceCalculators;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Services.ImportExport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sivar.Erp.Documents.DocumentToTransactions;

namespace Tests.ElSalvador
{
    [TestFixture]
    public class AccountingServiceTests
    {
        private readonly string _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "Tests", "ElSalvador", "Data", "New");
        private IAccountImportExportService _accountImportService;
        private AccountingService _accountingService;
        private ObjectDb _objectDb;

        [SetUp]
        public void Setup()
        {
            // Create an El Salvador-specific account validator
            var elSalvadorAccountValidator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
            _accountImportService = new AccountImportExportService(elSalvadorAccountValidator);

            // Create object database
            _objectDb = new ObjectDb();

            // Create required services
            var optionService = new OptionService();
       
            var dateTimeZoneService = new DateTimeZoneService();
            var activityStreamService = new ActivityStreamService(dateTimeZoneService,_objectDb);
            var fiscalPeriodService = new FiscalPeriodService(_objectDb);
            var accountBalanceCalculator = new AccountBalanceCalculatorServiceBase();
            var documentToTransactionService = new DocumentToTransactionService();
            var sequencerService = new SequencerService(_objectDb);

            _accountingService = new AccountingService(
                optionService,
                activityStreamService,
                dateTimeZoneService,
                fiscalPeriodService,
                accountBalanceCalculator,
                documentToTransactionService,
                sequencerService
            );
        }

        [Test]
        public async Task LoadChartOfAccountsAndPostTransaction_January2020_Success()
        {
            // Arrange
            string chartOfAccountsPath = Path.Combine(_testDataPath, "ComercialChartOfAccounts.txt");
            string csvContent = await File.ReadAllTextAsync(chartOfAccountsPath);

            // Import the chart of accounts
            var (importedAccounts, errors) = await _accountImportService.ImportFromCsvAsync(csvContent, "AccountingServiceTest");
            Assert.That(errors, Is.Empty, "Chart of accounts import should not have errors");

            // Add accounts to object database
            _objectDb.Accounts = importedAccounts.ToList();

            // Create fiscal period for January 2020
            var fiscalPeriod = new FiscalPeriodDto
            {
                Oid = Guid.NewGuid(),
                Name = "January 2020",
                StartDate = new DateOnly(2020, 1, 1),
                EndDate = new DateOnly(2020, 1, 31),
                Status = FiscalPeriodStatus.Open
            };

            _objectDb.fiscalPeriods.Add(fiscalPeriod);
            // Create a test transaction
            var cashAccount = importedAccounts.First(a => a.OfficialCode == "11010101"); // CAJA GENERAL
            var expenseAccount = importedAccounts.First(a => a.OfficialCode == "430119"); // PAPELERIA Y UTILES DE OFICINA

            var transaction = new TransactionDto
            {
                Oid = Guid.NewGuid(),
                TransactionDate = new DateOnly(2020, 1, 15),
                Description = "Office supplies purchase",
                IsPosted = false,
                LedgerEntries = new List<LedgerEntryDto>
                {
                    new LedgerEntryDto
                    {
                        Oid = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        AccountId = expenseAccount.Oid,
                        Amount = 100.00m,
                         EntryType = EntryType.Debit
                    },
                    new LedgerEntryDto
                    {
                        Oid = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        AccountId = cashAccount.Oid,
                   
                         EntryType = EntryType.Credit,
                        Amount = 100.00m
                    }
                }
            };
            _accountingService.RegisterSequence(null);
            // Act
            bool posted = await _accountingService.PostTransactionAsync(transaction);

            // Assert
            Assert.That(posted, Is.True, "Transaction should be posted successfully");
            
            // Verify transaction is in the activity stream
            var activityRecord = _objectDb.ActivityRecords.FirstOrDefault(a => 
                a.Id == transaction.Oid);

            Assert.That(activityRecord, Is.Not.Null, "Activity record should be created for posted transaction");
        }
    }

  
}