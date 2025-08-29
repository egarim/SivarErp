using Sivar.Erp.Modules.Payments.Models;
using Sivar.Erp.Modules.Payments.Validation;
using Sivar.Erp.Modules.ImportExport;
using System.Text;

namespace Sivar.Erp.Infrastructure.ImportExport
{
    /// <summary>
    /// Infrastructure layer implementation of payment method import/export service (.NET 9)
    /// </summary>
    public class PaymentMethodImportExportService : IPaymentMethodImportExportService
    {
        private readonly PaymentMethodValidator _paymentMethodValidator;

        /// <summary>
        /// Initializes a new instance of the PaymentMethodImportExportService class
        /// </summary>
        public PaymentMethodImportExportService()
        {
            _paymentMethodValidator = new PaymentMethodValidator();
        }

        /// <summary>
        /// Initializes a new instance of the PaymentMethodImportExportService class with a custom validator
        /// </summary>
        /// <param name="paymentMethodValidator">Custom payment method validator</param>
        public PaymentMethodImportExportService(PaymentMethodValidator paymentMethodValidator)
        {
            _paymentMethodValidator = paymentMethodValidator ?? new PaymentMethodValidator();
        }

        /// <summary>
        /// Imports payment methods from a CSV file using PaymentMethodDto
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported payment methods and any validation errors</returns>
        public Task<(IEnumerable<IPaymentMethod> ImportedPaymentMethods, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<IPaymentMethod> importedPaymentMethods = new List<IPaymentMethod>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<IPaymentMethod>, IEnumerable<string>)>((importedPaymentMethods, errors));
            }

            try
            {
                // Split the CSV into lines (preserve all lines)
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<IPaymentMethod>, IEnumerable<string>)>((importedPaymentMethods, errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<IPaymentMethod>, IEnumerable<string>)>((importedPaymentMethods, errors));
                }

                // Track codes to detect duplicates within the import file
                HashSet<string> codesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

                    // Create PaymentMethodDto from CSV fields
                    var paymentMethodDto = CreatePaymentMethodDtoFromCsvFields(headers, fields);

                    // Validate payment method using PaymentMethodValidator
                    if (!_paymentMethodValidator.ValidatePaymentMethod(paymentMethodDto))
                    {
                        errors.Add($"Line {i + 1}: Payment method validation failed for payment method {paymentMethodDto.Name}");
                        continue;
                    }

                    // Check for duplicate codes within the import file
                    if (codesInFile.Contains(paymentMethodDto.Code))
                    {
                        errors.Add($"Line {i + 1}: Duplicate payment method code '{paymentMethodDto.Code}' found in import file");
                        continue;
                    }

                    codesInFile.Add(paymentMethodDto.Code);
                    importedPaymentMethods.Add(paymentMethodDto);
                }

                return Task.FromResult<(IEnumerable<IPaymentMethod>, IEnumerable<string>)>((importedPaymentMethods, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<IPaymentMethod>, IEnumerable<string>)>((importedPaymentMethods, errors));
            }
        }

        /// <summary>
        /// Exports payment methods to a CSV format
        /// </summary>
        /// <param name="paymentMethods">Payment methods to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IPaymentMethod> paymentMethods)
        {
            try
            {
                var paymentMethodsToExport = paymentMethods?.ToList() ?? new List<IPaymentMethod>();

                if (!paymentMethodsToExport.Any())
                {
                    return Task.FromResult(GetCsvHeader());
                }

                StringBuilder csvBuilder = new StringBuilder();

                // Add header
                csvBuilder.AppendLine(GetCsvHeader());

                // Add data rows
                foreach (var paymentMethod in paymentMethodsToExport)
                {
                    csvBuilder.AppendLine(GetCsvRow(paymentMethod));
                }

                return Task.FromResult(csvBuilder.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error exporting payment methods to CSV: {ex.Message}", ex);
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Creates a PaymentMethodDto from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New PaymentMethodDto with populated properties</returns>
        private PaymentMethodDto CreatePaymentMethodDtoFromCsvFields(string[] headers, string[] fields)
        {
            var paymentMethod = new PaymentMethodDto
            {
                IsActive = true
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];

                switch (headers[i].ToLowerInvariant())
                {
                    case "code":
                        paymentMethod.Code = value;
                        break;
                    case "name":
                        paymentMethod.Name = value;
                        break;
                    case "type":
                        if (Enum.TryParse<PaymentMethodType>(value, true, out var paymentMethodType))
                        {
                            paymentMethod.Type = paymentMethodType;
                        }
                        else
                        {
                            // Default to Cash if invalid
                            paymentMethod.Type = PaymentMethodType.Cash;
                        }
                        break;
                    case "accountcode":
                        paymentMethod.AccountCode = string.IsNullOrWhiteSpace(value) ? null : value;
                        break;
                    case "requiresbankaccount":
                        if (bool.TryParse(value, out var requiresBankAccount))
                        {
                            paymentMethod.RequiresBankAccount = requiresBankAccount;
                        }
                        break;
                    case "requiresreference":
                        if (bool.TryParse(value, out var requiresReference))
                        {
                            paymentMethod.RequiresReference = requiresReference;
                        }
                        break;
                    case "isactive":
                        if (bool.TryParse(value, out var isActive))
                        {
                            paymentMethod.IsActive = isActive;
                        }
                        break;
                }
            }

            return paymentMethod;
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
        /// Validates CSV headers for required fields
        /// </summary>
        /// <param name="headers">Array of header names</param>
        /// <param name="errors">Collection to add any validation errors to</param>
        /// <returns>True if headers are valid, false otherwise</returns>
        private bool ValidateHeaders(string[] headers, List<string> errors)
        {
            string[] requiredHeaders = { "code", "name", "type" };

            foreach (string requiredHeader in requiredHeaders)
            {
                if (!headers.Any(h => h.ToLowerInvariant() == requiredHeader))
                {
                    errors.Add($"Required header '{requiredHeader}' is missing");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the CSV header row
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "Code,Name,Type,AccountCode,RequiresBankAccount,RequiresReference,IsActive";
        }

        /// <summary>
        /// Gets a CSV row for a payment method
        /// </summary>
        /// <param name="paymentMethod">Payment method to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(IPaymentMethod paymentMethod)
        {
            string accountCode = string.IsNullOrWhiteSpace(paymentMethod.AccountCode)
                ? string.Empty
                : paymentMethod.AccountCode;

            return $"\"{paymentMethod.Code}\",\"{paymentMethod.Name}\",{paymentMethod.Type},\"{accountCode}\",{paymentMethod.RequiresBankAccount},{paymentMethod.RequiresReference},{paymentMethod.IsActive}";
        }

        #endregion
    }
}
