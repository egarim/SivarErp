using System;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory.Models
{
    /// <summary>
    /// Represents a reservation of inventory item quantity for a specific document
    /// </summary>
    public class ItemReservationDto : IEntity, IInventoryReservation
    {
        public Guid Oid { get; set; }
        public string Id { get; set; }
        
        /// <summary>
        /// Item code being reserved
        /// </summary>
        public string ItemCode { get; set; }
        
        /// <summary>
        /// Warehouse where the item is reserved
        /// </summary>
        public string WarehouseCode { get; set; }
        
        /// <summary>
        /// Quantity of the item reserved
        /// </summary>
        public decimal Quantity { get; set; }
        
        /// <summary>
        /// Type of document making the reservation
        /// </summary>
        public string DocumentType { get; set; }
        
        /// <summary>
        /// Document number making the reservation
        /// </summary>
        public string DocumentNumber { get; set; }
        
        /// <summary>
        /// Line number in the document
        /// </summary>
        public int LineNumber { get; set; }
        
        /// <summary>
        /// When the reservation was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Who created the reservation
        /// </summary>
        public string CreatedBy { get; set; }
        
        /// <summary>
        /// When the reservation was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// When the reservation expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Status of the reservation using Models namespace enum
        /// </summary>
        public ReservationStatus Status { get; set; }
        
        /// <summary>
        /// Unique lock token for the reservation
        /// </summary>
        public string ReservationToken { get; set; }
        
        // IInventoryReservation implementation
        public string ReservationId 
        {
            get => Id;
            set => Id = value;
        }
        
        public IInventoryItem Item { get; set; }
        
        public string SourceDocumentNumber 
        {
            get => DocumentNumber;
            set => DocumentNumber = value;
        }
        
        // Map to the Inventory namespace ReservationStatus for interface compatibility
        Inventory.ReservationStatus IInventoryReservation.Status 
        {
            get => Status switch
            {
                ReservationStatus.Active => Inventory.ReservationStatus.Active,
                ReservationStatus.Committed => Inventory.ReservationStatus.Fulfilled,
                ReservationStatus.Released => Inventory.ReservationStatus.Cancelled,
                ReservationStatus.Expired => Inventory.ReservationStatus.Expired,
                _ => Inventory.ReservationStatus.Active
            };
            set => Status = value switch
            {
                Inventory.ReservationStatus.Active => ReservationStatus.Active,
                Inventory.ReservationStatus.Fulfilled => ReservationStatus.Committed,
                Inventory.ReservationStatus.Cancelled => ReservationStatus.Released,
                Inventory.ReservationStatus.Expired => ReservationStatus.Expired,
                _ => ReservationStatus.Active
            };
        }
        
        public DateTime LastUpdated 
        {
            get => UpdatedAt;
            set => UpdatedAt = value;
        }
        
        public string Notes { get; set; } = "";
        
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }
    
    public enum ReservationStatus
    {
        /// <summary>
        /// Reservation is active and quantity is allocated
        /// </summary>
        Active,
        
        /// <summary>
        /// Reservation has been committed (e.g., document posted)
        /// </summary>
        Committed,
        
        /// <summary>
        /// Reservation has been released
        /// </summary>
        Released,
        
        /// <summary>
        /// Reservation has expired without being committed
        /// </summary>
        Expired
    }
}