using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Domain
{
    /// <summary>
    /// Interface for items
    /// </summary>
    [Description("Interface for items")]
    public interface IItem : IEntity
    {
        /// <summary>
        /// Gets or sets the item code
        /// </summary>
        [Description("Item code")]
        string ItemCode { get; set; }

        /// <summary>
        /// Gets or sets the item name
        /// </summary>
        [Description("Item name")]
        string ItemName { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [Description("Description")]
        string? Description { get; set; }

        /// <summary>
        /// Gets or sets the item type
        /// </summary>
        [Description("Item type")]
        string ItemType { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure
        /// </summary>
        [Description("Unit of measure")]
        string UnitOfMeasure { get; set; }

        /// <summary>
        /// Gets or sets the unit price
        /// </summary>
        [Description("Unit price")]
        decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the cost
        /// </summary>
        [Description("Cost")]
        decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets whether the item is active
        /// </summary>
        [Description("Whether the item is active")]
        bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets whether inventory is tracked
        /// </summary>
        [Description("Whether inventory is tracked")]
        bool TrackInventory { get; set; }

        /// <summary>
        /// Gets or sets the minimum stock level
        /// </summary>
        [Description("Minimum stock level")]
        decimal MinimumStockLevel { get; set; }

        /// <summary>
        /// Gets or sets the maximum stock level
        /// </summary>
        [Description("Maximum stock level")]
        decimal MaximumStockLevel { get; set; }

        /// <summary>
        /// Gets or sets the reorder level
        /// </summary>
        [Description("Reorder level")]
        decimal ReorderLevel { get; set; }

        /// <summary>
        /// Gets or sets the tax category
        /// </summary>
        [Description("Tax category")]
        string? TaxCategory { get; set; }
    }
}
