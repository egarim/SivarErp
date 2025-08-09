using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.DataImport.Importers;
using Sivar.Erp.Core.Modules.DataImport.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Sivar.Erp.Core.Modules.DataImport
{
    /// <summary>
    /// Implementation of the CSV import service
    /// </summary>
    [Description("Implementation of CSV import functionality")]
    public class CsvImportService : ICsvImportService
    {
        private readonly IRepository _repository;
        private readonly ILogger<CsvImportService> _logger;
        private readonly IServiceProvider _serviceProvider;
        
        /// <summary>
        /// Initializes a new instance of the CsvImportService class
        /// </summary>
        /// <param name="repository">The repository for data access</param>
        /// <param name="logger">The logger for diagnostic information</param>
        /// <param name="serviceProvider">The service provider for resolving importers</param>
        public CsvImportService(
            IRepository repository, 
            ILogger<CsvImportService> logger,
            IServiceProvider serviceProvider)
        {
            _repository = repository;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        
        /// <inheritdoc/>
        public async Task<CsvImportResult<T>> ImportFromCsvAsync<T>(
            string csvContent, 
            string userName, 
            CsvImportOptions<T>? options = null) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new CsvImportResult<T>();
            
            try
            {
                _logger.LogInformation("Starting CSV import for type {EntityType}", typeof(T).Name);
                
                // Try to get entity-specific importer
                var importer = ResolveImporter<T>();
                
                if (importer != null)
                {
                    _logger.LogInformation("Using specialized importer {ImporterType} for {EntityType}", 
                        importer.GetType().Name, typeof(T).Name);
                    
                    // Use the specialized importer
                    var importResult = await importer.ImportAsync(_repository, csvContent, userName);
                    
                    // Convert to CsvImportResult
                    result.Success = importResult.Success;
                    result.Errors = importResult.Errors;
                    result.ImportedEntities = importResult.ImportedEntities.Cast<T>().ToList();
                    result.TotalProcessed = importResult.TotalProcessed;
                }
                else
                {
                    _logger.LogInformation("No specialized importer found for {EntityType}", typeof(T).Name);
                    
                    // Check if T is concrete and has a parameterless constructor
                    if (!typeof(T).IsAbstract && typeof(T).GetConstructor(Type.EmptyTypes) != null)
                    {
                        _logger.LogInformation("Using generic import for {EntityType}", typeof(T).Name);
                        // Use reflection to call the generic import method
                        var importMethod = typeof(CsvImportService).GetMethod(
                            nameof(ImportGenericAsync), 
                            BindingFlags.NonPublic | BindingFlags.Instance);
                            
                        var genericMethod = importMethod?.MakeGenericMethod(typeof(T));
                        
                        if (genericMethod != null)
                        {
                            await (Task)genericMethod.Invoke(this, new object?[] { csvContent, userName, options, result })!;
                        }
                        else
                        {
                            _logger.LogError("Failed to create generic method for {EntityType}", typeof(T).Name);
                            result.Success = false;
                            result.Errors.Add($"Failed to create generic import method for {typeof(T).Name}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Cannot use generic import for {EntityType} because it's abstract or has no parameterless constructor", typeof(T).Name);
                        result.Success = false;
                        result.Errors.Add($"No specialized importer found for {typeof(T).Name} and generic import is not possible");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing CSV data for type {EntityType}", typeof(T).Name);
                result.Success = false;
                result.Errors.Add($"Import failed: {ex.Message}");
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                _logger.LogInformation("CSV import for {EntityType} completed in {Duration}ms with {EntityCount} entities and {ErrorCount} errors", 
                    typeof(T).Name, stopwatch.ElapsedMilliseconds, result.ImportedEntities.Count, result.Errors.Count);
            }
            
            return result;
        }

        private IEntityImporter<T>? ResolveImporter<T>() where T : class
        {
            // Try to get specific importer from DI
            var importerType = typeof(IEntityImporter<>).MakeGenericType(typeof(T));
            var importer = _serviceProvider.GetService(importerType);
            
            if (importer != null)
            {
                return importer as IEntityImporter<T>;
            }
            
            return null;
        }
        
        private async Task ImportGenericAsync<T>(
            string csvContent, 
            string userName, 
            CsvImportOptions<T>? options,
            CsvImportResult<T> result) where T : class, new()
        {
            // Split the CSV into lines
            string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length <= 1)
            {
                result.Errors.Add("CSV file contains no data rows");
                return;
            }
            
            // Get header row index
            int headerRowIndex = options?.HeaderRowIndex ?? 0;
            if (headerRowIndex >= lines.Length)
            {
                result.Errors.Add($"Header row index {headerRowIndex} is out of range. CSV has {lines.Length} lines.");
                return;
            }
            
            // Parse headers
            string[] headers = ParseCsvLine(lines[headerRowIndex], options?.Delimiter ?? ',');
            
            // Process data rows
            for (int i = headerRowIndex + 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line) && options?.SkipEmptyRows != false)
                {
                    continue;
                }
                
                try
                {
                    // Parse line
                    string[] fields = ParseCsvLine(line, options?.Delimiter ?? ',');
                    
                    if (fields.Length != headers.Length)
                    {
                        result.Errors.Add($"Line {i + 1}: Field count mismatch. Expected {headers.Length}, got {fields.Length}");
                        if (options?.ContinueOnError != true)
                        {
                            break;
                        }
                        continue;
                    }
                    
                    // Create new entity
                    T entity = _repository.CreateObject<T>();
                    
                    // Map fields to properties
                    for (int j = 0; j < headers.Length; j++)
                    {
                        string propertyName = options?.FieldMapper?.Invoke(headers[j]) ?? headers[j];
                        
                        // Skip empty fields
                        if (string.IsNullOrWhiteSpace(fields[j]))
                        {
                            continue;
                        }
                        
                        // Get property
                        PropertyInfo? property = typeof(T).GetProperty(propertyName, 
                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        
                        if (property == null)
                        {
                            _logger.LogWarning("Property {PropertyName} not found on type {EntityType}", 
                                propertyName, typeof(T).Name);
                            continue;
                        }
                        
                        try
                        {
                            // Convert value
                            object? value;
                            if (options?.ValueConverter != null)
                            {
                                value = options.ValueConverter(propertyName, fields[j]);
                            }
                            else
                            {
                                value = ConvertValue(fields[j], property.PropertyType);
                            }
                            
                            // Set property value
                            property.SetValue(entity, value);
                        }
                        catch (Exception ex)
                        {
                            string error = $"Line {i + 1}, Column {propertyName}: {ex.Message}";
                            result.Errors.Add(error);
                            if (options?.ContinueOnError != true)
                            {
                                throw new FormatException(error, ex);
                            }
                        }
                    }
                    
                    // Validate entity
                    if (options?.Validator != null)
                    {
                        var validationErrors = options.Validator(entity);
                        if (validationErrors.Any())
                        {
                            foreach (var error in validationErrors)
                            {
                                result.Errors.Add($"Line {i + 1}: {error}");
                            }
                            
                            if (options.ContinueOnError != true)
                            {
                                break;
                            }
                            continue;
                        }
                    }
                    
                    // Add to result
                    result.ImportedEntities.Add(entity);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Line {i + 1}: {ex.Message}");
                    if (options?.ContinueOnError != true)
                    {
                        break;
                    }
                }
            }
            
            result.TotalProcessed = lines.Length - 1 - headerRowIndex;
            result.Success = result.Errors.Count == 0;
            
            if (result.Success && result.ImportedEntities.Any())
            {
                await _repository.CommitChanges();
            }
        }
        
        private string[] ParseCsvLine(string line, char delimiter)
        {
            var result = new List<string>();
            var inQuotes = false;
            var field = new System.Text.StringBuilder();
            
            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }
            
            result.Add(field.ToString());
            return result.ToArray();
        }
        
        private object? ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            
            if (targetType == typeof(string))
            {
                return value;
            }
            else if (targetType == typeof(int) || targetType == typeof(int?))
            {
                return int.Parse(value);
            }
            else if (targetType == typeof(decimal) || targetType == typeof(decimal?))
            {
                return decimal.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (targetType == typeof(double) || targetType == typeof(double?))
            {
                return double.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                return DateTime.Parse(value);
            }
            else if (targetType == typeof(DateOnly) || targetType == typeof(DateOnly?))
            {
                return DateOnly.Parse(value);
            }
            else if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                return bool.Parse(value);
            }
            else if (targetType == typeof(Guid) || targetType == typeof(Guid?))
            {
                return Guid.Parse(value);
            }
            else if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value, ignoreCase: true);
            }
            
            // Default to string conversion
            return Convert.ChangeType(value, targetType);
        }
    }
}