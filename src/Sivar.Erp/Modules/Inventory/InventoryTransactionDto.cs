using System;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Implementation of IInventoryTransaction
    /// </summary>
    public class InventoryTransactionDto : IInventoryTransaction, IEntity
    {
        public Guid Oid { get; set; }
        public string Id { get; set; }
        public string TransactionId { get; set; }
        public string TransactionNumber { get; set; }
        public Documents.IInventoryItem Item { get; set; }
        public InventoryTransactionType TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string SourceWarehouseCode { get; set; }
        public string DestinationWarehouseCode { get; set; }
        public string ReferenceDocumentNumber { get; set; }
        public DateOnly TransactionDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Notes { get; set; }
        
        /// <summary>
        /// Gets the total value of this transaction (Quantity * UnitCost)
        /// </summary>
        public decimal TotalValue => Quantity * UnitCost;
    }
}