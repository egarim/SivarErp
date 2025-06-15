namespace Sivar.Erp.Services.Accounting.Transactions
{
    /// <summary>
    /// Implementation of the transaction entity
    /// </summary>
    public class TransactionDto : ITransaction
    {
        /// <summary>
        /// Unique identifier for the transaction
        /// </summary>
        public Guid Oid { get; set; }

        /// <summary>
        /// Reference to the parent document
        /// </summary>
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Date of the transaction (may differ from document date)
        /// </summary>
        public DateOnly TransactionDate { get; set; }

        /// <summary>
        /// Description of the transaction
        /// </summary>
        public string Description { get; set; } = string.Empty;
        public IEnumerable<ILedgerEntry> LedgerEntries { get; set; }
        public bool IsPosted { get; set; }

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