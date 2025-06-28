using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Main interface for the inventory service that manages inventory operations
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Gets an inventory item by code
        /// </summary>
        /// <param name="itemCode">Item code to search for</param>
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
        /// Updates an existing inventory item
        /// </summary>
        /// <param name="item">Item to update</param>
        /// <param name="userName">User updating the item</param>
        /// <returns>The updated inventory item</returns>
        Task<IInventoryItem> UpdateInventoryItemAsync(IInventoryItem item, string userName);
        
        /// <summary>
        /// Gets current stock levels for an item
        /// </summary>
        /// <param name="itemCode">Item code to check</param>
        /// <param name="warehouseCode">Optional warehouse code to filter by</param>
        /// <returns>List of stock levels for the item across warehouses</returns>
        Task<IEnumerable<IStockLevel>> GetStockLevelsAsync(string itemCode, string warehouseCode = null);
        
        /// <summary>
        /// Gets current stock level for a specific item in a specific warehouse
        /// </summary>
        /// <param name="itemCode">Item code</param>
        /// <param name="warehouseCode">Warehouse code</param>
        /// <returns>Stock level for the item in the warehouse</returns>
        Task<IStockLevel> GetStockLevelAsync(string itemCode, string warehouseCode);
        
        /// <summary>
        /// Records an inventory receipt (stock in)
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
        /// Records an inventory issue (stock out)
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
            
        /// <summary>
        /// Records an inventory transfer between warehouses
        /// </summary>
        /// <param name="item">Item being transferred</param>
        /// <param name="quantity">Quantity being transferred</param>
        /// <param name="sourceWarehouseCode">Source warehouse code</param>
        /// <param name="destinationWarehouseCode">Destination warehouse code</param>
        /// <param name="referenceDocument">Reference document number</param>
        /// <param name="userName">User recording the transfer</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The created inventory transaction</returns>
        Task<IInventoryTransaction> RecordInventoryTransferAsync(
            IInventoryItem item,
            decimal quantity,
            string sourceWarehouseCode,
            string destinationWarehouseCode,
            string referenceDocument,
            string userName,
            string notes = null);
            
        /// <summary>
        /// Gets inventory transactions for an item within a date range
        /// </summary>
        /// <param name="itemCode">Item code to filter by</param>
        /// <param name="startDate">Start date of the range</param>
        /// <param name="endDate">End date of the range</param>
        /// <param name="warehouseCode">Optional warehouse code to filter by</param>
        /// <returns>List of inventory transactions matching the criteria</returns>
        Task<IEnumerable<IInventoryTransaction>> GetInventoryTransactionsAsync(
            string itemCode,
            DateOnly startDate,
            DateOnly endDate,
            string warehouseCode = null);
            
        /// <summary>
        /// Checks if sufficient stock is available for an item
        /// </summary>
        /// <param name="itemCode">Item code to check</param>
        /// <param name="quantity">Quantity required</param>
        /// <param name="warehouseCode">Warehouse code to check</param>
        /// <returns>True if sufficient stock is available, false otherwise</returns>
        Task<bool> IsStockAvailableAsync(string itemCode, decimal quantity, string warehouseCode);
    }
}