using System;
using System.Collections.Generic;
using System.Linq;

namespace Sivar.Erp.Modules.Inventory.Reports
{
    // Advanced inventory report models
    public class InventoryTurnoverAnalysis
    {
        public DateTime AnalysisDate { get; set; }
        public List<ItemTurnoverData> Items { get; set; } = new();
        public TurnoverSummary Summary { get; set; } = new();
        public InventoryAnalyticsParameters Parameters { get; set; } = new();
    }

    public class ItemTurnoverData
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal AverageInventory { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal TurnoverRatio { get; set; }
        public int DaysInInventory { get; set; }
        public TurnoverClassification Classification { get; set; }
        public decimal InventoryValue { get; set; }
        public int TransactionCount { get; set; }
    }

    public class TurnoverSummary
    {
        public decimal OverallTurnoverRatio { get; set; }
        public int AverageDaysInInventory { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int FastMovingItems { get; set; }
        public int SlowMovingItems { get; set; }
        public int DeadStockItems { get; set; }
        public Dictionary<string, decimal> TurnoverByCategory { get; set; } = new();
    }

    public class ABCAnalysisReport
    {
        public DateTime AnalysisDate { get; set; }
        public ABCAnalysisParameters Parameters { get; set; } = new();
        public List<ABCClassificationItem> Items { get; set; } = new();
        public ABCAnalysisSummary Summary { get; set; } = new();
        public List<ABCRecommendation> Recommendations { get; set; } = new();
    }

    public class ABCClassificationItem
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal AnnualValue { get; set; }
        public decimal AnnualQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal CumulativeValue { get; set; }
        public decimal CumulativePercentage { get; set; }
        public string ABCClass { get; set; } = string.Empty;
        public int Rank { get; set; }
        public decimal Frequency { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class ABCAnalysisSummary
    {
        public int TotalItems { get; set; }
        public decimal TotalValue { get; set; }
        public int AClassItems { get; set; }
        public decimal AClassValue { get; set; }
        public decimal AClassPercentage { get; set; }
        public int BClassItems { get; set; }
        public decimal BClassValue { get; set; }
        public decimal BClassPercentage { get; set; }
        public int CClassItems { get; set; }
        public decimal CClassValue { get; set; }
        public decimal CClassPercentage { get; set; }
    }

    public class ABCRecommendation
    {
        public string Category { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public decimal PotentialSavings { get; set; }
    }

    public class SlowMovingInventoryReport
    {
        public DateTime ReportDate { get; set; }
        public SlowMovingAnalysisParameters Parameters { get; set; } = new();
        public List<SlowMovingItem> Items { get; set; } = new();
        public SlowMovingSummary Summary { get; set; } = new();
        public List<DisposalRecommendation> Recommendations { get; set; } = new();
    }

    public class SlowMovingItem
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal CurrentQuantity { get; set; }
        public decimal CurrentValue { get; set; }
        public DateTime LastMovementDate { get; set; }
        public int DaysSinceLastMovement { get; set; }
        public decimal MonthlyUsage { get; set; }
        public decimal MonthsOfSupply { get; set; }
        public SlowMovingCategory SlowMovingCategory { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
        public decimal EstimatedCarryingCost { get; set; }
    }

    public class SlowMovingSummary
    {
        public int TotalSlowMovingItems { get; set; }
        public decimal TotalSlowMovingValue { get; set; }
        public decimal TotalCarryingCost { get; set; }
        public int ObsoleteItems { get; set; }
        public decimal ObsoleteValue { get; set; }
        public Dictionary<string, int> ItemsByCategory { get; set; } = new();
        public Dictionary<string, decimal> ValueByCategory { get; set; } = new();
    }

    public class DisposalRecommendation
    {
        public string ItemCode { get; set; } = string.Empty;
        public string DisposalMethod { get; set; } = string.Empty;
        public decimal EstimatedRecoveryValue { get; set; }
        public string Justification { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }

    public class InventoryAgingReport
    {
        public DateTime ReportDate { get; set; }
        public InventoryAgingParameters Parameters { get; set; } = new();
        public List<InventoryAgingItem> Items { get; set; } = new();
        public InventoryAgingSummary Summary { get; set; } = new();
        public List<AgingBucket> AgingBuckets { get; set; } = new();
    }

    public class InventoryAgingItem
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<AgingLayer> AgingLayers { get; set; } = new();
        public decimal TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public decimal WeightedAverageAge { get; set; }
        public int OldestLayerDays { get; set; }
    }

    public class AgingLayer
    {
        public DateTime ReceiptDate { get; set; }
        public int AgeDays { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LayerValue { get; set; }
        public string AgingBucket { get; set; } = string.Empty;
    }

    public class InventoryAgingSummary
    {
        public decimal TotalInventoryValue { get; set; }
        public int TotalItems { get; set; }
        public decimal WeightedAverageAge { get; set; }
        public Dictionary<string, AgingBucketSummary> BucketSummaries { get; set; } = new();
    }

    public class AgingBucket
    {
        public string Name { get; set; } = string.Empty;
        public int MinDays { get; set; }
        public int MaxDays { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
        public int ItemCount { get; set; }
    }

    public class AgingBucketSummary
    {
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
        public int ItemCount { get; set; }
    }

    public class DemandForecastReport
    {
        public DateTime ReportDate { get; set; }
        public DemandForecastParameters Parameters { get; set; } = new();
        public List<ItemForecast> Forecasts { get; set; } = new();
        public ForecastAccuracy Accuracy { get; set; } = new();
        public List<ForecastAlert> Alerts { get; set; } = new();
    }

    public class ItemForecast
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public List<ForecastPeriod> Periods { get; set; } = new();
        public ForecastStatistics Statistics { get; set; } = new();
        public decimal SafetyStock { get; set; }
        public decimal ReorderPoint { get; set; }
        public string ForecastModel { get; set; } = string.Empty;
    }

    public class ForecastPeriod
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal ForecastDemand { get; set; }
        public decimal ConfidenceInterval { get; set; }
        public decimal LowerBound { get; set; }
        public decimal UpperBound { get; set; }
        public decimal? ActualDemand { get; set; }
        public decimal? Variance { get; set; }
    }

    public class ForecastStatistics
    {
        public decimal MeanAbsoluteError { get; set; }
        public decimal MeanAbsolutePercentageError { get; set; }
        public decimal StandardDeviation { get; set; }
        public decimal TrendSlope { get; set; }
        public decimal SeasonalityIndex { get; set; }
        public decimal ModelAccuracy { get; set; }
    }

    public class ForecastAccuracy
    {
        public decimal OverallAccuracy { get; set; }
        public Dictionary<string, decimal> AccuracyByMethod { get; set; } = new();
        public Dictionary<string, decimal> AccuracyByCategory { get; set; } = new();
        public List<string> ModelRecommendations { get; set; } = new();
    }

    public class ForecastAlert
    {
        public string ItemCode { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime AlertDate { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    public class StockOptimizationReport
    {
        public DateTime ReportDate { get; set; }
        public StockOptimizationParameters Parameters { get; set; } = new();
        public List<OptimizationRecommendation> Recommendations { get; set; } = new();
        public OptimizationSummary Summary { get; set; } = new();
        public List<CostBenefit> CostBenefitAnalysis { get; set; } = new();
    }

    public class OptimizationRecommendation
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal RecommendedStock { get; set; }
        public decimal OptimalOrderQuantity { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal SafetyStock { get; set; }
        public string RecommendationType { get; set; } = string.Empty;
        public decimal EstimatedSavings { get; set; }
        public string Justification { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public class OptimizationSummary
    {
        public decimal TotalCurrentValue { get; set; }
        public decimal TotalOptimizedValue { get; set; }
        public decimal EstimatedSavings { get; set; }
        public decimal SavingsPercentage { get; set; }
        public int ItemsToIncrease { get; set; }
        public int ItemsToDecrease { get; set; }
        public int ItemsOptimal { get; set; }
    }

    public class CostBenefit
    {
        public string Category { get; set; } = string.Empty;
        public decimal CurrentCost { get; set; }
        public decimal OptimizedCost { get; set; }
        public decimal Savings { get; set; }
        public string CostType { get; set; } = string.Empty;
    }

    public class InventoryPerformanceMetrics
    {
        public DateTime ReportDate { get; set; }
        public PerformanceMetricsParameters Parameters { get; set; } = new();
        public Dictionary<string, MetricValue> Metrics { get; set; } = new();
        public List<MetricTrend> Trends { get; set; } = new();
        public List<PerformanceAlert> Alerts { get; set; } = new();
        public PerformanceBenchmarks Benchmarks { get; set; } = new();
    }

    public class MetricValue
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal? PreviousPeriodValue { get; set; }
        public decimal? ChangePercent { get; set; }
        public string Trend { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class MetricTrend
    {
        public string MetricName { get; set; } = string.Empty;
        public List<TrendDataPoint> DataPoints { get; set; } = new();
        public decimal TrendSlope { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
        public decimal RSquared { get; set; }
    }

    public class TrendDataPoint
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Period { get; set; } = string.Empty;
    }

    public class PerformanceAlert
    {
        public string MetricName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal ThresholdValue { get; set; }
        public DateTime AlertDate { get; set; }
    }

    public class PerformanceBenchmarks
    {
        public Dictionary<string, BenchmarkData> IndustryBenchmarks { get; set; } = new();
        public Dictionary<string, BenchmarkData> InternalBenchmarks { get; set; } = new();
        public Dictionary<string, BenchmarkData> BestPractices { get; set; } = new();
    }

    public class BenchmarkData
    {
        public decimal Value { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class InventoryDashboardData
    {
        public DateTime LastUpdated { get; set; }
        public DashboardParameters Parameters { get; set; } = new();
        public List<DashboardWidget> Widgets { get; set; } = new();
        public Dictionary<string, object> RealTimeMetrics { get; set; } = new();
        public List<DashboardAlert> Alerts { get; set; } = new();
    }

    public class DashboardWidget
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public WidgetConfiguration Configuration { get; set; } = new();
        public DateTime LastRefresh { get; set; }
    }

    public class WidgetConfiguration
    {
        public int RefreshIntervalSeconds { get; set; } = 300;
        public Dictionary<string, object> Settings { get; set; } = new();
        public List<string> DataSources { get; set; } = new();
    }

    public class DashboardAlert
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime AlertTime { get; set; }
        public string Source { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public Dictionary<string, object> ActionData { get; set; } = new();
    }

    // Transfer related models
    public class InventoryTransferRequest
    {
        public string TransferId { get; set; } = string.Empty;
        public TransferRequestData RequestData { get; set; } = new();
        public TransferStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public string? ApprovalNotes { get; set; }
        public List<TransferApprovalHistory> ApprovalHistory { get; set; } = new();
    }

    public class TransferApprovalHistory
    {
        public DateTime ActionDate { get; set; }
        public string ActionBy { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public TransferStatus PreviousStatus { get; set; }
        public TransferStatus NewStatus { get; set; }
    }

    public class InventoryTransferExecution
    {
        public string TransferId { get; set; } = string.Empty;
        public string ExecutionId { get; set; } = string.Empty;
        public TransferExecutionData ExecutionData { get; set; } = new();
        public List<string> SourceTransactionIds { get; set; } = new();
        public List<string> DestinationTransactionIds { get; set; } = new();
        public TransferExecutionStatus Status { get; set; }
        public List<TransferExecutionStep> ExecutionSteps { get; set; } = new();
        public DateTime CompletedAt { get; set; }
    }

    public class TransferExecutionStep
    {
        public string StepName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public Dictionary<string, object> StepData { get; set; } = new();
    }

    public class InventoryTransferStatus
    {
        public string TransferId { get; set; } = string.Empty;
        public TransferStatus Status { get; set; }
        public decimal PercentComplete { get; set; }
        public string CurrentStep { get; set; } = string.Empty;
        public DateTime? EstimatedCompletion { get; set; }
        public List<TransferStatusUpdate> StatusHistory { get; set; } = new();
        public Dictionary<string, object> TrackingData { get; set; } = new();
    }

    public class TransferStatusUpdate
    {
        public DateTime UpdateTime { get; set; }
        public TransferStatus Status { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class InventoryTransferHistory
    {
        public string TransferId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string FromWarehouse { get; set; } = string.Empty;
        public string ToWarehouse { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public TransferStatus Status { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string? ExecutedBy { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal? ActualQuantity { get; set; }
    }

    public class PendingTransfer
    {
        public string TransferId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string FromWarehouse { get; set; } = string.Empty;
        public string ToWarehouse { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public TransferPriority Priority { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOutstanding { get; set; }
    }

    public class BulkTransferResult
    {
        public string BatchId { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public List<BulkTransferError> Errors { get; set; } = new();
        public List<string> CreatedTransferIds { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
    }

    public class BulkTransferError
    {
        public int RequestIndex { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, object> ErrorDetails { get; set; } = new();
    }

    // Cycle counting models
    public class CycleCountSchedule
    {
        public string ScheduleId { get; set; } = string.Empty;
        public CycleCountScheduleRequest ScheduleData { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastExecuted { get; set; }
        public DateTime? NextExecution { get; set; }
        public List<CycleCountExecution> ExecutionHistory { get; set; } = new();
        public CycleCountStatistics Statistics { get; set; } = new();
    }

    public class CycleCountRule
    {
        public string RuleType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; }
    }

    public class CycleCountExecution
    {
        public string ExecutionId { get; set; } = string.Empty;
        public DateTime ExecutionDate { get; set; }
        public int TasksGenerated { get; set; }
        public int TasksCompleted { get; set; }
        public string Status { get; set; } = string.Empty;
        public Dictionary<string, object> ExecutionData { get; set; } = new();
    }

    public class CycleCountStatistics
    {
        public int TotalExecutions { get; set; }
        public int TotalTasksGenerated { get; set; }
        public int TotalTasksCompleted { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageAccuracy { get; set; }
        public DateTime? LastAccuracyUpdate { get; set; }
    }

    public class CycleCountTask
    {
        public string TaskId { get; set; } = string.Empty;
        public string ScheduleId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public decimal SystemQuantity { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? DueDate { get; set; }
        public CycleCountPriority Priority { get; set; }
        public string? AssignedTo { get; set; }
        public CycleCountTaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
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
        public string? ResolutionNotes { get; set; }
        public List<CycleCountValidation> Validations { get; set; } = new();
    }

    public class CycleCountObservation
    {
        public string ObservationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime ObservedAt { get; set; }
        public string ObservedBy { get; set; } = string.Empty;
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
        public List<DiscrepancyResolutionStep> ResolutionSteps { get; set; } = new();
        public DateTime ResolvedAt { get; set; }
        public string? AdjustmentTransactionId { get; set; }
    }

    public class DiscrepancyResolutionStep
    {
        public string StepName { get; set; } = string.Empty;
        public DateTime StepDate { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
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
        public List<string> CommonIssues { get; set; } = new();
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
        public Dictionary<string, object> AlertData { get; set; } = new();
    }

    public class OutstandingCycleCount
    {
        public string TaskId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
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
        public string Status { get; set; } = string.Empty;
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
        public Dictionary<string, int> VariancesByCategory { get; set; } = new();
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
        public List<AdjustmentDetail> Adjustments { get; set; } = new();
        public AdjustmentSummary Summary { get; set; } = new();
        public AutoAdjustmentRules Rules { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
        public AdjustmentStatus Status { get; set; }
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
        public string TransactionId { get; set; } = string.Empty;
        public bool RequiredApproval { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
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

    // Supporting enums for advanced inventory models
    public enum TurnoverClassification
    {
        Fast,
        Medium,
        Slow,
        Dead
    }

    public enum SlowMovingCategory
    {
        SlowMoving,
        Obsolete,
        Seasonal,
        Damaged
    }

    public enum TransferExecutionStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        PartiallyCompleted
    }

    public enum CycleCountTaskStatus
    {
        Scheduled,
        Assigned,
        InProgress,
        Completed,
        Cancelled,
        Overdue
    }

    public enum CycleCountResultStatus
    {
        Pending,
        Accurate,
        HasDiscrepancy,
        Resolved,
        RequiresInvestigation
    }

    public enum DiscrepancyResolutionStatus
    {
        Pending,
        Investigating,
        Resolved,
        Approved,
        Rejected
    }

    public enum AdjustmentStatus
    {
        Pending,
        Approved,
        Rejected,
        Applied,
        Failed
    }
}
