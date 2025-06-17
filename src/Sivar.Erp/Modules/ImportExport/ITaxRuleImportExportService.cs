using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Services.Taxes.TaxRule;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Interface for importing and exporting tax rules from/to external formats
    /// </summary>
    public interface ITaxRuleImportExportService
    {
        /// <summary>
        /// Imports tax rules from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported tax rules and any validation errors</returns>
        Task<(IEnumerable<ITaxRule> ImportedTaxRules, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports tax rules to a CSV format
        /// </summary>
        /// <param name="taxRules">Tax rules to export</param>
        /// <returns>CSV content as a string</returns>
        Task<string> ExportToCsvAsync(IEnumerable<ITaxRule> taxRules);
    }
}