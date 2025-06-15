using Sivar.Erp.Taxes.TaxGroup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.ImportExport
{
    /// <summary>
    /// Interface for tax group import/export operations
    /// </summary>
    public interface ITaxGroupImportExportService
    {
        /// <summary>
        /// Imports tax groups from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported tax groups and any validation errors</returns>
        Task<(IEnumerable<ITaxGroup> ImportedTaxGroups, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports tax groups to a CSV format
        /// </summary>
        /// <param name="taxGroups">Tax groups to export</param>
        /// <returns>CSV content as a string</returns>
        Task<string> ExportToCsvAsync(IEnumerable<ITaxGroup> taxGroups);
    }
}