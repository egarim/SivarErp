using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Implementation of business entity import/export service
    /// </summary>
    public class BusinessEntityImportExportService : IBusinessEntityImportExportService
    {
        private readonly BusinessEntityValidator _businessEntityValidator;

        /// <summary>
        /// Initializes a new instance of the BusinessEntityImportExportService class
        /// </summary>
        public BusinessEntityImportExportService()
        {
            _businessEntityValidator = new BusinessEntityValidator();
        }

        /// <summary>
        /// Initializes a new instance of the BusinessEntityImportExportService class with a custom validator
        /// </summary>
        /// <param name="businessEntityValidator">Custom business entity validator</param>
        public BusinessEntityImportExportService(BusinessEntityValidator businessEntityValidator)
        {
            _businessEntityValidator = businessEntityValidator ?? new BusinessEntityValidator();
        }

        /// <summary>
        /// Imports business entities from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported business entities and any validation errors</returns>
        public Task<(IEnumerable<IBusinessEntity> ImportedBusinessEntities, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<BusinessEntityDto> importedBusinessEntities = new List<BusinessEntityDto>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<IBusinessEntity>, IEnumerable<string>)>((importedBusinessEntities, errors));
            }

            try
            {
                // Split the CSV into lines (preserve all lines)
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<IBusinessEntity>, IEnumerable<string>)>((importedBusinessEntities, errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<IBusinessEntity>, IEnumerable<string>)>((importedBusinessEntities, errors));
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

                    var businessEntity = CreateBusinessEntityFromCsvFields(headers, fields);

                    // Validate business entity
                    if (!_businessEntityValidator.ValidateBusinessEntity(businessEntity))
                    {
                        errors.Add($"Line {i + 1}: Business entity validation failed for entity {businessEntity.Code}");
                        continue;
                    }

                    importedBusinessEntities.Add(businessEntity);
                }

                return Task.FromResult<(IEnumerable<IBusinessEntity>, IEnumerable<string>)>((importedBusinessEntities, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<IBusinessEntity>, IEnumerable<string>)>((importedBusinessEntities, errors));
            }
        }

        /// <summary>
        /// Exports business entities to a CSV format
        /// </summary>
        /// <param name="businessEntities">Business entities to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IBusinessEntity> businessEntities)
        {
            if (businessEntities == null || !businessEntities.Any())
            {
                return Task.FromResult(GetCsvHeader());
            }

            StringBuilder csvBuilder = new StringBuilder();

            // Add header
            csvBuilder.AppendLine(GetCsvHeader());

            // Add data rows
            foreach (var businessEntity in businessEntities)
            {
                csvBuilder.AppendLine(GetCsvRow(businessEntity));
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
            string[] requiredHeaders = { "Code", "Name" };

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
        /// Creates a business entity from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New business entity with populated properties</returns>
        private BusinessEntityDto CreateBusinessEntityFromCsvFields(string[] headers, string[] fields)
        {
            var businessEntity = new BusinessEntityDto
            {
                Oid = Guid.NewGuid()
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];
                if (string.IsNullOrWhiteSpace(value)) continue;

                switch (headers[i].ToLowerInvariant())
                {
                    case "code":
                        businessEntity.Code = value;
                        break;
                    case "name":
                        businessEntity.Name = value;
                        break;
                    case "address":
                        businessEntity.Address = value;
                        break;
                    case "city":
                        businessEntity.City = value;
                        break;
                    case "state":
                        businessEntity.State = value;
                        break;
                    case "zipcode":
                        businessEntity.ZipCode = value;
                        break;
                    case "country":
                        businessEntity.Country = value;
                        break;
                    case "phonenumber":
                        businessEntity.PhoneNumber = value;
                        break;
                    case "email":
                        businessEntity.Email = value;
                        break;
                }
            }

            return businessEntity;
        }

        /// <summary>
        /// Gets the CSV header row
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "Code,Name,Address,City,State,ZipCode,Country,PhoneNumber,Email";
        }

        /// <summary>
        /// Gets a CSV row for a business entity
        /// </summary>
        /// <param name="businessEntity">Business entity to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(IBusinessEntity businessEntity)
        {
            return $"\"{businessEntity.Code}\",\"{businessEntity.Name}\",\"{businessEntity.Address ?? string.Empty}\",\"{businessEntity.City ?? string.Empty}\",\"{businessEntity.State ?? string.Empty}\",\"{businessEntity.ZipCode ?? string.Empty}\",\"{businessEntity.Country ?? string.Empty}\",\"{businessEntity.PhoneNumber ?? string.Empty}\",\"{businessEntity.Email ?? string.Empty}\"";
        }
    }
}