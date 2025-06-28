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
        
        private const string INVENTORY_TRANSACTION_SEQUENCE_CODE = "INV_TRANS";

        /// <summary>
        /// Initializes a new instance of the InventoryService class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="objectDb">Object database</param>
        /// <param name="sequencerService">Sequencer service</param>
        /// <param name="contextProvider">Performance context provider</param>
        public InventoryService(
            ILogger<InventoryService> logger,
            IObjectDb objectDb,
            ISequencerService sequencerService,
            IPerformanceContextProvider contextProvider = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _objectDb = objectDb ?? throw new ArgumentNullException(nameof(objectDb));
            _sequencerService = sequencerService ?? throw new ArgumentNullException(nameof(sequencerService));
            _performanceLogger = new PerformanceLogger<InventoryService>(logger, PerformanceLogMode.All, 100, 10_000_000, objectDb, contextProvider);
            
            // Initialize collections if they don't exist
            _objectDb.InventoryItems ??= new List<IInventoryItem>();
            _objectDb.StockLevels ??= new List<IStockLevel>();
            _objectDb.InventoryTransactions ??= new List<IInventoryTransaction>();
        }

        /// <summary>
        /// Gets an inventory item by code
        /// </summary>
        public async Task<IInventoryItem> GetInventoryItemAsync(string itemCode)
        {
            return await _performanceLogger.Track(nameof(GetInventoryItemAsync), async () =>
            {
                if (string.IsNullOrEmpty(itemCode))
                    throw new ArgumentNullException(nameof(itemCode));

                // First try to find in inventory items
                var inventoryItem = _objectDb.InventoryItems.FirstOrDefault(
                    i => string.Equals(i.Code, itemCode, StringComparison.OrdinalIgnoreCase));

                if (inventoryItem != null)
                    return inventoryItem;

                // If not found, try to find in regular items and convert if found
                var regularItem = _objectDb.Items.FirstOrDefault(
                    i => string.Equals(i.Code, itemCode, StringComparison.OrdinalIgnoreCase));

                if (regularItem == null)
                    return null;

                // Create a new inventory item based on the regular item
                return new InventoryItemDto
                {
                    Code = regularItem.Code,
                    Type = regularItem.Type,
                    Description = regularItem.Description,
                    BasePrice = regularItem.BasePrice,
                    IsInventoryTracked = false, // Default to not tracked
                    UnitOfMeasure = "EA", // Default unit
                    ReorderPoint = 0,
                    ReorderQuantity = 0,
                    AverageCost = 0,
                    Location = ""
                };
            });
        }

        /// <summary>
        /// Creates a new inventory item
        /// </summary>
        public async Task<IInventoryItem> CreateInventoryItemAsync(IInventoryItem item, string userName)
        {
            return await _performanceLogger.Track(nameof(CreateInventoryItemAsync), async () =>
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));
                
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                // Check if item with same code already exists
                var existingItem = await GetInventoryItemAsync(item.Code);
                if (existingItem != null)
                    throw new InvalidOperationException($"Item with code {item.Code} already exists");

                // Add to inventory items collection
                _objectDb.InventoryItems.Add(item);

                _logger.LogInformation("Inventory item {ItemCode} created by {UserName}", item.Code, userName);
                return item;
            });
        }

        /// <summary>
        /// Updates an existing inventory item
        /// </summary>
        public async Task<IInventoryItem> UpdateInventoryItemAsync(IInventoryItem item, string userName)
        {
            return await _performanceLogger.Track(nameof(UpdateInventoryItemAsync), async () =>
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));
                
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                // Find existing item
                var existingIndex = _objectDb.InventoryItems.ToList().FindIndex(
                    i => string.Equals(i.Code, item.Code, StringComparison.OrdinalIgnoreCase));

                if (existingIndex == -1)
                    throw new InvalidOperationException($"Item with code {item.Code} not found");

                // Replace existing item with updated one
                _objectDb.InventoryItems[existingIndex] = item;
                
                _logger.LogInformation("Inventory item {ItemCode} updated by {UserName}", item.Code, userName);
                return item;
            });
        }

        /// <summary>
        /// Gets current stock levels for an item
        /// </summary>
        public async Task<IEnumerable<IStockLevel>> GetStockLevelsAsync(string itemCode, string warehouseCode = null)
        {
            return await _performanceLogger.Track(nameof(GetStockLevelsAsync), async () =>
            {
                if (string.IsNullOrEmpty(itemCode))
                    throw new ArgumentNullException(nameof(itemCode));

                // Get all stock levels for the item
                var stockLevels = _objectDb.StockLevels.Where(
                    sl => string.Equals(sl.Item.Code, itemCode, StringComparison.OrdinalIgnoreCase));

                // Filter by warehouse if provided
                if (!string.IsNullOrEmpty(warehouseCode))
                {
                    stockLevels = stockLevels.Where(
                        sl => string.Equals(sl.WarehouseCode, warehouseCode, StringComparison.OrdinalIgnoreCase));
                }

                return stockLevels.ToList();
            });
        }

        /// <summary>
        /// Gets current stock level for a specific item in a specific warehouse
        /// </summary>
        public async Task<IStockLevel> GetStockLevelAsync(string itemCode, string warehouseCode)
        {
            return await _performanceLogger.Track(nameof(GetStockLevelAsync), async () =>
            {
                if (string.IsNullOrEmpty(itemCode))
                    throw new ArgumentNullException(nameof(itemCode));
                
                if (string.IsNullOrEmpty(warehouseCode))
                    throw new ArgumentNullException(nameof(warehouseCode));

                // Find stock level for the item in the specified warehouse
                var stockLevel = _objectDb.StockLevels.FirstOrDefault(
                    sl => string.Equals(sl.Item.Code, itemCode, StringComparison.OrdinalIgnoreCase) && 
                          string.Equals(sl.WarehouseCode, warehouseCode, StringComparison.OrdinalIgnoreCase));

                if (stockLevel == null)
                {
                    // Get the inventory item
                    var item = await GetInventoryItemAsync(itemCode);
                    if (item == null)
                        throw new InvalidOperationException($"Item with code {itemCode} not found");

                    // Create a new stock level record
                    stockLevel = new StockLevelDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Item = item,
                        WarehouseCode = warehouseCode,
                        QuantityOnHand = 0,
                        QuantityReserved = 0,
                        QuantityOnOrder = 0,
                        LastUpdated = DateTime.Now
                    };

                    _objectDb.StockLevels.Add(stockLevel);
                }

                return stockLevel;
            });
        }

        /// <summary>
        /// Records an inventory receipt (stock in)
        /// </summary>
        public async Task<IInventoryTransaction> RecordInventoryReceiptAsync(
            IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            decimal unitCost,
            string userName,
            string notes = null)
        {
            return await _performanceLogger.Track(nameof(RecordInventoryReceiptAsync), async () =>
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));
                
                if (quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
                
                if (string.IsNullOrEmpty(warehouseCode))
                    throw new ArgumentNullException(nameof(warehouseCode));
                
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                // Get or create stock level
                var stockLevel = await GetStockLevelAsync(item.Code, warehouseCode);

                // Create transaction record
                var transaction = new InventoryTransactionDto
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    TransactionNumber = await _sequencerService.GetNextNumberAsync(INVENTORY_TRANSACTION_SEQUENCE_CODE),
                    Item = item,
                    TransactionType = transactionType,
                    Quantity = quantity, // Receipts are positive values
                    UnitCost = unitCost,
                    SourceWarehouseCode = null, // No source for receipts
                    DestinationWarehouseCode = warehouseCode,
                    ReferenceDocumentNumber = referenceDocument,
                    TransactionDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedBy = userName,
                    CreatedAt = DateTime.Now,
                    Notes = notes
                };

                // Update stock level
                decimal previousQuantity = stockLevel.QuantityOnHand;
                decimal previousCost = ((InventoryItemDto)item).AverageCost;
                
                // Update quantity on hand
                stockLevel.QuantityOnHand += quantity;
                stockLevel.LastUpdated = DateTime.Now;
                
                // Update average cost
                if (stockLevel.QuantityOnHand > 0)
                {
                    var totalCostBefore = previousQuantity * previousCost;
                    var newCost = quantity * unitCost;
                    var totalCostAfter = totalCostBefore + newCost;
                    
                    // Calculate new average cost
                    ((InventoryItemDto)item).AverageCost = totalCostAfter / stockLevel.QuantityOnHand;
                }

                // Save transaction
                _objectDb.InventoryTransactions.Add(transaction);
                
                _logger.LogInformation(
                    "Inventory receipt recorded: Item {ItemCode}, Quantity {Quantity}, Warehouse {WarehouseCode}, Reference {ReferenceDocument}, User {UserName}",
                    item.Code, quantity, warehouseCode, referenceDocument, userName);

                return transaction;
            });
        }

        /// <summary>
        /// Records an inventory issue (stock out)
        /// </summary>
        public async Task<IInventoryTransaction> RecordInventoryIssueAsync(
            IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            string userName,
            string notes = null)
        {
            return await _performanceLogger.Track(nameof(RecordInventoryIssueAsync), async () =>
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));
                
                if (quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
                
                if (string.IsNullOrEmpty(warehouseCode))
                    throw new ArgumentNullException(nameof(warehouseCode));
                
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));

                // Get stock level
                var stockLevel = await GetStockLevelAsync(item.Code, warehouseCode);
                
                // Check if there's enough stock
                if (stockLevel.QuantityOnHand < quantity)
                    throw new InvalidOperationException($"Insufficient stock for item {item.Code} in warehouse {warehouseCode}. Required: {quantity}, Available: {stockLevel.QuantityOnHand}");

                // Create transaction record with negative quantity for issues
                var transaction = new InventoryTransactionDto
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    TransactionNumber = await _sequencerService.GetNextNumberAsync(INVENTORY_TRANSACTION_SEQUENCE_CODE),
                    Item = item,
                    TransactionType = transactionType,
                    Quantity = -quantity, // Issues are negative values
                    UnitCost = ((InventoryItemDto)item).AverageCost, // Use current average cost
                    SourceWarehouseCode = warehouseCode,
                    DestinationWarehouseCode = null, // No destination for issues
                    ReferenceDocumentNumber = referenceDocument,
                    TransactionDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedBy = userName,
                    CreatedAt = DateTime.Now,
                    Notes = notes
                };

                // Update stock level
                stockLevel.QuantityOnHand -= quantity;
                stockLevel.LastUpdated = DateTime.Now;

                // Save transaction
                _objectDb.InventoryTransactions.Add(transaction);
                
                _logger.LogInformation(
                    "Inventory issue recorded: Item {ItemCode}, Quantity {Quantity}, Warehouse {WarehouseCode}, Reference {ReferenceDocument}, User {UserName}",
                    item.Code, quantity, warehouseCode, referenceDocument, userName);

                return transaction;
            });
        }

        /// <summary>
        /// Records an inventory transfer between warehouses
        /// </summary>
        public async Task<IInventoryTransaction> RecordInventoryTransferAsync(
            IInventoryItem item,
            decimal quantity,
            string sourceWarehouseCode,
            string destinationWarehouseCode,
            string referenceDocument,
            string userName,
            string notes = null)
        {
            return await _performanceLogger.Track(nameof(RecordInventoryTransferAsync), async () =>
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));
                
                if (quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
                
                if (string.IsNullOrEmpty(sourceWarehouseCode))
                    throw new ArgumentNullException(nameof(sourceWarehouseCode));
                
                if (string.IsNullOrEmpty(destinationWarehouseCode))
                    throw new ArgumentNullException(nameof(destinationWarehouseCode));
                
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName));
                
                if (sourceWarehouseCode.Equals(destinationWarehouseCode, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Source and destination warehouses cannot be the same");

                // Get source stock level
                var sourceStockLevel = await GetStockLevelAsync(item.Code, sourceWarehouseCode);
                
                // Check if there's enough stock in source
                if (sourceStockLevel.QuantityOnHand < quantity)
                    throw new InvalidOperationException($"Insufficient stock for item {item.Code} in source warehouse {sourceWarehouseCode}. Required: {quantity}, Available: {sourceStockLevel.QuantityOnHand}");

                // Get destination stock level
                var destStockLevel = await GetStockLevelAsync(item.Code, destinationWarehouseCode);

                // Create transaction record
                var transaction = new InventoryTransactionDto
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    TransactionNumber = await _sequencerService.GetNextNumberAsync(INVENTORY_TRANSACTION_SEQUENCE_CODE),
                    Item = item,
                    TransactionType = InventoryTransactionType.Transfer,
                    Quantity = quantity, // Transfers use positive values
                    UnitCost = ((InventoryItemDto)item).AverageCost, // Use current average cost
                    SourceWarehouseCode = sourceWarehouseCode,
                    DestinationWarehouseCode = destinationWarehouseCode,
                    ReferenceDocumentNumber = referenceDocument,
                    TransactionDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedBy = userName,
                    CreatedAt = DateTime.Now,
                    Notes = notes
                };

                // Update stock levels
                sourceStockLevel.QuantityOnHand -= quantity;
                sourceStockLevel.LastUpdated = DateTime.Now;
                
                destStockLevel.QuantityOnHand += quantity;
                destStockLevel.LastUpdated = DateTime.Now;

                // Save transaction
                _objectDb.InventoryTransactions.Add(transaction);
                
                _logger.LogInformation(
                    "Inventory transfer recorded: Item {ItemCode}, Quantity {Quantity}, From {SourceWarehouse} to {DestinationWarehouse}, Reference {ReferenceDocument}, User {UserName}",
                    item.Code, quantity, sourceWarehouseCode, destinationWarehouseCode, referenceDocument, userName);

                return transaction;
            });
        }

        /// <summary>
        /// Gets inventory transactions for an item within a date range
        /// </summary>
        public async Task<IEnumerable<IInventoryTransaction>> GetInventoryTransactionsAsync(
            string itemCode,
            DateOnly startDate,
            DateOnly endDate,
            string warehouseCode = null)
        {
            return await _performanceLogger.Track(nameof(GetInventoryTransactionsAsync), async () =>
            {
                if (string.IsNullOrEmpty(itemCode))
                    throw new ArgumentNullException(nameof(itemCode));

                // Query transactions
                var query = _objectDb.InventoryTransactions
                    .Where(t => string.Equals(t.Item.Code, itemCode, StringComparison.OrdinalIgnoreCase) &&
                                t.TransactionDate >= startDate &&
                                t.TransactionDate <= endDate);

                // Filter by warehouse if specified
                if (!string.IsNullOrEmpty(warehouseCode))
                {
                    query = query.Where(t =>
                        (t.SourceWarehouseCode != null && string.Equals(t.SourceWarehouseCode, warehouseCode, StringComparison.OrdinalIgnoreCase)) ||
                        (t.DestinationWarehouseCode != null && string.Equals(t.DestinationWarehouseCode, warehouseCode, StringComparison.OrdinalIgnoreCase))
                    );
                }

                return query.OrderBy(t => t.TransactionDate).ThenBy(t => t.CreatedAt).ToList();
            });
        }

        /// <summary>
        /// Checks if sufficient stock is available for an item
        /// </summary>
        public async Task<bool> IsStockAvailableAsync(string itemCode, decimal quantity, string warehouseCode)
        {
            return await _performanceLogger.Track(nameof(IsStockAvailableAsync), async () =>
            {
                if (string.IsNullOrEmpty(itemCode))
                    throw new ArgumentNullException(nameof(itemCode));
                
                if (quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
                
                if (string.IsNullOrEmpty(warehouseCode))
                    throw new ArgumentNullException(nameof(warehouseCode));

                // Get stock level
                var stockLevel = await GetStockLevelAsync(itemCode, warehouseCode);
                
                // Check available quantity (on hand minus reserved)
                return stockLevel.AvailableQuantity >= quantity;
            });
        }
    }
}