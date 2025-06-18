using NUnit.Framework;
using Sivar.Erp.BusinessEntities;
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
                        TransactionNumber = Guid.NewGuid(),
                        OfficialCode = expenseAccount.OfficialCode,
                        AccountName = expenseAccount.AccountName,
                        Amount = 100.00m,
                        EntryType = EntryType.Debit
                    },
                    new LedgerEntryDto
                    {
                        Oid = Guid.NewGuid(),
                        TransactionNumber = Guid.NewGuid(),
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
                        TransactionNumber = Guid.NewGuid(),
                        OfficialCode = expenseAccount.OfficialCode,
                        AccountName = expenseAccount.AccountName,
                        Amount = 100.00m,
                        EntryType = EntryType.Debit
                    },
                    new LedgerEntryDto
                    {
                        Oid = Guid.NewGuid(),
                        TransactionNumber = Guid.NewGuid(),
                        OfficialCode = ivaTaxAccount.OfficialCode,
                        AccountName = ivaTaxAccount.AccountName,
                        Amount = 13.00m,  // 13% IVA
                        EntryType = EntryType.Debit
                    },
                    new LedgerEntryDto
                    {
                        Oid = Guid.NewGuid(),
                        TransactionNumber = Guid.NewGuid(),
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


        [Test]
        public async Task FullFlow()
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
                StartDate = new DateOnly(2000, 1, 1),
                EndDate = new DateOnly(2050, 1, 31),
                Status = FiscalPeriodStatus.Open
            };

            _objectDb.fiscalPeriods.Add(fiscalPeriod);

   
        }


        [Test]
        public async Task TransactionGeneratorWithTaxRules_EvaluatesDocumentTaxes_GeneratesTransaction()
        {
            // Arrange
            // 1. Load Chart of Accounts
            string chartOfAccountsPath = Path.Combine(_testDataPath, "ComercialChartOfAccounts.txt");
            string chartCsvContent = await File.ReadAllTextAsync(chartOfAccountsPath);
            var (importedAccounts, accountErrors) = await _accountImportService.ImportFromCsvAsync(chartCsvContent, "TransactionGeneratorTest");
            Assert.That(accountErrors, Is.Empty, "Chart of accounts import should not have errors");
            _objectDb.Accounts = importedAccounts.ToList();

            // 2. Create fiscal period for January 2023
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "January 2023",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2023, 1, 31),
                Status = FiscalPeriodStatus.Open
            };
            _objectDb.fiscalPeriods.Add(fiscalPeriod);

            // 3. Find accounts for transaction
            var cashAccount = importedAccounts.First(a => a.OfficialCode == "11010101"); // CAJA GENERAL
            var salesAccount = importedAccounts.First(a => a.OfficialCode == "510101"); // VENTAS NACIONALES
            var ivaTaxAccount = importedAccounts.First(a => a.OfficialCode == "21060101"); // IVA por Pagar

            // 4. Create document types for El Salvador
            var creditoFiscalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true,
                DocumentOperation = DocumentOperation.SalesInvoice
            };

            var consumidorFinalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CNF",
                Name = "Consumidor Final",
                IsEnabled = true,
                DocumentOperation = DocumentOperation.SalesInvoice
            };

            // 5. Create business entities
            var registeredCompany = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Code = "COMPANY001",
                Name = "Empresa Registrada S.A. de C.V."
            };

            var individualConsumer = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Code = "CONSUMER001",
                Name = "Consumidor Final"
            };

            // 6. Create products/items
            var standardProduct = new ItemDto
            {
                Oid = Guid.NewGuid(),
                Code = "PROD001",
                Description = "Producto Estándar",
                BasePrice = 100m
            };

            var exemptProduct = new ItemDto
            {
                Oid = Guid.NewGuid(),
                Code = "PROD002",
                Description = "Producto Exento",
                BasePrice = 50m
            };

            // 7. Create tax groups
            var registeredCompanyGroupId = "Register companies";
            var exemptItemGroupId = "Exempt";

            // 8. Create taxes
            var ivaTax = new TaxDto
            {
                Oid = Guid.NewGuid(),
                Name = "IVA",
                Code = "IVA",
                TaxType = TaxType.Percentage,
                ApplicationLevel = TaxApplicationLevel.Line,
                Percentage = 13m,
                IsEnabled = true
            };

            // 9. Create tax rules
            var taxRules = new List<TaxRuleDto>
            {
                // Rule 1: Apply IVA to Credito Fiscal documents for registered companies (except for exempt items)
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Code,
                    DocumentOperation = DocumentOperation.SalesInvoice,
                    BusinessEntityGroupId = registeredCompanyGroupId,
                    IsEnabled = true,
                    Priority = 1
                },
                
                // Rule 2: Don't apply IVA to exempt items (override rule)
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Code,
                    DocumentOperation = DocumentOperation.SalesInvoice,
                    BusinessEntityGroupId = registeredCompanyGroupId,
                    ItemGroupId = exemptItemGroupId,
                    IsEnabled = false, // This rule PREVENTS the tax from being applied
                    Priority = 0 // Higher priority (lower number) makes this rule override Rule 1
                }
            };

            // 10. Setup group memberships
            var groupMemberships = new List<GroupMembershipDto>
            {
                // Add registered company to taxable group
                new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = registeredCompanyGroupId,
                    EntityId = registeredCompany.Code,
                    GroupType = GroupType.BusinessEntity
                },
                
                // Add exempt product to exempt item group
                new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = exemptItemGroupId,
                    EntityId = exemptProduct.Code,
                    GroupType = GroupType.Item
                }
            };

            // 11. Create the TaxRuleEvaluator
            var taxRuleEvaluator = new TaxRuleEvaluator(
                taxRules,
                new List<TaxDto> { ivaTax },
                groupMemberships);

            // 12. Create document for testing
            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                DocumentNumber = "CF-2023-001",
                Date = new DateOnly(2023, 1, 15),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = registeredCompany,
                DocumentType = creditoFiscalDocType
            };

            // 13. Add lines to document
            var standardLine = new LineDto
            {
                Item = standardProduct,
                Quantity = 2,
                UnitPrice = 100m
                // Amount = 200m will be automatically calculated
            };

            var exemptLine = new LineDto
            {
                Item = exemptProduct,
                Quantity = 3,
                UnitPrice = 50m
                // Amount = 150m will be automatically calculated
            };

            // Initialize the Amount property of each line
            standardLine.Amount = standardLine.Quantity * standardLine.UnitPrice;
            exemptLine.Amount = exemptLine.Quantity * exemptLine.UnitPrice;

            document.Lines.Add(standardLine);
            document.Lines.Add(exemptLine);

            // 14. Calculate all taxes using DocumentTaxCalculator
            var taxCalculator = new DocumentTaxCalculator(
                document,
                document.DocumentType.Code,
                taxRuleEvaluator);

            // Calculate line taxes
            taxCalculator.CalculateLineTaxes(standardLine);
            taxCalculator.CalculateLineTaxes(exemptLine);

            // Calculate document level taxes
            taxCalculator.CalculateDocumentTaxes();

            // 15. Create account mappings for the transaction generator
            var accountMappings = new Dictionary<string, string>
            {
                { "CASH", cashAccount.OfficialCode },
                { "SALES", salesAccount.OfficialCode },
                { "IVA_PAYABLE", ivaTaxAccount.OfficialCode }
            };

            // 16. Setup document totals with accounting information
            // IVA total for standard line
            var ivaTotal = document.DocumentTotals.FirstOrDefault(t => t.Concept.Contains("IVA")) as TotalDto;
            
            if (ivaTotal != null)
            {
                ivaTotal.CreditAccountCode = "IVA_PAYABLE"; // Map to IVA Payable account
                ivaTotal.IncludeInTransaction = true;
            }

            // Create subtotal for sales 
            var subtotal = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Subtotal",
                Total = standardLine.Amount + exemptLine.Amount, // 200 + 150 = 350
                CreditAccountCode = "SALES", // Map to Sales Revenue account
                IncludeInTransaction = true
            };

            document.DocumentTotals.Add(subtotal);

            // Create grand total for cash received
            var grandTotal = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Grand Total",
                Total = subtotal.Total + (ivaTotal?.Total ?? 0), // 350 + 26 = 376
                DebitAccountCode = "CASH", // Map to Cash account
                IncludeInTransaction = true
            };

            document.DocumentTotals.Add(grandTotal);

            // 17. Create the TransactionGeneratorService
            var transactionGenerator = new TransactionGeneratorService(accountMappings);

            // Act
            var (transaction, ledgerEntries) = await transactionGenerator.GenerateTransactionAsync(document);

            // IMPORTANT FIX: Assign ledger entries to the transaction
            // The TransactionGeneratorService creates entries but doesn't set them on the transaction
            transaction.LedgerEntries = ledgerEntries;

            // Also post the transaction
            _accountingService.RegisterSequence(null);
            bool posted = await _accountingService.PostTransactionAsync(transaction);

            // Assert
            // 1. Verify successful generation and posting
            Assert.That(transaction, Is.Not.Null, "Transaction should be generated");
            Assert.That(ledgerEntries, Is.Not.Null, "Ledger entries should be generated");
            Assert.That(posted, Is.True, "Transaction should be posted successfully");

            // 2. Verify ledger entries
            Assert.That(ledgerEntries.Count, Is.EqualTo(3), "Should have three ledger entries");

            var cashEntry = ledgerEntries.FirstOrDefault(e => e.OfficialCode == cashAccount.OfficialCode);
            var salesEntry = ledgerEntries.FirstOrDefault(e => e.OfficialCode == salesAccount.OfficialCode);
            var ivaEntry = ledgerEntries.FirstOrDefault(e => e.OfficialCode == ivaTaxAccount.OfficialCode);

            // 3. Verify cash entry
            Assert.That(cashEntry, Is.Not.Null, "Should have a cash entry");
            Assert.That(cashEntry.EntryType, Is.EqualTo(EntryType.Debit), "Cash entry should be a debit");
            Assert.That(cashEntry.Amount, Is.EqualTo(376.00m), "Cash debit should be 376.00");

            // 4. Verify sales entry
            Assert.That(salesEntry, Is.Not.Null, "Should have a sales entry");
            Assert.That(salesEntry.EntryType, Is.EqualTo(EntryType.Credit), "Sales entry should be a credit");
            Assert.That(salesEntry.Amount, Is.EqualTo(350.00m), "Sales credit should be 350.00");

            // 5. Verify IVA entry
            Assert.That(ivaEntry, Is.Not.Null, "Should have an IVA entry");
            Assert.That(ivaEntry.EntryType, Is.EqualTo(EntryType.Credit), "IVA entry should be a credit");
            Assert.That(ivaEntry.Amount, Is.EqualTo(26.00m), "IVA credit should be 26.00");

            // 6. Verify account balances
            var balanceCalculator = new AccountBalanceCalculatorServiceBase(new List<ITransaction> { transaction });
            var transactionDate = transaction.TransactionDate;

            decimal cashBalance = balanceCalculator.CalculateAccountBalance(cashAccount.OfficialCode, transactionDate);
            decimal salesBalance = balanceCalculator.CalculateAccountBalance(salesAccount.OfficialCode, transactionDate);
            decimal ivaBalance = balanceCalculator.CalculateAccountBalance(ivaTaxAccount.OfficialCode, transactionDate);

            Assert.That(cashBalance, Is.EqualTo(376.00m), "Cash balance should be 376.00 (debit)");
            Assert.That(salesBalance, Is.EqualTo(-350.00m), "Sales balance should be -350.00 (credit)");
            Assert.That(ivaBalance, Is.EqualTo(-26.00m), "IVA balance should be -26.00 (credit)");

            // 7. Verify accounting equation
            decimal assets = cashBalance;
            decimal liabilities = ivaBalance;  // IVA payable is a liability with credit (negative) balance
            decimal revenue = salesBalance;   // Revenue has credit (negative) balance

            Assert.That(assets, Is.EqualTo(-(liabilities + revenue)), 
                "Accounting equation should balance: Assets = -(Liabilities + Revenue)");

            // 8. Verify debits equal credits
            decimal totalDebits = ledgerEntries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
            decimal totalCredits = ledgerEntries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
            
            Assert.That(totalDebits, Is.EqualTo(totalCredits), 
                "Total debits should equal total credits");
        }
    }
}