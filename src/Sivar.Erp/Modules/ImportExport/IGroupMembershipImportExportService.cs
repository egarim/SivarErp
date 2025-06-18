using Sivar.Erp.Services.Taxes.TaxGroup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Interface for group membership import/export operations
    /// </summary>
    public interface IGroupMembershipImportExportService
    {
        /// <summary>
        /// Imports group memberships from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported group memberships and any validation errors</returns>
        Task<(IEnumerable<GroupMembershipDto> ImportedMemberships, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports group memberships to a CSV format
        /// </summary>
        /// <param name="memberships">Group memberships to export</param>
        /// <returns>CSV content as a string</returns>
        Task<string> ExportToCsvAsync(IEnumerable<GroupMembershipDto> memberships);
    }
}