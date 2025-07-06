using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.ImportExport
{
    /// <summary>
    /// Service for importing and exporting tax groups (.NET 9 Infrastructure)
    /// </summary>
    public interface ITaxGroupImportExportService
    {
        /// <summary>
        /// Imports tax groups from CSV content
        /// </summary>
        Task<(IEnumerable<ITaxGroup> ImportedTaxGroups, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports tax groups to CSV format
        /// </summary>
        Task<string> ExportToCsvAsync(IEnumerable<ITaxGroup> taxGroups);
    }
}