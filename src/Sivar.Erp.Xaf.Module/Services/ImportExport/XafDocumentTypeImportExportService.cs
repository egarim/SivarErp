using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Documents.Core.Interfaces;
using Sivar.Erp.Infrastructure.ImportExport.Documents;
using System.Text;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.ImportExport
{
    /// <summary>
    /// XAF implementation of document type import/export service using IObjectSpace
    /// </summary>
    public class XafDocumentTypeImportExportService : IDocumentTypeImportExportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly ILogger<XafDocumentTypeImportExportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafDocumentTypeImportExportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafDocumentTypeImportExportService(IObjectSpace objectSpace, ILogger<XafDocumentTypeImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _logger = logger;
        }

        /// <summary>
        /// Imports document types from a CSV file using XAF ObjectSpace
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported document types and any validation errors</returns>
        public Task<(IEnumerable<IDocumentType> ImportedDocumentTypes, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            var importedDocumentTypes = new List<IDocumentType>();
            var errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<IDocumentType>, IEnumerable<string>)>((importedDocumentTypes, errors));
            }

            try
            {
                _logger?.LogInformation("Starting document type import from CSV for user: {UserName}", userName);

                // Split the CSV into lines (preserve all lines)
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<IDocumentType>, IEnumerable<string>)>((importedDocumentTypes, errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<IDocumentType>, IEnumerable<string>)>((importedDocumentTypes, errors));
                }

                // Process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue; // Skip empty lines
                    string[] fields = ParseCsvLine(lines[i]);

                    if (fields.Length != headers.Length)
                    {
                        errors.Add($"Line {i + 1}: Column count mismatch. Expected {headers.Length}, got {fields.Length}");
                        continue;
                    }

                    try
                    {
                        var documentTypeDto = CreateDocumentTypeFromCsvFields(headers, fields);

                        // Validate document type
                        if (!ValidateDocumentType(documentTypeDto, errors, i + 1))
                        {
                            continue;
                        }

                        // Check if document type already exists
                        var existingDocumentType = _objectSpace.FindObject<DocumentType>(
                            CriteriaOperator.Parse("Code = ?", documentTypeDto.Code));

                        DocumentType documentType;
                        if (existingDocumentType != null)
                        {
                            _logger?.LogInformation("Updating existing document type with code: {Code}", documentTypeDto.Code);
                            documentType = existingDocumentType;
                        }
                        else
                        {
                            _logger?.LogInformation("Creating new document type with code: {Code}", documentTypeDto.Code);
                            documentType = _objectSpace.CreateObject<DocumentType>();
                        }

                        // Update properties
                        UpdateDocumentTypeFromDto(documentType, documentTypeDto, userName);

                        importedDocumentTypes.Add(documentType);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Line {i + 1}: Error processing document type - {ex.Message}");
                        _logger?.LogError(ex, "Error processing line {LineNumber} during document type import", i + 1);
                    }
                }

                // Save changes to ObjectSpace
                if (importedDocumentTypes.Any() && !errors.Any())
                {
                    try
                    {
                        _objectSpace.CommitChanges();
                        _logger?.LogInformation("Successfully imported {Count} document types", importedDocumentTypes.Count);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error saving changes to database: {ex.Message}");
                        _logger?.LogError(ex, "Error saving document types to database");
                    }
                }

                return Task.FromResult<(IEnumerable<IDocumentType>, IEnumerable<string>)>((importedDocumentTypes, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                _logger?.LogError(ex, "Unexpected error during document type import");
                return Task.FromResult<(IEnumerable<IDocumentType>, IEnumerable<string>)>((importedDocumentTypes, errors));
            }
        }

        /// <summary>
        /// Exports document types to a CSV format using XAF ObjectSpace
        /// </summary>
        /// <param name="documentTypes">Document types to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IDocumentType> documentTypes)
        {
            try
            {
                _logger?.LogInformation("Starting document type export to CSV");

                if (documentTypes == null || !documentTypes.Any())
                {
                    _logger?.LogInformation("No document types provided for export, returning header only");
                    return Task.FromResult(GetCsvHeader());
                }

                StringBuilder csvBuilder = new StringBuilder();

                // Add header
                csvBuilder.AppendLine(GetCsvHeader());

                // Add data rows
                foreach (var documentType in documentTypes)
                {
                    csvBuilder.AppendLine(GetCsvRow(documentType));
                }

                _logger?.LogInformation("Successfully exported {Count} document types to CSV", documentTypes.Count());
                return Task.FromResult(csvBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during document type export");
                throw;
            }
        }

        /// <summary>
        /// Parses a CSV line into fields, handling quoted values
        /// </summary>
        /// <param name="line">CSV line to parse</param>
        /// <returns>Array of fields</returns>
        private string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            var field = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        field.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // Field separator
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            // Add last field
            fields.Add(field.ToString());

            return fields.ToArray();
        }

        /// <summary>
        /// Validates CSV headers for required fields
        /// </summary>
        /// <param name="headers">Array of header names</param>
        /// <param name="errors">Collection to add any validation errors to</param>
        /// <returns>True if headers are valid, false otherwise</returns>
        private bool ValidateHeaders(string[] headers, List<string> errors)
        {
            // Define required headers
            string[] requiredHeaders = { "Code", "Name", "DocumentOperation" };

            foreach (var requiredHeader in requiredHeaders)
            {
                if (!headers.Contains(requiredHeader, StringComparer.OrdinalIgnoreCase))
                {
                    errors.Add($"Required header '{requiredHeader}' is missing");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates a document type DTO
        /// </summary>
        /// <param name="documentType">Document type to validate</param>
        /// <param name="errors">Collection to add validation errors to</param>
        /// <param name="lineNumber">Line number being processed for error reporting</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateDocumentType(DocumentTypeData documentType, List<string> errors, int lineNumber)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(documentType.Code))
            {
                errors.Add($"Line {lineNumber}: Document type code is required");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(documentType.Name))
            {
                errors.Add($"Line {lineNumber}: Document type name is required");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(documentType.DocumentOperation))
            {
                errors.Add($"Line {lineNumber}: Document operation is required");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Creates a document type DTO from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New document type with populated properties</returns>
        private DocumentTypeData CreateDocumentTypeFromCsvFields(string[] headers, string[] fields)
        {
            var documentType = new DocumentTypeData
            {
                IsEnabled = true
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i].Trim();

                switch (headers[i].ToLowerInvariant())
                {
                    case "code":
                        documentType.Code = value;
                        break;
                    case "name":
                        documentType.Name = value;
                        break;
                    case "isenabled":
                        if (bool.TryParse(value, out var isEnabled))
                        {
                            documentType.IsEnabled = isEnabled;
                        }
                        break;
                    case "documentoperation":
                        documentType.DocumentOperation = value;
                        break;
                }
            }

            return documentType;
        }

        /// <summary>
        /// Updates a DocumentType entity from DTO data
        /// </summary>
        /// <param name="documentType">DocumentType entity to update</param>
        /// <param name="dto">DTO with updated data</param>
        /// <param name="userName">User performing the update</param>
        private void UpdateDocumentTypeFromDto(DocumentType documentType, DocumentTypeData dto, string userName)
        {
            var currentTime = DateTime.UtcNow;
            
            documentType.Code = dto.Code;
            documentType.Name = dto.Name;
            documentType.IsEnabled = dto.IsEnabled;
            documentType.DocumentOperation = dto.DocumentOperation;

            // Set audit fields - XAF automatically handles the ID/Oid
            documentType.InsertedBy = userName;
            documentType.InsertedAt = currentTime;
            documentType.UpdatedBy = userName;
            documentType.UpdatedAt = currentTime;
        }

        /// <summary>
        /// Gets the CSV header row
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "Code,Name,DocumentOperation,IsEnabled";
        }

        /// <summary>
        /// Gets a CSV row for a document type
        /// </summary>
        /// <param name="documentType">Document type to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(IDocumentType documentType)
        {
            return $"\"{documentType.Code}\",\"{documentType.Name}\",{documentType.DocumentOperation},{documentType.IsEnabled}";
        }

        /// <summary>
        /// Internal data structure for processing document type data during import
        /// </summary>
        private class DocumentTypeData
        {
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public bool IsEnabled { get; set; } = true;
            public string DocumentOperation { get; set; } = string.Empty;
        }
    }
}
