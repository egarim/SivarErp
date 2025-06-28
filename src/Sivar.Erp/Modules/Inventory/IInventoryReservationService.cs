using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Interface for the inventory reservation service that manages inventory reservations
    /// </summary>
    public interface IInventoryReservationService
    {
        /// <summary>
        /// Creates a reservation for an inventory item
        /// </summary>
        /// <param name="item">Item to reserve</param>
        /// <param name="quantity">Quantity to reserve</param>
        /// <param name="warehouseCode">Warehouse code where the item is reserved</param>
        /// <param name="sourceDocumentNumber">Source document number (e.g. sales order)</param>
        /// <param name="expiryMinutes">Minutes until the reservation expires</param>
        /// <param name="userName">User creating the reservation</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The created reservation if successful, null if insufficient stock</returns>
        Task<IInventoryReservation> CreateReservationAsync(
            IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            string sourceDocumentNumber,
            int expiryMinutes,
            string userName,
            string notes = null);
            
        /// <summary>
        /// Updates an existing reservation
        /// </summary>
        /// <param name="reservationId">Reservation ID to update</param>
        /// <param name="newQuantity">New quantity (if changing)</param>
        /// <param name="newExpiryMinutes">New expiry minutes from now (if changing)</param>
        /// <param name="userName">User updating the reservation</param>
        /// <returns>The updated reservation</returns>
        Task<IInventoryReservation> UpdateReservationAsync(
            string reservationId,
            decimal? newQuantity,
            int? newExpiryMinutes,
            string userName);
            
        /// <summary>
        /// Cancels an existing reservation and releases the reserved stock
        /// </summary>
        /// <param name="reservationId">Reservation ID to cancel</param>
        /// <param name="userName">User cancelling the reservation</param>
        /// <param name="notes">Optional notes for why it was cancelled</param>
        /// <returns>True if successfully cancelled</returns>
        Task<bool> CancelReservationAsync(
            string reservationId,
            string userName,
            string notes = null);
            
        /// <summary>
        /// Fulfills a reservation by converting it to an inventory transaction
        /// </summary>
        /// <param name="reservationId">Reservation ID to fulfill</param>
        /// <param name="actualQuantity">Actual quantity fulfilled (may be less than reserved)</param>
        /// <param name="userName">User fulfilling the reservation</param>
        /// <returns>The created inventory transaction</returns>
        Task<IInventoryTransaction> FulfillReservationAsync(
            string reservationId,
            decimal actualQuantity,
            string userName);
            
        /// <summary>
        /// Gets a reservation by ID
        /// </summary>
        /// <param name="reservationId">Reservation ID to retrieve</param>
        /// <returns>The reservation if found, null otherwise</returns>
        Task<IInventoryReservation> GetReservationAsync(string reservationId);
        
        /// <summary>
        /// Gets all reservations for an item
        /// </summary>
        /// <param name="itemCode">Item code to filter by</param>
        /// <param name="includeExpired">Whether to include expired reservations</param>
        /// <param name="includeFullfilled">Whether to include fulfilled reservations</param>
        /// <returns>List of reservations for the item</returns>
        Task<IEnumerable<IInventoryReservation>> GetReservationsForItemAsync(
            string itemCode, 
            bool includeExpired = false,
            bool includeFullfilled = false);
            
        /// <summary>
        /// Gets reservations by document number
        /// </summary>
        /// <param name="documentNumber">Document number to filter by</param>
        /// <returns>List of reservations for the document</returns>
        Task<IEnumerable<IInventoryReservation>> GetReservationsByDocumentAsync(string documentNumber);
        
        /// <summary>
        /// Processes all expired reservations by cancelling them
        /// </summary>
        /// <param name="systemUserName">System user name to use for the cancellation</param>
        /// <returns>Number of reservations processed</returns>
        Task<int> ProcessExpiredReservationsAsync(string systemUserName);
    }
}