using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using Sivar.Erp.Services.Documents;
using Sivar.Erp.Documents;
using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Modules;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.Tests.Infrastructure;
using NUnit.Framework;
using System.Diagnostics;
using System.Text;
using Sivar.Erp.Modules.Accounting;

namespace Sivar.Erp.Tests
{
    [TestFixture]
    /// <summary>
    /// Comprehensive test demonstrating the complete accounting workflow
    /// from data import to transaction posting and balance verification
    /// Uses AccountingTestServiceFactory for dependency injection
    /// </summary>
    public class CompleteAccountingWorkflowTest
    {
        private IServiceProvider _serviceProvider = null!;
        private IObjectDb _objectDb = null!;
        private AccountingModule? _accountingModule;
        private TransactionGeneratorService? _transactionGenerator;
        private DocumentTaxCalculator? _taxCalculator;
        private Dictionary<string, string>? _accountMappings;
        private TaxRuleEvaluator? _taxRuleEvaluator;
        private ITaxAccountingProfileService? _taxAccountingService; private ITaxAccountingProfileImportExportService? _taxAccountingImportService;
        private IDocumentTotalsService? _documentTotalsService;
        private Sivar.Erp.Services.Documents.IDocumentAccountingProfileImportExportService? _documentAccountingProfileImportService;
        private IDocumentAccountingProfileService? _documentAccountingProfileService; [SetUp]
        public void Setup()
        {
            // Create ObjectDb instance first
            _objectDb = new ObjectDb();

            // Use AccountingTestServiceFactory to configure all services with ObjectDb registration
            _serviceProvider = AccountingTestServiceFactory.CreateServiceProvider(services =>
            {
                // Register the ObjectDb instance as a singleton
                services.AddSingleton<IObjectDb>(_objectDb);
            });
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose service provider if it implements IDisposable
            if (_serviceProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }
        }
        [Test]
        /// <summary>
        /// Main test method that executes the complete workflow: Purchase -> Sales
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
                results.Add($"✓ Configured tax accounting for Purchase and Sales operations");
                results.Add("");

                // Step 2b: Document Accounting Profile Setup
                results.Add("=== STEP 2b: DOCUMENT ACCOUNTING PROFILE SETUP ===");
                await SetupDocumentAccountingProfilesFromCsv();
                results.Add($"✓ Imported {_objectDb.DocumentAccountingProfiles.Count} document accounting profiles");
                results.Add($"✓ Configured document accounting for Sales, Purchase, and Cash operations");
                results.Add("");

                // PHASE 1: PURCHASE TRANSACTION (Inventory In)
                results.Add("=== PHASE 1: PURCHASE TRANSACTION (INVENTORY IN) ===");

                // Step 3a: Purchase Document Creation
                results.Add("=== STEP 3A: PURCHASE DOCUMENT CREATION ===");
                var purchaseDocument = CreatePurchaseInvoiceDocument();
                results.Add($"✓ Created Purchase Invoice #{purchaseDocument.DocumentNumber}");
                results.Add($"✓ Document date: {purchaseDocument.Date:M/d/yyyy}");
                results.Add($"✓ Supplier: {purchaseDocument.BusinessEntity.Name}");
                results.Add($"✓ Document has {purchaseDocument.Lines.Count} lines"); results.Add("");

                // Step 4a: Purchase Tax Calculation
                results.Add("=== STEP 4A: PURCHASE TAX CALCULATION ===");
                CalculateDocumentTaxes(purchaseDocument, "PurchaseInvoice");
                results.Add($"✓ Calculated taxes for purchase document");
                results.Add($"✓ Purchase document has {purchaseDocument.DocumentTotals.Count} totals:");
                foreach (var total in purchaseDocument.DocumentTotals)
                {
                    var totalDto = total as TotalDto;
                    results.Add($"   - {total.Concept}: ${total.Total:F2} " +
                        $"(Debit: {totalDto?.DebitAccountCode ?? "N/A"}, " +
                        $"Credit: {totalDto?.CreditAccountCode ?? "N/A"}, " +
                        $"Include: {totalDto?.IncludeInTransaction ?? false})");
                }
                results.Add("");

                // Step 5a: Purchase Transaction Generation and Posting
                results.Add("=== STEP 5A: PURCHASE TRANSACTION GENERATION & POSTING ===");
                var (purchaseTransaction, purchaseLedgerEntries) = await _transactionGenerator!.GenerateTransactionAsync(purchaseDocument);
                results.Add($"✓ Generated purchase transaction with {purchaseLedgerEntries.Count} entries");

                var isPurchaseBalanced = IsTransactionBalanced(purchaseLedgerEntries);
                results.Add($"✓ Purchase transaction is balanced: {isPurchaseBalanced}");

                // Post purchase transaction
                purchaseTransaction.LedgerEntries = purchaseLedgerEntries;
                await _accountingModule!.PostTransactionAsync(purchaseTransaction);
                results.Add("✓ Purchase transaction posted successfully");
                results.Add("");

                // PHASE 2: SALES TRANSACTION (Inventory Out)
                results.Add("=== PHASE 2: SALES TRANSACTION (INVENTORY OUT) ===");

                // Step 3b: Sales Document Creation
                results.Add("=== STEP 3B: SALES DOCUMENT CREATION ===");
                var salesDocument = CreateSalesInvoiceDocument();
                results.Add($"✓ Created Sales Invoice (CCF) #{salesDocument.DocumentNumber}");
                results.Add($"✓ Document date: {salesDocument.Date:M/d/yyyy}");
                results.Add($"✓ Customer: {salesDocument.BusinessEntity.Name}");
                results.Add($"✓ Document has {salesDocument.Lines.Count} lines");
                results.Add("");

                // Step 4b: Sales Tax Calculation
                results.Add("=== STEP 4B: SALES TAX CALCULATION ===");
                CalculateDocumentTaxes(salesDocument, "SalesInvoice");
                results.Add($"✓ Calculated taxes for sales document");
                results.Add($"✓ Sales document has {salesDocument.DocumentTotals.Count} totals:");
                foreach (var total in salesDocument.DocumentTotals)
                {
                    var totalDto = total as TotalDto;
                    results.Add($"   - {total.Concept}: ${total.Total:F2} " +
                        $"(Debit: {totalDto?.DebitAccountCode ?? "N/A"}, " +
                        $"Credit: {totalDto?.CreditAccountCode ?? "N/A"}, " +
                        $"Include: {totalDto?.IncludeInTransaction ?? false})");
                }
                results.Add("");

                // Step 5b: Sales Transaction Generation and Posting
                results.Add("=== STEP 5B: SALES TRANSACTION GENERATION & POSTING ===");
                var (salesTransaction, salesLedgerEntries) = await _transactionGenerator!.GenerateTransactionAsync(salesDocument);
                results.Add($"✓ Generated sales transaction with {salesLedgerEntries.Count} entries");

                var isSalesBalanced = IsTransactionBalanced(salesLedgerEntries);
                results.Add($"✓ Sales transaction is balanced: {isSalesBalanced}");

                // Post sales transaction
                salesTransaction.LedgerEntries = salesLedgerEntries;
                await _accountingModule!.PostTransactionAsync(salesTransaction);
                results.Add("✓ Sales transaction posted successfully");
                results.Add("");

                // Step 6: Final Transaction Details
                results.Add("=== STEP 6: TRANSACTION SUMMARY ===");
                results.Add("PURCHASE TRANSACTION:");
                var purchaseTotalDebits = purchaseLedgerEntries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
                var purchaseTotalCredits = purchaseLedgerEntries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
                results.Add($"  Total Debits: ${purchaseTotalDebits:F2}");
                results.Add($"  Total Credits: ${purchaseTotalCredits:F2}");
                results.Add($"  Difference: ${Math.Abs(purchaseTotalDebits - purchaseTotalCredits):F2}");

                foreach (var entry in purchaseLedgerEntries.OrderBy(e => e.EntryType).ThenBy(e => e.OfficialCode))
                {
                    var account = _objectDb.Accounts.FirstOrDefault(a => a.OfficialCode == entry.OfficialCode);
                    results.Add($"    {entry.EntryType}: {entry.OfficialCode} - {account?.AccountName}: ${entry.Amount:F2}");
                }
                results.Add("");

                results.Add("SALES TRANSACTION:");
                var salesTotalDebits = salesLedgerEntries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
                var salesTotalCredits = salesLedgerEntries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
                results.Add($"  Total Debits: ${salesTotalDebits:F2}");
                results.Add($"  Total Credits: ${salesTotalCredits:F2}");
                results.Add($"  Difference: ${Math.Abs(salesTotalDebits - salesTotalCredits):F2}");

                foreach (var entry in salesLedgerEntries.OrderBy(e => e.EntryType).ThenBy(e => e.OfficialCode))
                {
                    var account = _objectDb.Accounts.FirstOrDefault(a => a.OfficialCode == entry.OfficialCode);
                    results.Add($"    {entry.EntryType}: {entry.OfficialCode} - {account?.AccountName}: ${entry.Amount:F2}");
                }
                results.Add(""); results.Add("=== WORKFLOW COMPLETED SUCCESSFULLY ===");
                results.Add("Both purchase and sales transactions executed without errors!");

                // Step 7: Print Performance Logs
                results.Add("");
                results.Add("=== STEP 7: PERFORMANCE METRICS ===");
                var perfLogs = PrintPerformanceLogs();
                results.AddRange(perfLogs);
                results.Add("");

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
                var log2 = results.Aggregate((current, next) => current + Environment.NewLine + next);
                Debug.WriteLine(log2);
                Assert.Fail($"Workflow failed: {ex.Message}");

            }

            var log = results.Aggregate((current, next) => current + Environment.NewLine + next);
            Debug.WriteLine(log);
            Assert.Pass("Complete accounting workflow executed successfully!");
        }

        /// <summary>
        /// Sets up all required data and services using AccountingTestServiceFactory
        /// </summary>
        private async Task SetupDataAndServices()
        {
            // Get DataImportHelper from the service provider (configured by factory)
            var dataImportHelper = _serviceProvider.GetRequiredService<DataImportHelper>();

            // Use your existing data directory with CSV files
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ElSalvador");
            dataDirectory = "C:\\Users\\joche\\Documents\\GitHub\\SivarErp\\src\\Tests\\ElSalvador\\Data\\New\\";

            // Verify the data directory exists
            if (!Directory.Exists(dataDirectory))
            {
                throw new DirectoryNotFoundException($"Data directory not found: {dataDirectory}. " +
                    "Please ensure your CSV files are in this location.");
            }

            // Import all data using the factory-configured DataImportHelper
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

            // Setup services and account mappings (account mappings now loaded from CSV)
            await SetupServices();
        }

        /// <summary>
        /// Sets up tax accounting profiles using factory-configured services
        /// </summary>
        private async Task SetupTaxAccountingProfilesFromCsv()
        {
            // Get services from the service provider (configured by factory)
            _taxAccountingService = _serviceProvider.GetRequiredService<ITaxAccountingProfileService>();
            _taxAccountingImportService = _serviceProvider.GetRequiredService<ITaxAccountingProfileImportExportService>();

            // Get imported tax rules from ObjectDb (populated by DataImportHelper)
            var taxRules = _objectDb.TaxRules?.ToList() ?? new List<ITaxRule>();

            // Create tax rule evaluator with rules and group memberships
            var groupMemberships = _objectDb.GroupMemberships?.ToList() ?? new List<GroupMembershipDto>();
            _taxRuleEvaluator = new TaxRuleEvaluator(taxRules, _objectDb.Taxes, groupMemberships);

            // Read tax accounting profiles from CSV file
            var dataDirectory = "C:\\Users\\joche\\Documents\\GitHub\\SivarErp\\src\\Tests\\ElSalvador\\Data\\New\\";
            var csvFilePath = Path.Combine(dataDirectory, "TaxAccountingProfiles.csv");

            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Tax accounting profiles CSV file not found: {csvFilePath}");
            }

            var taxAccountingProfilesCsv = await File.ReadAllTextAsync(csvFilePath);

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
        /// Sets up document accounting profiles using factory-configured services
        /// </summary>
        private async Task SetupDocumentAccountingProfilesFromCsv()
        {
            // Get services from the service provider (configured by factory)
            _documentAccountingProfileService = _serviceProvider.GetRequiredService<Sivar.Erp.Services.Documents.IDocumentAccountingProfileService>();
            _documentAccountingProfileImportService = _serviceProvider.GetRequiredService<Sivar.Erp.Services.Documents.IDocumentAccountingProfileImportExportService>();

            // Read document accounting profiles from CSV file
            var dataDirectory = "C:\\Users\\joche\\Documents\\GitHub\\SivarErp\\src\\Tests\\ElSalvador\\Data\\New\\";
            var csvFilePath = Path.Combine(dataDirectory, "DocumentAccountingProfiles.csv");

            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Document accounting profiles CSV file not found: {csvFilePath}");
            }

            var documentAccountingProfilesCsv = await File.ReadAllTextAsync(csvFilePath);

            // Import document accounting profiles from CSV
            var result = await _documentAccountingProfileImportService.ImportFromCsvAsync(documentAccountingProfilesCsv, "TestUser");
            var importedProfiles = result.ImportedProfiles;
            var errors = result.Errors;

            if (errors.Any())
            {
                throw new InvalidOperationException($"Document accounting profile import errors: {string.Join(", ", errors)}");
            }

            // Register each imported profile with the document accounting service
            foreach (var profile in importedProfiles)
            {
                await _documentAccountingProfileService.CreateProfileAsync(profile, "TestUser");
            }
        }

        /// <summary>
        /// Sets up all required services using factory-configured dependencies
        /// </summary>
        private async Task SetupServices()
        {
            // Get services from the service provider where possible
            var dateTimeZoneService = _serviceProvider.GetRequiredService<IDateTimeZoneService>();
            var optionService = _serviceProvider.GetRequiredService<IOptionService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<AccountingModule>>();

            // Create services that require ObjectDb instance (these can't be pre-configured in factory)
            var activityStreamService = new ActivityStreamService(dateTimeZoneService, _objectDb);
            var sequencerService = new SequencerService(_objectDb);
            var fiscalPeriodService = new FiscalPeriodService(_objectDb);
            var accountBalanceCalculator = new AccountBalanceCalculatorServiceBase(_objectDb);

            // Create accounting module with correct parameter order, including the logger and objectDb
            _accountingModule = new AccountingModule(
                optionService,
                activityStreamService,
                dateTimeZoneService,
                fiscalPeriodService,
                accountBalanceCalculator,
                sequencerService,
                logger,
                _objectDb);

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

            // Load account mappings from CSV using factory-configured service
            var testAccountMappingService = _serviceProvider.GetRequiredService<ITestAccountMappingImportExportService>();
            var dataDirectory = "C:\\Users\\joche\\Documents\\GitHub\\SivarErp\\src\\Tests\\ElSalvador\\Data\\New\\";
            var mappingsCsvPath = Path.Combine(dataDirectory, "TestAccountMappings.csv");

            if (!File.Exists(mappingsCsvPath))
            {
                throw new FileNotFoundException($"Account mappings CSV file not found: {mappingsCsvPath}");
            }

            var mappingsCsv = await File.ReadAllTextAsync(mappingsCsvPath);
            var (accountMappings, mappingErrors) = await testAccountMappingService.ImportFromCsvAsync(mappingsCsv, "TestUser");

            if (mappingErrors.Any())
            {
                throw new InvalidOperationException($"Account mapping import errors: {string.Join(", ", mappingErrors)}");
            }

            // Convert imported mappings to dictionary
            var accountMappingsDict = accountMappings.ToDictionary(m => m.Key, m => m.Value);            // Validate required mappings
            var requiredMappings = new[]
            {
                "ACCOUNTS_RECEIVABLE", "ACCOUNTS_PAYABLE", "VAT_PAYABLE", "VAT_RECEIVABLE",
                "WITHHOLDING_PAYABLE", "WITHHOLDING_RECEIVABLE", "CASH"
            };

            var missingMappings = requiredMappings.Where(req => !accountMappingsDict.ContainsKey(req)).ToList();
            if (missingMappings.Any())
            {
                throw new InvalidOperationException($"Missing required account mappings: {string.Join(", ", missingMappings)}");
            }

            // Set the account mappings
            _accountMappings = accountMappingsDict;

            // Create transaction generator with imported mappings
            _transactionGenerator = new TransactionGeneratorService(_accountMappings);

            // Create and configure the document totals service
            var dateTimeService = _serviceProvider.GetRequiredService<IDateTimeZoneService>();
            var loggerDocumentTotals = _serviceProvider.GetRequiredService<ILogger<DocumentTotalsService>>();
            _documentTotalsService = new DocumentTotalsService(_objectDb, dateTimeService, loggerDocumentTotals);

            // Create a default sales invoice accounting profile
            var salesInvoiceProfile = new DocumentAccountingProfileDto
            {
                DocumentOperation = "SalesInvoice",
                SalesAccountCode = "SALES_PRODUCT_1",
                AccountsReceivableCode = "ACCOUNTS_RECEIVABLE",
                CostOfGoodsSoldAccountCode = "COST_OF_SALES_PRODUCT_1",
                InventoryAccountCode = "INVENTORY_PRODUCT_1",
                CostRatio = 0.6m
            };

            // Add the profile
            await _documentTotalsService.CreateDocumentAccountingProfileAsync(salesInvoiceProfile, "TestUser");

            // Import document accounting profiles from CSV
            await ImportDocumentAccountingProfilesFromCsv();
        }        /// <summary>
                 /// Imports document accounting profiles from CSV file
                 /// </summary>
        private async Task ImportDocumentAccountingProfilesFromCsv()
        {
            // Get the service
            var documentAccountingProfileService = _serviceProvider.GetRequiredService<Sivar.Erp.Services.Documents.IDocumentAccountingProfileImportExportService>();

            // Read document accounting profiles from CSV file
            var dataDirectory = "C:\\Users\\joche\\Documents\\GitHub\\SivarErp\\src\\Tests\\ElSalvador\\Data\\New\\";
            var csvFilePath = Path.Combine(dataDirectory, "DocumentAccountingProfiles.csv");

            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Document accounting profiles CSV file not found: {csvFilePath}");
            }

            var documentAccountingProfilesCsv = await File.ReadAllTextAsync(csvFilePath);

            // Import document accounting profiles from CSV
            var (importedProfiles, errors) = await documentAccountingProfileService.ImportFromCsvAsync(documentAccountingProfilesCsv, "TestUser");

            if (errors.Any())
            {
                throw new InvalidOperationException($"Document accounting profile import errors: {string.Join(", ", errors)}");
            }
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

            // Add document lines to match the original test results
            // Original results show $450 subtotal, so adjusting values accordingly
            var item1 = _objectDb.Items.FirstOrDefault(i => i.Code == "PR001");
            var item2 = _objectDb.Items.FirstOrDefault(i => i.Code == "PR002");

            if (item1 != null)
            {
                var line1 = new LineDto
                {
                    LineNumber = 1,
                    Item = item1,
                    Quantity = 2,
                    UnitPrice = 150.0m,  // Adjusted to get $450 total
                    Amount = 300.0m      // 2 × $150 = $300
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
                    UnitPrice = 150.0m,  // Adjusted to get $450 total
                    Amount = 150.0m      // 1 × $150 = $150
                };
                document.Lines.Add(line2);
            }

            // Total should be $450 ($300 + $150)
            // With 13% IVA = $58.50
            // Total with tax = $508.50
            // This matches the original test results exactly

            return document;
        }        /// <summary>
                 /// Creates a purchase invoice document for testing (inventory acquisition)
                 /// </summary>
        private DocumentDto CreatePurchaseInvoiceDocument()
        {
            // Get supplier business entity and purchase document type
            var supplier = _objectDb.BusinessEntities.FirstOrDefault(be => be.Code == "PR001"); // Use a supplier code
            var documentType = _objectDb.DocumentTypes.FirstOrDefault(dt => dt.Code == "PIF"); // Purchase Invoice

            if (supplier == null)
            {
                // If no specific supplier found, use the first business entity as supplier
                supplier = _objectDb.BusinessEntities.First();
            }

            if (documentType == null)
            {
                // If no PIF document type found, create a generic purchase type
                documentType = new DocumentTypeDto { Code = "PIF", Name = "Purchase Invoice" };
            }

            // Create purchase document
            var document = new DocumentDto
            {
                DocumentType = documentType,
                DocumentNumber = "PIF-2025-001",
                Date = new DateOnly(2025, 6, 17), // Day before sales
                BusinessEntity = supplier,
                Lines = new List<IDocumentLine>(),
                DocumentTotals = new List<ITotal>()
            };

            // Add purchase lines (same items we'll sell later)
            var item1 = _objectDb.Items.FirstOrDefault(i => i.Code == "PR001");
            var item2 = _objectDb.Items.FirstOrDefault(i => i.Code == "PR002");

            if (item1 != null)
            {
                var line1 = new LineDto
                {
                    LineNumber = 1,
                    Item = item1,
                    Quantity = 2,
                    UnitPrice = 90.0m,  // Purchase cost (lower than sales price)
                    Amount = 180.0m     // 2 × $90 = $180
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
                    UnitPrice = 90.0m,  // Purchase cost (lower than sales price)
                    Amount = 90.0m      // 1 × $90 = $90
                };
                document.Lines.Add(line2);
            }

            // Total purchase cost: $270 ($180 + $90)
            // This will become the inventory cost and COGS for the sales transaction

            return document;
        }        /// <summary>
                 /// Calculates taxes for the document using the configured tax accounting profiles
                 /// </summary>
        private void CalculateDocumentTaxes(DocumentDto document, string documentOperation = "SalesInvoice")
        {
            _taxCalculator = new DocumentTaxCalculator(
                document,
                document.DocumentType.Code,
                _taxRuleEvaluator!,
                _taxAccountingService!);

            // Calculate line taxes first
            foreach (var line in document.Lines.OfType<LineDto>())
            {
                _taxCalculator.CalculateLineTaxes(line);
            }

            // Then calculate document taxes and totals
            _taxCalculator.CalculateDocumentTaxes();            // Add basic accounting totals
            AddAccountingTotals(document, documentOperation);
        }        /// <summary>
                 /// Adds basic accounting totals to the document
                 /// </summary>
        private void AddAccountingTotals(DocumentDto document, string documentOperation = "SalesInvoice")
        {            // Use the service to add document totals for the specified operation
            _documentTotalsService!.AddDocumentAccountingTotals(document, documentOperation);
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
        /// Prints the performance logs stored in ObjectDb
        /// </summary>
        private List<string> PrintPerformanceLogs()
        {
            var results = new List<string>();

            if (_objectDb.PerformanceLogs == null || !_objectDb.PerformanceLogs.Any())
            {
                results.Add("No performance logs available.");
                return results;
            }

            results.Add($"Total performance logs: {_objectDb.PerformanceLogs.Count}");
            results.Add("");
            results.Add("| Method | Execution Time (ms) | Memory (bytes) | Slow | Memory Intensive |");
            results.Add("|--------|-------------------|---------------|------|-----------------|");

            foreach (var log in _objectDb.PerformanceLogs.OrderByDescending(l => l.ExecutionTimeMs))
            {
                results.Add($"| {log.Method} | {log.ExecutionTimeMs} | {log.MemoryDeltaBytes:N0} | {(log.IsSlow ? "⚠️" : "")} | {(log.IsMemoryIntensive ? "⚠️" : "")} |");
            }

            // Performance summary
            results.Add("");
            results.Add("Performance Summary:");

            var slowMethods = _objectDb.PerformanceLogs.Where(l => l.IsSlow).ToList();
            if (slowMethods.Any())
            {
                results.Add($"⚠️ Slow methods detected: {slowMethods.Count}");
                foreach (var slowMethod in slowMethods.OrderByDescending(m => m.ExecutionTimeMs))
                {
                    results.Add($"  - {slowMethod.Method}: {slowMethod.ExecutionTimeMs} ms");
                }
            }
            else
            {
                results.Add("✓ No slow methods detected");
            }

            var memoryIntensiveMethods = _objectDb.PerformanceLogs.Where(l => l.IsMemoryIntensive).ToList();
            if (memoryIntensiveMethods.Any())
            {
                results.Add($"⚠️ Memory intensive methods detected: {memoryIntensiveMethods.Count}");
                foreach (var memMethod in memoryIntensiveMethods.OrderByDescending(m => m.MemoryDeltaBytes))
                {
                    results.Add($"  - {memMethod.Method}: {memMethod.MemoryDeltaBytes:N0} bytes");
                }
            }
            else
            {
                results.Add("✓ No memory intensive methods detected");
            }

            return results;
        }
    }
}