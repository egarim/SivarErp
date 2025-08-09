using DevExpress.ExpressApp;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using Sivar.Erp.Modules.Inventory;
using Sivar.Erp.Xaf.Module.BusinessObjects.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Xaf.Module.Services.Inventory
{
    /// <summary>
    /// XAF-specific implementation of inventory reservation service that works with XAF object spaces
    /// Inherits from the base IInventoryReservationService but adapts it to work with XPO persistence
    /// </summary>
    public class XafInventoryReservationService : IInventoryReservationService
    {
        private readonly ILogger<XafInventoryReservationService> _logger;
        private readonly IObjectSpaceProvider _objectSpaceProvider;
        private readonly XafInventoryService _inventoryService;

        public XafInventoryReservationService(
            ILogger<XafInventoryReservationService> logger,
            IObjectSpaceProvider objectSpaceProvider,
            XafInventoryService inventoryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _objectSpaceProvider = objectSpaceProvider ?? throw new ArgumentNullException(nameof(objectSpaceProvider));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        }

        /// <summary>
        /// Creates a reservation for an inventory item using XAF object space
        /// </summary>
        public async Task<IInventoryReservation> CreateReservationAsync(
            IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            string sourceDocumentNumber,
            int expiryMinutes,
            string userName,
            string notes = null)
        {
            if (item == null)
            {
                _logger.LogError("Cannot reserve null item");
                return null;
            }

            if (quantity <= 0)
            {
                _logger.LogError("Cannot reserve quantity less than or equal to zero: {Quantity}", quantity);
                return null;
            }

            // Check if sufficient stock is available
            var isAvailable = await _inventoryService.IsStockAvailableAsync(item.Code, warehouseCode, quantity);
            if (!isAvailable)
            {
                _logger.LogWarning("Insufficient stock for item {ItemCode} in warehouse {WarehouseCode}", item.Code, warehouseCode);
                return null;
            }

            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

            // Get the inventory item in this object space
            var inventoryItem = objectSpace.GetObjects<InventoryItem>()
                .FirstOrDefault(i => i.Code == item.Code);

            if (inventoryItem == null)
            {
                _logger.LogError("Inventory item with code '{ItemCode}' not found", item.Code);
                return null;
            }

            // Get or create stock level
            var stockLevel = objectSpace.GetObjects<StockLevel>()
                .FirstOrDefault(sl => sl.Item.Code == item.Code && sl.WarehouseCode == warehouseCode);

            if (stockLevel == null)
            {
                _logger.LogError("Stock level not found for item {ItemCode} in warehouse {WarehouseCode}", item.Code, warehouseCode);
                return null;
            }

            // Create the reservation
            var reservation = objectSpace.CreateObject<InventoryReservation>();
            reservation.Item = inventoryItem;
            reservation.StockLevel = stockLevel;
            reservation.Quantity = quantity;
            reservation.WarehouseCode = warehouseCode;
            reservation.SourceDocumentNumber = sourceDocumentNumber;
            reservation.Status = ReservationStatus.Active;
            reservation.ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
            reservation.Notes = notes ?? string.Empty;

            // Update stock level to reflect the reservation
            stockLevel.QuantityReserved += quantity;

            objectSpace.CommitChanges();

            _logger.LogInformation(
                "Created reservation {ReservationId} for item {ItemCode} in warehouse {WarehouseCode}, quantity {Quantity}",
                reservation.ReservationId, item.Code, warehouseCode, quantity);

            return reservation;
        }

        /// <summary>
        /// Updates an existing reservation using XAF object space
        /// </summary>
        public async Task<IInventoryReservation> UpdateReservationAsync(
            string reservationId,
            decimal? newQuantity,
            int? newExpiryMinutes,
            string userName)
        {
            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

            var reservation = objectSpace.GetObjects<InventoryReservation>()
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                _logger.LogWarning("Reservation not found: {ReservationId}", reservationId);
                return null;
            }

            if (reservation.Status != ReservationStatus.Active)
            {
                _logger.LogWarning(
                    "Cannot update reservation {ReservationId} in status {Status}",
                    reservationId, reservation.Status);
                return null;
            }

            // Update quantity if specified
            if (newQuantity.HasValue && newQuantity.Value != reservation.Quantity)
            {
                // Check if increased quantity is available
                if (newQuantity.Value > reservation.Quantity)
                {
                    var additionalQuantity = newQuantity.Value - (reservation.Quantity ?? 0);
                    var isAvailable = await _inventoryService.IsStockAvailableAsync(
                        reservation.Item.Code, reservation.WarehouseCode, additionalQuantity);
                    
                    if (!isAvailable)
                    {
                        _logger.LogWarning(
                            "Insufficient stock to increase reservation {ReservationId} quantity from {OldQuantity} to {NewQuantity}",
                            reservationId, reservation.Quantity, newQuantity.Value);
                        return null;
                    }
                }

                // Update stock level reservation quantity
                var quantityDifference = newQuantity.Value - (reservation.Quantity ?? 0);
                if (reservation.StockLevel != null)
                {
                    reservation.StockLevel.QuantityReserved += quantityDifference;
                }

                reservation.Quantity = newQuantity.Value;
            }

            // Update expiry time if specified
            if (newExpiryMinutes.HasValue)
            {
                reservation.ExpiresAt = DateTime.UtcNow.AddMinutes(newExpiryMinutes.Value);
            }

            objectSpace.CommitChanges();

            _logger.LogInformation(
                "Updated reservation {ReservationId} for item {ItemCode}",
                reservationId, reservation.Item.Code);

            return reservation;
        }

        /// <summary>
        /// Cancels an existing reservation and releases the reserved stock
        /// </summary>
        public async Task<bool> CancelReservationAsync(
            string reservationId,
            string userName,
            string notes = null)
        {
            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

            var reservation = objectSpace.GetObjects<InventoryReservation>()
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                _logger.LogWarning("Reservation not found: {ReservationId}", reservationId);
                return false;
            }

            if (reservation.Status == ReservationStatus.Cancelled || 
                reservation.Status == ReservationStatus.Fulfilled)
            {
                _logger.LogWarning(
                    "Cannot cancel reservation {ReservationId} in status {Status}",
                    reservationId, reservation.Status);
                return false;
            }

            // Update reservation status
            reservation.Status = ReservationStatus.Cancelled;
            
            if (!string.IsNullOrEmpty(notes))
            {
                reservation.Notes = string.IsNullOrEmpty(reservation.Notes) 
                    ? notes 
                    : $"{reservation.Notes}\n{notes}";
            }

            // Release the reserved quantity from stock level
            if (reservation.StockLevel != null && reservation.Quantity.HasValue)
            {
                reservation.StockLevel.QuantityReserved -= reservation.Quantity.Value;
            }

            objectSpace.CommitChanges();

            _logger.LogInformation(
                "Cancelled reservation {ReservationId} for item {ItemCode} by user {UserName}",
                reservationId, reservation.Item.Code, userName);

            return true;
        }

        /// <summary>
        /// Fulfills a reservation by converting it to an inventory transaction
        /// </summary>
        public async Task<IInventoryTransaction> FulfillReservationAsync(
            string reservationId,
            decimal actualQuantity,
            string userName)
        {
            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

            var reservation = objectSpace.GetObjects<InventoryReservation>()
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                _logger.LogWarning("Reservation not found: {ReservationId}", reservationId);
                return null;
            }

            if (reservation.Status != ReservationStatus.Active)
            {
                _logger.LogWarning(
                    "Cannot fulfill reservation {ReservationId} in status {Status}",
                    reservationId, reservation.Status);
                return null;
            }

            if (actualQuantity <= 0 || actualQuantity > reservation.Quantity)
            {
                _logger.LogWarning(
                    "Invalid quantity {Quantity} for reservation {ReservationId} with quantity {ReservationQuantity}",
                    actualQuantity, reservationId, reservation.Quantity);
                return null;
            }

            // Record inventory issue using the inventory service
            var transaction = await _inventoryService.RecordInventoryIssueAsync(
                reservation.Item,
                actualQuantity,
                reservation.WarehouseCode,
                InventoryTransactionType.ReservationFulfillment,
                reservation.SourceDocumentNumber,
                userName,
                $"Fulfillment of reservation {reservationId}");

            if (transaction != null)
            {
                // Get the reservation again in case the inventory service used a different object space
                var reservationToUpdate = objectSpace.GetObjects<InventoryReservation>()
                    .FirstOrDefault(r => r.ReservationId == reservationId);

                if (reservationToUpdate != null)
                {
                    // Update reservation status
                    reservationToUpdate.Status = ReservationStatus.Fulfilled;

                    // Adjust reserved quantity in stock level
                    if (reservationToUpdate.StockLevel != null && reservationToUpdate.Quantity.HasValue)
                    {
                        reservationToUpdate.StockLevel.QuantityReserved -= reservationToUpdate.Quantity.Value;
                    }

                    objectSpace.CommitChanges();
                }
                
                _logger.LogInformation(
                    "Fulfilled reservation {ReservationId} for item {ItemCode}, quantity {Quantity}",
                    reservationId, reservation.Item.Code, actualQuantity);
            }

            return transaction;
        }

        /// <summary>
        /// Gets a reservation by ID using XAF object space
        /// </summary>
        public Task<IInventoryReservation> GetReservationAsync(string reservationId)
        {
            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

            var reservation = objectSpace.GetObjects<InventoryReservation>()
                .FirstOrDefault(r => r.ReservationId == reservationId);

            return Task.FromResult<IInventoryReservation>(reservation);
        }

        /// <summary>
        /// Gets all reservations for an item using XAF object space
        /// </summary>
        public Task<IEnumerable<IInventoryReservation>> GetReservationsForItemAsync(
            string itemCode,
            bool includeExpired = false,
            bool includeFullfilled = false)
        {
            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

            var now = DateTime.UtcNow;
                
            var query = objectSpace.GetObjects<InventoryReservation>()
                .Where(r => r.Item.Code == itemCode);
                
            if (!includeExpired)
            {
                query = query.Where(r => r.Status != ReservationStatus.Cancelled && 
                                       (r.Status != ReservationStatus.Active || r.ExpiresAt > now));
            }
                
            if (!includeFullfilled)
            {
                query = query.Where(r => r.Status != ReservationStatus.Fulfilled);
            }

            var reservations = query.ToList();
            return Task.FromResult<IEnumerable<IInventoryReservation>>(reservations.Cast<IInventoryReservation>());
        }

        /// <summary>
        /// Gets reservations by document number using XAF object space
        /// </summary>
        public Task<IEnumerable<IInventoryReservation>> GetReservationsByDocumentAsync(string documentNumber)
        {
            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

            var reservations = objectSpace.GetObjects<InventoryReservation>()
                .Where(r => r.SourceDocumentNumber == documentNumber)
                .ToList();

            return Task.FromResult<IEnumerable<IInventoryReservation>>(reservations.Cast<IInventoryReservation>());
        }

        /// <summary>
        /// Processes all expired reservations by cancelling them
        /// </summary>
        public async Task<int> ProcessExpiredReservationsAsync(string systemUserName)
        {
            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

            int processedCount = 0;
            var now = DateTime.UtcNow;
                
            var expiredReservations = objectSpace.GetObjects<InventoryReservation>()
                .Where(r => r.Status == ReservationStatus.Active && r.ExpiresAt < now)
                .ToList();
                
            foreach (var reservation in expiredReservations)
            {
                var cancelled = await CancelReservationAsync(
                    reservation.ReservationId, 
                    systemUserName, 
                    "Automatically cancelled due to expiration");
                    
                if (cancelled)
                {
                    processedCount++;
                }
            }
                
            if (processedCount > 0)
            {
                _logger.LogInformation("Processed {Count} expired reservations", processedCount);
            }
                
            return processedCount;
        }
    }
}