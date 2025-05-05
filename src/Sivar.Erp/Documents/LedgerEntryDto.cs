namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Implementation of the ledger entry entity
    /// </summary>
    public class LedgerEntryDto : ILedgerEntry
    {
        /// <summary>
        /// Unique identifier for the ledger entry
        /// </summary>
        public Guid Id { get; set; }

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
        /// Optional reference to a person for analysis
        /// </summary>
        public Guid? PersonId { get; set; }

        /// <summary>
        /// Optional reference to a cost center for analysis
        /// </summary>
        public Guid? CostCentreId { get; set; }
    }
}