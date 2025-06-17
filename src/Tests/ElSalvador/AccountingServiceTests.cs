using NUnit.Framework;
using Sivar.Erp.Documents;
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
using Sivar.Erp.Services.Taxes;
using Sivar.Erp.Services.Taxes.TaxAccountingProfiles;
using Sivar.Erp.Services.Taxes.TaxGroup;
using Sivar.Erp.Services.Taxes.TaxRule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.ElSalvador
{
    [TestFixture]
    public class AccountingServiceTests
    {
        private readonly string _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "Tests", "ElSalvador", "Data", "New");
        private IAccountImportExportService _accountImportService;
        private ITaxImportExportService _taxImportService;
        private ITaxGroupImportExportService _taxGroupImportService;
        private IBusinessEntityImportExportService _businessEntityImportService;
        private IItemImportExportService _itemImportService;
        private IDocumentTypeImportExportService _documentTypeImportService;
        private AccountingModule _accountingService;
        private ObjectDb _objectDb;

        [SetUp]
        public void Setup()
        {
            // Create an El Salvador-specific account validator
            var elSalvadorAccountValidator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
            _accountImportService = new AccountImportExportService(elSalvadorAccountValidator);
            _taxImportService = new TaxImportExportService();
            _taxGroupImportService = new TaxGroupImportExportService();
            _businessEntityImportService = new BusinessEntityImportExportService();
            _itemImportService = new ItemImportExportService();
            _documentTypeImportService = new DocumentTypeImportExportService();

            // Create object database
            _objectDb = new ObjectDb();

            // Create required services
            var optionService = new OptionService();
            var dateTimeZoneService = new DateTimeZoneService();
            var activityStreamService = new ActivityStreamService(dateTimeZoneService, _objectDb);
            var fiscalPeriodService = new FiscalPeriodService(_objectDb);
            var accountBalanceCalculator = new AccountBalanceCalculatorServiceBase();
            var sequencerService = new SequencerService(_objectDb);

            _accountingService = new AccountingModule(
                optionService,
                activityStreamService,
                dateTimeZoneService,
                fiscalPeriodService,
                accountBalanceCalculator,
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
                        OfficialCode = expenseAccount.OfficialCode,
                        AccountName = expenseAccount.AccountName,
                        Amount = 100.00m,
                        EntryType = EntryType.Debit
                    },
                    new LedgerEntryDto
                    {
                        Oid = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        OfficialCode = cashAccount.OfficialCode,
                        AccountName = cashAccount.AccountName,
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

            // Verify account balances using AccountBalanceCalculatorServiceBase
            var balanceCalculator = new AccountBalanceCalculatorServiceBase(new List<ITransaction> { transaction });
            var transactionDate = transaction.TransactionDate;

            decimal cashBalance = balanceCalculator.CalculateAccountBalance(cashAccount.OfficialCode, transactionDate);
            decimal expenseBalance = balanceCalculator.CalculateAccountBalance(expenseAccount.OfficialCode, transactionDate);

            Assert.That(cashBalance, Is.EqualTo(-100.00m), "Cash balance should be -100.00 (credit)");
            Assert.That(expenseBalance, Is.EqualTo(100.00m), "Expense balance should be 100.00 (debit)");

            // Verify turnovers
            var (cashDebitTurnover, cashCreditTurnover) = balanceCalculator.CalculateAccountTurnover(
                cashAccount.OfficialCode, transactionDate, transactionDate);
            var (expenseDebitTurnover, expenseCreditTurnover) = balanceCalculator.CalculateAccountTurnover(
                expenseAccount.OfficialCode, transactionDate, transactionDate);

            Assert.That(cashDebitTurnover, Is.EqualTo(0.00m), "Cash debit turnover should be 0.00");
            Assert.That(cashCreditTurnover, Is.EqualTo(100.00m), "Cash credit turnover should be 100.00");

            Assert.That(expenseDebitTurnover, Is.EqualTo(100.00m), "Expense debit turnover should be 100.00");
            Assert.That(expenseCreditTurnover, Is.EqualTo(0.00m), "Expense credit turnover should be 0.00");

            // Verify HasTransactions
            Assert.That(balanceCalculator.HasTransactions(cashAccount.OfficialCode), Is.True, "Cash account should have transactions");
            Assert.That(balanceCalculator.HasTransactions(expenseAccount.OfficialCode), Is.True, "Expense account should have transactions");

            // Verify accounting equation
            decimal assets = cashBalance; // Cash is our only asset in this transaction
            decimal liabilities = 0m; // No liabilities involved
            decimal equity = 0m; // No equity accounts involved
            decimal expenses = expenseBalance; // Expense account

            decimal leftSide = assets;
            decimal rightSide = liabilities + equity + expenses;

            Assert.That(leftSide, Is.EqualTo(rightSide), "Accounting equation should balance: Assets = Liabilities + Equity + Expenses");
        }

        [Test]
        public async Task LoadChartOfAccountsAndTaxData_January2020_Success()
        {
            // Arrange - Load Chart of Accounts
            string chartOfAccountsPath = Path.Combine(_testDataPath, "ComercialChartOfAccounts.txt");
            string chartCsvContent = await File.ReadAllTextAsync(chartOfAccountsPath);

            // Import the chart of accounts
            var (importedAccounts, accountErrors) = await _accountImportService.ImportFromCsvAsync(chartCsvContent, "AccountingServiceTest");
            Assert.That(accountErrors, Is.Empty, "Chart of accounts import should not have errors");

            // Add accounts to object database
            _objectDb.Accounts = importedAccounts.ToList();

            // Load Taxes
            string taxesPath = Path.Combine(_testDataPath, "ElSalvadorTaxes.txt");
            string taxesCsvContent = await File.ReadAllTextAsync(taxesPath);
            var (taxes, taxErrors) = await _taxImportService.ImportFromCsvAsync(taxesCsvContent, "AccountingServiceTest");
            Assert.That(taxErrors, Is.Empty, "Tax import should not have errors");

            // Add taxes to object database
            _objectDb.Taxes = taxes.Cast<ITax>().ToList();

            // Load Tax Groups
            string taxGroupsPath = Path.Combine(_testDataPath, "ElSalvadorTaxGroups.txt");
            string taxGroupsCsvContent = await File.ReadAllTextAsync(taxGroupsPath);
            var (taxGroups, taxGroupErrors) = await _taxGroupImportService.ImportFromCsvAsync(taxGroupsCsvContent, "AccountingServiceTest");
            Assert.That(taxGroupErrors, Is.Empty, "Tax group import should not have errors");

            // Add tax groups to object database
            _objectDb.TaxGroups = taxGroups.Cast<ITaxGroup>().ToList();

            // Create fiscal period for January 2020
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "January 2020",
                StartDate = new DateOnly(2020, 1, 1),
                EndDate = new DateOnly(2020, 1, 31),
                Status = FiscalPeriodStatus.Open
            };

            _objectDb.fiscalPeriods.Add(fiscalPeriod);

            // Create a test transaction with IVA tax account
            var cashAccount = importedAccounts.First(a => a.OfficialCode == "11010101"); // CAJA GENERAL
            var expenseAccount = importedAccounts.First(a => a.OfficialCode == "430119"); // PAPELERIA Y UTILES DE OFICINA
            var ivaTaxAccount = importedAccounts.First(a => a.OfficialCode == "21060101"); // IVA por Pagar

            var transaction = new TransactionDto
            {
                Oid = Guid.NewGuid(),
                TransactionDate = new DateOnly(2020, 1, 15),
                Description = "Office supplies purchase with IVA tax",
                IsPosted = false,
                LedgerEntries = new List<LedgerEntryDto>
                {
                    new LedgerEntryDto
                    {
                        Oid = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        OfficialCode = expenseAccount.OfficialCode,
                        AccountName = expenseAccount.AccountName,
                        Amount = 100.00m,
                        EntryType = EntryType.Debit
                    },
                    new LedgerEntryDto
                    {
                        Oid = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        OfficialCode = ivaTaxAccount.OfficialCode,
                        AccountName = ivaTaxAccount.AccountName,
                        Amount = 13.00m,  // 13% IVA
                        EntryType = EntryType.Debit
                    },
                    new LedgerEntryDto
                    {
                        Oid = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        OfficialCode = cashAccount.OfficialCode,
                        AccountName = cashAccount.AccountName,
                        EntryType = EntryType.Credit,
                        Amount = 113.00m  // Total with IVA
                    }
                }
            };

            _accountingService.RegisterSequence(null);

            // Act
            bool posted = await _accountingService.PostTransactionAsync(transaction);

            // Assert
            Assert.That(posted, Is.True, "Transaction should be posted successfully");

            //// Verify transaction is in the activity stream
            //var activityRecord = _objectDb.ActivityRecords.FirstOrDefault(a =>
            //    a.Id == transaction.Oid);

            // Verify account balances using AccountBalanceCalculatorServiceBase
            var balanceCalculator = new AccountBalanceCalculatorServiceBase(new List<ITransaction> { transaction });
            var transactionDate = transaction.TransactionDate;

            decimal cashBalance = balanceCalculator.CalculateAccountBalance(cashAccount.OfficialCode, transactionDate);
            decimal expenseBalance = balanceCalculator.CalculateAccountBalance(expenseAccount.OfficialCode, transactionDate);
            decimal ivaTaxBalance = balanceCalculator.CalculateAccountBalance(ivaTaxAccount.OfficialCode, transactionDate);

            Assert.That(cashBalance, Is.EqualTo(-113.00m), "Cash balance should be -113.00 (credit)");
            Assert.That(expenseBalance, Is.EqualTo(100.00m), "Expense balance should be 100.00 (debit)");
            Assert.That(ivaTaxBalance, Is.EqualTo(13.00m), "IVA tax balance should be 13.00 (debit)");

            // Verify turnovers
            var (cashDebitTurnover, cashCreditTurnover) = balanceCalculator.CalculateAccountTurnover(
                cashAccount.OfficialCode, transactionDate, transactionDate);
            var (expenseDebitTurnover, expenseCreditTurnover) = balanceCalculator.CalculateAccountTurnover(
                expenseAccount.OfficialCode, transactionDate, transactionDate);
            var (ivaTaxDebitTurnover, ivaTaxCreditTurnover) = balanceCalculator.CalculateAccountTurnover(
                ivaTaxAccount.OfficialCode, transactionDate, transactionDate);

            Assert.That(cashDebitTurnover, Is.EqualTo(0.00m), "Cash debit turnover should be 0.00");
            Assert.That(cashCreditTurnover, Is.EqualTo(113.00m), "Cash credit turnover should be 113.00");

            Assert.That(expenseDebitTurnover, Is.EqualTo(100.00m), "Expense debit turnover should be 100.00");
            Assert.That(expenseCreditTurnover, Is.EqualTo(0.00m), "Expense credit turnover should be 0.00");

            Assert.That(ivaTaxDebitTurnover, Is.EqualTo(13.00m), "IVA tax debit turnover should be 13.00");
            Assert.That(ivaTaxCreditTurnover, Is.EqualTo(0.00m), "IVA tax credit turnover should be 0.00");

            // Verify HasTransactions
            Assert.That(balanceCalculator.HasTransactions(cashAccount.OfficialCode), Is.True, "Cash account should have transactions");
            Assert.That(balanceCalculator.HasTransactions(expenseAccount.OfficialCode), Is.True, "Expense account should have transactions");
            Assert.That(balanceCalculator.HasTransactions(ivaTaxAccount.OfficialCode), Is.True, "IVA tax account should have transactions");

            // Verify accounting equation
            decimal assets = cashBalance;        // Cash is negative (credit balance)
            decimal liabilities = ivaTaxBalance; // IVA tax is positive (debit balance, not typical for a liability)
            decimal equity = 0m;                 // No equity accounts involved
            decimal expenses = expenseBalance;   // Expense is positive (debit balance)

            Assert.That(assets, Is.EqualTo(-(liabilities + expenses)),
                "Accounting equation should balance: Assets = -(Liabilities + Expenses)");
        }
    }
}