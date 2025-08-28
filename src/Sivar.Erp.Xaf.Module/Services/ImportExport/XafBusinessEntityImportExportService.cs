using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.BusinessEntities.Application.DTOs;
using Sivar.Erp.Modules.BusinessEntities.Application.Validators;
using Sivar.Erp.Core.Interfaces;
using Sivar.Erp.Modules.ImportExport;
using System.Text;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.ImportExport
{
    /// <summary>
    /// XAF implementation of business entity import/export service using IObjectSpace
    /// </summary>
    public class XafBusinessEntityImportExportService : IBusinessEntityImportExportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly BusinessEntityValidator _businessEntityValidator;
        private readonly ILogger<XafBusinessEntityImportExportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafBusinessEntityImportExportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafBusinessEntityImportExportService(IObjectSpace objectSpace, ILogger<XafBusinessEntityImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _businessEntityValidator = new BusinessEntityValidator();
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the XafBusinessEntityImportExportService class with a custom validator
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="businessEntityValidator">Custom business entity validator</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafBusinessEntityImportExportService(IObjectSpace objectSpace, BusinessEntityValidator businessEntityValidator, ILogger<XafBusinessEntityImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _businessEntityValidator = businessEntityValidator ?? new BusinessEntityValidator();
            _logger = logger;
        }

        /// <summary>
        /// Imports business entities from a CSV file using XAF ObjectSpace
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported business entities and any validation errors</returns>
        public Task<(IEnumerable<IBusinessEntity> ImportedBusinessEntities, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<BusinessEntity> importedBusinessEntities = new List<BusinessEntity>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<IBusinessEntity>, IEnumerable<string>)>((importedBusinessEntities, errors));
            }

            try
            {
                _logger?.LogInformation("Starting business entity import for user: {UserName}", userName);

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

                    // First create BusinessEntityDto from CSV fields (same as POCO implementation)
                    var businessEntityDto = CreateBusinessEntityDtoFromCsvFields(headers, fields);

                    // Validate business entity using BusinessEntityValidator
                    if (!_businessEntityValidator.ValidateBusinessEntity(businessEntityDto))
                    {
                        errors.Add($"Line {i + 1}: Business entity validation failed for entity {businessEntityDto.Code}");
                        continue;
                    }

                    // Check for duplicate business entities by Code in the database
                    var existingBusinessEntity = _objectSpace.FindObject<BusinessEntity>(CriteriaOperator.Parse("Code = ?", businessEntityDto.Code));
                    if (existingBusinessEntity != null)
                    {
                        errors.Add($"Line {i + 1}: Business entity with code '{businessEntityDto.Code}' already exists");
                        continue;
                    }

                    // Only now create the XAF entity from the validated DTO
                    var xafBusinessEntity = CreateXafBusinessEntityFromDto(businessEntityDto, userName);
                    importedBusinessEntities.Add(xafBusinessEntity);
                }

                // If there are no errors, commit the changes
                if (errors.Count == 0 && importedBusinessEntities.Count > 0)
                {
                    _objectSpace.CommitChanges();
                    _logger?.LogInformation("Successfully imported {Count} business entities", importedBusinessEntities.Count);
                }
                else if (errors.Count > 0)
                {
                    _logger?.LogWarning("Import completed with {ErrorCount} errors out of {TotalRows} rows", 
                        errors.Count, lines.Length - 1);
                }

                return Task.FromResult<(IEnumerable<IBusinessEntity>, IEnumerable<string>)>((importedBusinessEntities, errors));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error importing CSV content");
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<IBusinessEntity>, IEnumerable<string>)>((importedBusinessEntities, errors));
            }
        }

        /// <summary>
        /// Exports business entities to a CSV format
        /// </summary>
        /// <param name="businessEntities">Business entities to export (if null, exports all business entities from ObjectSpace)</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IBusinessEntity> businessEntities)
        {
            try
            {
                _logger?.LogInformation("Starting business entity export");

                // If no business entities provided, get all business entities from ObjectSpace
                var businessEntitiesToExport = businessEntities?.ToList() ?? 
                    _objectSpace.GetObjects<BusinessEntity>().Cast<IBusinessEntity>().ToList();

                if (!businessEntitiesToExport.Any())
                {
                    _logger?.LogInformation("No business entities found for export");
                    return Task.FromResult(GetCsvHeader());
                }

                StringBuilder csvBuilder = new StringBuilder();

                // Add header
                csvBuilder.AppendLine(GetCsvHeader());

                // Add data rows
                foreach (var businessEntity in businessEntitiesToExport)
                {
                    csvBuilder.AppendLine(GetCsvRow(businessEntity));
                }

                _logger?.LogInformation("Successfully exported {Count} business entities", businessEntitiesToExport.Count);
                return Task.FromResult(csvBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting business entities to CSV");
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Creates a BusinessEntityDto from CSV fields (copied from POCO implementation)
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New BusinessEntityDto with populated properties</returns>
        private BusinessEntityDto CreateBusinessEntityDtoFromCsvFields(string[] headers, string[] fields)
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
        /// Creates a XAF BusinessEntity entity from a validated BusinessEntityDto
        /// </summary>
        /// <param name="businessEntityDto">Validated BusinessEntityDto</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>New XAF BusinessEntity with populated properties</returns>
        private BusinessEntity CreateXafBusinessEntityFromDto(BusinessEntityDto businessEntityDto, string userName)
        {
            var businessEntity = _objectSpace.CreateObject<BusinessEntity>();
            var currentTime = DateTime.UtcNow;

            // Copy data from DTO to XAF entity
            businessEntity.Code = businessEntityDto.Code;
            businessEntity.Name = businessEntityDto.Name;
            businessEntity.Address = businessEntityDto.Address ?? string.Empty;
            businessEntity.City = businessEntityDto.City ?? string.Empty;
            businessEntity.State = businessEntityDto.State ?? string.Empty;
            businessEntity.ZipCode = businessEntityDto.ZipCode ?? string.Empty;
            businessEntity.Country = businessEntityDto.Country ?? string.Empty;
            businessEntity.PhoneNumber = businessEntityDto.PhoneNumber ?? string.Empty;
            businessEntity.Email = businessEntityDto.Email ?? string.Empty;

            // Set audit fields
            businessEntity.InsertedBy = userName;
            businessEntity.InsertedAt = currentTime;
            businessEntity.UpdatedBy = userName;
            businessEntity.UpdatedAt = currentTime;

            return businessEntity;
        }

        /// <summary>
        /// Parses a CSV line into fields, handling quoted values (copied from POCO implementation)
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
                    string field = line.Substring(startIndex, i - startIndex);
                    field = field.Trim('"'); // Remove surrounding quotes
                    fields.Add(field);
                    startIndex = i + 1;
                }
            }

            // Add the last field
            string lastField = line.Substring(startIndex);
            lastField = lastField.Trim('"'); // Remove surrounding quotes
            fields.Add(lastField);

            return fields.ToArray();
        }

        /// <summary>
        /// Validates CSV headers for required fields (copied from POCO implementation)
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
        /// Gets the CSV header row (copied from POCO implementation)
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "Code,Name,Address,City,State,ZipCode,Country,PhoneNumber,Email";
        }

        /// <summary>
        /// Gets a CSV row for a business entity (copied from POCO implementation)
        /// </summary>
        /// <param name="businessEntity">Business entity to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(IBusinessEntity businessEntity)
        {
            return $"\"{businessEntity.Code}\",\"{businessEntity.Name}\",\"{businessEntity.Address ?? string.Empty}\",\"{businessEntity.City ?? string.Empty}\",\"{businessEntity.State ?? string.Empty}\",\"{businessEntity.ZipCode ?? string.Empty}\",\"{businessEntity.Country ?? string.Empty}\",\"{businessEntity.PhoneNumber ?? string.Empty}\",\"{businessEntity.Email ?? string.Empty}\"";
        }

        #endregion
    }
}
