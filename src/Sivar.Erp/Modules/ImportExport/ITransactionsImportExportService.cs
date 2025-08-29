using Sivar.Erp.Modules.Accounting.Transactions;
using System;
using System.Linq;

namespace Sivar.Erp.Modules.ImportExport
{
    public interface ITransactionsImportExportService
    {
        /// <summary>
        /// Exports transactions and their associated ledger entries to CSV format
        /// </summary>
        /// <param name="transactionsWithEntries">List of transactions with their associated ledger entries</param>
        /// <returns>Tuple containing the transactions CSV and ledger entries CSV as strings</returns>
        (string TransactionsCsv, string LedgerEntriesCsv) ExportTransactions(List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> transactionsWithEntries);

        /// <summary>
        /// Exports transactions and their associated ledger entries to CSV format into a single method
        /// </summary>
        /// <param name="transactionsWithEntries">List of transactions with their associated ledger entries</param>
        /// <returns>CSV content as string with both transactions and ledger entries</returns>
        string ExportTransactionsToCsv(List<(ITransaction Transaction, IEnumerable<ILedgerEntry> Entries)> transactionsWithEntries);

        /// <summary>
        /// Imports transactions and their associated ledger entries from a CSV string 
        /// formatted by ExportTransactionsToCsv
        /// </summary>
        /// <param name="csvText">CSV text containing transactions and ledger entries</param>
        /// <returns>List of transactions with their associated ledger entries</returns>
        List<(ITransaction Transaction, IEnumerable<ILedgerEntry> Entries)> ImportFromCsv(string csvText);
    }
}
