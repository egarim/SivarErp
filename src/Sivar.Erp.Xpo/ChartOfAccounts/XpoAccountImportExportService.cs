using DevExpress.Xpo;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.Xpo.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Xpo.ChartOfAccounts
{
    /// <summary>
    /// Service for importing and exporting accounts using XPO
    /// </summary>
    public class XpoAccountImportExportService
    {
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the service
        /// </summary>
        /// <param name="auditService">Audit service</param>
        public XpoAccountImportExportService(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Imports accounts from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Tuple containing imported accounts and any errors</returns>
        public async Task<(IEnumerable<IAccount> ImportedAccounts, List<string> Errors)> ImportFromCsvAsync(
            string csvContent, string userName)
        {
            var errors = new List<string>();
            var importedAccounts = new List<XpoAccount>();

            if (string.IsNullOrWhiteSpace(csvContent))
            {
                errors.Add("CSV content is empty");
                return (importedAccounts, errors);
            }

            // Parse CSV content
            using var reader = new StringReader(csvContent);
            var headerLine = await reader.ReadLineAsync();

            if (headerLine == null)
            {
                errors.Add("CSV content is empty");
                return (importedAccounts, errors);
            }

            // Parse header
            var headers = ParseCsvLine(headerLine);

            // Validate required headers
            if (!headers.Contains("AccountName") || !headers.Contains("OfficialCode") || !headers.Contains("AccountType"))
            {
                errors.Add("CSV is missing required headers: AccountName, OfficialCode, AccountType");
                return (importedAccounts, errors);
            }

            // Create a UnitOfWork for the import
            using var uow = XpoDataAccessService.GetUnitOfWork();

            string? line;
            int lineNumber = 1;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = ParseCsvLine(line);

                if (values.Count != headers.Count)
                {
                    errors.Add($"Line {lineNumber} has {values.Count} values but header has {headers.Count}");
                    continue;
                }

                try
                {
                    // Create a dictionary of header->value
                    var data = new Dictionary<string, string>();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        data[headers[i]] = values[i];
                    }

                    // Create account
                    var account = new XpoAccount(uow)
                    {
                        Id = Guid.NewGuid(),
                        AccountName = data["AccountName"],
                        OfficialCode = data["OfficialCode"],
                        IsArchived = false
                    };

                    // Parse account type
                    if (Enum.TryParse<AccountType>(data["AccountType"], out var accountType))
                    {
                        account.AccountType = accountType;
                    }
                    else
                    {
                        // Default to Asset if type is invalid
                        account.AccountType = AccountType.Asset;
                    }                    // Parse balance and income line ID if present
                    if (data.TryGetValue("BalanceAndIncomeLineId", out var lineIdStr) &&
                        !string.IsNullOrWhiteSpace(lineIdStr) &&
                        Guid.TryParse(lineIdStr, out var lineId))
                    {
                        account.BalanceAndIncomeLineId = lineId;
                    }

                    // Parse parent official code if present
                    if (data.TryGetValue("ParentOfficialCode", out var parentCode) &&
                        !string.IsNullOrWhiteSpace(parentCode))
                    {
                        account.ParentOfficialCode = parentCode;
                        account.ParentAccountCode = parentCode; // Keep both for compatibility
                    }

                    // Set audit information
                    _auditService.SetCreationAudit(account, userName);

                    // Validate account
                    if (!account.Validate())
                    {
                        errors.Add($"Account validation failed on line {lineNumber}");
                        continue;
                    }

                    importedAccounts.Add(account);
                }
                catch (Exception ex)
                {
                    errors.Add($"Error on line {lineNumber}: {ex.Message}");
                }
            }

            if (importedAccounts.Count == 0 && lineNumber <= 1)
            {
                errors.Add("CSV contains no data rows");
                return (importedAccounts, errors);
            }

            // Save all accounts if there were no errors
            if (errors.Count == 0)
            {
                await uow.CommitChangesAsync();
            }

            return (importedAccounts, errors);
        }

        /// <summary>
        /// Exports accounts to CSV
        /// </summary>
        /// <param name="accounts">Accounts to export</param>
        /// <returns>CSV content</returns>
        public async Task<string> ExportToCsvAsync(IEnumerable<IAccount>? accounts)
        {
            var builder = new StringBuilder();            // Write header
            builder.AppendLine("AccountName,OfficialCode,AccountType,ParentOfficialCode,BalanceAndIncomeLineId");

            if (accounts == null || !accounts.Any())
            {
                return builder.ToString();
            }

            // Write data
            foreach (var account in accounts)
            {
                var line = new StringBuilder();                // Escape and quote values if needed
                line.Append(CsvEncode(account.AccountName)).Append(",");
                line.Append(CsvEncode(account.OfficialCode)).Append(",");
                line.Append(CsvEncode(account.AccountType.ToString())).Append(",");
                line.Append(CsvEncode(account.ParentOfficialCode ?? "")).Append(",");
                line.Append(account.BalanceAndIncomeLineId?.ToString() ?? "");

                builder.AppendLine(line.ToString());
            }

            return builder.ToString();
        }

        /// <summary>
        /// Helper method to parse a CSV line
        /// </summary>
        /// <param name="line">CSV line</param>
        /// <returns>List of field values</returns>
        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var inQuotes = false;
            var currentValue = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i < line.Length - 1 && line[i + 1] == '"')
                    {
                        // Handle escaped quote (two double quotes in a row)
                        currentValue.Append('"');
                        i++; // Skip the next quote
                    }
                    else
                    {
                        // Toggle quote mode
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // End of field
                    result.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    // Normal character
                    currentValue.Append(c);
                }
            }

            // Add the last field
            result.Add(currentValue.ToString());

            return result;
        }

        /// <summary>
        /// Helper method to encode a value for CSV
        /// </summary>
        /// <param name="value">Value to encode</param>
        /// <returns>CSV-encoded value</returns>
        private string CsvEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // If value contains comma, newline, or double quote, it needs to be quoted
            if (value.Contains(',') || value.Contains('\n') || value.Contains('"'))
            {
                // Double up any quotes and wrap in quotes
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
    }
}