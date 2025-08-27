using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.ImportExport
{
    /// <summary>
    /// Service for importing and exporting taxes (.NET 9 Infrastructure)
    /// </summary>
    public interface ITaxImportExportService
    {
        /// <summary>
        /// Imports taxes from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content to import</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Imported taxes and any validation errors</returns>
        Task<(IEnumerable<ITax> ImportedTaxes, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);
        
        /// <summary>
        /// Exports taxes to CSV format
        /// </summary>
        /// <param name="taxes">Taxes to export</param>
        /// <returns>CSV content</returns>
        Task<string> ExportToCsvAsync(IEnumerable<ITax> taxes);
    }
}