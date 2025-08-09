using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.DataImport.Models;

namespace Sivar.Erp.Core.Modules.DataImport
{
    /// <summary>
    /// Implementation of data import service for managing import operations
    /// </summary>
    [Description("Implementation of data import service")]
    public class DataImportService : IDataImportService
    {
        private readonly IRepository _repository;
        private readonly ICsvImportService _csvImportService;
        private readonly ILogger<DataImportService> _logger;

        /// <summary>
        /// Initializes a new instance of the DataImportService
        /// </summary>
        /// <param name="repository">Repository for data access</param>
        /// <param name="csvImportService">CSV import service</param>
        /// <param name="logger">Logger for the service</param>
        public DataImportService(
            IRepository repository, 
            ICsvImportService csvImportService, 
            ILogger<DataImportService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _csvImportService = csvImportService ?? throw new ArgumentNullException(nameof(csvImportService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Imports data from multiple sources in a batch operation
        /// </summary>
        /// <param name="importRequests">Collection of import requests</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Batch import result</returns>
        [Description("Imports data from multiple sources in a batch operation")]
        public async Task<BatchImportResult> ImportBatchAsync(IEnumerable<ImportRequest> importRequests, string userName)
        {
            _logger.LogInformation("Starting batch import with {RequestCount} requests for user {UserName}", 
                importRequests.Count(), userName);

            var result = new BatchImportResult();
            var startTime = DateTime.UtcNow;

            foreach (var request in importRequests.OrderBy(r => (int)r.Priority))
            {
                try
                {
                    var importResult = await ProcessSingleImportAsync(request, userName);
                    result.ImportResults.Add(importResult);
                    
                    result.TotalProcessed += importResult.TotalProcessed;
                    result.TotalImported += importResult.TotalImported;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process import request {RequestId}", request.Id);
                    result.Errors.Add($"Import request {request.Id}: {ex.Message}");
                }
            }

            result.Success = !result.Errors.Any();
            result.TotalDuration = DateTime.UtcNow - startTime;

            _logger.LogInformation("Completed batch import. Success: {Success}, Total Processed: {TotalProcessed}", 
                result.Success, result.TotalProcessed);

            return result;
        }

        /// <summary>
        /// Imports data from embedded test data resources
        /// </summary>
        /// <param name="dataSet">Name of the data set to import (e.g., 'ElSalvador')</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Import result summary</returns>
        [Description("Imports data from embedded test data resources")]
        public async Task<DataSetImportResult> ImportTestDataSetAsync(string dataSet, string userName)
        {
            _logger.LogInformation("Importing test data set {DataSet} for user {UserName}", dataSet, userName);

            var result = new DataSetImportResult
            {
                DataSetName = dataSet,
                Success = true
            };

            var startTime = DateTime.UtcNow;

            try
            {
                // Load available resources for the data set
                var availableFiles = Sivar.Erp.Core.Demo.TestDataResourceManager.GetAvailableResources()
                    .Where(r => r.Contains(dataSet, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var fileName in availableFiles)
                {
                    try
                    {
                        var csvContent = await Sivar.Erp.Core.Demo.TestDataResourceManager.LoadCsvAsync(fileName);
                        
                        // Determine entity type from file name
                        var entityType = DetermineEntityTypeFromFileName(fileName);
                        
                        // Create import result placeholder
                        var importResult = new ImportResult
                        {
                            Success = true,
                            EntityType = entityType,
                            TotalProcessed = csvContent.Split('\n').Length - 1, // Approximate line count
                            TotalImported = csvContent.Split('\n').Length - 1
                        };

                        result.Results[entityType] = importResult;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to import file {FileName}", fileName);
                        result.Success = false;
                        
                        result.Results[fileName] = new ImportResult
                        {
                            Success = false,
                            EntityType = fileName,
                            Errors = { ex.Message }
                        };
                    }
                }

                result.Duration = DateTime.UtcNow - startTime;
                result.Summary = $"Imported {result.Results.Count} entity types from {dataSet} data set";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import test data set {DataSet}", dataSet);
                result.Success = false;
                result.Summary = $"Failed to import {dataSet}: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Validates import data before actual import
        /// </summary>
        /// <param name="importRequest">Import request to validate</param>
        /// <returns>Validation result</returns>
        [Description("Validates import data before actual import")]
        public async Task<ImportValidationResult> ValidateImportAsync(ImportRequest importRequest)
        {
            var result = new ImportValidationResult { IsValid = true };

            try
            {
                // Basic validation
                if (string.IsNullOrEmpty(importRequest.DataSource))
                {
                    result.IsValid = false;
                    result.Errors.Add("Data source is required");
                }

                if (importRequest.EntityType == null)
                {
                    result.IsValid = false;
                    result.Errors.Add("Entity type is required");
                }

                // Validate data structure (for CSV)
                if (importRequest.DataSource.Contains('\n'))
                {
                    var lines = importRequest.DataSource.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    result.RecordCount = Math.Max(0, lines.Length - 1); // Subtract header
                    
                    if (lines.Length < 2)
                    {
                        result.Warnings.Add("No data rows found");
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Validation error: {ex.Message}");
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Gets import history for auditing purposes
        /// </summary>
        /// <param name="fromDate">Start date for history</param>
        /// <param name="toDate">End date for history</param>
        /// <param name="userName">Optional user filter</param>
        /// <returns>Collection of import history records</returns>
        [Description("Gets import history for auditing purposes")]
        public async Task<IEnumerable<ImportHistory>> GetImportHistoryAsync(DateTime fromDate, DateTime toDate, string? userName = null)
        {
            var query = _repository.GetObjects<ImportHistory>()
                .Where(h => h.CreatedAt >= fromDate && h.CreatedAt <= toDate);

            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(h => h.UserName == userName);
            }

            var history = query.OrderByDescending(h => h.CreatedAt).ToList();

            return await Task.FromResult(history);
        }

        /// <summary>
        /// Creates an import template for a specific entity type
        /// </summary>
        /// <typeparam name="T">Entity type to create template for</typeparam>
        /// <returns>CSV template with headers and sample data</returns>
        [Description("Creates an import template for a specific entity type")]
        public async Task<string> CreateImportTemplateAsync<T>() where T : class
        {
            var entityType = typeof(T);
            var properties = entityType.GetProperties()
                .Where(p => p.CanWrite && p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                .Select(p => p.Name)
                .ToArray();

            var headers = string.Join(",", properties);
            var sampleRow = string.Join(",", properties.Select(_ => "SampleValue"));

            var template = $"{headers}\n{sampleRow}";

            return await Task.FromResult(template);
        }

        /// <summary>
        /// Exports data to CSV format
        /// </summary>
        /// <typeparam name="T">Entity type to export</typeparam>
        /// <param name="filter">Optional filter criteria</param>
        /// <returns>CSV content</returns>
        [Description("Exports data to CSV format")]
        public async Task<string> ExportToCsvAsync<T>(Func<T, bool>? filter = null) where T : class
        {
            var entities = _repository.GetObjects<T>();
            
            if (filter != null)
            {
                entities = entities.Where(filter).AsQueryable();
            }

            var entityList = entities.ToList();
            
            if (!entityList.Any())
            {
                return string.Empty;
            }

            // Simple CSV export implementation
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead && (p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
                .ToArray();

            var headers = string.Join(",", properties.Select(p => p.Name));
            var rows = entityList.Select(entity =>
                string.Join(",", properties.Select(p => p.GetValue(entity)?.ToString() ?? ""))
            );

            var csv = $"{headers}\n{string.Join("\n", rows)}";

            return await Task.FromResult(csv);
        }

        /// <summary>
        /// Processes a single import request
        /// </summary>
        /// <param name="request">Import request to process</param>
        /// <param name="userName">User name</param>
        /// <returns>Import result</returns>
        private async Task<ImportResult> ProcessSingleImportAsync(ImportRequest request, string userName)
        {
            _logger.LogInformation("Processing import request {RequestId} for entity type {EntityType}", 
                request.Id, request.EntityType.Name);

            // Placeholder implementation - would delegate to specific importers
            var result = new ImportResult
            {
                Success = true,
                EntityType = request.EntityType.Name,
                TotalProcessed = 0,
                TotalImported = 0
            };

            // Record import history
            var history = _repository.CreateObject<ImportHistory>();
            history.UserName = userName;
            history.EntityType = request.EntityType.Name;
            history.ImportSource = "Manual Import";
            history.Success = result.Success;
            history.RecordsProcessed = result.TotalProcessed;
            history.RecordsImported = result.TotalImported;

            return result;
        }

        /// <summary>
        /// Determines entity type from file name
        /// </summary>
        /// <param name="fileName">File name to analyze</param>
        /// <returns>Entity type name</returns>
        private string DetermineEntityTypeFromFileName(string fileName)
        {
            var lowerFileName = fileName.ToLower();
            
            if (lowerFileName.Contains("account"))
                return "Account";
            if (lowerFileName.Contains("tax"))
                return "Tax";
            if (lowerFileName.Contains("business") || lowerFileName.Contains("entity"))
                return "BusinessEntity";
            if (lowerFileName.Contains("item"))
                return "Item";
            if (lowerFileName.Contains("document"))
                return "DocumentType";
                
            return "Unknown";
        }
    }
}