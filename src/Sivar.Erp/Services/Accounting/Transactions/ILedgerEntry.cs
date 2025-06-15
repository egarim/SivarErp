using System;
using System.Linq;

namespace Sivar.Erp.Services.Accounting.Transactions
{
    /// <summary>
    /// Interface for ledger entries
    /// </summary>
    public interface ILedgerEntry : IEntity
    {
        /// <summary>
        /// Reference to the parent transaction
        /// </summary>
        Guid TransactionId { get; set; }

        /// <summary>
        /// Reference to the account
        /// </summary>
        Guid AccountId { get; set; }

        /// <summary>
        /// Type of entry (debit or credit)
        /// </summary>
        EntryType EntryType { get; set; }

        /// <summary>
        /// Amount of the entry
        /// </summary>
        decimal Amount { get; set; }

        /// <summary>
        /// Optional reference to a person for analysis
        /// </summary>
        Guid? PersonId { get; set; }        /// <summary>
                                            /// Optional reference to a cost center for analysis
                                            /// </summary>
        Guid? CostCentreId { get; set; }

        /// <summary>
        /// Name of the account
        /// </summary>
        string AccountName { get; set; }

        /// <summary>
        /// Official code/identifier for the account
        /// </summary>
        string OfficialCode { get; set; }
    }
}
