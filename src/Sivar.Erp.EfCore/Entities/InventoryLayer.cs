using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Inventory;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Inventory Layers (for FIFO/LIFO costing)
/// </summary>
[Table("InventoryLayers")]
public class InventoryLayer : BaseEntity, IInventoryLayerDto
{
    /// <summary>
    /// Item code reference
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Warehouse code where the layer exists
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string WarehouseCode { get; set; } = string.Empty;

    /// <summary>
    /// Original quantity in this layer
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Remaining quantity in this layer
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal RemainingQuantity { get; set; }

    /// <summary>
    /// Unit cost for this layer
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Transaction ID that created this layer
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Date when the layer was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created the layer
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the inventory item
    /// </summary>
    [ForeignKey(nameof(ItemCode))]
    public virtual InventoryItem? InventoryItem { get; set; }
}
