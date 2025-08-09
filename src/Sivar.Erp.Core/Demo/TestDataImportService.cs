using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Accounting;
using Sivar.Erp.Core.Modules.DataImport;
using Sivar.Erp.Core.Modules.DataImport.Importers;
using Sivar.Erp.Core.Modules.Documents;
using Sivar.Erp.Core.Modules.Documents.Models;
using Sivar.Erp.Core.Modules.Taxes.Models;
using System.ComponentModel;

namespace Sivar.Erp.Core.Demo
{
    /// <summary>
    /// Service for importing embedded test data
    /// </summary>
    [Description("Service for importing embedded test data")]
    public class TestDataImportService
    {
        private readonly IRepository _repository;
        private readonly ICsvImportService _csvImportService;
        private readonly ILogger<TestDataImportService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        
        /// <summary>
        /// Initializes a new instance of the TestDataImportService class
        /// </summary>
        /// <param name="repository">The repository for data access</param>
        /// <param name="csvImportService">The CSV import service</param>
        /// <param name="logger">The logger for diagnostic information</param>
        /// <param name="loggerFactory">The logger factory</param>
        public TestDataImportService(
            IRepository repository,
            ICsvImportService csvImportService,
            ILogger<TestDataImportService> logger,
            ILoggerFactory loggerFactory)
        {
            _repository = repository;
            _csvImportService = csvImportService;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }
        
        /// <summary>
        /// Imports all test data
        /// </summary>
        /// <param name="userName">The user performing the import</param>
        /// <returns>A dictionary containing the import results</returns>
        public async Task<Dictionary<string, object>> ImportAllTestDataAsync(string userName = "TestDataImport")
        {
            var results = new Dictionary<string, object>();
            
            try
            {
                // Import chart of accounts
                var accountsResult = await ImportChartOfAccountsAsync(userName);
                results.Add("ChartOfAccounts", accountsResult);
                
                // Import taxes
                var taxesResult = await ImportTaxesAsync(userName);
                results.Add("Taxes", taxesResult);
                
                // Import business entities
                var businessEntitiesResult = await ImportBusinessEntitiesAsync(userName);
                results.Add("BusinessEntities", businessEntitiesResult);
                
                // Import items
                var itemsResult = await ImportItemsAsync(userName);
                results.Add("Items", itemsResult);
                
                // Import document types
                var documentTypesResult = await ImportDocumentTypesAsync(userName);
                results.Add("DocumentTypes", documentTypesResult);
                
                // Import tax accounting profiles
                var taxAccountingProfilesResult = await ImportTaxAccountingProfilesAsync(userName);
                results.Add("TaxAccountingProfiles", taxAccountingProfilesResult);
                
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing test data");
                results.Add("Error", ex.Message);
                return results;
            }
        }
        
        /// <summary>
        /// Imports the chart of accounts
        /// </summary>
        /// <param name="userName">The user performing the import</param>
        /// <returns>The import result</returns>
        public async Task<object> ImportChartOfAccountsAsync(string userName = "TestDataImport")
        {
            try
            {
                string csvContent = await TestDataResourceManager.LoadCsvAsync("ComercialChartOfAccounts.txt");
                
                // Create an account importer directly with the correct logger type
                var accountLogger = _loggerFactory.CreateLogger<AccountImporter>();
                var accountImporter = new AccountImporter(accountLogger);
                var result = await accountImporter.ImportAsync(_repository, csvContent, userName);
                
                _logger.LogInformation("Imported {Count} accounts with {ErrorCount} errors", 
                    result.ImportedEntities.Count, result.Errors.Count);
                
                return new
                {
                    Success = result.Success,
                    ImportedCount = result.ImportedEntities.Count,
                    Errors = result.Errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing chart of accounts");
                return new { Success = false, Error = ex.Message };
            }
        }
        
        /// <summary>
        /// Imports taxes
        /// </summary>
        /// <param name="userName">The user performing the import</param>
        /// <returns>The import result</returns>
        public async Task<object> ImportTaxesAsync(string userName = "TestDataImport")
        {
            try
            {
                string csvContent = await TestDataResourceManager.LoadCsvAsync("ElSalvadorTaxes.txt");
                var result = await _csvImportService.ImportFromCsvAsync<TaxDto>(csvContent, userName);
                
                _logger.LogInformation("Imported {Count} taxes with {ErrorCount} errors", 
                    result.ImportedEntities.Count, result.Errors.Count);
                
                return new
                {
                    Success = result.Success,
                    ImportedCount = result.ImportedEntities.Count,
                    Errors = result.Errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing taxes");
                return new { Success = false, Error = ex.Message };
            }
        }
        
        /// <summary>
        /// Imports business entities
        /// </summary>
        /// <param name="userName">The user performing the import</param>
        /// <returns>The import result</returns>
        public async Task<object> ImportBusinessEntitiesAsync(string userName = "TestDataImport")
        {
            try
            {
                string csvContent = await TestDataResourceManager.LoadCsvAsync("BusinesEntities.txt");
                var result = await _csvImportService.ImportFromCsvAsync<BusinessEntityDto>(csvContent, userName);
                
                _logger.LogInformation("Imported {Count} business entities with {ErrorCount} errors", 
                    result.ImportedEntities.Count, result.Errors.Count);
                
                return new
                {
                    Success = result.Success,
                    ImportedCount = result.ImportedEntities.Count,
                    Errors = result.Errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing business entities");
                return new { Success = false, Error = ex.Message };
            }
        }
        
        /// <summary>
        /// Imports items
        /// </summary>
        /// <param name="userName">The user performing the import</param>
        /// <returns>The import result</returns>
        public async Task<object> ImportItemsAsync(string userName = "TestDataImport")
        {
            try
            {
                string csvContent = await TestDataResourceManager.LoadCsvAsync("Items.txt");
                var result = await _csvImportService.ImportFromCsvAsync<ItemDto>(csvContent, userName);
                
                _logger.LogInformation("Imported {Count} items with {ErrorCount} errors", 
                    result.ImportedEntities.Count, result.Errors.Count);
                
                return new
                {
                    Success = result.Success,
                    ImportedCount = result.ImportedEntities.Count,
                    Errors = result.Errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing items");
                return new { Success = false, Error = ex.Message };
            }
        }
        
        /// <summary>
        /// Imports document types
        /// </summary>
        /// <param name="userName">The user performing the import</param>
        /// <returns>The import result</returns>
        public async Task<object> ImportDocumentTypesAsync(string userName = "TestDataImport")
        {
            try
            {
                string csvContent = await TestDataResourceManager.LoadCsvAsync("DocumentTypes.csv");
                var result = await _csvImportService.ImportFromCsvAsync<DocumentTypeDto>(csvContent, userName);
                
                _logger.LogInformation("Imported {Count} document types with {ErrorCount} errors", 
                    result.ImportedEntities.Count, result.Errors.Count);
                
                return new
                {
                    Success = result.Success,
                    ImportedCount = result.ImportedEntities.Count,
                    Errors = result.Errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing document types");
                return new { Success = false, Error = ex.Message };
            }
        }
        
        /// <summary>
        /// Imports tax accounting profiles
        /// </summary>
        /// <param name="userName">The user performing the import</param>
        /// <returns>The import result</returns>
        public async Task<object> ImportTaxAccountingProfilesAsync(string userName = "TestDataImport")
        {
            try
            {
                string csvContent = await TestDataResourceManager.LoadCsvAsync("TaxAccountingProfiles.csv");
                
                // For now, we'll just log the content since we haven't defined the TaxAccountingProfile model yet
                _logger.LogInformation("Tax accounting profiles content loaded, {Length} characters", csvContent.Length);
                
                return new
                {
                    Success = true,
                    ContentLength = csvContent.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing tax accounting profiles");
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}