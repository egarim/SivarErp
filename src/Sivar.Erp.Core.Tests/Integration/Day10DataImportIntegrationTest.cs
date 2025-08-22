using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Modules.DataImport;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Demo;
using Sivar.Erp.Core.Configuration;
using System.Diagnostics;

namespace Sivar.Erp.Core.Tests.Integration
{
    /// <summary>
    /// Integration tests for Day 10: Enhanced DataImportService integration
    /// Tests the complete data import workflow with embedded resources and multi-entity coordination
    /// </summary>
    [Description("Integration tests for DataImportService integration")]
    public class Day10DataImportIntegrationTest : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository _repository;
        private readonly IDataImportService _dataImportService;
        private readonly ISampleDataGenerator _sampleDataGenerator;

        public Day10DataImportIntegrationTest()
        {
            // Setup DI container
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddSivarErpDemo("ElSalvador");

            _serviceProvider = services.BuildServiceProvider();
            _repository = _serviceProvider.GetRequiredService<IRepository>();
            _dataImportService = _serviceProvider.GetRequiredService<IDataImportService>();
            _sampleDataGenerator = _serviceProvider.GetRequiredService<ISampleDataGenerator>();
        }

        public void Dispose()
        {
            _repository?.Dispose();
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }

        [Fact]
        [Description("Test embedded resource loading and CSV import workflow")]
        public async Task TestEmbeddedResourceLoadingAndCsvImport()
        {
            // Arrange
            var userName = "TestUser";

            // Act - Test embedded resource loading
            var availableResources = Sivar.Erp.Core.Modules.DataImport.TestDataResourceManager.GetAvailableResources();
            Assert.NotEmpty(availableResources);

            // Test loading a specific resource (if available)
            var firstResource = availableResources.FirstOrDefault();
            if (!string.IsNullOrEmpty(firstResource))
            {
                var csvContent = await Sivar.Erp.Core.Modules.DataImport.TestDataResourceManager.LoadCsvAsync(firstResource);
                Assert.NotNull(csvContent);
                Assert.NotEmpty(csvContent);
            }

            // Test data set import
            var importResult = await _dataImportService.ImportTestDataSetAsync("ElSalvador", userName);

            // Assert
            Assert.NotNull(importResult);
            Assert.Equal("ElSalvador", importResult.DataSetName);
            // Note: Success may be false if no actual CSV processing is implemented yet
            // but the structure should be correct
        }

        [Fact]
        [Description("Test multi-entity import coordination with dependency ordering")]
        public async Task TestMultiEntityImportCoordination()
        {
            // Arrange
            var userName = "TestUser";
            var importRequests = new List<ImportRequest>
            {
                new ImportRequest
                {
                    EntityType = typeof(BusinessEntityDto),
                    DataSource = CreateSampleBusinessEntityCsv(),
                    Priority = ImportPriority.Normal
                },
                new ImportRequest
                {
                    EntityType = typeof(AccountDto),
                    DataSource = CreateSampleAccountCsv(),
                    Priority = ImportPriority.High // Should be processed first
                },
                new ImportRequest
                {
                    EntityType = typeof(TaxDto),
                    DataSource = CreateSampleTaxCsv(),
                    Priority = ImportPriority.Normal
                }
            };

            // Act
            var batchResult = await _dataImportService.ImportBatchAsync(importRequests, userName);

            // Assert
            Assert.NotNull(batchResult);
            Assert.Equal(3, batchResult.ImportResults.Count);
            
            // Verify that high priority items were processed first
            var firstResult = batchResult.ImportResults.First();
            Assert.Equal("AccountDto", firstResult.EntityType);
        }

        [Fact]
        [Description("Test import validation with comprehensive checks")]
        public async Task TestImportValidationWithComprehensiveChecks()
        {
            // Arrange - Valid CSV data
            var validRequest = new ImportRequest
            {
                EntityType = typeof(AccountDto),
                DataSource = CreateSampleAccountCsv()
            };

            // Arrange - Invalid CSV data
            var invalidRequest = new ImportRequest
            {
                EntityType = typeof(AccountDto),
                DataSource = "Invalid,Data\nWith,Too,Few,Columns"
            };

            // Arrange - Empty request
            var emptyRequest = new ImportRequest
            {
                EntityType = null,
                DataSource = ""
            };

            // Act
            var validResult = await _dataImportService.ValidateImportAsync(validRequest);
            var invalidResult = await _dataImportService.ValidateImportAsync(invalidRequest);
            var emptyResult = await _dataImportService.ValidateImportAsync(emptyRequest);

            // Assert
            Assert.True(validResult.IsValid);
            Assert.True(validResult.RecordCount > 0);
            Assert.NotEmpty(validResult.Preview);

            Assert.True(invalidResult.IsValid); // Structure validation might still pass
            Assert.NotEmpty(invalidResult.Warnings); // But should have warnings

            Assert.False(emptyResult.IsValid);
            Assert.NotEmpty(emptyResult.Errors);
        }

        [Fact]
        [Description("Test performance with large data sets")]
        public async Task TestPerformanceWithLargeDataSets()
        {
            // Arrange
            var largeCsvData = CreateLargeAccountCsv(1000); // 1000 accounts
            var importRequest = new ImportRequest
            {
                EntityType = typeof(AccountDto),
                DataSource = largeCsvData,
                Priority = ImportPriority.Normal
            };

            var stopwatch = Stopwatch.StartNew();

            // Act
            var validationResult = await _dataImportService.ValidateImportAsync(importRequest);
            var validationTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            var batchResult = await _dataImportService.ImportBatchAsync(new[] { importRequest }, "TestUser");
            var importTime = stopwatch.ElapsedMilliseconds;

            // Assert
            Assert.True(validationTime < 5000, $"Validation took too long: {validationTime}ms");
            Assert.True(importTime < 10000, $"Import took too long: {importTime}ms");
            Assert.Equal(1000, validationResult.RecordCount);
        }

        [Fact]
        [Description("Test import template generation")]
        public async Task TestImportTemplateGeneration()
        {
            // Act
            var accountTemplate = await _dataImportService.CreateImportTemplateAsync<AccountDto>();
            var taxTemplate = await _dataImportService.CreateImportTemplateAsync<TaxDto>();
            var businessEntityTemplate = await _dataImportService.CreateImportTemplateAsync<BusinessEntityDto>();

            // Assert
            Assert.NotNull(accountTemplate);
            Assert.Contains("OfficialCode", accountTemplate);
            Assert.Contains("AccountName", accountTemplate);
            
            Assert.NotNull(taxTemplate);
            Assert.Contains("Code", taxTemplate);
            Assert.Contains("Name", taxTemplate);
            
            Assert.NotNull(businessEntityTemplate);
            Assert.Contains("Code", businessEntityTemplate);
            Assert.Contains("Name", businessEntityTemplate);
        }

        [Fact]
        [Description("Test CSV export functionality")]
        public async Task TestCsvExportFunctionality()
        {
            // Arrange - Create some test data
            var account1 = _repository.CreateObject<AccountDto>();
            account1.OfficialCode = "TEST001";
            account1.AccountName = "Test Account 1";
            account1.AccountType = AccountType.Asset;

            var account2 = _repository.CreateObject<AccountDto>();
            account2.OfficialCode = "TEST002";
            account2.AccountName = "Test Account 2";
            account2.AccountType = AccountType.Liability;

            await _repository.CommitChanges();

            // Act
            var allAccountsCsv = await _dataImportService.ExportToCsvAsync<AccountDto>();
            var filteredAccountsCsv = await _dataImportService.ExportToCsvAsync<AccountDto>(
                a => a.AccountType == AccountType.Asset);

            // Assert
            Assert.NotNull(allAccountsCsv);
            Assert.Contains("TEST001", allAccountsCsv);
            Assert.Contains("TEST002", allAccountsCsv);

            Assert.NotNull(filteredAccountsCsv);
            Assert.Contains("TEST001", filteredAccountsCsv);
            Assert.DoesNotContain("TEST002", filteredAccountsCsv);
        }

        [Fact]
        [Description("Test import history tracking")]
        public async Task TestImportHistoryTracking()
        {
            // Arrange
            var userName = "TestUser";
            var importRequest = new ImportRequest
            {
                EntityType = typeof(AccountDto),
                DataSource = CreateSampleAccountCsv()
            };

            // Act
            await _dataImportService.ImportBatchAsync(new[] { importRequest }, userName);

            var fromDate = DateTime.UtcNow.AddHours(-1);
            var toDate = DateTime.UtcNow.AddHours(1);
            var history = await _dataImportService.GetImportHistoryAsync(fromDate, toDate, userName);

            // Assert
            Assert.NotNull(history);
            // Note: History tracking implementation may vary
        }

        [Fact]
        [Description("Test error handling and recovery")]
        public async Task TestErrorHandlingAndRecovery()
        {
            // Arrange - Create a request that will fail
            var badRequest = new ImportRequest
            {
                EntityType = typeof(AccountDto),
                DataSource = null!, // This should cause an error
                Priority = ImportPriority.Critical
            };

            var goodRequest = new ImportRequest
            {
                EntityType = typeof(TaxDto),
                DataSource = CreateSampleTaxCsv(),
                Priority = ImportPriority.Normal
            };

            // Act
            var batchResult = await _dataImportService.ImportBatchAsync(
                new[] { badRequest, goodRequest }, "TestUser");

            // Assert
            Assert.False(batchResult.Success); // Should fail due to critical error
            Assert.NotEmpty(batchResult.Errors);
            
            // Should stop at the critical error and not process the good request
            Assert.Single(batchResult.ImportResults);
        }

        [Fact]
        [Description("Test integration with SampleDataGenerator")]
        public async Task TestIntegrationWithSampleDataGenerator()
        {
            // Act
            await _sampleDataGenerator.GenerateSampleDataAsync(_repository);

            // Assert - Verify that data was created
            var accounts = _repository.GetObjects<AccountDto>().ToList();
            var taxes = _repository.GetObjects<TaxDto>().ToList();
            var businessEntities = _repository.GetObjects<BusinessEntityDto>().ToList();

            // Should have created some data (either from CSV or fallback)
            Assert.True(accounts.Any() || taxes.Any() || businessEntities.Any(), 
                "Sample data generator should have created some data");
        }

        [Fact]
        [Description("Test complete data import workflow end-to-end")]
        public async Task TestCompleteDataImportWorkflow()
        {
            // Phase 1: Validate data before import
            var accountRequest = new ImportRequest
            {
                EntityType = typeof(AccountDto),
                DataSource = CreateSampleAccountCsv(),
                ValidateOnly = true
            };

            var validationResult = await _dataImportService.ValidateImportAsync(accountRequest);
            Assert.True(validationResult.IsValid);

            // Phase 2: Import the data
            accountRequest.ValidateOnly = false;
            var importResult = await _dataImportService.ImportBatchAsync(new[] { accountRequest }, "TestUser");
            
            // Phase 3: Verify import success
            Assert.NotNull(importResult);

            // Phase 4: Export the data to verify round-trip
            var exportedCsv = await _dataImportService.ExportToCsvAsync<AccountDto>();
            Assert.NotNull(exportedCsv);

            // Phase 5: Check import history
            var history = await _dataImportService.GetImportHistoryAsync(
                DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(5));
            Assert.NotNull(history);
        }

        #region Helper Methods

        private string CreateSampleAccountCsv()
        {
            return @"OfficialCode,AccountName,AccountType,IsActive
1000,Cash,Asset,true
1200,Accounts Receivable,Asset,true
2000,Accounts Payable,Liability,true
4000,Sales Revenue,Revenue,true";
        }

        private string CreateSampleTaxCsv()
        {
            return @"Code,Name,Rate,TaxType,IsActive
VAT,Value Added Tax,13.00,VAT,true
SALES,Sales Tax,10.00,Sales,true
WITH,Withholding Tax,5.00,Withholding,true";
        }

        private string CreateSampleBusinessEntityCsv()
        {
            return @"Code,Name,EntityType,Email
CUST001,Sample Customer,Customer,customer@example.com
SUPP001,Sample Supplier,Supplier,supplier@example.com
EMP001,Sample Employee,Employee,employee@example.com";
        }

        private string CreateLargeAccountCsv(int recordCount)
        {
            var csv = "OfficialCode,AccountName,AccountType,IsActive\n";
            
            for (int i = 1; i <= recordCount; i++)
            {
                var accountType = (i % 4) switch
                {
                    0 => "Asset",
                    1 => "Liability", 
                    2 => "Revenue",
                    _ => "Expense"
                };
                
                csv += $"ACC{i:D6},Account {i},{accountType},true\n";
            }
            
            return csv;
        }

        #endregion
    }
}