using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Accounting.Models
{
    /// <summary>
    /// Implementation of the ITransaction interface
    /// </summary>
    public class TransactionDto : Entity, ITransaction
    {
        /// <summary>
        /// Gets or sets the transaction number
        /// </summary>
        public string TransactionNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the transaction date
        /// </summary>
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets or sets the document number associated with this transaction
        /// </summary>
        public string? DocumentNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction description
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Gets or sets whether the transaction has been posted
        /// </summary>
        public bool IsPosted { get; set; }
        
        /// <summary>
        /// Gets or sets when the transaction was posted
        /// </summary>
        public DateTime? PostedDate { get; set; }
        
        /// <summary>
        /// Gets or sets the user who posted the transaction
        /// </summary>
        public string? PostedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of ledger entries for this transaction
        /// </summary>
        public ICollection<ILedgerEntry> LedgerEntries { get; set; } = new List<ILedgerEntry>();
    }
}