using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.ImportExport
{
    /// <summary>
    /// Service for importing and exporting items (.NET 9 Infrastructure)
    /// </summary>
    public interface IItemImportExportService
    {
        /// <summary>
        /// Imports items from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content to import</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Imported items and any validation errors</returns>
        Task<(IEnumerable<IItem> ImportedItems, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);
        
        /// <summary>
        /// Exports items to CSV format
        /// </summary>
        /// <param name="items">Items to export</param>
        /// <returns>CSV content</returns>
        Task<string> ExportToCsvAsync(IEnumerable<IItem> items);
    }
}