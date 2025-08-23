using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.Modules.Inventory.Reports;
using Sivar.Erp.Core.Modules.Inventory;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Main interface for the comprehensive inventory module
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

        // Advanced Services (NEW)
        /// <summary>
        /// Advanced inventory analytics service
        /// </summary>
        IInventoryAnalyticsService AnalyticsService { get; }

        /// <summary>
        /// Multi-location inventory transfer service
        /// </summary>
        IInventoryTransferService TransferService { get; }

        /// <summary>
        /// Automated cycle counting service
        /// </summary>
        ICycleCountingService CycleCountingService { get; }
        
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

        // Advanced Analytics Operations (NEW)
        /// <summary>
        /// Gets comprehensive inventory turnover analysis
        /// </summary>
        Task<InventoryTurnoverReport> GetTurnoverAnalysisAsync(TurnoverAnalysisParameters parameters);

        /// <summary>
        /// Gets ABC classification analysis
        /// </summary>
        Task<ABCClassificationReport> GetABCClassificationAsync(ABCClassificationParameters parameters);

        /// <summary>
        /// Gets slow-moving inventory analysis
        /// </summary>
        Task<SlowMovingInventoryReport> GetSlowMovingAnalysisAsync(SlowMovingAnalysisParameters parameters);

        /// <summary>
        /// Gets inventory aging analysis
        /// </summary>
        Task<InventoryAgingReport> GetAgingAnalysisAsync(AgingAnalysisParameters parameters);

        /// <summary>
        /// Gets demand forecasting analysis
        /// </summary>
        Task<InventoryForecastReport> GetDemandForecastAsync(DemandForecastParameters parameters);

        /// <summary>
        /// Gets stock optimization recommendations
        /// </summary>
        Task<StockOptimizationReport> GetStockOptimizationAsync(StockOptimizationParameters parameters);

        /// <summary>
        /// Gets comprehensive performance metrics
        /// </summary>
        Task<InventoryPerformanceMetrics> GetPerformanceMetricsAsync(PerformanceMetricsParameters parameters);

        /// <summary>
        /// Gets real-time inventory dashboard
        /// </summary>
        Task<InventoryDashboard> GetInventoryDashboardAsync(DashboardParameters parameters);

        // Multi-Location Transfer Operations (NEW)
        /// <summary>
        /// Creates a new transfer request between locations
        /// </summary>
        Task<TransferRequest> CreateTransferRequestAsync(CreateTransferRequestData request);

        /// <summary>
        /// Approves a pending transfer request
        /// </summary>
        Task<TransferApprovalResult> ApproveTransferAsync(string transferId, TransferApprovalData approval);

        /// <summary>
        /// Executes an approved transfer
        /// </summary>
        Task<TransferExecutionResult> ExecuteTransferAsync(string transferId, TransferExecutionData execution);

        /// <summary>
        /// Cancels a pending transfer
        /// </summary>
        Task<TransferCancellationResult> CancelTransferAsync(string transferId, TransferCancellationData cancellation);

        /// <summary>
        /// Gets transfer status for multiple transfers
        /// </summary>
        Task<List<TransferStatusUpdate>> GetTransferStatusAsync(List<string> transferIds);

        /// <summary>
        /// Gets transfer history with filtering options
        /// </summary>
        Task<List<TransferHistoryRecord>> GetTransferHistoryAsync(TransferHistoryQuery query);

        /// <summary>
        /// Gets pending transfers requiring action
        /// </summary>
        Task<List<PendingTransfer>> GetPendingTransfersAsync(PendingTransfersQuery query);

        /// <summary>
        /// Processes multiple transfers in bulk
        /// </summary>
        Task<BulkTransferResult> ProcessBulkTransferAsync(BulkTransferRequest request);

        // Cycle Counting Operations (NEW)
        /// <summary>
        /// Creates a new cycle count schedule
        /// </summary>
        Task<CycleCountSchedule> CreateCycleCountScheduleAsync(CycleCountScheduleRequest request);

        /// <summary>
        /// Generates cycle count tasks for a schedule
        /// </summary>
        Task<List<CycleCountTask>> GenerateCycleCountTasksAsync(string scheduleId, DateTime countDate);

        /// <summary>
        /// Records the results of a cycle count
        /// </summary>
        Task<CycleCountResult> RecordCycleCountAsync(string taskId, CycleCountData countData);

        /// <summary>
        /// Processes discrepancies found during counting
        /// </summary>
        Task<DiscrepancyResolution> ProcessDiscrepancyAsync(string countId, DiscrepancyResolutionData resolution);

        /// <summary>
        /// Gets cycle count accuracy metrics
        /// </summary>
        Task<CycleCountAccuracyMetrics> GetAccuracyMetricsAsync(AccuracyMetricsParameters parameters);

        /// <summary>
        /// Gets outstanding cycle counts requiring completion
        /// </summary>
        Task<List<OutstandingCycleCount>> GetOutstandingCountsAsync(OutstandingCountsQuery query);

        /// <summary>
        /// Gets variance analysis report
        /// </summary>
        Task<CycleCountVarianceReport> GetVarianceReportAsync(VarianceReportParameters parameters);

        /// <summary>
        /// Automatically adjusts inventory based on count results
        /// </summary>
        Task<InventoryAdjustmentResult> AutoAdjustInventoryAsync(string countId, AutoAdjustmentRules rules);
    }
}