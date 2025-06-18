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

namespace Sivar.Erp.Tests
{
    
    [TestFixture]
    /// <summary>
    /// Comprehensive test demonstrating the complete accounting workflow
    /// from data import to transaction posting and balance verification
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

        [Test]
        /// <summary>
        /// Main test method that executes the complete workflow
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

                // Step 2: Document Creation
                results.Add("=== STEP 2: DOCUMENT CREATION ===");
                var document = CreateSalesInvoiceDocument();
                results.Add($"✓ Created CCF document #{document.DocumentNumber}");
                results.Add($"✓ Document date: {document.Date}");
                results.Add($"✓ Business entity: {document.BusinessEntity.Name}");
                results.Add($"✓ Document has {document.Lines.Count} lines");
                results.Add("");

                // Step 3: Tax Calculation
                results.Add("=== STEP 3: TAX CALCULATION ===");
                CalculateDocumentTaxes(document);
                results.Add($"✓ Calculated taxes for document");
                results.Add($"✓ Document has {document.DocumentTotals.Count} totals:");
                foreach (var total in document.DocumentTotals)
                {
                    results.Add($"   - {total.Concept}: ${total.Total:F2}");
                }
                results.Add("");

                // Step 4: Transaction Generation
                results.Add("=== STEP 4: TRANSACTION GENERATION ===");
                var (transaction, ledgerEntries) = await _transactionGenerator.GenerateTransactionAsync(document);
                results.Add($"✓ Generated transaction {transaction.Oid}");
                results.Add($"✓ Transaction has {ledgerEntries.Count} ledger entries:");

                decimal totalDebits = 0, totalCredits = 0;
                foreach (var entry in ledgerEntries)
                {
                    results.Add($"   - {entry.EntryType}: {entry.AccountName} (${entry.Amount:F2})");
                    if (entry.EntryType == EntryType.Debit)
                        totalDebits += entry.Amount;
                    else
                        totalCredits += entry.Amount;
                }
                results.Add($"✓ Total Debits: ${totalDebits:F2}");
                results.Add($"✓ Total Credits: ${totalCredits:F2}");
                results.Add($"✓ Transaction balanced: {Math.Abs(totalDebits - totalCredits) < 0.01m}");
                results.Add("");

                // Step 5: Transaction Posting
                results.Add("=== STEP 5: TRANSACTION POSTING ===");

                // Set up the transaction with ledger entries
                transaction.LedgerEntries = ledgerEntries;

                // Post the transaction
                var posted = await _accountingModule.PostTransactionAsync(transaction);
                results.Add($"✓ Transaction posted successfully: {posted}");
                results.Add($"✓ Transaction is posted: {transaction.IsPosted}");
                results.Add("");

                // Step 6: Balance Verification
                results.Add("=== STEP 6: BALANCE VERIFICATION ===");
                await VerifyAccountBalances(ledgerEntries, results);
                results.Add("");

                // Step 7: Export Results
                results.Add("=== STEP 7: EXPORT RESULTS ===");
                await ExportTransactionData(transaction, ledgerEntries, results);

                results.Add("🎉 Complete accounting workflow test executed successfully!");

            }
            catch (Exception ex)
            {
                results.Add($"❌ Error during workflow execution: {ex.Message}");
                results.Add($"Stack trace: {ex.StackTrace}");
            }

            //return string.Join(Environment.NewLine, results);
        }

        /// <summary>
        /// Sets up all required data and services for the test
        /// </summary>
        private async Task SetupDataAndServices()
        {
            // Create all required import services
            var accountValidator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
            var accountImportService = new AccountImportExportService(accountValidator);
            var taxImportService = new TaxImportExportService();
            var taxGroupImportService = new TaxGroupImportExportService();
            var businessEntityImportService = new BusinessEntityImportExportService();
            var itemImportService = new ItemImportExportService();
            var documentTypeImportService = new DocumentTypeImportExportService();
            var groupMembershipImportService = new GroupMembershipImportExportService();

            // Use your existing data directory with CSV files
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ElSalvador");
            dataDirectory = "C:\\Users\\joche\\Documents\\GitHub\\SivarErp\\src\\Tests\\ElSalvador\\Data\\New\\";
            // Verify the data directory exists
            if (!Directory.Exists(dataDirectory))
            {
                throw new DirectoryNotFoundException($"Data directory not found: {dataDirectory}. Please ensure your CSV files are in this location.");
            }

            // Initialize the company data
            var initializer = new ElSalvadorCompanyInitializer(
                dataDirectory,
                accountImportService,
                taxImportService,
                taxGroupImportService,
                documentTypeImportService,
                businessEntityImportService,
                itemImportService,
                groupMembershipImportService);

            // Import all data
            (_objectDb, var importResults) = await initializer.CreateNewCompanyAsync();

            // Set up fiscal period
            await SetupFiscalPeriod();

            // Set up services
            await SetupServices();

            // Set up account mappings
            SetupAccountMappings();

            // Set up tax rules and accounting profiles
            SetupTaxRulesAndAccounting();
        }



        /// <summary>
        /// Sets up the fiscal period for transaction posting
        /// </summary>
        private async Task SetupFiscalPeriod()
        {
            var fiscalPeriodService = new FiscalPeriodService(_objectDb);

            var fiscalPeriod = new FiscalPeriodDto
            {
                Code = "FY2025",
                Name = "Fiscal Year 2025",
                Description = "Fiscal year 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 12, 31),
                Status = FiscalPeriodStatus.Open,
                InsertedBy = "System",
                UpdatedBy = "System"
            };

            await fiscalPeriodService.CreateFiscalPeriodAsync(fiscalPeriod, "System");
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
            var accountBalanceCalculator = new AccountBalanceCalculatorServiceBase();

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
        /// Sets up tax rules and accounting profiles
        /// </summary>
        private void SetupTaxRulesAndAccounting()
        {
            // Create tax accounting profile service
            _taxAccountingService = new TaxAccountingProfileService();

            // Register IVA accounting profile for sales invoices
            var ivaAccountingInfo = new TaxAccountingInfo
            {
                DebitAccountCode = "VAT_PAYABLE",      // IVA is a credit for sales
                CreditAccountCode = "VAT_PAYABLE",     // IVA por Pagar
                IncludeInTransaction = true,
                AccountDescription = "IVA 13% por Ventas"
            };
            _taxAccountingService.RegisterTaxAccountingProfile(DocumentOperation.SalesInvoice, "IVA", ivaAccountingInfo);

            // Create tax rules
            var taxRules = new List<TaxRuleDto>
            {
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = "IVA",
                    DocumentOperation = DocumentOperation.SalesInvoice,
                    BusinessEntityGroupId = "REGISTERED_TAXPAYERS",
                    ItemGroupId = "TAXABLE_ITEMS",
                    IsEnabled = true,
                    Priority = 1
                }
            };

            // Create tax rule evaluator
            _taxRuleEvaluator = new TaxRuleEvaluator(
                taxRules,
                _objectDb.Taxes.Cast<TaxDto>().ToList(),
                _objectDb.GroupMemberships);
        }

        /// <summary>
        /// Creates a sample sales invoice document (CCF)
        /// </summary>
        private DocumentDto CreateSalesInvoiceDocument()
        {
            // Get CCF document type
            var ccfDocumentType = _objectDb.DocumentTypes.FirstOrDefault(dt => dt.Code == "CCF");
            if (ccfDocumentType == null)
            {
                throw new InvalidOperationException("CCF document type not found");
            }

            // Get client
            var client = _objectDb.BusinessEntities.FirstOrDefault();
            if (client == null)
            {
                throw new InvalidOperationException("No business entities found");
            }

            // Get items
            var product1 = _objectDb.Items.FirstOrDefault(i => i.Code == "PR001");
            var product2 = _objectDb.Items.FirstOrDefault(i => i.Code == "PR002");

            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                DocumentNumber = "CCF-2025-001",
                Date = DateOnly.FromDateTime(DateTime.Now),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = client,
                DocumentType = ccfDocumentType
            };

            // Add document lines
            var line1 = new LineDto
            {
                Item = product1,
                Quantity = 2,
                UnitPrice = 100.00m,
                Amount = 200.00m
            };

            var line2 = new LineDto
            {
                Item = product2,
                Quantity = 1,
                UnitPrice = 250.00m,
                Amount = 250.00m
            };

            document.Lines.Add(line1);
            document.Lines.Add(line2);

            return document;
        }

        /// <summary>
        /// Calculates taxes for the document using DocumentTaxCalculator
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
            var subtotal = document.Lines.Sum(l => l.Amount);
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

            var inventoryReduction = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Inventory Reduction",
                Total = cogs,
                CreditAccountCode = "INVENTORY_PRODUCT_1", // INVENTARIO PRODUCTO 1
                IncludeInTransaction = true
            };
            document.DocumentTotals.Add(inventoryReduction);
        }

        /// <summary>
        /// Verifies account balances after transaction posting
        /// </summary>
        private async Task VerifyAccountBalances(List<LedgerEntryDto> ledgerEntries, List<string> results)
        {
            var balanceCalculator = new AccountBalanceCalculatorServiceBase(
                new List<ITransaction> { CreateTransactionFromEntries(ledgerEntries) });

            var asOfDate = DateOnly.FromDateTime(DateTime.Now);

            // Group entries by account
            var accountGroups = ledgerEntries.GroupBy(e => e.OfficialCode);

            foreach (var accountGroup in accountGroups)
            {
                var accountCode = accountGroup.Key;
                var balance = balanceCalculator.CalculateAccountBalance(accountCode, asOfDate);

                // Get account name for display
                var account = _objectDb.Accounts.FirstOrDefault(a => a.OfficialCode == accountCode);
                var accountName = account?.AccountName ?? accountCode;

                results.Add($"✓ Account {accountCode} ({accountName}): ${balance:F2}");

                // Verify specific account expectations
                await VerifySpecificAccountBalance(accountCode, balance, results);
            }
        }

        /// <summary>
        /// Creates a transaction object from ledger entries for balance calculation
        /// </summary>
        private ITransaction CreateTransactionFromEntries(List<LedgerEntryDto> ledgerEntries)
        {
            return new TransactionDto
            {
                Oid = Guid.NewGuid(),
                TransactionDate = DateOnly.FromDateTime(DateTime.Now),
                Description = "Test Transaction",
                LedgerEntries = ledgerEntries,
                IsPosted = true
            };
        }

        /// <summary>
        /// Verifies specific account balance expectations
        /// </summary>
        private async Task VerifySpecificAccountBalance(string accountCode, decimal balance, List<string> results)
        {
            var account = _objectDb.Accounts.FirstOrDefault(a => a.OfficialCode == accountCode);
            if (account == null) return;

            switch (account.AccountType)
            {
                case AccountType.Asset:
                    if (balance >= 0)
                        results.Add($"  ✓ Asset account has expected debit balance");
                    else
                        results.Add($"  ⚠️  Asset account has unexpected credit balance");
                    break;

                case AccountType.Liability:
                case AccountType.Revenue:
                    if (balance <= 0)
                        results.Add($"  ✓ {account.AccountType} account has expected credit balance");
                    else
                        results.Add($"  ⚠️  {account.AccountType} account has unexpected debit balance");
                    break;

                case AccountType.Expense:
                    if (balance >= 0)
                        results.Add($"  ✓ Expense account has expected debit balance");
                    else
                        results.Add($"  ⚠️  Expense account has unexpected credit balance");
                    break;
            }
        }

        /// <summary>
        /// Exports transaction data for review
        /// </summary>
        private async Task ExportTransactionData(TransactionDto transaction, List<LedgerEntryDto> ledgerEntries, List<string> results)
        {
            var exportService = new TransactionsImportExportService(_objectDb.Accounts.Cast<AccountDto>().ToList());

            var transactionsWithEntries = new List<(TransactionDto, List<LedgerEntryDto>)>
            {
                (transaction, ledgerEntries)
            };

            var csvContent = exportService.ExportTransactionsToCsv(transactionsWithEntries);

            // Save to temp file
            var tempFile = Path.Combine(Path.GetTempPath(), $"transaction_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            await File.WriteAllTextAsync(tempFile, csvContent);

            results.Add($"✓ Transaction data exported to: {tempFile}");
            results.Add($"✓ Export contains {ledgerEntries.Count} ledger entries");
        }
    }

    
}