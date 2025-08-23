using System.ComponentModel;
using Sivar.Erp.Core.Modules.Inventory.Models;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Service interface for cycle counting operations
    /// </summary>
    [Description("Service interface for cycle counting operations")]
    public interface ICycleCountingService
    {
        /// <summary>
        /// Initiates a cycle count
        /// </summary>
        /// <param name="data">Cycle count data</param>
        /// <returns>Task representing the operation</returns>
        [Description("Initiates a cycle count")]
        Task<CycleCountResult> InitiateCycleCountAsync(CycleCountData data);

        /// <summary>
        /// Updates cycle count progress
        /// </summary>
        /// <param name="countId">Count identifier</param>
        /// <param name="progress">Progress data</param>
        /// <returns>Task representing the operation</returns>
        [Description("Updates cycle count progress")]
        Task UpdateCycleCountProgressAsync(Guid countId, CycleCountProgress progress);

        /// <summary>
        /// Completes a cycle count
        /// </summary>
        /// <param name="countId">Count identifier</param>
        /// <param name="completionData">Completion data</param>
        /// <returns>Task representing the operation</returns>
        [Description("Completes a cycle count")]
        Task<CycleCountResult> CompleteCycleCountAsync(Guid countId, CycleCountCompletionData completionData);

        /// <summary>
        /// Gets cycle count history
        /// </summary>
        /// <param name="locationId">Location identifier</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Cycle count history</returns>
        [Description("Gets cycle count history")]
        Task<List<CycleCountHistory>> GetCycleCountHistoryAsync(Guid? locationId, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Resolves cycle count discrepancies
        /// </summary>
        /// <param name="countId">Count identifier</param>
        /// <param name="resolutionData">Resolution data</param>
        /// <returns>Task representing the operation</returns>
        [Description("Resolves cycle count discrepancies")]
        Task ResolveCycleCountDiscrepanciesAsync(Guid countId, DiscrepancyResolutionData resolutionData);

        /// <summary>
        /// Gets cycle count accuracy metrics
        /// </summary>
        /// <param name="parameters">Metrics parameters</param>
        /// <returns>Accuracy metrics</returns>
        [Description("Gets cycle count accuracy metrics")]
        Task<AccuracyMetrics> GetAccuracyMetricsAsync(AccuracyMetricsParameters parameters);
    }

    /// <summary>
    /// Service interface for inventory analytics
    /// </summary>
    [Description("Service interface for inventory analytics")]
    public interface IInventoryAnalyticsService
    {
        /// <summary>
        /// Performs ABC analysis
        /// </summary>
        /// <param name="parameters">Analysis parameters</param>
        /// <returns>ABC analysis result</returns>
        [Description("Performs ABC analysis")]
        Task<ABCAnalysisResult> PerformABCAnalysisAsync(ABCAnalysisParameters parameters);

        /// <summary>
        /// Analyzes slow moving inventory
        /// </summary>
        /// <param name="parameters">Analysis parameters</param>
        /// <returns>Slow moving analysis result</returns>
        [Description("Analyzes slow moving inventory")]
        Task<SlowMovingAnalysisResult> AnalyzeSlowMovingInventoryAsync(SlowMovingAnalysisParameters parameters);

        /// <summary>
        /// Gets inventory analytics dashboard data
        /// </summary>
        /// <param name="parameters">Dashboard parameters</param>
        /// <returns>Dashboard data</returns>
        [Description("Gets inventory analytics dashboard data")]
        Task<InventoryDashboardData> GetDashboardDataAsync(DashboardParameters parameters);

        /// <summary>
        /// Generates inventory reports
        /// </summary>
        /// <param name="reportType">Report type</param>
        /// <param name="parameters">Report parameters</param>
        /// <returns>Report data</returns>
        [Description("Generates inventory reports")]
        Task<InventoryReportData> GenerateReportAsync(string reportType, InventoryAnalyticsParameters parameters);

        /// <summary>
        /// Gets performance metrics
        /// </summary>
        /// <param name="parameters">Metrics parameters</param>
        /// <returns>Performance metrics</returns>
        [Description("Gets performance metrics")]
        Task<InventoryPerformanceMetrics> GetPerformanceMetricsAsync(PerformanceMetricsParameters parameters);
    }

    /// <summary>
    /// Service interface for inventory transfer operations
    /// </summary>
    [Description("Service interface for inventory transfer operations")]
    public interface IInventoryTransferService
    {
        /// <summary>
        /// Creates a transfer request
        /// </summary>
        /// <param name="data">Transfer request data</param>
        /// <returns>Transfer request result</returns>
        [Description("Creates a transfer request")]
        Task<TransferRequestResult> CreateTransferRequestAsync(TransferRequestData data);

        /// <summary>
        /// Executes a transfer
        /// </summary>
        /// <param name="transferId">Transfer identifier</param>
        /// <param name="executionData">Execution data</param>
        /// <returns>Transfer execution result</returns>
        [Description("Executes a transfer")]
        Task<TransferExecutionResult> ExecuteTransferAsync(Guid transferId, TransferExecutionData executionData);

        /// <summary>
        /// Gets transfer history
        /// </summary>
        /// <param name="query">History query</param>
        /// <returns>Transfer history</returns>
        [Description("Gets transfer history")]
        Task<List<TransferHistoryRecord>> GetTransferHistoryAsync(TransferHistoryQuery query);

        /// <summary>
        /// Gets pending transfers
        /// </summary>
        /// <param name="query">Pending transfers query</param>
        /// <returns>Pending transfers</returns>
        [Description("Gets pending transfers")]
        Task<List<PendingTransfer>> GetPendingTransfersAsync(PendingTransfersQuery query);

        /// <summary>
        /// Performs bulk transfer
        /// </summary>
        /// <param name="request">Bulk transfer request</param>
        /// <returns>Bulk transfer result</returns>
        [Description("Performs bulk transfer")]
        Task<BulkTransferResult> PerformBulkTransferAsync(BulkTransferRequest request);

        /// <summary>
        /// Cancels a transfer
        /// </summary>
        /// <param name="transferId">Transfer identifier</param>
        /// <param name="reason">Cancellation reason</param>
        /// <returns>Task representing the operation</returns>
        [Description("Cancels a transfer")]
        Task CancelTransferAsync(Guid transferId, string reason);
    }

    // Supporting classes for the interfaces
    public class CycleCountResult
    {
        public Guid CountId { get; set; }
        public bool IsSuccessful { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class CycleCountProgress
    {
        public int ItemsCompleted { get; set; }
        public int TotalItems { get; set; }
        public decimal PercentageComplete { get; set; }
        public List<CycleCountItem> CountedItems { get; set; } = new();
    }

    public class CycleCountCompletionData
    {
        public string CompletedBy { get; set; } = string.Empty;
        public DateTime CompletionDate { get; set; }
        public string? Notes { get; set; }
        public bool AutoResolveDiscrepancies { get; set; }
    }

    public class CycleCountHistory
    {
        public Guid CountId { get; set; }
        public DateTime CountDate { get; set; }
        public Guid LocationId { get; set; }
        public string CountType { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
        public int DiscrepanciesFound { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CycleCountItem
    {
        public Guid ItemId { get; set; }
        public decimal ExpectedQuantity { get; set; }
        public decimal CountedQuantity { get; set; }
        public decimal Variance { get; set; }
        public string? Notes { get; set; }
    }

    public class AccuracyMetrics
    {
        public decimal AccuracyPercentage { get; set; }
        public int TotalItemsCounted { get; set; }
        public int ItemsWithDiscrepancies { get; set; }
        public decimal AverageVariance { get; set; }
        public Dictionary<string, object> DetailedMetrics { get; set; } = new();
    }

    public class ABCAnalysisResult
    {
        public List<ABCClassificationItem> Items { get; set; } = new();
        public ABCAnalysisSummary Summary { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    public class ABCClassificationItem
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public ABCClassification Classification { get; set; }
        public decimal Value { get; set; }
        public decimal CumulativePercentage { get; set; }
    }

    public class ABCAnalysisSummary
    {
        public int ClassACount { get; set; }
        public int ClassBCount { get; set; }
        public int ClassCCount { get; set; }
        public decimal ClassAValue { get; set; }
        public decimal ClassBValue { get; set; }
        public decimal ClassCValue { get; set; }
    }

    public class SlowMovingAnalysisResult
    {
        public List<SlowMovingItem> SlowMovingItems { get; set; } = new();
        public SlowMovingAnalysisSummary Summary { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    public class SlowMovingItem
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal CurrentQuantity { get; set; }
        public DateTime LastMovementDate { get; set; }
        public int DaysSinceLastMovement { get; set; }
        public decimal EstimatedValue { get; set; }
    }

    public class SlowMovingAnalysisSummary
    {
        public int TotalSlowMovingItems { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AverageDaysWithoutMovement { get; set; }
    }

    public class InventoryDashboardData
    {
        public InventoryOverview Overview { get; set; } = new();
        public List<TopMovingItem> TopMovingItems { get; set; } = new();
        public List<LowStockAlert> LowStockAlerts { get; set; } = new();
        public InventoryTrends Trends { get; set; } = new();
    }

    public class InventoryOverview
    {
        public decimal TotalValue { get; set; }
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
    }

    public class TopMovingItem
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal MovementQuantity { get; set; }
        public decimal MovementValue { get; set; }
    }

    public class LowStockAlert
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal CurrentQuantity { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal ShortageQuantity { get; set; }
    }

    public class InventoryTrends
    {
        public List<TrendData> ValueTrends { get; set; } = new();
        public List<TrendData> MovementTrends { get; set; } = new();
        public List<TrendData> AccuracyTrends { get; set; } = new();
    }

    public class TrendData
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class InventoryReportData
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public object Data { get; set; } = new();
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class InventoryPerformanceMetrics
    {
        public decimal TurnoverRate { get; set; }
        public decimal AccuracyRate { get; set; }
        public decimal FulfillmentRate { get; set; }
        public TimeSpan AverageProcessingTime { get; set; }
        public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
    }

    public class TransferRequestResult
    {
        public Guid TransferRequestId { get; set; }
        public bool IsSuccessful { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class TransferExecutionResult
    {
        public Guid TransferId { get; set; }
        public bool IsSuccessful { get; set; }
        public string? Message { get; set; }
        public List<TransferExecutionItem> ProcessedItems { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class TransferHistoryRecord
    {
        public Guid TransferId { get; set; }
        public DateTime TransferDate { get; set; }
        public Guid SourceLocationId { get; set; }
        public Guid DestinationLocationId { get; set; }
        public TransferStatus Status { get; set; }
        public int ItemCount { get; set; }
        public string? Notes { get; set; }
    }

    public class PendingTransfer
    {
        public Guid TransferId { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid SourceLocationId { get; set; }
        public Guid DestinationLocationId { get; set; }
        public TransferPriority Priority { get; set; }
        public int ItemCount { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
    }

    public class BulkTransferResult
    {
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public List<Guid> CreatedTransferIds { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
