using System;
using System.Collections.Generic;

namespace Sivar.Erp.Services.Accounting.Transactions
{
    /// <summary>
    /// Implementation of the transaction batch entity
    /// </summary>
    public class TransactionBatchDto : ITransactionBatch
    {
        /// <summary>
        /// Unique identifier for the transaction batch
        /// </summary>
        public Guid Oid { get; set; }

        /// <summary>
        /// Batch reference code
        /// </summary>
        public string ReferenceCode { get; set; } = string.Empty;

        /// <summary>
        /// Date when the batch was created
        /// </summary>
        public DateOnly BatchDate { get; set; }

        /// <summary>
        /// Description of the transaction batch
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Status of the batch
        /// </summary>
        public BatchStatus Status { get; set; } = BatchStatus.Draft;
        
        /// <summary>
        /// Collection of transactions in this batch
        /// </summary>
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
        public bool IsPosted { get; set; }
        public string TransactionNumber { get; set; }

        public void Post()
        {
           this.IsPosted = true;
        }

        public void UnPost()
        {
            this.IsPosted = false;
        }
    }
}