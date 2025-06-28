using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory.Reports
{
    /// <summary>
    /// Interface for generating and querying inventory kardex reports
    /// </summary>
    public interface IKardexService
    {
        /// <summary>
        /// Generates a kardex report for an item for a specified period
        /// </summary>
        /// <param name="itemCode">Item code to generate report for</param>
        /// <param name="startDate">Start date of the report period</param>
        /// <param name="endDate">End date of the report period</param>
        /// <param name="warehouseCode">Optional warehouse code to filter by</param>
        /// <returns>The generated kardex report</returns>
        Task<KardexReportDto> GenerateKardexReportAsync(
            string itemCode, 
            DateOnly startDate, 
            DateOnly endDate, 
            string warehouseCode = null);
            
        /// <summary>
        /// Gets a kardex movement entry by transaction ID
        /// </summary>
        /// <param name="transactionId">Transaction ID to retrieve</param>
        /// <returns>The kardex entry if found, null otherwise</returns>
        Task<KardexMovementDto> GetKardexMovementByTransactionAsync(string transactionId);
        
        /// <summary>
        /// Gets current stock valuation for all inventory items
        /// </summary>
        /// <param name="asOfDate">Date for which to calculate the valuation</param>
        /// <param name="warehouseCode">Optional warehouse code to filter by</param>
        /// <returns>The inventory valuation report</returns>
        Task<InventoryValuationReportDto> GetInventoryValuationAsync(
            DateOnly asOfDate,
            string warehouseCode = null);
            
        /// <summary>
        /// Gets a list of items that need reordering (below reorder point)
        /// </summary>
        /// <param name="warehouseCode">Optional warehouse code to filter by</param>
        /// <returns>List of items that need reordering</returns>
        Task<IEnumerable<ReorderItemDto>> GetItemsToReorderAsync(string warehouseCode = null);
    }
}