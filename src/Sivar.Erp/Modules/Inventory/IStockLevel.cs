using System;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Interface for tracking current stock levels of inventory items
    /// </summary>
    public interface IStockLevel
    {
        /// <summary>
        /// Gets or sets the ID of the stock level entry
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the inventory item
        /// </summary>
        IInventoryItem Item { get; set; }

        /// <summary>
        /// Gets or sets the warehouse or location code
        /// </summary>
        string WarehouseCode { get; set; }

        /// <summary>
        /// Gets or sets the physical quantity on hand
        /// </summary>
        decimal QuantityOnHand { get; set; }

        /// <summary>
        /// Gets or sets the quantity reserved for sales orders
        /// </summary>
        decimal QuantityReserved { get; set; }

        /// <summary>
        /// Gets or sets the quantity on order (incoming)
        /// </summary>
        decimal QuantityOnOrder { get; set; }

        /// <summary>
        /// Gets or sets the stock level last updated date
        /// </summary>
        DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets the available quantity (on hand minus reserved)
        /// </summary>
        decimal AvailableQuantity { get; }
    }
}