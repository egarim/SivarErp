using Sivar.Erp.Core.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.Core.Contracts.ImportExport
{
    /// <summary>
    /// Core contract interface for document accounting profile import/export operations.
    /// This interface defines the contract that all document accounting profile import/export implementations must follow,
    /// regardless of whether they are in Infrastructure or Modules layers.
    /// </summary>
    public interface IDocumentAccountingProfileImportExportService
    {
        /// <summary>
        /// Imports document accounting profiles from CSV
        /// </summary>
        /// <param name="csvContent">The CSV content to import</param>
        /// <param name="userName">The user importing the profiles</param>
        /// <returns>A tuple of imported profiles and any errors</returns>
        Task<(IList<IDocumentAccountingProfile> ImportedProfiles, IList<string> Errors)> ImportFromCsvAsync(
            string csvContent,
            string userName);

        /// <summary>
        /// Exports document accounting profiles to CSV
        /// </summary>
        /// <param name="profiles">The profiles to export</param>
        /// <returns>The CSV content</returns>
        Task<string> ExportToCsvAsync(IList<IDocumentAccountingProfile> profiles);
    }
}
