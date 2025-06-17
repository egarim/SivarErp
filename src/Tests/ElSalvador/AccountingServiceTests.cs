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
                        AccountName= expenseAccount.AccountName,
                        Amount = 100.00m,
                        EntryType = EntryType.Debit
                    },
                    new LedgerEntryDto
                    {
                        Oid = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                      
                         OfficialCode = cashAccount.OfficialCode,
                        AccountName= cashAccount.AccountName,
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
            
            // Verify transaction is in the activity stream
            var activityRecord = _objectDb.ActivityRecords.FirstOrDefault(a => 
                a.Id == transaction.Oid);

            Assert.That(activityRecord, Is.Not.Null, "Activity record should be created for posted transaction");
        }

        [Test]
        public async Task LoadMasterData_Success()
        {
            // Arrange - Load Business Entities
            string businessEntitiesPath = Path.Combine(_testDataPath, "BusinesEntities.txt");
            string businessEntitiesCsvContent = await File.ReadAllTextAsync(businessEntitiesPath);

            var (businessEntities, businessEntityErrors) = await _businessEntityImportService.ImportFromCsvAsync(businessEntitiesCsvContent, "AccountingServiceTest");
            Assert.That(businessEntityErrors, Is.Empty, "Business entity import should not have errors");

            // Add business entities to object database
            _objectDb.BusinessEntities = businessEntities.ToList();

            // Load Items
            string itemsPath = Path.Combine(_testDataPath, "Items.txt");
            string itemsCsvContent = await File.ReadAllTextAsync(itemsPath);

            var (items, itemErrors) = await _itemImportService.ImportFromCsvAsync(itemsCsvContent, "AccountingServiceTest");
            Assert.That(itemErrors, Is.Empty, "Item import should not have errors");

            // Add items to object database
            _objectDb.Items = items.ToList();

            // Load Document Types
            string documentTypesPath = Path.Combine(_testDataPath, "DocumentTypes.txt");
            string documentTypesCsvContent = await File.ReadAllTextAsync(documentTypesPath);

            var (documentTypes, documentTypeErrors) = await _documentTypeImportService.ImportFromCsvAsync(documentTypesCsvContent, "AccountingServiceTest");
            Assert.That(documentTypeErrors, Is.Empty, "Document type import should not have errors");

            // Add document types to object database
            _objectDb.DocumentTypes = documentTypes.ToList();

            // Verify the imported data
            Assert.That(_objectDb.BusinessEntities, Is.Not.Empty, "Business entities should be imported");
            Assert.That(_objectDb.Items, Is.Not.Empty, "Items should be imported");
            Assert.That(_objectDb.DocumentTypes, Is.Not.Empty, "Document types should be imported");

            // Verify some specific data
            var nationalClient = _objectDb.BusinessEntities.FirstOrDefault(be => be.Code == "CL001");
            Assert.That(nationalClient, Is.Not.Null, "National client CL001 should exist");
            Assert.That(nationalClient.Name, Is.EqualTo("CLIENTE NACIONAL 1"));

            var product1 = _objectDb.Items.FirstOrDefault(i => i.Code == "PR001");
            Assert.That(product1, Is.Not.Null, "Product PR001 should exist");
            Assert.That(product1.Description, Is.EqualTo("PRODUCTO 1"));

            var ccfDocType = _objectDb.DocumentTypes.FirstOrDefault(dt => dt.Code == "CCF");
            Assert.That(ccfDocType, Is.Not.Null, "CCF document type should exist");
            Assert.That(ccfDocType.DocumentOperation, Is.EqualTo(DocumentOperation.SalesInvoice));
        }
        
        [Test]
        public async Task GenerateTransactionFromCCFDocument_Success()
        {
            // Arrange - Load all required data
            // 1. Load chart of accounts
            string chartOfAccountsPath = Path.Combine(_testDataPath, "ComercialChartOfAccounts.txt");
            string chartCsvContent = await File.ReadAllTextAsync(chartOfAccountsPath);
            var (importedAccounts, accountErrors) = await _accountImportService.ImportFromCsvAsync(chartCsvContent, "AccountingServiceTest");
            Assert.That(accountErrors, Is.Empty, "Chart of accounts import should not have errors");
            _objectDb.Accounts = importedAccounts.ToList();

            // 2. Load business entities
            string businessEntitiesPath = Path.Combine(_testDataPath, "BusinesEntities.txt");
            string businessEntitiesCsvContent = await File.ReadAllTextAsync(businessEntitiesPath);
            var (businessEntities, businessEntityErrors) = await _businessEntityImportService.ImportFromCsvAsync(businessEntitiesCsvContent, "AccountingServiceTest");
            Assert.That(businessEntityErrors, Is.Empty, "Business entity import should not have errors");
            _objectDb.BusinessEntities = businessEntities.ToList();

            // 3. Load document types
            string documentTypesPath = Path.Combine(_testDataPath, "DocumentTypes.txt");
            string documentTypesCsvContent = await File.ReadAllTextAsync(documentTypesPath);
            var (documentTypes, documentTypeErrors) = await _documentTypeImportService.ImportFromCsvAsync(documentTypesCsvContent, "AccountingServiceTest");
            Assert.That(documentTypeErrors, Is.Empty, "Document type import should not have errors");
            _objectDb.DocumentTypes = documentTypes.ToList();

            // Load Items
            string itemsPath = Path.Combine(_testDataPath, "Items.txt");
            string itemsCsvContent = await File.ReadAllTextAsync(itemsPath);

            var (items, itemErrors) = await _itemImportService.ImportFromCsvAsync(itemsCsvContent, "AccountingServiceTest");
            Assert.That(itemErrors, Is.Empty, "Item import should not have errors");

            // Add items to object database
            _objectDb.Items = items.ToList();

            // 4. Create fiscal period
            var fiscalPeriod = new FiscalPeriodDto
            {
               
                Name = "January 2023",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2023, 1, 31),
                Status = FiscalPeriodStatus.Open
            };
            _objectDb.fiscalPeriods.Add(fiscalPeriod);

            // 5. Set up account mappings for the transaction generator
            var accountMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            var cashAccount = importedAccounts.First(a => a.OfficialCode == "11010101"); // CAJA GENERAL
            var salesAccount = importedAccounts.First(a => a.OfficialCode == "51010101"); // VENTAS GRAVADAS
            var ivaTaxAccount = importedAccounts.First(a => a.OfficialCode == "21060101"); // IVA por Pagar
            
            accountMappings.Add("CASH", cashAccount.OfficialCode);
            accountMappings.Add("SALES", salesAccount.OfficialCode);
            accountMappings.Add("IVA_TAX", ivaTaxAccount.OfficialCode);

            // 6. Get specific entities needed for the test
            var client = _objectDb.BusinessEntities.First(be => be.Code == "CL001");
            var ccfDocType = _objectDb.DocumentTypes.First(dt => dt.Code == "CCF");
            var product = _objectDb.Items.First(i => i.Code == "PR001");
            
            // 7. Create tax needed for calculation
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
            
            // 8. Create tax rule to apply IVA to CCF documents
            var taxRules = new List<TaxRuleDto>
            {
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Oid,
                    DocumentOperation = DocumentOperation.SalesInvoice,
                    IsEnabled = true,
                    Priority = 1
                }
            };
            
            // 9. Create TaxRuleEvaluator
            var taxRuleEvaluator = new TaxRuleEvaluator(
                taxRules,
                new List<TaxDto> { ivaTax },
                new List<GroupMembershipDto>());
                
            // 10. Create TaxAccountingProfileService and configure it
            var taxAccountingService = new TaxAccountingProfileService();
            
            // Register accounting profiles for IVA tax
            taxAccountingService.RegisterTaxAccountingProfile(
                DocumentOperation.SalesInvoice,
                "IVA",
                new TaxAccountingInfo
                {
                    CreditAccountCode = "IVA_TAX",
                    IncludeInTransaction = true
                });
                
            // Register profile for product sales
            taxAccountingService.RegisterTaxAccountingProfile(
                DocumentOperation.SalesInvoice,
                "PRODUCT_SALES",
                new TaxAccountingInfo
                {
                    CreditAccountCode = "SALES",
                    IncludeInTransaction = true
                });
                
            // Register profile for payment
            taxAccountingService.RegisterTaxAccountingProfile(
                DocumentOperation.SalesInvoice,
                "PAYMENT",
                new TaxAccountingInfo
                {
                    DebitAccountCode = "CASH",
                    IncludeInTransaction = true
                });
            
            // 11. Create a document with lines
            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                DocumentNumber = "CCF-001",
                Date = new DateOnly(2023, 1, 15),
                BusinessEntity = client,
                DocumentType = ccfDocType
            };

            // 12. Add a line to the document
            var line = new LineDto
            {
                Item = product,
                Quantity = 2,
                UnitPrice = 50.00m,
                // Calculate amount
                Amount = 100.00m
            };
            
            document.Lines.Add(line);
            
            // 13. Use DocumentTaxCalculator to calculate taxes and create document totals
            var taxCalculator = new DocumentTaxCalculator(
                document,
                ccfDocType.Code,
                taxRuleEvaluator,
                taxAccountingService);  // Pass the tax accounting service
            
            // Calculate line taxes
            taxCalculator.CalculateLineTaxes(line);
            
            // Calculate document taxes
            taxCalculator.CalculateDocumentTaxes();
            
            // 14. Add product sales total with accounting info
            var productSalesTotal = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Product Sales",
                Total = line.Amount,
                CreditAccountCode = "SALES",
                IncludeInTransaction = true
            };
            document.DocumentTotals.Add(productSalesTotal);
            
            // 15. Add payment total with accounting info
            var ivaTaxTotal = document.DocumentTotals.FirstOrDefault(t => t.Concept.Contains("IVA")) as TotalDto;
            decimal totalPayment = line.Amount;
            if (ivaTaxTotal != null)
            {
                totalPayment += ivaTaxTotal.Total;
            }
            
            var paymentTotal = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Total Payment",
                Total = totalPayment,
                DebitAccountCode = "CASH",
                IncludeInTransaction = true
            };
            document.DocumentTotals.Add(paymentTotal);
            
            // 16. Create the transaction generator
            var transactionGenerator = new TransactionGeneratorService(accountMappings);
            
            // Act
            // Generate transaction from document with tax information
            var result = await transactionGenerator.GenerateTransactionAsync(document);
            
            // Assert
            // 1. Verify transaction and ledger entries were created
            Assert.That(result.Transaction, Is.Not.Null, "Generated transaction should not be null");
            Assert.That(result.LedgerEntries, Is.Not.Null, "Generated ledger entries should not be null");
            Assert.That(result.LedgerEntries.Count, Is.GreaterThan(0), "Transaction should have ledger entries");
            
            // 2. Verify transaction properties
            Assert.That(result.Transaction.DocumentId, Is.EqualTo(document.Oid), "Transaction should reference the document");
            Assert.That(result.Transaction.TransactionDate, Is.EqualTo(document.Date), "Transaction date should match document date");
            
            // 3. Verify document has correct totals
            var subtotal = line.Amount; // 100.00
            
            Assert.That(ivaTaxTotal, Is.Not.Null, "Document should have IVA tax total");
            Assert.That(ivaTaxTotal.Total, Is.EqualTo(13.00m), "IVA tax should be 13% of 100.00 = 13.00");
            
            // 4. Verify ledger entries were created for each accounting-enabled total
            Assert.That(result.LedgerEntries.Count, Is.GreaterThan(0), 
                "Should generate ledger entries for document totals");
            
            // 5. Verify IVA tax entry was created
            var ivaTaxEntry = result.LedgerEntries.FirstOrDefault(e => 
                e.OfficialCode == ivaTaxAccount.OfficialCode && 
                e.EntryType == EntryType.Credit);
            
            Assert.That(ivaTaxEntry, Is.Not.Null, "Should have a credit entry for IVA tax account");
            Assert.That(ivaTaxEntry.Amount, Is.EqualTo(13.00m), "IVA tax credit amount should be 13.00");
            
            // 6. Verify sales account entry
            var salesEntry = result.LedgerEntries.FirstOrDefault(e => 
                e.OfficialCode == salesAccount.OfficialCode && 
                e.EntryType == EntryType.Credit);
                
            Assert.That(salesEntry, Is.Not.Null, "Should have a credit entry for sales account");
            Assert.That(salesEntry.Amount, Is.EqualTo(100.00m), "Sales credit amount should be 100.00");
            
            // 7. Verify cash entry (represents payment)
            var cashEntry = result.LedgerEntries.FirstOrDefault(e => 
                e.OfficialCode == cashAccount.OfficialCode && 
                e.EntryType == EntryType.Debit);
                
            Assert.That(cashEntry, Is.Not.Null, "Should have a debit entry for cash account");
            Assert.That(cashEntry.Amount, Is.EqualTo(113.00m), "Cash debit amount should be 113.00 (subtotal + tax)");
            
            // 8. Verify accounting equation balances (debits = credits)
            decimal totalDebits = result.LedgerEntries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
            decimal totalCredits = result.LedgerEntries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
            
            Assert.That(totalDebits, Is.EqualTo(totalCredits), "Total debits should equal total credits");
            Assert.That(totalDebits, Is.EqualTo(113.00m), "Total should be 113.00 (100.00 + 13.00)");
        }
    }
}