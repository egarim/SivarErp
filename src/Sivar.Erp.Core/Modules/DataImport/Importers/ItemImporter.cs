using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Modules.DataImport.Models;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.DataImport.Importers
{
    /// <summary>
    /// Specialized importer for items/products
    /// </summary>
    [Description("Specialized importer for items/products")]
    public class ItemImporter : IEntityImporter<ItemDto>
    {
        private readonly ILogger<ItemImporter>? _logger;

        /// <summary>
        /// Initializes a new instance of the ItemImporter class
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public ItemImporter(ILogger<ItemImporter>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<EntityImportResult<ItemDto>> ImportAsync(
            IRepository repository,
            string csvContent,
            string userName)
        {
            _logger?.LogInformation("Starting item import");
            
            var result = new EntityImportResult<ItemDto>();
            
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
                        var item = repository.CreateObject<ItemDto>();
                        
                        // Map fields to properties
                        for (int j = 0; j < headers.Length; j++)
                        {
                            SetItemProperty(item, headers[j], fields[j]);
                        }
                        
                        // Validate item
                        var validationErrors = ValidateItem(item);
                        if (validationErrors.Any())
                        {
                            foreach (var error in validationErrors)
                            {
                                result.Errors.Add($"Line {i + 1}: {error}");
                            }
                            continue;
                        }
                        
                        // Set audit fields
                        item.CreatedAt = DateTime.UtcNow;
                        item.UpdatedAt = DateTime.UtcNow;
                        item.IsActive = true;
                        
                        // Add to result
                        result.ImportedEntities.Add(item);
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
                    _logger?.LogInformation("Imported {Count} items successfully", result.ImportedEntities.Count);
                }
                else
                {
                    _logger?.LogWarning("Item import had {ErrorCount} errors", result.Errors.Count);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Import failed: {ex.Message}");
                _logger?.LogError(ex, "Item import failed");
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
        
        private void SetItemProperty(ItemDto item, string propertyName, string value)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "code":
                    item.Code = value;
                    break;
                case "name":
                    item.Name = value;
                    break;
                case "description":
                    item.Description = string.IsNullOrWhiteSpace(value) ? null : value;
                    break;
                case "unitprice":
                    if (decimal.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var price))
                    {
                        item.UnitPrice = price;
                    }
                    else if (!string.IsNullOrWhiteSpace(value))
                    {
                        throw new FormatException($"Invalid unit price: {value}");
                    }
                    break;
                case "unitofmeasure":
                    item.UnitOfMeasure = string.IsNullOrWhiteSpace(value) ? "UNIT" : value;
                    break;
                case "category":
                    item.Category = string.IsNullOrWhiteSpace(value) ? null : value;
                    break;
            }
        }
        
        private List<string> ValidateItem(ItemDto item)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(item.Code))
            {
                errors.Add("Item code is required");
            }
            
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                errors.Add("Item name is required");
            }
            
            if (item.UnitPrice < 0)
            {
                errors.Add("Unit price cannot be negative");
            }
            
            return errors;
        }
    }
}