using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using Sivar.Erp.Modules.Inventory.Models;
using Sivar.Erp.Services;
using ModelsReservationStatus = Sivar.Erp.Modules.Inventory.Models.ReservationStatus;
using InventoryReservationStatus = Sivar.Erp.Modules.Inventory.ReservationStatus;

namespace Sivar.Erp.Modules.Inventory.Services
{
    public class ItemReservationService : IItemReservationService
    {
        private readonly IObjectDb _objectDb;
        private readonly ILogger<ItemReservationService> _logger;
        private readonly IInventoryService _inventoryService;
        private readonly TimeSpan _defaultReservationTimeout = TimeSpan.FromHours(24);
        
        // Simple in-memory lock dictionary for concurrency control
        private static readonly Dictionary<string, SemaphoreSlim> _locks = new();
        private static readonly object _lockCreationLock = new();

        public ItemReservationService(
            IObjectDb objectDb,
            IInventoryService inventoryService, 
            ILogger<ItemReservationService> logger)
        {
            _objectDb = objectDb ?? throw new ArgumentNullException(nameof(objectDb));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Initialize collections if they don't exist - use InventoryReservations, not ItemReservations
            _objectDb.InventoryReservations ??= new List<IInventoryReservation>();
        }
        
        public async Task<ItemReservationDto> ReserveItemAsync(
            string itemCode,
            string warehouseCode,
            decimal quantity,
            string documentType,
            string documentNumber,
            int lineNumber,
            string userId,
            TimeSpan? reservationTimeout = null)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));
            
            if (string.IsNullOrWhiteSpace(warehouseCode))
                throw new ArgumentException("Warehouse code cannot be null or empty", nameof(warehouseCode));
            
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
            
            // Get available quantity
            var availableQty = await _inventoryService.GetAvailableQuantityAsync(itemCode, warehouseCode);
            
            if (availableQty < quantity)
                throw new InvalidOperationException($"Insufficient quantity for item {itemCode}: available {availableQty}, requested {quantity}");
            
            // Create reservation with optimistic concurrency control
            using (var lockObj = await GetReservationLockAsync(itemCode, warehouseCode))
            {
                try
                {
                    // Check again inside lock to ensure atomicity
                    availableQty = await _inventoryService.GetAvailableQuantityAsync(itemCode, warehouseCode);
                    
                    if (availableQty < quantity)
                        throw new InvalidOperationException($"Insufficient quantity for item {itemCode}: available {availableQty}, requested {quantity}");
                    
                    // Create reservation
                    var reservation = new ItemReservationDto
                    {
                        Oid = Guid.NewGuid(),
                        Id = Guid.NewGuid().ToString(),
                        ItemCode = itemCode,
                        WarehouseCode = warehouseCode,
                        Quantity = quantity,
                        DocumentType = documentType,
                        DocumentNumber = documentNumber,
                        LineNumber = lineNumber,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.Add(reservationTimeout ?? _defaultReservationTimeout),
                        Status = ModelsReservationStatus.Active, // Use alias to be explicit
                        ReservationToken = Guid.NewGuid().ToString()
                    };
                    
                    // Add to collection - use InventoryReservations
                    _objectDb.InventoryReservations.Add(reservation as IInventoryReservation);
                    
                    // Adjust inventory item's reserved quantity
                    var stockLevel = await _inventoryService.GetStockLevelAsync(itemCode, warehouseCode);
                    if (stockLevel != null)
                    {
                        stockLevel.QuantityReserved += quantity; // Use correct property name
                        stockLevel.LastUpdated = DateTime.UtcNow;
                    }
                    
                    _logger.LogInformation(
                        "Reserved {Quantity} units of item {ItemCode} in warehouse {WarehouseCode} " +
                        "for document {DocumentType} {DocumentNumber}, line {LineNumber}",
                        quantity, 
                        itemCode, 
                        warehouseCode, 
                        documentType, 
                        documentNumber, 
                        lineNumber);
                    
                    return reservation;
                }
                finally
                {
                    lockObj.Release();
                }
            }
        }
        
        public async Task<IEnumerable<ItemReservationDto>> ReserveItemsForDocumentAsync(
            IDocument document,
            string userId,
            TimeSpan? reservationTimeout = null)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            
            var reservations = new List<ItemReservationDto>();
            
            // Process each line in the document
            foreach (var line in document.Lines)
            {
                // Skip lines without items
                if (line.Item == null)
                    continue;
                
                try
                {
                    // Default warehouse - in a real app get from user context or document
                    string warehouseCode = "MAIN";
                    
                    var reservation = await ReserveItemAsync(
                        line.Item.Code,
                        warehouseCode,
                        line.Quantity,
                        document.DocumentType.Code,
                        document.DocumentNumber,
                        (int)line.LineNumber,
                        userId,
                        reservationTimeout);
                    
                    reservations.Add(reservation);
                }
                catch (Exception ex)
                {
                    // Release any successful reservations
                    if (reservations.Any())
                    {
                        await ReleaseReservationsAsync(
                            document.DocumentType.Code,
                            document.DocumentNumber,
                            userId);
                    }
                    
                    throw new InvalidOperationException($"Failed to reserve item {line.Item.Code}: {ex.Message}", ex);
                }
            }
            
            return reservations;
        }
        
        public async Task<bool> CommitReservationsAsync(string documentType, string documentNumber, string userId)
        {
            if (string.IsNullOrWhiteSpace(documentType))
                throw new ArgumentException("Document type cannot be null or empty", nameof(documentType));
            
            if (string.IsNullOrWhiteSpace(documentNumber))
                throw new ArgumentException("Document number cannot be null or empty", nameof(documentNumber));
            
            // Find reservations for this document - use InventoryReservations and cast
            var reservations = _objectDb.InventoryReservations
                .OfType<ItemReservationDto>()
                .Where(r => r.DocumentType == documentType && 
                            r.DocumentNumber == documentNumber && 
                            r.Status == ModelsReservationStatus.Active)
                .ToList();
            
            if (!reservations.Any())
            {
                _logger.LogWarning(
                    "No active reservations found for document {DocumentType} {DocumentNumber}",
                    documentType,
                    documentNumber);
                
                return true; // Nothing to commit
            }
            
            // Process each reservation
            foreach (var reservation in reservations)
            {
                // Lock the item to ensure atomicity
                using (var lockObj = await GetReservationLockAsync(reservation.ItemCode, reservation.WarehouseCode))
                {
                    try
                    {
                        // Mark as committed
                        reservation.Status = ModelsReservationStatus.Committed;
                        reservation.UpdatedAt = DateTime.UtcNow;
                        
                        // Get stock level
                        var stockLevel = await _inventoryService.GetStockLevelAsync(
                            reservation.ItemCode, 
                            reservation.WarehouseCode);
                        
                        if (stockLevel != null)
                        {
                            // Decrease reserved quantity (already deducted from physical quantity when stock movement posted)
                            stockLevel.QuantityReserved -= reservation.Quantity; // Use correct property name
                            stockLevel.LastUpdated = DateTime.UtcNow;
                        }
                    }
                    finally
                    {
                        lockObj.Release();
                    }
                }
                
                _logger.LogInformation(
                    "Committed reservation {ReservationId} for item {ItemCode}, document {DocumentType} {DocumentNumber}",
                    reservation.Id,
                    reservation.ItemCode,
                    documentType,
                    documentNumber);
            }
            
            return true;
        }
        
        public async Task<bool> ReleaseReservationsAsync(string documentType, string documentNumber, string userId)
        {
            if (string.IsNullOrWhiteSpace(documentType))
                throw new ArgumentException("Document type cannot be null or empty", nameof(documentType));
            
            if (string.IsNullOrWhiteSpace(documentNumber))
                throw new ArgumentException("Document number cannot be null or empty", nameof(documentNumber));
            
            // Find active reservations for this document - use InventoryReservations and cast
            var reservations = _objectDb.InventoryReservations
                .OfType<ItemReservationDto>()
                .Where(r => r.DocumentType == documentType && 
                            r.DocumentNumber == documentNumber && 
                            r.Status == ModelsReservationStatus.Active)
                .ToList();
            
            if (!reservations.Any())
            {
                _logger.LogWarning(
                    "No active reservations found for document {DocumentType} {DocumentNumber}",
                    documentType,
                    documentNumber);
                
                return true; // Nothing to release
            }
            
            // Process each reservation
            foreach (var reservation in reservations)
            {
                // Lock the item to ensure atomicity
                using (var lockObj = await GetReservationLockAsync(reservation.ItemCode, reservation.WarehouseCode))
                {
                    try
                    {
                        // Mark as released
                        reservation.Status = ModelsReservationStatus.Released;
                        reservation.UpdatedAt = DateTime.UtcNow;
                        
                        // Get stock level
                        var stockLevel = await _inventoryService.GetStockLevelAsync(
                            reservation.ItemCode, 
                            reservation.WarehouseCode);
                        
                        if (stockLevel != null)
                        {
                            // Decrease reserved quantity
                            stockLevel.QuantityReserved -= reservation.Quantity; // Use correct property name
                            stockLevel.LastUpdated = DateTime.UtcNow;
                        }
                    }
                    finally
                    {
                        lockObj.Release();
                    }
                }
                
                _logger.LogInformation(
                    "Released reservation {ReservationId} for item {ItemCode}, document {DocumentType} {DocumentNumber}",
                    reservation.Id,
                    reservation.ItemCode,
                    documentType,
                    documentNumber);
            }
            
            return true;
        }
        
        public async Task<IEnumerable<ItemReservationDto>> GetItemReservationsAsync(
            string itemCode, 
            string warehouseCode = null, 
            InventoryReservationStatus? status = null)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));
            
            var query = _objectDb.InventoryReservations.OfType<ItemReservationDto>().AsQueryable();
            
            // Apply filters
            query = query.Where(r => r.ItemCode == itemCode);
            
            if (!string.IsNullOrWhiteSpace(warehouseCode))
                query = query.Where(r => r.WarehouseCode == warehouseCode);
            
            if (status.HasValue)
            {
                // Convert from InventoryReservationStatus to ModelsReservationStatus for comparison
                var modelsStatus = ConvertToModelsReservationStatus(status.Value);
                query = query.Where(r => r.Status == modelsStatus);
            }
            
            return await Task.FromResult(query.ToList());
        }
        
        public async Task<IEnumerable<ItemReservationDto>> GetDocumentReservationsAsync(
            string documentType, 
            string documentNumber)
        {
            if (string.IsNullOrWhiteSpace(documentType))
                throw new ArgumentException("Document type cannot be null or empty", nameof(documentType));
            
            if (string.IsNullOrWhiteSpace(documentNumber))
                throw new ArgumentException("Document number cannot be null or empty", nameof(documentNumber));
            
            var reservations = _objectDb.InventoryReservations
                .OfType<ItemReservationDto>()
                .Where(r => r.DocumentType == documentType && 
                            r.DocumentNumber == documentNumber)
                .ToList();
            
            return await Task.FromResult(reservations);
        }
        
        public async Task<int> ProcessExpiredReservationsAsync()
        {
            var now = DateTime.UtcNow;
            var expiredReservations = _objectDb.InventoryReservations
                .OfType<ItemReservationDto>()
                .Where(r => r.Status == ModelsReservationStatus.Active && r.ExpiresAt < now)
                .ToList();
            
            if (!expiredReservations.Any())
                return 0;
            
            int processedCount = 0;
            
            // Process each expired reservation
            foreach (var reservation in expiredReservations)
            {
                // Lock the item to ensure atomicity
                using (var lockObj = await GetReservationLockAsync(reservation.ItemCode, reservation.WarehouseCode))
                {
                    try
                    {
                        // Mark as expired
                        reservation.Status = ModelsReservationStatus.Expired;
                        reservation.UpdatedAt = now;
                        
                        // Get stock level
                        var stockLevel = await _inventoryService.GetStockLevelAsync(
                            reservation.ItemCode, 
                            reservation.WarehouseCode);
                        
                        if (stockLevel != null)
                        {
                            // Decrease reserved quantity
                            stockLevel.QuantityReserved -= reservation.Quantity; // Use correct property name
                            stockLevel.LastUpdated = now;
                        }
                        
                        processedCount++;
                        
                        _logger.LogInformation(
                            "Expired reservation {ReservationId} for item {ItemCode}, document {DocumentType} {DocumentNumber}",
                            reservation.Id,
                            reservation.ItemCode,
                            reservation.DocumentType,
                            reservation.DocumentNumber);
                    }
                    finally
                    {
                        lockObj.Release();
                    }
                }
            }
            
            return processedCount;
        }
        
        public async Task<bool> ExtendReservationAsync(string reservationId, TimeSpan extension, string userId)
        {
            if (string.IsNullOrWhiteSpace(reservationId))
                throw new ArgumentException("Reservation ID cannot be null or empty", nameof(reservationId));
            
            var reservation = _objectDb.InventoryReservations
                .OfType<ItemReservationDto>()
                .FirstOrDefault(r => r.Id == reservationId);
            
            if (reservation == null)
                throw new InvalidOperationException($"Reservation {reservationId} not found");
            
            if (reservation.Status != ModelsReservationStatus.Active)
                throw new InvalidOperationException($"Reservation {reservationId} is not active");
            
            // Update expiration
            reservation.ExpiresAt = DateTime.UtcNow.Add(extension);
            reservation.UpdatedAt = DateTime.UtcNow;
            
            _logger.LogInformation(
                "Extended reservation {ReservationId} until {ExpiryDate}",
                reservationId,
                reservation.ExpiresAt);
            
            return await Task.FromResult(true);
        }
        
        public async Task<ItemReservationDto> UpdateReservationQuantityAsync(
            string reservationId, 
            decimal newQuantity, 
            string userId)
        {
            if (string.IsNullOrWhiteSpace(reservationId))
                throw new ArgumentException("Reservation ID cannot be null or empty", nameof(reservationId));
            
            if (newQuantity <= 0)
                throw new ArgumentException("New quantity must be greater than zero", nameof(newQuantity));
            
            var reservation = _objectDb.InventoryReservations
                .OfType<ItemReservationDto>()
                .FirstOrDefault(r => r.Id == reservationId);
            
            if (reservation == null)
                throw new InvalidOperationException($"Reservation {reservationId} not found");
            
            if (reservation.Status != ModelsReservationStatus.Active)
                throw new InvalidOperationException($"Reservation {reservationId} is not active");
            
            // Lock the item to ensure atomicity
            using (var lockObj = await GetReservationLockAsync(reservation.ItemCode, reservation.WarehouseCode))
            {
                try
                {
                    decimal quantityDifference = newQuantity - reservation.Quantity;
                    
                    // If increasing quantity, check available
                    if (quantityDifference > 0)
                    {
                        var stockLevel = await _inventoryService.GetStockLevelAsync(
                            reservation.ItemCode, 
                            reservation.WarehouseCode);
                        
                        var availableQty = stockLevel.QuantityOnHand - stockLevel.QuantityReserved; // Use correct property name
                        
                        if (availableQty < quantityDifference)
                            throw new InvalidOperationException(
                                $"Insufficient quantity to increase reservation. " +
                                $"Available: {availableQty}, Additional needed: {quantityDifference}");
                        
                        // Update reserved quantity
                        stockLevel.QuantityReserved += quantityDifference; // Use correct property name
                        stockLevel.LastUpdated = DateTime.UtcNow;
                    }
                    else if (quantityDifference < 0)
                    {
                        // Decreasing quantity - release the difference
                        var stockLevel = await _inventoryService.GetStockLevelAsync(
                            reservation.ItemCode, 
                            reservation.WarehouseCode);
                        
                        if (stockLevel != null)
                        {
                            stockLevel.QuantityReserved += quantityDifference; // Adding negative = subtraction, use correct property name
                            stockLevel.LastUpdated = DateTime.UtcNow;
                        }
                    }
                    
                    // Update reservation
                    reservation.Quantity = newQuantity;
                    reservation.UpdatedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation(
                        "Updated reservation {ReservationId} quantity from {OldQuantity} to {NewQuantity}",
                        reservationId,
                        reservation.Quantity - quantityDifference,
                        newQuantity);
                    
                    return reservation;
                }
                finally
                {
                    lockObj.Release();
                }
            }
        }
        
        #region Private Helper Methods
        
        /// <summary>
        /// Gets a semaphore for locking operations on a specific item-warehouse combination
        /// </summary>
        private async Task<SemaphoreSlim> GetReservationLockAsync(string itemCode, string warehouseCode)
        {
            var lockKey = $"{itemCode}_{warehouseCode}";
            
            SemaphoreSlim semaphore;
            
            lock (_lockCreationLock)
            {
                if (!_locks.TryGetValue(lockKey, out semaphore))
                {
                    semaphore = new SemaphoreSlim(1, 1);
                    _locks[lockKey] = semaphore;
                }
            }
            
            await semaphore.WaitAsync();
            return semaphore;
        }
        
        /// <summary>
        /// Converts from Inventory.ReservationStatus to Models.ReservationStatus
        /// </summary>
        private ModelsReservationStatus ConvertToModelsReservationStatus(InventoryReservationStatus status)
        {
            return status switch
            {
                InventoryReservationStatus.Active => ModelsReservationStatus.Active,
                InventoryReservationStatus.Fulfilled => ModelsReservationStatus.Committed,
                InventoryReservationStatus.Cancelled => ModelsReservationStatus.Released,
                InventoryReservationStatus.Expired => ModelsReservationStatus.Expired,
                _ => ModelsReservationStatus.Active
            };
        }
        
        #endregion
    }
}