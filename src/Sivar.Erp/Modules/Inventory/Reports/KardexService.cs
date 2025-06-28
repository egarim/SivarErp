using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.Services;

namespace Sivar.Erp.Modules.Inventory.Reports
{
    /// <summary>
    /// Service for generating inventory kardex reports
    /// </summary>
    public class KardexService : IKardexService
    {
        private readonly ILogger<KardexService> _logger;
        private readonly IObjectDb _objectDb;
        private readonly IInventoryService _inventoryService;
        private readonly ISequencerService _sequencerService;
        private readonly PerformanceLogger<KardexService> _performanceLogger;
        
        private const string KARDEX_REPORT_SEQUENCE_CODE = "KRX_REPORT";

        /// <summary>
        /// Initializes a new instance of the KardexService class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="objectDb">Object database</param>
        /// <param name="inventoryService">Inventory service</param>
        /// <param name="sequencerService">Sequencer service</param>
        /// <param name="contextProvider">Performance context provider</param>
        public KardexService(
            ILogger<KardexService> logger,
            IObjectDb objectDb,
            IInventoryService inventoryService,
            ISequencerService sequencerService,
            IPerformanceContextProvider contextProvider = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _objectDb = objectDb ?? throw new ArgumentNullException(nameof(objectDb));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _sequencerService = sequencerService ?? throw new ArgumentNullException(nameof(sequencerService));
            _performanceLogger = new PerformanceLogger<KardexService>(logger, PerformanceLogMode.All, 100, 10_000_000, objectDb, contextProvider);
        }

        /// <summary>
        /// Generates a kardex report for an item for a specified period
        /// </summary>
        public async Task<KardexReportDto> GenerateKardexReportAsync(
            string itemCode, 
            DateOnly startDate, 
            DateOnly endDate, 
            string warehouseCode = null)
        {
            return await _performanceLogger.Track(nameof(GenerateKardexReportAsync), async () =>
            {
                if (string.IsNullOrEmpty(itemCode))
                    throw new ArgumentNullException(nameof(itemCode));
                
                // Get the inventory item
                var inventoryItem = await _inventoryService.GetInventoryItemAsync(itemCode);
                if (inventoryItem == null)
                    throw new InvalidOperationException($"Inventory item with code {itemCode} not found");

                // Get transactions for the period
                var transactions = await _inventoryService.GetInventoryTransactionsAsync(itemCode, startDate, endDate, warehouseCode);
                if (transactions == null)
                    transactions = new List<IInventoryTransaction>();

                // Create the report
                var reportId = await _sequencerService.GetNextNumberAsync(KARDEX_REPORT_SEQUENCE_CODE);
                var report = new KardexReportDto
                {
                    ReportId = reportId,
                    Item = inventoryItem,
                    WarehouseCode = warehouseCode,
                    StartDate = startDate,
                    EndDate = endDate,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = "System"
                };

                // Calculate opening balance
                var openingBalance = await CalculateOpeningBalanceAsync(itemCode, startDate, warehouseCode);
                report.OpeningQuantity = openingBalance.quantity;
                report.OpeningValue = openingBalance.value;

                // Calculate movements
                decimal runningQuantity = report.OpeningQuantity;
                decimal runningValue = report.OpeningValue;
                decimal totalInboundQuantity = 0;
                decimal totalInboundValue = 0;
                decimal totalOutboundQuantity = 0;
                decimal totalOutboundValue = 0;

                foreach (var transaction in transactions.OrderBy(t => t.TransactionDate).ThenBy(t => t.CreatedAt))
                {
                    var isInbound = transaction.Quantity > 0;
                    var quantity = Math.Abs(transaction.Quantity);
                    var value = quantity * transaction.UnitCost;

                    var movement = new KardexMovementDto
                    {
                        MovementId = Guid.NewGuid().ToString(),
                        Transaction = transaction,
                        TransactionDate = transaction.TransactionDate,
                        ReferenceDocument = transaction.ReferenceDocumentNumber,
                        Description = GetTransactionDescription(transaction)
                    };

                    if (isInbound)
                    {
                        movement.InboundQuantity = quantity;
                        movement.InboundValue = value;
                        movement.OutboundQuantity = 0;
                        movement.OutboundValue = 0;
                        
                        runningQuantity += quantity;
                        runningValue += value;
                        
                        totalInboundQuantity += quantity;
                        totalInboundValue += value;
                    }
                    else
                    {
                        movement.InboundQuantity = 0;
                        movement.InboundValue = 0;
                        movement.OutboundQuantity = quantity;
                        movement.OutboundValue = value;
                        
                        runningQuantity -= quantity;
                        runningValue -= value;
                        
                        totalOutboundQuantity += quantity;
                        totalOutboundValue += value;
                    }

                    movement.BalanceQuantity = runningQuantity;
                    movement.BalanceValue = runningValue;
                    movement.AverageUnitCost = runningQuantity > 0 ? runningValue / runningQuantity : 0;

                    report.Movements.Add(movement);
                }

                // Set report totals
                report.TotalInboundQuantity = totalInboundQuantity;
                report.TotalInboundValue = totalInboundValue;
                report.TotalOutboundQuantity = totalOutboundQuantity;
                report.TotalOutboundValue = totalOutboundValue;
                report.ClosingQuantity = runningQuantity;
                report.ClosingValue = runningValue;

                _logger.LogInformation(
                    "Generated kardex report {ReportId} for item {ItemCode}, period {StartDate} to {EndDate}, with {MovementCount} movements",
                    reportId, itemCode, startDate, endDate, report.Movements.Count);

                return report;
            });
        }

        /// <summary>
        /// Gets a kardex movement entry by transaction ID
        /// </summary>
        public async Task<KardexMovementDto> GetKardexMovementByTransactionAsync(string transactionId)
        {
            return await _performanceLogger.Track(nameof(GetKardexMovementByTransactionAsync), async () =>
            {
                if (string.IsNullOrEmpty(transactionId))
                    throw new ArgumentNullException(nameof(transactionId));

                // Find the transaction
                var transaction = _objectDb.InventoryTransactions.FirstOrDefault(t => t.TransactionId == transactionId);
                if (transaction == null)
                    return null;

                // Create a kardex movement for this transaction
                var isInbound = transaction.Quantity > 0;
                var quantity = Math.Abs(transaction.Quantity);
                var value = quantity * transaction.UnitCost;

                var movement = new KardexMovementDto
                {
                    MovementId = Guid.NewGuid().ToString(),
                    Transaction = transaction,
                    TransactionDate = transaction.TransactionDate,
                    ReferenceDocument = transaction.ReferenceDocumentNumber,
                    Description = GetTransactionDescription(transaction)
                };

                if (isInbound)
                {
                    movement.InboundQuantity = quantity;
                    movement.InboundValue = value;
                    movement.OutboundQuantity = 0;
                    movement.OutboundValue = 0;
                }
                else
                {
                    movement.InboundQuantity = 0;
                    movement.InboundValue = 0;
                    movement.OutboundQuantity = quantity;
                    movement.OutboundValue = value;
                }

                // We don't have the running balance when getting a single movement
                movement.BalanceQuantity = 0;
                movement.BalanceValue = 0;
                movement.AverageUnitCost = 0;

                return movement;
            });
        }

        /// <summary>
        /// Gets current stock valuation for all inventory items
        /// </summary>
        public async Task<InventoryValuationReportDto> GetInventoryValuationAsync(
            DateOnly asOfDate,
            string warehouseCode = null)
        {
            return await _performanceLogger.Track(nameof(GetInventoryValuationAsync), async () =>
            {
                // Create the report
                var report = new InventoryValuationReportDto
                {
                    ReportId = Guid.NewGuid().ToString(),
                    AsOfDate = asOfDate,
                    WarehouseCode = warehouseCode,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = "System"
                };

                // Get all inventory items
                var inventoryItems = _objectDb.InventoryItems
                    .Where(i => ((InventoryItemDto)i).IsInventoryTracked)
                    .ToList();

                decimal totalValue = 0;

                // Calculate valuation for each item
                foreach (var item in inventoryItems)
                {
                    // Get stock levels for the item
                    var stockLevels = await _inventoryService.GetStockLevelsAsync(item.Code, warehouseCode);
                    
                    foreach (var stockLevel in stockLevels)
                    {
                        var itemValue = stockLevel.QuantityOnHand * ((InventoryItemDto)item).AverageCost;
                        
                        var valuationItem = new InventoryValuationItemDto
                        {
                            Item = item,
                            WarehouseCode = stockLevel.WarehouseCode,
                            QuantityOnHand = stockLevel.QuantityOnHand,
                            AverageCost = ((InventoryItemDto)item).AverageCost,
                            TotalValue = itemValue
                        };
                        
                        report.Items.Add(valuationItem);
                        totalValue += itemValue;
                    }
                }

                report.TotalInventoryValue = totalValue;

                _logger.LogInformation(
                    "Generated inventory valuation report as of {AsOfDate} with {ItemCount} items, total value: {TotalValue}",
                    asOfDate, report.Items.Count, totalValue);

                return report;
            });
        }

        /// <summary>
        /// Gets a list of items that need reordering (below reorder point)
        /// </summary>
        public async Task<IEnumerable<ReorderItemDto>> GetItemsToReorderAsync(string warehouseCode = null)
        {
            return await _performanceLogger.Track(nameof(GetItemsToReorderAsync), async () =>
            {
                var result = new List<ReorderItemDto>();

                // Get all inventory items that are tracked
                var inventoryItems = _objectDb.InventoryItems
                    .Where(i => ((InventoryItemDto)i).IsInventoryTracked && ((InventoryItemDto)i).ReorderPoint > 0)
                    .ToList();

                foreach (var item in inventoryItems)
                {
                    // Get stock levels for the item
                    var stockLevels = await _inventoryService.GetStockLevelsAsync(item.Code, warehouseCode);
                    
                    foreach (var stockLevel in stockLevels)
                    {
                        // Check if stock level is below reorder point
                        if (stockLevel.QuantityOnHand <= ((InventoryItemDto)item).ReorderPoint)
                        {
                            // Find last purchase transaction
                            var lastPurchase = _objectDb.InventoryTransactions
                                .Where(t => t.Item.Code == item.Code &&
                                       t.TransactionType == InventoryTransactionType.PurchaseReceipt &&
                                       (warehouseCode == null || t.DestinationWarehouseCode == warehouseCode))
                                .OrderByDescending(t => t.TransactionDate)
                                .FirstOrDefault();

                            var reorderItem = new ReorderItemDto
                            {
                                Item = item,
                                WarehouseCode = stockLevel.WarehouseCode,
                                QuantityOnHand = stockLevel.QuantityOnHand,
                                AvailableQuantity = stockLevel.AvailableQuantity,
                                ReorderPoint = ((InventoryItemDto)item).ReorderPoint,
                                SuggestedOrderQuantity = ((InventoryItemDto)item).ReorderQuantity
                            };

                            if (lastPurchase != null)
                            {
                                reorderItem.LastPurchaseDate = lastPurchase.TransactionDate;
                                reorderItem.LastPurchaseCost = lastPurchase.UnitCost;
                            }

                            result.Add(reorderItem);
                        }
                    }
                }

                _logger.LogInformation(
                    "Found {ItemCount} items that need reordering",
                    result.Count);

                return result;
            });
        }

        /// <summary>
        /// Calculate opening balance for an item as of a specific date
        /// </summary>
        private async Task<(decimal quantity, decimal value)> CalculateOpeningBalanceAsync(
            string itemCode, 
            DateOnly asOfDate, 
            string warehouseCode)
        {
            // Get all transactions for the item before the start date
            var previousTransactions = _objectDb.InventoryTransactions
                .Where(t => t.Item.Code == itemCode && 
                       t.TransactionDate < asOfDate &&
                       (warehouseCode == null || 
                        t.SourceWarehouseCode == warehouseCode || 
                        t.DestinationWarehouseCode == warehouseCode))
                .ToList();

            // Calculate opening balance
            decimal openingQuantity = 0;
            decimal openingValue = 0;

            foreach (var transaction in previousTransactions.OrderBy(t => t.TransactionDate).ThenBy(t => t.CreatedAt))
            {
                var isAffectingWarehouse = warehouseCode == null || 
                                          (transaction.Quantity > 0 && transaction.DestinationWarehouseCode == warehouseCode) ||
                                          (transaction.Quantity < 0 && transaction.SourceWarehouseCode == warehouseCode);

                if (isAffectingWarehouse)
                {
                    var quantity = transaction.Quantity;
                    var value = quantity * transaction.UnitCost;
                    
                    // For transfers, we need to handle source and destination
                    if (transaction.TransactionType == InventoryTransactionType.Transfer)
                    {
                        if (warehouseCode != null)
                        {
                            if (transaction.SourceWarehouseCode == warehouseCode)
                            {
                                // This is an outbound transfer from our warehouse
                                quantity = -Math.Abs(transaction.Quantity);
                                value = quantity * transaction.UnitCost;
                            }
                            else if (transaction.DestinationWarehouseCode == warehouseCode)
                            {
                                // This is an inbound transfer to our warehouse
                                quantity = Math.Abs(transaction.Quantity);
                                value = quantity * transaction.UnitCost;
                            }
                            else
                            {
                                // Transfer doesn't affect this warehouse
                                continue;
                            }
                        }
                    }
                    
                    openingQuantity += quantity;
                    openingValue += value;
                }
            }

            return (openingQuantity, openingValue);
        }

        /// <summary>
        /// Gets a human-readable description for a transaction
        /// </summary>
        private string GetTransactionDescription(IInventoryTransaction transaction)
        {
            return transaction.TransactionType switch
            {
                InventoryTransactionType.PurchaseReceipt => $"Purchase Receipt ({transaction.ReferenceDocumentNumber})",
                InventoryTransactionType.SalesIssue => $"Sales Issue ({transaction.ReferenceDocumentNumber})",
                InventoryTransactionType.Adjustment => "Inventory Adjustment",
                InventoryTransactionType.Transfer => $"Transfer from {transaction.SourceWarehouseCode} to {transaction.DestinationWarehouseCode}",
                InventoryTransactionType.PhysicalCount => "Physical Count Adjustment",
                InventoryTransactionType.CustomerReturn => $"Customer Return ({transaction.ReferenceDocumentNumber})",
                InventoryTransactionType.SupplierReturn => $"Supplier Return ({transaction.ReferenceDocumentNumber})",
                InventoryTransactionType.ProductionInput => "Production Material Consumption",
                InventoryTransactionType.ProductionOutput => "Production Output",
                InventoryTransactionType.WriteOff => "Inventory Write-off",
                InventoryTransactionType.Samples => "Samples / Marketing",
                _ => "Unknown Transaction"
            };
        }
    }
}