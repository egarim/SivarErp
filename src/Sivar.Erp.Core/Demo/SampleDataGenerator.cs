using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Configuration;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.DataImport;
using System.ComponentModel;
using System.Diagnostics;

namespace Sivar.Erp.Core.Demo
{
    /// <summary>
    /// Enhanced comprehensive sample data generator for demos and testing with multi-data set support
    /// </summary>
    [Description("Enhanced comprehensive sample data generator for demos and testing")]
    public class SampleDataGenerator : ISampleDataGenerator
    {
        private readonly ILogger<SampleDataGenerator> _logger;
        private readonly DemoOptions _options;
        private readonly IDataImportService _dataImportService;

        /// <summary>
        /// Initializes a new instance of the SampleDataGenerator
        /// </summary>
        /// <param name="logger">Logger for diagnostic information</param>
        /// <param name="options">Demo configuration options</param>
        /// <param name="dataImportService">Data import service for CSV processing</param>
        public SampleDataGenerator(
            ILogger<SampleDataGenerator> logger,
            IOptions<DemoOptions> options,
            IDataImportService dataImportService)
        {
            _logger = logger;
            _options = options.Value;
            _dataImportService = dataImportService;
        }

        /// <inheritdoc/>
        public async Task GenerateSampleDataAsync(IRepository repository)
        {
            _logger.LogInformation("Starting enhanced sample data generation for {TestDataSet} with advanced features", _options.TestDataSet);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Phase 1: Import embedded CSV data using enhanced DataImportService
                await ImportEmbeddedDataSetAsync(repository);

                // Phase 2: Generate additional scenario-specific data
                if (_options.IncludeTestTransactions)
                {
                    await GenerateAdvancedTestTransactionsAsync(repository);
                }

                if (_options.IncludeTestDocuments)
                {
                    await GenerateAdvancedTestDocumentsAsync(repository);
                }

                // Phase 3: Generate additional demo scenarios based on data set
                await GenerateDataSetSpecificScenariosAsync(repository);

                await repository.CommitChanges();
                
                stopwatch.Stop();
                var stats = repository.GetStatistics();
                var totalRecords = stats.Values.Sum();
                
                _logger.LogInformation("Enhanced sample data generation completed successfully in {Duration}ms. Generated {TotalRecords} total records across {EntityTypes} entity types", 
                    stopwatch.ElapsedMilliseconds, totalRecords, stats.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate enhanced sample data, attempting fallback");
                repository.Rollback();
                
                try
                {
                    await GenerateEnhancedFallbackDataAsync(repository);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Enhanced fallback sample data generation also failed");
                    throw;
                }
            }
        }

        /// <summary>
        /// Imports embedded data set using the enhanced DataImportService
        /// </summary>
        /// <param name="repository">Repository for data storage</param>
        private async Task ImportEmbeddedDataSetAsync(IRepository repository)
        {
            _logger.LogInformation("Importing embedded data set: {DataSet}", _options.TestDataSet);

            try
            {
                // Use the enhanced DataImportService to import all embedded CSV data
                var importResult = await _dataImportService.ImportTestDataSetAsync(_options.TestDataSet, "SampleDataGenerator");

                if (importResult.Success)
                {
                    _logger.LogInformation("Successfully imported test data set {DataSet}: {Summary}", 
                        _options.TestDataSet, importResult.Summary);
                }
                else
                {
                    _logger.LogWarning("Failed to import test data set {DataSet}: {Summary}. Will use fallback data generation.", 
                        _options.TestDataSet, importResult.Summary);
                    
                    // If CSV import fails, generate basic data
                    await GenerateBasicDataSetAsync(repository);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception during embedded data import for {DataSet}. Using fallback generation.", _options.TestDataSet);
                await GenerateBasicDataSetAsync(repository);
            }
        }

        /// <summary>
        /// Generates data set specific scenarios based on the configured test data set
        /// </summary>
        /// <param name="repository">Repository for data storage</param>
        private async Task GenerateDataSetSpecificScenariosAsync(IRepository repository)
        {
            _logger.LogDebug("Generating data set specific scenarios for: {DataSet}", _options.TestDataSet);

            switch (_options.TestDataSet.ToLowerInvariant())
            {
                case "elsalvador":
                    await GenerateElSalvadorSpecificScenariosAsync(repository);
                    break;
                case "usa":
                    await GenerateUSASpecificScenariosAsync(repository);
                    break;
                case "manufacturing":
                    await GenerateManufacturingSpecificScenariosAsync(repository);
                    break;
                case "service":
                    await GenerateServiceSpecificScenariosAsync(repository);
                    break;
                case "retail":
                    await GenerateRetailSpecificScenariosAsync(repository);
                    break;
                default:
                    await GenerateGenericScenariosAsync(repository);
                    break;
            }
        }

        /// <summary>
        /// Generates El Salvador specific demo scenarios
        /// </summary>
        private async Task GenerateElSalvadorSpecificScenariosAsync(IRepository repository)
        {
            _logger.LogDebug("Generating El Salvador specific scenarios");

            // El Salvador specific business scenarios
            await GenerateElSalvadorTaxScenariosAsync(repository);
            await GenerateElSalvadorBusinessEntitiesAsync(repository);
            await GenerateElSalvadorDocumentTypesAsync(repository);
        }

        /// <summary>
        /// Generates USA specific demo scenarios
        /// </summary>
        private async Task GenerateUSASpecificScenariosAsync(IRepository repository)
        {
            _logger.LogDebug("Generating USA specific scenarios");

            // USA specific tax scenarios (Sales Tax, State Tax)
            await GenerateUSATaxScenariosAsync(repository);
            await GenerateUSABusinessEntitiesAsync(repository);
        }

        /// <summary>
        /// Generates manufacturing specific demo scenarios
        /// </summary>
        private async Task GenerateManufacturingSpecificScenariosAsync(IRepository repository)
        {
            _logger.LogDebug("Generating Manufacturing specific scenarios");

            await GenerateManufacturingAccountsAsync(repository);
            await GenerateManufacturingItemsAsync(repository);
            await GenerateManufacturingWorkflowsAsync(repository);
        }

        /// <summary>
        /// Generates service business specific demo scenarios
        /// </summary>
        private async Task GenerateServiceSpecificScenariosAsync(IRepository repository)
        {
            _logger.LogDebug("Generating Service business specific scenarios");

            await GenerateServiceAccountsAsync(repository);
            await GenerateServiceContractsAsync(repository);
        }

        /// <summary>
        /// Generates retail specific demo scenarios
        /// </summary>
        private async Task GenerateRetailSpecificScenariosAsync(IRepository repository)
        {
            _logger.LogDebug("Generating Retail specific scenarios");

            await GenerateRetailAccountsAsync(repository);
            await GenerateRetailInventoryAsync(repository);
            await GenerateRetailPOSTransactionsAsync(repository);
        }

        /// <summary>
        /// Generates generic business scenarios
        /// </summary>
        private async Task GenerateGenericScenariosAsync(IRepository repository)
        {
            _logger.LogDebug("Generating generic business scenarios");

            await GenerateGenericAccountsAsync(repository);
            await GenerateGenericBusinessEntitiesAsync(repository);
        }

        /// <summary>
        /// Generates enhanced test transactions with realistic scenarios
        /// </summary>
        private async Task GenerateAdvancedTestTransactionsAsync(IRepository repository)
        {
            _logger.LogDebug("Generating advanced test transactions");

            var accounts = repository.GetObjects<AccountDto>().ToList();
            if (accounts.Count < 4) return;

            var transactionCount = Math.Min(_options.MaxSampleRecords / 10, 50); // Up to 50 transactions
            
            for (int i = 1; i <= transactionCount; i++)
            {
                await GenerateRealisticTransactionScenarioAsync(repository, i, accounts);
            }

            _logger.LogDebug("Generated {TransactionCount} advanced test transactions", transactionCount);
        }

        /// <summary>
        /// Generates enhanced test documents with complete workflow scenarios
        /// </summary>
        private async Task GenerateAdvancedTestDocumentsAsync(IRepository repository)
        {
            _logger.LogDebug("Generating advanced test documents");

            var documentTypes = repository.GetObjects<DocumentTypeDto>().ToList();
            var businessEntities = repository.GetObjects<BusinessEntityDto>().ToList();
            var items = repository.GetObjects<ItemDto>().ToList();

            if (!documentTypes.Any() || !businessEntities.Any()) return;

            var documentCount = Math.Min(_options.MaxSampleRecords / 20, 25); // Up to 25 documents

            for (int i = 1; i <= documentCount; i++)
            {
                await GenerateRealisticDocumentScenarioAsync(repository, i, documentTypes, businessEntities, items);
            }

            _logger.LogDebug("Generated {DocumentCount} advanced test documents", documentCount);
        }

        /// <summary>
        /// Generates enhanced fallback data when CSV import fails
        /// </summary>
        private async Task GenerateEnhancedFallbackDataAsync(IRepository repository)
        {
            _logger.LogInformation("Generating enhanced fallback sample data");

            try
            {
                // Create comprehensive chart of accounts
                await CreateComprehensiveChartOfAccountsAsync(repository);
                
                // Create advanced tax configurations
                await CreateAdvancedTaxConfigurationsAsync(repository);
                
                // Create diverse business entities
                await CreateDiverseBusinessEntitiesAsync(repository);
                
                // Create comprehensive document types
                await CreateComprehensiveDocumentTypesAsync(repository);

                // Create sample items if needed
                await CreateSampleItemsAsync(repository);

                if (_options.IncludeTestTransactions)
                {
                    await GenerateAdvancedTestTransactionsAsync(repository);
                }

                if (_options.IncludeTestDocuments)
                {
                    await GenerateAdvancedTestDocumentsAsync(repository);
                }

                await repository.CommitChanges();
                _logger.LogInformation("Enhanced fallback sample data generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate enhanced fallback sample data");
                repository.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Generates basic data set when import fails
        /// </summary>
        private async Task GenerateBasicDataSetAsync(IRepository repository)
        {
            _logger.LogDebug("Generating basic data set as fallback");

            await CreateBasicChartOfAccountsAsync(repository);
            await CreateBasicTaxesAsync(repository);
            await CreateBasicBusinessEntitiesAsync(repository);
            await CreateBasicDocumentTypesAsync(repository);
        }

        #region Enhanced Entity Creation Methods

        /// <summary>
        /// Creates a comprehensive chart of accounts
        /// </summary>
        private async Task CreateComprehensiveChartOfAccountsAsync(IRepository repository)
        {
            _logger.LogDebug("Creating comprehensive chart of accounts");

            var accounts = new[]
            {
                // Assets
                new { Code = "1000", Name = "Cash", Type = AccountType.Asset, ParentCode = "" },
                new { Code = "1010", Name = "Checking Account", Type = AccountType.Asset, ParentCode = "1000" },
                new { Code = "1020", Name = "Savings Account", Type = AccountType.Asset, ParentCode = "1000" },
                new { Code = "1200", Name = "Accounts Receivable", Type = AccountType.Asset, ParentCode = "" },
                new { Code = "1210", Name = "Trade Receivables", Type = AccountType.Asset, ParentCode = "1200" },
                new { Code = "1220", Name = "Employee Advances", Type = AccountType.Asset, ParentCode = "1200" },
                new { Code = "1300", Name = "Inventory", Type = AccountType.Asset, ParentCode = "" },
                new { Code = "1310", Name = "Raw Materials", Type = AccountType.Asset, ParentCode = "1300" },
                new { Code = "1320", Name = "Work in Progress", Type = AccountType.Asset, ParentCode = "1300" },
                new { Code = "1330", Name = "Finished Goods", Type = AccountType.Asset, ParentCode = "1300" },
                new { Code = "1500", Name = "Fixed Assets", Type = AccountType.Asset, ParentCode = "" },
                new { Code = "1510", Name = "Equipment", Type = AccountType.Asset, ParentCode = "1500" },
                new { Code = "1520", Name = "Accumulated Depreciation", Type = AccountType.Asset, ParentCode = "1500" },

                // Liabilities
                new { Code = "2000", Name = "Accounts Payable", Type = AccountType.Liability, ParentCode = "" },
                new { Code = "2010", Name = "Trade Payables", Type = AccountType.Liability, ParentCode = "2000" },
                new { Code = "2020", Name = "Accrued Expenses", Type = AccountType.Liability, ParentCode = "2000" },
                new { Code = "2100", Name = "Taxes Payable", Type = AccountType.Liability, ParentCode = "" },
                new { Code = "2110", Name = "VAT Payable", Type = AccountType.Liability, ParentCode = "2100" },
                new { Code = "2120", Name = "Income Tax Payable", Type = AccountType.Liability, ParentCode = "2100" },
                new { Code = "2200", Name = "Long-term Debt", Type = AccountType.Liability, ParentCode = "" },

                // Equity
                new { Code = "3000", Name = "Owner's Equity", Type = AccountType.Equity, ParentCode = "" },
                new { Code = "3010", Name = "Capital", Type = AccountType.Equity, ParentCode = "3000" },
                new { Code = "3020", Name = "Retained Earnings", Type = AccountType.Equity, ParentCode = "3000" },

                // Revenue
                new { Code = "4000", Name = "Sales Revenue", Type = AccountType.Revenue, ParentCode = "" },
                new { Code = "4010", Name = "Product Sales", Type = AccountType.Revenue, ParentCode = "4000" },
                new { Code = "4020", Name = "Service Revenue", Type = AccountType.Revenue, ParentCode = "4000" },
                new { Code = "4100", Name = "Other Income", Type = AccountType.Revenue, ParentCode = "" },

                // Expenses
                new { Code = "5000", Name = "Cost of Goods Sold", Type = AccountType.Expense, ParentCode = "" },
                new { Code = "5010", Name = "Materials Cost", Type = AccountType.Expense, ParentCode = "5000" },
                new { Code = "5020", Name = "Labor Cost", Type = AccountType.Expense, ParentCode = "5000" },
                new { Code = "6000", Name = "Operating Expenses", Type = AccountType.Expense, ParentCode = "" },
                new { Code = "6010", Name = "Salaries", Type = AccountType.Expense, ParentCode = "6000" },
                new { Code = "6020", Name = "Rent", Type = AccountType.Expense, ParentCode = "6000" },
                new { Code = "6030", Name = "Utilities", Type = AccountType.Expense, ParentCode = "6000" },
                new { Code = "6040", Name = "Marketing", Type = AccountType.Expense, ParentCode = "6000" },
                new { Code = "6100", Name = "Administrative Expenses", Type = AccountType.Expense, ParentCode = "" }
            };

            foreach (var accountDef in accounts)
            {
                var account = repository.CreateObject<AccountDto>();
                account.Id = Guid.NewGuid();
                account.CreatedAt = DateTime.UtcNow;
                account.UpdatedAt = DateTime.UtcNow;
                account.OfficialCode = accountDef.Code;
                account.AccountName = accountDef.Name;
                account.AccountType = accountDef.Type;
                account.IsActive = true;
                account.Balance = 0m;
            }

            _logger.LogDebug("Created {AccountCount} comprehensive accounts", accounts.Length);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates advanced tax configurations
        /// </summary>
        private async Task CreateAdvancedTaxConfigurationsAsync(IRepository repository)
        {
            _logger.LogDebug("Creating advanced tax configurations");

            var taxes = new[]
            {
                new { Code = "VAT13", Name = "Value Added Tax 13%", Rate = 13.00m, Type = TaxType.VAT },
                new { Code = "VAT0", Name = "Value Added Tax 0%", Rate = 0.00m, Type = TaxType.VAT },
                new { Code = "SALES", Name = "Sales Tax", Rate = 8.50m, Type = TaxType.Sales },
                new { Code = "WITH_RENT", Name = "Withholding Tax - Rent", Rate = 10.00m, Type = TaxType.Withholding },
                new { Code = "WITH_SERV", Name = "Withholding Tax - Services", Rate = 5.00m, Type = TaxType.Withholding },
                new { Code = "INCOME_TAX", Name = "Income Tax", Rate = 25.00m, Type = TaxType.Income }
            };

            foreach (var taxDef in taxes)
            {
                var tax = repository.CreateObject<TaxDto>();
                tax.Id = Guid.NewGuid();
                tax.CreatedAt = DateTime.UtcNow;
                tax.UpdatedAt = DateTime.UtcNow;
                tax.Code = taxDef.Code;
                tax.Name = taxDef.Name;
                tax.Rate = taxDef.Rate;
                tax.TaxType = taxDef.Type;
                tax.IsActive = true;
            }

            _logger.LogDebug("Created {TaxCount} advanced tax configurations", taxes.Length);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates diverse business entities
        /// </summary>
        private async Task CreateDiverseBusinessEntitiesAsync(IRepository repository)
        {
            _logger.LogDebug("Creating diverse business entities");

            var entities = new[]
            {
                // Customers
                new { Code = "CUST001", Name = "ABC Corporation", Type = BusinessEntityType.Customer, Email = "contact@abc-corp.com" },
                new { Code = "CUST002", Name = "XYZ Industries", Type = BusinessEntityType.Customer, Email = "info@xyz-industries.com" },
                new { Code = "CUST003", Name = "Global Solutions Ltd", Type = BusinessEntityType.Customer, Email = "sales@globalsolutions.com" },
                
                // Suppliers
                new { Code = "SUPP001", Name = "Materials Plus Inc", Type = BusinessEntityType.Supplier, Email = "orders@materialsplus.com" },
                new { Code = "SUPP002", Name = "Office Supplies Co", Type = BusinessEntityType.Supplier, Email = "sales@officesupplies.com" },
                new { Code = "SUPP003", Name = "Tech Equipment LLC", Type = BusinessEntityType.Supplier, Email = "procurement@techequip.com" },
                
                // Employees
                new { Code = "EMP001", Name = "John Smith", Type = BusinessEntityType.Employee, Email = "john.smith@company.com" },
                new { Code = "EMP002", Name = "Sarah Johnson", Type = BusinessEntityType.Employee, Email = "sarah.johnson@company.com" },
                new { Code = "EMP003", Name = "Michael Brown", Type = BusinessEntityType.Employee, Email = "michael.brown@company.com" }
            };

            foreach (var entityDef in entities)
            {
                var entity = repository.CreateObject<BusinessEntityDto>();
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.Code = entityDef.Code;
                entity.Name = entityDef.Name;
                entity.EntityType = entityDef.Type;
                entity.Email = entityDef.Email;
            }

            _logger.LogDebug("Created {EntityCount} diverse business entities", entities.Length);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates comprehensive document types
        /// </summary>
        private async Task CreateComprehensiveDocumentTypesAsync(IRepository repository)
        {
            _logger.LogDebug("Creating comprehensive document types");

            var documentTypes = new[]
            {
                new { Code = "INV", Name = "Sales Invoice", GeneratesTransaction = true, Prefix = "INV-" },
                new { Code = "PUR", Name = "Purchase Invoice", GeneratesTransaction = true, Prefix = "PUR-" },
                new { Code = "PAY", Name = "Payment Voucher", GeneratesTransaction = true, Prefix = "PAY-" },
                new { Code = "REC", Name = "Receipt", GeneratesTransaction = true, Prefix = "REC-" },
                new { Code = "ADJ", Name = "Adjustment Entry", GeneratesTransaction = true, Prefix = "ADJ-" },
                new { Code = "QUO", Name = "Sales Quotation", GeneratesTransaction = false, Prefix = "QUO-" },
                new { Code = "PO", Name = "Purchase Order", GeneratesTransaction = false, Prefix = "PO-" },
                new { Code = "DEL", Name = "Delivery Note", GeneratesTransaction = false, Prefix = "DEL-" },
                new { Code = "CN", Name = "Credit Note", GeneratesTransaction = true, Prefix = "CN-" },
                new { Code = "DN", Name = "Debit Note", GeneratesTransaction = true, Prefix = "DN-" }
            };

            foreach (var docTypeDef in documentTypes)
            {
                var docType = repository.CreateObject<DocumentTypeDto>();
                docType.Id = Guid.NewGuid();
                docType.CreatedAt = DateTime.UtcNow;
                docType.UpdatedAt = DateTime.UtcNow;
                docType.Code = docTypeDef.Code;
                docType.Name = docTypeDef.Name;
                docType.GeneratesTransaction = docTypeDef.GeneratesTransaction;
            }

            _logger.LogDebug("Created {DocumentTypeCount} comprehensive document types", documentTypes.Length);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates sample items for inventory scenarios
        /// </summary>
        private async Task CreateSampleItemsAsync(IRepository repository)
        {
            _logger.LogDebug("Creating sample items");

            var items = new[]
            {
                new { Code = "ITEM001", Name = "Standard Widget", Category = "Widgets", Price = 25.00m },
                new { Code = "ITEM002", Name = "Premium Widget", Category = "Widgets", Price = 45.00m },
                new { Code = "ITEM003", Name = "Office Chair", Category = "Furniture", Price = 150.00m },
                new { Code = "ITEM004", Name = "Desk Lamp", Category = "Furniture", Price = 35.00m },
                new { Code = "SERV001", Name = "Consulting Hour", Category = "Services", Price = 125.00m },
                new { Code = "SERV002", Name = "Training Session", Category = "Services", Price = 200.00m }
            };

            foreach (var itemDef in items)
            {
                var item = repository.CreateObject<ItemDto>();
                item.Id = Guid.NewGuid();
                item.CreatedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
                item.Code = itemDef.Code;
                item.Name = itemDef.Name;
                item.Category = itemDef.Category;
                item.UnitPrice = itemDef.Price;
                item.IsActive = true;
            }

            _logger.LogDebug("Created {ItemCount} sample items", items.Length);
            await Task.CompletedTask;
        }

        #endregion

        #region Basic Data Creation Methods (unchanged)

        /// <summary>
        /// Creates a basic chart of accounts
        /// </summary>
        private async Task CreateBasicChartOfAccountsAsync(IRepository repository)
        {
            _logger.LogDebug("Creating basic chart of accounts");

            var accounts = new[]
            {
                new { Code = "1000", Name = "Cash", Type = AccountType.Asset },
                new { Code = "1200", Name = "Accounts Receivable", Type = AccountType.Asset },
                new { Code = "1300", Name = "Inventory", Type = AccountType.Asset },
                new { Code = "2000", Name = "Accounts Payable", Type = AccountType.Liability },
                new { Code = "2100", Name = "Accrued Liabilities", Type = AccountType.Liability },
                new { Code = "3000", Name = "Equity", Type = AccountType.Equity },
                new { Code = "4000", Name = "Sales Revenue", Type = AccountType.Revenue },
                new { Code = "5000", Name = "Cost of Goods Sold", Type = AccountType.Expense },
                new { Code = "6000", Name = "Operating Expenses", Type = AccountType.Expense }
            };

            foreach (var accountDef in accounts)
            {
                var account = repository.CreateObject<AccountDto>();
                account.Id = Guid.NewGuid();
                account.CreatedAt = DateTime.UtcNow;
                account.UpdatedAt = DateTime.UtcNow;
                account.OfficialCode = accountDef.Code;
                account.AccountName = accountDef.Name;
                account.AccountType = accountDef.Type;
                account.IsActive = true;
                account.Balance = 0m;
            }

            _logger.LogDebug("Created {AccountCount} basic accounts", accounts.Length);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates basic tax definitions
        /// </summary>
        private async Task CreateBasicTaxesAsync(IRepository repository)
        {
            _logger.LogDebug("Creating basic tax definitions");

            var taxes = new[]
            {
                new { Code = "VAT", Name = "Value Added Tax", Rate = 13.00m, Type = TaxType.VAT },
                new { Code = "SALES", Name = "Sales Tax", Rate = 10.00m, Type = TaxType.Sales },
                new { Code = "WITH", Name = "Withholding Tax", Rate = 5.00m, Type = TaxType.Withholding }
            };

            foreach (var taxDef in taxes)
            {
                var tax = repository.CreateObject<TaxDto>();
                tax.Id = Guid.NewGuid();
                tax.CreatedAt = DateTime.UtcNow;
                tax.UpdatedAt = DateTime.UtcNow;
                tax.Code = taxDef.Code;
                tax.Name = taxDef.Name;
                tax.Rate = taxDef.Rate;
                tax.TaxType = taxDef.Type;
                tax.IsActive = true;
            }

            _logger.LogDebug("Created {TaxCount} basic taxes", taxes.Length);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates basic business entities
        /// </summary>
        private async Task CreateBasicBusinessEntitiesAsync(IRepository repository)
        {
            _logger.LogDebug("Creating basic business entities");

            var entities = new[]
            {
                new { Code = "CUST001", Name = "Sample Customer", Type = BusinessEntityType.Customer, Email = "customer@example.com" },
                new { Code = "SUPP001", Name = "Sample Supplier", Type = BusinessEntityType.Supplier, Email = "supplier@example.com" },
                new { Code = "EMP001", Name = "Sample Employee", Type = BusinessEntityType.Employee, Email = "employee@example.com" }
            };

            foreach (var entityDef in entities)
            {
                var entity = repository.CreateObject<BusinessEntityDto>();
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.Code = entityDef.Code;
                entity.Name = entityDef.Name;
                entity.EntityType = entityDef.Type;
                entity.Email = entityDef.Email;
            }

            _logger.LogDebug("Created {EntityCount} basic business entities", entities.Length);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates basic document types
        /// </summary>
        private async Task CreateBasicDocumentTypesAsync(IRepository repository)
        {
            _logger.LogDebug("Creating basic document types");

            var documentTypes = new[]
            {
                new { Code = "INV", Name = "Sales Invoice", GeneratesTransaction = true },
                new { Code = "PUR", Name = "Purchase Invoice", GeneratesTransaction = true },
                new { Code = "PAY", Name = "Payment", GeneratesTransaction = true },
                new { Code = "ADJ", Name = "Adjustment", GeneratesTransaction = true },
                new { Code = "QUO", Name = "Quotation", GeneratesTransaction = false }
            };

            foreach (var docTypeDef in documentTypes)
            {
                var docType = repository.CreateObject<DocumentTypeDto>();
                docType.Id = Guid.NewGuid();
                docType.CreatedAt = DateTime.UtcNow;
                docType.UpdatedAt = DateTime.UtcNow;
                docType.Code = docTypeDef.Code;
                docType.Name = docTypeDef.Name;
                docType.GeneratesTransaction = docTypeDef.GeneratesTransaction;
            }

            _logger.LogDebug("Created {DocumentTypeCount} basic document types", documentTypes.Length);
            await Task.CompletedTask;
        }

        #endregion

        #region Scenario-Specific Generation Methods

        private async Task GenerateRealisticTransactionScenarioAsync(IRepository repository, int scenarioNumber, List<AccountDto> accounts)
        {
            var random = new Random(scenarioNumber); // Deterministic for reproducibility
            var scenarios = new[]
            {
                "Cash Sale", "Credit Sale", "Cash Purchase", "Credit Purchase", 
                "Payment Received", "Payment Made", "Bank Transfer", "Expense Payment",
                "Salary Payment", "Utility Payment", "Rent Payment", "Equipment Purchase"
            };

            var scenarioType = scenarios[scenarioNumber % scenarios.Length];
            var amount = (decimal)(random.NextDouble() * 9000 + 1000); // $1,000 to $10,000

            var transaction = repository.CreateObject<TransactionDto>();
            transaction.Id = Guid.NewGuid();
            transaction.CreatedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.TransactionNumber = $"TXN-{scenarioType.Replace(" ", "").ToUpper()}-{scenarioNumber:D3}";
            transaction.TransactionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-random.Next(30)));
            transaction.Description = $"{scenarioType} - Demo transaction {scenarioNumber}";
            transaction.IsPosted = random.NextDouble() > 0.3; // 70% posted

            // Add realistic ledger entries based on scenario
            await AddRealisticLedgerEntriesAsync(repository, transaction, scenarioType, amount, accounts, random);
        }

        private async Task AddRealisticLedgerEntriesAsync(IRepository repository, TransactionDto transaction, string scenarioType, decimal amount, List<AccountDto> accounts, Random random)
        {
            var cashAccount = accounts.FirstOrDefault(a => a.OfficialCode == "1000") ?? accounts[0];
            var revenueAccount = accounts.FirstOrDefault(a => a.OfficialCode == "4000") ?? accounts[1];
            var expenseAccount = accounts.FirstOrDefault(a => a.OfficialCode == "6000") ?? accounts.Last();

            switch (scenarioType)
            {
                case "Cash Sale":
                    await AddLedgerEntry(repository, transaction, cashAccount.OfficialCode, EntryType.Debit, amount, "Cash received from sale");
                    await AddLedgerEntry(repository, transaction, revenueAccount.OfficialCode, EntryType.Credit, amount, "Sales revenue");
                    break;
                case "Expense Payment":
                    await AddLedgerEntry(repository, transaction, expenseAccount.OfficialCode, EntryType.Debit, amount, "Operating expense");
                    await AddLedgerEntry(repository, transaction, cashAccount.OfficialCode, EntryType.Credit, amount, "Cash payment");
                    break;
                default:
                    // Generic double-entry
                    await AddLedgerEntry(repository, transaction, accounts[random.Next(accounts.Count)].OfficialCode, EntryType.Debit, amount, $"Debit entry for {scenarioType}");
                    await AddLedgerEntry(repository, transaction, accounts[random.Next(accounts.Count)].OfficialCode, EntryType.Credit, amount, $"Credit entry for {scenarioType}");
                    break;
            }
        }

        private async Task AddLedgerEntry(IRepository repository, TransactionDto transaction, string accountCode, EntryType entryType, decimal amount, string description)
        {
            var entry = repository.CreateObject<LedgerEntryDto>();
            entry.Id = Guid.NewGuid();
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.TransactionNumber = transaction.TransactionNumber;
            entry.AccountCode = accountCode;
            entry.EntryType = entryType;
            entry.Amount = amount;
            entry.Description = description;
            transaction.LedgerEntries.Add(entry);
            await Task.CompletedTask;
        }

        private async Task GenerateRealisticDocumentScenarioAsync(IRepository repository, int scenarioNumber, List<DocumentTypeDto> documentTypes, List<BusinessEntityDto> businessEntities, List<ItemDto> items)
        {
            var random = new Random(scenarioNumber);
            var docType = documentTypes[random.Next(documentTypes.Count)];
            var businessEntity = businessEntities[random.Next(businessEntities.Count)];

            var document = repository.CreateObject<DocumentDto>();
            document.Id = Guid.NewGuid();
            document.CreatedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;
            document.DocumentNumber = $"{docType.Code}-DEMO-{scenarioNumber:D3}";
            document.Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-random.Next(60)));
            document.Status = (DocumentStatus)(random.Next(4)); // Random status

            // Add realistic document totals
            var lineCount = random.Next(1, 6); // 1-5 lines
            for (int i = 0; i < lineCount; i++)
            {
                var total = repository.CreateObject<DocumentTotalDto>();
                total.Id = Guid.NewGuid();
                total.CreatedAt = DateTime.UtcNow;
                total.UpdatedAt = DateTime.UtcNow;
                total.Concept = items.Any() ? items[random.Next(items.Count)].Name : $"Line item {i + 1}";
                total.Total = (decimal)(random.NextDouble() * 500 + 50); // $50-$550 per line
                total.DebitAccountCode = "1000";
                total.CreditAccountCode = "4000";
                total.IncludeInTransaction = docType.GeneratesTransaction;
                document.DocumentTotals.Add(total);
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Data Set Specific Scenario Methods

        private async Task GenerateElSalvadorTaxScenariosAsync(IRepository repository)
        {
            // El Salvador specific tax scenarios would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateElSalvadorBusinessEntitiesAsync(IRepository repository)
        {
            // El Salvador specific business entities would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateElSalvadorDocumentTypesAsync(IRepository repository)
        {
            // El Salvador specific document types would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateUSATaxScenariosAsync(IRepository repository)
        {
            // USA specific tax scenarios would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateUSABusinessEntitiesAsync(IRepository repository)
        {
            // USA specific business entities would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateManufacturingAccountsAsync(IRepository repository)
        {
            // Manufacturing specific accounts would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateManufacturingItemsAsync(IRepository repository)
        {
            // Manufacturing specific items would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateManufacturingWorkflowsAsync(IRepository repository)
        {
            // Manufacturing specific workflows would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateServiceAccountsAsync(IRepository repository)
        {
            // Service business specific accounts would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateServiceContractsAsync(IRepository repository)
        {
            // Service business specific contracts would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateRetailAccountsAsync(IRepository repository)
        {
            // Retail specific accounts would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateRetailInventoryAsync(IRepository repository)
        {
            // Retail specific inventory would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateRetailPOSTransactionsAsync(IRepository repository)
        {
            // Retail POS transactions would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateGenericAccountsAsync(IRepository repository)
        {
            // Generic business accounts would be implemented here
            await Task.CompletedTask;
        }

        private async Task GenerateGenericBusinessEntitiesAsync(IRepository repository)
        {
            // Generic business entities would be implemented here
            await Task.CompletedTask;
        }

        #endregion
    }
}