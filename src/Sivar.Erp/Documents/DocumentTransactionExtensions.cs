using Sivar.Erp.Accounting.Transactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Extension methods for document transaction generation
    /// </summary>
    public static class DocumentTransactionExtensions
    {
        /// <summary>
        /// Generates a transaction from the document
        /// </summary>
        /// <param name="document">The document</param>
        /// <param name="generator">The transaction generator</param>
        /// <returns>A tuple containing the transaction and ledger entries</returns>
        public static async Task<(TransactionDto Transaction, List<LedgerEntryDto> LedgerEntries)> 
            GenerateTransactionAsync(this DocumentDto document, DocumentTransactionGenerator generator)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
                
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));
                
            return await generator.GenerateTransactionAsync(document);
        }
        
        /// <summary>
        /// Generates and persists a transaction from the document
        /// </summary>
        /// <param name="document">The document</param>
        /// <param name="generator">The transaction generator</param>
        /// <returns>The generated transaction</returns>
        public static async Task<TransactionDto> GenerateAndSaveTransactionAsync(
            this DocumentDto document, DocumentTransactionGenerator generator)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
                
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));
                
            return await generator.GenerateAndSaveTransactionAsync(document);
        }
    }
}