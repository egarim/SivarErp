using Sivar.Erp.Documents;
using Sivar.Erp.Services.Taxes;
using Sivar.Erp.Services.Taxes.TaxAccountingProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Interface for importing and exporting tax accounting profiles
    /// </summary>
    public interface ITaxAccountingProfileImportExportService
    {
        /// <summary>
        /// Imports tax accounting profiles from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content containing tax accounting profiles</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Collection of imported profiles and any validation errors</returns>
        Task<(IEnumerable<TaxAccountingProfile> ImportedProfiles, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports tax accounting profiles to CSV format
        /// </summary>
        /// <param name="profiles">Tax accounting profiles to export</param>
        /// <returns>CSV content as a string</returns>
        Task<string> ExportToCsvAsync(IEnumerable<TaxAccountingProfile> profiles);
    }
}