using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Service for importing and exporting test account mappings
    /// </summary>
    public class TestAccountMappingImportExportService : ITestAccountMappingImportExportService
    {
        /// <summary>
        /// Imports account mappings from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content with LogicalName,AccountCode,Description columns</param>
        /// <param name="userName">Username for audit purposes</param>
        /// <returns>Dictionary of logical names to account codes and any import errors</returns>
        public Task<(Dictionary<string, string> AccountMappings, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            var accountMappings = new Dictionary<string, string>();
            var errors = new List<string>();

            try
            {
                if (string.IsNullOrWhiteSpace(csvContent))
                {
                    errors.Add("CSV content is empty");
                    return Task.FromResult<(Dictionary<string, string>, IEnumerable<string>)>((accountMappings, errors));
                }

                var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length < 2)
                {
                    errors.Add("CSV must contain at least a header and one data row");
                    return Task.FromResult<(Dictionary<string, string>, IEnumerable<string>)>((accountMappings, errors));
                }

                var headers = lines[0].Split(',');

                // Validate headers
                if (headers.Length < 2 ||
                    !headers[0].Trim().Equals("LogicalName", StringComparison.OrdinalIgnoreCase) ||
                    !headers[1].Trim().Equals("AccountCode", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add("CSV must have LogicalName,AccountCode,Description columns");
                    return Task.FromResult<(Dictionary<string, string>, IEnumerable<string>)>((accountMappings, errors));
                }

                // Process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    var fields = lines[i].Split(',');

                    if (fields.Length < 2)
                    {
                        errors.Add($"Line {i + 1}: Insufficient columns. Expected at least 2, got {fields.Length}");
                        continue;
                    }

                    var logicalName = fields[0].Trim();
                    var accountCode = fields[1].Trim();

                    if (string.IsNullOrEmpty(logicalName))
                    {
                        errors.Add($"Line {i + 1}: LogicalName cannot be empty");
                        continue;
                    }

                    if (string.IsNullOrEmpty(accountCode))
                    {
                        errors.Add($"Line {i + 1}: AccountCode cannot be empty");
                        continue;
                    }

                    if (accountMappings.ContainsKey(logicalName))
                    {
                        errors.Add($"Line {i + 1}: Duplicate LogicalName '{logicalName}'");
                        continue;
                    }

                    accountMappings[logicalName] = accountCode;
                }

                return Task.FromResult<(Dictionary<string, string>, IEnumerable<string>)>((accountMappings, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(Dictionary<string, string>, IEnumerable<string>)>((accountMappings, errors));
            }
        }

        /// <summary>
        /// Imports account mappings from a CSV file
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <param name="userName">Username for audit purposes</param>
        /// <returns>Dictionary of logical names to account codes and any import errors</returns>
        public async Task<(Dictionary<string, string> AccountMappings, IEnumerable<string> Errors)> ImportFromFileAsync(string filePath, string userName)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return (new Dictionary<string, string>(), new[] { $"File not found: {filePath}" });
                }

                var csvContent = await File.ReadAllTextAsync(filePath);
                return await ImportFromCsvAsync(csvContent, userName);
            }
            catch (Exception ex)
            {
                return (new Dictionary<string, string>(), new[] { $"Error reading file: {ex.Message}" });
            }
        }

        /// <summary>
        /// Exports account mappings to CSV format
        /// </summary>
        /// <param name="accountMappings">Dictionary of logical names to account codes</param>
        /// <param name="descriptions">Optional dictionary of logical names to descriptions</param>
        /// <returns>CSV content as string</returns>
        public Task<string> ExportToCsvAsync(Dictionary<string, string> accountMappings, Dictionary<string, string> descriptions = null)
        {
            if (accountMappings == null || !accountMappings.Any())
            {
                return Task.FromResult("LogicalName,AccountCode,Description");
            }

            var lines = new List<string> { "LogicalName,AccountCode,Description" };

            foreach (var mapping in accountMappings.OrderBy(x => x.Key))
            {
                var description = descriptions?.GetValueOrDefault(mapping.Key, "") ?? "";
                lines.Add($"{mapping.Key},{mapping.Value},{description}");
            }

            return Task.FromResult(string.Join(Environment.NewLine, lines));
        }
    }
}