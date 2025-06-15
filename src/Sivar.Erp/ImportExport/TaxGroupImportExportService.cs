using Sivar.Erp.Taxes.TaxGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.ImportExport
{
    /// <summary>
    /// Implementation of tax group import/export service
    /// </summary>
    public class TaxGroupImportExportService : ITaxGroupImportExportService
    {
        private readonly TaxGroupValidator _taxGroupValidator;

        /// <summary>
        /// Initializes a new instance of the TaxGroupImportExportService class
        /// </summary>
        public TaxGroupImportExportService()
        {
            _taxGroupValidator = new TaxGroupValidator();
        }

        /// <summary>
        /// Initializes a new instance of the TaxGroupImportExportService class with a custom validator
        /// </summary>
        /// <param name="taxGroupValidator">Custom tax group validator</param>
        public TaxGroupImportExportService(TaxGroupValidator taxGroupValidator)
        {
            _taxGroupValidator = taxGroupValidator ?? new TaxGroupValidator();
        }

        /// <summary>
        /// Imports tax groups from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported tax groups and any validation errors</returns>
        public Task<(IEnumerable<ITaxGroup> ImportedTaxGroups, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<TaxGroupDto> importedTaxGroups = new List<TaxGroupDto>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<ITaxGroup>, IEnumerable<string>)>((importedTaxGroups, errors));
            }

            try
            {
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

                    var taxGroup = CreateTaxGroupFromCsvFields(headers, fields);

                    // Validate tax group
                    if (!_taxGroupValidator.ValidateTaxGroup(taxGroup))
                    {
                        errors.Add($"Line {i + 1}: Tax group validation failed for {taxGroup.Name}");
                        continue;
                    }

                    importedTaxGroups.Add(taxGroup);
                }

                return Task.FromResult<(IEnumerable<ITaxGroup>, IEnumerable<string>)>((importedTaxGroups, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<ITaxGroup>, IEnumerable<string>)>((importedTaxGroups, errors));
            }
        }

        /// <summary>
        /// Exports tax groups to a CSV format
        /// </summary>
        /// <param name="taxGroups">Tax groups to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<ITaxGroup> taxGroups)
        {
            if (taxGroups == null || !taxGroups.Any())
            {
                return Task.FromResult(GetCsvHeader());
            }

            StringBuilder csvBuilder = new StringBuilder();

            // Add header
            csvBuilder.AppendLine(GetCsvHeader());

            // Add data rows
            foreach (var taxGroup in taxGroups)
            {
                csvBuilder.AppendLine(GetCsvRow(taxGroup));
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
        /// Creates a tax group from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New tax group with populated properties</returns>
        private TaxGroupDto CreateTaxGroupFromCsvFields(string[] headers, string[] fields)
        {
            var taxGroup = new TaxGroupDto
            {
                Oid = Guid.NewGuid(),
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
        /// Gets the CSV header row
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "Code,Name,Description,IsEnabled,GroupType";
        }

        /// <summary>
        /// Gets a CSV row for a tax group
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
    }
}