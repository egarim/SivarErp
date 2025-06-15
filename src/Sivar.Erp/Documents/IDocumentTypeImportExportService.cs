using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Interface for document type import/export operations
    /// </summary>
    public interface IDocumentTypeImportExportService
    {
        /// <summary>
        /// Imports document types from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported document types and any validation errors</returns>
        Task<(IEnumerable<IDocumentType> ImportedDocumentTypes, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports document types to a CSV format
        /// </summary>
        /// <param name="documentTypes">Document types to export</param>
        /// <returns>CSV content as a string</returns>
        Task<string> ExportToCsvAsync(IEnumerable<IDocumentType> documentTypes);
    }
}
