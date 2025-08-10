using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Modules.DataImport.Models;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.DataImport.Importers
{
    /// <summary>
    /// Specialized importer for tax definitions
    /// </summary>
    [Description("Specialized importer for tax definitions")]
    public class TaxImporter : IEntityImporter<ITax>
    {
        private readonly ILogger<TaxImporter>? _logger;

        /// <summary>
        /// Initializes a new instance of the TaxImporter class
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public TaxImporter(ILogger<TaxImporter>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<EntityImportResult<ITax>> ImportAsync(
            IRepository repository,
            string csvContent,
            string userName)
        {
            _logger?.LogInformation("Starting tax import");
            
            var result = new EntityImportResult<ITax>();
            
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
                        var tax = repository.CreateObject<TaxDto>();
                        
                        // Map fields to properties
                        for (int j = 0; j < headers.Length; j++)
                        {
                            SetTaxProperty(tax, headers[j], fields[j]);
                        }
                        
                        // Validate tax
                        var validationErrors = ValidateTax(tax);
                        if (validationErrors.Any())
                        {
                            foreach (var error in validationErrors)
                            {
                                result.Errors.Add($"Line {i + 1}: {error}");
                            }
                            continue;
                        }
                        
                        // Set audit fields
                        tax.CreatedAt = DateTime.UtcNow;
                        tax.UpdatedAt = DateTime.UtcNow;
                        tax.IsActive = true;
                        
                        // Add to result as ITax
                        result.ImportedEntities.Add(tax);
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
                    _logger?.LogInformation("Imported {Count} taxes successfully", result.ImportedEntities.Count);
                }
                else
                {
                    _logger?.LogWarning("Tax import had {ErrorCount} errors", result.Errors.Count);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Import failed: {ex.Message}");
                _logger?.LogError(ex, "Tax import failed");
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
            var requiredHeaders = new[] { "Code", "Name", "Rate" };
            var missingHeaders = requiredHeaders.Where(h => !headers.Contains(h, StringComparer.OrdinalIgnoreCase)).ToList();
            
            if (missingHeaders.Any())
            {
                errors.Add($"Missing required headers: {string.Join(", ", missingHeaders)}");
                return false;
            }
            
            return true;
        }
        
        private void SetTaxProperty(TaxDto tax, string propertyName, string value)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "code":
                    tax.Code = value;
                    break;
                case "name":
                    tax.Name = value;
                    break;
                case "rate":
                    if (decimal.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var rate))
                    {
                        tax.Rate = rate;
                    }
                    else
                    {
                        throw new FormatException($"Invalid tax rate: {value}");
                    }
                    break;
                case "taxtype":
                    if (Enum.TryParse<TaxType>(value, true, out var taxType))
                    {
                        tax.TaxType = taxType;
                    }
                    else
                    {
                        // Default to VAT if not specified or invalid
                        tax.TaxType = TaxType.VAT;
                    }
                    break;
            }
        }
        
        private List<string> ValidateTax(TaxDto tax)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(tax.Code))
            {
                errors.Add("Tax code is required");
            }
            
            if (string.IsNullOrWhiteSpace(tax.Name))
            {
                errors.Add("Tax name is required");
            }
            
            if (tax.Rate < 0 || tax.Rate > 100)
            {
                errors.Add("Tax rate must be between 0 and 100");
            }
            
            return errors;
        }
    }
}