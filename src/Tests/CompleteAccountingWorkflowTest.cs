using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Services.Accounting.BalanceCalculators;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Services.ImportExport;
using Sivar.Erp.Services.Taxes;
using Sivar.Erp.Services.Taxes.TaxGroup;
using Sivar.Erp.Services.Taxes.TaxRule;
using Sivar.Erp.Services.Taxes.TaxAccountingProfiles;
using Sivar.Erp.Documents;
using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Modules;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.ErpSystem.Sequencers;
using NUnit.Framework;
using System.Diagnostics;

namespace Sivar.Erp.Tests
{
    [TestFixture]
    /// <summary>
    /// Comprehensive test demonstrating the complete accounting workflow
    /// from data import to transaction posting and balance verification
    /// Uses TaxAccountingProfileImportExportService for tax accounting configuration
    /// </summary>
    public class CompleteAccountingWorkflowTest
    {
        private IObjectDb _objectDb;
        private AccountingModule _accountingModule;
        private TransactionGeneratorService _transactionGenerator;
        private DocumentTaxCalculator _taxCalculator;
        private Dictionary<string, string> _accountMappings;
        private TaxRuleEvaluator _taxRuleEvaluator;
        private ITaxAccountingProfileService _taxAccountingService;
        private ITaxAccountingProfileImportExportService _taxAccountingImportService;

        [Test]
        /// <summary>
        /// Main test method that executes the complete workflow using TaxAccountingProfileImportExportService
        /// </summary>
        public async Task ExecuteCompleteWorkflowTest()
        {
            var results = new List<string>();

            try
            {
                // Step 1: Data Import and Setup
                results.Add("=== STEP 1: DATA IMPORT AND SETUP ===");
                await SetupDataAndServices();
                results.Add($"✓ Imported {_objectDb.Accounts.Count} accounts");
                results.Add($"✓ Imported {_objectDb.BusinessEntities.Count} business entities");
                results.Add($"✓ Imported {_objectDb.DocumentTypes.Count} document types");
                results.Add($"✓ Imported {_objectDb.TaxGroups.Count} tax groups");
                results.Add($"✓ Imported {_objectDb.Taxes.Count} taxes");
                results.Add($"✓ Imported {_objectDb.Items.Count} items");
                results.Add($"✓ Created fiscal period and account mappings");
                results.Add("");

                // Step 2: Tax Accounting Profile Setup
                results.Add("=== STEP 2: TAX ACCOUNTING PROFILE SETUP ===");
                await SetupTaxAccountingProfilesFromCsv();
                results.Add("✓ Loaded tax accounting profiles from CSV");
                results.Add($"✓ Configured tax accounting for SalesInvoice operations");
                results.Add("");

                // Step 3: Document Creation
                results.Add("=== STEP 3: DOCUMENT CREATION ===");
                var document = CreateSalesInvoiceDocument();
                results.Add($"✓ Created CCF document #{document.DocumentNumber}");
                results.Add($"✓ Document date: {document.Date}");
                results.Add($"✓ Business entity: {document.BusinessEntity.Name}");
                results.Add($"✓ Document has {document.Lines.Count} lines");
                results.Add("");

                // Step 4: Tax Calculation
                results.Add("=== STEP 4: TAX CALCULATION ===");
                CalculateDocumentTaxes(document);
                results.Add($"✓ Calculated taxes for document");

                if (document.DocumentTotals?.Any() == true)
                {
                    results.Add($"✓ Document has {document.DocumentTotals.Count} totals:");
                    foreach (var total in document.DocumentTotals)
                    {
                        results.Add($"   - {total.Concept}: ${total.Total:F2}");
                    }
                }
                results.Add("");

                // Step 5: Transaction Generation
                results.Add("=== STEP 5: TRANSACTION GENERATION ===");
                var (transaction, ledgerEntries) = await GenerateTransactionFromDocument(document);
                results.Add($"✓ Generated transaction with {ledgerEntries.Count} entries");
                results.Add($"✓ Transaction is balanced: {IsTransactionBalanced(ledgerEntries)}");
                results.Add("");

                // Step 6: Display Transaction Details
                results.Add("=== STEP 6: TRANSACTION DETAILS ===");
                foreach (var entry in ledgerEntries.OrderBy(e => e.EntryType).ThenBy(e => e.OfficialCode))
                {
                    var account = _objectDb.Accounts.FirstOrDefault(a => a.OfficialCode == entry.OfficialCode);
                    string accountName = account?.AccountName ?? "Unknown Account";
                    results.Add($"  {entry.EntryType}: {entry.OfficialCode} - {accountName}: {entry.Amount:C}");
                }
                results.Add("");

                // Step 7: Post Transaction and Verify Balances
                results.Add("=== STEP 7: TRANSACTION POSTING & BALANCE VERIFICATION ===");
                await PostTransactionAndVerifyBalances(transaction, ledgerEntries);
                results.Add("✓ Transaction posted successfully");
                results.Add("✓ Account balances updated");
                results.Add("");

                // Display final results
                results.Add("=== WORKFLOW COMPLETED SUCCESSFULLY ===");
                results.Add("All steps executed without errors!");

                // Output all results
                foreach (var result in results)
                {
                    Console.WriteLine(result);
                }

               
            }
            catch (Exception ex)
            {
                results.Add($"❌ ERROR: {ex.Message}");
                results.Add($"Stack trace: {ex.StackTrace}");

                foreach (var result in results)
                {
                    Console.WriteLine(result);
                }

                Assert.Fail($"Workflow failed: {ex.Message}");
            }
            var log= results.Aggregate((current, next) => current + Environment.NewLine + next);
            Debug.WriteLine(log);
            Assert.Pass("Complete accounting workflow executed successfully!");
        }

        /// <summary>
        /// Sets up all required data and services for the test
        /// </summary>
        private async Task SetupDataAndServices()
        {
            // Initialize ObjectDb
            _objectDb = new ObjectDb();

            // Create all required import services
            var accountValidator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
            var accountImportService = new AccountImportExportService(accountValidator);
            var taxImportService = new TaxImportExportService();
            var taxGroupImportService = new TaxGroupImportExportService();
            var businessEntityImportService = new BusinessEntityImportExportService();
            var itemImportService = new ItemImportExportService();
            var documentTypeImportService = new DocumentTypeImportExportService();
            var groupMembershipImportService = new GroupMembershipImportExportService();
            var taxRuleImportService = new TaxRuleImportExportService(new TaxRuleValidator());

            // Use your existing data directory with CSV files
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ElSalvador");
            dataDirectory = "C:\\Users\\joche\\Documents\\GitHub\\SivarErp\\src\\Tests\\ElSalvador\\Data\\New\\";

            // Verify the data directory exists
            if (!Directory.Exists(dataDirectory))
            {
                throw new DirectoryNotFoundException($"Data directory not found: {dataDirectory}. Please ensure your CSV files are in this location.");
            }

            // Create DataImportHelper and import all data
            var dataImportHelper = new DataImportHelper(
                accountImportService, taxImportService, taxGroupImportService,
                documentTypeImportService, businessEntityImportService, itemImportService,
                groupMembershipImportService, taxRuleImportService, "TestUser");

            var importResults = await dataImportHelper.ImportAllDataAsync(_objectDb, dataDirectory);

            // Check for import errors
            foreach (var fileResult in importResults)
            {
                var errors = fileResult.Value.Where(msg => msg.StartsWith("Error") || msg.StartsWith("❌")).ToList();
                if (errors.Any())
                {
                    throw new InvalidOperationException($"Import errors in {fileResult.Key}: {string.Join(", ", errors)}");
                }
            }

            // Setup services and account mappings
            await SetupServices();
            SetupAccountMappings();
        }

        /// <summary>
        /// Sets up tax accounting profiles using TaxAccountingProfileImportExportService
        /// </summary>
        private async Task SetupTaxAccountingProfilesFromCsv()
        {
            // Create tax accounting profile service and import service
            _taxAccountingService = new TaxAccountingProfileService();
            _taxAccountingImportService = new TaxAccountingProfileImportExportService();

            // Get imported tax rules from ObjectDb (populated by DataImportHelper)
            var taxRules = _objectDb.TaxRules?.ToList() ?? new List<ITaxRule>();

            // Create tax rule evaluator with rules and group memberships
            var groupMemberships = _objectDb.GroupMemberships?.ToList() ?? new List<GroupMembershipDto>();
            _taxRuleEvaluator = new TaxRuleEvaluator(taxRules, _objectDb.Taxes, groupMemberships);

            // Create tax accounting profiles CSV content
            // This replaces the manual configuration with CSV-based configuration
            var taxAccountingProfilesCsv = CreateTaxAccountingProfilesCsv();

            // Import tax accounting profiles from CSV
            var (importedProfiles, errors) = await _taxAccountingImportService.ImportFromCsvAsync(taxAccountingProfilesCsv, "TestUser");

            if (errors.Any())
            {
                throw new InvalidOperationException($"Tax accounting profile import errors: {string.Join(", ", errors)}");
            }

            // Register each imported profile with the tax accounting service
            foreach (var profile in importedProfiles)
            {
                var accountingInfo = new TaxAccountingInfo
                {
                    DebitAccountCode = profile.DebitAccountCode,
                    CreditAccountCode = profile.CreditAccountCode,
                    IncludeInTransaction = profile.IncludeInTransaction
                };

                _taxAccountingService.RegisterTaxAccountingProfile(
                    profile.DocumentOperation,
                    profile.TaxCode,
                    accountingInfo);
            }
        }

        /// <summary>
        /// Creates CSV content for tax accounting profiles
        /// This replaces the manual configuration in the original test
        /// </summary>
        private string CreateTaxAccountingProfilesCsv()
        {
            var csvContent = @"TaxCode,DocumentOperation,DebitAccountCode,CreditAccountCode,AccountDescription,IncludeInTransaction
IVA,SalesInvoice,,VAT_PAYABLE,IVA DEBITO FISCAL - CONTRIBUYENTES,true
IVA,PurchaseInvoice,VAT_RECEIVABLE,,IVA CREDITO FISCAL,true
IVA,SalesCreditNote,VAT_PAYABLE,,IVA DEBITO FISCAL - CONTRIBUYENTES,true
IVA,PurchaseCreditNote,,VAT_RECEIVABLE,IVA CREDITO FISCAL,true
RENTA,SalesInvoice,,WITHHOLDING_PAYABLE,RETENCION DE RENTA POR PAGAR,true
RENTA,PurchaseInvoice,WITHHOLDING_RECEIVABLE,,RETENCION DE RENTA POR COBRAR,true";

            return csvContent;
        }

        /// <summary>
        /// Sets up all required services
        /// </summary>
        private async Task SetupServices()
        {
            // Create required services
            var dateTimeZoneService = new DateTimeZoneService();
            var activityStreamService = new ActivityStreamService(dateTimeZoneService, _objectDb);
            var optionService = new OptionService();
            var sequencerService = new SequencerService(_objectDb);
            var fiscalPeriodService = new FiscalPeriodService(_objectDb);
            var accountBalanceCalculator = new AccountBalanceCalculatorServiceBase(_objectDb);

            // Create accounting module
            _accountingModule = new AccountingModule(
                optionService,
                activityStreamService,
                dateTimeZoneService,
                fiscalPeriodService,
                accountBalanceCalculator,
                sequencerService);

            // Register sequences
            _accountingModule.RegisterSequence(_objectDb.Sequences);

            // Create fiscal period for 2025
            var fiscalPeriod = new FiscalPeriodDto
            {
                
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 12, 31),
                Status = FiscalPeriodStatus.Open,
                Name = "Fiscal Year 2025"
            };

            await fiscalPeriodService.CreateFiscalPeriodAsync(fiscalPeriod, "TestUser");
        }

        /// <summary>
        /// Sets up account mappings for transaction generation using actual chart of accounts
        /// </summary>
        private void SetupAccountMappings()
        {
            _accountMappings = new Dictionary<string, string>
            {
                ["ACCOUNTS_RECEIVABLE"] = "11030101",    // CLIENTES NACIONALES
                ["INVENTORY_PRODUCT_1"] = "1105010201",  // INVENTARIO PRODUCTO 1
                ["INVENTORY_PRODUCT_2"] = "1105010202",  // INVENTARIO PRODUCTO 2
                ["INVENTORY_PRODUCT_3"] = "1105010203",  // INVENTARIO PRODUCTO 3
                ["INVENTORY_PRODUCT_4"] = "1105010204",  // INVENTARIO PRODUCTO 4
                ["INVENTORY_PRODUCT_5"] = "1105010205",  // INVENTARIO PRODUCTO 5
                ["SALES_PRODUCT_1"] = "51010101",        // VENTA DE PRODUCTO 1
                ["SALES_PRODUCT_2"] = "51010102",        // VENTA DE PRODUCTO 2
                ["SALES_PRODUCT_3"] = "51010103",        // VENTA DE PRODUCTO 3
                ["SALES_PRODUCT_4"] = "51010104",        // VENTA DE PRODUCTO 4
                ["SALES_PRODUCT_5"] = "51010105",        // VENTA DE PRODUCTO 5
                ["VAT_PAYABLE"] = "21060101",             // IVA DEBITO FISCAL - CONTRIBUYENTES
                ["VAT_RECEIVABLE"] = "11050201",          // IVA CREDITO FISCAL
                ["WITHHOLDING_PAYABLE"] = "21070101",     // RETENCION DE RENTA POR PAGAR
                ["WITHHOLDING_RECEIVABLE"] = "11050301",  // RETENCION DE RENTA POR COBRAR
                ["COST_OF_SALES_PRODUCT_1"] = "41010111", // COSTO DE VENTA PRODUCTO 1
                ["COST_OF_SALES_PRODUCT_2"] = "41010112", // COSTO DE VENTA PRODUCTO 2
                ["COST_OF_SALES_PRODUCT_3"] = "41010113", // COSTO DE VENTA PRODUCTO 3
                ["COST_OF_SALES_PRODUCT_4"] = "41010114", // COSTO DE VENTA PRODUCTO 4
                ["COST_OF_SALES_PRODUCT_5"] = "41010115", // COSTO DE VENTA PRODUCTO 5
                ["CASH"] = "11010101",                    // CAJA GENERAL
                ["BANK_CUSCATLAN"] = "11010201",          // BANCO CUSCATLAN
                ["BANK_AGRICOLA"] = "11010202"            // BANCO AGRICOLA
            };

            _transactionGenerator = new TransactionGeneratorService(_accountMappings);
        }

        /// <summary>
        /// Creates a sales invoice document for testing
        /// </summary>
        private DocumentDto CreateSalesInvoiceDocument()
        {
            // Get business entity and document type
            var businessEntity = _objectDb.BusinessEntities.FirstOrDefault(be => be.Code == "CL001");
            var documentType = _objectDb.DocumentTypes.FirstOrDefault(dt => dt.Code == "CCF");

            if (businessEntity == null || documentType == null)
            {
                throw new InvalidOperationException("Required business entity or document type not found");
            }

            // Create document with test scenario data
            var document = new DocumentDto
            {
                DocumentType = documentType,
                DocumentNumber = "CCF-2025-001",
                Date = new DateOnly(2025, 6, 18),
                BusinessEntity = businessEntity,
                Lines = new List<IDocumentLine>(),
                DocumentTotals = new List<ITotal>()
            };

            // Add document lines based on SCENARIO_001 from TestDocumentLines.csv
            var item1 = _objectDb.Items.FirstOrDefault(i => i.Code == "PR001");
            var item2 = _objectDb.Items.FirstOrDefault(i => i.Code == "PR002");

            if (item1 != null)
            {
                var line1 = new LineDto
                {
                    LineNumber = 1,
                    Item = item1,
                    Quantity = 2,
                    UnitPrice = 100.00m,
                    Amount = 200.00m
                };
                document.Lines.Add(line1);
            }

            if (item2 != null)
            {
                var line2 = new LineDto
                {
                    LineNumber = 2,
                    Item = item2,
                    Quantity = 1,
                    UnitPrice = 250.00m,
                    Amount = 250.00m
                };
                document.Lines.Add(line2);
            }

            return document;
        }

        /// <summary>
        /// Calculates taxes for the document using the configured tax accounting profiles
        /// </summary>
        private void CalculateDocumentTaxes(DocumentDto document)
        {
            _taxCalculator = new DocumentTaxCalculator(
                document,
                document.DocumentType.Code,
                _taxRuleEvaluator,
                _taxAccountingService);

            // Calculate line taxes first
            foreach (var line in document.Lines.OfType<LineDto>())
            {
                _taxCalculator.CalculateLineTaxes(line);
            }

            // Then calculate document taxes and totals
            _taxCalculator.CalculateDocumentTaxes();

            // Add basic accounting totals
            AddAccountingTotals(document);
        }

        /// <summary>
        /// Adds basic accounting totals to the document
        /// </summary>
        private void AddAccountingTotals(DocumentDto document)
        {
            // Subtotal (line amounts before tax) - credit to sales account
            var subtotal = document.Lines.OfType<LineDto>().Sum(l => l.Amount);
            var subtotalDto = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Subtotal",
                Total = subtotal,
                CreditAccountCode = "SALES_PRODUCT_1", // Will use VENTA DE PRODUCTO 1 for simplicity
                IncludeInTransaction = true
            };

            // Add subtotal at the beginning
            document.DocumentTotals.Insert(0, subtotalDto);

            // Calculate total amount including taxes
            var totalAmount = document.DocumentTotals.Sum(t => t.Total);

            // Add accounts receivable (total amount as debit)
            var accountsReceivableTotal = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Accounts Receivable",
                Total = totalAmount,
                DebitAccountCode = "ACCOUNTS_RECEIVABLE", // CLIENTES NACIONALES
                IncludeInTransaction = true
            };
            document.DocumentTotals.Add(accountsReceivableTotal);

            // Add cost of goods sold and inventory reduction
            var cogs = subtotal * 0.6m; // Assume 60% cost ratio
            var cogsTotal = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Cost of Goods Sold",
                Total = cogs,
                DebitAccountCode = "COST_OF_SALES_PRODUCT_1", // COSTO DE VENTA PRODUCTO 1
                IncludeInTransaction = true
            };
            document.DocumentTotals.Add(cogsTotal);

            var inventoryReductionTotal = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Inventory Reduction",
                Total = cogs,
                CreditAccountCode = "INVENTORY_PRODUCT_1", // INVENTARIO PRODUCTO 1
                IncludeInTransaction = true
            };
            document.DocumentTotals.Add(inventoryReductionTotal);
        }

        /// <summary>
        /// Generates accounting transaction from document
        /// </summary>
        private async Task<(TransactionDto Transaction, List<LedgerEntryDto> LedgerEntries)> GenerateTransactionFromDocument(DocumentDto document)
        {
            return await _transactionGenerator.GenerateTransactionAsync(document);
        }

        /// <summary>
        /// Creates an initial transaction for beginning balances
        /// </summary>
        private TransactionDto CreateBeginningBalanceTransaction()
        {
            var beginningBalanceTransaction = new TransactionDto
            {
                TransactionDate = new DateOnly(2025, 1, 1),
                Description = "Beginning balance - Inventory setup",
                DocumentNumber = "INITIAL-001",
                LedgerEntries = new List<LedgerEntryDto>
                {
                    // Beginning inventory for SCENARIO_001 products
                    new LedgerEntryDto
                    {
                        OfficialCode = "1105010201", // INVENTARIO PRODUCTO 1
                        EntryType = EntryType.Debit,
                        Amount = 500.00m
                    },
                    new LedgerEntryDto
                    {
                        OfficialCode = "31010101", // CAPITAL SOCIAL PAGADO
                        EntryType = EntryType.Credit,
                        Amount = 500.00m
                    }
                }
            };

            return beginningBalanceTransaction;
        }

        /// <summary>
        /// Checks if a transaction is balanced (total debits = total credits)
        /// </summary>
        private bool IsTransactionBalanced(List<LedgerEntryDto> entries)
        {
            var totalDebits = entries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
            var totalCredits = entries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
            return Math.Abs(totalDebits - totalCredits) < 0.01m; // Allow for small rounding differences
        }

        /// <summary>
        /// Posts transaction and verifies account balances
        /// </summary>
        private async Task PostTransactionAndVerifyBalances(TransactionDto transaction, List<LedgerEntryDto> ledgerEntries)
        {
            // Post the beginning balance transaction first
            var beginningBalance = CreateBeginningBalanceTransaction();
            await _accountingModule.PostTransactionAsync(beginningBalance);

            // Set up the main transaction with ledger entries
            transaction.LedgerEntries = ledgerEntries;

            // Post the main transaction
            await _accountingModule.PostTransactionAsync(transaction);

            // Verify some key balances exist (basic validation)
            var receivablesEntries = ledgerEntries.Where(e => e.OfficialCode == "11030101").ToList();
            var vatPayableEntries = ledgerEntries.Where(e => e.OfficialCode == "21060101").ToList();

            Assert.That(receivablesEntries.Any(), Is.True, "Should have accounts receivable entries");
            Assert.That(transaction.IsPosted, Is.True, "Transaction should be posted");
        }
    }
}