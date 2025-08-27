using Sivar.Erp.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.Core.Contracts.ImportExport
{
    /// <summary>
    /// Core contract interface for item import/export operations.
    /// This interface defines the contract that all item import/export implementations must follow,
    /// regardless of whether they are in Infrastructure or Modules layers.
    /// </summary>
    public interface IItemImportExportService
    {
        /// <summary>
        /// Imports items from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported items and any validation errors</returns>
        Task<(IEnumerable<IItem> ImportedItems, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports items to a CSV format
        /// </summary>
        /// <param name="items">Items to export</param>
        /// <returns>CSV content as a string</returns>
        Task<string> ExportToCsvAsync(IEnumerable<IItem> items);
    }
}
