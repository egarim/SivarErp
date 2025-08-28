using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Modules.Inventory;
using Sivar.Erp.Modules.Inventory.Core.Interfaces;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Inventory Reservations
/// </summary>
[Table("InventoryReservations")]
[Index(nameof(ReservationId), IsUnique = true, Name = "IX_InventoryReservations_ReservationId")]
[Index(nameof(ItemCode), nameof(WarehouseCode), Name = "IX_InventoryReservations_Item_Warehouse")]
[Index(nameof(SourceDocumentNumber), Name = "IX_InventoryReservations_SourceDocument")]
public class InventoryReservation : BaseEntity, IInventoryReservation
{
    /// <summary>
    /// Unique reservation identifier
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string ReservationId { get; set; } = string.Empty;

    /// <summary>
    /// Code of the inventory item being reserved
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Quantity being reserved
    /// </summary>
    [Column(TypeName = "decimal(18,6)")]
    public virtual decimal Quantity { get; set; }

    /// <summary>
    /// Warehouse code where the item is reserved
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string WarehouseCode { get; set; } = string.Empty;

    /// <summary>
    /// Source document number (e.g., sales order number)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string SourceDocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Reservation status
    /// </summary>
    public virtual ReservationStatus Status { get; set; }

    /// <summary>
    /// Date/time when the reservation was created
    /// </summary>
    [Required]
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created the reservation
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Date/time when the reservation expires
    /// </summary>
    [Required]
    public virtual DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Date/time when the reservation was last updated
    /// </summary>
    [Required]
    public virtual DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Priority level of the reservation
    /// </summary>
    public virtual int Priority { get; set; } = 1;

    /// <summary>
    /// Optional notes about the reservation
    /// </summary>
    [Required]
    [MaxLength(500)]
    public virtual string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets whether the reservation is expired based on current time
    /// </summary>
    [NotMapped]
    public bool IsExpired => ExpiresAt < DateTime.UtcNow;

    // Navigation property for the inventory item
    [ForeignKey(nameof(ItemCode))]
    public virtual InventoryItem? Item { get; set; }

    // Interface implementation - map to InventoryItem entity
    [NotMapped]
    IInventoryItem IInventoryReservation.Item 
    { 
        get => Item!; 
        set => Item = (InventoryItem)value; 
    }
}