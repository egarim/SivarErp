using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.ImportExport
{
    /// <summary>
    /// Service for importing and exporting business entities (.NET 9 Infrastructure)
    /// </summary>
    public interface IBusinessEntityImportExportService
    {
        /// <summary>
        /// Imports business entities from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content to import</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Imported business entities and any validation errors</returns>
        Task<(IEnumerable<IBusinessEntity> ImportedBusinessEntities, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);
        
        /// <summary>
        /// Exports business entities to CSV format
        /// </summary>
        /// <param name="businessEntities">Business entities to export</param>
        /// <returns>CSV content</returns>
        Task<string> ExportToCsvAsync(IEnumerable<IBusinessEntity> businessEntities);
    }
}