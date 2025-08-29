using Sivar.Erp.Modules.Accounting.Transactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.Modules.ImportExport
{
    /// <summary>
    /// Interface for importing and exporting transactions and ledger entries
    /// </summary>
    public interface ITransactionImportExportService
    {
        /// <summary>
        /// Exports transactions and their associated ledger entries to CSV format
        /// </summary>
        /// <param name="transactionsWithEntries">List of transactions with their associated ledger entries</param>
        /// <returns>Tuple containing the transactions CSV and ledger entries CSV as strings</returns>
        Task<(string TransactionsCsv, string LedgerEntriesCsv)> ExportTransactionsAsync(
            List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> transactionsWithEntries);

        /// <summary>
        /// Exports transactions and their associated ledger entries to a single CSV format
        /// </summary>
        /// <param name="transactionsWithEntries">List of transactions with their associated ledger entries</param>
        /// <returns>CSV content as string with both transactions and ledger entries</returns>
        Task<string> ExportTransactionsToCsvAsync(
            List<(ITransaction Transaction, IEnumerable<ILedgerEntry> Entries)> transactionsWithEntries);

        /// <summary>
        /// Imports transactions and their associated ledger entries from a CSV string 
        /// formatted by ExportTransactionsToCsv
        /// </summary>
        /// <param name="csvText">CSV text containing transactions and ledger entries</param>
        /// <returns>List of transactions with their associated ledger entries and any import errors</returns>
        Task<(List<(ITransaction Transaction, IEnumerable<ILedgerEntry> Entries)> ImportedData, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvText);
    }
}
