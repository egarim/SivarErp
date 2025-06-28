using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.Modules.Inventory.Reports;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Main interface for the inventory module
    /// </summary>
    public interface IInventoryModule
    {
        /// <summary>
        /// Gets the inventory service
        /// </summary>
        IInventoryService InventoryService { get; }
        
        /// <summary>
        /// Gets the inventory reservation service
        /// </summary>
        IInventoryReservationService ReservationService { get; }
        
        /// <summary>
        /// Gets the kardex service
        /// </summary>
        IKardexService KardexService { get; }
        
        /// <summary>
        /// Initializes the inventory module and creates necessary sequences
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Registers inventory sequences in the system
        /// </summary>
        /// <param name="sequenceDtos">Collection of sequence DTOs to register with</param>
        void RegisterSequence(IEnumerable<SequenceDto> sequenceDtos);
        
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
        /// Gets the stock level for an item
        /// </summary>
        /// <param name="itemCode">Item code to check</param>
        /// <param name="warehouseCode">Optional warehouse code</param>
        /// <returns>The stock level information</returns>
        Task<IStockLevel> GetStockLevelAsync(string itemCode, string warehouseCode = null);
        
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
        /// Creates a reservation for an item
        /// </summary>
        /// <param name="item">Item to reserve</param>
        /// <param name="quantity">Quantity to reserve</param>
        /// <param name="warehouseCode">Warehouse code</param>
        /// <param name="sourceDocumentNumber">Source document number</param>
        /// <param name="expiryMinutes">Minutes until expiration</param>
        /// <param name="userName">User creating the reservation</param>
        /// <returns>The created reservation</returns>
        Task<IInventoryReservation> ReserveInventoryAsync(
            IInventoryItem item,
            decimal quantity,
            string warehouseCode,
            string sourceDocumentNumber,
            int expiryMinutes,
            string userName);
            
        /// <summary>
        /// Cancels an inventory reservation
        /// </summary>
        /// <param name="reservationId">Reservation ID to cancel</param>
        /// <param name="userName">User cancelling the reservation</param>
        /// <returns>True if cancelled successfully</returns>
        Task<bool> CancelReservationAsync(string reservationId, string userName);
        
        /// <summary>
        /// Fulfills an inventory reservation by issuing the stock
        /// </summary>
        /// <param name="reservationId">Reservation ID to fulfill</param>
        /// <param name="actualQuantity">Actual quantity fulfilled</param>
        /// <param name="userName">User fulfilling the reservation</param>
        /// <returns>The inventory transaction created</returns>
        Task<IInventoryTransaction> FulfillReservationAsync(
            string reservationId,
            decimal actualQuantity,
            string userName);
            
        /// <summary>
        /// Generates a kardex report for an item
        /// </summary>
        /// <param name="itemCode">Item code</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="warehouseCode">Optional warehouse code</param>
        /// <returns>The kardex report</returns>
        Task<KardexReportDto> GenerateKardexReportAsync(
            string itemCode,
            DateOnly startDate,
            DateOnly endDate,
            string warehouseCode = null);
            
        /// <summary>
        /// Gets inventory valuation as of a specific date
        /// </summary>
        /// <param name="asOfDate">Date for valuation</param>
        /// <param name="warehouseCode">Optional warehouse code</param>
        /// <returns>The inventory valuation report</returns>
        Task<InventoryValuationReportDto> GetInventoryValuationAsync(
            DateOnly asOfDate,
            string warehouseCode = null);
    }
}