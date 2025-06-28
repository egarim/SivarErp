using System;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Interface for inventory item reservations
    /// </summary>
    public interface IInventoryReservation
    {
        /// <summary>
        /// Gets or sets the reservation ID
        /// </summary>
        string ReservationId { get; set; }
        
        /// <summary>
        /// Gets or sets the inventory item being reserved
        /// </summary>
        IInventoryItem Item { get; set; }
        
        /// <summary>
        /// Gets or sets the quantity being reserved
        /// </summary>
        decimal Quantity { get; set; }
        
        /// <summary>
        /// Gets or sets the warehouse code where the item is reserved
        /// </summary>
        string WarehouseCode { get; set; }
        
        /// <summary>
        /// Gets or sets the source document number (e.g., sales order number)
        /// </summary>
        string SourceDocumentNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the reservation status
        /// </summary>
        ReservationStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the user who created the reservation
        /// </summary>
        string CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets when the reservation was created
        /// </summary>
        DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets when the reservation expires
        /// </summary>
        DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Gets or sets when the reservation was last updated
        /// </summary>
        DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Gets or sets notes or comments
        /// </summary>
        string Notes { get; set; }
        
        /// <summary>
        /// Gets whether the reservation is expired based on current time
        /// </summary>
        bool IsExpired { get; }
    }
}