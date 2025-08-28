using System;
using System.Collections.Generic;

namespace Sivar.Erp.Modules.Accounting.Transactions
{
    /// <summary>
    /// Interface for transaction batches
    /// </summary>
    public interface ITransactionBatch 
    {
        /// <summary>
        /// Batch reference code
        /// </summary>
        string ReferenceCode { get; set; }



        string TransactionNumber { get; set; }
        /// <summary>
        /// Date when the batch was created
        /// </summary>
        DateOnly BatchDate { get; set; }

        /// <summary>
        /// Description of the transaction batch
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Status of the batch
        /// </summary>
        BatchStatus Status { get; set; }

        void Post();
        void UnPost();
        bool IsPosted { get; set; }
    }
}
