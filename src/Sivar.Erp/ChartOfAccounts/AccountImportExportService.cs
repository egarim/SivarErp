using System.Text;

namespace Sivar.Erp.ChartOfAccounts
{
    /// <summary>
    /// Implementation of account import/export service
    /// </summary>
    public class AccountImportExportService : IAccountImportExportService
    {
    
        private readonly AccountValidator _accountValidator;

        /// <summary>
        /// Initializes a new instance of the AccountImportExportService class
        /// </summary>
        /// <param name="auditService">Audit service for setting audit information</param>
        public AccountImportExportService()
        {
            
            _accountValidator = new AccountValidator();
        }

        /// <summary>
        /// Initializes a new instance of the AccountImportExportService class with a custom validator
        /// </summary>
        /// <param name="auditService">Audit service for setting audit information</param>
        /// <param name="accountValidator">Custom account validator</param>
        public AccountImportExportService( AccountValidator accountValidator)
        {
           
            _accountValidator = accountValidator ?? new AccountValidator();
        }

        /// <summary>
        /// Imports accounts from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported accounts and any validation errors</returns>
        public Task<(IEnumerable<IAccount> ImportedAccounts, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<AccountDto> importedAccounts = new List<AccountDto>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<IAccount>, IEnumerable<string>)>((importedAccounts, errors));
            }

            try
            {
                // Split the CSV into lines (preserve all lines)
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<IAccount>, IEnumerable<string>)>((importedAccounts, errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<IAccount>, IEnumerable<string>)>((importedAccounts, errors));
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

                    var account = CreateAccountFromCsvFields(headers, fields);

                    // Validate account
                    if (!_accountValidator.ValidateAccount(account))
                    {
                        errors.Add($"Line {i + 1}: Account validation failed for account {account.AccountName}");
                        continue;
                    }

                   
                    importedAccounts.Add(account);
                }

                return Task.FromResult<(IEnumerable<IAccount>, IEnumerable<string>)>((importedAccounts, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<IAccount>, IEnumerable<string>)>((importedAccounts, errors));
            }
        }

        /// <summary>
        /// Exports accounts to a CSV format
        /// </summary>
        /// <param name="accounts">Accounts to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IAccount> accounts)
        {
            if (accounts == null || !accounts.Any())
            {
                return Task.FromResult(GetCsvHeader());
            }

            StringBuilder csvBuilder = new StringBuilder();

            // Add header
            csvBuilder.AppendLine(GetCsvHeader());

            // Add data rows
            foreach (var account in accounts)
            {
                csvBuilder.AppendLine(GetCsvRow(account));
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
            string[] requiredHeaders = { "AccountName", "OfficialCode", "AccountType" };

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
        /// Creates an account from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New account with populated properties</returns>
        private AccountDto CreateAccountFromCsvFields(string[] headers, string[] fields)
        {
            var account = new AccountDto
            {
                Id = Guid.NewGuid(),
                IsArchived = false
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];

                switch (headers[i].ToLowerInvariant())
                {
                    case "accountname":
                        account.AccountName = value;
                        break;
                    case "officialcode":
                        account.OfficialCode = value;
                        break;
                    case "accounttype":
                        if (Enum.TryParse<AccountType>(value, true, out var accountType))
                        {
                            account.AccountType = accountType;
                        }
                        else
                        {
                            // Default to Asset if invalid
                            account.AccountType = AccountType.Asset;
                        }
                        break;
                    case "balanceandincomelineid":
                        if (Guid.TryParse(value, out var lineId))
                        {
                            account.BalanceAndIncomeLineId = lineId;
                        }
                        break;
                    case "parentofficialcode":
                        account.ParentOfficialCode = string.IsNullOrWhiteSpace(value) ? null : value;
                        break;
                }
            }

            return account;
        }

        /// <summary>
        /// Gets the CSV header row
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "AccountName,OfficialCode,AccountType,ParentOfficialCode,BalanceAndIncomeLineId";
        }

        /// <summary>
        /// Gets a CSV row for an account
        /// </summary>
        /// <param name="account">Account to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(IAccount account)
        {
            string balanceAndIncomeLineId = account.BalanceAndIncomeLineId.HasValue
                ? account.BalanceAndIncomeLineId.Value.ToString()
                : string.Empty;

            string parentOfficialCode = string.IsNullOrWhiteSpace(account.ParentOfficialCode)
                ? string.Empty
                : account.ParentOfficialCode;

            return $"\"{account.AccountName}\",\"{account.OfficialCode}\",{account.AccountType},\"{parentOfficialCode}\",{balanceAndIncomeLineId}";
        }
    }
}