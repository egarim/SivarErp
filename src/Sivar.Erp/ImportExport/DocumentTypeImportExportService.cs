using Sivar.Erp.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.ImportExport
{
    /// <summary>
    /// Implementation of document type import/export service
    /// </summary>
    public class DocumentTypeImportExportService : IDocumentTypeImportExportService
    {
        /// <summary>
        /// Initializes a new instance of the DocumentTypeImportExportService class
        /// </summary>
        public DocumentTypeImportExportService()
        {
        }

        /// <summary>
        /// Imports document types from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported document types and any validation errors</returns>
        public Task<(IEnumerable<IDocumentType> ImportedDocumentTypes, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<DocumentTypeDto> importedDocumentTypes = new List<DocumentTypeDto>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<IDocumentType>, IEnumerable<string>)>((importedDocumentTypes, errors));
            }

            try
            {
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

                    var documentType = CreateDocumentTypeFromCsvFields(headers, fields);

                    // Validate document type
                    if (!ValidateDocumentType(documentType))
                    {
                        errors.Add($"Line {i + 1}: Document type validation failed for document type {documentType.Name}");
                        continue;
                    }

                    importedDocumentTypes.Add(documentType);
                }

                return Task.FromResult<(IEnumerable<IDocumentType>, IEnumerable<string>)>((importedDocumentTypes, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<IDocumentType>, IEnumerable<string>)>((importedDocumentTypes, errors));
            }
        }

        /// <summary>
        /// Exports document types to a CSV format
        /// </summary>
        /// <param name="documentTypes">Document types to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IDocumentType> documentTypes)
        {
            if (documentTypes == null || !documentTypes.Any())
            {
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

            return Task.FromResult(csvBuilder.ToString());
        }

        /// <summary>
        /// Parses a CSV line into fields, handling quoted values
        /// </summary>
        /// <param name="line">CSV line to parse</param>
        /// <returns>Array of fields</returns>
        private string[] ParseCsvLine(string line)
        {
            List<string> fields = new List<string>();
            bool inQuotes = false;
            int startIndex = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (line[i] == ',' && !inQuotes)
                {
                    fields.Add(line.Substring(startIndex, i - startIndex).Trim().TrimStart('"').TrimEnd('"'));
                    startIndex = i + 1;
                }
            }

            // Add the last field
            fields.Add(line.Substring(startIndex).Trim().TrimStart('"').TrimEnd('"'));

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
        /// Validates a document type
        /// </summary>
        /// <param name="documentType">Document type to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateDocumentType(DocumentTypeDto documentType)
        {
            if (string.IsNullOrWhiteSpace(documentType.Code))
                return false;

            if (string.IsNullOrWhiteSpace(documentType.Name))
                return false;

            return true;
        }

        /// <summary>
        /// Creates a document type from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New document type with populated properties</returns>
        private DocumentTypeDto CreateDocumentTypeFromCsvFields(string[] headers, string[] fields)
        {
            var documentType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                IsEnabled = true
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];

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
                        if (Enum.TryParse<DocumentOperation>(value, true, out var documentOperation))
                        {
                            documentType.DocumentOperation = documentOperation;
                        }
                        else
                        {
                            // Default to PurchaseOrder if invalid
                            documentType.DocumentOperation = DocumentOperation.PurchaseOrder;
                        }
                        break;
                }
            }

            return documentType;
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
    }
}
