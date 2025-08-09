using System.ComponentModel;
using Sivar.Erp.Core.Modules.Documents;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Service for managing accounting operations
    /// </summary>
    [Description("Service for managing accounting operations")]
    public interface IAccountingService
    {
        /// <summary>
        /// Creates a transaction from a document
        /// </summary>
        /// <param name="document">The source document</param>
        /// <param name="description">Optional description for the transaction</param>
        /// <returns>The created transaction</returns>
        [Description("Creates a transaction from a document")]
        Task<ITransaction> CreateTransactionAsync(IDocument document, string? description = null);
        
        /// <summary>
        /// Posts a transaction to the ledger
        /// </summary>
        /// <param name="transaction">The transaction to post</param>
        [Description("Posts a transaction to the ledger")]
        Task PostTransactionAsync(ITransaction transaction);
        
        /// <summary>
        /// Calculates account balances
        /// </summary>
        /// <param name="accountCode">The account code to calculate balances for</param>
        /// <param name="asOfDate">Optional date to calculate balances as of</param>
        /// <returns>The account balance</returns>
        [Description("Calculates account balances")]
        Task<decimal> CalculateAccountBalanceAsync(string accountCode, DateTime? asOfDate = null);
    }
}