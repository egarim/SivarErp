using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Accounting.Models
{
    /// <summary>
    /// Implementation of the ILedgerEntry interface
    /// </summary>
    public class LedgerEntryDto : Entity, ILedgerEntry
    {
        /// <summary>
        /// Gets or sets the unique identifier for this entry
        /// </summary>
        public string LedgerEntryNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the transaction number associated with this entry
        /// </summary>
        public string TransactionNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the entry type (debit or credit)
        /// </summary>
        public EntryType EntryType { get; set; }
        
        /// <summary>
        /// Gets or sets the account code
        /// </summary>
        public string OfficialCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the amount for this entry
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Gets or sets the description for this entry
        /// </summary>
        public string? Description { get; set; }
    }
}