﻿using System;
using System.Linq;

namespace Sivar.Erp.Services.Accounting.Transactions
{
    /// <summary>
    /// Enumeration of ledger entry types
    /// </summary>
    public enum EntryType
    {
        /// <summary>
        /// Debit entry
        /// </summary>
        Debit = 'D',

        /// <summary>
        /// Credit entry
        /// </summary>
        Credit = 'C'
    }
}
