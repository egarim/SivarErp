using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Sivar.Erp.Modules.Documents.Core.Enums;
using Sivar.Erp.Modules.Inventory.Core.Enums;
using Sivar.Erp.Modules.Inventory.Core.Interfaces;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Inventory Items
/// </summary>
[Table("InventoryItems")]
public class InventoryItem : BaseEntity, IInventoryItem
{
    /// <summary>
    /// Unique code for the item
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; set; } = string.Empty;

    /// <summary>
    /// Type of item (Product, Service, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Type { get; set; } = string.Empty;

    /// <summary>
    /// Description of the item
    /// </summary>
    [Required]
    [MaxLength(500)]
    public virtual string Description { get; set; } = string.Empty;

    /// <summary>
    /// Base price of the item
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public virtual decimal BasePrice { get; set; }

    /// <summary>
    /// Whether the item is tracked in inventory
    /// </summary>
    public virtual bool IsInventoryTracked { get; set; } = true;

    /// <summary>
    /// Unit of measure for the item
    /// </summary>
    [Required]
    [MaxLength(20)]
    public virtual string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>
    /// Minimum stock level before reordering
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public virtual decimal ReorderPoint { get; set; }

    /// <summary>
    /// Target stock level when reordering
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public virtual decimal ReorderQuantity { get; set; }

    /// <summary>
    /// Current average cost of the item
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public virtual decimal AverageCost { get; set; }

    /// <summary>
    /// Warehouse location
    /// </summary>
    [MaxLength(100)]
    public virtual string Location { get; set; } = string.Empty;

    /// <summary>
    /// Inventory valuation method (WeightedAverage, FIFO, LIFO)
    /// </summary>
    public virtual InventoryValuationMethod ValuationMethod { get; set; } = InventoryValuationMethod.WeightedAverage;
}
