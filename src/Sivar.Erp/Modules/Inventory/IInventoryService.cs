using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Service interface for managing inventory operations
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Gets an inventory item by code
        /// </summary>
        /// <param name="itemCode">Item code to retrieve</param>
        /// <returns>The inventory item if found, null otherwise</returns>
        Task<IInventoryItem> GetInventoryItemAsync(string itemCode);
        
        /// <summary>
        /// Creates a new inventory item
        /// </summary>
        /// <param name="item">Item to create</param>
        /// <param name="userName">User creating the item</param>
        /// <returns>The created inventory item</returns>
        Task<IInventoryItem> CreateInventoryItemAsync(IInventoryItem item, string userName);
        
        /// <summary>
        /// Gets the stock level for an item in a specific warehouse
        /// </summary>
        /// <param name="itemCode">Item code to check</param>
        /// <param name="warehouseCode">Warehouse code (optional)</param>
        /// <returns>The stock level information</returns>
        Task<IStockLevel> GetStockLevelAsync(string itemCode, string warehouseCode = null);
        
        /// <summary>
        /// Gets all stock levels for an item across all warehouses
        /// </summary>
        /// <param name="itemCode">Item code to check</param>
        /// <returns>Collection of stock levels</returns>
        Task<IEnumerable<IStockLevel>> GetStockLevelsAsync(string itemCode);
        
        /// <summary>
        /// Gets stock levels for an item filtered by warehouse
        /// </summary>
        /// <param name="itemCode">Item code to check</param>
        /// <param name="warehouseCode">Warehouse code to filter by (optional)</param>
        /// <returns>Collection of stock levels</returns>
        Task<IEnumerable<IStockLevel>> GetStockLevelsAsync(string itemCode, string warehouseCode);
        
        /// <summary>
        /// Processes a receipt of inventory
        /// </summary>
        /// <param name="item">Item being received</param>
        /// <param name="quantity">Quantity being received</param>
        /// <param name="warehouseCode">Warehouse receiving the stock</param>
        /// <param name="transactionType">Type of receipt transaction</param>
        /// <param name="referenceDocument">Reference document number</param>
        /// <param name="unitCost">Unit cost of the items</param>
        /// <param name="userName">User recording the receipt</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The created inventory transaction</returns>
        Task<IInventoryTransaction> ReceiveInventoryAsync(
            IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            decimal unitCost,
            string userName,
            string notes = null);
            
        /// <summary>
        /// Processes an issue of inventory
        /// </summary>
        /// <param name="item">Item being issued</param>
        /// <param name="quantity">Quantity being issued</param>
        /// <param name="warehouseCode">Warehouse issuing the stock</param>
        /// <param name="transactionType">Type of issue transaction</param>
        /// <param name="referenceDocument">Reference document number</param>
        /// <param name="userName">User recording the issue</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The created inventory transaction</returns>
        Task<IInventoryTransaction> IssueInventoryAsync(
            IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            string userName,
            string notes = null);
            
        /// <summary>
        /// Gets inventory transactions for an item within a date range
        /// </summary>
        /// <param name="itemCode">Item code</param>
        /// <param name="fromDate">Start date (optional)</param>
        /// <param name="toDate">End date (optional)</param>
        /// <param name="warehouseCode">Warehouse code (optional)</param>
        /// <returns>Collection of inventory transactions</returns>
        Task<IEnumerable<IInventoryTransaction>> GetInventoryTransactionsAsync(
            string itemCode,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            string warehouseCode = null);
            
        /// <summary>
        /// Gets available quantity considering reservations
        /// </summary>
        /// <param name="itemCode">Item code</param>
        /// <param name="warehouseCode">Warehouse code</param>
        /// <returns>Available quantity</returns>
        Task<decimal> GetAvailableQuantityAsync(string itemCode, string warehouseCode);
        
        /// <summary>
        /// Checks if sufficient stock is available for a given quantity
        /// </summary>
        /// <param name="itemCode">Item code</param>
        /// <param name="warehouseCode">Warehouse code</param>
        /// <param name="quantity">Required quantity</param>
        /// <returns>True if stock is available, false otherwise</returns>
        Task<bool> IsStockAvailableAsync(string itemCode, string warehouseCode, decimal quantity);
        
        /// <summary>
        /// Records an inventory receipt (alias for ReceiveInventoryAsync for compatibility)
        /// </summary>
        /// <param name="item">Item being received</param>
        /// <param name="quantity">Quantity being received</param>
        /// <param name="warehouseCode">Warehouse receiving the stock</param>
        /// <param name="transactionType">Type of receipt transaction</param>
        /// <param name="referenceDocument">Reference document number</param>
        /// <param name="unitCost">Unit cost of the items</param>
        /// <param name="userName">User recording the receipt</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The created inventory transaction</returns>
        Task<IInventoryTransaction> RecordInventoryReceiptAsync(
            IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            decimal unitCost,
            string userName,
            string notes = null);
            
        /// <summary>
        /// Records an inventory issue (alias for IssueInventoryAsync for compatibility)
        /// </summary>
        /// <param name="item">Item being issued</param>
        /// <param name="quantity">Quantity being issued</param>
        /// <param name="warehouseCode">Warehouse issuing the stock</param>
        /// <param name="transactionType">Type of issue transaction</param>
        /// <param name="referenceDocument">Reference document number</param>
        /// <param name="userName">User recording the issue</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The created inventory transaction</returns>
        Task<IInventoryTransaction> RecordInventoryIssueAsync(
            IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            InventoryTransactionType transactionType,
            string referenceDocument,
            string userName,
            string notes = null);
    }
}