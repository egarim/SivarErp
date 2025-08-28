using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Modules.Inventory;
using Sivar.Erp.Modules.Inventory.Core.Interfaces;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Stock Levels
/// </summary>
[Table("StockLevels")]
[Index(nameof(ItemCode), nameof(WarehouseCode), IsUnique = true, Name = "IX_StockLevels_Item_Warehouse")]
[Index(nameof(WarehouseCode), Name = "IX_StockLevels_Warehouse")]
public class StockLevel : BaseEntity, IStockLevel
{
    /// <summary>
    /// Interface implementation - map BaseEntity.Oid to IStockLevel.Id
    /// </summary>
    [NotMapped]
    string IStockLevel.Id 
    { 
        get => Oid.ToString(); 
        set => Oid = Guid.TryParse(value, out var guid) ? guid : Guid.NewGuid(); 
    }
    /// <summary>
    /// Item code for which this stock level is tracked
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Warehouse or location code
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string WarehouseCode { get; set; } = string.Empty;

    /// <summary>
    /// Physical quantity on hand
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal QuantityOnHand { get; set; }

    /// <summary>
    /// Quantity reserved for sales orders
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal QuantityReserved { get; set; }

    /// <summary>
    /// Quantity on order (incoming)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal QuantityOnOrder { get; set; }

    /// <summary>
    /// Date/time when the stock level was last updated
    /// </summary>
    [Required]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the available quantity (on hand minus reserved)
    /// </summary>
    [NotMapped]
    public decimal AvailableQuantity => QuantityOnHand - QuantityReserved;

    // Navigation property for the inventory item
    [ForeignKey(nameof(ItemCode))]
    public virtual InventoryItem? Item { get; set; }

    // Interface implementation - map to InventoryItem entity
    [NotMapped]
    IInventoryItem IStockLevel.Item 
    { 
        get => Item!; 
        set => Item = (InventoryItem)value; 
    }
}