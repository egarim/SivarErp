using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Modules.Domain;
using System.ComponentModel;
using System.Globalization;

namespace Sivar.Erp.Core.Modules.DataImport.Importers
{
    /// <summary>
    /// Specialized importer for chart of accounts
    /// </summary>
    [Description("Specialized importer for chart of accounts")]
    public class AccountImporter : IEntityImporter<IAccount>
    {
        private readonly ILogger<AccountImporter>? _logger;

        /// <summary>
        /// Initializes a new instance of the AccountImporter class
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public AccountImporter(ILogger<AccountImporter>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<EntityImportResult<IAccount>> ImportAsync(
            IRepository repository,
            string csvContent,
            string userName)
        {
            _logger?.LogInformation("Starting account import");
            
            var result = new EntityImportResult<IAccount>();
            
            try
            {
                // Split the CSV into lines
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                
                if (lines.Length <= 1)
                {
                    result.Errors.Add("CSV file contains no data rows");
                    return result;
                }
                
                // Parse header
                string[] headers = ParseCsvLine(lines[0]);
                
                // Validate headers
                if (!ValidateHeaders(headers, result.Errors))
                {
                    return result;
                }
                
                // Process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    
                    string[] fields = ParseCsvLine(lines[i]);
                    
                    if (fields.Length != headers.Length)
                    {
                        result.Errors.Add($"Line {i + 1}: Field count mismatch. Expected {headers.Length}, got {fields.Length}");
                        continue;
                    }
                    
                    try
                    {
                        var account = repository.CreateObject<AccountDto>();
                        
                        // Map fields to properties
                        for (int j = 0; j < headers.Length; j++)
                        {
                            SetAccountProperty(account, headers[j], fields[j]);
                        }
                        
                        // Validate account
                        var validationErrors = ValidateAccount(account);
                        if (validationErrors.Any())
                        {
                            foreach (var error in validationErrors)
                            {
                                result.Errors.Add($"Line {i + 1}: {error}");
                            }
                            continue;
                        }
                        
                        // Set additional properties
                        account.IsActive = true;
                        
                        // Add to result as IAccount
                        result.ImportedEntities.Add(account);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Line {i + 1}: {ex.Message}");
                    }
                }
                
                result.TotalProcessed = lines.Length - 1;
                result.Success = result.Errors.Count == 0;
                
                if (result.Success)
                {
                    await repository.CommitChanges();
                    _logger?.LogInformation("Imported {Count} accounts successfully", result.ImportedEntities.Count);
                }
                else
                {
                    _logger?.LogWarning("Account import had {ErrorCount} errors", result.Errors.Count);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Import failed: {ex.Message}");
                _logger?.LogError(ex, "Account import failed");
            }
            
            return result;
        }
        
        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var inQuotes = false;
            var field = new System.Text.StringBuilder();
            
            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }
            
            result.Add(field.ToString());
            return result.ToArray();
        }
        
        private bool ValidateHeaders(string[] headers, List<string> errors)
        {
            var requiredHeaders = new[] { "AccountName", "OfficialCode", "AccountType" };
            var missingHeaders = requiredHeaders.Where(h => !headers.Contains(h, StringComparer.OrdinalIgnoreCase)).ToList();
            
            if (missingHeaders.Any())
            {
                errors.Add($"Missing required headers: {string.Join(", ", missingHeaders)}");
                return false;
            }
            
            return true;
        }
        
        private void SetAccountProperty(AccountDto account, string propertyName, string value)
        {
            switch (propertyName.ToLowerInvariant())
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
                        throw new FormatException($"Invalid account type: {value}");
                    }
                    break;
                case "parentofficialcode":
                    account.ParentAccountCode = string.IsNullOrWhiteSpace(value) ? null : value;
                    break;
                case "description":
                    account.Description = value;
                    break;
            }
        }
        
        private List<string> ValidateAccount(AccountDto account)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(account.OfficialCode))
            {
                errors.Add("Account code is required");
            }
            
            if (string.IsNullOrWhiteSpace(account.AccountName))
            {
                errors.Add("Account name is required");
            }
            
            return errors;
        }
    }
}