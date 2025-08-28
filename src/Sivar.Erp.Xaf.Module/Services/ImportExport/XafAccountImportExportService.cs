using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Accounting.ChartOfAccounts;
using Sivar.Erp.Modules.ImportExport;
using System.Text;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.ImportExport
{
    /// <summary>
    /// XAF implementation of account import/export service using IObjectSpace
    /// </summary>
    public class XafAccountImportExportService : IAccountImportExportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly AccountValidator _accountValidator;
        private readonly ILogger<XafAccountImportExportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafAccountImportExportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafAccountImportExportService(IObjectSpace objectSpace, ILogger<XafAccountImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _accountValidator = new AccountValidator();
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the XafAccountImportExportService class with a custom validator
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="accountValidator">Custom account validator</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafAccountImportExportService(IObjectSpace objectSpace, AccountValidator accountValidator, ILogger<XafAccountImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _accountValidator = accountValidator ?? new AccountValidator();
            _logger = logger;
        }

        /// <summary>
        /// Imports accounts from a CSV file using XAF ObjectSpace
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported accounts and any validation errors</returns>
        public Task<(IEnumerable<IAccount> ImportedAccounts, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<Account> importedAccounts = new List<Account>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<IAccount>, IEnumerable<string>)>((importedAccounts, errors));
            }

            try
            {
                _logger?.LogInformation("Starting account import for user: {UserName}", userName);

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

                    // First create AccountDto from CSV fields (same as POCO implementation)
                    var accountDto = CreateAccountDtoFromCsvFields(headers, fields);

                    // Validate account using AccountValidator
                    if (!_accountValidator.ValidateAccount(accountDto))
                    {
                        errors.Add($"Line {i + 1}: Account validation failed for account {accountDto.AccountName}");
                        continue;
                    }

                    // Check for duplicate accounts by OfficialCode in the database
                    var existingAccount = _objectSpace.FindObject<Account>(CriteriaOperator.Parse("OfficialCode = ?", accountDto.OfficialCode));
                    if (existingAccount != null)
                    {
                        errors.Add($"Line {i + 1}: Account with official code '{accountDto.OfficialCode}' already exists");
                        continue;
                    }

                    // Only now create the XAF entity from the validated DTO
                    var xafAccount = CreateXafAccountFromDto(accountDto, userName);
                    importedAccounts.Add(xafAccount);
                }

                // If there are no errors, commit the changes
                if (errors.Count == 0 && importedAccounts.Count > 0)
                {
                    _objectSpace.CommitChanges();
                    _logger?.LogInformation("Successfully imported {Count} accounts", importedAccounts.Count);
                }
                else if (errors.Count > 0)
                {
                    _logger?.LogWarning("Import completed with {ErrorCount} errors out of {TotalRows} rows", 
                        errors.Count, lines.Length - 1);
                }

                return Task.FromResult<(IEnumerable<IAccount>, IEnumerable<string>)>((importedAccounts, errors));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error importing CSV content");
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<IAccount>, IEnumerable<string>)>((importedAccounts, errors));
            }
        }

        /// <summary>
        /// Exports accounts to a CSV format
        /// </summary>
        /// <param name="accounts">Accounts to export (if null, exports all accounts from ObjectSpace)</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IAccount> accounts)
        {
            try
            {
                _logger?.LogInformation("Starting account export");

                // If no accounts provided, get all accounts from ObjectSpace
                var accountsToExport = accounts?.ToList() ?? 
                    _objectSpace.GetObjects<Account>().Cast<IAccount>().ToList();

                if (!accountsToExport.Any())
                {
                    _logger?.LogInformation("No accounts found for export");
                    return Task.FromResult(GetCsvHeader());
                }

                StringBuilder csvBuilder = new StringBuilder();

                // Add header
                csvBuilder.AppendLine(GetCsvHeader());

                // Add data rows
                foreach (var account in accountsToExport)
                {
                    csvBuilder.AppendLine(GetCsvRow(account));
                }

                _logger?.LogInformation("Successfully exported {Count} accounts", accountsToExport.Count);
                return Task.FromResult(csvBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting accounts to CSV");
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Creates an AccountDto from CSV fields (copied from POCO implementation)
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New AccountDto with populated properties</returns>
        private AccountDto CreateAccountDtoFromCsvFields(string[] headers, string[] fields)
        {
            var account = new AccountDto
            {
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
        /// Creates a XAF Account entity from a validated AccountDto
        /// </summary>
        /// <param name="accountDto">Validated AccountDto</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>New XAF Account with populated properties</returns>
        private Account CreateXafAccountFromDto(AccountDto accountDto, string userName)
        {
            var account = _objectSpace.CreateObject<Account>();
            var currentTime = DateTime.UtcNow;

            // Copy data from DTO to XAF entity
            account.AccountName = accountDto.AccountName;
            account.OfficialCode = accountDto.OfficialCode;
            account.AccountType = accountDto.AccountType;
            account.ParentOfficialCode = accountDto.ParentOfficialCode;
            account.BalanceAndIncomeLineId = accountDto.BalanceAndIncomeLineId;

            // Set audit fields
            account.InsertedBy = userName;
            account.InsertedAt = currentTime;
            account.UpdatedBy = userName;
            account.UpdatedAt = currentTime;

            return account;
        }

        /// <summary>
        /// Creates a XAF Account entity from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>New XAF Account with populated properties</returns>
        [Obsolete("Use CreateAccountDtoFromCsvFields followed by CreateXafAccountFromDto instead")]
        private Account CreateXafAccountFromCsvFields(string[] headers, string[] fields, string userName)
        {
            var account = _objectSpace.CreateObject<Account>();
            var currentTime = DateTime.UtcNow;

            // Set audit fields
            account.InsertedBy = userName;
            account.InsertedAt = currentTime;
            account.UpdatedBy = userName;
            account.UpdatedAt = currentTime;

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
                    case "parentofficialcode":
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            account.ParentOfficialCode = value;
                        }
                        break;
                    case "balanceandincomelineid":
                        if (Guid.TryParse(value, out var lineId))
                        {
                            account.BalanceAndIncomeLineId = lineId;
                        }
                        break;
                }
            }

            return account;
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
            string[] requiredHeaders = { "accountname", "officialcode", "accounttype" };

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
        /// Gets the CSV header row (copied from POCO implementation)
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "AccountName,OfficialCode,AccountType,ParentOfficialCode,BalanceAndIncomeLineId";
        }

        /// <summary>
        /// Gets a CSV row for an account (copied from POCO implementation)
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

        #endregion
    }
}
