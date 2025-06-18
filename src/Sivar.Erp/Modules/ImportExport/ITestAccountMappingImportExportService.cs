using System;
using System.Linq;

namespace Sivar.Erp.Services.ImportExport
{
    public interface ITestAccountMappingImportExportService
    {
        /// <summary>
        /// Exports account mappings to CSV format
        /// </summary>
        /// <param name="accountMappings">Dictionary of logical names to account codes</param>
        /// <param name="descriptions">Optional dictionary of logical names to descriptions</param>
        /// <returns>CSV content as string</returns>
        Task<string> ExportToCsvAsync(Dictionary<string, string> accountMappings, Dictionary<string, string> descriptions = null);

        /// <summary>
        /// Imports account mappings from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content with LogicalName,AccountCode,Description columns</param>
        /// <param name="userName">Username for audit purposes</param>
        /// <returns>Dictionary of logical names to account codes and any import errors</returns>
        Task<(Dictionary<string, string> AccountMappings, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Imports account mappings from a CSV file
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <param name="userName">Username for audit purposes</param>
        /// <returns>Dictionary of logical names to account codes and any import errors</returns>
        Task<(Dictionary<string, string> AccountMappings, IEnumerable<string> Errors)> ImportFromFileAsync(string filePath, string userName);
    }
}