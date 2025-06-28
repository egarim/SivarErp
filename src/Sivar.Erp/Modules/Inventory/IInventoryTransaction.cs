using System;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Interface for inventory transaction records (stock movements)
    /// </summary>
    public interface IInventoryTransaction
    {
        /// <summary>
        /// Gets or sets the transaction ID
        /// </summary>
        string TransactionId { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction number (user-friendly identifier)
        /// </summary>
        string TransactionNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the inventory item
        /// </summary>
        IInventoryItem Item { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction type
        /// </summary>
        InventoryTransactionType TransactionType { get; set; }
        
        /// <summary>
        /// Gets or sets the quantity (positive for in, negative for out)
        /// </summary>
        decimal Quantity { get; set; }
        
        /// <summary>
        /// Gets or sets the unit cost for this transaction
        /// </summary>
        decimal UnitCost { get; set; }
        
        /// <summary>
        /// Gets or sets the source warehouse code
        /// </summary>
        string SourceWarehouseCode { get; set; }
        
        /// <summary>
        /// Gets or sets the destination warehouse code (for transfers)
        /// </summary>
        string DestinationWarehouseCode { get; set; }
        
        /// <summary>
        /// Gets or sets the reference document number
        /// </summary>
        string ReferenceDocumentNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction date
        /// </summary>
        DateOnly TransactionDate { get; set; }
        
        /// <summary>
        /// Gets or sets the user who created the transaction
        /// </summary>
        string CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets notes or comments
        /// </summary>
        string Notes { get; set; }
        
        /// <summary>
        /// Gets the total value of this transaction (Quantity * UnitCost)
        /// </summary>
        decimal TotalValue { get; }
    }
}