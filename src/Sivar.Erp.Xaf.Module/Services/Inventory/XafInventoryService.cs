using DevExpress.ExpressApp;
using DevExpress.Xpo;
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
    /// XAF-specific implementation of inventory service that works with XAF object spaces
    /// Inherits from the base InventoryService but adapts it to work with XPO persistence
    /// </summary>
    public class XafInventoryService : IInventoryService
    {
        private readonly ILogger<XafInventoryService> _logger;
        private readonly IObjectSpaceProvider _objectSpaceProvider;

        public XafInventoryService(
            ILogger<XafInventoryService> logger,
            IObjectSpaceProvider objectSpaceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _objectSpaceProvider = objectSpaceProvider ?? throw new ArgumentNullException(nameof(objectSpaceProvider));
        }

        /// <summary>
        /// Gets an inventory item by code using XAF object space
        /// </summary>
        public Task<IInventoryItem> GetInventoryItemAsync(string itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
            var item = objectSpace.GetObjects<InventoryItem>()
                .FirstOrDefault(i => i.Code == itemCode);

            return Task.FromResult<IInventoryItem>(item);
        }

        /// <summary>
        /// Creates a new inventory item using XAF object space
        /// </summary>
        public Task<IInventoryItem> CreateInventoryItemAsync(IInventoryItem item, string userName)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("User name cannot be null or empty", nameof(userName));

            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
            
            // Check if item already exists
            var existingItem = objectSpace.GetObjects<InventoryItem>()
                .FirstOrDefault(i => i.Code == item.Code);
            
            if (existingItem != null)
                throw new InvalidOperationException($"Inventory item with code '{item.Code}' already exists");

            // Create new XAF inventory item
            var newItem = objectSpace.CreateObject<InventoryItem>();
            
            // Copy properties from the interface - only map properties that exist in both interfaces
            newItem.Code = item.Code;
            newItem.Description = item.Description;
            newItem.Type = item.Type;
            newItem.BasePrice = item.BasePrice;
            
            // Map IInventoryItem specific properties
            newItem.IsInventoryTracked = item.IsInventoryTracked;
            newItem.UnitOfMeasure = item.UnitOfMeasure;
            newItem.ReorderPoint = item.ReorderPoint;
            newItem.ReorderQuantity = item.ReorderQuantity;
            newItem.AverageCost = item.AverageCost;
            newItem.Location = item.Location;
            newItem.ValuationMethod = item.ValuationMethod;
            
            // Set audit fields
            newItem.CreatedBy = userName;
            newItem.CreatedOn = DateTime.UtcNow;

            objectSpace.CommitChanges();

            _logger.LogInformation("Created new inventory item {ItemCode} by user {UserName}", 
                item.Code, userName);

            return Task.FromResult<IInventoryItem>(newItem);
        }

        /// <summary>
        /// Gets the stock level for an item using XAF object space
        /// </summary>
        public Task<IStockLevel> GetStockLevelAsync(string itemCode, string warehouseCode = null)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
            
            var query = objectSpace.GetObjects<StockLevel>()
                .Where(sl => sl.Item != null && sl.Item.Code == itemCode);

            if (!string.IsNullOrWhiteSpace(warehouseCode))
                query = query.Where(sl => sl.WarehouseCode == warehouseCode);

            var stockLevel = query.FirstOrDefault();
            return Task.FromResult<IStockLevel>(stockLevel);
        }

        /// <summary>
        /// Gets all stock levels for an item across all warehouses
        /// </summary>
        public Task<IEnumerable<IStockLevel>> GetStockLevelsAsync(string itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
            
            var stockLevels = objectSpace.GetObjects<StockLevel>()
                .Where(sl => sl.Item != null && sl.Item.Code == itemCode)
                .ToList();

            return Task.FromResult<IEnumerable<IStockLevel>>(stockLevels.Cast<IStockLevel>());
        }

        /// <summary>
        /// Gets stock levels for an item filtered by warehouse
        /// </summary>
        public Task<IEnumerable<IStockLevel>> GetStockLevelsAsync(string itemCode, string warehouseCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
            
            var query = objectSpace.GetObjects<StockLevel>()
                .Where(sl => sl.Item != null && sl.Item.Code == itemCode);

            if (!string.IsNullOrWhiteSpace(warehouseCode))
                query = query.Where(sl => sl.WarehouseCode == warehouseCode);

            var stockLevels = query.ToList();
            return Task.FromResult<IEnumerable<IStockLevel>>(stockLevels.Cast<IStockLevel>());
        }

        /// <summary>
        /// Processes a receipt of inventory using XAF object space
        /// </summary>
        public async Task<IInventoryTransaction> ReceiveInventoryAsync(
            IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            decimal unitCost,
            string userName,
            string notes = null)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            if (string.IsNullOrWhiteSpace(warehouseCode))
                throw new ArgumentException("Warehouse code cannot be null or empty", nameof(warehouseCode));

            if (unitCost < 0)
                throw new ArgumentException("Unit cost cannot be negative", nameof(unitCost));

            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
            
            // Get the inventory item in this object space
            var inventoryItem = objectSpace.GetObjects<InventoryItem>()
                .FirstOrDefault(i => i.Code == item.Code);
            
            if (inventoryItem == null)
                throw new InvalidOperationException($"Inventory item with code '{item.Code}' not found");

            // Create inventory transaction
            var transaction = objectSpace.CreateObject<InventoryTransaction>();
            transaction.Item = inventoryItem;
            transaction.Quantity = quantity;
            transaction.TransactionType = transactionType;
            transaction.SourceWarehouseCode = warehouseCode;
            transaction.DestinationWarehouseCode = warehouseCode;
            transaction.ReferenceDocumentNumber = referenceDocument;
            transaction.UnitCost = unitCost;
            transaction.TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
            transaction.Notes = notes;

            // Update stock level
            await UpdateStockLevelAsync(objectSpace, inventoryItem, warehouseCode, quantity, userName);

            // Update inventory item costs using weighted average
            await UpdateInventoryItemCostAsync(objectSpace, inventoryItem, quantity, unitCost, userName);

            // Create inventory layer for FIFO/LIFO costing
            await CreateInventoryLayerAsync(objectSpace, inventoryItem.Code, warehouseCode, quantity, unitCost, transaction.TransactionId, userName);

            objectSpace.CommitChanges();

            _logger.LogInformation(
                "Received {Quantity} units of item {ItemCode} at cost {UnitCost} in warehouse {WarehouseCode}",
                quantity, item.Code, unitCost, warehouseCode);

            return transaction;
        }

        /// <summary>
        /// Processes an issue of inventory using XAF object space
        /// </summary>
        public async Task<IInventoryTransaction> IssueInventoryAsync(
            IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            string userName,
            string notes = null)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            if (string.IsNullOrWhiteSpace(warehouseCode))
                throw new ArgumentException("Warehouse code cannot be null or empty", nameof(warehouseCode));

            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
            
            // Get the inventory item in this object space
            var inventoryItem = objectSpace.GetObjects<InventoryItem>()
                .FirstOrDefault(i => i.Code == item.Code);
            
            if (inventoryItem == null)
                throw new InvalidOperationException($"Inventory item with code '{item.Code}' not found");

            // Check available stock
            var stockLevel = objectSpace.GetObjects<StockLevel>()
                .FirstOrDefault(sl => sl.Item.Code == item.Code && sl.WarehouseCode == warehouseCode);
            
            if (stockLevel == null || stockLevel.AvailableQuantity < quantity)
            {
                var available = stockLevel?.AvailableQuantity ?? 0;
                throw new InvalidOperationException(
                    $"Insufficient stock for item {item.Code} in warehouse {warehouseCode}. " +
                    $"Available: {available}, Requested: {quantity}");
            }

            // Calculate weighted average cost for the issue
            var averageCost = await CalculateAverageCostAsync(objectSpace, item.Code, warehouseCode);

            // Create inventory transaction
            var transaction = objectSpace.CreateObject<InventoryTransaction>();
            transaction.Item = inventoryItem;
            transaction.Quantity = -quantity; // Negative for issues
            transaction.TransactionType = transactionType;
            transaction.SourceWarehouseCode = warehouseCode;
            transaction.DestinationWarehouseCode = warehouseCode;
            transaction.ReferenceDocumentNumber = referenceDocument;
            transaction.UnitCost = averageCost;
            transaction.TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
            transaction.Notes = notes;

            // Update stock level (negative quantity for issue)
            await UpdateStockLevelAsync(objectSpace, inventoryItem, warehouseCode, -quantity, userName);

            // Consume inventory layers for FIFO/LIFO costing
            await ConsumeInventoryLayersAsync(objectSpace, inventoryItem.Code, warehouseCode, quantity, transaction.TransactionId, userName);

            objectSpace.CommitChanges();

            _logger.LogInformation(
                "Issued {Quantity} units of item {ItemCode} at cost {UnitCost} from warehouse {WarehouseCode}",
                quantity, item.Code, averageCost, warehouseCode);

            return transaction;
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
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
            
            var query = objectSpace.GetObjects<InventoryTransaction>()
                .Where(t => t.Item.Code == itemCode);

            if (fromDate.HasValue)
                query = query.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.TransactionDate <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(warehouseCode))
                query = query.Where(t => t.SourceWarehouseCode == warehouseCode || t.DestinationWarehouseCode == warehouseCode);

            var transactions = query.OrderBy(t => t.TransactionDate).ThenBy(t => t.CreatedOn).ToList();
            return Task.FromResult<IEnumerable<IInventoryTransaction>>(transactions.Cast<IInventoryTransaction>());
        }

        /// <summary>
        /// Gets available quantity considering reservations
        /// </summary>
        public Task<decimal> GetAvailableQuantityAsync(string itemCode, string warehouseCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("Item code cannot be null or empty", nameof(itemCode));

            if (string.IsNullOrWhiteSpace(warehouseCode))
                throw new ArgumentException("Warehouse code cannot be null or empty", nameof(warehouseCode));

            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
            
            var stockLevel = objectSpace.GetObjects<StockLevel>()
                .FirstOrDefault(sl => sl.Item != null && sl.Item.Code == itemCode && sl.WarehouseCode == warehouseCode);

            return Task.FromResult(stockLevel?.AvailableQuantity ?? 0);
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
            IInventoryItem item,
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
            IInventoryItem item,
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
        private Task UpdateStockLevelAsync(IObjectSpace objectSpace, InventoryItem item, string warehouseCode, decimal quantityChange, string userName)
        {
            var stockLevel = objectSpace.GetObjects<StockLevel>()
                .FirstOrDefault(sl => sl.Item.Code == item.Code && sl.WarehouseCode == warehouseCode);

            if (stockLevel == null)
            {
                // Create new stock level record
                stockLevel = objectSpace.CreateObject<StockLevel>();
                stockLevel.Item = item;
                stockLevel.WarehouseCode = warehouseCode;
                stockLevel.QuantityOnHand = quantityChange;
                stockLevel.QuantityReserved = 0;
            }
            else
            {
                // Update existing stock level
                stockLevel.QuantityOnHand += quantityChange;
            }

            _logger.LogDebug(
                "Updated stock level for item {ItemCode} in warehouse {WarehouseCode}: " +
                "Change={QuantityChange}, New OnHand={OnHand}, Available={Available}",
                item.Code, warehouseCode, quantityChange, stockLevel.QuantityOnHand, stockLevel.AvailableQuantity);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates inventory item cost using weighted average method
        /// </summary>
        private async Task UpdateInventoryItemCostAsync(IObjectSpace objectSpace, InventoryItem item, decimal quantity, decimal unitCost, string userName)
        {
            // Get current total value
            var totalOnHand = 0m;
            var totalValue = 0m;

            // Sum up quantities and values across all warehouses
            var stockLevels = objectSpace.GetObjects<StockLevel>()
                .Where(sl => sl.Item.Code == item.Code)
                .ToList();

            foreach (var stockLevel in stockLevels)
            {
                totalOnHand += stockLevel.QuantityOnHand;
                totalValue += stockLevel.QuantityOnHand * item.AverageCost;
            }

            // Add the new receipt
            totalOnHand += quantity;
            totalValue += quantity * unitCost;

            // Calculate new weighted average cost
            if (totalOnHand > 0)
            {
                item.AverageCost = totalValue / totalOnHand;
            }

            _logger.LogDebug(
                "Updated weighted average cost for item {ItemCode}: New cost={Cost}, Total quantity={Quantity}",
                item.Code, item.AverageCost, totalOnHand);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates an inventory layer for FIFO/LIFO costing
        /// </summary>
        private Task CreateInventoryLayerAsync(
            IObjectSpace objectSpace,
            string itemCode, 
            string warehouseCode, 
            decimal quantity, 
            decimal unitCost, 
            string transactionId, 
            string userName)
        {
            var layer = objectSpace.CreateObject<InventoryLayer>();
            layer.Item = objectSpace.GetObjects<InventoryItem>().FirstOrDefault(i => i.Code == itemCode);
            layer.WarehouseCode = warehouseCode;
            layer.OriginalQuantity = quantity;
            layer.RemainingQuantity = quantity;
            layer.UnitCost = unitCost;
            layer.SourceTransactionId = transactionId; // Updated property name

            _logger.LogDebug(
                "Created inventory layer for item {ItemCode} in warehouse {WarehouseCode}: " +
                "Quantity={Quantity}, Cost={Cost}, Transaction={TransactionId}",
                itemCode, warehouseCode, quantity, unitCost, transactionId);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Consumes inventory layers for FIFO/LIFO costing
        /// </summary>
        private Task ConsumeInventoryLayersAsync(
            IObjectSpace objectSpace,
            string itemCode, 
            string warehouseCode, 
            decimal quantityToConsume, 
            string transactionId, 
            string userName)
        {
            // Get available layers ordered by creation date (FIFO)
            var availableLayers = objectSpace.GetObjects<InventoryLayer>()
                .Where(l => l.Item.Code == itemCode && 
                           l.WarehouseCode == warehouseCode && 
                           l.RemainingQuantity > 0)
                .OrderBy(l => l.CreatedOn)
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

            return Task.CompletedTask;
        }

        /// <summary>
        /// Calculates the weighted average cost for an item in a warehouse
        /// </summary>
        private Task<decimal> CalculateAverageCostAsync(IObjectSpace objectSpace, string itemCode, string warehouseCode)
        {
            var layers = objectSpace.GetObjects<InventoryLayer>()
                .Where(l => l.Item.Code == itemCode && 
                           l.WarehouseCode == warehouseCode && 
                           l.RemainingQuantity > 0)
                .ToList();

            if (!layers.Any())
            {
                // Fall back to item average cost
                var item = objectSpace.GetObjects<InventoryItem>()
                    .FirstOrDefault(i => i.Code == itemCode);
                return Task.FromResult(item?.AverageCost ?? 0);
            }

            var totalQuantity = layers.Sum(l => l.RemainingQuantity);
            var totalValue = layers.Sum(l => l.RemainingQuantity * l.UnitCost);

            return Task.FromResult(totalQuantity > 0 ? totalValue / totalQuantity : 0);
        }

        #endregion
    }
}