using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Payments.Models;
using Sivar.Erp.Modules.Payments.Validation;
using Sivar.Erp.Modules.ImportExport;
using System.Text;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.ImportExport
{
    /// <summary>
    /// XAF implementation of payment method import/export service using IObjectSpace
    /// </summary>
    public class XafPaymentMethodImportExportService : IPaymentMethodImportExportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly PaymentMethodValidator _paymentMethodValidator;
        private readonly ILogger<XafPaymentMethodImportExportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafPaymentMethodImportExportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafPaymentMethodImportExportService(IObjectSpace objectSpace, ILogger<XafPaymentMethodImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _paymentMethodValidator = new PaymentMethodValidator();
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the XafPaymentMethodImportExportService class with a custom validator
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="paymentMethodValidator">Custom payment method validator</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafPaymentMethodImportExportService(IObjectSpace objectSpace, PaymentMethodValidator paymentMethodValidator, ILogger<XafPaymentMethodImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _paymentMethodValidator = paymentMethodValidator ?? new PaymentMethodValidator();
            _logger = logger;
        }

        /// <summary>
        /// Imports payment methods from a CSV file using XAF ObjectSpace
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported payment methods and any validation errors</returns>
        public Task<(IEnumerable<IPaymentMethod> ImportedPaymentMethods, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<PaymentMethod> importedPaymentMethods = new List<PaymentMethod>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<IPaymentMethod>, IEnumerable<string>)>((importedPaymentMethods, errors));
            }

            try
            {
                _logger?.LogInformation("Starting payment method import for user: {UserName}", userName);

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

                    // First create PaymentMethodDto from CSV fields
                    var paymentMethodDto = CreatePaymentMethodDtoFromCsvFields(headers, fields);

                    // Validate payment method using PaymentMethodValidator
                    if (!_paymentMethodValidator.ValidatePaymentMethod(paymentMethodDto))
                    {
                        errors.Add($"Line {i + 1}: Payment method validation failed for payment method {paymentMethodDto.Name}");
                        continue;
                    }

                    // Check for duplicate payment methods by Code in the database
                    var existingPaymentMethod = _objectSpace.FindObject<PaymentMethod>(CriteriaOperator.Parse("Code = ?", paymentMethodDto.Code));
                    if (existingPaymentMethod != null)
                    {
                        errors.Add($"Line {i + 1}: Payment method with code '{paymentMethodDto.Code}' already exists");
                        continue;
                    }

                    // Only now create the XAF entity from the validated DTO
                    var xafPaymentMethod = CreateXafPaymentMethodFromDto(paymentMethodDto, userName);
                    importedPaymentMethods.Add(xafPaymentMethod);
                }

                // If there are no errors, commit the changes
                if (errors.Count == 0 && importedPaymentMethods.Count > 0)
                {
                    _objectSpace.CommitChanges();
                    _logger?.LogInformation("Successfully imported {Count} payment methods", importedPaymentMethods.Count);
                }
                else if (errors.Count > 0)
                {
                    _logger?.LogWarning("Import completed with {ErrorCount} errors out of {TotalRows} rows", 
                        errors.Count, lines.Length - 1);
                }

                return Task.FromResult<(IEnumerable<IPaymentMethod>, IEnumerable<string>)>((importedPaymentMethods, errors));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error importing CSV content");
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<IPaymentMethod>, IEnumerable<string>)>((importedPaymentMethods, errors));
            }
        }

        /// <summary>
        /// Exports payment methods to a CSV format
        /// </summary>
        /// <param name="paymentMethods">Payment methods to export (if null, exports all payment methods from ObjectSpace)</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IPaymentMethod> paymentMethods)
        {
            try
            {
                _logger?.LogInformation("Starting payment method export");

                // If no payment methods provided, get all payment methods from ObjectSpace
                var paymentMethodsToExport = paymentMethods?.ToList() ?? 
                    _objectSpace.GetObjects<PaymentMethod>().Cast<IPaymentMethod>().ToList();

                if (!paymentMethodsToExport.Any())
                {
                    _logger?.LogInformation("No payment methods found for export");
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

                _logger?.LogInformation("Successfully exported {Count} payment methods", paymentMethodsToExport.Count);
                return Task.FromResult(csvBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting payment methods to CSV");
                throw;
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
        /// Creates a XAF PaymentMethod entity from a validated PaymentMethodDto
        /// </summary>
        /// <param name="paymentMethodDto">Validated PaymentMethodDto</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>New XAF PaymentMethod with populated properties</returns>
        private PaymentMethod CreateXafPaymentMethodFromDto(PaymentMethodDto paymentMethodDto, string userName)
        {
            var paymentMethod = _objectSpace.CreateObject<PaymentMethod>();
            var currentTime = DateTime.UtcNow;

            // Copy data from DTO to XAF entity
            paymentMethod.Code = paymentMethodDto.Code;
            paymentMethod.Name = paymentMethodDto.Name;
            paymentMethod.Type = paymentMethodDto.Type;
            paymentMethod.AccountCode = paymentMethodDto.AccountCode;
            paymentMethod.RequiresBankAccount = paymentMethodDto.RequiresBankAccount;
            paymentMethod.RequiresReference = paymentMethodDto.RequiresReference;
            paymentMethod.IsActive = paymentMethodDto.IsActive;

            // Set audit fields
            paymentMethod.InsertedBy = userName;
            paymentMethod.InsertedAt = currentTime;
            paymentMethod.UpdatedBy = userName;
            paymentMethod.UpdatedAt = currentTime;

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
