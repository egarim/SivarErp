using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.ImportExport
{
    /// <summary>
    /// Interface for account import/export operations
    /// </summary>
    public interface IAccountImportExportService
    {
        /// <summary>
        /// Imports accounts from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content to import</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Imported accounts and any validation errors</returns>
        Task<(IEnumerable<IAccount> ImportedAccounts, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);
        
        /// <summary>
        /// Exports accounts to CSV format
        /// </summary>
        /// <param name="accounts">Accounts to export</param>
        /// <returns>CSV content</returns>
        Task<string> ExportToCsvAsync(IEnumerable<IAccount> accounts);
    }
}