using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.ImportExport
{
    /// <summary>
    /// Service for importing and exporting group memberships (.NET 9 Infrastructure)
    /// </summary>
    public interface IGroupMembershipImportExportService
    {
        /// <summary>
        /// Imports group memberships from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content to import</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Imported group memberships and any validation errors</returns>
        Task<(IEnumerable<IGroupMembership> ImportedGroupMemberships, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);
        
        /// <summary>
        /// Exports group memberships to CSV format
        /// </summary>
        /// <param name="groupMemberships">Group memberships to export</param>
        /// <returns>CSV content</returns>
        Task<string> ExportToCsvAsync(IEnumerable<IGroupMembership> groupMemberships);
    }
}