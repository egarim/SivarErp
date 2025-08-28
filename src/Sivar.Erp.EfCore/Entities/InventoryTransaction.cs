using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Inventory;
using Sivar.Erp.Modules.Inventory.Core.Interfaces;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Inventory Transactions
/// </summary>
[Table("InventoryTransactions")]
public class InventoryTransaction : BaseEntity, IInventoryTransaction
{
    /// <summary>
    /// Unique transaction identifier
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string Id { get; set; } = string.Empty;

    /// <summary>
    /// Transaction ID
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly transaction number
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to InventoryItem
    /// </summary>
    public virtual Guid ItemId { get; set; }

    /// <summary>
    /// Navigation property to InventoryItem
    /// </summary>
    [ForeignKey(nameof(ItemId))]
    public virtual InventoryItem InventoryItem { get; set; } = null!;

    /// <summary>
    /// Transaction type
    /// </summary>
    public virtual InventoryTransactionType TransactionType { get; set; }

    /// <summary>
    /// Quantity (positive for in, negative for out)
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public virtual decimal Quantity { get; set; }

    /// <summary>
    /// Unit cost for this transaction
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public virtual decimal UnitCost { get; set; }

    /// <summary>
    /// Source warehouse code
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string SourceWarehouseCode { get; set; } = string.Empty;

    /// <summary>
    /// Destination warehouse code (for transfers)
    /// </summary>
    [MaxLength(50)]
    public virtual string? DestinationWarehouseCode { get; set; }

    /// <summary>
    /// Reference document number
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string ReferenceDocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date
    /// </summary>
    public virtual DateOnly TransactionDate { get; set; }

    /// <summary>
    /// User who created the transaction
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Notes or comments
    /// </summary>
    [MaxLength(1000)]
    public virtual string? Notes { get; set; }

    /// <summary>
    /// Total value of this transaction (Quantity * UnitCost)
    /// </summary>
    [NotMapped]
    public decimal TotalValue => Math.Abs(Quantity) * UnitCost;

    // Interface implementation
    IInventoryItem IInventoryTransaction.Item 
    { 
        get => InventoryItem; 
        set => InventoryItem = (InventoryItem)value; 
    }
}
