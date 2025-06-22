using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Services.Documents
{
    /// <summary>
    /// Interface for importing and exporting document accounting profiles from/to CSV format
    /// </summary>
    public interface IDocumentAccountingProfileImportExportService
    {
        /// <summary>
        /// Imports document accounting profiles from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported profiles and any validation errors</returns>
        Task<(IEnumerable<DocumentAccountingProfileDto> ImportedProfiles, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports document accounting profiles to a CSV format
        /// </summary>
        /// <param name="profiles">Profiles to export</param>
        /// <returns>CSV content as a string</returns>
        Task<string> ExportToCsvAsync(IEnumerable<DocumentAccountingProfileDto> profiles);
    }
}
