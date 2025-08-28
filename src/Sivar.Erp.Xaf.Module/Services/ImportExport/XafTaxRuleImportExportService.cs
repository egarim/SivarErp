using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Documents.Core.Enums;
using Sivar.Erp.Modules.Taxes.TaxRule;
using Sivar.Erp.Modules.ImportExport;
using System.Text;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.ImportExport
{
    /// <summary>
    /// XAF implementation of tax rule import/export service using IObjectSpace
    /// </summary>
    public class XafTaxRuleImportExportService : ITaxRuleImportExportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly TaxRuleValidator _taxRuleValidator;
        private readonly ILogger<XafTaxRuleImportExportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafTaxRuleImportExportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafTaxRuleImportExportService(IObjectSpace objectSpace, ILogger<XafTaxRuleImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _taxRuleValidator = new TaxRuleValidator();
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the XafTaxRuleImportExportService class with a custom validator
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="taxRuleValidator">Custom tax rule validator</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafTaxRuleImportExportService(IObjectSpace objectSpace, TaxRuleValidator taxRuleValidator, ILogger<XafTaxRuleImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _taxRuleValidator = taxRuleValidator ?? new TaxRuleValidator();
            _logger = logger;
        }

        /// <summary>
        /// Imports tax rules from a CSV file using XAF ObjectSpace
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported tax rules and any validation errors</returns>
        public Task<(IEnumerable<ITaxRule> ImportedTaxRules, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<TaxRule> importedTaxRules = new List<TaxRule>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<ITaxRule>, IEnumerable<string>)>((importedTaxRules, errors));
            }

            try
            {
                _logger?.LogInformation("Starting tax rule import for user: {UserName}", userName);

                // Split the CSV into lines (preserve all lines)
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<ITaxRule>, IEnumerable<string>)>((importedTaxRules, errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<ITaxRule>, IEnumerable<string>)>((importedTaxRules, errors));
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

                    // First create TaxRuleDto from CSV fields (same as POCO implementation)
                    var taxRuleDto = CreateTaxRuleDtoFromCsvFields(headers, fields);

                    // Validate tax rule using TaxRuleValidator
                    if (!_taxRuleValidator.ValidateTaxRule(taxRuleDto))
                    {
                        errors.Add($"Line {i + 1}: Tax rule validation failed for tax rule with TaxId '{taxRuleDto.TaxId}'");
                        continue;
                    }

                    // Check for duplicate tax rules by combination of key fields in the database
                    var existingTaxRule = FindExistingTaxRule(taxRuleDto);
                    if (existingTaxRule != null)
                    {
                        errors.Add($"Line {i + 1}: Tax rule with similar criteria already exists (TaxId: {taxRuleDto.TaxId}, DocumentOperation: {taxRuleDto.DocumentOperation})");
                        continue;
                    }

                    // Only now create the XAF entity from the validated DTO
                    var xafTaxRule = CreateXafTaxRuleFromDto(taxRuleDto, userName);
                    importedTaxRules.Add(xafTaxRule);
                }

                // If there are no errors, commit the changes
                if (errors.Count == 0 && importedTaxRules.Count > 0)
                {
                    _objectSpace.CommitChanges();
                    _logger?.LogInformation("Successfully imported {Count} tax rules", importedTaxRules.Count);
                }
                else if (errors.Count > 0)
                {
                    _logger?.LogWarning("Import completed with {ErrorCount} errors out of {TotalRows} rows", 
                        errors.Count, lines.Length - 1);
                }

                return Task.FromResult<(IEnumerable<ITaxRule>, IEnumerable<string>)>((importedTaxRules, errors));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error importing CSV content");
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<ITaxRule>, IEnumerable<string>)>((importedTaxRules, errors));
            }
        }

        /// <summary>
        /// Exports tax rules to a CSV format
        /// </summary>
        /// <param name="taxRules">Tax rules to export (if null, exports all tax rules from ObjectSpace)</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<ITaxRule> taxRules)
        {
            try
            {
                _logger?.LogInformation("Starting tax rule export");

                // If no tax rules provided, get all tax rules from ObjectSpace
                var taxRulesToExport = taxRules?.ToList() ?? 
                    _objectSpace.GetObjects<TaxRule>().Cast<ITaxRule>().ToList();

                if (!taxRulesToExport.Any())
                {
                    _logger?.LogInformation("No tax rules found for export");
                    return Task.FromResult(GetCsvHeader());
                }

                StringBuilder csvBuilder = new StringBuilder();

                // Add header
                csvBuilder.AppendLine(GetCsvHeader());

                // Add data rows
                foreach (var taxRule in taxRulesToExport)
                {
                    csvBuilder.AppendLine(GetCsvRow(taxRule));
                }

                _logger?.LogInformation("Successfully exported {Count} tax rules", taxRulesToExport.Count);
                return Task.FromResult(csvBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting tax rules to CSV");
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Creates a TaxRuleDto from CSV fields (copied from POCO implementation)
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New TaxRuleDto with populated properties</returns>
        private TaxRuleDto CreateTaxRuleDtoFromCsvFields(string[] headers, string[] fields)
        {
            var taxRule = new TaxRuleDto
            {
                ID = Guid.NewGuid(),
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
                        // For now, we'll use the tax code directly as TaxId
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
        /// Creates a XAF TaxRule entity from a validated TaxRuleDto
        /// </summary>
        /// <param name="taxRuleDto">Validated TaxRuleDto</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>New XAF TaxRule with populated properties</returns>
        private TaxRule CreateXafTaxRuleFromDto(TaxRuleDto taxRuleDto, string userName)
        {
            var taxRule = _objectSpace.CreateObject<TaxRule>();
            var currentTime = DateTime.UtcNow;

            // Copy data from DTO to XAF entity
            taxRule.TaxId = taxRuleDto.TaxId;
            taxRule.DocumentOperation = taxRuleDto.DocumentOperation;
            taxRule.BusinessEntityGroupId = taxRuleDto.BusinessEntityGroupId;
            taxRule.ItemGroupId = taxRuleDto.ItemGroupId;
            taxRule.IsEnabled = taxRuleDto.IsEnabled;
            taxRule.Priority = taxRuleDto.Priority;

            // Set audit fields
            taxRule.InsertedBy = userName;
            taxRule.InsertedAt = currentTime;
            taxRule.UpdatedBy = userName;
            taxRule.UpdatedAt = currentTime;

            return taxRule;
        }

        /// <summary>
        /// Finds an existing tax rule with similar criteria
        /// </summary>
        /// <param name="taxRuleDto">Tax rule to check for duplicates</param>
        /// <returns>Existing tax rule if found, null otherwise</returns>
        private TaxRule? FindExistingTaxRule(TaxRuleDto taxRuleDto)
        {
            // Create criteria to find similar tax rules
            // A rule is considered duplicate if it has the same TaxId, DocumentOperation, BusinessEntityGroupId, and ItemGroupId
            var criteria = CriteriaOperator.Parse(
                "TaxId = ? AND DocumentOperation = ? AND BusinessEntityGroupId = ? AND ItemGroupId = ?",
                taxRuleDto.TaxId,
                taxRuleDto.DocumentOperation,
                taxRuleDto.BusinessEntityGroupId ?? string.Empty,
                taxRuleDto.ItemGroupId ?? string.Empty);

            return _objectSpace.FindObject<TaxRule>(criteria);
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
        /// Gets the CSV header row (copied from POCO implementation)
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "TaxCode,DocumentOperation,BusinessEntityGroupCode,ItemGroupCode,IsEnabled,Priority";
        }

        /// <summary>
        /// Gets a CSV row for a tax rule (copied from POCO implementation)
        /// </summary>
        /// <param name="taxRule">Tax rule to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(ITaxRule taxRule)
        {
            // Note: In a real implementation, tax code, business entity group code, and item group code
            // would need to be looked up from their respective repositories based on their IDs.
            // For this example, we'll use the direct values or create placeholder values.

            string taxCode = taxRule.TaxId;
            string documentOperation = taxRule.DocumentOperation?.ToString() ?? "";
            string businessEntityGroupCode = taxRule.BusinessEntityGroupId ?? "";
            string itemGroupCode = taxRule.ItemGroupId ?? "";

            return $"\"{taxCode}\",\"{documentOperation}\",\"{businessEntityGroupCode}\",\"{itemGroupCode}\",{taxRule.IsEnabled},{taxRule.Priority}";
        }

        #endregion
    }
}
