using Sivar.Erp.Documents;
using System;
using System.Linq;
using System.Text;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Implementation of tax accounting profile import/export service
    /// </summary>
    public class TaxAccountingProfileImportExportService : ITaxAccountingProfileImportExportService
    {
        /// <summary>
        /// Imports tax accounting profiles from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content containing tax accounting profiles</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Collection of imported profiles and any validation errors</returns>
        public Task<(IEnumerable<TaxAccountingProfile> ImportedProfiles, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<TaxAccountingProfile> importedProfiles = new List<TaxAccountingProfile>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<TaxAccountingProfile>, IEnumerable<string>)>((importedProfiles, errors));
            }

            try
            {
                // Split the CSV into lines
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<TaxAccountingProfile>, IEnumerable<string>)>((importedProfiles, errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<TaxAccountingProfile>, IEnumerable<string>)>((importedProfiles, errors));
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

                    var profile = CreateProfileFromCsvFields(headers, fields);

                    // Validate profile
                    if (!ValidateProfile(profile, errors, i + 1))
                    {
                        continue;
                    }

                    importedProfiles.Add(profile);
                }

                return Task.FromResult<(IEnumerable<TaxAccountingProfile>, IEnumerable<string>)>((importedProfiles, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<TaxAccountingProfile>, IEnumerable<string>)>((importedProfiles, errors));
            }
        }

        /// <summary>
        /// Exports tax accounting profiles to CSV format
        /// </summary>
        /// <param name="profiles">Tax accounting profiles to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<TaxAccountingProfile> profiles)
        {
            if (profiles == null || !profiles.Any())
            {
                return Task.FromResult(GetCsvHeader());
            }

            StringBuilder csvBuilder = new StringBuilder();

            // Add header
            csvBuilder.AppendLine(GetCsvHeader());

            // Add data rows
            foreach (var profile in profiles)
            {
                csvBuilder.AppendLine(GetCsvRow(profile));
            }

            return Task.FromResult(csvBuilder.ToString());
        }

        /// <summary>
        /// Creates a tax accounting profile from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New tax accounting profile with populated properties</returns>
        private TaxAccountingProfile CreateProfileFromCsvFields(string[] headers, string[] fields)
        {
            var profile = new TaxAccountingProfile
            {
                IncludeInTransaction = true // Default value
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];

                switch (headers[i].ToLowerInvariant())
                {
                    case "taxcode":
                        profile.TaxCode = value;
                        break;
                    case "documentoperation":
                        if (Enum.TryParse<DocumentOperation>(value, true, out var docOperation))
                        {
                            profile.DocumentOperation = docOperation;
                        }
                        else
                        {
                            // Try to parse common variations
                            if (value.Equals("SalesInvoice", StringComparison.OrdinalIgnoreCase))
                                profile.DocumentOperation = DocumentOperation.SalesInvoice;
                            else if (value.Equals("PurchaseInvoice", StringComparison.OrdinalIgnoreCase))
                                profile.DocumentOperation = DocumentOperation.PurchaseInvoice;
                            // Add more variations as needed
                        }
                        break;
                    case "debitaccountcode":
                        profile.DebitAccountCode = string.IsNullOrWhiteSpace(value) ? null : value;
                        break;
                    case "creditaccountcode":
                        profile.CreditAccountCode = string.IsNullOrWhiteSpace(value) ? null : value;
                        break;
                    case "accountdescription":
                        profile.AccountDescription = value;
                        break;
                    case "includeintransaction":
                        if (bool.TryParse(value, out var includeInTransaction))
                        {
                            profile.IncludeInTransaction = includeInTransaction;
                        }
                        break;
                }
            }

            return profile;
        }

        /// <summary>
        /// Validates a tax accounting profile
        /// </summary>
        /// <param name="profile">Profile to validate</param>
        /// <param name="errors">Collection to add errors to</param>
        /// <param name="lineNumber">Line number for error reporting</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateProfile(TaxAccountingProfile profile, List<string> errors, int lineNumber)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(profile.TaxCode))
            {
                errors.Add($"Line {lineNumber}: TaxCode is required");
                isValid = false;
            }

            // Must have either debit or credit account code (or both)
            if (string.IsNullOrWhiteSpace(profile.DebitAccountCode) && string.IsNullOrWhiteSpace(profile.CreditAccountCode))
            {
                errors.Add($"Line {lineNumber}: Either DebitAccountCode or CreditAccountCode (or both) must be specified");
                isValid = false;
            }

            return isValid;
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
            string[] requiredHeaders = { "TaxCode", "DocumentOperation" };

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
        /// Gets the CSV header row
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "TaxCode,DocumentOperation,DebitAccountCode,CreditAccountCode,AccountDescription,IncludeInTransaction";
        }

        /// <summary>
        /// Gets a CSV row for a tax accounting profile
        /// </summary>
        /// <param name="profile">Profile to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(TaxAccountingProfile profile)
        {
            string debitAccountCode = string.IsNullOrWhiteSpace(profile.DebitAccountCode) ? string.Empty : profile.DebitAccountCode;
            string creditAccountCode = string.IsNullOrWhiteSpace(profile.CreditAccountCode) ? string.Empty : profile.CreditAccountCode;
            string accountDescription = string.IsNullOrWhiteSpace(profile.AccountDescription) ? string.Empty : profile.AccountDescription;

            return $"\"{profile.TaxCode}\",\"{profile.DocumentOperation}\",\"{debitAccountCode}\",\"{creditAccountCode}\",\"{accountDescription}\",{profile.IncludeInTransaction}";
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
    }
}