using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.ImportExport
{
    /// <summary>
    /// Service for importing and exporting document types (.NET 9 Infrastructure)
    /// </summary>
    public interface IDocumentTypeImportExportService
    {
        /// <summary>
        /// Imports document types from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content to import</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Imported document types and any validation errors</returns>
        Task<(IEnumerable<IDocumentType> ImportedDocumentTypes, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports document types to CSV format
        /// </summary>
        /// <param name="documentTypes">Document types to export</param>
        /// <returns>CSV content</returns>
        Task<string> ExportToCsvAsync(IEnumerable<IDocumentType> documentTypes);
    }
}