using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Sivar.Erp.Modules.Payments.Models;

namespace Sivar.Erp.Modules.Payments
{
    // ==================== PAYMENT ANALYTICS SERVICE ====================
    /// <summary>
    /// Service for advanced payment analytics and financial intelligence
    /// </summary>
    [Description("Advanced payment analytics service")]
    public interface IPaymentAnalyticsService
    {
        /// <summary>
        /// Analyzes payment patterns and trends
        /// </summary>
        Task<PaymentAnalyticsReport> GetPaymentAnalyticsAsync(PaymentAnalyticsParameters parameters);

        /// <summary>
        /// Gets cash flow analysis and projections
        /// </summary>
        Task<CashFlowAnalysisReport> GetCashFlowAnalysisAsync(CashFlowAnalysisParameters parameters);

        /// <summary>
        /// Analyzes payment method performance and usage
        /// </summary>
        Task<PaymentMethodAnalysisReport> GetPaymentMethodAnalysisAsync(PaymentMethodAnalysisParameters parameters);

        /// <summary>
        /// Gets accounts receivable aging analysis
        /// </summary>
        Task<ARAgingReport> GetAccountsReceivableAgingAsync(ARAgingParameters parameters);

        /// <summary>
        /// Gets accounts payable aging analysis
        /// </summary>
        Task<APAgingReport> GetAccountsPayableAgingAsync(APAgingParameters parameters);

        /// <summary>
        /// Provides payment forecasting and predictions
        /// </summary>
        Task<PaymentForecastReport> GetPaymentForecastAsync(PaymentForecastParameters parameters);

        /// <summary>
        /// Calculates payment performance metrics and KPIs
        /// </summary>
        Task<PaymentPerformanceMetrics> GetPaymentPerformanceMetricsAsync(PaymentPerformanceParameters parameters);

        /// <summary>
        /// Provides real-time payment dashboard data
        /// </summary>
        Task<PaymentDashboard> GetPaymentDashboardAsync(PaymentDashboardParameters parameters);
    }

    // ==================== PAYMENT RECONCILIATION SERVICE ====================
    /// <summary>
    /// Service for payment reconciliation and matching
    /// </summary>
    [Description("Payment reconciliation service")]
    public interface IPaymentReconciliationService
    {
        /// <summary>
        /// Creates a new bank reconciliation
        /// </summary>
        Task<BankReconciliation> CreateBankReconciliationAsync(BankReconciliationRequest request);

        /// <summary>
        /// Automatically matches payments with bank transactions
        /// </summary>
        Task<PaymentMatchingResult> AutoMatchPaymentsAsync(AutoMatchingParameters parameters);

        /// <summary>
        /// Manually matches a payment with a bank transaction
        /// </summary>
        Task<PaymentMatchResult> ManualMatchPaymentAsync(string paymentId, string bankTransactionId, MatchingData matchingData);

        /// <summary>
        /// Gets unmatched payments requiring reconciliation
        /// </summary>
        Task<List<UnmatchedPayment>> GetUnmatchedPaymentsAsync(UnmatchedPaymentsQuery query);

        /// <summary>
        /// Gets bank statement import and processing
        /// </summary>
        Task<BankStatementImportResult> ImportBankStatementAsync(BankStatementImportRequest request);

        /// <summary>
        /// Generates reconciliation reports
        /// </summary>
        Task<ReconciliationReport> GenerateReconciliationReportAsync(ReconciliationReportParameters parameters);

        /// <summary>
        /// Processes reconciliation adjustments
        /// </summary>
        Task<ReconciliationAdjustmentResult> ProcessReconciliationAdjustmentAsync(ReconciliationAdjustment adjustment);

        /// <summary>
        /// Finalizes and closes a bank reconciliation
        /// </summary>
        Task<BankReconciliationResult> FinalizeBankReconciliationAsync(string reconciliationId, BankReconciliationFinalization finalization);
    }

    // ==================== PAYMENT SECURITY SERVICE ====================
    /// <summary>
    /// Service for payment security, fraud detection, and compliance
    /// </summary>
    [Description("Payment security and fraud detection service")]
    public interface IPaymentSecurityService
    {
        /// <summary>
        /// Validates payment security and compliance
        /// </summary>
        Task<PaymentSecurityValidation> ValidatePaymentSecurityAsync(PaymentSecurityRequest request);

        /// <summary>
        /// Detects potential fraudulent payment patterns
        /// </summary>
        Task<FraudDetectionResult> DetectFraudAsync(FraudDetectionParameters parameters);

        /// <summary>
        /// Manages payment authorization and approval workflows
        /// </summary>
        Task<PaymentAuthorizationResult> AuthorizePaymentAsync(string paymentId, PaymentAuthorizationData authorization);

        /// <summary>
        /// Gets compliance audit trails and reporting
        /// </summary>
        Task<PaymentComplianceReport> GetComplianceReportAsync(PaymentComplianceParameters parameters);

        /// <summary>
        /// Manages payment limits and controls
        /// </summary>
        Task<PaymentLimitsValidation> ValidatePaymentLimitsAsync(PaymentLimitsRequest request);

        /// <summary>
        /// Processes suspicious payment alerts and investigations
        /// </summary>
        Task<SuspiciousPaymentInvestigation> ProcessSuspiciousPaymentAsync(string paymentId, SuspiciousPaymentData suspiciousData);

        /// <summary>
        /// Generates security and compliance metrics
        /// </summary>
        Task<PaymentSecurityMetrics> GetSecurityMetricsAsync(PaymentSecurityMetricsParameters parameters);

        /// <summary>
        /// Manages payment encryption and security protocols
        /// </summary>
        Task<PaymentEncryptionResult> EncryptPaymentDataAsync(PaymentEncryptionRequest request);
    }

    // ==================== PARAMETER CLASSES ====================
    // Analytics Parameters
    public class PaymentAnalyticsParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? BusinessEntityCode { get; set; }
        public List<string> PaymentMethodCodes { get; set; } = new();
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public List<PaymentStatus> StatusFilter { get; set; } = new();
        public bool IncludePending { get; set; } = true;
        public PaymentAnalysisType AnalysisType { get; set; }
        public string GroupBy { get; set; } = "Month";
    }

    public class CashFlowAnalysisParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProjectionDays { get; set; } = 90;
        public List<string> AccountCodes { get; set; } = new();
        public bool IncludeProjections { get; set; } = true;
        public CashFlowAnalysisMethod Method { get; set; }
        public string Currency { get; set; } = "USD";
        public bool GroupByCategory { get; set; }
    }

    public class PaymentMethodAnalysisParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> PaymentMethodCodes { get; set; } = new();
        public bool IncludeInactive { get; set; }
        public bool AnalyzePerformance { get; set; } = true;
        public bool AnalyzeCosts { get; set; } = true;
        public string GroupBy { get; set; } = "PaymentMethod";
    }

    public class ARAgingParameters
    {
        public DateTime AsOfDate { get; set; }
        public List<string> CustomerCodes { get; set; } = new();
        public List<int> AgingBuckets { get; set; } = new() { 30, 60, 90, 120 };
        public bool IncludeZeroBalance { get; set; }
        public bool GroupByCustomer { get; set; } = true;
        public string Currency { get; set; } = "USD";
    }

    public class APAgingParameters
    {
        public DateTime AsOfDate { get; set; }
        public List<string> SupplierCodes { get; set; } = new();
        public List<int> AgingBuckets { get; set; } = new() { 30, 60, 90, 120 };
        public bool IncludeZeroBalance { get; set; }
        public bool GroupBySupplier { get; set; } = true;
        public string Currency { get; set; } = "USD";
    }

    public class PaymentForecastParameters
    {
        public DateTime StartDate { get; set; }
        public int ForecastDays { get; set; } = 90;
        public List<string> PaymentMethodCodes { get; set; } = new();
        public ForecastMethod Method { get; set; }
        public decimal ConfidenceLevel { get; set; } = 95m;
        public bool IncludeSeasonality { get; set; }
        public bool IncludeHistoricalPatterns { get; set; } = true;
    }

    public class PaymentPerformanceParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> MetricTypes { get; set; } = new();
        public bool IncludeBenchmarks { get; set; }
        public string? ComparisonPeriod { get; set; }
        public bool GroupByPeriod { get; set; }
    }

    public class PaymentDashboardParameters
    {
        public List<string> WidgetTypes { get; set; } = new();
        public DateTime? RefreshTime { get; set; }
        public bool IncludeAlerts { get; set; } = true;
        public string TimeZone { get; set; } = "UTC";
        public string Currency { get; set; } = "USD";
    }

    // Reconciliation Parameters
    public class BankReconciliationRequest
    {
        public string BankAccountCode { get; set; } = string.Empty;
        public DateTime StatementDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal StatementBalance { get; set; }
        public string StatementNumber { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public List<BankTransaction> BankTransactions { get; set; } = new();
    }

    public class AutoMatchingParameters
    {
        public string BankAccountCode { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal ToleranceAmount { get; set; } = 0.01m;
        public int ToleranceDays { get; set; } = 3;
        public bool UseAdvancedMatching { get; set; } = true;
        public List<MatchingRule> MatchingRules { get; set; } = new();
    }

    public class MatchingData
    {
        public string MatchingReason { get; set; } = string.Empty;
        public decimal? AmountDifference { get; set; }
        public int? DateDifference { get; set; }
        public string? Notes { get; set; }
        public string MatchedBy { get; set; } = string.Empty;
        public DateTime MatchedAt { get; set; }
    }

    public class UnmatchedPaymentsQuery
    {
        public string? BankAccountCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int MaxResults { get; set; } = 100;
        public bool IncludeMatchingSuggestions { get; set; } = true;
    }

    public class BankStatementImportRequest
    {
        public string BankAccountCode { get; set; } = string.Empty;
        public string FileContent { get; set; } = string.Empty;
        public BankStatementFormat Format { get; set; }
        public DateTime StatementDate { get; set; }
        public string ImportedBy { get; set; } = string.Empty;
        public Dictionary<string, object> ImportSettings { get; set; } = new();
    }

    public class ReconciliationReportParameters
    {
        public string ReconciliationId { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? BankAccountCode { get; set; }
        public bool IncludeDetails { get; set; } = true;
        public bool IncludeUnmatched { get; set; } = true;
    }

    public class ReconciliationAdjustment
    {
        public string ReconciliationId { get; set; } = string.Empty;
        public ReconciliationAdjustmentType Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public string AdjustedBy { get; set; } = string.Empty;
        public DateTime AdjustmentDate { get; set; }
        public string? Reference { get; set; }
    }

    public class BankReconciliationFinalization
    {
        public string FinalizedBy { get; set; } = string.Empty;
        public DateTime FinalizationDate { get; set; }
        public string? Notes { get; set; }
        public bool AutoPostAdjustments { get; set; } = true;
        public List<string> ApprovedAdjustmentIds { get; set; } = new();
    }

    // Security Parameters
    public class PaymentSecurityRequest
    {
        public string PaymentId { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public List<SecurityValidationType> ValidationTypes { get; set; } = new();
        public Dictionary<string, object> ValidationData { get; set; } = new();
    }

    public class FraudDetectionParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> PaymentIds { get; set; } = new();
        public List<FraudDetectionRule> Rules { get; set; } = new();
        public decimal SuspiciousAmountThreshold { get; set; } = 10000m;
        public bool IncludeMLAnalysis { get; set; } = true;
    }

    public class PaymentAuthorizationData
    {
        public string AuthorizerId { get; set; } = string.Empty;
        public string AuthorizationLevel { get; set; } = string.Empty;
        public DateTime AuthorizationDate { get; set; }
        public bool IsApproved { get; set; }
        public string? AuthorizationNotes { get; set; }
        public List<string> RequiredDocuments { get; set; } = new();
    }

    public class PaymentComplianceParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ComplianceType> ComplianceTypes { get; set; } = new();
        public string? JurisdictionCode { get; set; }
        public bool IncludeViolations { get; set; } = true;
        public bool IncludeRemediation { get; set; } = true;
    }

    public class PaymentLimitsRequest
    {
        public string PaymentId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethodCode { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public List<PaymentLimitType> LimitTypes { get; set; } = new();
    }

    public class SuspiciousPaymentData
    {
        public string ReportedBy { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public List<SuspiciousIndicator> Indicators { get; set; } = new();
        public string SuspiciousReason { get; set; } = string.Empty;
        public InvestigationPriority Priority { get; set; }
        public string? AdditionalInfo { get; set; }
    }

    public class PaymentSecurityMetricsParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<SecurityMetricType> MetricTypes { get; set; } = new();
        public bool IncludeTrends { get; set; } = true;
        public bool IncludeAlerts { get; set; } = true;
    }

    public class PaymentEncryptionRequest
    {
        public string PaymentId { get; set; } = string.Empty;
        public Dictionary<string, object> DataToEncrypt { get; set; } = new();
        public EncryptionMethod Method { get; set; }
        public string? KeyId { get; set; }
        public bool IncludeIntegrityCheck { get; set; } = true;
    }

    // ==================== ENUMS ====================
    public enum PaymentAnalysisType
    {
        Volume,
        Value,
        Frequency,
        Trend,
        Comparison
    }

    public enum CashFlowAnalysisMethod
    {
        Direct,
        Indirect,
        Predictive,
        Historical
    }

    public enum ForecastMethod
    {
        LinearRegression,
        MovingAverage,
        ExponentialSmoothing,
        SeasonalDecomposition,
        MachineLearning
    }

    public enum BankStatementFormat
    {
        CSV,
        Excel,
        QIF,
        OFX,
        MT940,
        Custom
    }

    public enum ReconciliationAdjustmentType
    {
        BankFee,
        Interest,
        OutstandingCheck,
        DepositInTransit,
        NSFCheck,
        Correction,
        Other
    }

    public enum SecurityValidationType
    {
        AmountLimit,
        DailyLimit,
        MonthlyLimit,
        Authorization,
        Compliance,
        FraudCheck,
        DuplicateCheck,
        BlacklistCheck
    }

    public enum ComplianceType
    {
        AML,
        KYC,
        GDPR,
        CCPA,
        SOX,
        PCI,
        Regulatory,
        Internal
    }

    public enum PaymentLimitType
    {
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Transaction,
        Cumulative
    }

    public enum InvestigationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum SecurityMetricType
    {
        FraudAttempts,
        AuthorizationFailures,
        ComplianceViolations,
        SecurityIncidents,
        EncryptionUsage,
        AccessViolations
    }

    public enum EncryptionMethod
    {
        AES256,
        RSA,
        ECDSA,
        Hybrid,
        TokenVault
    }

    public enum SuspiciousIndicator
    {
        UnusualAmount,
        UnusualFrequency,
        UnusualPattern,
        BlacklistedEntity,
        VelocityCheck,
        GeographicalAnomaly,
        TimeAnomaly,
        DuplicatePayment
    }

    // ==================== SUPPORTING CLASSES ====================
    public class BankTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal RunningBalance { get; set; }
        public bool IsMatched { get; set; }
        public string? MatchedPaymentId { get; set; }
    }

    public class MatchingRule
    {
        public string RuleName { get; set; } = string.Empty;
        public MatchingCriteria Criteria { get; set; } = new();
        public decimal Priority { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class MatchingCriteria
    {
        public bool MatchByAmount { get; set; } = true;
        public bool MatchByDate { get; set; } = true;
        public bool MatchByReference { get; set; } = true;
        public decimal AmountTolerance { get; set; } = 0.01m;
        public int DateToleranceDays { get; set; } = 3;
        public bool UsePartialMatching { get; set; }
    }

    public class FraudDetectionRule
    {
        public string RuleName { get; set; } = string.Empty;
        public FraudRuleType RuleType { get; set; }
        public Dictionary<string, object> RuleParameters { get; set; } = new();
        public decimal RiskWeight { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public enum FraudRuleType
    {
        AmountThreshold,
        VelocityCheck,
        PatternAnalysis,
        BlacklistCheck,
        GeolocationCheck,
        TimeAnalysis,
        DuplicateDetection,
        MLModel
    }
}
