namespace Sivar.Erp.ChartOfAccounts
{
    /// <summary>
    /// Interface for chart of accounts import/export operations
    /// </summary>
    public interface IAccountImportExportService
    {
        /// <summary>
        /// Imports accounts from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported accounts and any validation errors</returns>
        Task<(IEnumerable<IAccount> ImportedAccounts, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports accounts to a CSV format
        /// </summary>
        /// <param name="accounts">Accounts to export</param>
        /// <returns>CSV content as a string</returns>
        Task<string> ExportToCsvAsync(IEnumerable<IAccount> accounts);
    }
}