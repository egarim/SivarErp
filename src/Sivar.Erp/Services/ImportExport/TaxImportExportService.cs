using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sivar.Erp.Taxes;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Implementation of tax import/export service
    /// </summary>
    public class TaxImportExportService : ITaxImportExportService
    {
        private readonly TaxValidator _taxValidator;

        /// <summary>
        /// Initializes a new instance of the TaxImportExportService class
        /// </summary>
        public TaxImportExportService()
        {
            _taxValidator = new TaxValidator();
        }

        /// <summary>
        /// Initializes a new instance of the TaxImportExportService class with a custom validator
        /// </summary>
        /// <param name="taxValidator">Custom tax validator</param>
        public TaxImportExportService(TaxValidator taxValidator)
        {
            _taxValidator = taxValidator ?? new TaxValidator();
        }

        /// <summary>
        /// Imports taxes from a CSV file
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

                    var tax = CreateTaxFromCsvFields(headers, fields);

                    // Validate tax
                    if (!_taxValidator.ValidateTax(tax))
                    {
                        errors.Add($"Line {i + 1}: Tax validation failed for tax {tax.Name} ({tax.Code})");
                        continue;
                    }

                    importedTaxes.Add(tax);
                }

                return Task.FromResult<(IEnumerable<TaxDto>, IEnumerable<string>)>((importedTaxes, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<TaxDto>, IEnumerable<string>)>((importedTaxes, errors));
            }
        }

        /// <summary>
        /// Exports taxes to a CSV format
        /// </summary>
        /// <param name="taxes">Taxes to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<TaxDto> taxes)
        {
            if (taxes == null || !taxes.Any())
            {
                return Task.FromResult(GetCsvHeader());
            }

            StringBuilder csvBuilder = new StringBuilder();

            // Add header
            csvBuilder.AppendLine(GetCsvHeader());

            // Add data rows
            foreach (var tax in taxes)
            {
                csvBuilder.AppendLine(GetCsvRow(tax));
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
            string[] requiredHeaders = { "Code", "Name", "TaxType", "ApplicationLevel" };

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
        /// Creates a tax from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New tax with populated properties</returns>
        private TaxDto CreateTaxFromCsvFields(string[] headers, string[] fields)
        {
            var tax = new TaxDto
            {
                Oid = Guid.NewGuid(),
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
                    case "debitaccountcode":
                        // This would be stored in a TaxAccountingInfo object
                        // For now, just log it or handle it as needed
                        break;
                    case "creditaccountcode":
                        // This would be stored in a TaxAccountingInfo object
                        // For now, just log it or handle it as needed
                        break;
                    case "accountdescription":
                        // This would be stored in a TaxAccountingInfo object
                        // For now, just log it or handle it as needed
                        break;
                }
            }

            return tax;
        }

        /// <summary>
        /// Gets the CSV header row
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "Code,Name,TaxType,ApplicationLevel,Percentage,Amount,IsEnabled,IsIncludedInPrice,DebitAccountCode,CreditAccountCode,AccountDescription";
        }

        /// <summary>
        /// Gets a CSV row for a tax
        /// </summary>
        /// <param name="tax">Tax to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(TaxDto tax)
        {
            string percentage = tax.TaxType == TaxType.Percentage ? tax.Percentage.ToString() : string.Empty;
            string amount = tax.TaxType == TaxType.FixedAmount || tax.TaxType == TaxType.AmountPerUnit ? tax.Amount.ToString() : string.Empty;
            
            // Note: debitAccountCode, creditAccountCode, and accountDescription would come from TaxAccountingInfo
            // which is not directly part of TaxDto. For now, we leave them empty.
            string debitAccountCode = string.Empty;
            string creditAccountCode = string.Empty;
            string accountDescription = string.Empty;

            return $"\"{tax.Code}\",\"{tax.Name}\",{tax.TaxType},{tax.ApplicationLevel},{percentage},{amount},{tax.IsEnabled},{tax.IsIncludedInPrice},\"{debitAccountCode}\",\"{creditAccountCode}\",\"{accountDescription}\"";
        }
    }
}