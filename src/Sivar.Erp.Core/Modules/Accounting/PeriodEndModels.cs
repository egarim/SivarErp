using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Result of period-end processing operations
    /// </summary>
    [Description("Period-end processing result")]
    public class PeriodEndResult
    {
        public DateOnly PeriodEndDate { get; set; }
        public string ProcessType { get; set; } = string.Empty; // Month-End, Quarter-End, Year-End
        public string ProcessedBy { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<ProcessingStep> Steps { get; set; } = new();
        public TimeSpan ProcessingTime => CompletedAt.HasValue && StartedAt.HasValue ? 
            CompletedAt.Value - StartedAt.Value : TimeSpan.Zero;
    }

    /// <summary>
    /// Individual step in period-end processing
    /// </summary>
    [Description("Processing step")]
    public class ProcessingStep
    {
        public string StepName { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public TimeSpan Duration => CompletedAt.HasValue && StartedAt.HasValue ? 
            CompletedAt.Value - StartedAt.Value : TimeSpan.Zero;
    }

    /// <summary>
    /// Result of accrual processing
    /// </summary>
    [Description("Accrual processing result")]
    public class AccrualResult
    {
        public DateOnly PeriodEndDate { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<AccrualEntry> AccrualEntries { get; set; } = new();
        public decimal TotalAccrualAmount { get; set; }
    }

    /// <summary>
    /// Accrual entry details
    /// </summary>
    [Description("Accrual entry")]
    public class AccrualEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AccountCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateOnly TransactionDate { get; set; }
        public string AccrualType { get; set; } = string.Empty; // Revenue, Expense, Prepaid, Deferred
        public bool IsReversal { get; set; }
        public string OriginalEntryId { get; set; } = string.Empty;
        public string JournalEntryId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validation result for account balances and data integrity
    /// </summary>
    [Description("Validation result")]
    public class ValidationResult
    {
        public DateOnly ValidationDate { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public bool IsValid { get; set; }
        public List<ValidationError> ValidationErrors { get; set; } = new();
        public string Summary => IsValid ? "All validations passed" : $"{ValidationErrors.Count} validation errors found";
    }

    /// <summary>
    /// Individual validation error
    /// </summary>
    [Description("Validation error")]
    public class ValidationError
    {
        public string ErrorType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Critical, Warning, Info
        public string AccountCode { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of depreciation processing
    /// </summary>
    [Description("Depreciation processing result")]
    public class DepreciationResult
    {
        public DateOnly PeriodEndDate { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<DepreciationEntry> DepreciationEntries { get; set; } = new();
        public decimal TotalDepreciationAmount { get; set; }
    }

    /// <summary>
    /// Depreciation entry details
    /// </summary>
    [Description("Depreciation entry")]
    public class DepreciationEntry
    {
        public string AssetId { get; set; } = string.Empty;
        public string AssetDescription { get; set; } = string.Empty;
        public decimal DepreciationAmount { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public decimal BookValue { get; set; }
        public DateOnly TransactionDate { get; set; }
        public string DepreciationMethod { get; set; } = string.Empty;
        public string JournalEntryId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Depreciable asset information
    /// </summary>
    [Description("Depreciable asset")]
    public class DepreciableAsset
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CostBasis { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public decimal SalvageValue { get; set; }
        public int UsefulLifeMonths { get; set; }
        public DateOnly AcquisitionDate { get; set; }
        public string DepreciationMethod { get; set; } = "Straight-Line";
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Result of currency revaluation processing
    /// </summary>
    [Description("Currency revaluation result")]
    public class RevaluationResult
    {
        public DateOnly RevaluationDate { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<CurrencyRevaluationEntry> RevaluationEntries { get; set; } = new();
        public decimal NetRevaluationAdjustment { get; set; }
    }

    /// <summary>
    /// Currency revaluation entry
    /// </summary>
    [Description("Currency revaluation entry")]
    public class CurrencyRevaluationEntry
    {
        public string AccountCode { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal PreviousRate { get; set; }
        public decimal CurrentRate { get; set; }
        public decimal ForeignCurrencyBalance { get; set; }
        public decimal RevaluationAdjustment { get; set; }
        public DateOnly TransactionDate { get; set; }
        public string JournalEntryId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Foreign currency account information
    /// </summary>
    [Description("Foreign currency account")]
    public class ForeignCurrencyAccount
    {
        public string OfficialCode { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal OriginalRate { get; set; }
        public decimal? LastRevaluationRate { get; set; }
        public DateOnly? LastRevaluationDate { get; set; }
    }

    /// <summary>
    /// Audit trail entry for period-end processing
    /// </summary>
    [Description("Period-end audit entry")]
    public class PeriodEndAuditEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateOnly PeriodEndDate { get; set; }
        public string ProcessType { get; set; } = string.Empty;
        public string ProcessedBy { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; }
        public string ErrorDetails { get; set; } = string.Empty;
    }

    /// <summary>
    /// Period status information
    /// </summary>
    [Description("Period status")]
    public class PeriodStatus
    {
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public string PeriodType { get; set; } = string.Empty; // Month, Quarter, Year
        public bool IsOpen { get; set; } = true;
        public bool IsClosed { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string ClosedBy { get; set; } = string.Empty;
        public bool HasAdjustments { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalTransactionAmount { get; set; }
    }

    /// <summary>
    /// Financial period configuration
    /// </summary>
    [Description("Financial period configuration")]
    public class FinancialPeriodConfig
    {
        public DateOnly FiscalYearStart { get; set; }
        public DateOnly FiscalYearEnd { get; set; }
        public int NumberOfPeriods { get; set; } = 12;
        public string PeriodType { get; set; } = "Monthly";
        public bool AutoCreatePeriods { get; set; } = true;
        public bool AllowFuturePeriodPosting { get; set; }
        public int MaxFuturePeriods { get; set; } = 1;
        public bool RequireApprovalForPeriodClose { get; set; }
    }

    /// <summary>
    /// Closing checklist item
    /// </summary>
    [Description("Period closing checklist item")]
    public class ClosingChecklistItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string CompletedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Period close approval
    /// </summary>
    [Description("Period close approval")]
    public class PeriodCloseApproval
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateOnly PeriodEndDate { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string ApprovalStatus { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public string ApprovalComments { get; set; } = string.Empty;
        public List<ClosingChecklistItem> ChecklistItems { get; set; } = new();
    }
}
