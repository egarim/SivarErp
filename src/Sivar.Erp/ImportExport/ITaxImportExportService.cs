using Sivar.Erp.Documents.Tax;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.ImportExport
{
    /// <summary>
    /// Interface for tax import/export operations
    /// </summary>
    public interface ITaxImportExportService
    {
        /// <summary>
        /// Imports taxes from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported taxes and any validation errors</returns>
        Task<(IEnumerable<TaxDto> ImportedTaxes, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports taxes to a CSV format
        /// </summary>
        /// <param name="taxes">Taxes to export</param>
        /// <returns>CSV content as a string</returns>
        Task<string> ExportToCsvAsync(IEnumerable<TaxDto> taxes);
    }
}