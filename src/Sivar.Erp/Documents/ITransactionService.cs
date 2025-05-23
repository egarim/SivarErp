﻿namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Interface for transaction service operations
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Creates a new transaction
        /// </summary>
        /// <param name="transaction">Transaction to create</param>
        /// <returns>Created transaction with ID</returns>
        Task<ITransaction> CreateTransactionAsync(ITransaction transaction);

        /// <summary>
        /// Validates a transaction for accounting balance
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="entries">Ledger entries for the transaction</param>
        /// <returns>True if valid, false otherwise</returns>
        Task<bool> ValidateTransactionAsync(Guid transactionId, IEnumerable<ILedgerEntry> entries);

        /// <summary>
        /// Retrieves all transactions for a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Collection of transactions</returns>
        Task<IEnumerable<ITransaction>> GetTransactionsByDocumentIdAsync(Guid documentId);

        /// <summary>
        /// Retrieves all ledger entries for a transaction
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <returns>Collection of ledger entries</returns>
        Task<IEnumerable<ILedgerEntry>> GetLedgerEntriesByTransactionIdAsync(Guid transactionId);
    }
}