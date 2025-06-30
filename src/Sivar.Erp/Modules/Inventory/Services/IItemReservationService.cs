using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Documents;
using Sivar.Erp.Modules.Inventory.Models;
using InventoryReservationStatus = Sivar.Erp.Modules.Inventory.ReservationStatus;

namespace Sivar.Erp.Modules.Inventory.Services
{
    /// <summary>
    /// Service for managing item reservations
    /// </summary>
    public interface IItemReservationService
    {
        /// <summary>
        /// Creates a reservation for an item
        /// </summary>
        Task<ItemReservationDto> ReserveItemAsync(
            string itemCode,
            string warehouseCode,
            decimal quantity,
            string documentType,
            string documentNumber,
            int lineNumber,
            string userId,
            TimeSpan? reservationTimeout = null);
        
        /// <summary>
        /// Creates reservations for all items in a document
        /// </summary>
        Task<IEnumerable<ItemReservationDto>> ReserveItemsForDocumentAsync(
            IDocument document,
            string userId,
            TimeSpan? reservationTimeout = null);
        
        /// <summary>
        /// Commits reservations for a document (when document is posted)
        /// </summary>
        Task<bool> CommitReservationsAsync(string documentType, string documentNumber, string userId);
        
        /// <summary>
        /// Releases reservations for a document (e.g., canceled document)
        /// </summary>
        Task<bool> ReleaseReservationsAsync(string documentType, string documentNumber, string userId);
        
        /// <summary>
        /// Gets all reservations for an item
        /// </summary>
        Task<IEnumerable<ItemReservationDto>> GetItemReservationsAsync(
            string itemCode, 
            string warehouseCode = null, 
            InventoryReservationStatus? status = null);
        
        /// <summary>
        /// Gets all reservations for a document
        /// </summary>
        Task<IEnumerable<ItemReservationDto>> GetDocumentReservationsAsync(
            string documentType, 
            string documentNumber);
        
        /// <summary>
        /// Processes expired reservations
        /// </summary>
        Task<int> ProcessExpiredReservationsAsync();
        
        /// <summary>
        /// Extends the expiration time of a reservation
        /// </summary>
        Task<bool> ExtendReservationAsync(string reservationId, TimeSpan extension, string userId);
        
        /// <summary>
        /// Updates the quantity of an existing reservation
        /// </summary>
        Task<ItemReservationDto> UpdateReservationQuantityAsync(
            string reservationId, 
            decimal newQuantity, 
            string userId);
    }
}