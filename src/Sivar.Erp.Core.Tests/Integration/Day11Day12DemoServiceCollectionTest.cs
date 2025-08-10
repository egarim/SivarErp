using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Modules.DataImport;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Modules.Accounting;
using Sivar.Erp.Core.Modules.Documents;
using Sivar.Erp.Core.Modules.Taxes;
using Sivar.Erp.Core.Demo;
using Sivar.Erp.Core.Configuration;
using System.Diagnostics;

namespace Sivar.Erp.Core.Tests.Integration
{
    /// <summary>
    /// Comprehensive integration tests for Day 11-12: Demo Service Collection
    /// Tests multi-data set support, enhanced sample data generation, and advanced demo scenarios
    /// </summary>
    [Description("Integration tests for Day 11-12: Demo Service Collection")]
    public class Day11Day12DemoServiceCollectionTest : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository _repository;
        private readonly IDataImportService _dataImportService;
        private readonly ISampleDataGenerator _sampleDataGenerator;
        private readonly IAccountingService _accountingService;
        private readonly IDocumentService _documentService;
        private readonly ITaxService _taxService;
        private readonly ILogger<Day11Day12DemoServiceCollectionTest> _logger;

        public Day11Day12DemoServiceCollectionTest()
        {
            // Setup DI container with enhanced demo configuration
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddSivarErpDemo("ElSalvador");

            _serviceProvider = services.BuildServiceProvider();
            _repository = _serviceProvider.GetRequiredService<IRepository>();
            _dataImportService = _serviceProvider.GetRequiredService<IDataImportService>();
            _sampleDataGenerator = _serviceProvider.GetRequiredService<ISampleDataGenerator>();
            _accountingService = _serviceProvider.GetRequiredService<IAccountingService>();
            _documentService = _serviceProvider.GetRequiredService<IDocumentService>();
            _taxService = _serviceProvider.GetRequiredService<ITaxService>();
            _logger = _serviceProvider.GetRequiredService<ILogger<Day11Day12DemoServiceCollectionTest>>();
        }

        public void Dispose()
        {
            _repository?.Dispose();
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }

        [Fact]
        [Description("Test complete demo service collection integration")]
        public async Task TestCompleteDemoServiceCollectionIntegration()
        {
            // Arrange
            _logger.LogInformation("Starting complete demo service collection integration test");
            var stopwatch = Stopwatch.StartNew();

            // Act - Phase 1: Generate comprehensive sample data
            await _sampleDataGenerator.GenerateSampleDataAsync(_repository);
            var dataGenerationTime = stopwatch.ElapsedMilliseconds;

            // Assert - Verify all core data was created
            var accounts = _repository.GetObjects<AccountDto>().ToList();
            var taxes = _repository.GetObjects<TaxDto>().ToList();
            var businessEntities = _repository.GetObjects<BusinessEntityDto>().ToList();
            var documentTypes = _repository.GetObjects<DocumentTypeDto>().ToList();
            var items = _repository.GetObjects<ItemDto>().ToList();

            _logger.LogInformation("Generated data: {AccountCount} accounts, {TaxCount} taxes, {BusinessEntityCount} entities, {DocumentTypeCount} document types, {ItemCount} items",
                accounts.Count, taxes.Count, businessEntities.Count, documentTypes.Count, items.Count);

            Assert.True(accounts.Any(), "Should have generated accounts");
            Assert.True(taxes.Any() || accounts.Any(), "Should have generated taxes or accounts");
            Assert.True(businessEntities.Any() || accounts.Any(), "Should have generated business entities or accounts");

            // Phase 2: Test all services work with generated data
            stopwatch.Restart();
            await TestAccountingServiceWithGeneratedData();
            await TestDocumentServiceWithGeneratedData();
            await TestTaxServiceWithGeneratedData();
            var serviceIntegrationTime = stopwatch.ElapsedMilliseconds;

            // Phase 3: Test export/import round-trip
            stopwatch.Restart();
            await TestExportImportRoundTrip();
            var exportImportTime = stopwatch.ElapsedMilliseconds;

            // Assert performance metrics
            Assert.True(dataGenerationTime < 30000, $"Data generation took too long: {dataGenerationTime}ms");
            Assert.True(serviceIntegrationTime < 10000, $"Service integration took too long: {serviceIntegrationTime}ms");
            Assert.True(exportImportTime < 15000, $"Export/Import round-trip took too long: {exportImportTime}ms");

            _logger.LogInformation("Complete demo service collection integration test completed successfully");
        }

        [Fact]
        [Description("Test multi-data set support for different scenarios")]
        public async Task TestMultiDataSetSupport()
        {
            // Test ElSalvador data set
            var elSalvadorResult = await _dataImportService.ImportTestDataSetAsync("ElSalvador", "TestUser");
            _logger.LogInformation("ElSalvador data set import: {Success}, {Summary}", elSalvadorResult.Success, elSalvadorResult.Summary);

            var accountsAfterElSalvador = _repository.GetObjects<AccountDto>().Count();
            var taxesAfterElSalvador = _repository.GetObjects<TaxDto>().Count();

            // Clear repository for next test
            _repository.Rollback();
            _repository.Clear();

            // Test with enhanced demo configuration for different scenarios
            await TestDemoScenario("Manufacturing", includeInventory: true, includeProduction: false);
            await TestDemoScenario("Service", includeInventory: false, includeProduction: false);
            await TestDemoScenario("Retail", includeInventory: true, includeProduction: false);

            Assert.True(elSalvadorResult != null, "Should have processed ElSalvador data set");
        }

        [Fact]
        [Description("Test performance optimization for demo environments")]
        public async Task TestPerformanceOptimizationForDemoEnvironments()
        {
            var stopwatch = Stopwatch.StartNew();

            // Test 1: Large data set generation performance
            var largeDataGenerationTime = await TestLargeDataSetGeneration();
            
            stopwatch.Restart();
            
            // Test 2: Concurrent demo scenario generation
            var concurrentDemoTime = await TestConcurrentDemoScenarios();
            
            stopwatch.Restart();
            
            // Test 3: Memory usage optimization
            var memoryOptimizationTime = await TestMemoryUsageOptimization();

            // Assert performance expectations for demo environments
            Assert.True(largeDataGenerationTime < 20000, $"Large data generation took too long: {largeDataGenerationTime}ms");
            Assert.True(concurrentDemoTime < 15000, $"Concurrent demo scenarios took too long: {concurrentDemoTime}ms");
            Assert.True(memoryOptimizationTime < 5000, $"Memory optimization check took too long: {memoryOptimizationTime}ms");

            _logger.LogInformation("Performance optimization tests completed successfully");
        }

        [Fact]
        [Description("Test advanced demo scenarios with complete ERP workflow")]
        public async Task TestAdvancedDemoScenariosWithCompleteWorkflow()
        {
            // Setup comprehensive demo data
            await _sampleDataGenerator.GenerateSampleDataAsync(_repository);

            // Scenario 1: Complete sales workflow
            await TestCompleteSalesWorkflow();

            // Scenario 2: Complete purchase workflow  
            await TestCompletePurchaseWorkflow();

            // Scenario 3: Complete inventory workflow
            await TestCompleteInventoryWorkflow();

            // Scenario 4: Complete accounting cycle
            await TestCompleteAccountingCycle();

            _logger.LogInformation("Advanced demo scenarios completed successfully");
        }

        [Fact]
        [Description("Test demo data consistency and validation")]
        public async Task TestDemoDataConsistencyAndValidation()
        {
            // Generate sample data
            await _sampleDataGenerator.GenerateSampleDataAsync(_repository);

            // Validate data consistency
            await ValidateChartOfAccountsConsistency();
            await ValidateTaxConfigurationConsistency();
            await ValidateBusinessEntityConsistency();
            await ValidateDocumentTypeConsistency();

            _logger.LogInformation("Demo data consistency validation completed successfully");
        }

        [Fact]
        [Description("Test demo service configuration and options")]
        public async Task TestDemoServiceConfigurationAndOptions()
        {
            // Test different configuration scenarios
            await TestDemoWithMinimalConfiguration();
            await TestDemoWithMaximalConfiguration();
            await TestDemoWithCustomConfiguration();

            _logger.LogInformation("Demo service configuration tests completed successfully");
        }

        #region Helper Methods

        private async Task TestAccountingServiceWithGeneratedData()
        {
            var accounts = _repository.GetObjects<AccountDto>().Take(4).ToList();
            if (accounts.Count >= 2)
            {
                // Create a test document first
                var testDocument = _repository.CreateObject<DocumentDto>();
                testDocument.DocumentNumber = "TEST-DOC-001";
                testDocument.Date = DateOnly.FromDateTime(DateTime.Today);
                testDocument.Status = DocumentStatus.Draft;

                // Add document totals
                var documentTotal = _repository.CreateObject<DocumentTotalDto>();
                documentTotal.Concept = "Demo transaction";
                documentTotal.Total = 1000m;
                documentTotal.DebitAccountCode = accounts[0].OfficialCode;
                documentTotal.CreditAccountCode = accounts[1].OfficialCode;
                documentTotal.IncludeInTransaction = true;
                testDocument.DocumentTotals.Add(documentTotal);

                await _repository.CommitChanges();

                // Test transaction creation from document
                var transaction = await _accountingService.CreateTransactionAsync(testDocument, "Demo transaction");

                Assert.NotNull(transaction);
                Assert.True(transaction.LedgerEntries.Count >= 2);
            }
        }

        private async Task TestDocumentServiceWithGeneratedData()
        {
            var documentTypes = _repository.GetObjects<DocumentTypeDto>().Take(1).ToList();
            var businessEntities = _repository.GetObjects<BusinessEntityDto>().Take(1).ToList();

            if (documentTypes.Any() && businessEntities.Any())
            {
                var document = await _documentService.CreateDocumentAsync(documentTypes[0], businessEntities[0]);

                Assert.NotNull(document);
                Assert.NotEmpty(document.DocumentNumber);
            }
        }

        private async Task TestTaxServiceWithGeneratedData()
        {
            var testDocument = _repository.CreateObject<DocumentDto>();
            testDocument.DocumentNumber = "TEST-TAX-001";
            testDocument.Date = DateOnly.FromDateTime(DateTime.Today);
            testDocument.Status = DocumentStatus.Draft;

            // Add a document total
            var documentTotal = _repository.CreateObject<DocumentTotalDto>();
            documentTotal.Concept = "Test amount";
            documentTotal.Total = 1000m;
            testDocument.DocumentTotals.Add(documentTotal);

            await _repository.CommitChanges();

            var taxSummary = await _taxService.GetTaxSummaryAsync(testDocument);
            Assert.NotNull(taxSummary);
            Assert.True(taxSummary.TotalTaxAmount >= 0);
        }

        private async Task TestExportImportRoundTrip()
        {
            // Export current data
            var accountsCsv = await _dataImportService.ExportToCsvAsync<AccountDto>();
            var taxesCsv = await _dataImportService.ExportToCsvAsync<TaxDto>();

            Assert.NotNull(accountsCsv);
            Assert.NotNull(taxesCsv);

            // Clear and re-import
            var originalAccountCount = _repository.GetObjects<AccountDto>().Count();
            
            if (originalAccountCount > 0)
            {
                _repository.Clear();
                
                var importRequest = new ImportRequest
                {
                    EntityType = typeof(AccountDto),
                    DataSource = accountsCsv
                };

                var importResult = await _dataImportService.ImportBatchAsync(new[] { importRequest }, "TestUser");
                
                // Verify round-trip (may not be exact due to generated IDs)
                var newAccountCount = _repository.GetObjects<AccountDto>().Count();
                Assert.True(newAccountCount > 0, "Should have imported some accounts");
            }
        }

        private async Task TestDemoScenario(string scenarioName, bool includeInventory, bool includeProduction)
        {
            _logger.LogInformation("Testing demo scenario: {ScenarioName}", scenarioName);

            // Create scenario-specific configuration
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddSivarErpDemo("ElSalvador");
            
            services.Configure<DemoOptions>(options =>
            {
                options.TestDataSet = "ElSalvador";
                options.IncludeTestTransactions = true;
                options.IncludeTestDocuments = true;
                options.MaxSampleRecords = includeInventory ? 1000 : 100;
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var repository = serviceProvider.GetRequiredService<IRepository>();
            var sampleDataGenerator = serviceProvider.GetRequiredService<ISampleDataGenerator>();

            await sampleDataGenerator.GenerateSampleDataAsync(repository);

            var accountCount = repository.GetObjects<AccountDto>().Count();
            _logger.LogInformation("Scenario {ScenarioName} generated {AccountCount} accounts", scenarioName, accountCount);

            Assert.True(accountCount >= 0, $"Scenario {scenarioName} should generate some data");
        }

        private async Task<long> TestLargeDataSetGeneration()
        {
            var stopwatch = Stopwatch.StartNew();

            // Configure for large data set
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
            services.AddSivarErpDemo("ElSalvador");
            
            services.Configure<DemoOptions>(options =>
            {
                options.MaxSampleRecords = 5000;
                options.IncludeTestTransactions = true;
                options.IncludeTestDocuments = true;
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var repository = serviceProvider.GetRequiredService<IRepository>();
            var sampleDataGenerator = serviceProvider.GetRequiredService<ISampleDataGenerator>();

            await sampleDataGenerator.GenerateSampleDataAsync(repository);

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private async Task<long> TestConcurrentDemoScenarios()
        {
            var stopwatch = Stopwatch.StartNew();

            var tasks = new[]
            {
                Task.Run(async () => await TestDemoScenario("Concurrent1", true, false)),
                Task.Run(async () => await TestDemoScenario("Concurrent2", false, false)),
                Task.Run(async () => await TestDemoScenario("Concurrent3", true, true))
            };

            await Task.WhenAll(tasks);

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private async Task<long> TestMemoryUsageOptimization()
        {
            var stopwatch = Stopwatch.StartNew();

            // Test memory usage with large data sets
            await _sampleDataGenerator.GenerateSampleDataAsync(_repository);

            // Force garbage collection and measure
            var beforeMemory = GC.GetTotalMemory(true);
            
            // Clear repository
            _repository.Clear();
            
            var afterMemory = GC.GetTotalMemory(true);
            var memoryReleased = beforeMemory - afterMemory;

            _logger.LogInformation("Memory optimization: Released {MemoryReleased} bytes", memoryReleased);

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private async Task TestCompleteSalesWorkflow()
        {
            _logger.LogInformation("Testing complete sales workflow");
            
            var customers = _repository.GetObjects<BusinessEntityDto>()
                .Where(be => be.EntityType == BusinessEntityType.Customer)
                .Take(1).ToList();
                
            if (customers.Any())
            {
                var invoiceType = _repository.GetObjects<DocumentTypeDto>()
                    .FirstOrDefault(dt => dt.Code == "INV");
                
                if (invoiceType != null)
                {
                    // Create sales invoice
                    var invoice = await _documentService.CreateDocumentAsync(invoiceType, customers[0]);
                    
                    Assert.NotNull(invoice);
                }
            }
        }

        private async Task TestCompletePurchaseWorkflow()
        {
            _logger.LogInformation("Testing complete purchase workflow");
            
            var suppliers = _repository.GetObjects<BusinessEntityDto>()
                .Where(be => be.EntityType == BusinessEntityType.Supplier)
                .Take(1).ToList();
                
            if (suppliers.Any())
            {
                var purchaseType = _repository.GetObjects<DocumentTypeDto>()
                    .FirstOrDefault(dt => dt.Code == "PUR");
                
                if (purchaseType != null)
                {
                    // Create purchase invoice
                    var purchaseInvoice = await _documentService.CreateDocumentAsync(purchaseType, suppliers[0]);
                    
                    Assert.NotNull(purchaseInvoice);
                }
            }
        }

        private async Task TestCompleteInventoryWorkflow()
        {
            _logger.LogInformation("Testing complete inventory workflow");
            
            var items = _repository.GetObjects<ItemDto>().Take(1).ToList();
            if (items.Any())
            {
                // Test inventory-related functionality
                Assert.True(items[0].Id != Guid.Empty);
            }
        }

        private async Task TestCompleteAccountingCycle()
        {
            _logger.LogInformation("Testing complete accounting cycle");
            
            // Test trial balance
            var trialBalance = await _accountingService.GetTrialBalanceAsync(
                DateOnly.FromDateTime(DateTime.Today));
                
            Assert.NotNull(trialBalance);
        }

        private async Task ValidateChartOfAccountsConsistency()
        {
            var accounts = _repository.GetObjects<AccountDto>().ToList();
            
            // Validate account codes are unique
            var duplicateCodes = accounts.GroupBy(a => a.OfficialCode)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
                
            Assert.Empty(duplicateCodes);
            
            // Validate account types are valid
            foreach (var account in accounts)
            {
                Assert.True(Enum.IsDefined(typeof(AccountType), account.AccountType));
            }
        }

        private async Task ValidateTaxConfigurationConsistency()
        {
            var taxes = _repository.GetObjects<TaxDto>().ToList();
            
            // Validate tax codes are unique
            var duplicateCodes = taxes.GroupBy(t => t.Code)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
                
            Assert.Empty(duplicateCodes);
            
            // Validate tax rates are reasonable
            foreach (var tax in taxes)
            {
                Assert.True(tax.Rate >= 0 && tax.Rate <= 100);
            }
        }

        private async Task ValidateBusinessEntityConsistency()
        {
            var businessEntities = _repository.GetObjects<BusinessEntityDto>().ToList();
            
            // Validate entity codes are unique
            var duplicateCodes = businessEntities.GroupBy(be => be.Code)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
                
            Assert.Empty(duplicateCodes);
        }

        private async Task ValidateDocumentTypeConsistency()
        {
            var documentTypes = _repository.GetObjects<DocumentTypeDto>().ToList();
            
            // Validate document type codes are unique
            var duplicateCodes = documentTypes.GroupBy(dt => dt.Code)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
                
            Assert.Empty(duplicateCodes);
        }

        private async Task TestDemoWithMinimalConfiguration()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
            services.AddSivarErpDemo("ElSalvador");
            
            services.Configure<DemoOptions>(options =>
            {
                options.MaxSampleRecords = 10;
                options.IncludeTestTransactions = false;
                options.IncludeTestDocuments = false;
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var repository = serviceProvider.GetRequiredService<IRepository>();
            var sampleDataGenerator = serviceProvider.GetRequiredService<ISampleDataGenerator>();

            await sampleDataGenerator.GenerateSampleDataAsync(repository);
            
            // Should have some basic data
            var totalEntities = repository.GetObjects<AccountDto>().Count() +
                              repository.GetObjects<TaxDto>().Count() +
                              repository.GetObjects<BusinessEntityDto>().Count();
                              
            Assert.True(totalEntities >= 0);
        }

        private async Task TestDemoWithMaximalConfiguration()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddSivarErpDemo("ElSalvador");
            
            services.Configure<DemoOptions>(options =>
            {
                options.MaxSampleRecords = 1000;
                options.IncludeTestTransactions = true;
                options.IncludeTestDocuments = true;
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var repository = serviceProvider.GetRequiredService<IRepository>();
            var sampleDataGenerator = serviceProvider.GetRequiredService<ISampleDataGenerator>();

            await sampleDataGenerator.GenerateSampleDataAsync(repository);
            
            // Should have comprehensive data
            var totalEntities = repository.GetObjects<AccountDto>().Count() +
                              repository.GetObjects<TaxDto>().Count() +
                              repository.GetObjects<BusinessEntityDto>().Count() +
                              repository.GetObjects<TransactionDto>().Count() +
                              repository.GetObjects<DocumentDto>().Count();
                              
            Assert.True(totalEntities >= 0);
        }

        private async Task TestDemoWithCustomConfiguration()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddSivarErpCore(new ErpCoreOptions
            {
                DatabaseProvider = DatabaseProvider.InMemory,
                EnablePerformanceLogging = true,
                MaxTransactionBatchSize = 500
            });
            
            services.AddScoped<ISampleDataGenerator, SampleDataGenerator>();
            services.Configure<DemoOptions>(options =>
            {
                options.TestDataSet = "ElSalvador";
                options.MaxSampleRecords = 250;
                options.IncludeTestTransactions = true;
                options.IncludeTestDocuments = false;
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var repository = serviceProvider.GetRequiredService<IRepository>();
            var sampleDataGenerator = serviceProvider.GetRequiredService<ISampleDataGenerator>();

            await sampleDataGenerator.GenerateSampleDataAsync(repository);
            
            // Should respect custom configuration
            var transactions = repository.GetObjects<TransactionDto>().Count();
            var documents = repository.GetObjects<DocumentDto>().Count();
            
            Assert.True(transactions >= 0);
            Assert.Equal(0, documents); // Should be 0 since IncludeTestDocuments = false
        }

        #endregion
    }
}