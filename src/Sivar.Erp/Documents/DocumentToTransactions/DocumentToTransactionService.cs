using Sivar.Erp.Services.Accounting.Transactions;

namespace Sivar.Erp.Documents.DocumentToTransactions
{
    /// <summary>
    /// Implementation of transaction service
    /// </summary>
    public class DocumentToTransactionService : IDocumentToTransactionService
    {
       

        /// <summary>
        /// Validates a transaction for accounting balance
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="entries">Ledger entries for the transaction</param>
        /// <returns>True if valid, false otherwise</returns>
        public Task<bool> ValidateTransactionAsync(Guid transactionId, IEnumerable<ILedgerEntry> entries)
        {
            // Validate transaction has entries
            if (entries == null || !entries.Any())
            {
                return Task.FromResult(false);
            }

            // Calculate total debits and credits
            decimal totalDebits = entries
                .Where(e => e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            decimal totalCredits = entries
                .Where(e => e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            // Transaction is valid if debits equal credits
            return Task.FromResult(Math.Abs(totalDebits - totalCredits) < 0.01m);
        }

        /// <summary>
        /// Retrieves all transactions for a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Collection of transactions</returns>
        public Task<IEnumerable<ITransaction>> GetTransactionsByDocumentIdAsync(Guid documentId)
        {
            // Here would be the repository call to get transactions
            // For this example, we'll return an empty list

            return Task.FromResult<IEnumerable<ITransaction>>(new List<ITransaction>());
        }

        /// <summary>
        /// Retrieves all ledger entries for a transaction
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <returns>Collection of ledger entries</returns>
        public Task<IEnumerable<ILedgerEntry>> GetLedgerEntriesByTransactionIdAsync(Guid transactionId)
        {
            // Here would be the repository call to get ledger entries
            // For this example, we'll return an empty list

            return Task.FromResult<IEnumerable<ILedgerEntry>>(new List<ILedgerEntry>());
        }
    }
}