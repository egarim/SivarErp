using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Represents a financial transaction
    /// </summary>
    [Description("Represents a financial transaction")]
    public interface ITransaction
    {
        /// <summary>
        /// Gets or sets the transaction number
        /// </summary>
        string TransactionNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction date
        /// </summary>
        DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// Gets or sets the document number associated with this transaction
        /// </summary>
        string? DocumentNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction description
        /// </summary>
        string? Description { get; set; }
        
        /// <summary>
        /// Gets or sets whether the transaction has been posted
        /// </summary>
        bool IsPosted { get; set; }
        
        /// <summary>
        /// Gets or sets when the transaction was posted
        /// </summary>
        DateTime? PostedDate { get; set; }
        
        /// <summary>
        /// Gets or sets the user who posted the transaction
        /// </summary>
        string? PostedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of ledger entries for this transaction
        /// </summary>
        ICollection<ILedgerEntry> LedgerEntries { get; set; }
    }
}