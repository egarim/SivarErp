using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Taxes.TaxGroup;
using Sivar.Erp.Modules.ImportExport;
using System.Text;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.ImportExport
{
    /// <summary>
    /// XAF implementation of tax group import/export service using IObjectSpace
    /// </summary>
    public class XafTaxGroupImportExportService : ITaxGroupImportExportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly TaxGroupValidator _taxGroupValidator;
        private readonly ILogger<XafTaxGroupImportExportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafTaxGroupImportExportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafTaxGroupImportExportService(IObjectSpace objectSpace, ILogger<XafTaxGroupImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _taxGroupValidator = new TaxGroupValidator();
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the XafTaxGroupImportExportService class with a custom validator
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="taxGroupValidator">Custom tax group validator</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafTaxGroupImportExportService(IObjectSpace objectSpace, TaxGroupValidator taxGroupValidator, ILogger<XafTaxGroupImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _taxGroupValidator = taxGroupValidator ?? new TaxGroupValidator();
            _logger = logger;
        }

        /// <summary>
        /// Imports tax groups from a CSV file using XAF ObjectSpace
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported tax groups and any validation errors</returns>
        public Task<(IEnumerable<ITaxGroup> ImportedTaxGroups, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<TaxGroup> importedTaxGroups = new List<TaxGroup>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<ITaxGroup>, IEnumerable<string>)>((importedTaxGroups, errors));
            }

            try
            {
                _logger?.LogInformation("Starting tax group import for user: {UserName}", userName);

                // Split the CSV into lines (preserve all lines)
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<ITaxGroup>, IEnumerable<string>)>((importedTaxGroups, errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<ITaxGroup>, IEnumerable<string>)>((importedTaxGroups, errors));
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

                    // First create TaxGroupDto from CSV fields (same as POCO implementation)
                    var taxGroupDto = CreateTaxGroupDtoFromCsvFields(headers, fields);

                    // Validate tax group using TaxGroupValidator
                    if (!_taxGroupValidator.ValidateTaxGroup(taxGroupDto))
                    {
                        errors.Add($"Line {i + 1}: Tax group validation failed for tax group {taxGroupDto.Name}");
                        continue;
                    }

                    // Check for duplicate tax groups by Code in the database
                    var existingTaxGroup = _objectSpace.FindObject<TaxGroup>(CriteriaOperator.Parse("Code = ?", taxGroupDto.Code));
                    if (existingTaxGroup != null)
                    {
                        errors.Add($"Line {i + 1}: Tax group with code '{taxGroupDto.Code}' already exists");
                        continue;
                    }

                    // Only now create the XAF entity from the validated DTO
                    var xafTaxGroup = CreateXafTaxGroupFromDto(taxGroupDto, userName);
                    importedTaxGroups.Add(xafTaxGroup);
                }

                // If there are no errors, commit the changes
                if (errors.Count == 0 && importedTaxGroups.Count > 0)
                {
                    _objectSpace.CommitChanges();
                    _logger?.LogInformation("Successfully imported {Count} tax groups", importedTaxGroups.Count);
                }
                else if (errors.Count > 0)
                {
                    _logger?.LogWarning("Import completed with {ErrorCount} errors out of {TotalRows} rows", 
                        errors.Count, lines.Length - 1);
                }

                return Task.FromResult<(IEnumerable<ITaxGroup>, IEnumerable<string>)>((importedTaxGroups, errors));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error importing CSV content");
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<ITaxGroup>, IEnumerable<string>)>((importedTaxGroups, errors));
            }
        }

        /// <summary>
        /// Exports tax groups to a CSV format
        /// </summary>
        /// <param name="taxGroups">Tax groups to export (if null, exports all tax groups from ObjectSpace)</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<ITaxGroup> taxGroups)
        {
            try
            {
                _logger?.LogInformation("Starting tax group export");

                // If no tax groups provided, get all tax groups from ObjectSpace
                var taxGroupsToExport = taxGroups?.ToList() ?? 
                    _objectSpace.GetObjects<TaxGroup>().Cast<ITaxGroup>().ToList();

                if (!taxGroupsToExport.Any())
                {
                    _logger?.LogInformation("No tax groups found for export");
                    return Task.FromResult(GetCsvHeader());
                }

                StringBuilder csvBuilder = new StringBuilder();

                // Add header
                csvBuilder.AppendLine(GetCsvHeader());

                // Add data rows
                foreach (var taxGroup in taxGroupsToExport)
                {
                    csvBuilder.AppendLine(GetCsvRow(taxGroup));
                }

                _logger?.LogInformation("Successfully exported {Count} tax groups", taxGroupsToExport.Count);
                return Task.FromResult(csvBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting tax groups to CSV");
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Creates a TaxGroupDto from CSV fields (copied from POCO implementation)
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New TaxGroupDto with populated properties</returns>
        private TaxGroupDto CreateTaxGroupDtoFromCsvFields(string[] headers, string[] fields)
        {
            var taxGroup = new TaxGroupDto
            {
                ID = Guid.NewGuid(),
                IsEnabled = true  // Default to enabled
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];

                switch (headers[i].ToLowerInvariant())
                {
                    case "code":
                        taxGroup.Code = value;
                        break;
                    case "name":
                        taxGroup.Name = value;
                        break;
                    case "description":
                        taxGroup.Description = value;
                        break;
                    case "isenabled":
                        if (bool.TryParse(value, out var isEnabled))
                        {
                            taxGroup.IsEnabled = isEnabled;
                        }
                        break;
                    case "grouptype":
                        // This is just for documentation in the CSV, not an actual property of TaxGroupDto
                        // But useful for users to distinguish between business entity groups and item groups
                        break;
                }
            }

            return taxGroup;
        }

        /// <summary>
        /// Creates a XAF TaxGroup entity from a validated TaxGroupDto
        /// </summary>
        /// <param name="taxGroupDto">Validated TaxGroupDto</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>New XAF TaxGroup with populated properties</returns>
        private TaxGroup CreateXafTaxGroupFromDto(TaxGroupDto taxGroupDto, string userName)
        {
            var taxGroup = _objectSpace.CreateObject<TaxGroup>();
            var currentTime = DateTime.UtcNow;

            // Copy data from DTO to XAF entity
            taxGroup.Code = taxGroupDto.Code;
            taxGroup.Name = taxGroupDto.Name;
            taxGroup.Description = taxGroupDto.Description;
            taxGroup.IsEnabled = taxGroupDto.IsEnabled;

            // Set audit fields
            taxGroup.InsertedBy = userName;
            taxGroup.InsertedAt = currentTime;
            taxGroup.UpdatedBy = userName;
            taxGroup.UpdatedAt = currentTime;

            return taxGroup;
        }

        /// <summary>
        /// Creates a XAF TaxGroup entity from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>New XAF TaxGroup with populated properties</returns>
        [Obsolete("Use CreateTaxGroupDtoFromCsvFields followed by CreateXafTaxGroupFromDto instead")]
        private TaxGroup CreateXafTaxGroupFromCsvFields(string[] headers, string[] fields, string userName)
        {
            var taxGroup = _objectSpace.CreateObject<TaxGroup>();
            var currentTime = DateTime.UtcNow;

            // Set audit fields
            taxGroup.InsertedBy = userName;
            taxGroup.InsertedAt = currentTime;
            taxGroup.UpdatedBy = userName;
            taxGroup.UpdatedAt = currentTime;
            taxGroup.IsEnabled = true; // Default to enabled

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];

                switch (headers[i].ToLowerInvariant())
                {
                    case "code":
                        taxGroup.Code = value;
                        break;
                    case "name":
                        taxGroup.Name = value;
                        break;
                    case "description":
                        taxGroup.Description = value;
                        break;
                    case "isenabled":
                        if (bool.TryParse(value, out var isEnabled))
                        {
                            taxGroup.IsEnabled = isEnabled;
                        }
                        break;
                    case "grouptype":
                        // This is just for documentation in the CSV, not an actual property of TaxGroup entity
                        break;
                }
            }

            return taxGroup;
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
            string[] requiredHeaders = { "Code", "Name", "GroupType" };

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
            return "Code,Name,Description,IsEnabled,GroupType";
        }

        /// <summary>
        /// Gets a CSV row for a tax group (copied from POCO implementation)
        /// </summary>
        /// <param name="taxGroup">Tax group to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(ITaxGroup taxGroup)
        {
            string description = string.IsNullOrWhiteSpace(taxGroup.Description)
                ? string.Empty
                : taxGroup.Description;

            // Note: GroupType is not a property of ITaxGroup, so we're leaving it empty
            // Users would need to fill this in manually or it could be determined from another source
            
            return $"\"{taxGroup.Code}\",\"{taxGroup.Name}\",\"{description}\",{taxGroup.IsEnabled},";
        }

        #endregion
    }
}
