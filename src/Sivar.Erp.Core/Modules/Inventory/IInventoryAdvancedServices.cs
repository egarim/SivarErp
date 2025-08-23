using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Modules.Inventory.Reports;
using Sivar.Erp.Core.Modules.Inventory.Models;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Advanced inventory services interface
    /// </summary>
    public interface IInventoryAdvancedServices
    {
        /// <summary>
        /// Inventory analytics service
        /// </summary>
        IInventoryAnalyticsService AnalyticsService { get; }
        
        /// <summary>
        /// Multi-location transfer service
        /// </summary>
        IInventoryTransferService TransferService { get; }
        
        /// <summary>
        /// Cycle counting automation service
        /// </summary>
        ICycleCountingService CycleCountingService { get; }
    }

    /// <summary>
    /// Advanced inventory analytics service
    /// </summary>
    public interface IInventoryAnalyticsService
    {
        /// <summary>
        /// Gets inventory turnover analysis
        /// </summary>
        Task<InventoryTurnoverAnalysis> GetInventoryTurnoverAsync(InventoryAnalyticsParameters parameters);
        
        /// <summary>
        /// Gets ABC analysis for inventory items
        /// </summary>
        Task<ABCAnalysisReport> GetABCAnalysisAsync(ABCAnalysisParameters parameters);
        
        /// <summary>
        /// Gets slow-moving inventory report
        /// </summary>
        Task<SlowMovingInventoryReport> GetSlowMovingInventoryAsync(SlowMovingAnalysisParameters parameters);
        
        /// <summary>
        /// Gets inventory aging analysis
        /// </summary>
        Task<InventoryAgingReport> GetInventoryAgingAsync(InventoryAgingParameters parameters);
        
        /// <summary>
        /// Gets demand forecasting analysis
        /// </summary>
        Task<DemandForecastReport> GetDemandForecastAsync(DemandForecastParameters parameters);
        
        /// <summary>
        /// Gets stock optimization recommendations
        /// </summary>
        Task<StockOptimizationReport> GetStockOptimizationAsync(StockOptimizationParameters parameters);
        
        /// <summary>
        /// Gets inventory performance metrics
        /// </summary>
        Task<InventoryPerformanceMetrics> GetPerformanceMetricsAsync(PerformanceMetricsParameters parameters);
        
        /// <summary>
        /// Gets real-time inventory dashboard data
        /// </summary>
        Task<InventoryDashboardData> GetInventoryDashboardAsync(DashboardParameters parameters);
    }

    /// <summary>
    /// Multi-location inventory transfer service
    /// </summary>
    public interface IInventoryTransferService
    {
        /// <summary>
        /// Creates an inventory transfer request
        /// </summary>
        Task<InventoryTransferRequest> CreateTransferRequestAsync(TransferRequestData request);
        
        /// <summary>
        /// Approves an inventory transfer request
        /// </summary>
        Task<bool> ApproveTransferRequestAsync(string transferId, string approvedBy, string notes = null);
        
        /// <summary>
        /// Executes an approved inventory transfer
        /// </summary>
        Task<InventoryTransferExecution> ExecuteTransferAsync(string transferId, TransferExecutionData execution);
        
        /// <summary>
        /// Cancels a pending transfer request
        /// </summary>
        Task<bool> CancelTransferRequestAsync(string transferId, string cancelledBy, string reason);
        
        /// <summary>
        /// Gets transfer status and tracking information
        /// </summary>
        Task<InventoryTransferStatus> GetTransferStatusAsync(string transferId);
        
        /// <summary>
        /// Gets transfer history for an item or location
        /// </summary>
        Task<List<InventoryTransferHistory>> GetTransferHistoryAsync(TransferHistoryQuery query);
        
        /// <summary>
        /// Gets pending transfers requiring attention
        /// </summary>
        Task<List<PendingTransfer>> GetPendingTransfersAsync(PendingTransfersQuery query);
        
        /// <summary>
        /// Bulk transfer processing for multiple items
        /// </summary>
        Task<BulkTransferResult> ProcessBulkTransferAsync(BulkTransferRequest request);
    }

    /// <summary>
    /// Cycle counting automation service
    /// </summary>
    public interface ICycleCountingService
    {
        /// <summary>
        /// Creates a cycle count schedule
        /// </summary>
        Task<CycleCountSchedule> CreateCycleCountScheduleAsync(CycleCountScheduleRequest request);
        
        /// <summary>
        /// Generates cycle count tasks based on schedule
        /// </summary>
        Task<List<CycleCountTask>> GenerateCycleCountTasksAsync(string scheduleId, DateTime countDate);
        
        /// <summary>
        /// Records cycle count results
        /// </summary>
        Task<CycleCountResult> RecordCycleCountAsync(string taskId, CycleCountData countData);
        
        /// <summary>
        /// Processes cycle count discrepancies
        /// </summary>
        Task<DiscrepancyResolution> ProcessDiscrepancyAsync(string countId, DiscrepancyResolutionData resolution);
        
        /// <summary>
        /// Gets cycle count accuracy metrics
        /// </summary>
        Task<CycleCountAccuracyMetrics> GetAccuracyMetricsAsync(AccuracyMetricsParameters parameters);
        
        /// <summary>
        /// Gets outstanding cycle count tasks
        /// </summary>
        Task<List<OutstandingCycleCount>> GetOutstandingCountsAsync(OutstandingCountsQuery query);
        
        /// <summary>
        /// Generates cycle count variance report
        /// </summary>
        Task<CycleCountVarianceReport> GetVarianceReportAsync(VarianceReportParameters parameters);
        
        /// <summary>
        /// Auto-adjusts inventory based on cycle count results
        /// </summary>
        Task<InventoryAdjustmentResult> AutoAdjustInventoryAsync(string countId, AutoAdjustmentRules rules);
    }

    // Supporting model classes for parameters and results
    public class InventoryAnalyticsParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ItemCode { get; set; }
        public string? CategoryCode { get; set; }
        public string? WarehouseCode { get; set; }
        public string? BusinessEntityId { get; set; }
    }

    public class ABCAnalysisParameters : InventoryAnalyticsParameters
    {
        public ABCClassificationMethod Method { get; set; } = ABCClassificationMethod.Value;
        public decimal AThresholdPercent { get; set; } = 80m;
        public decimal BThresholdPercent { get; set; } = 15m;
        public int MinimumTransactions { get; set; } = 5;
    }

    public class SlowMovingAnalysisParameters : InventoryAnalyticsParameters
    {
        public int SlowMovingDays { get; set; } = 90;
        public decimal MinimumValue { get; set; } = 100m;
        public bool IncludeObsolete { get; set; } = true;
    }

    public class InventoryAgingParameters : InventoryAnalyticsParameters
    {
        public List<int> AgingBuckets { get; set; } = new List<int> { 30, 60, 90, 180, 365 };
        public InventoryValuationMethod ValuationMethod { get; set; } = InventoryValuationMethod.FIFO;
    }

    public class DemandForecastParameters : InventoryAnalyticsParameters
    {
        public int ForecastPeriodDays { get; set; } = 90;
        public ForecastMethod Method { get; set; } = ForecastMethod.MovingAverage;
        public int SeasonalityPeriods { get; set; } = 12;
        public decimal ConfidenceLevel { get; set; } = 95m;
    }

    public class StockOptimizationParameters : InventoryAnalyticsParameters
    {
        public decimal ServiceLevel { get; set; } = 95m;
        public int LeadTimeDays { get; set; } = 14;
        public decimal CarryingCostPercent { get; set; } = 25m;
        public decimal OrderingCost { get; set; } = 50m;
    }

    public class PerformanceMetricsParameters : InventoryAnalyticsParameters
    {
        public List<string> MetricTypes { get; set; } = new List<string>();
        public bool IncludeTrends { get; set; } = true;
        public int TrendPeriods { get; set; } = 12;
    }

    public class DashboardParameters
    {
        public string? UserId { get; set; }
        public string? WarehouseCode { get; set; }
        public List<string> WidgetTypes { get; set; } = new List<string>();
        public bool RealTimeData { get; set; } = true;
    }

    public class TransferRequestData
    {
        public string ItemCode { get; set; } = string.Empty;
        public string FromWarehouse { get; set; } = string.Empty;
        public string ToWarehouse { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public TransferPriority Priority { get; set; } = TransferPriority.Normal;
    }

    public class TransferExecutionData
    {
        public decimal ActualQuantity { get; set; }
        public string ExecutedBy { get; set; } = string.Empty;
        public DateTime ExecutionDate { get; set; }
        public string TransportMethod { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    public class TransferHistoryQuery
    {
        public string? ItemCode { get; set; }
        public string? WarehouseCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TransferStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class PendingTransfersQuery
    {
        public string? WarehouseCode { get; set; }
        public string? AssignedTo { get; set; }
        public TransferPriority? Priority { get; set; }
        public bool OverdueOnly { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class BulkTransferRequest
    {
        public List<TransferRequestData> Transfers { get; set; } = new();
        public string RequestedBy { get; set; } = string.Empty;
        public bool ValidateInventory { get; set; } = true;
        public bool ProcessImmediately { get; set; } = false;
    }

    public class CycleCountScheduleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CycleCountMethod Method { get; set; } = CycleCountMethod.ABC;
        public string WarehouseCode { get; set; } = string.Empty;
        public CycleCountFrequency Frequency { get; set; } = CycleCountFrequency.Monthly;
        public DateTime StartDate { get; set; }
        public List<CycleCountRule> Rules { get; set; } = new();
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class CycleCountData
    {
        public string ItemCode { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public decimal CountedQuantity { get; set; }
        public decimal SystemQuantity { get; set; }
        public string CountedBy { get; set; } = string.Empty;
        public DateTime CountDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<CycleCountObservation> Observations { get; set; } = new();
    }

    public class DiscrepancyResolutionData
    {
        public string ResolutionType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string ResolvedBy { get; set; } = string.Empty;
        public DateTime ResolutionDate { get; set; }
        public bool ApproveAdjustment { get; set; }
        public string AdditionalNotes { get; set; } = string.Empty;
    }

    public class AccuracyMetricsParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? WarehouseCode { get; set; }
        public string? CountedBy { get; set; }
        public CycleCountMethod? Method { get; set; }
    }

    public class OutstandingCountsQuery
    {
        public string? WarehouseCode { get; set; }
        public string? AssignedTo { get; set; }
        public bool OverdueOnly { get; set; } = false;
        public CycleCountPriority? Priority { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class VarianceReportParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? WarehouseCode { get; set; }
        public decimal? MinimumVariancePercent { get; set; }
        public decimal? MinimumVarianceValue { get; set; }
        public bool IncludeResolved { get; set; } = true;
    }

    public class AutoAdjustmentRules
    {
        public decimal MaximumVariancePercent { get; set; } = 5m;
        public decimal MaximumVarianceValue { get; set; } = 100m;
        public bool RequireApproval { get; set; } = true;
        public List<string> ExcludedItemCodes { get; set; } = new();
    }

    // Enums for advanced inventory functionality
    public enum ABCClassificationMethod
    {
        Value,
        Quantity,
        Frequency,
        Margin
    }

    public enum ForecastMethod
    {
        MovingAverage,
        ExponentialSmoothing,
        LinearRegression,
        SeasonalDecomposition
    }

    public enum TransferPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }

    public enum TransferStatus
    {
        Pending,
        Approved,
        InTransit,
        Completed,
        Cancelled,
        Failed
    }

    public enum CycleCountMethod
    {
        ABC,
        Random,
        VelocityBased,
        ValueBased,
        Manual
    }

    public enum CycleCountFrequency
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Annually
    }

    public enum CycleCountPriority
    {
        Low,
        Normal,
        High,
        Critical
    }
}
