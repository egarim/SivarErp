using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Sivar.Erp.Documents;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.FiscalPeriods;
using Sivar.Erp.Taxes.TaxGroup;
using Sivar.Erp.Documents.Tax;
using Sivar.Erp.Taxes.TaxRule;
using Sivar.Erp;
using Sivar.Erp.Taxes;

namespace Tests.IntegrationTests.ElSalvador
{
    /// <summary>
    /// Complete integration test simulating the full business flow for El Salvador
    /// Flow: Import chart of accounts -> Create document types -> Create transaction templates -> 
    /// Create fiscal period -> Create items -> Create tax groups -> Create customers -> Get trial balance
    /// </summary>
    [TestFixture]
    public class ElSalvadorCompleteFlowTests
    {
        private TransactionService _transactionService;
        private AccountValidator _accountValidator;
        private AccountBalanceCalculatorBase _accountBalanceCalculator;
        private IAccountImportExportService _accountImportExportService;
        private FiscalPeriodService _fiscalPeriodService;

        // Collections to store our data
        private Dictionary<string, AccountDto> _accounts;
        private Dictionary<string, DocumentTypeDto> _documentTypes;
        private Dictionary<string, ITransaction> _transactionTemplates;
        private Dictionary<string, FiscalPeriodDto> _fiscalPeriods;
        private Dictionary<string, ItemDto> _items;
        private Dictionary<string, TaxGroupDto> _taxGroups;
        private Dictionary<string, BusinessEntityDto> _customers;
        private Dictionary<string, TaxDto> _taxes;
        private List<TaxRuleDto> _taxRules;
        private List<GroupMembershipDto> _groupMemberships;

        // Constants for testing
        private const string TEST_USER = "CompleteFlowTestUser";
        private readonly DateOnly _testStartDate = new DateOnly(2024, 1, 1);  // Beginning of fiscal year
        private readonly DateOnly _testEndDate = new DateOnly(2024, 12, 31);  // End of fiscal year

        // Company details
        private readonly string _companyName = "El Salvador Complete Flow Test Company S.A de C.V";
        private readonly string _companyAddress = "Avenida Principal #456, San Salvador, El Salvador";
        private readonly string _companyCurrency = "USD"; [SetUp]
        public void Setup()
        {
            // Initialize services
            _transactionService = new TransactionService();
            _accountValidator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
            _accountBalanceCalculator = new AccountBalanceCalculatorBase();
            _accountImportExportService = new AccountImportExportService(_accountValidator);
            _fiscalPeriodService = new FiscalPeriodService();

            // Initialize collections
            _accounts = new Dictionary<string, AccountDto>();
            _documentTypes = new Dictionary<string, DocumentTypeDto>();
            _transactionTemplates = new Dictionary<string, ITransaction>();
            _fiscalPeriods = new Dictionary<string, FiscalPeriodDto>();
            _items = new Dictionary<string, ItemDto>();
            _taxGroups = new Dictionary<string, TaxGroupDto>();
            _customers = new Dictionary<string, BusinessEntityDto>();
            _taxes = new Dictionary<string, TaxDto>();
            _taxRules = new List<TaxRuleDto>();
            _groupMemberships = new List<GroupMembershipDto>();

            Console.WriteLine("=== Starting Complete Flow Test ===");
        }

        [Test]
        public async Task CompleteBusinessFlow_ShouldExecuteAllStepsSuccessfully()
        {
            // Step 1: Import chart of accounts
            Console.WriteLine("\n=== Step 1: Import Chart of Accounts ===");
            await ImportChartOfAccounts();

            // Step 2: Create document types
            Console.WriteLine("\n=== Step 2: Create Document Types ===");
            CreateDocumentTypes();

            // Step 3: Create transaction templates
            Console.WriteLine("\n=== Step 3: Create Transaction Templates ===");
            CreateTransactionTemplates();

            // Step 4: Create fiscal period
            Console.WriteLine("\n=== Step 4: Create Fiscal Period ===");
            CreateFiscalPeriod();

            // Step 5: Create items
            Console.WriteLine("\n=== Step 5: Create Items ===");
            CreateItems();

            // Step 6: Create tax groups
            Console.WriteLine("\n=== Step 6: Create Tax Groups ===");
            CreateTaxGroups();

            // Step 7: Create customers
            Console.WriteLine("\n=== Step 7: Create Customers ===");
            CreateCustomers();

            // Step 8: Create and execute tax transactions
            Console.WriteLine("\n=== Step 8: Create and Execute Tax Transactions ===");
            await CreateTaxTransactions();            // Step 9: Get trial balance (moved from step 8)
            Console.WriteLine("\n=== Step 9: Get Trial Balance ===");
            GetTrialBalance();

            Console.WriteLine("\n=== Complete Flow Test Completed Successfully ===");
        }

        #region Step 1: Import Chart of Accounts

        /// <summary>
        /// Step 1: Import chart of accounts from CSV file
        /// </summary>
        private async Task ImportChartOfAccounts()
        {
            try
            {
                // Load CSV content from embedded resource or file
                string csvContent = await LoadCsvContent();

                // Import accounts from CSV
                var (importedAccounts, errors) = await _accountImportExportService.ImportFromCsvAsync(csvContent, TEST_USER);

                Console.WriteLine($"CSV import results: {importedAccounts.Count()} accounts imported, {errors.Count()} errors");

                if (errors.Any())
                {
                    Console.WriteLine("Errors during CSV import:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"- {error}");
                    }
                }

                // Add imported accounts to our dictionary
                foreach (var account in importedAccounts.Cast<AccountDto>())
                {
                    string key = $"{account.OfficialCode}_{account.AccountName}";
                    int counter = 1;
                    string originalKey = key;
                    while (_accounts.ContainsKey(key))
                    {
                        key = $"{originalKey}_{counter}";
                        counter++;
                    }
                    _accounts[key] = account;
                }

                Console.WriteLine($"Total accounts loaded: {_accounts.Count}");

                // Verify accounts by type
                var assetAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Asset);
                var liabilityAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Liability);
                var equityAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Equity);
                var revenueAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Revenue);
                var expenseAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Expense);

                Console.WriteLine($"- Asset accounts: {assetAccounts}");
                Console.WriteLine($"- Liability accounts: {liabilityAccounts}");
                Console.WriteLine($"- Equity accounts: {equityAccounts}");
                Console.WriteLine($"- Revenue accounts: {revenueAccounts}");
                Console.WriteLine($"- Expense accounts: {expenseAccounts}");

                // Assertions
                Assert.That(_accounts.Count, Is.GreaterThan(100), "Should have imported many accounts from CSV");
                Assert.That(assetAccounts, Is.GreaterThan(50), "Should have many asset accounts");
                Assert.That(liabilityAccounts, Is.GreaterThan(10), "Should have liability accounts");
                Assert.That(equityAccounts, Is.GreaterThan(5), "Should have equity accounts");
                Assert.That(revenueAccounts, Is.GreaterThan(5), "Should have revenue accounts");
                Assert.That(expenseAccounts, Is.GreaterThan(20), "Should have expense accounts");

                Console.WriteLine("✓ Chart of accounts imported successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing chart of accounts: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load the CSV content for chart of accounts
        /// </summary>
        private async Task<string> LoadCsvContent()
        {
            try
            {
                // Load from embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "Sivar.Erp.Tests.IntegrationTests.chart_of_accounts_csv_complete.txt";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    return await reader.ReadToEndAsync();
                }

                // Fallback: try direct file path
                string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "chart_of_accounts_csv_complete.txt");
                if (File.Exists(filePath))
                {
                    return await File.ReadAllTextAsync(filePath);
                }

                // Fallback: try relative path from project directory
                string projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
                for (int i = 0; i < 5; i++)
                {
                    string testFilePath = Path.Combine(projectPath, "IntegrationTests", "chart_of_accounts_csv_complete.txt");
                    if (File.Exists(testFilePath))
                    {
                        return await File.ReadAllTextAsync(testFilePath);
                    }
                    projectPath = Path.GetDirectoryName(projectPath)!;
                    if (string.IsNullOrEmpty(projectPath)) break;
                }

                throw new FileNotFoundException($"Could not find chart_of_accounts_csv_complete.txt. Resource name tried: {resourceName}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading CSV content: {ex.Message}", ex);
            }
        }

        #endregion

        #region Step 2: Create Document Types

        /// <summary>
        /// Step 2: Create document types for El Salvador
        /// </summary>
        private void CreateDocumentTypes()
        {
            try
            {
                // Create Credito Fiscal document type
                var creditoFiscal = new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "CF",
                    Name = "Credito Fiscal",
                    IsEnabled = true
                };

                // Create Consumidor Final document type
                var consumidorFinal = new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "CNF",
                    Name = "Consumidor Final",
                    IsEnabled = true
                };

                // Create Factura de Exportacion document type
                var facturaExportacion = new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "FEX",
                    Name = "Factura de Exportacion",
                    IsEnabled = true
                };

                // Create Comprobante de Credito Fiscal document type
                var comprobanteCreditoFiscal = new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "CCF",
                    Name = "Comprobante de Credito Fiscal",
                    IsEnabled = true
                };

                // Create Purchase Order document type
                var purchaseOrder = new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "PO",
                    Name = "Purchase Order",
                    IsEnabled = true
                };

                // Create Receipt document type
                var receipt = new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "REC",
                    Name = "Receipt",
                    IsEnabled = true
                };

                // Add to dictionary
                _documentTypes["CF"] = creditoFiscal;
                _documentTypes["CNF"] = consumidorFinal;
                _documentTypes["FEX"] = facturaExportacion;
                _documentTypes["CCF"] = comprobanteCreditoFiscal;
                _documentTypes["PO"] = purchaseOrder;
                _documentTypes["REC"] = receipt;

                Console.WriteLine($"Created {_documentTypes.Count} document types:");
                foreach (var docType in _documentTypes.Values)
                {
                    Console.WriteLine($"- {docType.Code}: {docType.Name}");
                }

                // Assertions
                Assert.That(_documentTypes.Count, Is.EqualTo(6), "Should have created 6 document types");
                Assert.That(_documentTypes.ContainsKey("CF"), Is.True, "Should have Credito Fiscal");
                Assert.That(_documentTypes.ContainsKey("CNF"), Is.True, "Should have Consumidor Final");

                Console.WriteLine("✓ Document types created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating document types: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Step 3: Create Transaction Templates        /// <summary>
        /// Step 3: Create transaction templates for common business operations using specific account codes from the CSV
        /// </summary>
        private void CreateTransactionTemplates()
        {
            try
            {                // Get specific accounts using their official codes from the CSV file
                var cashAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "11010101"); // CAJA GENERAL
                var bankAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "11010201"); // BANCO CUSCATLAN
                var clientsAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "11030101"); // CLIENTES NACIONALES
                var salesRevenueAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "51010101"); // VENTA DE PRODUCTO 1
                var inventoryAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "11050102"); // INVENTARIO DE PRODUCTO TERMINADO
                var cogsAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "41010101"); // COSTO DE VENTA DE AGENDAS
                var providersAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "21020101"); // PROVEEDORES LOCALES
                var ivaDebitoAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "21060101"); // IVA DEBITO FISCAL - CONTRIBUYENTES
                var ivaCreditoAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "11070101"); // CREDITO FISCAL- COMPRA LOCAL

                // Get product-specific revenue accounts
                var ventaProducto2 = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "51010102"); // VENTA DE PRODUCTO 2
                var ventaProducto3 = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "51010103"); // VENTA DE PRODUCTO 3
                var ventaProducto4 = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "51010104"); // VENTA DE PRODUCTO 4
                var ventaProducto5 = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "51010105"); // VENTA DE PRODUCTO 5
                var ventaProducto6 = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "51010106"); // VENTA DE PRODUCTO 6

                // Get product-specific cost accounts  
                var costoSombrillas = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "41010102"); // COSTO DE VENTA DE SOMBRILLAS
                var costoCuadernos = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "41010103"); // COSTO DE VENTA DE CUADERNOS
                var costoPromocionales = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "41010104"); // COSTO DE VENTA DE PRODUCTOS PROMOCIONALES
                var costoTextiles = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "41010105"); // COSTO DE VENTA DE TEXTILES
                var costoMarroquineria = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "41010106"); // COSTO DE VENTA DE MARROQUINERIA                // Log which accounts were found
                Console.WriteLine("Account lookup results:");
                Console.WriteLine($"- Cash account (11010101): {(cashAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Bank account (11010201): {(bankAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Clients account (11030101): {(clientsAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Sales revenue account (51010101): {(salesRevenueAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Inventory account (11050102): {(inventoryAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- COGS account (41010101): {(cogsAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Providers account (21020101): {(providersAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- IVA Debito account (21060101): {(ivaDebitoAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- IVA Credito account (11070101): {(ivaCreditoAccount != null ? "FOUND" : "NOT FOUND")}");

                Console.WriteLine("Product-specific accounts:");
                Console.WriteLine($"- Venta Producto 2 (51010102): {(ventaProducto2 != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Venta Producto 3 (51010103): {(ventaProducto3 != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Venta Producto 4 (51010104): {(ventaProducto4 != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Costo Sombrillas (41010102): {(costoSombrillas != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Costo Cuadernos (41010103): {(costoCuadernos != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Costo Promocionales (41010104): {(costoPromocionales != null ? "FOUND" : "NOT FOUND")}");

                if (cashAccount == null || salesRevenueAccount == null)
                {
                    Console.WriteLine("Warning: Could not find required accounts for transaction templates");
                    Console.WriteLine($"Cash account (11010101) found: {cashAccount != null}");
                    Console.WriteLine($"Bank account (11010201) found: {bankAccount != null}");
                    Console.WriteLine($"Clients account (11030101) found: {clientsAccount != null}");
                    Console.WriteLine($"Sales revenue account (51010101) found: {salesRevenueAccount != null}");
                    Console.WriteLine($"Inventory account (11050102) found: {inventoryAccount != null}");
                    Console.WriteLine($"COGS account (41010101) found: {cogsAccount != null}");
                    Console.WriteLine($"Providers account (21020101) found: {providersAccount != null}");
                    Console.WriteLine($"IVA Debito account (21060101) found: {ivaDebitoAccount != null}");
                    Console.WriteLine($"IVA Credito account (11070101) found: {ivaCreditoAccount != null}");
                    return;
                }// Template 1: Cash Sales Transaction (Cash -> Sales Revenue + IVA)
                var cashSalesTemplate = new TransactionDto
                {
                    Id = Guid.NewGuid(),
                    DocumentId = Guid.NewGuid(),
                    TransactionDate = _testStartDate,
                    Description = "Cash Sales Transaction Template",
                    LedgerEntries = new List<ILedgerEntry>
                    {
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = cashAccount.Id,
                            EntryType = EntryType.Debit,
                            Amount = 1130m, // $1000 + $130 IVA
                            AccountName = cashAccount.AccountName,
                            OfficialCode = cashAccount.OfficialCode
                        },
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = salesRevenueAccount.Id,
                            EntryType = EntryType.Credit,
                            Amount = 1000m,
                            AccountName = salesRevenueAccount.AccountName,
                            OfficialCode = salesRevenueAccount.OfficialCode
                        },
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = ivaDebitoAccount.Id,
                            EntryType = EntryType.Credit,
                            Amount = 130m, // 13% IVA
                            AccountName = ivaDebitoAccount.AccountName,
                            OfficialCode = ivaDebitoAccount.OfficialCode
                        }
                    }
                };

                // Template 2: Credit Sales Transaction (Accounts Receivable -> Sales Revenue + IVA)
                var creditSalesTemplate = new TransactionDto
                {
                    Id = Guid.NewGuid(),
                    DocumentId = Guid.NewGuid(),
                    TransactionDate = _testStartDate,
                    Description = "Credit Sales Transaction Template",
                    LedgerEntries = new List<ILedgerEntry>
                    {
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = clientsAccount.Id,
                            EntryType = EntryType.Debit,
                            Amount = 2260m, // $2000 + $260 IVA
                            AccountName = clientsAccount.AccountName,
                            OfficialCode = clientsAccount.OfficialCode
                        },
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = salesRevenueAccount.Id,
                            EntryType = EntryType.Credit,
                            Amount = 2000m,
                            AccountName = salesRevenueAccount.AccountName,
                            OfficialCode = salesRevenueAccount.OfficialCode
                        },
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = ivaDebitoAccount.Id,
                            EntryType = EntryType.Credit,
                            Amount = 260m, // 13% IVA
                            AccountName = ivaDebitoAccount.AccountName,
                            OfficialCode = ivaDebitoAccount.OfficialCode
                        }
                    }
                };

                // Template 3: Purchase Transaction with IVA Credit (Inventory + IVA Credit -> Accounts Payable)
                TransactionDto? purchaseTemplate = null;
                if (inventoryAccount != null && providersAccount != null && ivaCreditoAccount != null)
                {
                    purchaseTemplate = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate,
                        Description = "Purchase Transaction Template with IVA",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = inventoryAccount.Id,
                                EntryType = EntryType.Debit,
                                Amount = 500m,
                                AccountName = inventoryAccount.AccountName,
                                OfficialCode = inventoryAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = ivaCreditoAccount.Id,
                                EntryType = EntryType.Debit,
                                Amount = 65m, // 13% IVA Credit
                                AccountName = ivaCreditoAccount.AccountName,
                                OfficialCode = ivaCreditoAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = providersAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 565m, // $500 + $65 IVA
                                AccountName = providersAccount.AccountName,
                                OfficialCode = providersAccount.OfficialCode
                            }
                        }
                    };
                }

                // Template 4: Cost of Goods Sold (COGS -> Inventory)
                TransactionDto? cogsTemplate = null;
                if (cogsAccount != null && inventoryAccount != null)
                {
                    cogsTemplate = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate,
                        Description = "Cost of Goods Sold Template",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = cogsAccount.Id,
                                EntryType = EntryType.Debit,
                                Amount = 300m,
                                AccountName = cogsAccount.AccountName,
                                OfficialCode = cogsAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = inventoryAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 300m,
                                AccountName = inventoryAccount.AccountName,
                                OfficialCode = inventoryAccount.OfficialCode
                            }
                        }
                    };
                }

                // Template 5: Bank Deposit (Bank -> Cash)
                TransactionDto? bankDepositTemplate = null;
                if (bankAccount != null)
                {
                    bankDepositTemplate = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate,
                        Description = "Bank Deposit Template",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = bankAccount.Id,
                                EntryType = EntryType.Debit,
                                Amount = 1000m,
                                AccountName = bankAccount.AccountName,
                                OfficialCode = bankAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = cashAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 1000m,
                                AccountName = cashAccount.AccountName,
                                OfficialCode = cashAccount.OfficialCode
                            }
                        }
                    };
                }                // Create product-specific sales templates
                CreateProductSpecificTemplates(_accounts, cashAccount, clientsAccount, ivaDebitoAccount,
                                             inventoryAccount, cogsAccount);

                // Add templates to dictionary
                _transactionTemplates["CashSales"] = cashSalesTemplate;
                _transactionTemplates["CreditSales"] = creditSalesTemplate;
                if (purchaseTemplate != null)
                {
                    _transactionTemplates["Purchase"] = purchaseTemplate;
                }
                if (cogsTemplate != null)
                {
                    _transactionTemplates["COGS"] = cogsTemplate;
                }
                if (bankDepositTemplate != null)
                {
                    _transactionTemplates["BankDeposit"] = bankDepositTemplate;
                }

                Console.WriteLine($"Created {_transactionTemplates.Count} transaction templates:");
                foreach (var template in _transactionTemplates)
                {
                    Console.WriteLine($"- {template.Key}: {template.Value.Description}");
                }                // Assertions
                Assert.That(_transactionTemplates.Count, Is.GreaterThan(0), "Should have created transaction templates");

                Console.WriteLine("✓ Transaction templates created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating transaction templates: {ex.Message}");
                throw;
            }
        }        /// <summary>
                 /// Create product-specific transaction templates for each item
                 /// </summary>
        private void CreateProductSpecificTemplates(Dictionary<string, AccountDto> accounts,
                                                   AccountDto? cashAccount,
                                                   AccountDto? receivablesAccount,
                                                   AccountDto? ivaDebitoAccount,
                                                   AccountDto? inventoryAccount,
                                                   AccountDto? cogsAccount)
        {
            try
            {
                // Product-specific revenue accounts
                var ventaProducto1 = accounts.Values.FirstOrDefault(a => a.OfficialCode == "51010101"); // VENTA DE PRODUCTO 1 - Laptop
                var ventaProducto2 = accounts.Values.FirstOrDefault(a => a.OfficialCode == "51010102"); // VENTA DE PRODUCTO 2 - Office Chair
                var ventaProducto3 = accounts.Values.FirstOrDefault(a => a.OfficialCode == "51010103"); // VENTA DE PRODUCTO 3 - Mobile Phone

                // Product-specific cost accounts - mapping to actual business products
                var costoAgendas = accounts.Values.FirstOrDefault(a => a.OfficialCode == "41010101"); // COSTO DE VENTA DE AGENDAS -> Laptop
                var costoSombrillas = accounts.Values.FirstOrDefault(a => a.OfficialCode == "41010102"); // COSTO DE VENTA DE SOMBRILLAS -> Office Chair
                var costoCuadernos = accounts.Values.FirstOrDefault(a => a.OfficialCode == "41010103"); // COSTO DE VENTA DE CUADERNOS -> Mobile Phone

                Console.WriteLine("Product-specific accounts found:");
                Console.WriteLine($"- Revenue Producto 1 (Laptop): {ventaProducto1?.AccountName} ({ventaProducto1?.OfficialCode})");
                Console.WriteLine($"- Revenue Producto 2 (Office Chair): {ventaProducto2?.AccountName} ({ventaProducto2?.OfficialCode})");
                Console.WriteLine($"- Revenue Producto 3 (Mobile Phone): {ventaProducto3?.AccountName} ({ventaProducto3?.OfficialCode})");
                Console.WriteLine($"- Cost Agendas (Laptop): {costoAgendas?.AccountName} ({costoAgendas?.OfficialCode})");
                Console.WriteLine($"- Cost Sombrillas (Office Chair): {costoSombrillas?.AccountName} ({costoSombrillas?.OfficialCode})");
                Console.WriteLine($"- Cost Cuadernos (Mobile Phone): {costoCuadernos?.AccountName} ({costoCuadernos?.OfficialCode})");

                // Template 1: Laptop Sales (PROD-001)
                if (ventaProducto1 != null && cashAccount != null && ivaDebitoAccount != null)
                {
                    var laptopSalesTemplate = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate,
                        Description = "Laptop Sales Template (PROD-001)",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = cashAccount.Id,
                                EntryType = EntryType.Debit,
                                Amount = 1356m, // $1200 + $156 IVA (13%)
                                AccountName = cashAccount.AccountName,
                                OfficialCode = cashAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = ventaProducto1.Id,
                                EntryType = EntryType.Credit,
                                Amount = 1200m,
                                AccountName = ventaProducto1.AccountName,
                                OfficialCode = ventaProducto1.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = ivaDebitoAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 156m, // 13% IVA
                                AccountName = ivaDebitoAccount.AccountName,
                                OfficialCode = ivaDebitoAccount.OfficialCode
                            }
                        }
                    };
                    _transactionTemplates["LaptopSales"] = laptopSalesTemplate;
                }

                // Template 2: Office Chair Sales (PROD-002)
                if (ventaProducto2 != null && cashAccount != null && ivaDebitoAccount != null)
                {
                    var chairSalesTemplate = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate,
                        Description = "Office Chair Sales Template (PROD-002)",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = cashAccount.Id,
                                EntryType = EntryType.Debit,
                                Amount = 282.50m, // $250 + $32.50 IVA (13%)
                                AccountName = cashAccount.AccountName,
                                OfficialCode = cashAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = ventaProducto2.Id,
                                EntryType = EntryType.Credit,
                                Amount = 250m,
                                AccountName = ventaProducto2.AccountName,
                                OfficialCode = ventaProducto2.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = ivaDebitoAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 32.50m, // 13% IVA
                                AccountName = ivaDebitoAccount.AccountName,
                                OfficialCode = ivaDebitoAccount.OfficialCode
                            }
                        }
                    };
                    _transactionTemplates["OfficeChairSales"] = chairSalesTemplate;
                }

                // Template 3: Mobile Phone Sales (PROD-003)
                if (ventaProducto3 != null && receivablesAccount != null && ivaDebitoAccount != null)
                {
                    var phoneSalesTemplate = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate,
                        Description = "Mobile Phone Credit Sales Template (PROD-003)",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = receivablesAccount.Id,
                                EntryType = EntryType.Debit,
                                Amount = 904m, // $800 + $104 IVA (13%)
                                AccountName = receivablesAccount.AccountName,
                                OfficialCode = receivablesAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = ventaProducto3.Id,
                                EntryType = EntryType.Credit,
                                Amount = 800m,
                                AccountName = ventaProducto3.AccountName,
                                OfficialCode = ventaProducto3.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = ivaDebitoAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 104m, // 13% IVA
                                AccountName = ivaDebitoAccount.AccountName,
                                OfficialCode = ivaDebitoAccount.OfficialCode
                            }
                        }
                    };
                    _transactionTemplates["MobilePhoneSales"] = phoneSalesTemplate;
                }

                // Cost of Goods Sold Templates

                // Template 4: Laptop COGS (PROD-001)
                if (costoAgendas != null && inventoryAccount != null)
                {
                    var laptopCogsTemplate = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate,
                        Description = "Laptop COGS Template (PROD-001)",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = costoAgendas.Id,
                                EntryType = EntryType.Debit,
                                Amount = 800m, // Cost of laptop
                                AccountName = costoAgendas.AccountName,
                                OfficialCode = costoAgendas.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = inventoryAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 800m,
                                AccountName = inventoryAccount.AccountName,
                                OfficialCode = inventoryAccount.OfficialCode
                            }
                        }
                    };
                    _transactionTemplates["LaptopCOGS"] = laptopCogsTemplate;
                }

                // Template 5: Office Chair COGS (PROD-002)
                if (costoSombrillas != null && inventoryAccount != null)
                {
                    var chairCogsTemplate = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate,
                        Description = "Office Chair COGS Template (PROD-002)",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = costoSombrillas.Id,
                                EntryType = EntryType.Debit,
                                Amount = 150m, // Cost of chair
                                AccountName = costoSombrillas.AccountName,
                                OfficialCode = costoSombrillas.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = inventoryAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 150m,
                                AccountName = inventoryAccount.AccountName,
                                OfficialCode = inventoryAccount.OfficialCode
                            }
                        }
                    };
                    _transactionTemplates["OfficeChairCOGS"] = chairCogsTemplate;
                }

                // Template 6: Mobile Phone COGS (PROD-003)
                if (costoCuadernos != null && inventoryAccount != null)
                {
                    var phoneCogsTemplate = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate,
                        Description = "Mobile Phone COGS Template (PROD-003)",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = costoCuadernos.Id,
                                EntryType = EntryType.Debit,
                                Amount = 500m, // Cost of phone
                                AccountName = costoCuadernos.AccountName,
                                OfficialCode = costoCuadernos.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = inventoryAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 500m,
                                AccountName = inventoryAccount.AccountName,
                                OfficialCode = inventoryAccount.OfficialCode
                            }
                        }
                    };
                    _transactionTemplates["MobilePhoneCOGS"] = phoneCogsTemplate;
                }

                Console.WriteLine($"Created {_transactionTemplates.Count - 5} product-specific transaction templates");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product-specific templates: {ex.Message}");
                // Don't throw - this is supplementary functionality
            }
        }

        #endregion

        #region Step 4: Create Fiscal Period

        /// <summary>
        /// Step 4: Create fiscal period for the year
        /// </summary>
        private void CreateFiscalPeriod()
        {
            try
            {
                // Create annual fiscal period
                var annualPeriod = new FiscalPeriodDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Fiscal Year 2024",
                    Description = "Annual fiscal period for complete flow test",
                    StartDate = _testStartDate,
                    EndDate = _testEndDate,
                    Status = FiscalPeriodStatus.Open,
                    InsertedBy = TEST_USER,
                    UpdatedBy = TEST_USER,
                    InsertedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Create quarterly periods
                var q1Period = new FiscalPeriodDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Q1 2024",
                    Description = "First quarter 2024",
                    StartDate = new DateOnly(2024, 1, 1),
                    EndDate = new DateOnly(2024, 3, 31),
                    Status = FiscalPeriodStatus.Open,
                    InsertedBy = TEST_USER,
                    UpdatedBy = TEST_USER,
                    InsertedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var q2Period = new FiscalPeriodDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Q2 2024",
                    Description = "Second quarter 2024",
                    StartDate = new DateOnly(2024, 4, 1),
                    EndDate = new DateOnly(2024, 6, 30),
                    Status = FiscalPeriodStatus.Open,
                    InsertedBy = TEST_USER,
                    UpdatedBy = TEST_USER,
                    InsertedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var q3Period = new FiscalPeriodDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Q3 2024",
                    Description = "Third quarter 2024",
                    StartDate = new DateOnly(2024, 7, 1),
                    EndDate = new DateOnly(2024, 9, 30),
                    Status = FiscalPeriodStatus.Open,
                    InsertedBy = TEST_USER,
                    UpdatedBy = TEST_USER,
                    InsertedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var q4Period = new FiscalPeriodDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Q4 2024",
                    Description = "Fourth quarter 2024",
                    StartDate = new DateOnly(2024, 10, 1),
                    EndDate = new DateOnly(2024, 12, 31),
                    Status = FiscalPeriodStatus.Open,
                    InsertedBy = TEST_USER,
                    UpdatedBy = TEST_USER,
                    InsertedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add to dictionary
                _fiscalPeriods["Annual"] = annualPeriod;
                _fiscalPeriods["Q1"] = q1Period;
                _fiscalPeriods["Q2"] = q2Period;
                _fiscalPeriods["Q3"] = q3Period;
                _fiscalPeriods["Q4"] = q4Period;

                Console.WriteLine($"Created {_fiscalPeriods.Count} fiscal periods:");
                foreach (var period in _fiscalPeriods.Values)
                {
                    Console.WriteLine($"- {period.Name}: {period.StartDate} to {period.EndDate} ({period.Status})");

                    // Validate each period
                    Assert.That(period.Validate(), Is.True, $"Fiscal period {period.Name} should be valid");
                }

                // Assertions
                Assert.That(_fiscalPeriods.Count, Is.EqualTo(5), "Should have created 5 fiscal periods");
                Assert.That(_fiscalPeriods["Annual"].IsOpen(), Is.True, "Annual period should be open");

                Console.WriteLine("✓ Fiscal periods created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating fiscal periods: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Step 5: Create Items

        /// <summary>
        /// Step 5: Create items (products/services)
        /// </summary>
        private void CreateItems()
        {
            try
            {
                // Create product items
                var product1 = new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "PROD-001",
                    Description = "Laptop Computer",
                    BasePrice = 1200.00m
                };

                var product2 = new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "PROD-002",
                    Description = "Office Chair",
                    BasePrice = 250.00m
                };

                var product3 = new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "PROD-003",
                    Description = "Mobile Phone",
                    BasePrice = 800.00m
                };

                // Create service items
                var service1 = new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "SERV-001",
                    Description = "IT Consulting Service",
                    BasePrice = 150.00m
                };

                var service2 = new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "SERV-002",
                    Description = "Software Installation",
                    BasePrice = 75.00m
                };

                var service3 = new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "SERV-003",
                    Description = "Training Service",
                    BasePrice = 100.00m
                };

                // Add to dictionary
                _items["PROD-001"] = product1;
                _items["PROD-002"] = product2;
                _items["PROD-003"] = product3;
                _items["SERV-001"] = service1;
                _items["SERV-002"] = service2;
                _items["SERV-003"] = service3;

                Console.WriteLine($"Created {_items.Count} items:");
                foreach (var item in _items.Values)
                {
                    Console.WriteLine($"- {item.Code}: {item.Description} (${item.BasePrice:F2})");
                }

                // Assertions
                Assert.That(_items.Count, Is.EqualTo(6), "Should have created 6 items");
                Assert.That(_items.Values.All(i => i.BasePrice > 0), Is.True, "All items should have positive prices");

                Console.WriteLine("✓ Items created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating items: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Step 6: Create Tax Groups

        /// <summary>
        /// Step 6: Create tax groups and taxes
        /// </summary>
        private void CreateTaxGroups()
        {
            try
            {
                // Create tax groups for business entities
                var taxableEntitiesGroup = new TaxGroupDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "TAXABLE_ENTITIES",
                    Name = "Taxable Business Entities",
                    Description = "Business entities subject to standard tax rates",
                    IsEnabled = true
                };

                var exemptEntitiesGroup = new TaxGroupDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "EXEMPT_ENTITIES",
                    Name = "Tax Exempt Entities",
                    Description = "Business entities exempt from taxes",
                    IsEnabled = true
                };

                // Create tax groups for items
                var standardItemsGroup = new TaxGroupDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "STANDARD_ITEMS",
                    Name = "Standard Taxable Items",
                    Description = "Items subject to standard tax rates",
                    IsEnabled = true
                };

                var exemptItemsGroup = new TaxGroupDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "EXEMPT_ITEMS",
                    Name = "Tax Exempt Items",
                    Description = "Items exempt from taxes",
                    IsEnabled = true
                };

                // Add to dictionary
                _taxGroups["TAXABLE_ENTITIES"] = taxableEntitiesGroup;
                _taxGroups["EXEMPT_ENTITIES"] = exemptEntitiesGroup;
                _taxGroups["STANDARD_ITEMS"] = standardItemsGroup;
                _taxGroups["EXEMPT_ITEMS"] = exemptItemsGroup;

                // Create IVA tax (13% for El Salvador)
                var ivaTax = new TaxDto
                {
                    Oid = Guid.NewGuid(),
                    Name = "IVA",
                    Code = "IVA",
                    TaxType = TaxType.Percentage,
                    ApplicationLevel = TaxApplicationLevel.Line,
                    Percentage = 13m,
                    IsEnabled = true,
                    IsIncludedInPrice = false
                };

                // Create retention tax
                var retentionTax = new TaxDto
                {
                    Oid = Guid.NewGuid(),
                    Name = "Income Tax Retention",
                    Code = "RET",
                    TaxType = TaxType.Percentage,
                    ApplicationLevel = TaxApplicationLevel.Document,
                    Percentage = 10m,
                    IsEnabled = true,
                    IsIncludedInPrice = false
                };

                // Add taxes to dictionary
                _taxes["IVA"] = ivaTax;
                _taxes["RET"] = retentionTax;

                // Create tax rules
                var ivaRule = new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Oid,
                    DocumentTypeCode = "CF", // Apply IVA to Credito Fiscal documents
                    BusinessEntityGroupId = taxableEntitiesGroup.Oid,
                    ItemGroupId = standardItemsGroup.Oid,
                    IsEnabled = true,
                    Priority = 1
                };

                var retentionRule = new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = retentionTax.Oid,
                    DocumentTypeCode = "CF",
                    BusinessEntityGroupId = taxableEntitiesGroup.Oid,
                    IsEnabled = true,
                    Priority = 2
                };

                _taxRules.Add(ivaRule);
                _taxRules.Add(retentionRule);

                Console.WriteLine($"Created {_taxGroups.Count} tax groups:");
                foreach (var taxGroup in _taxGroups.Values)
                {
                    Console.WriteLine($"- {taxGroup.Code}: {taxGroup.Name}");
                }

                Console.WriteLine($"Created {_taxes.Count} taxes:");
                foreach (var tax in _taxes.Values)
                {
                    Console.WriteLine($"- {tax.Code}: {tax.Name} ({tax.Percentage}%)");
                }

                Console.WriteLine($"Created {_taxRules.Count} tax rules");

                // Assertions
                Assert.That(_taxGroups.Count, Is.EqualTo(4), "Should have created 4 tax groups");
                Assert.That(_taxes.Count, Is.EqualTo(2), "Should have created 2 taxes");
                Assert.That(_taxRules.Count, Is.EqualTo(2), "Should have created 2 tax rules");

                Console.WriteLine("✓ Tax groups and taxes created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating tax groups: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Step 7: Create Customers

        /// <summary>
        /// Step 7: Create customers (business entities)
        /// </summary>
        private void CreateCustomers()
        {
            try
            {
                // Create taxable customers
                var customer1 = new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Name = "Empresa ABC S.A. de C.V.",
                    Code = "CUST-001"
                };

                var customer2 = new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Name = "Comercial XYZ Ltda.",
                    Code = "CUST-002"
                };

                var customer3 = new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Name = "Servicios Profesionales DEF",
                    Code = "CUST-003"
                };

                // Create non-taxable customers (final consumers)
                var finalConsumer1 = new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Name = "Juan Pérez",
                    Code = "CONS-001"
                };

                var finalConsumer2 = new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Name = "María García",
                    Code = "CONS-002"
                };

                // Add to dictionary
                _customers["CUST-001"] = customer1;
                _customers["CUST-002"] = customer2;
                _customers["CUST-003"] = customer3;
                _customers["CONS-001"] = finalConsumer1;
                _customers["CONS-002"] = finalConsumer2;

                // Create group memberships
                var taxableGroup = _taxGroups["TAXABLE_ENTITIES"];
                var exemptGroup = _taxGroups["EXEMPT_ENTITIES"];

                // Add business customers to taxable group
                _groupMemberships.Add(new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = taxableGroup.Oid,
                    EntityId = customer1.Oid,
                    GroupType = GroupType.BusinessEntity
                });

                _groupMemberships.Add(new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = taxableGroup.Oid,
                    EntityId = customer2.Oid,
                    GroupType = GroupType.BusinessEntity
                });

                _groupMemberships.Add(new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = taxableGroup.Oid,
                    EntityId = customer3.Oid,
                    GroupType = GroupType.BusinessEntity
                });

                // Add final consumers to exempt group
                _groupMemberships.Add(new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = exemptGroup.Oid,
                    EntityId = finalConsumer1.Oid,
                    GroupType = GroupType.BusinessEntity
                });

                _groupMemberships.Add(new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = exemptGroup.Oid,
                    EntityId = finalConsumer2.Oid,
                    GroupType = GroupType.BusinessEntity
                });

                // Create item group memberships
                var standardItemsGroup = _taxGroups["STANDARD_ITEMS"];
                foreach (var item in _items.Values)
                {
                    _groupMemberships.Add(new GroupMembershipDto
                    {
                        Oid = Guid.NewGuid(),
                        GroupId = standardItemsGroup.Oid,
                        EntityId = item.Oid,
                        GroupType = GroupType.Item
                    });
                }

                Console.WriteLine($"Created {_customers.Count} customers:");
                foreach (var customer in _customers.Values)
                {
                    Console.WriteLine($"- {customer.Code}: {customer.Name}");
                }

                Console.WriteLine($"Created {_groupMemberships.Count} group memberships");

                // Assertions
                Assert.That(_customers.Count, Is.EqualTo(5), "Should have created 5 customers");
                Assert.That(_groupMemberships.Count, Is.GreaterThan(5), "Should have created group memberships");

                Console.WriteLine("✓ Customers created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating customers: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Step 8: Create and Execute Tax Transactions

        /// <summary>
        /// Step 8: Create and execute tax-inclusive transactions
        /// </summary>
        private async Task CreateTaxTransactions()
        {
            try
            {
                // Get specific accounts
                var cashAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "11010101"); // CAJA GENERAL
                var salesRevenueAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "51010101"); // VENTA DE PRODUCTO 1
                var clientsAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "11030101"); // CLIENTES NACIONALES
                var ivaDebitoAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "21060101"); // IVA DEBITO FISCAL - CONTRIBUYENTES

                // Log which accounts were found
                Console.WriteLine("Account lookup results for tax transactions:");
                Console.WriteLine($"- Cash account (11010101): {(cashAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Sales revenue account (51010101): {(salesRevenueAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Clients account (11030101): {(clientsAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- IVA Debito account (21060101): {(ivaDebitoAccount != null ? "FOUND" : "NOT FOUND")}");

                if (cashAccount == null || salesRevenueAccount == null || clientsAccount == null)
                {
                    Console.WriteLine("Warning: Could not find required accounts for tax transactions");
                    return;
                }

                // Template 1: Cash Sale with IVA (Cash -> Sales Revenue + IVA)
                var cashSaleWithIva = new TransactionDto
                {
                    Id = Guid.NewGuid(),
                    DocumentId = Guid.NewGuid(),
                    TransactionDate = _testStartDate,
                    Description = "Cash Sale with IVA",
                    LedgerEntries = new List<ILedgerEntry>
                    {
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = cashAccount.Id,
                            EntryType = EntryType.Debit,
                            Amount = 1130m, // $1000 + $130 IVA
                            AccountName = cashAccount.AccountName,
                            OfficialCode = cashAccount.OfficialCode
                        },
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = salesRevenueAccount.Id,
                            EntryType = EntryType.Credit,
                            Amount = 1000m,
                            AccountName = salesRevenueAccount.AccountName,
                            OfficialCode = salesRevenueAccount.OfficialCode
                        },
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = ivaDebitoAccount.Id,
                            EntryType = EntryType.Credit,
                            Amount = 130m, // 13% IVA
                            AccountName = ivaDebitoAccount.AccountName,
                            OfficialCode = ivaDebitoAccount.OfficialCode
                        }
                    }
                };

                // Template 2: Credit Sale with IVA (Accounts Receivable -> Sales Revenue + IVA)
                var creditSaleWithIva = new TransactionDto
                {
                    Id = Guid.NewGuid(),
                    DocumentId = Guid.NewGuid(),
                    TransactionDate = _testStartDate,
                    Description = "Credit Sale with IVA",
                    LedgerEntries = new List<ILedgerEntry>
                    {
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = clientsAccount.Id,
                            EntryType = EntryType.Debit,
                            Amount = 2260m, // $2000 + $260 IVA
                            AccountName = clientsAccount.AccountName,
                            OfficialCode = clientsAccount.OfficialCode
                        },
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = salesRevenueAccount.Id,
                            EntryType = EntryType.Credit,
                            Amount = 2000m,
                            AccountName = salesRevenueAccount.AccountName,
                            OfficialCode = salesRevenueAccount.OfficialCode
                        },
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = ivaDebitoAccount.Id,
                            EntryType = EntryType.Credit,
                            Amount = 260m, // 13% IVA
                            AccountName = ivaDebitoAccount.AccountName,
                            OfficialCode = ivaDebitoAccount.OfficialCode
                        }
                    }
                };

                // Add transactions to be executed
                var transactionsToExecute = new List<ITransaction>
                {
                    cashSaleWithIva,
                    creditSaleWithIva
                };                // Create transactions (for demonstration purposes)
                foreach (var transaction in transactionsToExecute)
                {
                    await _transactionService.CreateTransactionAsync(transaction);
                    Console.WriteLine($"Created transaction: {transaction.Description}");
                }

                Console.WriteLine($"Created and executed {transactionsToExecute.Count} tax transactions");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating or executing tax transactions: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Step 9: Get Trial Balance        /// <summary>
        /// Step 9: Generate trial balance report
        /// </summary>
        private void GetTrialBalance()
        {
            try
            {                // Create some sample transactions to have data for trial balance
                CreateSampleTransactions();

                // Calculate trial balance manually using our account balance calculator
                var trialBalanceEntries = new List<TrialBalanceEntry>();
                decimal totalDebits = 0m;
                decimal totalCredits = 0m;

                foreach (var account in _accounts.Values.OrderBy(a => a.OfficialCode))
                {
                    // Calculate balance for this account as of test end date
                    decimal balance = _accountBalanceCalculator.CalculateAccountBalance(account.Id, _testEndDate); if (Math.Abs(balance) > 0.01m) // Only include accounts with meaningful balances
                    {
                        var entry = new TrialBalanceEntry
                        {
                            AccountId = account.Id,
                            AccountCode = account.OfficialCode,
                            AccountName = account.AccountName,
                            AccountType = account.AccountType
                        };

                        // Determine debit or credit balance based on account type and balance
                        if (account.HasDebitBalance())
                        {
                            if (balance >= 0)
                            {
                                entry.DebitBalance = balance;
                                totalDebits += balance;
                            }
                            else
                            {
                                entry.CreditBalance = Math.Abs(balance);
                                totalCredits += Math.Abs(balance);
                            }
                        }
                        else
                        {
                            if (balance <= 0)
                            {
                                entry.CreditBalance = Math.Abs(balance);
                                totalCredits += Math.Abs(balance);
                            }
                            else
                            {
                                entry.DebitBalance = balance;
                                totalDebits += balance;
                            }
                        }

                        trialBalanceEntries.Add(entry);
                    }
                }                // Display trial balance in cascade format
                Console.WriteLine($"\n=== TRIAL BALANCE (CASCADE FORMAT) as of {_testEndDate:yyyy-MM-dd} ===");
                DisplayTrialBalanceInCascadeFormat(trialBalanceEntries, totalDebits, totalCredits);

                // Verify trial balance is balanced
                decimal difference = Math.Abs(totalDebits - totalCredits);
                Console.WriteLine($"\nTrial Balance Verification:");
                Console.WriteLine($"Total Debits:  ${totalDebits:F2}");
                Console.WriteLine($"Total Credits: ${totalCredits:F2}");
                Console.WriteLine($"Difference:    ${difference:F2}");

                // Group by account type for summary
                var summary = trialBalanceEntries
                    .GroupBy(e => e.AccountType)
                    .Select(g => new
                    {
                        AccountType = g.Key,
                        DebitTotal = g.Sum(e => e.DebitBalance),
                        CreditTotal = g.Sum(e => e.CreditBalance),
                        Count = g.Count()
                    })
                    .OrderBy(s => s.AccountType);

                Console.WriteLine($"\n=== TRIAL BALANCE SUMMARY BY ACCOUNT TYPE ===");
                foreach (var item in summary)
                {
                    Console.WriteLine($"{item.AccountType}: {item.Count} accounts, " +
                                    $"Debits: ${item.DebitTotal:F2}, Credits: ${item.CreditTotal:F2}");
                }

                // Assertions
                Assert.That(trialBalanceEntries.Count, Is.GreaterThan(0), "Should have trial balance entries");
                Assert.That(difference, Is.LessThan(1.00m), "Trial balance should be balanced (difference < $1.00)");

                Console.WriteLine("✓ Trial balance generated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating trial balance: {ex.Message}");
                throw;
            }
        }        /// <summary>
                 /// Create some sample transactions to populate the trial balance
                 /// </summary>
        private void CreateSampleTransactions()
        {
            try
            {
                var transactions = new List<ITransaction>();

                // Get specific accounts using their official codes from the CSV file (same as in CreateTransactionTemplates)
                var cashAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "11010101"); // CAJA GENERAL
                var salesAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "51010101"); // VENTA DE PRODUCTO 1
                var equityAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "31010101"); // CAPITAL SOCIAL PAGADO

                Console.WriteLine("Sample transactions account lookup:");
                Console.WriteLine($"- Cash account (11010101): {(cashAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Sales account (51010101): {(salesAccount != null ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"- Equity account (31010101): {(equityAccount != null ? "FOUND" : "NOT FOUND")}");

                if (cashAccount == null || salesAccount == null || equityAccount == null)
                {
                    Console.WriteLine("Warning: Could not find required accounts for sample transactions");
                    return;
                }

                // Transaction 1: Initial capital investment
                var capitalTransaction = new TransactionDto
                {
                    Id = Guid.NewGuid(),
                    DocumentId = Guid.NewGuid(),
                    TransactionDate = _testStartDate,
                    Description = "Initial capital investment",
                    LedgerEntries = new List<ILedgerEntry>
                    {
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = cashAccount.Id,
                            EntryType = EntryType.Debit,
                            Amount = 10000.00m,
                            AccountName = cashAccount.AccountName,
                            OfficialCode = cashAccount.OfficialCode
                        },
                        new LedgerEntryDto
                        {
                            Id = Guid.NewGuid(),
                            AccountId = equityAccount.Id,
                            EntryType = EntryType.Credit,
                            Amount = 10000.00m,
                            AccountName = equityAccount.AccountName,
                            OfficialCode = equityAccount.OfficialCode
                        }
                    }
                };                // Transaction 2: Cash Sale with IVA (showing tax collected)
                ITransaction salesTransaction;
                var ivaDebitoAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "21060101"); // IVA DEBITO FISCAL - CONTRIBUYENTES

                if (ivaDebitoAccount != null)
                {
                    salesTransaction = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate.AddDays(15),
                        Description = "Cash Sale with IVA - El Salvador Tax Demo",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = cashAccount.Id,
                                EntryType = EntryType.Debit,
                                Amount = 2825.00m, // $2500 + $325 IVA (13%)
                                AccountName = cashAccount.AccountName,
                                OfficialCode = cashAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = salesAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 2500.00m,
                                AccountName = salesAccount.AccountName,
                                OfficialCode = salesAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = ivaDebitoAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 325.00m, // 13% El Salvador IVA
                                AccountName = ivaDebitoAccount.AccountName,
                                OfficialCode = ivaDebitoAccount.OfficialCode
                            }
                        }
                    };
                }
                else
                {
                    // Fallback to simple sales transaction if IVA account not found
                    salesTransaction = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate.AddDays(15),
                        Description = "Cash Sale (no tax account found)",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = cashAccount.Id,
                                EntryType = EntryType.Debit,
                                Amount = 2500.00m,
                                AccountName = cashAccount.AccountName,
                                OfficialCode = cashAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = salesAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 2500.00m,
                                AccountName = salesAccount.AccountName,
                                OfficialCode = salesAccount.OfficialCode
                            }
                        }
                    };
                }
                transactions.Add(capitalTransaction);
                transactions.Add(salesTransaction);

                // Transaction 3: Purchase with IVA Credit (if accounts available)
                var inventoryAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "11050102"); // INVENTARIO DE PRODUCTO TERMINADO
                var providersAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "21020101"); // PROVEEDORES LOCALES
                var ivaCreditoAccount = _accounts.Values.FirstOrDefault(a => a.OfficialCode == "11070101"); // CREDITO FISCAL- COMPRA LOCAL

                if (inventoryAccount != null && providersAccount != null && ivaCreditoAccount != null)
                {
                    var purchaseTransaction = new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        TransactionDate = _testStartDate.AddDays(30),
                        Description = "Inventory Purchase with IVA Credit",
                        LedgerEntries = new List<ILedgerEntry>
                        {
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = inventoryAccount.Id,
                                EntryType = EntryType.Debit,
                                Amount = 1000.00m,
                                AccountName = inventoryAccount.AccountName,
                                OfficialCode = inventoryAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = ivaCreditoAccount.Id,
                                EntryType = EntryType.Debit,
                                Amount = 130.00m, // 13% IVA Credit
                                AccountName = ivaCreditoAccount.AccountName,
                                OfficialCode = ivaCreditoAccount.OfficialCode
                            },
                            new LedgerEntryDto
                            {
                                Id = Guid.NewGuid(),
                                AccountId = providersAccount.Id,
                                EntryType = EntryType.Credit,
                                Amount = 1130.00m, // $1000 + $130 IVA
                                AccountName = providersAccount.AccountName,
                                OfficialCode = providersAccount.OfficialCode
                            }
                        }
                    };
                    transactions.Add(purchaseTransaction);
                    Console.WriteLine("Added purchase transaction with IVA Credit to demonstrate tax accounts");
                }

                // Update the account balance calculator with our transactions
                _accountBalanceCalculator = new AccountBalanceCalculatorBase(transactions);

                Console.WriteLine($"Created {transactions.Count} sample transactions for trial balance (including tax accounts)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating sample transactions: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Display trial balance in cascade (hierarchical) format grouped by account type and sorted by account code
        /// </summary>
        private void DisplayTrialBalanceInCascadeFormat(List<TrialBalanceEntry> trialBalanceEntries, decimal totalDebits, decimal totalCredits)
        {
            // Group by account type and order accounts within each group hierarchically
            var groupedEntries = trialBalanceEntries
                .GroupBy(e => e.AccountType)
                .OrderBy(g => GetAccountTypeOrder(g.Key))
                .ToList();

            Console.WriteLine($"{"Account Structure",-60} {"Debit",-15} {"Credit",-15}");
            Console.WriteLine(new string('=', 90));

            decimal typeDebitsTotal = 0m;
            decimal typeCreditsTotal = 0m;

            foreach (var accountTypeGroup in groupedEntries)
            {
                var typeDebits = accountTypeGroup.Sum(e => e.DebitBalance);
                var typeCredits = accountTypeGroup.Sum(e => e.CreditBalance);
                typeDebitsTotal += typeDebits;
                typeCreditsTotal += typeCredits;

                // Display account type header
                Console.WriteLine($"\n{accountTypeGroup.Key.ToString().ToUpper(),-60} {typeDebits,-15:F2} {typeCredits,-15:F2}");
                Console.WriteLine(new string('-', 90));

                // Group accounts by major category (first 2 digits) for cascade effect
                var accountsByMajorCategory = accountTypeGroup
                    .GroupBy(e => e.AccountCode.Length >= 2 ? e.AccountCode.Substring(0, 2) : e.AccountCode)
                    .OrderBy(g => g.Key)
                    .ToList();

                foreach (var majorCategoryGroup in accountsByMajorCategory)
                {
                    var categoryDebits = majorCategoryGroup.Sum(e => e.DebitBalance);
                    var categoryCredits = majorCategoryGroup.Sum(e => e.CreditBalance);

                    // Find a representative account to get the major category name
                    var representativeAccount = majorCategoryGroup.First();
                    var majorCategoryName = GetMajorCategoryName(representativeAccount.AccountCode, representativeAccount.AccountType);

                    // Display major category subtotal (if more than one account in this category)
                    if (majorCategoryGroup.Count() > 1)
                    {
                        Console.WriteLine($"  {majorCategoryName,-58} {categoryDebits,-15:F2} {categoryCredits,-15:F2}");
                    }

                    // Group by subcategory (first 4 digits) for further cascade
                    var accountsBySubCategory = majorCategoryGroup
                        .GroupBy(e => e.AccountCode.Length >= 4 ? e.AccountCode.Substring(0, 4) : e.AccountCode)
                        .OrderBy(g => g.Key)
                        .ToList();

                    foreach (var subCategoryGroup in accountsBySubCategory)
                    {
                        var subCategoryDebits = subCategoryGroup.Sum(e => e.DebitBalance);
                        var subCategoryCredits = subCategoryGroup.Sum(e => e.CreditBalance);

                        // Display subcategory subtotal (if more than one account and major category has multiple subcategories)
                        if (subCategoryGroup.Count() > 1 && accountsBySubCategory.Count > 1)
                        {
                            var subCategoryName = GetSubCategoryName(subCategoryGroup.First().AccountCode, subCategoryGroup.First().AccountType);
                            Console.WriteLine($"    {subCategoryName,-56} {subCategoryDebits,-15:F2} {subCategoryCredits,-15:F2}");
                        }                        // Display individual accounts
                        foreach (var entry in subCategoryGroup.OrderBy(e => e.AccountCode))
                        {
                            var indent = majorCategoryGroup.Count() > 1 && subCategoryGroup.Count() > 1 ? "      " :
                                        majorCategoryGroup.Count() > 1 ? "    " : "  ";
                            var displayName = $"{entry.AccountCode} - {entry.AccountName}";
                            var fullDisplayName = $"{indent}{displayName}";

                            Console.WriteLine($"{fullDisplayName,-60} " +
                                            $"{(entry.DebitBalance > 0 ? entry.DebitBalance.ToString("F2") : ""),-15} " +
                                            $"{(entry.CreditBalance > 0 ? entry.CreditBalance.ToString("F2") : ""),-15}");
                        }
                    }
                }
            }

            Console.WriteLine(new string('=', 90));
            Console.WriteLine($"{"GRAND TOTALS",-60} {totalDebits,-15:F2} {totalCredits,-15:F2}");
            Console.WriteLine(new string('=', 90));

            // Balance verification
            decimal difference = Math.Abs(totalDebits - totalCredits);
            Console.WriteLine($"\nBalance Verification:");
            Console.WriteLine($"Total Debits:  ${totalDebits:F2}");
            Console.WriteLine($"Total Credits: ${totalCredits:F2}");
            Console.WriteLine($"Difference:    ${difference:F2} {(difference < 0.01m ? "✓ BALANCED" : "⚠ NOT BALANCED")}");

            // Account Balance Calculator Verification
            Console.WriteLine($"\n=== ACCOUNT BALANCE CALCULATOR VERIFICATION ===");
            Console.WriteLine($"Accounts with balances: {trialBalanceEntries.Count}");
            Console.WriteLine($"Total accounts imported: {_accounts.Count}");
            Console.WriteLine($"Accounts with zero balances: {_accounts.Count - trialBalanceEntries.Count}");
            // Sample detailed verification for key accounts
            Console.WriteLine($"\nDetailed verification for key accounts:");
            var keyAccounts = new[] { "11010101", "51010101", "31010101" }; // Cash, Sales, Equity
            foreach (var accountCode in keyAccounts)
            {
                var account = _accounts.Values.FirstOrDefault(a => a.OfficialCode == accountCode);
                if (account != null)
                {
                    var calculatedBalance = _accountBalanceCalculator.CalculateAccountBalance(account.Id, _testEndDate);
                    var trialBalanceEntry = trialBalanceEntries.FirstOrDefault(e => e.AccountCode == accountCode);

                    // Determine expected trial balance representation
                    bool isDebitAccount = account.HasDebitBalance();
                    decimal expectedTrialBalanceAmount;
                    string balanceType;

                    if (isDebitAccount)
                    {
                        // For debit accounts: positive calculator balance = debit, negative = credit
                        expectedTrialBalanceAmount = calculatedBalance >= 0 ? calculatedBalance : Math.Abs(calculatedBalance);
                        balanceType = calculatedBalance >= 0 ? "(Debit)" : "(Credit)";
                    }
                    else
                    {
                        // For credit accounts: negative calculator balance = credit, positive = debit
                        expectedTrialBalanceAmount = calculatedBalance <= 0 ? Math.Abs(calculatedBalance) : calculatedBalance;
                        balanceType = calculatedBalance <= 0 ? "(Credit)" : "(Debit)";
                    }
                    var actualTrialBalanceAmount = trialBalanceEntry?.DebitBalance > 0 ? trialBalanceEntry.DebitBalance :
                                                trialBalanceEntry?.CreditBalance > 0 ? trialBalanceEntry.CreditBalance : 0m;
                    var actualBalanceType = trialBalanceEntry?.DebitBalance > 0 ? "(Debit)" : "(Credit)";

                    bool isMatch = Math.Abs(expectedTrialBalanceAmount - actualTrialBalanceAmount) < 0.01m;

                    Console.WriteLine($"  {accountCode} - {account.AccountName}:");
                    Console.WriteLine($"    Calculator balance: ${calculatedBalance:F2}");
                    Console.WriteLine($"    Expected in trial balance: ${expectedTrialBalanceAmount:F2} {balanceType}");
                    Console.WriteLine($"    Actual in trial balance: ${actualTrialBalanceAmount:F2} {actualBalanceType}");
                    Console.WriteLine($"    Account type: {account.AccountType} ({(isDebitAccount ? "Debit Normal" : "Credit Normal")})");
                    Console.WriteLine($"    Verification: {(isMatch ? "✓ MATCH" : "⚠ MISMATCH")}");
                }
            }
        }

        /// <summary>
        /// Get the sort order for account types to display them in logical order
        /// </summary>
        private int GetAccountTypeOrder(AccountType accountType)
        {
            return accountType switch
            {
                AccountType.Asset => 1,
                AccountType.Liability => 2,
                AccountType.Equity => 3,
                AccountType.Revenue => 4,
                AccountType.Expense => 5,
                _ => 6
            };
        }

        /// <summary>
        /// Get a descriptive name for major account categories based on the first 2 digits
        /// </summary>
        private string GetMajorCategoryName(string accountCode, AccountType accountType)
        {
            if (accountCode.Length < 2) return "Unknown Category";

            var firstTwoDigits = accountCode.Substring(0, 2);

            return accountType switch
            {
                AccountType.Asset => firstTwoDigits switch
                {
                    "11" => "Current Assets",
                    "12" => "Fixed Assets",
                    "13" => "Intangible Assets",
                    "14" => "Other Assets",
                    _ => $"Assets ({firstTwoDigits})"
                },
                AccountType.Liability => firstTwoDigits switch
                {
                    "21" => "Current Liabilities",
                    "22" => "Long-term Liabilities",
                    _ => $"Liabilities ({firstTwoDigits})"
                },
                AccountType.Equity => firstTwoDigits switch
                {
                    "31" => "Capital",
                    "32" => "Retained Earnings",
                    "33" => "Other Equity",
                    _ => $"Equity ({firstTwoDigits})"
                },
                AccountType.Revenue => firstTwoDigits switch
                {
                    "51" => "Operating Revenue",
                    "52" => "Other Revenue",
                    _ => $"Revenue ({firstTwoDigits})"
                },
                AccountType.Expense => firstTwoDigits switch
                {
                    "41" => "Cost of Sales",
                    "42" => "Operating Expenses",
                    "43" => "Administrative Expenses",
                    "44" => "Financial Expenses",
                    _ => $"Expenses ({firstTwoDigits})"
                },
                _ => $"Category ({firstTwoDigits})"
            };
        }

        /// <summary>
        /// Get a descriptive name for subcategories based on the first 4 digits
        /// </summary>
        private string GetSubCategoryName(string accountCode, AccountType accountType)
        {
            if (accountCode.Length < 4) return GetMajorCategoryName(accountCode, accountType);

            var firstFourDigits = accountCode.Substring(0, 4);

            return accountType switch
            {
                AccountType.Asset => firstFourDigits switch
                {
                    "1101" => "Cash & Banks",
                    "1102" => "Investments",
                    "1103" => "Accounts Receivable",
                    "1104" => "Other Receivables",
                    "1105" => "Inventory",
                    "1106" => "Prepaid Expenses",
                    "1107" => "Tax Assets",
                    _ => $"Assets ({firstFourDigits})"
                },
                AccountType.Liability => firstFourDigits switch
                {
                    "2101" => "Accounts Payable",
                    "2102" => "Providers",
                    "2103" => "Accrued Expenses",
                    "2104" => "Deferred Revenue",
                    "2105" => "Tax Liabilities",
                    _ => $"Liabilities ({firstFourDigits})"
                },
                AccountType.Equity => firstFourDigits switch
                {
                    "3101" => "Capital Contributions",
                    "3102" => "Retained Earnings",
                    "3103" => "Other Comprehensive Income",
                    _ => $"Equity ({firstFourDigits})"
                },
                AccountType.Revenue => firstFourDigits switch
                {
                    "5101" => "Product Sales",
                    "5102" => "Service Income",
                    "5103" => "Other Revenue",
                    _ => $"Revenue ({firstFourDigits})"
                },
                AccountType.Expense => firstFourDigits switch
                {
                    "4101" => "Cost of Goods Sold",
                    "4102" => "Selling Expenses",
                    "4103" => "Administrative Expenses",
                    "4104" => "Financial Expenses",
                    "4105" => "Tax Expenses",
                    _ => $"Expenses ({firstFourDigits})"
                },
                _ => $"Subcategory ({firstFourDigits})"
            };
        }

        #endregion
    }

    /// <summary>
    /// Represents a trial balance entry for reporting
    /// </summary>
    public class TrialBalanceEntry
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
    }
}