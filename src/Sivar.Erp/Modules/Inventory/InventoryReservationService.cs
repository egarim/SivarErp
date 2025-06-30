using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Services;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Implementation of the inventory reservation service
    /// </summary>
    public class InventoryReservationService : IInventoryReservationService
    {
        private readonly IObjectDb _objectDb;
        private readonly ILogger<InventoryReservationService> _logger;
        private readonly IInventoryService _inventoryService;
        private readonly IDateTimeZoneService _dateTimeService;
        private readonly PerformanceLogger<InventoryReservationService> _performanceLogger;

        /// <summary>
        /// Initializes a new instance of the InventoryReservationService class
        /// </summary>
        public InventoryReservationService(
            ILogger<InventoryReservationService> logger,
            IInventoryService inventoryService,
            IDateTimeZoneService dateTimeService,
            IObjectDb objectDb = null,
            IPerformanceContextProvider contextProvider = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _objectDb = objectDb;
            _performanceLogger = new PerformanceLogger<InventoryReservationService>(logger, objectDb: objectDb, contextProvider: contextProvider);
        }

        /// <summary>
        /// Creates a reservation for an inventory item
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
            return await _performanceLogger.Track(nameof(CreateReservationAsync), async () =>
            {
                if (_objectDb == null)
                {
                    _logger.LogError("ObjectDb is not initialized");
                    return null;
                }

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

                // Create the reservation
                var reservation = new InventoryReservationDto
                {
                    ReservationId = Guid.NewGuid().ToString(),
                    Item = item,
                    Quantity = quantity,
                    WarehouseCode = warehouseCode,
                    SourceDocumentNumber = sourceDocumentNumber,
                    Status = ReservationStatus.Active,
                    CreatedBy = userName,
                    CreatedAt = _dateTimeService.Now(),
                    ExpiresAt = _dateTimeService.Now().AddMinutes(expiryMinutes),
                    LastUpdated = _dateTimeService.Now(),
                    Notes = notes ?? string.Empty
                };

                _objectDb.InventoryReservations.Add(reservation);

                _logger.LogInformation(
                    "Created reservation {ReservationId} for item {ItemCode} in warehouse {WarehouseCode}, quantity {Quantity}",
                    reservation.ReservationId, item.Code, warehouseCode, quantity);

                return reservation;
            });
        }

        /// <summary>
        /// Updates an existing reservation
        /// </summary>
        public async Task<IInventoryReservation> UpdateReservationAsync(
            string reservationId,
            decimal? newQuantity,
            int? newExpiryMinutes,
            string userName)
        {
            return await _performanceLogger.Track(nameof(UpdateReservationAsync), async () =>
            {
                if (_objectDb == null)
                {
                    _logger.LogError("ObjectDb is not initialized");
                    return null;
                }

                var reservation = await GetReservationAsync(reservationId);
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
                        var additionalQuantity = newQuantity.Value - reservation.Quantity;
                        var isAvailable = await _inventoryService.IsStockAvailableAsync(
                            reservation.WarehouseCode, reservation.Item.Code, additionalQuantity);
                        
                        if (!isAvailable)
                        {
                            _logger.LogWarning(
                                "Insufficient stock to increase reservation {ReservationId} quantity from {OldQuantity} to {NewQuantity}",
                                reservationId, reservation.Quantity, newQuantity.Value);
                            return null;
                        }
                    }

                    reservation.Quantity = newQuantity.Value;
                }

                // Update expiry time if specified
                if (newExpiryMinutes.HasValue)
                {
                    reservation.ExpiresAt = _dateTimeService.Now().AddMinutes(newExpiryMinutes.Value);
                }

                reservation.LastUpdated = _dateTimeService.Now();

                _logger.LogInformation(
                    "Updated reservation {ReservationId} for item {ItemCode}",
                    reservationId, reservation.Item.Code);

                return reservation;
            });
        }

        /// <summary>
        /// Cancels an existing reservation and releases the reserved stock
        /// </summary>
        public async Task<bool> CancelReservationAsync(
            string reservationId,
            string userName,
            string notes = null)
        {
            return await _performanceLogger.Track(nameof(CancelReservationAsync), async () =>
            {
                if (_objectDb == null)
                {
                    _logger.LogError("ObjectDb is not initialized");
                    return false;
                }

                var reservation = await GetReservationAsync(reservationId);
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
                reservation.LastUpdated = _dateTimeService.Now();
                
                if (!string.IsNullOrEmpty(notes))
                {
                    reservation.Notes = string.IsNullOrEmpty(reservation.Notes) 
                        ? notes 
                        : $"{reservation.Notes}\n{notes}";
                }

                _logger.LogInformation(
                    "Cancelled reservation {ReservationId} for item {ItemCode} by user {UserName}",
                    reservationId, reservation.Item.Code, userName);

                return true;
            });
        }

        /// <summary>
        /// Fulfills a reservation by converting it to an inventory transaction
        /// </summary>
        public async Task<IInventoryTransaction> FulfillReservationAsync(
            string reservationId,
            decimal actualQuantity,
            string userName)
        {
            return await _performanceLogger.Track(nameof(FulfillReservationAsync), async () =>
            {
                if (_objectDb == null)
                {
                    _logger.LogError("ObjectDb is not initialized");
                    return null;
                }

                var reservation = await GetReservationAsync(reservationId);
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

                // Record inventory issue
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
                    // Update reservation status
                    reservation.Status = ReservationStatus.Fulfilled;
                    reservation.LastUpdated = _dateTimeService.Now();
                    
                    _logger.LogInformation(
                        "Fulfilled reservation {ReservationId} for item {ItemCode}, quantity {Quantity}",
                        reservationId, reservation.Item.Code, actualQuantity);
                }

                return transaction;
            });
        }

        /// <summary>
        /// Gets a reservation by ID
        /// </summary>
        public Task<IInventoryReservation> GetReservationAsync(string reservationId)
        {
            return Task.FromResult(_performanceLogger.Track("GetReservationAsync", () =>
            {
                if (_objectDb == null || _objectDb.InventoryReservations == null)
                {
                    _logger.LogError("ObjectDb or InventoryReservations collection is not initialized");
                    return (IInventoryReservation)null;
                }

                return _objectDb.InventoryReservations.FirstOrDefault(r => r.ReservationId == reservationId);
            }));
        }

        /// <summary>
        /// Gets all reservations for an item
        /// </summary>
        public Task<IEnumerable<IInventoryReservation>> GetReservationsForItemAsync(
            string itemCode,
            bool includeExpired = false,
            bool includeFullfilled = false)
        {
            return Task.FromResult(_performanceLogger.Track("GetReservationsForItemAsync", () =>
            {
                if (_objectDb == null || _objectDb.InventoryReservations == null)
                {
                    _logger.LogError("ObjectDb or InventoryReservations collection is not initialized");
                    return Enumerable.Empty<IInventoryReservation>();
                }

                var now = _dateTimeService.Now();
                
                var query = _objectDb.InventoryReservations.Where(r => r.Item.Code == itemCode);
                
                if (!includeExpired)
                {
                    query = query.Where(r => r.Status != ReservationStatus.Cancelled && 
                                           (r.Status != ReservationStatus.Active || r.ExpiresAt > now));
                }
                
                if (!includeFullfilled)
                {
                    query = query.Where(r => r.Status != ReservationStatus.Fulfilled);
                }
                
                return query.ToList();
            }));
        }

        /// <summary>
        /// Gets reservations by document number
        /// </summary>
        public Task<IEnumerable<IInventoryReservation>> GetReservationsByDocumentAsync(string documentNumber)
        {
            return Task.FromResult(_performanceLogger.Track("GetReservationsByDocumentAsync", () =>
            {
                if (_objectDb == null || _objectDb.InventoryReservations == null)
                {
                    _logger.LogError("ObjectDb or InventoryReservations collection is not initialized");
                    return Enumerable.Empty<IInventoryReservation>();
                }

                return _objectDb.InventoryReservations
                    .Where(r => r.SourceDocumentNumber == documentNumber)
                    .ToList();
            }));
        }

        /// <summary>
        /// Processes all expired reservations by cancelling them
        /// </summary>
        public async Task<int> ProcessExpiredReservationsAsync(string systemUserName)
        {
            return await _performanceLogger.Track(nameof(ProcessExpiredReservationsAsync), async () =>
            {
                if (_objectDb == null || _objectDb.InventoryReservations == null)
                {
                    _logger.LogError("ObjectDb or InventoryReservations collection is not initialized");
                    return 0;
                }

                int processedCount = 0;
                var now = _dateTimeService.Now();
                
                var expiredReservations = _objectDb.InventoryReservations
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
            });
        }
    }
}