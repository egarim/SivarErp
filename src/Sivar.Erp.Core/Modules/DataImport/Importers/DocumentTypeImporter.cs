using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Modules.DataImport.Models;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.DataImport.Importers
{
    /// <summary>
    /// Specialized importer for document types
    /// </summary>
    [Description("Specialized importer for document types")]
    public class DocumentTypeImporter : IEntityImporter<IDocumentType>
    {
        private readonly ILogger<DocumentTypeImporter>? _logger;

        /// <summary>
        /// Initializes a new instance of the DocumentTypeImporter class
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public DocumentTypeImporter(ILogger<DocumentTypeImporter>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<EntityImportResult<IDocumentType>> ImportAsync(
            IRepository repository,
            string csvContent,
            string userName)
        {
            _logger?.LogInformation("Starting document type import");
            
            var result = new EntityImportResult<IDocumentType>();
            
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
                        var documentType = repository.CreateObject<DocumentTypeDto>();
                        
                        // Map fields to properties
                        for (int j = 0; j < headers.Length; j++)
                        {
                            SetDocumentTypeProperty(documentType, headers[j], fields[j]);
                        }
                        
                        // Validate document type
                        var validationErrors = ValidateDocumentType(documentType);
                        if (validationErrors.Any())
                        {
                            foreach (var error in validationErrors)
                            {
                                result.Errors.Add($"Line {i + 1}: {error}");
                            }
                            continue;
                        }
                        
                        // Set audit fields
                        documentType.CreatedAt = DateTime.UtcNow;
                        documentType.UpdatedAt = DateTime.UtcNow;
                        
                        // Add to result as IDocumentType
                        result.ImportedEntities.Add(documentType);
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
                    _logger?.LogInformation("Imported {Count} document types successfully", result.ImportedEntities.Count);
                }
                else
                {
                    _logger?.LogWarning("Document type import had {ErrorCount} errors", result.Errors.Count);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Import failed: {ex.Message}");
                _logger?.LogError(ex, "Document type import failed");
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
        
        private void SetDocumentTypeProperty(DocumentTypeDto documentType, string propertyName, string value)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "code":
                    documentType.Code = value;
                    break;
                case "name":
                    documentType.Name = value;
                    break;
                case "description":
                    documentType.Description = string.IsNullOrWhiteSpace(value) ? null : value;
                    break;
                case "generatestransaction":
                    if (bool.TryParse(value, out var generates))
                    {
                        documentType.GeneratesTransaction = generates;
                    }
                    else if (!string.IsNullOrWhiteSpace(value))
                    {
                        // Try to parse common text values
                        var lowerValue = value.ToLowerInvariant();
                        documentType.GeneratesTransaction = lowerValue is "yes" or "true" or "1" or "y";
                    }
                    break;
            }
        }
        
        private List<string> ValidateDocumentType(DocumentTypeDto documentType)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(documentType.Code))
            {
                errors.Add("Document type code is required");
            }
            
            if (string.IsNullOrWhiteSpace(documentType.Name))
            {
                errors.Add("Document type name is required");
            }
            
            return errors;
        }
    }
}