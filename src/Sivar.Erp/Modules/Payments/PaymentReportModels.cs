using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sivar.Erp.Modules.Payments.Models;

namespace Sivar.Erp.Modules.Payments
{
    // ==================== ANALYTICS REPORT MODELS ====================
    /// <summary>
    /// Comprehensive payment analytics report
    /// </summary>
    [Description("Payment analytics report")]
    public class PaymentAnalyticsReport
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public PaymentAnalyticsParameters Parameters { get; set; } = new();
        public PaymentPeriodSummary PeriodSummary { get; set; } = new();
        public List<PaymentTrend> TrendAnalysis { get; set; } = new();
        public List<PaymentCategoryBreakdown> CategoryBreakdown { get; set; } = new();
        public PaymentAnalyticsPerformance PerformanceMetrics { get; set; } = new();
        public List<PaymentInsight> Insights { get; set; } = new();
        public List<PaymentRecommendation> Recommendations { get; set; } = new();
    }

    [Description("Payment period summary")]
    public class PaymentPeriodSummary
    {
        public int TotalPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AveragePaymentSize { get; set; }
        public Dictionary<string, decimal> PaymentMethodDistribution { get; set; } = new();
        public double GrowthRate { get; set; }
    }

    [Description("Payment trend data")]
    public class PaymentTrend
    {
        public DateTime Date { get; set; }
        public int PaymentCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    [Description("Payment category breakdown")]
    public class PaymentCategoryBreakdown
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    [Description("Payment analytics performance metrics")]
    public class PaymentAnalyticsPerformance
    {
        public decimal SuccessRate { get; set; }
        public decimal AverageProcessingTime { get; set; }
        public decimal FailureRate { get; set; }
        public decimal ReconciliationAccuracy { get; set; }
    }

    [Description("Payment insight")]
    public class PaymentInsight
    {
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public decimal ConfidenceScore { get; set; }
    }

    [Description("Payment recommendation")]
    public class PaymentRecommendation
    {
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EstimatedImpact { get; set; } = string.Empty;
        public string ImplementationEffort { get; set; } = string.Empty;
    }

    // ==================== CASH FLOW ANALYSIS MODELS ====================
    [Description("Cash flow analysis report")]
    public class CashFlowAnalysisReport
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public CashFlowAnalysisParameters Parameters { get; set; } = new();
        public CashFlowSummary CashFlowSummary { get; set; } = new();
        public List<CashFlowData> HistoricalData { get; set; } = new();
        public List<CashFlowProjection>? Projections { get; set; }
        public List<CashFlowTrend> Trends { get; set; } = new();
        public CashFlowRiskAnalysis RiskAnalysis { get; set; } = new();
        public List<CashFlowRecommendation> Recommendations { get; set; } = new();
    }

    [Description("Cash flow summary")]
    public class CashFlowSummary
    {
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal NetCashFlow { get; set; }
        public double CashFlowGrowthRate { get; set; }
    }

    [Description("Cash flow projection")]
    public class CashFlowProjection
    {
        public DateTime Date { get; set; }
        public decimal ProjectedInflow { get; set; }
        public decimal ProjectedOutflow { get; set; }
        public decimal NetProjection { get; set; }
        public int ConfidenceLevel { get; set; }
    }

    // Placeholder classes for additional cash flow models
    public class CashFlowData { }
    public class CashFlowTrend { }
    public class CashFlowRiskAnalysis { }
    public class CashFlowRecommendation { }

    // ==================== PAYMENT METHOD ANALYSIS MODELS ====================
    [Description("Payment method analysis report")]
    public class PaymentMethodAnalysisReport
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public PaymentMethodAnalysisParameters Parameters { get; set; } = new();
        public List<PaymentMethodComparison> MethodComparison { get; set; } = new();
        public List<PaymentMethodUsage> UsageStatistics { get; set; } = new();
        public PaymentMethodCostAnalysis? CostAnalysis { get; set; }
        public PaymentMethodPerformance? PerformanceMetrics { get; set; }
        public List<PaymentMethodTrend> TrendAnalysis { get; set; } = new();
        public List<PaymentMethodRecommendation> Recommendations { get; set; } = new();
    }

    // Placeholder classes for payment method models
    public class PaymentMethodComparison { }
    public class PaymentMethodUsage { }
    public class PaymentMethodCostAnalysis { }
    public class PaymentMethodPerformance { }
    public class PaymentMethodTrend { }
    public class PaymentMethodRecommendation { }

    // ==================== AGING REPORT MODELS ====================
    [Description("Accounts receivable aging report")]
    public class ARAgingReport
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public DateTime AsOfDate { get; set; }
        public ARAgingParameters Parameters { get; set; } = new();
        public ARAgingSummary AgingSummary { get; set; } = new();
        public List<CustomerAging> CustomerAging { get; set; } = new();
        public List<AgingBucket> AgingBuckets { get; set; } = new();
        public List<CriticalAccount> CriticalAccounts { get; set; } = new();
        public List<ARTrend> TrendAnalysis { get; set; } = new();
        public CollectionMetrics CollectionMetrics { get; set; } = new();
    }

    [Description("Accounts payable aging report")]
    public class APAgingReport
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public DateTime AsOfDate { get; set; }
        public APAgingParameters Parameters { get; set; } = new();
        public APAgingSummary AgingSummary { get; set; } = new();
        public List<SupplierAging> SupplierAging { get; set; } = new();
        public List<AgingBucket> AgingBuckets { get; set; } = new();
        public List<CriticalPayable> CriticalPayables { get; set; } = new();
        public List<APTrend> TrendAnalysis { get; set; } = new();
        public List<PaymentPriority> PaymentPriority { get; set; } = new();
    }

    // Placeholder classes for aging models
    public class ARAgingSummary { }
    public class CustomerAging { }
    public class AgingBucket { }
    public class CriticalAccount { }
    public class ARTrend { }
    public class CollectionMetrics { }
    public class APAgingSummary { }
    public class SupplierAging { }
    public class CriticalPayable { }
    public class APTrend { }

    // ==================== FORECAST MODELS ====================
    [Description("Payment forecast report")]
    public class PaymentForecastReport
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public PaymentForecastParameters Parameters { get; set; } = new();
        public PaymentForecastSummary ForecastSummary { get; set; } = new();
        public List<DailyForecast> DailyForecasts { get; set; } = new();
        public List<MethodForecastBreakdown> MethodBreakdown { get; set; } = new();
        public SeasonalityAnalysis? Seasonality { get; set; }
        public List<ConfidenceInterval> ConfidenceIntervals { get; set; } = new();
        public ScenarioAnalysis ScenarioAnalysis { get; set; } = new();
    }

    // Placeholder classes for forecast models
    public class PaymentForecastSummary { }
    public class DailyForecast { }
    public class MethodForecastBreakdown { }
    public class SeasonalityAnalysis { }
    public class ConfidenceInterval { }
    public class ScenarioAnalysis { }

    // ==================== PERFORMANCE METRICS MODELS ====================
    [Description("Payment performance metrics")]
    public class PaymentPerformanceMetrics
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public PaymentPerformanceParameters Parameters { get; set; } = new();
        public VolumeMetrics VolumeMetrics { get; set; } = new();
        public TimingMetrics TimingMetrics { get; set; } = new();
        public QualityMetrics QualityMetrics { get; set; } = new();
        public EfficiencyMetrics EfficiencyMetrics { get; set; } = new();
        public PerformanceBenchmarks? Benchmarks { get; set; }
        public TrendComparison TrendComparison { get; set; } = new();
    }

    // Placeholder classes for performance models
    public class VolumeMetrics { }
    public class TimingMetrics { }
    public class QualityMetrics { }
    public class EfficiencyMetrics { }
    public class PerformanceBenchmarks { }
    public class TrendComparison { }

    // ==================== DASHBOARD MODELS ====================
    [Description("Payment dashboard")]
    public class PaymentDashboard
    {
        public string DashboardId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public DateTime RefreshTime { get; set; }
        public PaymentDashboardParameters Parameters { get; set; } = new();
        public List<DashboardWidget> Widgets { get; set; } = new();
        public List<DashboardAlert> Alerts { get; set; } = new();
        public RealTimeMetrics RealTimeMetrics { get; set; } = new();
        public List<QuickAction> QuickActions { get; set; } = new();
    }

    // Placeholder classes for dashboard models
    public class DashboardWidget { }
    public class DashboardAlert { }
    public class RealTimeMetrics { }
    public class QuickAction { }

    // ==================== RECONCILIATION MODELS ====================
    [Description("Bank reconciliation")]
    public class BankReconciliation
    {
        public string ReconciliationId { get; set; } = string.Empty;
        public string BankAccountCode { get; set; } = string.Empty;
        public DateTime StatementDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal StatementBalance { get; set; }
        public string StatementNumber { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ReconciliationStatus Status { get; set; }
        public List<BankTransaction> BankTransactions { get; set; } = new();
        public decimal BookBalance { get; set; }
        public List<ReconciliationItem> ReconciliationItems { get; set; } = new();
        public List<ReconciliationAdjustment> Adjustments { get; set; } = new();
        public ReconciliationSummary ReconciliationSummary { get; set; } = new();
    }

    [Description("Reconciliation summary")]
    public class ReconciliationSummary
    {
        public decimal BookBalance { get; set; }
        public decimal StatementBalance { get; set; }
        public int MatchedTransactions { get; set; }
        public int UnmatchedTransactions { get; set; }
        public decimal TotalAdjustments { get; set; }
        public decimal ReconciledBalance { get; set; }
        public decimal Variance { get; set; }
    }

    [Description("Reconciliation item")]
    public class ReconciliationItem
    {
        public string ItemId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool IsMatched { get; set; }
    }

    [Description("Payment matching result")]
    public class PaymentMatchingResult
    {
        public string MatchingId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public AutoMatchingParameters Parameters { get; set; } = new();
        public List<PaymentMatch> Matches { get; set; } = new();
        public MatchingSummary MatchingSummary { get; set; } = new();
    }

    [Description("Payment match")]
    public class PaymentMatch
    {
        public string MatchId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string BankTransactionId { get; set; } = string.Empty;
        public decimal ConfidenceScore { get; set; }
        public MatchType MatchType { get; set; }
        public string MatchingReason { get; set; } = string.Empty;
    }

    [Description("Matching summary")]
    public class MatchingSummary
    {
        public int TotalPayments { get; set; }
        public int TotalTransactions { get; set; }
        public int MatchedCount { get; set; }
        public int UnmatchedPayments { get; set; }
        public int UnmatchedTransactions { get; set; }
        public double MatchingAccuracy { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }

    [Description("Payment match result")]
    public class PaymentMatchResult
    {
        public string MatchId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string BankTransactionId { get; set; } = string.Empty;
        public MatchingData MatchingData { get; set; } = new();
        public MatchType MatchType { get; set; }
        public DateTime ProcessedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public decimal ConfidenceScore { get; set; }
    }

    [Description("Unmatched payment")]
    public class UnmatchedPayment
    {
        public string PaymentId { get; set; } = string.Empty;
        public string PaymentNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string BusinessEntityName { get; set; } = string.Empty;
        public string PaymentMethodCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public int DaysUnmatched { get; set; }
        public List<MatchingSuggestion> MatchingSuggestions { get; set; } = new();
    }

    [Description("Matching suggestion")]
    public class MatchingSuggestion
    {
        public string BankTransactionId { get; set; } = string.Empty;
        public string SuggestionType { get; set; } = string.Empty;
        public decimal ConfidenceScore { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> MatchingCriteria { get; set; } = new();
    }

    [Description("Bank statement import result")]
    public class BankStatementImportResult
    {
        public string ImportId { get; set; } = string.Empty;
        public string BankAccountCode { get; set; } = string.Empty;
        public DateTime ImportedAt { get; set; }
        public string ImportedBy { get; set; } = string.Empty;
        public BankStatementFormat Format { get; set; }
        public bool IsSuccessful { get; set; }
        public ImportSummary ImportSummary { get; set; } = new();
        public List<ImportedTransaction> ImportedTransactions { get; set; } = new();
        public List<ImportValidationError> ValidationErrors { get; set; } = new();
        public List<ImportLogEntry> ProcessingLog { get; set; } = new();
    }

    [Description("Import summary")]
    public class ImportSummary
    {
        public int TotalLines { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public decimal TotalAmount { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }

    [Description("Imported transaction")]
    public class ImportedTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Reference { get; set; }
    }

    [Description("Import validation error")]
    public class ImportValidationError
    {
        public int LineNumber { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }

    [Description("Import log entry")]
    public class ImportLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    [Description("Raw transaction")]
    public class RawTransaction
    {
        public int LineNumber { get; set; }
        public string RawData { get; set; } = string.Empty;
    }

    [Description("Reconciliation report")]
    public class ReconciliationReport
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public ReconciliationReportParameters Parameters { get; set; } = new();
        public ReconciliationReportSummary ReconciliationSummary { get; set; } = new();
        public List<MatchedReconciliationItem> MatchedItems { get; set; } = new();
        public List<UnmatchedReconciliationItem> UnmatchedItems { get; set; } = new();
        public List<ReconciliationAdjustment> Adjustments { get; set; } = new();
        public VarianceAnalysis VarianceAnalysis { get; set; } = new();
        public List<AuditTrailEntry> AuditTrail { get; set; } = new();
    }

    // Placeholder classes for reconciliation report models
    public class ReconciliationReportSummary { }
    public class MatchedReconciliationItem { }
    public class UnmatchedReconciliationItem { }
    public class VarianceAnalysis { }
    public class AuditTrailEntry { }

    [Description("Reconciliation adjustment result")]
    public class ReconciliationAdjustmentResult
    {
        public string AdjustmentId { get; set; } = string.Empty;
        public string ReconciliationId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
        public ReconciliationAdjustment Adjustment { get; set; } = new();
        public string? JournalEntryId { get; set; }
        public bool IsSuccessful { get; set; }
    }

    [Description("Bank reconciliation result")]
    public class BankReconciliationResult
    {
        public string ReconciliationId { get; set; } = string.Empty;
        public DateTime FinalizedAt { get; set; }
        public string FinalizedBy { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; }
        public List<string> ProcessedAdjustments { get; set; } = new();
        public FinalizationSummary FinalizationSummary { get; set; } = new();
    }

    [Description("Finalization summary")]
    public class FinalizationSummary
    {
        public int TotalMatched { get; set; }
        public int TotalUnmatched { get; set; }
        public int TotalAdjustments { get; set; }
        public decimal FinalVariance { get; set; }
    }

    // ==================== SECURITY MODELS ====================
    [Description("Payment security validation")]
    public class PaymentSecurityValidation
    {
        public string ValidationId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public DateTime ValidatedAt { get; set; }
        public string ValidatedBy { get; set; } = string.Empty;
        public List<SecurityValidationResult> ValidationResults { get; set; } = new();
        public SecurityValidationStatus OverallStatus { get; set; }
        public decimal RiskScore { get; set; }
        public List<SecurityAction> RequiredActions { get; set; } = new();
        public List<ComplianceFlag> ComplianceFlags { get; set; } = new();
    }

    [Description("Security validation result")]
    public class SecurityValidationResult
    {
        public SecurityValidationType ValidationType { get; set; }
        public ValidationStatus Status { get; set; }
        public decimal RiskScore { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> ValidationData { get; set; } = new();
    }

    [Description("Security action")]
    public class SecurityAction
    {
        public string ActionType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RequiredBy { get; set; } = string.Empty;
    }

    [Description("Compliance flag")]
    public class ComplianceFlag
    {
        public ComplianceType Type { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RequiredAction { get; set; } = string.Empty;
    }

    [Description("Fraud detection result")]
    public class FraudDetectionResult
    {
        public string DetectionId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public FraudDetectionParameters Parameters { get; set; } = new();
        public List<SuspiciousPaymentAlert> SuspiciousPayments { get; set; } = new();
        public List<FraudIndicatorResult> FraudIndicators { get; set; } = new();
        public FraudRiskAnalysis RiskAnalysis { get; set; } = new();
        public MLFraudAnalysisResult? MLAnalysisResult { get; set; }
        public List<FraudAction> RecommendedActions { get; set; } = new();
    }

    [Description("Suspicious payment alert")]
    public class SuspiciousPaymentAlert
    {
        public string AlertId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public List<SuspiciousIndicator> Indicators { get; set; } = new();
        public decimal RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
    }

    [Description("Fraud indicator result")]
    public class FraudIndicatorResult
    {
        public string IndicatorType { get; set; } = string.Empty;
        public bool IsTriggered { get; set; }
        public decimal Score { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    [Description("Fraud risk analysis")]
    public class FraudRiskAnalysis
    {
        public decimal OverallRiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<string> PrimaryRiskFactors { get; set; } = new();
        public string RecommendedAction { get; set; } = string.Empty;
    }

    [Description("ML fraud analysis result")]
    public class MLFraudAnalysisResult
    {
        public string ModelVersion { get; set; } = string.Empty;
        public decimal ConfidenceScore { get; set; }
        public decimal RiskScore { get; set; }
        public double PredictedFraudProbability { get; set; }
        public Dictionary<string, decimal> FeatureImportance { get; set; } = new();
    }

    [Description("Fraud action")]
    public class FraudAction
    {
        public string ActionType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ResponsibleParty { get; set; } = string.Empty;
    }

    [Description("Payment authorization result")]
    public class PaymentAuthorizationResult
    {
        public string AuthorizationId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public PaymentAuthorizationData AuthorizationData { get; set; } = new();
        public AuthorizationStatus AuthorizationStatus { get; set; }
        public List<RequiredApproval> RequiredApprovals { get; set; } = new();
        public List<ComplianceCheckResult> ComplianceChecks { get; set; } = new();
        public List<AuthorizationAuditEntry> AuditTrail { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    [Description("Required approval")]
    public class RequiredApproval
    {
        public string ApprovalType { get; set; } = string.Empty;
        public string RequiredRole { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsObtained { get; set; }
    }

    [Description("Compliance check result")]
    public class ComplianceCheckResult
    {
        public ComplianceType Type { get; set; }
        public bool Passed { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
    }

    [Description("Authorization audit entry")]
    public class AuthorizationAuditEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    [Description("Authorizer validation result")]
    public class AuthorizerValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    [Description("Payment compliance report")]
    public class PaymentComplianceReport
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public PaymentComplianceParameters Parameters { get; set; } = new();
        public ComplianceSummary ComplianceSummary { get; set; } = new();
        public List<ComplianceMetric> ComplianceMetrics { get; set; } = new();
        public List<ComplianceViolation> Violations { get; set; } = new();
        public List<RemediationAction> RemediationActions { get; set; } = new();
        public List<RegulatoryRequirement> RegulatoryRequirements { get; set; } = new();
        public List<ComplianceAuditEntry> AuditTrail { get; set; } = new();
    }

    // Placeholder classes for compliance models
    public class ComplianceSummary { }
    public class ComplianceMetric { }
    public class ComplianceViolation { }
    public class RemediationAction { }
    public class RegulatoryRequirement { }
    public class ComplianceAuditEntry { }

    [Description("Payment limits validation")]
    public class PaymentLimitsValidation
    {
        public string ValidationId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public DateTime ValidatedAt { get; set; }
        public List<PaymentLimitCheck> LimitChecks { get; set; } = new();
        public LimitValidationStatus OverallStatus { get; set; }
        public decimal TotalAvailableLimit { get; set; }
        public List<string> RequiredApprovals { get; set; } = new();
    }

    [Description("Payment limit check")]
    public class PaymentLimitCheck
    {
        public PaymentLimitType LimitType { get; set; }
        public decimal CurrentUsage { get; set; }
        public decimal LimitAmount { get; set; }
        public LimitCheckStatus Status { get; set; }
        public decimal AvailableAmount { get; set; }
    }

    [Description("Suspicious payment investigation")]
    public class SuspiciousPaymentInvestigation
    {
        public string InvestigationId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public DateTime InitiatedAt { get; set; }
        public string InitiatedBy { get; set; } = string.Empty;
        public InvestigationPriority Priority { get; set; }
        public InvestigationStatus Status { get; set; }
        public List<SuspiciousIndicator> SuspiciousIndicators { get; set; } = new();
        public List<InvestigationStep> InvestigationSteps { get; set; } = new();
        public List<InvestigationFinding> Findings { get; set; } = new();
        public List<InvestigationAction> RecommendedActions { get; set; } = new();
        public InvestigationResolution? Resolution { get; set; }
    }

    // Placeholder classes for investigation models
    public class InvestigationStep { }
    public class InvestigationFinding { }
    public class InvestigationAction { }
    public class InvestigationResolution { }

    [Description("Payment security metrics")]
    public class PaymentSecurityMetrics
    {
        public string MetricsId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public PaymentSecurityMetricsParameters Parameters { get; set; } = new();
        public SecuritySummary SecuritySummary { get; set; } = new();
        public FraudMetrics FraudMetrics { get; set; } = new();
        public SecurityComplianceMetrics ComplianceMetrics { get; set; } = new();
        public AuthorizationMetrics AuthorizationMetrics { get; set; } = new();
        public EncryptionMetrics EncryptionMetrics { get; set; } = new();
        public List<SecurityTrend> Trends { get; set; } = new();
        public List<SecurityAlert> Alerts { get; set; } = new();
    }

    // Placeholder classes for security metrics models
    public class SecuritySummary { }
    public class FraudMetrics { }
    public class SecurityComplianceMetrics { }
    public class AuthorizationMetrics { }
    public class EncryptionMetrics { }
    public class SecurityTrend { }
    public class SecurityAlert { }

    [Description("Payment encryption result")]
    public class PaymentEncryptionResult
    {
        public string EncryptionId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public EncryptionMethod Method { get; set; }
        public Dictionary<string, string> EncryptedData { get; set; } = new();
        public string KeyId { get; set; } = string.Empty;
        public string? IntegrityCheck { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
