﻿using System;
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
        string TransactionNumber { get; set; }
        [BusinessKey]
        string LedgerEntryNumber { get; set; }

        /// <summary>
        /// Type of entry (debit or credit)
        /// </summary>
        EntryType EntryType { get; set; }
        /// <summary>
        /// Amount of the entry
        /// </summary>
        decimal Amount { get; set; }
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
