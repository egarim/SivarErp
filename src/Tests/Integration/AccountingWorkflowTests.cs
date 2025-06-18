using NUnit.Framework;
using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Modules;
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

namespace Tests.Integration
{
    [TestFixture]
    public class AccountingWorkflowTests
    {
        private string _testDataPath;
        private DataImportHelper _dataImportHelper;
        private IObjectDb _objectDb;
        private AccountingModule _accountingModule;
        private IFiscalPeriodService _fiscalPeriodService;
        private IAccountBalanceCalculator _accountBalanceCalculator;
        private ISequencerService _sequencerService;
        private TransactionGeneratorService _transactionGenerator;

        [SetUp]
        public void Setup()
        {
            // Create object database
            _objectDb = new ObjectDb();
            
            // Setup the test data path
            _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "TestData");
            Directory.CreateDirectory(_testDataPath); // Ensure the directory exists

            // Create required services
            var optionService = new OptionService();
            var dateTimeZoneService = new DateTimeZoneService();
            var activityStreamService = new ActivityStreamService(dateTimeZoneService, _objectDb);
            _fiscalPeriodService = new FiscalPeriodService(_objectDb);
            _accountBalanceCalculator = new AccountBalanceCalculatorServiceBase();
            _sequencerService = new SequencerService(_objectDb);

            // Initialize the accounting module
            _accountingModule = new AccountingModule(
                optionService,
                activityStreamService,
                dateTimeZoneService,
                _fiscalPeriodService,
                _accountBalanceCalculator,
                _sequencerService
            );

            // Setup import services with El Salvador configurations
            var elSalvadorAccountValidator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
            var accountImportService = new AccountImportExportService(elSalvadorAccountValidator);
            var taxImportService = new TaxImportExportService();
            var taxGroupImportService = new TaxGroupImportExportService();
            var businessEntityImportService = new BusinessEntityImportExportService();
            var itemImportService = new ItemImportExportService();
            var documentTypeImportService = new DocumentTypeImportExportService();
            var groupMembershipImportService = new GroupMembershipImportExportService();

            // Initialize the data import helper
            _dataImportHelper = new DataImportHelper(
                accountImportService,
                taxImportService,
                taxGroupImportService,
                documentTypeImportService,
                businessEntityImportService,
                itemImportService,
                groupMembershipImportService,
                "AccountingWorkflowTest"
            );

            // Initialize transaction generator with empty mappings
            // We'll add specific mappings later
            _transactionGenerator = new TransactionGeneratorService();
        }

        [Test]
        public async Task Complete_CCF_DocumentToTransaction_Workflow()
        {
            // STEP 1: Load test data using DataImportHelper
            // First create sample data files if they don't exist
            await CreateSampleDataFiles();

            // Import all data using the DataImportHelper
            var importResults = await _dataImportHelper.ImportAllDataAsync(_objectDb, _testDataPath);
            
            // Verify import was successful
            Assert.That(importResults, Is.Not.Null);
            foreach (var result in importResults)
            {
                foreach (var message in result.Value)
                {
                    Assert.That(message, Does.Not.Contain("Error"), $"Error in {result.Key}: {message}");
                }
            }

            // STEP 2: Create a fiscal period for testing
            var fiscalPeriod = new FiscalPeriodDto
            {
                Code = "FP2023", 
                Name = "Fiscal Period 2023",
                Description = "Testing fiscal period",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2023, 12, 31),
                Status = FiscalPeriodStatus.Open,
                InsertedBy = "Test",
                InsertedAt = DateTime.UtcNow,
                UpdatedBy = "Test",
                UpdatedAt = DateTime.UtcNow
            };
            
            _objectDb.fiscalPeriods.Add(fiscalPeriod);

            // Register the accounting module sequences
            _accountingModule.RegisterSequence(new List<SequenceDto>());

            // STEP 3: Find the required accounts, entities and document types
            // These have already been loaded into _objectDb by the DataImportHelper

            // Find accounts
            var cashAccount = _objectDb.Accounts.FirstOrDefault(a => a.OfficialCode == "11010101"); // CAJA GENERAL
            Assert.That(cashAccount, Is.Not.Null, "Cash account not found");
            
            var salesAccount = _objectDb.Accounts.FirstOrDefault(a => a.OfficialCode == "510101"); // VENTAS NACIONALES
            Assert.That(salesAccount, Is.Not.Null, "Sales account not found");
            
            var ivaTaxAccount = _objectDb.Accounts.FirstOrDefault(a => a.OfficialCode == "21060101"); // IVA POR PAGAR
            Assert.That(ivaTaxAccount, Is.Not.Null, "IVA tax account not found");

            // Find the Comprobante de Crédito Fiscal (CCF) document type
            var ccfDocType = _objectDb.DocumentTypes.FirstOrDefault(dt => dt.Code == "CCF");
            Assert.That(ccfDocType, Is.Not.Null, "CCF document type not found");

            // Find a business entity (client)
            var client = _objectDb.BusinessEntities.FirstOrDefault();
            Assert.That(client, Is.Not.Null, "Business entity not found");

            // Find a product
            var product = _objectDb.Items.FirstOrDefault();
            Assert.That(product, Is.Not.Null, "Product not found");

            // Find the IVA tax
            var ivaTax = _objectDb.Taxes.FirstOrDefault(t => t.Code == "IVA");
            Assert.That(ivaTax, Is.Not.Null, "IVA tax not found");

            // STEP 4: Create a document (CCF - Comprobante de Crédito Fiscal)
            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                DocumentNumber = "CCF-2023-001",
                Date = new DateOnly(2023, 6, 1),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = client,
                DocumentType = ccfDocType
            };

            // Create a line with a product
            var line = new LineDto
            {
                Item = product,
                Quantity = 2,
                UnitPrice = 100.00m,
                Amount = 200.00m  // 2 * 100.00
            };

            // Add the line to the document
            document.Lines.Add(line);

            // STEP 5: Calculate taxes using DocumentTaxCalculator
            // Setup tax rules to always apply IVA (13%) to all lines
            var taxRules = new List<TaxRuleDto>
            {
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Code,
                    DocumentOperation = ccfDocType.DocumentOperation,
                    IsEnabled = true,
                    Priority = 1
                }
            };

            // Create a tax rule evaluator
            var taxRuleEvaluator = new TaxRuleEvaluator(
                taxRules,
                _objectDb.Taxes.Where(t => t.Code == ivaTax.Code).Cast<TaxDto>().ToList(),
                _objectDb.GroupMemberships
            );

            // Create a simple tax accounting profile service
            var taxAccountingProfileService = new SimpleTaxAccountingProfileService();

            // Calculate taxes for the document
            var taxCalculator = new DocumentTaxCalculator(
                document,
                document.DocumentType.Code,
                taxRuleEvaluator,
                taxAccountingProfileService
            );

            // Calculate line taxes first
            taxCalculator.CalculateLineTaxes(line);
            
            // Then document taxes
            taxCalculator.CalculateDocumentTaxes();

            // Verify tax calculations
            var lineTaxTotal = line.LineTotals.FirstOrDefault(t => t.Concept.Contains("IVA"));
            Assert.That(lineTaxTotal, Is.Not.Null, "Line tax total not found");
            Assert.That(lineTaxTotal.Total, Is.EqualTo(26.00m), "Line IVA should be 26.00 (13% of 200.00)");

            var docTaxTotal = document.DocumentTotals.FirstOrDefault(t => t.Concept.Contains("IVA"));
            Assert.That(docTaxTotal, Is.Not.Null, "Document tax total not found");

            // STEP 6: Setup account mappings for the transaction generator
            var accountMappings = new Dictionary<string, string>
            {
                { "CASH", cashAccount.OfficialCode },
                { "SALES", salesAccount.OfficialCode },
                { "IVA_PAYABLE", ivaTaxAccount.OfficialCode }
            };

            // Update transaction generator with account mappings
            _transactionGenerator = new TransactionGeneratorService(accountMappings);

            // Add accounting information to document totals
            foreach (var docTotal in document.DocumentTotals.OfType<TotalDto>().ToList())
            {
                if (docTotal.Concept.Contains("Tax:", StringComparison.OrdinalIgnoreCase) || 
                    docTotal.Concept.Contains("IVA", StringComparison.OrdinalIgnoreCase))
                {
                    docTotal.CreditAccountCode = "IVA_PAYABLE";
                    docTotal.IncludeInTransaction = true;
                }
                else if (docTotal.Concept == "Subtotal")
                {
                    docTotal.CreditAccountCode = "SALES";
                    docTotal.IncludeInTransaction = true;
                }
            }

            // Add a grand total that debits cash
            decimal subtotal = line.Amount;
            decimal taxTotal = line.LineTotals.Where(t => t.Concept.Contains("IVA")).Sum(t => t.Total);
            decimal grandTotal = subtotal + taxTotal;  // 200 + 26 = 226

            var grandTotalItem = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Grand Total",
                Total = grandTotal,
                DebitAccountCode = "CASH",
                IncludeInTransaction = true
            };

            document.DocumentTotals.Add(grandTotalItem);

            // STEP 7: Generate a transaction from the document
            var (transaction, ledgerEntries) = await _transactionGenerator.GenerateTransactionAsync(document);
            
            // Assign ledger entries to the transaction
            transaction.LedgerEntries = ledgerEntries;

            // Verify the transaction is balanced
            bool isBalanced = await transaction.ValidateTransactionAsync();
            Assert.That(isBalanced, Is.True, "Transaction should be balanced");

            // STEP 8: Post the transaction using the accounting module
            bool posted = await _accountingModule.PostTransactionAsync(transaction);
            Assert.That(posted, Is.True, "Transaction should be posted successfully");
            Assert.That(transaction.IsPosted, Is.True, "Transaction should be marked as posted");

            // STEP 9: Check account balances
            var balanceCalculator = new AccountBalanceCalculatorServiceBase(new List<ITransaction> { transaction });
            var transactionDate = transaction.TransactionDate;

            // Get account balances
            decimal cashBalance = balanceCalculator.CalculateAccountBalance(cashAccount.OfficialCode, transactionDate);
            decimal salesBalance = balanceCalculator.CalculateAccountBalance(salesAccount.OfficialCode, transactionDate);
            decimal ivaBalance = balanceCalculator.CalculateAccountBalance(ivaTaxAccount.OfficialCode, transactionDate);

            // Verify balances
            Assert.That(cashBalance, Is.EqualTo(grandTotal), "Cash balance incorrect");
            Assert.That(salesBalance, Is.EqualTo(-subtotal), "Sales balance incorrect");
            Assert.That(ivaBalance, Is.EqualTo(-taxTotal), "IVA balance incorrect");

            // Verify accounting equation: Assets = Liabilities + Equity + (Revenue - Expenses)
            Assert.That(cashBalance, Is.EqualTo(-(ivaBalance + salesBalance)), 
                "Accounting equation not balanced");

            // Verify debits equal credits
            decimal totalDebits = ledgerEntries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
            decimal totalCredits = ledgerEntries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
            Assert.That(totalDebits, Is.EqualTo(totalCredits), "Debits should equal credits");
        }

        /// <summary>
        /// Creates sample data files for testing
        /// </summary>
        private async Task CreateSampleDataFiles()
        {
            // 1. Chart of Accounts
            var chartOfAccounts = @"OfficialCode,AccountName,IsControlAccount,AccountType,Active
11010101,CAJA GENERAL,false,Asset,true
11010201,BANCO CUSCATLAN CTA. CTE,false,Asset,true
11020101,CUENTAS POR COBRAR CLIENTES,false,Asset,true
21060101,IVA POR PAGAR,false,Liability,true
31010101,CAPITAL SOCIAL,false,Equity,true
510101,VENTAS NACIONALES,false,Revenue,true
410101,COSTO DE VENTAS DE MERCADERIA,false,Expense,true";
            
            await File.WriteAllTextAsync(Path.Combine(_testDataPath, "ComercialChartOfAccounts.txt"), chartOfAccounts);

            // 2. Tax Groups
            var taxGroups = @"Oid,Code,Name,IsEnabled
c6b88e57-db97-4f98-8b79-ae26551a8979,STANDARD,Standard Tax Group,true
d7c99f68-db97-4f98-8b79-ae26551a8980,EXEMPT,Tax Exempt Group,true";
            
            await File.WriteAllTextAsync(Path.Combine(_testDataPath, "ElSalvadorTaxGroups.txt"), taxGroups);
            
            // 3. Taxes
            var taxes = @"Oid,Code,Name,TaxType,Percentage,Amount,ApplicationLevel,IsEnabled,Country
e8d9af79-db97-4f98-8b79-ae26551a8981,IVA,Impuesto al Valor Agregado,Percentage,13,0.00,Line,true,SV";
            
            await File.WriteAllTextAsync(Path.Combine(_testDataPath, "ElSalvadorTaxes.txt"), taxes);

            // 4. Business Entities
            var businessEntities = @"Code,Name,Address,City,State,ZipCode,Country,PhoneNumber,Email
CL001,Empresa ABC SA de CV,Calle Principal 123,San Salvador,San Salvador,1101,SV,2222-1111,info@empresaabc.com";
            
            await File.WriteAllTextAsync(Path.Combine(_testDataPath, "BusinesEntities.txt"), businessEntities);

            // 5. Items/Products
            var items = @"Code,Type,Description,BasePrice,Unit,IsActive,Notes
PR001,Product,Laptop Dell XPS 13,1200.00,UNIT,true,High performance laptop";
            
            await File.WriteAllTextAsync(Path.Combine(_testDataPath, "Items.txt"), items);
            
            // 6. Document Types
            var documentTypes = @"Oid,Code,Name,IsEnabled,DocumentOperation
f0cab08b-db97-4f98-8b79-ae26551a8983,CCF,Comprobante de Crédito Fiscal,true,SalesInvoice
f1dbc19c-db97-4f98-8b79-ae26551a8984,FCF,Factura Consumidor Final,true,SalesInvoice";
            
            await File.WriteAllTextAsync(Path.Combine(_testDataPath, "DocumentTypes.csv"), documentTypes);
        }
    }

    /// <summary>
    /// Simple tax accounting profile service for testing
    /// </summary>
    public class SimpleTaxAccountingProfileService : ITaxAccountingProfileService
    {
        public TaxAccountingInfo GetTaxAccountingInfo(DocumentOperation documentOperation, string taxCode)
        {
            if (documentOperation == DocumentOperation.SalesInvoice && taxCode == "IVA")
            {
                return new TaxAccountingInfo
                {
                    CreditAccountCode = "IVA_PAYABLE",
                    IncludeInTransaction = true
                };
            }
            
            return null;
        }

        public Dictionary<string, TaxAccountingInfo> GetTaxAccountingMapForCategory(DocumentOperation documentOperation)
        {
            return new Dictionary<string, TaxAccountingInfo>
            {
                ["IVA"] = new TaxAccountingInfo
                {
                    CreditAccountCode = "IVA_PAYABLE",
                    IncludeInTransaction = true
                }
            };
        }

        public void RegisterTaxAccountingProfile(DocumentOperation documentOperation, string taxCode, TaxAccountingInfo accountingInfo)
        {
            // Not needed for test implementation
        }
    }
}