using Sivar.Erp.Documents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Interface for business entity import/export operations
    /// </summary>
    public interface IBusinessEntityImportExportService
    {
        /// <summary>
        /// Imports business entities from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported business entities and any validation errors</returns>
        Task<(IEnumerable<IBusinessEntity> ImportedBusinessEntities, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports business entities to a CSV format
        /// </summary>
        /// <param name="businessEntities">Business entities to export</param>
        /// <returns>CSV content as a string</returns>
        Task<string> ExportToCsvAsync(IEnumerable<IBusinessEntity> businessEntities);
    }
}