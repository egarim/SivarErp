using System;
using Sivar.Erp.Modules.Inventory;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Interface for inventory items with stock-related properties
    /// Extends the basic IItem with inventory-specific properties
    /// </summary>
    public interface IInventoryItem : IItem
    {
        /// <summary>
        /// Gets or sets whether the item is tracked in inventory
        /// </summary>
        bool IsInventoryTracked { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure for the item
        /// </summary>
        string UnitOfMeasure { get; set; }

        /// <summary>
        /// Gets or sets the minimum stock level before reordering
        /// </summary>
        decimal ReorderPoint { get; set; }

        /// <summary>
        /// Gets or sets the target stock level when reordering
        /// </summary>
        decimal ReorderQuantity { get; set; }

        /// <summary>
        /// Gets or sets the current average cost of the item
        /// </summary>
        decimal AverageCost { get; set; }

        /// <summary>
        /// Gets or sets the warehouse location
        /// </summary>
        string Location { get; set; }
        
        /// <summary>
        /// Gets or sets the inventory valuation method (AverageCost, FIFO, LIFO)
        /// </summary>
        InventoryValuationMethod ValuationMethod { get; set; }
    }
}