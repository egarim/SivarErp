using System;
using System.Collections.Generic;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Interface for transaction batches
    /// </summary>
    public interface ITransactionBatch : IEntity
    {
        /// <summary>
        /// Batch reference code
        /// </summary>
        string ReferenceCode { get; set; }

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
    }
}