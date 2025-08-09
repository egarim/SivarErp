using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Represents an entry in the accounting ledger
    /// </summary>
    [Description("Represents an entry in the accounting ledger")]
    public interface ILedgerEntry
    {
        /// <summary>
        /// Gets or sets the unique identifier for this entry
        /// </summary>
        string LedgerEntryNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction number associated with this entry
        /// </summary>
        string TransactionNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the entry type (debit or credit)
        /// </summary>
        EntryType EntryType { get; set; }
        
        /// <summary>
        /// Gets or sets the account code
        /// </summary>
        string OfficialCode { get; set; }
        
        /// <summary>
        /// Gets or sets the amount for this entry
        /// </summary>
        decimal Amount { get; set; }
        
        /// <summary>
        /// Gets or sets the description for this entry
        /// </summary>
        string? Description { get; set; }
    }
    
    /// <summary>
    /// Types of ledger entries
    /// </summary>
    public enum EntryType
    {
        /// <summary>
        /// Debit entry
        /// </summary>
        Debit,
        
        /// <summary>
        /// Credit entry
        /// </summary>
        Credit
    }
}