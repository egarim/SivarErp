namespace Sivar.Erp.Services.Accounting.Transactions
{
    /// <summary>
    /// Implementation of the ledger entry entity
    /// </summary>
    public class LedgerEntryDto : ILedgerEntry
    {
        /// <summary>
        /// Unique identifier for the ledger entry
        /// </summary>
        public Guid Oid { get; set; }

        /// <summary>
        /// Reference to the parent transaction
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Reference to the account
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Type of entry (debit or credit)
        /// </summary>
        public EntryType EntryType { get; set; }

        /// <summary>
        /// Amount of the entry
        /// </summary>
        public decimal Amount { get; set; }

     

        /// <summary>
        /// Name of the account
        /// </summary>
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Official code/identifier for the account
        /// </summary>
        public string OfficialCode { get; set; } = string.Empty;
    }
}