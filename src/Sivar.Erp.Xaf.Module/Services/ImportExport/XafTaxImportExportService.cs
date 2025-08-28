using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Taxes;
using Sivar.Erp.Modules.ImportExport;
using System.Text;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.ImportExport
{
    /// <summary>
    /// XAF implementation of tax import/export service using IObjectSpace
    /// </summary>
    public class XafTaxImportExportService : ITaxImportExportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly TaxValidator _taxValidator;
        private readonly ILogger<XafTaxImportExportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafTaxImportExportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafTaxImportExportService(IObjectSpace objectSpace, ILogger<XafTaxImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _taxValidator = new TaxValidator();
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the XafTaxImportExportService class with a custom validator
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="taxValidator">Custom tax validator</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafTaxImportExportService(IObjectSpace objectSpace, TaxValidator taxValidator, ILogger<XafTaxImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _taxValidator = taxValidator ?? new TaxValidator();
            _logger = logger;
        }

        /// <summary>
        /// Imports taxes from a CSV file using XAF ObjectSpace
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported taxes and any validation errors</returns>
        public Task<(IEnumerable<TaxDto> ImportedTaxes, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<TaxDto> importedTaxes = new List<TaxDto>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<TaxDto>, IEnumerable<string>)>((importedTaxes, errors));
            }

            try
            {
                _logger?.LogInformation("Starting tax import for user: {UserName}", userName);

                // Split the CSV into lines (preserve all lines)
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<TaxDto>, IEnumerable<string>)>((importedTaxes, errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<TaxDto>, IEnumerable<string>)>((importedTaxes, errors));
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

                    // First create TaxDto from CSV fields (same as POCO implementation)
                    var taxDto = CreateTaxDtoFromCsvFields(headers, fields);

                    // Validate tax using TaxValidator
                    if (!_taxValidator.ValidateTax(taxDto))
                    {
                        errors.Add($"Line {i + 1}: Tax validation failed for tax {taxDto.Name} ({taxDto.Code})");
                        continue;
                    }

                    // Check for duplicate taxes by Code in the database
                    var existingTax = _objectSpace.FindObject<Tax>(CriteriaOperator.Parse("Code = ?", taxDto.Code));
                    if (existingTax != null)
                    {
                        errors.Add($"Line {i + 1}: Tax with code '{taxDto.Code}' already exists");
                        continue;
                    }

                    // Only now create the XAF entity from the validated DTO
                    var xafTax = CreateXafTaxFromDto(taxDto, userName);
                    
                    // Convert back to TaxDto for return collection
                    var resultDto = ConvertXafTaxToDto(xafTax);
                    importedTaxes.Add(resultDto);
                }

                // If there are no errors, commit the changes
                if (errors.Count == 0 && importedTaxes.Count > 0)
                {
                    _objectSpace.CommitChanges();
                    _logger?.LogInformation("Successfully imported {Count} taxes", importedTaxes.Count);
                }
                else if (errors.Count > 0)
                {
                    _logger?.LogWarning("Import completed with {ErrorCount} errors out of {TotalRows} rows", 
                        errors.Count, lines.Length - 1);
                }

                return Task.FromResult<(IEnumerable<TaxDto>, IEnumerable<string>)>((importedTaxes, errors));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error importing CSV content");
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<TaxDto>, IEnumerable<string>)>((importedTaxes, errors));
            }
        }

        /// <summary>
        /// Exports taxes to a CSV format
        /// </summary>
        /// <param name="taxes">Taxes to export (if null, exports all taxes from ObjectSpace)</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<TaxDto> taxes)
        {
            try
            {
                _logger?.LogInformation("Starting tax export");

                List<TaxDto> taxesToExport;

                // If no taxes provided, get all taxes from ObjectSpace and convert to DTOs
                if (taxes == null || !taxes.Any())
                {
                    var xafTaxes = _objectSpace.GetObjects<Tax>().ToList();
                    taxesToExport = xafTaxes.Select(ConvertXafTaxToDto).ToList();
                }
                else
                {
                    taxesToExport = taxes.ToList();
                }

                if (!taxesToExport.Any())
                {
                    _logger?.LogInformation("No taxes found for export");
                    return Task.FromResult(GetCsvHeader());
                }

                StringBuilder csvBuilder = new StringBuilder();

                // Add header
                csvBuilder.AppendLine(GetCsvHeader());

                // Add data rows
                foreach (var tax in taxesToExport)
                {
                    csvBuilder.AppendLine(GetCsvRow(tax));
                }

                _logger?.LogInformation("Successfully exported {Count} taxes", taxesToExport.Count);
                return Task.FromResult(csvBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting taxes to CSV");
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Creates a TaxDto from CSV fields (copied from POCO implementation)
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New TaxDto with populated properties</returns>
        private TaxDto CreateTaxDtoFromCsvFields(string[] headers, string[] fields)
        {
            var tax = new TaxDto
            {
                ID = Guid.NewGuid(),
                IsEnabled = true, // Default to enabled
                IsIncludedInPrice = false // Default to not included in price
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];
                if (string.IsNullOrWhiteSpace(value)) continue;

                switch (headers[i].ToLowerInvariant())
                {
                    case "code":
                        tax.Code = value;
                        break;
                    case "name":
                        tax.Name = value;
                        break;
                    case "taxtype":
                        if (Enum.TryParse<TaxType>(value, true, out var taxType))
                        {
                            tax.TaxType = taxType;
                        }
                        else
                        {
                            // Default to Percentage if invalid
                            tax.TaxType = TaxType.Percentage;
                        }
                        break;
                    case "applicationlevel":
                        if (Enum.TryParse<TaxApplicationLevel>(value, true, out var applicationLevel))
                        {
                            tax.ApplicationLevel = applicationLevel;
                        }
                        else
                        {
                            // Default to Line if invalid
                            tax.ApplicationLevel = TaxApplicationLevel.Line;
                        }
                        break;
                    case "percentage":
                        if (decimal.TryParse(value, out var percentage))
                        {
                            tax.Percentage = percentage;
                        }
                        break;
                    case "amount":
                        if (decimal.TryParse(value, out var amount))
                        {
                            tax.Amount = amount;
                        }
                        break;
                    case "isenabled":
                        if (bool.TryParse(value, out var isEnabled))
                        {
                            tax.IsEnabled = isEnabled;
                        }
                        break;
                    case "isincludedinprice":
                        if (bool.TryParse(value, out var isIncludedInPrice))
                        {
                            tax.IsIncludedInPrice = isIncludedInPrice;
                        }
                        break;
                }
            }

            return tax;
        }

        /// <summary>
        /// Creates a XAF Tax entity from a validated TaxDto
        /// </summary>
        /// <param name="taxDto">Validated TaxDto</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>New XAF Tax with populated properties</returns>
        private Tax CreateXafTaxFromDto(TaxDto taxDto, string userName)
        {
            var tax = _objectSpace.CreateObject<Tax>();
            var currentTime = DateTime.UtcNow;

            // Copy data from DTO to XAF entity
            tax.Name = taxDto.Name;
            tax.Code = taxDto.Code;
            tax.TaxType = taxDto.TaxType;
            tax.ApplicationLevel = taxDto.ApplicationLevel;
            tax.Percentage = taxDto.Percentage;
            tax.Amount = taxDto.Amount;
            tax.IsEnabled = taxDto.IsEnabled;
            tax.IsIncludedInPrice = taxDto.IsIncludedInPrice;

            // Set audit fields
            tax.InsertedBy = userName;
            tax.InsertedAt = currentTime;
            tax.UpdatedBy = userName;
            tax.UpdatedAt = currentTime;

            return tax;
        }

        /// <summary>
        /// Converts a XAF Tax entity to TaxDto
        /// </summary>
        /// <param name="xafTax">XAF Tax entity</param>
        /// <returns>TaxDto with populated properties</returns>
        private TaxDto ConvertXafTaxToDto(Tax xafTax)
        {
            return new TaxDto
            {
                ID = xafTax.ID,
                Name = xafTax.Name,
                Code = xafTax.Code,
                TaxType = xafTax.TaxType,
                ApplicationLevel = xafTax.ApplicationLevel,
                Percentage = xafTax.Percentage,
                Amount = xafTax.Amount,
                IsEnabled = xafTax.IsEnabled,
                IsIncludedInPrice = xafTax.IsIncludedInPrice
            };
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
            // Define required headers (same as POCO implementation)
            string[] requiredHeaders = { "Code", "Name", "TaxType", "ApplicationLevel" };

            foreach (var requiredHeader in requiredHeaders)
            {
                if (!headers.Any(h => h.Equals(requiredHeader, StringComparison.OrdinalIgnoreCase)))
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
            return "Code,Name,TaxType,ApplicationLevel,Percentage,Amount,IsEnabled,IsIncludedInPrice";
        }

        /// <summary>
        /// Gets a CSV row for a tax (copied from POCO implementation)
        /// </summary>
        /// <param name="tax">Tax to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(TaxDto tax)
        {
            string percentage = tax.TaxType == TaxType.Percentage ? tax.Percentage.ToString() : string.Empty;
            string amount = tax.TaxType == TaxType.FixedAmount || tax.TaxType == TaxType.AmountPerUnit ? tax.Amount.ToString() : string.Empty;

            return $"\"{tax.Code}\",\"{tax.Name}\",{tax.TaxType},{tax.ApplicationLevel},{percentage},{amount},{tax.IsEnabled},{tax.IsIncludedInPrice}";
        }

        #endregion
    }
}
