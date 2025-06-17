using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.Taxes.TaxRule;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Implementation of tax rule import/export service
    /// </summary>
    public class TaxRuleImportExportService : ITaxRuleImportExportService
    {
        private readonly TaxRuleValidator _taxRuleValidator;

        /// <summary>
        /// Initializes a new instance of the TaxRuleImportExportService class
        /// </summary>
        public TaxRuleImportExportService()
        {
            _taxRuleValidator = new TaxRuleValidator();
        }

        /// <summary>
        /// Initializes a new instance of the TaxRuleImportExportService class with a custom validator
        /// </summary>
        /// <param name="taxRuleValidator">Custom tax rule validator</param>
        public TaxRuleImportExportService(TaxRuleValidator taxRuleValidator)
        {
            _taxRuleValidator = taxRuleValidator ?? new TaxRuleValidator();
        }

        /// <summary>
        /// Imports tax rules from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported tax rules and any validation errors</returns>
        public Task<(IEnumerable<ITaxRule> ImportedTaxRules, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<TaxRuleDto> importedTaxRules = new List<TaxRuleDto>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<ITaxRule>, IEnumerable<string>)>((importedTaxRules.Cast<ITaxRule>(), errors));
            }

            try
            {
                // Split the CSV into lines (preserve all lines)
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<ITaxRule>, IEnumerable<string>)>((importedTaxRules.Cast<ITaxRule>(), errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<ITaxRule>, IEnumerable<string>)>((importedTaxRules.Cast<ITaxRule>(), errors));
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

                    var taxRule = CreateTaxRuleFromCsvFields(headers, fields);

                    // Validate tax rule
                    if (!_taxRuleValidator.ValidateTaxRule(taxRule))
                    {
                        errors.Add($"Line {i + 1}: Tax rule validation failed");
                        continue;
                    }

                    importedTaxRules.Add(taxRule);
                }

                return Task.FromResult<(IEnumerable<ITaxRule>, IEnumerable<string>)>((importedTaxRules.Cast<ITaxRule>(), errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<ITaxRule>, IEnumerable<string>)>((importedTaxRules.Cast<ITaxRule>(), errors));
            }
        }

        /// <summary>
        /// Exports tax rules to a CSV format
        /// </summary>
        /// <param name="taxRules">Tax rules to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<ITaxRule> taxRules)
        {
            if (taxRules == null || !taxRules.Any())
            {
                return Task.FromResult(GetCsvHeader());
            }

            StringBuilder csvBuilder = new StringBuilder();

            // Add header
            csvBuilder.AppendLine(GetCsvHeader());

            // Add data rows
            foreach (var taxRule in taxRules)
            {
                csvBuilder.AppendLine(GetCsvRow(taxRule));
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
            string[] requiredHeaders = { "TaxCode", "DocumentOperation", "Priority", "IsEnabled" };

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
        /// Creates a tax rule from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New tax rule with populated properties</returns>
        private TaxRuleDto CreateTaxRuleFromCsvFields(string[] headers, string[] fields)
        {
            var taxRule = new TaxRuleDto
            {
                Oid = Guid.NewGuid(),
                IsEnabled = true,
                Priority = 1
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                switch (headers[i].ToLowerInvariant())
                {
                    case "taxcode":
                        // TaxCode would need to be resolved to TaxId in the actual application
                        // For now, we'll create a deterministic Guid based on the tax code
                        taxRule.TaxId = value;
                        break;
                    case "documentoperation":
                        if (Enum.TryParse<DocumentOperation>(value, true, out var documentOperation))
                        {
                            taxRule.DocumentOperation = documentOperation;
                        }
                        break;
                    case "businessentitygroupcode":
                        // BusinessEntityGroupCode would need to be resolved to BusinessEntityGroupId
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            taxRule.BusinessEntityGroupId = value;
                        }
                        break;
                    case "itemgroupcode":
                        // ItemGroupCode would need to be resolved to ItemGroupId
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            taxRule.ItemGroupId = value;
                        }
                        break;
                    case "isenabled":
                        if (bool.TryParse(value, out bool isEnabled))
                        {
                            taxRule.IsEnabled = isEnabled;
                        }
                        break;
                    case "priority":
                        if (int.TryParse(value, out int priority))
                        {
                            taxRule.Priority = priority;
                        }
                        break;
                }
            }

            return taxRule;
        }

       

        /// <summary>
        /// Gets the CSV header row
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "TaxCode,DocumentOperation,BusinessEntityGroupCode,ItemGroupCode,IsEnabled,Priority";
        }

        /// <summary>
        /// Gets a CSV row for a tax rule
        /// </summary>
        /// <param name="taxRule">Tax rule to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(ITaxRule taxRule)
        {
            // Note: In a real implementation, tax code, business entity group code, and item group code
            // would need to be looked up from their respective repositories based on their IDs.
            // For this example, we'll use placeholder values.

            string taxCode = "TAX_" + taxRule.TaxId.ToString().Substring(0, 8);
            string documentOperation = taxRule.DocumentOperation?.ToString() ?? "";
            string businessEntityGroupCode =!string.IsNullOrEmpty(taxRule.BusinessEntityGroupId) ? 
                "BEG_" + taxRule.BusinessEntityGroupId.Substring(0, 8) : "";
            string itemGroupCode = !string.IsNullOrEmpty(taxRule.ItemGroupId) ? 
                "IG_" + taxRule.ItemGroupId.Substring(0, 8) : "";

            return $"{taxCode},{documentOperation},{businessEntityGroupCode},{itemGroupCode},{taxRule.IsEnabled},{taxRule.Priority}";
        }
    }
}