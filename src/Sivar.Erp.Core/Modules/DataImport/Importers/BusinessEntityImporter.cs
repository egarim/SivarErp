using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Modules.DataImport.Models;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.DataImport.Importers
{
    /// <summary>
    /// Specialized importer for business entities
    /// </summary>
    [Description("Specialized importer for business entities")]
    public class BusinessEntityImporter : IEntityImporter<IBusinessEntity>
    {
        private readonly ILogger<BusinessEntityImporter>? _logger;

        /// <summary>
        /// Initializes a new instance of the BusinessEntityImporter class
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public BusinessEntityImporter(ILogger<BusinessEntityImporter>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<EntityImportResult<IBusinessEntity>> ImportAsync(
            IRepository repository,
            string csvContent,
            string userName)
        {
            _logger?.LogInformation("Starting business entity import");
            
            var result = new EntityImportResult<IBusinessEntity>();
            
            try
            {
                // Split the CSV into lines
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                
                if (lines.Length <= 1)
                {
                    result.Errors.Add("CSV file contains no data rows");
                    return result;
                }
                
                // Parse header
                string[] headers = ParseCsvLine(lines[0]);
                
                // Validate headers
                if (!ValidateHeaders(headers, result.Errors))
                {
                    return result;
                }
                
                // Process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    
                    string[] fields = ParseCsvLine(lines[i]);
                    
                    if (fields.Length != headers.Length)
                    {
                        result.Errors.Add($"Line {i + 1}: Field count mismatch. Expected {headers.Length}, got {fields.Length}");
                        continue;
                    }
                    
                    try
                    {
                        var businessEntity = repository.CreateObject<BusinessEntityDto>();
                        
                        // Map fields to properties
                        for (int j = 0; j < headers.Length; j++)
                        {
                            SetBusinessEntityProperty(businessEntity, headers[j], fields[j]);
                        }
                        
                        // Validate business entity
                        var validationErrors = ValidateBusinessEntity(businessEntity);
                        if (validationErrors.Any())
                        {
                            foreach (var error in validationErrors)
                            {
                                result.Errors.Add($"Line {i + 1}: {error}");
                            }
                            continue;
                        }
                        
                        // Set audit fields
                        businessEntity.CreatedAt = DateTime.UtcNow;
                        businessEntity.UpdatedAt = DateTime.UtcNow;
                        
                        // Add to result as IBusinessEntity
                        result.ImportedEntities.Add(businessEntity);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Line {i + 1}: {ex.Message}");
                    }
                }
                
                result.TotalProcessed = lines.Length - 1;
                result.Success = result.Errors.Count == 0;
                
                if (result.Success)
                {
                    await repository.CommitChanges();
                    _logger?.LogInformation("Imported {Count} business entities successfully", result.ImportedEntities.Count);
                }
                else
                {
                    _logger?.LogWarning("Business entity import had {ErrorCount} errors", result.Errors.Count);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Import failed: {ex.Message}");
                _logger?.LogError(ex, "Business entity import failed");
            }
            
            return result;
        }
        
        private string[] ParseCsvLine(string line)
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
                else if (c == ',' && !inQuotes)
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
        
        private bool ValidateHeaders(string[] headers, List<string> errors)
        {
            var requiredHeaders = new[] { "Code", "Name" };
            var missingHeaders = requiredHeaders.Where(h => !headers.Contains(h, StringComparer.OrdinalIgnoreCase)).ToList();
            
            if (missingHeaders.Any())
            {
                errors.Add($"Missing required headers: {string.Join(", ", missingHeaders)}");
                return false;
            }
            
            return true;
        }
        
        private void SetBusinessEntityProperty(BusinessEntityDto businessEntity, string propertyName, string value)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "code":
                    businessEntity.Code = value;
                    break;
                case "name":
                    businessEntity.Name = value;
                    break;
                case "email":
                    businessEntity.Email = string.IsNullOrWhiteSpace(value) ? null : value;
                    break;
                case "entitytype":
                    if (Enum.TryParse<BusinessEntityType>(value, true, out var entityType))
                    {
                        businessEntity.EntityType = entityType;
                    }
                    else
                    {
                        throw new FormatException($"Invalid entity type: {value}");
                    }
                    break;
            }
        }
        
        private List<string> ValidateBusinessEntity(BusinessEntityDto businessEntity)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(businessEntity.Code))
            {
                errors.Add("Business entity code is required");
            }
            
            if (string.IsNullOrWhiteSpace(businessEntity.Name))
            {
                errors.Add("Business entity name is required");
            }
            
            return errors;
        }
    }
}