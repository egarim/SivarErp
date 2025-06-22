using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Services.Documents
{
    /// <summary>
    /// Service for importing and exporting document accounting profiles from/to CSV format
    /// </summary>
    public class DocumentAccountingProfileImportExportService : IDocumentAccountingProfileImportExportService
    {
        private readonly ILogger<DocumentAccountingProfileImportExportService> _logger;

        public DocumentAccountingProfileImportExportService(ILogger<DocumentAccountingProfileImportExportService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Imports document accounting profiles from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported profiles and any validation errors</returns>
        public async Task<(IEnumerable<DocumentAccountingProfileDto> ImportedProfiles, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            var profiles = new List<DocumentAccountingProfileDto>();
            var errors = new List<string>();

            try
            {
                // Skip header line
                using var reader = new StringReader(csvContent);
                string? header = await reader.ReadLineAsync();
                if (header == null)
                {
                    errors.Add("CSV file is empty or invalid.");
                    return (profiles, errors);
                }

                // Validate header
                var expectedColumns = new[] {
                    "DocumentOperation", "SalesAccountCode", "AccountsReceivableCode",
                    "InventoryAccountCode", "CostOfGoodsSoldAccountCode", "CostRatio"
                };

                var headerColumns = header.Split(',');
                foreach (var column in expectedColumns)
                {
                    if (!headerColumns.Contains(column))
                    {
                        errors.Add($"Required column '{column}' missing from CSV header.");
                    }
                }

                if (errors.Any())
                {
                    return (profiles, errors);
                }

                // Process rows
                string? line;
                int lineNumber = 1;  // start at 1 because we already read the header

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;
                    try
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;  // Skip empty lines
                        }

                        var columns = line.Split(',');
                        if (columns.Length < expectedColumns.Length)
                        {
                            errors.Add($"Line {lineNumber}: Not enough columns. Expected {expectedColumns.Length}, found {columns.Length}.");
                            continue;
                        }

                        // Extract values (matching the expected column order)
                        string documentOperation = columns[0];
                        string salesAccountCode = columns[1];
                        string accountsReceivableCode = columns[2];
                        string inventoryAccountCode = columns[3];
                        string costOfGoodsSoldAccountCode = columns[4];
                        string costRatioStr = columns[5];

                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(documentOperation))
                        {
                            errors.Add($"Line {lineNumber}: DocumentOperation is required.");
                            continue;
                        }

                        // Parse cost ratio
                        decimal costRatio = 0;
                        if (!string.IsNullOrWhiteSpace(costRatioStr) &&
                            !decimal.TryParse(costRatioStr, out costRatio))
                        {
                            errors.Add($"Line {lineNumber}: Invalid CostRatio format '{costRatioStr}'.");
                            continue;
                        }

                        // Create profile
                        var profile = new DocumentAccountingProfileDto
                        {
                            DocumentOperation = documentOperation,
                            SalesAccountCode = salesAccountCode,
                            AccountsReceivableCode = accountsReceivableCode,
                            InventoryAccountCode = inventoryAccountCode,
                            CostOfGoodsSoldAccountCode = costOfGoodsSoldAccountCode,
                            CostRatio = costRatio,
                            CreatedBy = userName,
                            CreatedDate = DateTimeOffset.Now
                        };

                        profiles.Add(profile);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Line {lineNumber}: Error processing record: {ex.Message}");
                        _logger.LogError(ex, "Error processing document accounting profile at line {LineNumber}", lineNumber);
                    }
                }

                _logger.LogInformation("Imported {Count} document accounting profiles with {ErrorCount} errors",
                    profiles.Count, errors.Count);
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing document accounting profiles: {ex.Message}");
                _logger.LogError(ex, "Error importing document accounting profiles from CSV");
            }

            return (profiles, errors);
        }

        /// <summary>
        /// Exports document accounting profiles to a CSV format
        /// </summary>
        /// <param name="profiles">Profiles to export</param>
        /// <returns>CSV content as a string</returns>
        public async Task<string> ExportToCsvAsync(IEnumerable<DocumentAccountingProfileDto> profiles)
        {
            // Use TaskCompletionSource to make this method truly async
            var tcs = new TaskCompletionSource<string>();

            try
            {
                var sb = new StringBuilder();

                // Add header
                sb.AppendLine("DocumentOperation,SalesAccountCode,AccountsReceivableCode,InventoryAccountCode,CostOfGoodsSoldAccountCode,CostRatio");

                // Add rows
                foreach (var profile in profiles)
                {
                    sb.AppendLine(string.Join(",",
                        Escape(profile.DocumentOperation),
                        Escape(profile.SalesAccountCode),
                        Escape(profile.AccountsReceivableCode),
                        Escape(profile.InventoryAccountCode),
                        Escape(profile.CostOfGoodsSoldAccountCode),
                        profile.CostRatio.ToString("G")
                    ));
                }

                // Complete the task with the CSV content
                tcs.SetResult(sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting document accounting profiles to CSV");
                tcs.SetException(ex);
            }

            return await tcs.Task;
        }

        /// <summary>
        /// Escapes a string for CSV format
        /// </summary>
        private string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                value = value.Replace("\"", "\"\"");
                value = $"\"{value}\"";
            }

            return value;
        }
    }
}
