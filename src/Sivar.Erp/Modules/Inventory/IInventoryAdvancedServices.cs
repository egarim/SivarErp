using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Sivar.Erp.Modules.Inventory
{
    // ==================== INVENTORY ANALYTICS SERVICE ====================
    /// <summary>
    /// Service for advanced inventory analytics and intelligence
    /// </summary>
    [Description("Advanced inventory analytics service")]
    public interface IInventoryAnalyticsService
    {
        /// <summary>
        /// Analyzes inventory turnover rates and patterns
        /// </summary>
        Task<InventoryTurnoverReport> GetTurnoverAnalysisAsync(TurnoverAnalysisParameters parameters);

        /// <summary>
        /// Performs ABC classification analysis on inventory
        /// </summary>
        Task<ABCClassificationReport> GetABCClassificationAsync(ABCClassificationParameters parameters);

        /// <summary>
        /// Identifies slow-moving inventory items
        /// </summary>
        Task<SlowMovingInventoryReport> GetSlowMovingAnalysisAsync(SlowMovingAnalysisParameters parameters);

        /// <summary>
        /// Analyzes inventory aging and obsolescence risk
        /// </summary>
        Task<InventoryAgingReport> GetAgingAnalysisAsync(AgingAnalysisParameters parameters);

        /// <summary>
        /// Provides demand forecasting and trend analysis
        /// </summary>
        Task<InventoryForecastReport> GetDemandForecastAsync(DemandForecastParameters parameters);

        /// <summary>
        /// Generates stock optimization recommendations
        /// </summary>
        Task<StockOptimizationReport> GetStockOptimizationAsync(StockOptimizationParameters parameters);

        /// <summary>
        /// Calculates comprehensive inventory performance metrics
        /// </summary>
        Task<InventoryPerformanceMetrics> GetPerformanceMetricsAsync(PerformanceMetricsParameters parameters);

        /// <summary>
        /// Provides real-time inventory dashboard data
        /// </summary>
        Task<InventoryDashboard> GetInventoryDashboardAsync(DashboardParameters parameters);
    }

    // ==================== INVENTORY TRANSFER SERVICE ====================
    /// <summary>
    /// Service for managing multi-location inventory transfers
    /// </summary>
    [Description("Multi-location inventory transfer service")]
    public interface IInventoryTransferService
    {
        /// <summary>
        /// Creates a new transfer request between warehouses/locations
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
        /// Gets current status of multiple transfers
        /// </summary>
        Task<List<TransferStatusUpdate>> GetTransferStatusAsync(List<string> transferIds);

        /// <summary>
        /// Retrieves transfer history with advanced filtering
        /// </summary>
        Task<List<TransferHistoryRecord>> GetTransferHistoryAsync(TransferHistoryQuery query);

        /// <summary>
        /// Gets transfers that require user action
        /// </summary>
        Task<List<PendingTransfer>> GetPendingTransfersAsync(PendingTransfersQuery query);

        /// <summary>
        /// Processes multiple transfers in a single operation
        /// </summary>
        Task<BulkTransferResult> ProcessBulkTransferAsync(BulkTransferRequest request);
    }

    // ==================== CYCLE COUNTING SERVICE ====================
    /// <summary>
    /// Service for automated cycle counting management
    /// </summary>
    [Description("Automated cycle counting service")]
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
        /// Processes discrepancies found during counting
        /// </summary>
        Task<DiscrepancyResolution> ProcessDiscrepancyAsync(string countId, DiscrepancyResolutionData resolution);

        /// <summary>
        /// Calculates cycle count accuracy metrics
        /// </summary>
        Task<CycleCountAccuracyMetrics> GetAccuracyMetricsAsync(AccuracyMetricsParameters parameters);

        /// <summary>
        /// Gets outstanding cycle counts requiring completion
        /// </summary>
        Task<List<OutstandingCycleCount>> GetOutstandingCountsAsync(OutstandingCountsQuery query);

        /// <summary>
        /// Generates variance analysis reports
        /// </summary>
        Task<CycleCountVarianceReport> GetVarianceReportAsync(VarianceReportParameters parameters);

        /// <summary>
        /// Automatically adjusts inventory based on count results
        /// </summary>
        Task<InventoryAdjustmentResult> AutoAdjustInventoryAsync(string countId, AutoAdjustmentRules rules);
    }

    // ==================== PARAMETER CLASSES ====================
    // Analytics Parameters
    public class TurnoverAnalysisParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? WarehouseCode { get; set; }
        public string? CategoryFilter { get; set; }
        public List<string> ItemCodes { get; set; } = new();
        public bool IncludeInactive { get; set; }
        public TurnoverCalculationMethod Method { get; set; }
    }

    public class ABCClassificationParameters
    {
        public DateTime AsOfDate { get; set; }
        public string? WarehouseCode { get; set; }
        public ABCCriteria ClassificationCriteria { get; set; }
        public decimal AClassPercentage { get; set; } = 80m;
        public decimal BClassPercentage { get; set; } = 15m;
        public decimal CClassPercentage { get; set; } = 5m;
        public bool IncludeZeroValue { get; set; }
    }

    public class SlowMovingAnalysisParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysWithoutMovement { get; set; } = 90;
        public string? WarehouseCode { get; set; }
        public decimal MinimumValue { get; set; }
        public bool IncludeObsolete { get; set; }
    }

    public class AgingAnalysisParameters
    {
        public DateTime AsOfDate { get; set; }
        public string? WarehouseCode { get; set; }
        public List<int> AgingBuckets { get; set; } = new() { 30, 60, 90, 180, 365 };
        public bool GroupByCategory { get; set; }
        public bool IncludeZeroQuantity { get; set; }
    }

    public class DemandForecastParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ForecastPeriodDays { get; set; } = 90;
        public List<string> ItemCodes { get; set; } = new();
        public ForecastMethod Method { get; set; }
        public decimal ConfidenceLevel { get; set; } = 95m;
        public bool IncludeSeasonality { get; set; }
    }

    public class StockOptimizationParameters
    {
        public string? WarehouseCode { get; set; }
        public string? CategoryFilter { get; set; }
        public decimal ServiceLevel { get; set; } = 95m;
        public int LeadTimeDays { get; set; } = 7;
        public decimal CarryingCostRate { get; set; } = 25m;
        public bool OptimizeByABC { get; set; }
        public List<string> ExcludedItems { get; set; } = new();
    }

    public class PerformanceMetricsParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? WarehouseCode { get; set; }
        public List<string> MetricTypes { get; set; } = new();
        public bool IncludeBenchmarks { get; set; }
        public string? ComparisonPeriod { get; set; }
    }

    public class DashboardParameters
    {
        public string? WarehouseCode { get; set; }
        public List<string> WidgetTypes { get; set; } = new();
        public DateTime? RefreshTime { get; set; }
        public bool IncludeAlerts { get; set; } = true;
        public string TimeZone { get; set; } = "UTC";
    }

    // Transfer Parameters
    public class CreateTransferRequestData
    {
        public string FromWarehouseCode { get; set; } = string.Empty;
        public string ToWarehouseCode { get; set; } = string.Empty;
        public List<TransferItem> Items { get; set; } = new();
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public TransferPriority Priority { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public bool RequireApproval { get; set; } = true;
    }

    public class TransferApprovalData
    {
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime ApprovalDate { get; set; }
        public bool IsApproved { get; set; }
        public string? ApprovalNotes { get; set; }
        public List<string>? ModifiedItems { get; set; }
    }

    public class TransferExecutionData
    {
        public string ExecutedBy { get; set; } = string.Empty;
        public DateTime ExecutionDate { get; set; }
        public List<TransferItemExecution> ItemExecutions { get; set; } = new();
        public string? ShippingReference { get; set; }
        public string? TrackingNumber { get; set; }
    }

    public class TransferCancellationData
    {
        public string CancelledBy { get; set; } = string.Empty;
        public DateTime CancellationDate { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
        public bool RefundShippingCosts { get; set; }
    }

    public class TransferHistoryQuery
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? FromWarehouse { get; set; }
        public string? ToWarehouse { get; set; }
        public List<TransferStatus> StatusFilter { get; set; } = new();
        public string? RequestedBy { get; set; }
        public int MaxResults { get; set; } = 100;
    }

    public class PendingTransfersQuery
    {
        public string? AssignedTo { get; set; }
        public string? WarehouseCode { get; set; }
        public TransferPriority? Priority { get; set; }
        public bool OverdueOnly { get; set; }
        public int MaxResults { get; set; } = 50;
    }

    public class BulkTransferRequest
    {
        public List<string> TransferIds { get; set; } = new();
        public BulkTransferOperation Operation { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
        public DateTime ProcessDate { get; set; }
        public Dictionary<string, object> OperationData { get; set; } = new();
    }

    // Cycle Counting Parameters
    public class CycleCountScheduleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public CycleCountMethod Method { get; set; }
        public CycleCountFrequency Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ItemsPerCycle { get; set; }
        public Dictionary<string, object> MethodParameters { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    public class CycleCountData
    {
        public string ItemCode { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public string? LocationCode { get; set; }
        public decimal SystemQuantity { get; set; }
        public decimal CountedQuantity { get; set; }
        public string CountedBy { get; set; } = string.Empty;
        public DateTime CountDate { get; set; }
        public string? SerialNumbers { get; set; }
        public string? BatchNumbers { get; set; }
        public string? CountNotes { get; set; }
    }

    public class DiscrepancyResolutionData
    {
        public string ResolutionType { get; set; } = string.Empty;
        public string ResolvedBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool ApproveAdjustment { get; set; }
        public string? AdditionalNotes { get; set; }
        public Dictionary<string, object> ResolutionDetails { get; set; } = new();
    }

    public class AccuracyMetricsParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? WarehouseCode { get; set; }
        public string? CounterId { get; set; }
        public bool GroupByPeriod { get; set; }
        public string GroupingPeriod { get; set; } = "Month";
    }

    public class OutstandingCountsQuery
    {
        public string? WarehouseCode { get; set; }
        public string? AssignedTo { get; set; }
        public bool OverdueOnly { get; set; }
        public CycleCountPriority? Priority { get; set; }
        public int MaxResults { get; set; } = 100;
    }

    public class VarianceReportParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? WarehouseCode { get; set; }
        public decimal MinimumVarianceAmount { get; set; }
        public decimal MinimumVariancePercentage { get; set; }
        public bool IncludeResolved { get; set; }
    }

    public class AutoAdjustmentRules
    {
        public decimal MaximumVariancePercent { get; set; } = 5m;
        public decimal MaximumVarianceValue { get; set; } = 100m;
        public bool RequireApproval { get; set; } = true;
        public List<string> ExcludedItemCodes { get; set; } = new();
        public List<string> ExcludedCategories { get; set; } = new();
        public bool AllowNegativeAdjustments { get; set; } = true;
    }

    // ==================== ENUMS ====================
    public enum TurnoverCalculationMethod
    {
        COGS,
        AverageCost,
        StandardCost,
        LastCost
    }

    public enum ABCCriteria
    {
        Value,
        Quantity,
        Usage,
        Combined
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
        Critical
    }

    public enum TransferStatus
    {
        Draft,
        Pending,
        Approved,
        InTransit,
        Received,
        Cancelled,
        Completed
    }

    public enum BulkTransferOperation
    {
        Approve,
        Cancel,
        Execute,
        Receive
    }

    public enum CycleCountMethod
    {
        ABC,
        Random,
        VelocityBased,
        ValueBased
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

    public enum CycleCountTaskStatus
    {
        Scheduled,
        Assigned,
        InProgress,
        Completed,
        Cancelled
    }

    public enum CycleCountResultStatus
    {
        Pending,
        Accurate,
        HasDiscrepancy,
        Resolved
    }

    public enum DiscrepancyResolutionStatus
    {
        Pending,
        InProgress,
        Approved,
        Resolved,
        Rejected
    }

    public enum AdjustmentStatus
    {
        Pending,
        Applied,
        Rejected,
        Cancelled
    }

    // ==================== SUPPORTING CLASSES ====================
    public class TransferItem
    {
        public string ItemCode { get; set; } = string.Empty;
        public decimal RequestedQuantity { get; set; }
        public decimal? AvailableQuantity { get; set; }
        public string? Notes { get; set; }
    }

    public class TransferItemExecution
    {
        public string ItemCode { get; set; } = string.Empty;
        public decimal ShippedQuantity { get; set; }
        public decimal? ReceivedQuantity { get; set; }
        public string? SerialNumbers { get; set; }
        public string? BatchNumbers { get; set; }
    }
}
