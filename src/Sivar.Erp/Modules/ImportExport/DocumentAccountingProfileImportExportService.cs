using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.TimeService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Service for importing and exporting document accounting profiles
    /// </summary>
    public class DocumentAccountingProfileImportExportService : IDocumentAccountingProfileImportExportService
    {
        private readonly IDateTimeZoneService _dateTimeService;
        private readonly ILogger<DocumentAccountingProfileImportExportService> _logger;

        // CSV header
        private const string CSV_HEADER = "DocumentOperation,SalesAccountCode,AccountsReceivableCode,CostOfGoodsSoldAccountCode,InventoryAccountCode,CostRatio";

        /// <summary>
        /// Initializes a new instance of the DocumentAccountingProfileImportExportService class
        /// </summary>
        /// <param name="dateTimeService">The date/time service</param>
        /// <param name="logger">The logger</param>
        public DocumentAccountingProfileImportExportService(
            IDateTimeZoneService dateTimeService,
            ILogger<DocumentAccountingProfileImportExportService> logger)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Imports document accounting profiles from CSV
        /// </summary>
        public async Task<(IList<IDocumentAccountingProfile> ImportedProfiles, IList<string> Errors)> ImportFromCsvAsync(
            string csvContent,
            string userName)
        {
            var importedProfiles = new List<IDocumentAccountingProfile>();
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(csvContent))
            {
                errors.Add("CSV content is empty");
                return (importedProfiles, errors);
            }

            try
            {
                // Parse CSV content
                using var reader = new StringReader(csvContent);
                string line;
                int lineNumber = 0;

                // Read header
                line = await reader.ReadLineAsync();
                lineNumber++;

                if (line == null || !line.Trim().Equals(CSV_HEADER, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Invalid CSV header. Expected: {CSV_HEADER}");
                    return (importedProfiles, errors);
                }

                // Read data lines
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    try
                    {
                        var profile = ParseProfileFromCsvLine(line, userName);
                        importedProfiles.Add(profile);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error parsing line {lineNumber}: {ex.Message}");
                    }
                }

                _logger.LogInformation("Imported {Count} document accounting profiles", importedProfiles.Count);
                return (importedProfiles, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing document accounting profiles from CSV");
                errors.Add($"Import error: {ex.Message}");
                return (importedProfiles, errors);
            }
        }

        /// <summary>
        /// Exports document accounting profiles to CSV
        /// </summary>
        public async Task<string> ExportToCsvAsync(IList<IDocumentAccountingProfile> profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException(nameof(profiles));
            }

            try
            {
                var sb = new StringBuilder();

                // Write header
                sb.AppendLine(CSV_HEADER);

                // Write data
                foreach (var profile in profiles)
                {
                    sb.AppendLine(string.Join(",",
                        profile.DocumentOperation,
                        profile.SalesAccountCode ?? string.Empty,
                        profile.AccountsReceivableCode ?? string.Empty,
                        profile.CostOfGoodsSoldAccountCode ?? string.Empty,
                        profile.InventoryAccountCode ?? string.Empty,
                        profile.CostRatio.ToString(CultureInfo.InvariantCulture)));
                }

                return await Task.FromResult(sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting document accounting profiles to CSV");
                throw;
            }
        }

        private IDocumentAccountingProfile ParseProfileFromCsvLine(string line, string userName)
        {
            var values = line.Split(',');

            if (values.Length < 6)
            {
                throw new FormatException("CSV line does not have enough values");
            }

            decimal costRatio;
            if (!decimal.TryParse(values[5], NumberStyles.Any, CultureInfo.InvariantCulture, out costRatio))
            {
                throw new FormatException("Invalid cost ratio value");
            }

            return new DocumentAccountingProfileDto
            {
                DocumentOperation = values[0],
                SalesAccountCode = values[1],
                AccountsReceivableCode = values[2],
                CostOfGoodsSoldAccountCode = values[3],
                InventoryAccountCode = values[4],
                CostRatio = costRatio,
                CreatedBy = userName,
                CreatedDate = _dateTimeService.Now()
            };
        }
    }
}