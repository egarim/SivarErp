using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Tax rules and audit models
    /// </summary>

    /// <summary>
    /// Tax rule
    /// </summary>
    [Description("Tax rule")]
    public class TaxRule
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsActive { get; set; } = true;
        public DocumentOperation DocumentOperation { get; set; }
        public string? BusinessEntityGroupId { get; set; }
        public string? ItemGroupId { get; set; }
        public string? CountryCode { get; set; }
        public string? RegionCode { get; set; }
        public TaxRuleCondition Conditions { get; set; } = new();
        public List<TaxRuleAction> Actions { get; set; } = new();
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    /// <summary>
    /// Tax rule condition
    /// </summary>
    [Description("Tax rule condition")]
    public class TaxRuleCondition
    {
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public List<string> RequiredItemCodes { get; set; } = new();
        public List<string> ExcludedItemCodes { get; set; } = new();
        public List<string> RequiredEntityGroups { get; set; } = new();
        public List<string> ExcludedEntityGroups { get; set; } = new();
        public string? CustomExpression { get; set; }
        public TaxRuleLogicalOperator LogicalOperator { get; set; } = TaxRuleLogicalOperator.And;
    }

    /// <summary>
    /// Tax rule action
    /// </summary>
    [Description("Tax rule action")]
    public class TaxRuleAction
    {
        public TaxRuleActionType ActionType { get; set; }
        public string TaxCode { get; set; } = string.Empty;
        public decimal? OverrideRate { get; set; }
        public bool ApplyTax { get; set; } = true;
        public string? ExemptionReason { get; set; }
    }

    /// <summary>
    /// Tax rule filter
    /// </summary>
    [Description("Tax rule filter")]
    public class TaxRuleFilter
    {
        public string? Name { get; set; }
        public DocumentOperation? DocumentOperation { get; set; }
        public bool? IsActive { get; set; }
        public string? BusinessEntityGroupId { get; set; }
        public string? ItemGroupId { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }

    /// <summary>
    /// Tax rule evaluation result
    /// </summary>
    [Description("Tax rule evaluation result")]
    public class TaxRuleEvaluationResult
    {
        public Guid DocumentId { get; set; }
        public List<ApplicableTaxRule> ApplicableRules { get; set; } = new();
        public List<string> ApplicableTaxCodes { get; set; } = new();
        public List<TaxExemptionResult> Exemptions { get; set; } = new();
        public List<TaxRateOverride> RateOverrides { get; set; } = new();
        public DateTime EvaluatedAt { get; set; }
        public TimeSpan EvaluationDuration { get; set; }
    }

    /// <summary>
    /// Applicable tax rule
    /// </summary>
    [Description("Applicable tax rule")]
    public class ApplicableTaxRule
    {
        public Guid RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool WasApplied { get; set; }
        public string? SkipReason { get; set; }
    }

    /// <summary>
    /// Tax exemption result
    /// </summary>
    [Description("Tax exemption result")]
    public class TaxExemptionResult
    {
        public string TaxCode { get; set; } = string.Empty;
        public TaxExemptionType ExemptionType { get; set; }
        public string ExemptionReason { get; set; } = string.Empty;
        public bool IsApplicable { get; set; }
    }

    /// <summary>
    /// Tax rate override
    /// </summary>
    [Description("Tax rate override")]
    public class TaxRateOverride
    {
        public string TaxCode { get; set; } = string.Empty;
        public decimal OriginalRate { get; set; }
        public decimal OverrideRate { get; set; }
        public string OverrideReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax rule test data
    /// </summary>
    [Description("Tax rule test data")]
    public class TaxRuleTestData
    {
        public DocumentOperation DocumentOperation { get; set; }
        public decimal Amount { get; set; }
        public List<string> ItemCodes { get; set; } = new();
        public List<string> EntityGroups { get; set; } = new();
        public string? CountryCode { get; set; }
        public string? RegionCode { get; set; }
        public Dictionary<string, object> CustomProperties { get; set; } = new();
    }

    /// <summary>
    /// Tax rule test result
    /// </summary>
    [Description("Tax rule test result")]
    public class TaxRuleTestResult
    {
        public bool RuleMatched { get; set; }
        public string? MatchReason { get; set; }
        public string? NoMatchReason { get; set; }
        public List<TaxRuleAction> ActionsToExecute { get; set; } = new();
        public Dictionary<string, object> EvaluationDetails { get; set; } = new();
    }

    /// <summary>
    /// Tax rule evaluation history
    /// </summary>
    [Description("Tax rule evaluation history")]
    public class TaxRuleEvaluationHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RuleId { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime EvaluatedAt { get; set; }
        public bool RuleMatched { get; set; }
        public bool WasApplied { get; set; }
        public string? SkipReason { get; set; }
        public TimeSpan EvaluationDuration { get; set; }
        public Dictionary<string, object> EvaluationContext { get; set; } = new();
    }

    /// <summary>
    /// Tax calculation audit
    /// </summary>
    [Description("Tax calculation audit")]
    public class TaxCalculationAudit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public Guid? BusinessEntityId { get; set; }
        public string TaxCode { get; set; } = string.Empty;
        public decimal BaseAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal CalculatedAmount { get; set; }
        public string CalculationMethod { get; set; } = string.Empty;
        public DateTime CalculatedAt { get; set; }
        public string CalculatedBy { get; set; } = string.Empty;
        public string? CalculationNotes { get; set; }
        public Dictionary<string, object> CalculationInputs { get; set; } = new();
    }

    /// <summary>
    /// Tax change audit
    /// </summary>
    [Description("Tax change audit")]
    public class TaxChangeAudit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TaxCode { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? ChangeReason { get; set; }
        public string? ApprovalStatus { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    /// <summary>
    /// Tax audit report parameters
    /// </summary>
    [Description("Tax audit report parameters")]
    public class TaxAuditReportParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? TaxCode { get; set; }
        public Guid? BusinessEntityId { get; set; }
        public Guid? DocumentId { get; set; }
        public string? AuditType { get; set; }
        public bool IncludeSystemChanges { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax audit report
    /// </summary>
    [Description("Tax audit report")]
    public class TaxAuditReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public TaxAuditReportParameters Parameters { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public List<TaxCalculationAudit> CalculationAudits { get; set; } = new();
        public List<TaxChangeAudit> ChangeAudits { get; set; } = new();
        public TaxAuditSummary Summary { get; set; } = new();
    }

    /// <summary>
    /// Tax audit summary
    /// </summary>
    [Description("Tax audit summary")]
    public class TaxAuditSummary
    {
        public int TotalCalculations { get; set; }
        public int TotalChanges { get; set; }
        public Dictionary<string, int> CalculationsByTax { get; set; } = new();
        public Dictionary<string, int> ChangesByType { get; set; } = new();
        public List<string> MostActiveUsers { get; set; } = new();
        public List<string> MostChangedTaxes { get; set; } = new();
    }

    /// <summary>
    /// Tax integrity check result
    /// </summary>
    [Description("Tax integrity check result")]
    public class TaxIntegrityCheckResult
    {
        public DateTime CheckedAt { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<TaxIntegrityIssue> Issues { get; set; } = new();
        public TaxIntegrityStatistics Statistics { get; set; } = new();
        public bool IsIntegrityOk { get; set; }
    }

    /// <summary>
    /// Tax integrity issue
    /// </summary>
    [Description("Tax integrity issue")]
    public class TaxIntegrityIssue
    {
        public string IssueType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public decimal ExpectedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax integrity statistics
    /// </summary>
    [Description("Tax integrity statistics")]
    public class TaxIntegrityStatistics
    {
        public int TotalDocumentsChecked { get; set; }
        public int DocumentsWithIssues { get; set; }
        public int TotalIssues { get; set; }
        public decimal IntegrityScore { get; set; }
        public Dictionary<string, int> IssuesByType { get; set; } = new();
        public Dictionary<string, int> IssuesBySeverity { get; set; } = new();
    }

    /// <summary>
    /// Suspicious tax pattern
    /// </summary>
    [Description("Suspicious tax pattern")]
    public class SuspiciousTaxPattern
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PatternType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public List<Guid> AffectedDocuments { get; set; } = new();
        public Guid? BusinessEntityId { get; set; }
        public string? BusinessEntityName { get; set; }
        public DateTime DetectedAt { get; set; }
        public Dictionary<string, object> PatternData { get; set; } = new();
        public string? InvestigationNotes { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// Suspicious tax pattern filter
    /// </summary>
    [Description("Suspicious tax pattern filter")]
    public class SuspiciousTaxPatternFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PatternType { get; set; }
        public string? RiskLevel { get; set; }
        public Guid? BusinessEntityId { get; set; }
        public string? Status { get; set; }
        public int? MinDocumentCount { get; set; }
    }

    /// <summary>
    /// Enumerations for tax rules and audit
    /// </summary>
    public enum TaxRuleLogicalOperator
    {
        And,
        Or,
        Not
    }

    public enum TaxRuleActionType
    {
        ApplyTax,
        ExemptTax,
        OverrideRate,
        SkipTax
    }
}
