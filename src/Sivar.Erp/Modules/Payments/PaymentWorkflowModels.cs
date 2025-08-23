using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sivar.Erp.Modules.Payments.Models;

namespace Sivar.Erp.Modules.Payments
{
    // ==================== ANALYTICS DASHBOARD MODELS ====================
    [Description("Payment analytics dashboard request")]
    public class PaymentAnalyticsDashboardRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeOverview { get; set; } = true;
        public bool IncludeCashFlow { get; set; } = true;
        public bool IncludePaymentMethods { get; set; } = true;
        public bool IncludeAging { get; set; } = true;
        public bool IncludeForecasting { get; set; } = true;
        public bool IncludePerformance { get; set; } = true;
        public string RequestedBy { get; set; } = string.Empty;
        public string TimeZone { get; set; } = "UTC";
        public string Currency { get; set; } = "USD";
        public List<string> FilterCriteria { get; set; } = new();
    }

    [Description("Payment analytics dashboard")]
    public class PaymentAnalyticsDashboard
    {
        public string DashboardId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public PaymentAnalyticsDashboardRequest Request { get; set; } = new();
        public List<AnalyticsDashboardModule> Modules { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
        public string Status { get; set; } = "Completed";
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    // ==================== COMPREHENSIVE RECONCILIATION MODELS ====================
    [Description("Comprehensive reconciliation request")]
    public class ComprehensiveReconciliationRequest
    {
        public string BankAccountCode { get; set; } = string.Empty;
        public DateTime StatementDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string StatementFileContent { get; set; } = string.Empty;
        public BankStatementFormat StatementFormat { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public bool IncludeManualReview { get; set; } = true;
        public bool ProcessAdjustments { get; set; } = true;
        public bool AutoFinalize { get; set; } = false;
        public decimal ToleranceAmount { get; set; } = 0.01m;
        public int ToleranceDays { get; set; } = 3;
        public List<string> NotificationRecipients { get; set; } = new();
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }

    [Description("Comprehensive reconciliation result")]
    public class ComprehensiveReconciliationResult
    {
        public string ReconciliationId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public ComprehensiveReconciliationRequest Request { get; set; } = new();
        public List<ReconciliationStep> Steps { get; set; } = new();
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public ReconciliationMetrics Metrics { get; set; } = new();
        public List<string> Notifications { get; set; } = new();
    }

    [Description("Reconciliation metrics")]
    public class ReconciliationMetrics
    {
        public int TotalTransactionsProcessed { get; set; }
        public int AutoMatchedTransactions { get; set; }
        public int ManuallyMatchedTransactions { get; set; }
        public int UnmatchedTransactions { get; set; }
        public int AdjustmentsProcessed { get; set; }
        public decimal TotalReconciledAmount { get; set; }
        public decimal VarianceAmount { get; set; }
        public double MatchingAccuracy { get; set; }
        public TimeSpan AverageMatchingTime { get; set; }
    }

    // ==================== SECURITY ASSESSMENT MODELS ====================
    [Description("Payment security assessment request")]
    public class PaymentSecurityAssessmentRequest
    {
        public List<string> PaymentIds { get; set; } = new();
        public DateTime AnalysisStartDate { get; set; }
        public DateTime AnalysisEndDate { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public List<SecurityValidationType> ValidationTypes { get; set; } = new();
        public bool IncludeFraudDetection { get; set; } = true;
        public bool IncludeCompliance { get; set; } = true;
        public bool IncludeMLAnalysis { get; set; } = true;
        public List<ComplianceType> ComplianceTypes { get; set; } = new();
        public string AssessmentLevel { get; set; } = "Comprehensive";
        public Dictionary<string, object> CustomCriteria { get; set; } = new();
    }

    [Description("Payment security assessment")]
    public class PaymentSecurityAssessment
    {
        public string AssessmentId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public PaymentSecurityAssessmentRequest Request { get; set; } = new();
        public List<PaymentSecurityValidation> SecurityValidations { get; set; } = new();
        public List<FraudDetectionResult> FraudDetectionResults { get; set; } = new();
        public List<PaymentComplianceReport> ComplianceResults { get; set; } = new();
        public decimal OverallSecurityScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<SecurityRecommendation> RecommendedActions { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
        public SecurityAssessmentSummary Summary { get; set; } = new();
    }

    [Description("Security assessment summary")]
    public class SecurityAssessmentSummary
    {
        public int TotalPaymentsAssessed { get; set; }
        public int HighRiskPayments { get; set; }
        public int MediumRiskPayments { get; set; }
        public int LowRiskPayments { get; set; }
        public int FraudAlertsGenerated { get; set; }
        public int ComplianceViolations { get; set; }
        public int SecurityRecommendations { get; set; }
        public decimal AverageRiskScore { get; set; }
        public string OverallAssessment { get; set; } = string.Empty;
    }

    // ==================== INTEGRATED WORKFLOW MODELS ====================
    [Description("Integrated payment workflow request")]
    public class IntegratedPaymentWorkflowRequest
    {
        public string WorkflowId { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public List<PaymentRequest> PaymentRequests { get; set; } = new();
        public bool IncludeSecurityValidation { get; set; } = true;
        public bool IncludeAnalytics { get; set; } = true;
        public bool IncludeReconciliation { get; set; } = true;
        public WorkflowSettings Settings { get; set; } = new();
        public List<string> NotificationRecipients { get; set; } = new();
        public Dictionary<string, object> CustomParameters { get; set; } = new();
    }

    [Description("Workflow settings")]
    public class WorkflowSettings
    {
        public bool AutoProcessPayments { get; set; } = true;
        public bool RequireApproval { get; set; } = false;
        public decimal ApprovalThreshold { get; set; } = 10000m;
        public bool GenerateRealTimeReports { get; set; } = true;
        public bool SendNotifications { get; set; } = true;
        public string DefaultCurrency { get; set; } = "USD";
        public int BatchSize { get; set; } = 100;
        public TimeSpan ProcessingTimeout { get; set; } = TimeSpan.FromMinutes(30);
        public bool EnableRollback { get; set; } = true;
        public Dictionary<string, object> AdditionalSettings { get; set; } = new();
    }

    [Description("Payment request")]
    public class PaymentRequest
    {
        public string RequestId { get; set; } = string.Empty;
        public string BusinessEntityCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime PaymentDate { get; set; }
        public string PaymentMethodCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public PaymentType PaymentType { get; set; }
        public List<string> DocumentReferences { get; set; } = new();
        public Dictionary<string, object> CustomFields { get; set; } = new();
        public PaymentPriority Priority { get; set; } = PaymentPriority.Normal;
    }

    [Description("Integrated payment workflow result")]
    public class IntegratedPaymentWorkflowResult
    {
        public string WorkflowId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public IntegratedPaymentWorkflowRequest Request { get; set; } = new();
        public List<PaymentProcessingResult> ProcessedPayments { get; set; } = new();
        public List<PaymentSecurityValidation> SecurityResults { get; set; } = new();
        public PaymentAnalyticsReport AnalyticsResults { get; set; } = new();
        public List<BankReconciliation> ReconciliationResults { get; set; } = new();
        public WorkflowStatus OverallStatus { get; set; }
        public List<WorkflowError> ProcessingErrors { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
        public WorkflowMetrics Metrics { get; set; } = new();
        public List<string> GeneratedReports { get; set; } = new();
    }

    [Description("Payment processing result")]
    public class PaymentProcessingResult
    {
        public string RequestId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public PaymentStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> GeneratedDocuments { get; set; } = new();
        public PaymentValidationResult ValidationResult { get; set; } = new();
        public decimal ProcessingFee { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public Dictionary<string, object> ProcessingMetadata { get; set; } = new();
    }

    [Description("Payment validation result")]
    public class PaymentValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
        public List<ValidationWarning> Warnings { get; set; } = new();
        public decimal RiskScore { get; set; }
        public string ValidationLevel { get; set; } = string.Empty;
        public DateTime ValidatedAt { get; set; }
        public string ValidatedBy { get; set; } = string.Empty;
    }

    [Description("Validation error")]
    public class ValidationError
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error";
        public string? SuggestedFix { get; set; }
    }

    [Description("Validation warning")]
    public class ValidationWarning
    {
        public string WarningCode { get; set; } = string.Empty;
        public string WarningMessage { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
        public bool CanProceed { get; set; } = true;
    }

    [Description("Workflow error")]
    public class WorkflowError
    {
        public string ErrorId { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;
        public string? PaymentId { get; set; }
        public string Severity { get; set; } = string.Empty;
        public bool IsRecoverable { get; set; }
        public string? StackTrace { get; set; }
        public Dictionary<string, object> Context { get; set; } = new();
    }

    [Description("Workflow metrics")]
    public class WorkflowMetrics
    {
        public int TotalPaymentsRequested { get; set; }
        public int SuccessfullyProcessed { get; set; }
        public int Failed { get; set; }
        public int RequiringApproval { get; set; }
        public decimal TotalAmountProcessed { get; set; }
        public decimal TotalFees { get; set; }
        public TimeSpan AverageProcessingTime { get; set; }
        public double SuccessRate { get; set; }
        public int SecurityViolations { get; set; }
        public int ComplianceFlags { get; set; }
        public WorkflowPerformanceMetrics Performance { get; set; } = new();
    }

    [Description("Workflow performance metrics")]
    public class WorkflowPerformanceMetrics
    {
        public TimeSpan SecurityValidationTime { get; set; }
        public TimeSpan PaymentProcessingTime { get; set; }
        public TimeSpan AnalyticsGenerationTime { get; set; }
        public TimeSpan ReconciliationTime { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public double ThroughputPerMinute { get; set; }
        public int PeakConcurrentOperations { get; set; }
        public double ResourceUtilization { get; set; }
        public List<PerformanceBottleneck> Bottlenecks { get; set; } = new();
    }

    [Description("Performance bottleneck")]
    public class PerformanceBottleneck
    {
        public string Component { get; set; } = string.Empty;
        public string BottleneckType { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string Impact { get; set; } = string.Empty;
        public string? Recommendation { get; set; }
    }

    // ==================== PAYMENT BATCH PROCESSING MODELS ====================
    [Description("Payment batch request")]
    public class PaymentBatchRequest
    {
        public string BatchId { get; set; } = string.Empty;
        public string BatchName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public List<PaymentRequest> Payments { get; set; } = new();
        public BatchProcessingSettings Settings { get; set; } = new();
        public string? ScheduledExecutionTime { get; set; }
        public BatchPriority Priority { get; set; } = BatchPriority.Normal;
        public List<string> ApprovalRequired { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    [Description("Batch processing settings")]
    public class BatchProcessingSettings
    {
        public int ConcurrentProcessingLimit { get; set; } = 10;
        public bool StopOnFirstError { get; set; } = false;
        public bool RequireIndividualApproval { get; set; } = false;
        public decimal IndividualApprovalThreshold { get; set; } = 10000m;
        public bool GenerateDetailedReports { get; set; } = true;
        public bool SendProgressNotifications { get; set; } = true;
        public TimeSpan ProcessingTimeout { get; set; } = TimeSpan.FromHours(2);
        public bool EnableRetryLogic { get; set; } = true;
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(5);
    }

    [Description("Payment batch result")]
    public class PaymentBatchResult
    {
        public string BatchId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public BatchStatus Status { get; set; }
        public List<PaymentProcessingResult> Results { get; set; } = new();
        public BatchProcessingSummary Summary { get; set; } = new();
        public List<BatchError> Errors { get; set; } = new();
        public TimeSpan TotalProcessingTime { get; set; }
        public string? CompletionReport { get; set; }
        public List<string> NotificationsSent { get; set; } = new();
    }

    [Description("Batch processing summary")]
    public class BatchProcessingSummary
    {
        public int TotalPayments { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public int Pending { get; set; }
        public int RequiringApproval { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessfulAmount { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan AverageProcessingTime { get; set; }
    }

    [Description("Batch error")]
    public class BatchError
    {
        public string ErrorId { get; set; } = string.Empty;
        public string PaymentRequestId { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsRecoverable { get; set; }
        public int RetryAttempts { get; set; }
        public string? Resolution { get; set; }
    }

    // ==================== ENUMS ====================
    public enum PaymentType
    {
        Sale,
        Purchase,
        Expense,
        Refund,
        Transfer,
        Adjustment,
        Fee,
        Interest,
        Dividend,
        Other
    }

    public enum PaymentPriority
    {
        Low,
        Normal,
        High,
        Urgent,
        Critical
    }

    public enum BatchPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    public enum BatchStatus
    {
        Pending,
        InProgress,
        Completed,
        PartiallyCompleted,
        Failed,
        Cancelled,
        RequiresApproval
    }
}
