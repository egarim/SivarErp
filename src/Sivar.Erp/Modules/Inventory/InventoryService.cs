using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.Services;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Service for managing inventory operations
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly ILogger<InventoryService> _logger;
        private readonly IObjectDb _objectDb;
        private readonly ISequencerService _sequencerService;
        private readonly PerformanceLogger<InventoryService> _performanceLogger;
        
        private const string INVENTORY_TRANSACTION_SEQUENCE_CODE = "INVTXN";

        public InventoryService(
            ILogger<InventoryService> logger,
            IObjectDb objectDb,
            ISequencerService sequencerService,
            IPerformanceContextProvider? contextProvider = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _objectDb = objectDb ?? throw new ArgumentNullException(nameof(objectDb));
            _sequencerService = sequencerService ?? throw new ArgumentNullException(nameof(sequencerService));
            
            _performanceLogger = new PerformanceLogger<InventoryService>(
                _logger, 
                PerformanceLogMode.All, 
                100, 
                10_000_000, 
                _objectDb, 
                contextProvider);
                
            // Initialize collections if they don't exist
            _objectDb.InventoryItems ??= new List<Documents.IInventoryItem>();
            _objectDb.StockLevels ??= new List<IStockLevel>();
            _objectDb.InventoryTransactions ??= new List<IInventoryTransaction>();
            _objectDb.InventoryReservations ??= new List<IInventoryReservation>();
            _objectDb.InventoryLayers ??= new List<InventoryLayerDto>();
        }

        /// <summary>
        /// Gets an inventory item by code
        /// </summary>
        public Task<Documents.IInventoryItem> GetInventoryItemAsync(string itemCode)
        {
            return Task.FromResult(_performanceLogger.Track("GetInventoryItemAsync", () =>
            {
                if (string.IsNullOrWhiteSpace(itemCode))
                    throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

                return _objectDb.InventoryItems.FirstOrDefault(i => i.Code == itemCode);
            }));
        }

        /// <summary>
        /// Creates a new inventory item
        /// </summary>
        public async Task<Documents.IInventoryItem> CreateInventoryItemAsync(Documents.IInventoryItem item, string userName)
        {
            return await _performanceLogger.Track(nameof(CreateInventoryItemAsync), async () =>
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                if (string.IsNullOrWhiteSpace(userName))
                    throw new ArgumentException("User name cannot be null or empty", nameof(userName));

                // Check if item already exists
                var existingItem = await GetInventoryItemAsync(item.Code);
                if (existingItem != null)
                    throw new InvalidOperationException($"Inventory item with code '{item.Code}' already exists");

                // Note: The IInventoryItem interface doesn't have audit fields like CreatedBy, CreatedAt, LastUpdated
                // These would typically be handled by the concrete implementation or a wrapper DTO
                // For now, we just add the item to the collection as-is

                // Add to collection
                _objectDb.InventoryItems.Add(item);

                _logger.LogInformation("Created new inventory item {ItemCode} by user {UserName}", 
                    item.Code, userName);

                return item;
            });
        }

        /// <summary>
        /// Gets the stock level for an item
        /// </summary>
        public Task<IStockLevel> GetStockLevelAsync(string itemCode, string warehouseCode = null)
        {
            return Task.FromResult(_performanceLogger.Track("GetStockLevelAsync", () =>
            {
                if (string.IsNullOrWhiteSpace(itemCode))
                    throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

                var query = _objectDb.StockLevels.Where(sl => 
                    sl.Item != null && sl.Item.Code == itemCode);

                if (!string.IsNullOrWhiteSpace(warehouseCode))
                    query = query.Where(sl => sl.WarehouseCode == warehouseCode);

                return query.FirstOrDefault();
            }));
        }

        /// <summary>
        /// Gets all stock levels for an item across all warehouses
        /// </summary>
        public Task<IEnumerable<IStockLevel>> GetStockLevelsAsync(string itemCode)
        {
            return Task.FromResult(_performanceLogger.Track("GetStockLevelsAsync", () =>
            {
                if (string.IsNullOrWhiteSpace(itemCode))
                    throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

                return _objectDb.StockLevels
                    .Where(sl => sl.Item != null && sl.Item.Code == itemCode)
                    .ToList().AsEnumerable();
            }));
        }
        
        /// <summary>
        /// Gets stock levels for an item filtered by warehouse
        /// </summary>
        public Task<IEnumerable<IStockLevel>> GetStockLevelsAsync(string itemCode, string warehouseCode)
        {
            return Task.FromResult(_performanceLogger.Track("GetStockLevelsAsync", () =>
            {
                if (string.IsNullOrWhiteSpace(itemCode))
                    throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

                var query = _objectDb.StockLevels
                    .Where(sl => sl.Item != null && sl.Item.Code == itemCode);

                if (!string.IsNullOrWhiteSpace(warehouseCode))
                    query = query.Where(sl => sl.WarehouseCode == warehouseCode);

                return query.ToList().AsEnumerable();
            }));
        }

        /// <summary>
        /// Processes a receipt of inventory
        /// </summary>
        public async Task<IInventoryTransaction> ReceiveInventoryAsync(
            Documents.IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            decimal unitCost,
            string userName,
            string notes = null)
        {
            return await _performanceLogger.Track(nameof(ReceiveInventoryAsync), async () =>
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                if (quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

                if (string.IsNullOrWhiteSpace(warehouseCode))
                    throw new ArgumentException("Warehouse code cannot be null or empty", nameof(warehouseCode));

                if (unitCost < 0)
                    throw new ArgumentException("Unit cost cannot be negative", nameof(unitCost));

                // Create inventory transaction
                var transaction = new InventoryTransactionDto
                {
                    Oid = Guid.NewGuid(),
                    Id = Guid.NewGuid().ToString(),
                    TransactionId = await _sequencerService.GetNextNumberAsync(INVENTORY_TRANSACTION_SEQUENCE_CODE),
                    TransactionNumber = await _sequencerService.GetNextNumberAsync(INVENTORY_TRANSACTION_SEQUENCE_CODE),
                    Item = item,
                    Quantity = quantity,
                    TransactionType = transactionType,
                    SourceWarehouseCode = warehouseCode,
                    DestinationWarehouseCode = warehouseCode,
                    ReferenceDocumentNumber = referenceDocument,
                    UnitCost = unitCost,
                    TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    CreatedBy = userName,
                    CreatedAt = DateTime.UtcNow,
                    Notes = notes
                };

                // Add to collection
                _objectDb.InventoryTransactions.Add(transaction);

                // Update stock level
                await UpdateStockLevelAsync(item, warehouseCode, quantity, userName);

                // Update inventory item costs using weighted average
                await UpdateInventoryItemCostAsync(item, quantity, unitCost, userName);

                // Create inventory layer for FIFO/LIFO costing
                await CreateInventoryLayerAsync(item.Code, warehouseCode, quantity, unitCost, transaction.TransactionId, userName);

                _logger.LogInformation(
                    "Received {Quantity} units of item {ItemCode} at cost {UnitCost} in warehouse {WarehouseCode}",
                    quantity, item.Code, unitCost, warehouseCode);

                return transaction;
            });
        }

        /// <summary>
        /// Processes an issue of inventory
        /// </summary>
        public async Task<IInventoryTransaction> IssueInventoryAsync(
            Documents.IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            string userName,
            string notes = null)
        {
            return await _performanceLogger.Track(nameof(IssueInventoryAsync), async () =>
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                if (quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

                if (string.IsNullOrWhiteSpace(warehouseCode))
                    throw new ArgumentException("Warehouse code cannot be null or empty", nameof(warehouseCode));

                // Check available stock
                var stockLevel = await GetStockLevelAsync(item.Code, warehouseCode);
                if (stockLevel == null || stockLevel.AvailableQuantity < quantity)
                {
                    var available = stockLevel?.AvailableQuantity ?? 0;
                    throw new InvalidOperationException(
                        $"Insufficient stock for item {item.Code} in warehouse {warehouseCode}. " +
                        $"Available: {available}, Requested: {quantity}");
                }

                // Calculate weighted average cost for the issue
                var averageCost = await CalculateAverageCostAsync(item.Code, warehouseCode);

                // Create inventory transaction
                var transaction = new InventoryTransactionDto
                {
                    Oid = Guid.NewGuid(),
                    Id = Guid.NewGuid().ToString(),
                    TransactionId = await _sequencerService.GetNextNumberAsync(INVENTORY_TRANSACTION_SEQUENCE_CODE),
                    TransactionNumber = await _sequencerService.GetNextNumberAsync(INVENTORY_TRANSACTION_SEQUENCE_CODE),
                    Item = item,
                    Quantity = -quantity, // Negative for issues
                    TransactionType = transactionType,
                    SourceWarehouseCode = warehouseCode,
                    DestinationWarehouseCode = warehouseCode,
                    ReferenceDocumentNumber = referenceDocument,
                    UnitCost = averageCost,
                    TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    CreatedBy = userName,
                    CreatedAt = DateTime.UtcNow,
                    Notes = notes
                };

                // Add to collection
                _objectDb.InventoryTransactions.Add(transaction);

                // Update stock level (negative quantity for issue)
                await UpdateStockLevelAsync(item, warehouseCode, -quantity, userName);

                // Consume inventory layers for FIFO/LIFO costing
                await ConsumeInventoryLayersAsync(item.Code, warehouseCode, quantity, transaction.TransactionId, userName);

                _logger.LogInformation(
                    "Issued {Quantity} units of item {ItemCode} at cost {UnitCost} from warehouse {WarehouseCode}",
                    quantity, item.Code, averageCost, warehouseCode);

                return transaction;
            });
        }

        /// <summary>
        /// Gets inventory transactions for an item
        /// </summary>
        public Task<IEnumerable<IInventoryTransaction>> GetInventoryTransactionsAsync(
            string itemCode,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            string warehouseCode = null)
        {
            return Task.FromResult(_performanceLogger.Track("GetInventoryTransactionsAsync", () =>
            {
                if (string.IsNullOrWhiteSpace(itemCode))
                    throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

                var query = _objectDb.InventoryTransactions.Where(t => t.Item.Code == itemCode);

                if (fromDate.HasValue)
                    query = query.Where(t => t.TransactionDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(t => t.TransactionDate <= toDate.Value);

                if (!string.IsNullOrWhiteSpace(warehouseCode))
                    query = query.Where(t => t.SourceWarehouseCode == warehouseCode || t.DestinationWarehouseCode == warehouseCode);

                return query.OrderBy(t => t.TransactionDate).ToList().AsEnumerable();
            }));
        }

        /// <summary>
        /// Gets available quantity considering reservations
        /// </summary>
        public Task<decimal> GetAvailableQuantityAsync(string itemCode, string warehouseCode)
        {
            return Task.FromResult(_performanceLogger.Track("GetAvailableQuantityAsync", () =>
            {
                if (string.IsNullOrWhiteSpace(itemCode))
                    throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

                if (string.IsNullOrWhiteSpace(warehouseCode))
                    throw new ArgumentException("Warehouse code cannot be null or empty", nameof(warehouseCode));

                var stockLevel = _objectDb.StockLevels.FirstOrDefault(sl => 
                    sl.Item != null && sl.Item.Code == itemCode && sl.WarehouseCode == warehouseCode);

                return stockLevel?.AvailableQuantity ?? 0;
            }));
        }
        
        /// <summary>
        /// Checks if sufficient stock is available for a given quantity
        /// </summary>
        public async Task<bool> IsStockAvailableAsync(string itemCode, string warehouseCode, decimal quantity)
        {
            var availableQuantity = await GetAvailableQuantityAsync(itemCode, warehouseCode);
            return availableQuantity >= quantity;
        }
        
        /// <summary>
        /// Records an inventory receipt (alias for ReceiveInventoryAsync for compatibility)
        /// </summary>
        public Task<IInventoryTransaction> RecordInventoryReceiptAsync(
            Documents.IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            decimal unitCost,
            string userName,
            string notes = null)
        {
            return ReceiveInventoryAsync(item, quantity, warehouseCode, transactionType, 
                referenceDocument, unitCost, userName, notes);
        }
        
        /// <summary>
        /// Records an inventory issue (alias for IssueInventoryAsync for compatibility)
        /// </summary>
        public Task<IInventoryTransaction> RecordInventoryIssueAsync(
            Documents.IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            string userName,
            string notes = null)
        {
            return IssueInventoryAsync(item, quantity, warehouseCode, transactionType, 
                referenceDocument, userName, notes);
        }

        #region Private Helper Methods

        /// <summary>
        /// Updates stock level for an item in a warehouse
        /// </summary>
        private async Task UpdateStockLevelAsync(Documents.IInventoryItem item, string warehouseCode, decimal quantityChange, string userName)
        {
            var stockLevel = await GetStockLevelAsync(item.Code, warehouseCode);

            if (stockLevel == null)
            {
                // Create new stock level record using existing DTO
                stockLevel = new StockLevelDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Item = item,
                    WarehouseCode = warehouseCode,
                    QuantityOnHand = quantityChange,
                    QuantityReserved = 0,
                    LastUpdated = DateTime.UtcNow
                };

                _objectDb.StockLevels.Add(stockLevel);
            }
            else if (stockLevel is StockLevelDto stockLevelDto)
            {
                // Update existing stock level
                stockLevelDto.QuantityOnHand += quantityChange;
                stockLevelDto.LastUpdated = DateTime.UtcNow;
            }

            _logger.LogDebug(
                "Updated stock level for item {ItemCode} in warehouse {WarehouseCode}: " +
                "Change={QuantityChange}, New OnHand={OnHand}, Available={Available}",
                item.Code, warehouseCode, quantityChange, stockLevel.QuantityOnHand, stockLevel.AvailableQuantity);
        }

        /// <summary>
        /// Updates inventory item cost using weighted average method
        /// </summary>
        private async Task UpdateInventoryItemCostAsync(Documents.IInventoryItem item, decimal quantity, decimal unitCost, string userName)
        {
            // Get current total value
            var totalOnHand = 0m;
            var totalValue = 0m;

            // Sum up quantities and values across all warehouses
            var stockLevels = await GetStockLevelsAsync(item.Code);
            foreach (var stockLevel in stockLevels)
            {
                totalOnHand += stockLevel.QuantityOnHand;
                // Use AverageCost property instead of StandardCost
                totalValue += stockLevel.QuantityOnHand * item.AverageCost;
            }

            // Add the new receipt
            totalOnHand += quantity;
            totalValue += quantity * unitCost;

            // Calculate new weighted average cost and update the AverageCost property
            if (totalOnHand > 0)
            {
                item.AverageCost = totalValue / totalOnHand;
                // Note: IInventoryItem doesn't have LastUpdated property
                // This would typically be handled by the concrete implementation
            }

            _logger.LogDebug(
                "Updated weighted average cost for item {ItemCode}: New cost={Cost}, Total quantity={Quantity}",
                item.Code, item.AverageCost, totalOnHand);
        }

        /// <summary>
        /// Creates an inventory layer for FIFO/LIFO costing
        /// </summary>
        private async Task CreateInventoryLayerAsync(
            string itemCode, 
            string warehouseCode, 
            decimal quantity, 
            decimal unitCost, 
            string transactionId, 
            string userName)
        {
            var layer = new InventoryLayerDto
            {
                Oid = Guid.NewGuid(),
                ItemCode = itemCode,
                WarehouseCode = warehouseCode,
                Quantity = quantity,
                RemainingQuantity = quantity,
                UnitCost = unitCost,
                TransactionId = transactionId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userName
            };

            _objectDb.InventoryLayers.Add(layer);

            _logger.LogDebug(
                "Created inventory layer for item {ItemCode} in warehouse {WarehouseCode}: " +
                "Quantity={Quantity}, Cost={Cost}, Transaction={TransactionId}",
                itemCode, warehouseCode, quantity, unitCost, transactionId);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Consumes inventory layers for FIFO/LIFO costing
        /// </summary>
        private async Task ConsumeInventoryLayersAsync(
            string itemCode, 
            string warehouseCode, 
            decimal quantityToConsume, 
            string transactionId, 
            string userName)
        {
            // Get available layers ordered by creation date (FIFO)
            var availableLayers = _objectDb.InventoryLayers
                .Where(l => l.ItemCode == itemCode && 
                           l.WarehouseCode == warehouseCode && 
                           l.RemainingQuantity > 0)
                .OrderBy(l => l.CreatedDate)
                .ToList();

            var remainingToConsume = quantityToConsume;

            foreach (var layer in availableLayers)
            {
                if (remainingToConsume <= 0)
                    break;

                var consumeFromThisLayer = Math.Min(remainingToConsume, layer.RemainingQuantity);
                
                layer.RemainingQuantity -= consumeFromThisLayer;
                remainingToConsume -= consumeFromThisLayer;

                _logger.LogDebug(
                    "Consumed {ConsumedQuantity} from layer {LayerId} for item {ItemCode}. " +
                    "Layer remaining: {RemainingQuantity}",
                    consumeFromThisLayer, layer.Oid, itemCode, layer.RemainingQuantity);
            }

            if (remainingToConsume > 0)
            {
                _logger.LogWarning(
                    "Could not fully consume requested quantity for item {ItemCode}. " +
                    "Requested: {RequestedQuantity}, Remaining: {RemainingQuantity}",
                    itemCode, quantityToConsume, remainingToConsume);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Calculates the weighted average cost for an item in a warehouse
        /// </summary>
        private async Task<decimal> CalculateAverageCostAsync(string itemCode, string warehouseCode)
        {
            var layers = _objectDb.InventoryLayers
                .Where(l => l.ItemCode == itemCode && 
                           l.WarehouseCode == warehouseCode && 
                           l.RemainingQuantity > 0)
                .ToList();

            if (!layers.Any())
            {
                // Fall back to item average cost
                var item = await GetInventoryItemAsync(itemCode);
                return item?.AverageCost ?? 0;
            }

            var totalQuantity = layers.Sum(l => l.RemainingQuantity);
            var totalValue = layers.Sum(l => l.RemainingQuantity * l.UnitCost);

            return totalQuantity > 0 ? totalValue / totalQuantity : 0;
        }

        #endregion
    }

    #region Supporting DTOs

    /// <summary>
    /// Represents an inventory layer for FIFO/LIFO costing
    /// </summary>
    public class InventoryLayerDto
    {
        public Guid Oid { get; set; }
        public string ItemCode { get; set; }
        public string WarehouseCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public string TransactionId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }

    #endregion
}