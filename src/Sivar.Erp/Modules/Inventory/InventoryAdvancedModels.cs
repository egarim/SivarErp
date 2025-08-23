using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sivar.Erp.Modules.Inventory
{
    // ==================== INVENTORY ANALYTICS MODELS ====================
    /// <summary>
    /// Comprehensive inventory turnover analysis report
    /// </summary>
    [Description("Inventory turnover analysis report")]
    public class InventoryTurnoverReport
    {
        public DateTime ReportDate { get; set; }
        public TurnoverAnalysisParameters Parameters { get; set; } = new();
        public TurnoverSummary Summary { get; set; } = new();
        public List<ItemTurnoverDetail> ItemDetails { get; set; } = new();
        public List<CategoryTurnoverSummary> CategorySummaries { get; set; } = new();
        public TurnoverTrends Trends { get; set; } = new();
        public List<TurnoverAlert> Alerts { get; set; } = new();
    }

    public class TurnoverSummary
    {
        public decimal OverallTurnoverRate { get; set; }
        public decimal AverageTurnoverRate { get; set; }
        public decimal MedianTurnoverRate { get; set; }
        public int TotalItemsAnalyzed { get; set; }
        public int FastMovingItems { get; set; }
        public int SlowMovingItems { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public decimal TotalCOGS { get; set; }
    }

    public class ItemTurnoverDetail
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal TurnoverRate { get; set; }
        public decimal AverageInventoryValue { get; set; }
        public decimal COGS { get; set; }
        public int DaysOnHand { get; set; }
        public TurnoverRating Rating { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    public class CategoryTurnoverSummary
    {
        public string Category { get; set; } = string.Empty;
        public decimal AverageTurnoverRate { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
        public TurnoverRating OverallRating { get; set; }
    }

    public class TurnoverTrends
    {
        public List<TurnoverTrendPoint> MonthlyTrends { get; set; } = new();
        public string TrendDirection { get; set; } = string.Empty;
        public decimal TrendPercentage { get; set; }
        public bool IsImproving { get; set; }
    }

    public class TurnoverTrendPoint
    {
        public DateTime Period { get; set; }
        public decimal TurnoverRate { get; set; }
        public decimal InventoryValue { get; set; }
    }

    public class TurnoverAlert
    {
        public string AlertType { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime AlertDate { get; set; }
    }

    /// <summary>
    /// ABC classification analysis report
    /// </summary>
    [Description("ABC classification analysis report")]
    public class ABCClassificationReport
    {
        public DateTime ReportDate { get; set; }
        public ABCClassificationParameters Parameters { get; set; } = new();
        public ABCSummary Summary { get; set; } = new();
        public List<ABCItemDetail> ItemClassifications { get; set; } = new();
        public ABCDistribution Distribution { get; set; } = new();
        public List<ABCRecommendation> Recommendations { get; set; } = new();
    }

    public class ABCSummary
    {
        public int TotalItems { get; set; }
        public int AClassItems { get; set; }
        public int BClassItems { get; set; }
        public int CClassItems { get; set; }
        public decimal AClassValue { get; set; }
        public decimal BClassValue { get; set; }
        public decimal CClassValue { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class ABCItemDetail
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ABCClass { get; set; } = string.Empty;
        public decimal AnnualUsage { get; set; }
        public decimal UnitCost { get; set; }
        public decimal AnnualValue { get; set; }
        public decimal CumulativePercentage { get; set; }
        public string ControlStrategy { get; set; } = string.Empty;
    }

    public class ABCDistribution
    {
        public decimal AClassPercentage { get; set; }
        public decimal BClassPercentage { get; set; }
        public decimal CClassPercentage { get; set; }
        public bool IsOptimalDistribution { get; set; }
        public string DistributionAnalysis { get; set; } = string.Empty;
    }

    public class ABCRecommendation
    {
        public string ABCClass { get; set; } = string.Empty;
        public string RecommendationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public List<string> AffectedItems { get; set; } = new();
    }

    /// <summary>
    /// Slow-moving inventory analysis report
    /// </summary>
    [Description("Slow-moving inventory analysis report")]
    public class SlowMovingInventoryReport
    {
        public DateTime ReportDate { get; set; }
        public SlowMovingAnalysisParameters Parameters { get; set; } = new();
        public SlowMovingSummary Summary { get; set; } = new();
        public List<SlowMovingItem> SlowMovingItems { get; set; } = new();
        public List<ObsoleteItem> ObsoleteItems { get; set; } = new();
        public SlowMovingTrends Trends { get; set; } = new();
        public List<ActionRecommendation> ActionRecommendations { get; set; } = new();
    }

    public class SlowMovingSummary
    {
        public int TotalSlowMovingItems { get; set; }
        public decimal TotalSlowMovingValue { get; set; }
        public decimal PercentageOfTotalInventory { get; set; }
        public int ObsoleteItems { get; set; }
        public decimal ObsoleteValue { get; set; }
        public decimal EstimatedCarryingCost { get; set; }
    }

    public class SlowMovingItem
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal OnHandQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime LastMovementDate { get; set; }
        public int DaysSinceLastMovement { get; set; }
        public decimal AverageMonthlyUsage { get; set; }
        public int MonthsOfSupply { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class ObsoleteItem
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal OnHandQuantity { get; set; }
        public decimal BookValue { get; set; }
        public decimal EstimatedRecoveryValue { get; set; }
        public decimal EstimatedLoss { get; set; }
        public string ObsolescenceReason { get; set; } = string.Empty;
        public DateTime IdentifiedDate { get; set; }
    }

    public class SlowMovingTrends
    {
        public List<SlowMovingTrendPoint> MonthlyTrends { get; set; } = new();
        public string TrendDirection { get; set; } = string.Empty;
        public bool IsWorsening { get; set; }
    }

    public class SlowMovingTrendPoint
    {
        public DateTime Period { get; set; }
        public int SlowMovingItemCount { get; set; }
        public decimal SlowMovingValue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ActionRecommendation
    {
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public List<string> AffectedItems { get; set; } = new();
        public decimal EstimatedImpact { get; set; }
        public string Timeline { get; set; } = string.Empty;
    }

    /// <summary>
    /// Inventory aging analysis report
    /// </summary>
    [Description("Inventory aging analysis report")]
    public class InventoryAgingReport
    {
        public DateTime ReportDate { get; set; }
        public AgingAnalysisParameters Parameters { get; set; } = new();
        public AgingSummary Summary { get; set; } = new();
        public List<AgingBucketSummary> AgingBuckets { get; set; } = new();
        public List<ItemAgingDetail> ItemDetails { get; set; } = new();
        public List<CategoryAgingSummary> CategorySummaries { get; set; } = new();
    }

    public class AgingSummary
    {
        public decimal TotalInventoryValue { get; set; }
        public int TotalItems { get; set; }
        public decimal AverageAge { get; set; }
        public decimal OldestItemAge { get; set; }
        public string OldestItemCode { get; set; } = string.Empty;
        public decimal AtRiskValue { get; set; }
        public int AtRiskItems { get; set; }
    }

    public class AgingBucketSummary
    {
        public string BucketName { get; set; } = string.Empty;
        public int MinDays { get; set; }
        public int MaxDays { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }

    public class ItemAgingDetail
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime ReceiptDate { get; set; }
        public int AgeInDays { get; set; }
        public string AgingBucket { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
    }

    public class CategoryAgingSummary
    {
        public string Category { get; set; } = string.Empty;
        public decimal AverageAge { get; set; }
        public decimal TotalValue { get; set; }
        public int ItemCount { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }

    /// <summary>
    /// Inventory demand forecast report
    /// </summary>
    [Description("Inventory demand forecast report")]
    public class InventoryForecastReport
    {
        public DateTime ReportDate { get; set; }
        public DemandForecastParameters Parameters { get; set; } = new();
        public ForecastSummary Summary { get; set; } = new();
        public List<ItemForecast> ItemForecasts { get; set; } = new();
        public ForecastAccuracy AccuracyMetrics { get; set; } = new();
        public List<ForecastAlert> Alerts { get; set; } = new();
    }

    public class ForecastSummary
    {
        public int TotalItemsForecasted { get; set; }
        public decimal TotalForecastDemand { get; set; }
        public decimal ForecastAccuracy { get; set; }
        public string ForecastMethod { get; set; } = string.Empty;
        public decimal ConfidenceLevel { get; set; }
        public DateTime ForecastPeriodStart { get; set; }
        public DateTime ForecastPeriodEnd { get; set; }
    }

    public class ItemForecast
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<ForecastPeriod> ForecastPeriods { get; set; } = new();
        public decimal TotalForecastDemand { get; set; }
        public decimal ForecastAccuracy { get; set; }
        public string TrendIndicator { get; set; } = string.Empty;
        public bool HasSeasonality { get; set; }
        public decimal ConfidenceInterval { get; set; }
    }

    public class ForecastPeriod
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal ForecastDemand { get; set; }
        public decimal UpperBound { get; set; }
        public decimal LowerBound { get; set; }
        public decimal? ActualDemand { get; set; }
    }

    public class ForecastAccuracy
    {
        public decimal MeanAbsoluteError { get; set; }
        public decimal MeanAbsolutePercentageError { get; set; }
        public decimal RootMeanSquareError { get; set; }
        public decimal TrackingSignal { get; set; }
        public string AccuracyRating { get; set; } = string.Empty;
    }

    public class ForecastAlert
    {
        public string AlertType { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime AlertDate { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Stock optimization recommendations report
    /// </summary>
    [Description("Stock optimization recommendations report")]
    public class StockOptimizationReport
    {
        public DateTime ReportDate { get; set; }
        public StockOptimizationParameters Parameters { get; set; } = new();
        public OptimizationSummary Summary { get; set; } = new();
        public List<ItemOptimization> ItemRecommendations { get; set; } = new();
        public List<OptimizationOpportunity> Opportunities { get; set; } = new();
        public OptimizationMetrics Metrics { get; set; } = new();
    }

    public class OptimizationSummary
    {
        public int TotalItemsAnalyzed { get; set; }
        public int ItemsWithRecommendations { get; set; }
        public decimal CurrentInventoryValue { get; set; }
        public decimal OptimizedInventoryValue { get; set; }
        public decimal PotentialSavings { get; set; }
        public decimal ServiceLevelImpact { get; set; }
        public string OverallRecommendation { get; set; } = string.Empty;
    }

    public class ItemOptimization
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal CurrentStockLevel { get; set; }
        public decimal RecommendedStockLevel { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal SafetyStock { get; set; }
        public decimal EconomicOrderQuantity { get; set; }
        public string OptimizationType { get; set; } = string.Empty;
        public decimal PotentialSavings { get; set; }
        public string Justification { get; set; } = string.Empty;
    }

    public class OptimizationOpportunity
    {
        public string OpportunityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedSavings { get; set; }
        public decimal ImplementationCost { get; set; }
        public decimal ROI { get; set; }
        public string Priority { get; set; } = string.Empty;
        public List<string> AffectedItems { get; set; } = new();
        public string Timeline { get; set; } = string.Empty;
    }

    public class OptimizationMetrics
    {
        public decimal InventoryTurnover { get; set; }
        public decimal TargetTurnover { get; set; }
        public decimal ServiceLevel { get; set; }
        public decimal StockoutRisk { get; set; }
        public decimal CarryingCost { get; set; }
        public decimal OrderingCost { get; set; }
    }

    /// <summary>
    /// Inventory performance metrics
    /// </summary>
    [Description("Inventory performance metrics")]
    public class InventoryPerformanceMetrics
    {
        public DateTime ReportDate { get; set; }
        public PerformanceMetricsParameters Parameters { get; set; } = new();
        public PerformanceSummary Summary { get; set; } = new();
        public List<KPIMetric> KPIs { get; set; } = new();
        public List<PerformanceTrend> Trends { get; set; } = new();
        public List<BenchmarkComparison> Benchmarks { get; set; } = new();
        public List<PerformanceAlert> Alerts { get; set; } = new();
    }

    public class PerformanceSummary
    {
        public decimal OverallScore { get; set; }
        public string PerformanceGrade { get; set; } = string.Empty;
        public int MetricsImproved { get; set; }
        public int MetricsDeclined { get; set; }
        public int MetricsStable { get; set; }
        public string TopPerformingArea { get; set; } = string.Empty;
        public string AreaNeedingImprovement { get; set; } = string.Empty;
    }

    public class KPIMetric
    {
        public string MetricName { get; set; } = string.Empty;
        public string MetricCategory { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal PreviousValue { get; set; }
        public decimal TargetValue { get; set; }
        public decimal VarianceFromTarget { get; set; }
        public string PerformanceIndicator { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class PerformanceTrend
    {
        public string MetricName { get; set; } = string.Empty;
        public List<TrendDataPoint> DataPoints { get; set; } = new();
        public string TrendDirection { get; set; } = string.Empty;
        public decimal TrendSlope { get; set; }
        public bool IsStatisticallySignificant { get; set; }
    }

    public class TrendDataPoint
    {
        public DateTime Period { get; set; }
        public decimal Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class BenchmarkComparison
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal YourValue { get; set; }
        public decimal IndustryAverage { get; set; }
        public decimal BestInClass { get; set; }
        public decimal PercentileRank { get; set; }
        public string ComparisonRating { get; set; } = string.Empty;
    }

    public class PerformanceAlert
    {
        public string AlertType { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime AlertDate { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Real-time inventory dashboard
    /// </summary>
    [Description("Real-time inventory dashboard")]
    public class InventoryDashboard
    {
        public DateTime LastUpdated { get; set; }
        public DashboardParameters Parameters { get; set; } = new();
        public List<DashboardWidget> Widgets { get; set; } = new();
        public List<DashboardAlert> Alerts { get; set; } = new();
        public DashboardSummary Summary { get; set; } = new();
    }

    public class DashboardWidget
    {
        public string WidgetId { get; set; } = string.Empty;
        public string WidgetType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public string ChartType { get; set; } = string.Empty;
        public DateTime LastRefresh { get; set; }
        public bool IsRealTime { get; set; }
    }

    public class DashboardAlert
    {
        public string AlertId { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime AlertTime { get; set; }
        public bool RequiresAction { get; set; }
        public string ActionUrl { get; set; } = string.Empty;
    }

    public class DashboardSummary
    {
        public decimal TotalInventoryValue { get; set; }
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public int OverstockItems { get; set; }
        public decimal DailyMovementValue { get; set; }
        public int ActiveAlerts { get; set; }
        public decimal TurnoverRate { get; set; }
    }

    // ==================== TRANSFER MODELS ====================
    /// <summary>
    /// Transfer request entity
    /// </summary>
    [Description("Transfer request entity")]
    public class TransferRequest
    {
        public string TransferId { get; set; } = string.Empty;
        public CreateTransferRequestData RequestData { get; set; } = new();
        public TransferStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RejectionReason { get; set; }
        public List<TransferStatusHistory> StatusHistory { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class TransferStatusHistory
    {
        public TransferStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class TransferApprovalResult
    {
        public string TransferId { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public TransferApprovalData ApprovalData { get; set; } = new();
        public string? ValidationMessage { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    public class TransferExecutionResult
    {
        public string TransferId { get; set; } = string.Empty;
        public TransferExecutionData ExecutionData { get; set; } = new();
        public bool IsSuccessful { get; set; }
        public List<string> ProcessedItems { get; set; } = new();
        public List<string> FailedItems { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public DateTime ExecutionTime { get; set; }
    }

    public class TransferCancellationResult
    {
        public string TransferId { get; set; } = string.Empty;
        public TransferCancellationData CancellationData { get; set; } = new();
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CancellationTime { get; set; }
    }

    public class TransferStatusUpdate
    {
        public string TransferId { get; set; } = string.Empty;
        public TransferStatus CurrentStatus { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? LastUpdatedBy { get; set; }
        public decimal CompletionPercentage { get; set; }
        public string? StatusDescription { get; set; }
        public List<string> RecentActivities { get; set; } = new();
    }

    public class TransferHistoryRecord
    {
        public string TransferId { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string FromWarehouse { get; set; } = string.Empty;
        public string ToWarehouse { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public TransferStatus Status { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Notes { get; set; }
    }

    public class PendingTransfer
    {
        public string TransferId { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string FromWarehouse { get; set; } = string.Empty;
        public string ToWarehouse { get; set; } = string.Empty;
        public TransferPriority Priority { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
        public string? AssignedTo { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysPending { get; set; }
        public string ActionRequired { get; set; } = string.Empty;
    }

    public class BulkTransferResult
    {
        public BulkTransferRequest Request { get; set; } = new();
        public int TotalTransfers { get; set; }
        public int SuccessfulTransfers { get; set; }
        public int FailedTransfers { get; set; }
        public List<string> SuccessfulTransferIds { get; set; } = new();
        public List<BulkTransferError> Errors { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }

    public class BulkTransferError
    {
        public string TransferId { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; }
    }

    // ==================== CYCLE COUNTING MODELS ====================
    /// <summary>
    /// Cycle count schedule entity
    /// </summary>
    [Description("Cycle count schedule entity")]
    public class CycleCountSchedule
    {
        public string ScheduleId { get; set; } = string.Empty;
        public CycleCountScheduleRequest ScheduleData { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? NextExecution { get; set; }
        public DateTime? LastExecution { get; set; }
        public CycleCountStatistics Statistics { get; set; } = new();
        public List<CycleCountExecutionHistory> ExecutionHistory { get; set; } = new();
    }

    public class CycleCountStatistics
    {
        public int TotalExecutions { get; set; }
        public int TotalTasksGenerated { get; set; }
        public int TotalTasksCompleted { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageAccuracy { get; set; }
        public DateTime? LastAccuracyCheck { get; set; }
    }

    public class CycleCountExecutionHistory
    {
        public DateTime ExecutionDate { get; set; }
        public int TasksGenerated { get; set; }
        public int TasksCompleted { get; set; }
        public decimal AccuracyRate { get; set; }
        public int DiscrepanciesFound { get; set; }
        public string? Notes { get; set; }
    }

    public class CycleCountTask
    {
        public string TaskId { get; set; } = string.Empty;
        public string ScheduleId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public string? LocationCode { get; set; }
        public decimal SystemQuantity { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? DueDate { get; set; }
        public CycleCountPriority Priority { get; set; }
        public CycleCountTaskStatus Status { get; set; }
        public string? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Dictionary<string, object> TaskData { get; set; } = new();
    }

    public class CycleCountResult
    {
        public string ResultId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public CycleCountData CountData { get; set; } = new();
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public bool HasDiscrepancy { get; set; }
        public CycleCountResultStatus Status { get; set; }
        public DateTime CompletedAt { get; set; }
        public List<CycleCountValidation> Validations { get; set; } = new();
        public string? ResolutionNotes { get; set; }
    }

    public class CycleCountValidation
    {
        public string ValidationType { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ValidationRule { get; set; } = string.Empty;
        public DateTime ValidatedAt { get; set; }
    }

    public class DiscrepancyResolution
    {
        public string ResolutionId { get; set; } = string.Empty;
        public string CountId { get; set; } = string.Empty;
        public DiscrepancyResolutionData ResolutionData { get; set; } = new();
        public DiscrepancyResolutionStatus Status { get; set; }
        public DateTime ResolvedAt { get; set; }
        public List<DiscrepancyResolutionStep> ResolutionSteps { get; set; } = new();
        public string? AdjustmentTransactionId { get; set; }
    }

    public class DiscrepancyResolutionStep
    {
        public string StepName { get; set; } = string.Empty;
        public DateTime StepDate { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public Dictionary<string, object> StepData { get; set; } = new();
    }

    public class CycleCountAccuracyMetrics
    {
        public DateTime ReportDate { get; set; }
        public AccuracyMetricsParameters Parameters { get; set; } = new();
        public decimal OverallAccuracy { get; set; }
        public List<AccuracyByPeriod> AccuracyTrends { get; set; } = new();
        public List<AccuracyByCounter> AccuracyByCounter { get; set; } = new();
        public List<AccuracyByCategory> AccuracyByCategory { get; set; } = new();
        public AccuracyStatistics Statistics { get; set; } = new();
        public List<AccuracyAlert> Alerts { get; set; } = new();
    }

    public class AccuracyByPeriod
    {
        public DateTime Period { get; set; }
        public decimal Accuracy { get; set; }
        public int CountsCompleted { get; set; }
        public int AccurateCounts { get; set; }
        public decimal AverageVariance { get; set; }
    }

    public class AccuracyByCounter
    {
        public string CounterId { get; set; } = string.Empty;
        public string CounterName { get; set; } = string.Empty;
        public decimal Accuracy { get; set; }
        public int CountsCompleted { get; set; }
        public decimal AverageVariance { get; set; }
        public string PerformanceRating { get; set; } = string.Empty;
    }

    public class AccuracyByCategory
    {
        public string Category { get; set; } = string.Empty;
        public decimal Accuracy { get; set; }
        public int CountsCompleted { get; set; }
        public decimal AverageVariance { get; set; }
    }

    public class AccuracyStatistics
    {
        public decimal MeanAccuracy { get; set; }
        public decimal MedianAccuracy { get; set; }
        public decimal StandardDeviation { get; set; }
        public decimal BestAccuracy { get; set; }
        public decimal WorstAccuracy { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
    }

    public class AccuracyAlert
    {
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime AlertDate { get; set; }
        public string? RecommendedAction { get; set; }
    }

    public class OutstandingCycleCount
    {
        public string TaskId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public string? LocationCode { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? DueDate { get; set; }
        public CycleCountPriority Priority { get; set; }
        public string? AssignedTo { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOutstanding { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class CycleCountVarianceReport
    {
        public DateTime ReportDate { get; set; }
        public VarianceReportParameters Parameters { get; set; } = new();
        public List<VarianceDetail> Variances { get; set; } = new();
        public VarianceSummary Summary { get; set; } = new();
        public List<VarianceAnalysis> Analysis { get; set; } = new();
    }

    public class VarianceDetail
    {
        public string CountId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public DateTime CountDate { get; set; }
        public decimal SystemQuantity { get; set; }
        public decimal CountedQuantity { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public decimal VarianceValue { get; set; }
        public string CountedBy { get; set; } = string.Empty;
        public CycleCountResultStatus Status { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class VarianceSummary
    {
        public int TotalVariances { get; set; }
        public decimal TotalVarianceValue { get; set; }
        public decimal AverageVariancePercentage { get; set; }
        public int PositiveVariances { get; set; }
        public int NegativeVariances { get; set; }
        public decimal LargestVarianceValue { get; set; }
    }

    public class VarianceAnalysis
    {
        public string AnalysisType { get; set; } = string.Empty;
        public string Finding { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public decimal Impact { get; set; }
        public string Priority { get; set; } = string.Empty;
    }

    public class InventoryAdjustmentResult
    {
        public string AdjustmentId { get; set; } = string.Empty;
        public string CountId { get; set; } = string.Empty;
        public AutoAdjustmentRules Rules { get; set; } = new();
        public List<AdjustmentDetail> Adjustments { get; set; } = new();
        public AdjustmentStatus Status { get; set; }
        public AdjustmentSummary Summary { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
    }

    public class AdjustmentDetail
    {
        public string ItemCode { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public decimal OldQuantity { get; set; }
        public decimal NewQuantity { get; set; }
        public decimal AdjustmentQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal AdjustmentValue { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool RequiredApproval { get; set; }
        public string? TransactionId { get; set; }
    }

    public class AdjustmentSummary
    {
        public int TotalAdjustments { get; set; }
        public decimal TotalAdjustmentValue { get; set; }
        public int PositiveAdjustments { get; set; }
        public int NegativeAdjustments { get; set; }
        public int AutoApprovedAdjustments { get; set; }
        public int PendingApprovalAdjustments { get; set; }
        public decimal NetAdjustmentValue { get; set; }
    }

    // ==================== ENUMS ====================
    public enum TurnoverRating
    {
        Excellent,
        Good,
        Average,
        Poor,
        Critical
    }
}
