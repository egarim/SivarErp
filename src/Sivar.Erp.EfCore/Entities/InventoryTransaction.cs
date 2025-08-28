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
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Transaction ID
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly transaction number
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to InventoryItem
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Navigation property to InventoryItem
    /// </summary>
    [ForeignKey(nameof(ItemId))]
    public virtual InventoryItem InventoryItem { get; set; } = null!;

    /// <summary>
    /// Transaction type
    /// </summary>
    public InventoryTransactionType TransactionType { get; set; }

    /// <summary>
    /// Quantity (positive for in, negative for out)
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit cost for this transaction
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Source warehouse code
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SourceWarehouseCode { get; set; } = string.Empty;

    /// <summary>
    /// Destination warehouse code (for transfers)
    /// </summary>
    [MaxLength(50)]
    public string? DestinationWarehouseCode { get; set; }

    /// <summary>
    /// Reference document number
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ReferenceDocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date
    /// </summary>
    public DateOnly TransactionDate { get; set; }

    /// <summary>
    /// User who created the transaction
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Notes or comments
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

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
