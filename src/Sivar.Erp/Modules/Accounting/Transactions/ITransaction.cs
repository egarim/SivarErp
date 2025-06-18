using System;
using System.Linq;

namespace Sivar.Erp.Services.Accounting.Transactions
{
    /// <summary>
    /// Interface for financial transactions
    /// </summary>
    public interface ITransaction : IEntity
    {
        /// <summary>
        /// Reference to the parent document
        /// </summary>
        string DocumentNumber { get; set; }

        [BusinessKey()]
        string TransactionNumber { get; set; }

        /// <summary>
        /// Date of the transaction (may differ from document date)
        /// </summary>
        DateOnly TransactionDate { get; set; }

        /// <summary>
        /// Description of the transaction
        /// </summary>
        string Description { get; set; }

        IEnumerable<ILedgerEntry> LedgerEntries { get; set; }
        void Post();
        void UnPost();
        bool IsPosted { get; set; }
    }
}
