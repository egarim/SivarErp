using System.ComponentModel;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Taxes.Models
{
    /// <summary>
    /// Represents a tax audit event
    /// </summary>
    [Description("Represents a tax audit event")]
    public class TaxAuditEvent : IEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Description("Entity identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        [Description("Last update date")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the event type
        /// </summary>
        [Description("Event type")]
        public TaxAuditEventType EventType { get; set; }

        /// <summary>
        /// Gets or sets the entity ID
        /// </summary>
        [Description("Entity ID")]
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity type
        /// </summary>
        [Description("Entity type")]
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        [Description("Timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        [Description("User ID")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [Description("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the old values
        /// </summary>
        [Description("Old values")]
        public Dictionary<string, object> OldValues { get; set; } = new();

        /// <summary>
        /// Gets or sets the new values
        /// </summary>
        [Description("New values")]
        public Dictionary<string, object> NewValues { get; set; } = new();

        /// <summary>
        /// Gets or sets the metadata
        /// </summary>
        [Description("Metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a tax compliance violation
    /// </summary>
    [Description("Represents a tax compliance violation")]
    public class TaxComplianceViolation : IEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Description("Entity identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        [Description("Last update date")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the violation type
        /// </summary>
        [Description("Violation type")]
        public TaxViolationType ViolationType { get; set; }

        /// <summary>
        /// Gets or sets the severity
        /// </summary>
        [Description("Severity")]
        public ViolationSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the entity ID
        /// </summary>
        [Description("Entity ID")]
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity type
        /// </summary>
        [Description("Entity type")]
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [Description("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detected date
        /// </summary>
        [Description("Detected date")]
        public DateTime DetectedDate { get; set; }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        [Description("Status")]
        public ViolationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the resolution
        /// </summary>
        [Description("Resolution")]
        public string? Resolution { get; set; }

        /// <summary>
        /// Gets or sets the resolved date
        /// </summary>
        [Description("Resolved date")]
        public DateTime? ResolvedDate { get; set; }

        /// <summary>
        /// Gets or sets the resolved by user ID
        /// </summary>
        [Description("Resolved by user ID")]
        public string? ResolvedBy { get; set; }
    }

    /// <summary>
    /// Represents a tax rule set
    /// </summary>
    [Description("Represents a tax rule set")]
    public class TaxRuleSet : IEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Description("Entity identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        [Description("Last update date")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [Description("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [Description("Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
        [Description("Version")]
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Gets or sets whether the rule set is active
        /// </summary>
        [Description("Whether the rule set is active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the effective date
        /// </summary>
        [Description("Effective date")]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date
        /// </summary>
        [Description("Expiration date")]
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the priority
        /// </summary>
        [Description("Priority")]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the conditions
        /// </summary>
        [Description("Conditions")]
        public List<TaxRuleCondition> Conditions { get; set; } = new();

        /// <summary>
        /// Gets or sets the actions
        /// </summary>
        [Description("Actions")]
        public List<TaxRuleAction> Actions { get; set; } = new();

        /// <summary>
        /// Gets or sets the metadata
        /// </summary>
        [Description("Metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a tax data integrity report
    /// </summary>
    [Description("Represents a tax data integrity report")]
    public class TaxDataIntegrityReport : IEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Description("Entity identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        [Description("Last update date")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the report date
        /// </summary>
        [Description("Report date")]
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// Gets or sets the start date
        /// </summary>
        [Description("Start date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date
        /// </summary>
        [Description("End date")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the total records checked
        /// </summary>
        [Description("Total records checked")]
        public int TotalRecordsChecked { get; set; }

        /// <summary>
        /// Gets or sets the issues found
        /// </summary>
        [Description("Issues found")]
        public List<TaxDataIntegrityIssue> Issues { get; set; } = new();

        /// <summary>
        /// Gets or sets the summary
        /// </summary>
        [Description("Summary")]
        public TaxDataIntegritySummary Summary { get; set; } = new();
    }

    /// <summary>
    /// Represents a tax audit recommendation
    /// </summary>
    [Description("Represents a tax audit recommendation")]
    public class TaxAuditRecommendation : IEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Description("Entity identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        [Description("Last update date")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the recommendation type
        /// </summary>
        [Description("Recommendation type")]
        public RecommendationType Type { get; set; }

        /// <summary>
        /// Gets or sets the priority
        /// </summary>
        [Description("Priority")]
        public RecommendationPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        [Description("Title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [Description("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the recommendation
        /// </summary>
        [Description("Recommendation")]
        public string Recommendation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the impact
        /// </summary>
        [Description("Impact")]
        public string? Impact { get; set; }

        /// <summary>
        /// Gets or sets the estimated effort
        /// </summary>
        [Description("Estimated effort")]
        public string? EstimatedEffort { get; set; }

        /// <summary>
        /// Gets or sets the related entity ID
        /// </summary>
        [Description("Related entity ID")]
        public Guid? RelatedEntityId { get; set; }

        /// <summary>
        /// Gets or sets the related entity type
        /// </summary>
        [Description("Related entity type")]
        public string? RelatedEntityType { get; set; }
    }

    /// <summary>
    /// Represents a tax rule validation result
    /// </summary>
    [Description("Represents a tax rule validation result")]
    public class TaxRuleValidationResult : ValidationResult
    {
        /// <summary>
        /// Gets or sets the rule ID
        /// </summary>
        [Description("Rule ID")]
        public Guid RuleId { get; set; }

        /// <summary>
        /// Gets or sets the rule name
        /// </summary>
        [Description("Rule name")]
        public string RuleName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the validation issues
        /// </summary>
        [Description("Validation issues")]
        public List<TaxRuleValidationIssue> Issues { get; set; } = new();

        /// <summary>
        /// Gets or sets the performance metrics
        /// </summary>
        [Description("Performance metrics")]
        public TaxRulePerformanceMetrics? PerformanceMetrics { get; set; }
    }

    // Supporting classes and enums

    /// <summary>
    /// Tax audit event types
    /// </summary>
    [Description("Tax audit event types")]
    public enum TaxAuditEventType
    {
        [Description("Tax Calculated")]
        TaxCalculated,
        [Description("Tax Modified")]
        TaxModified,
        [Description("Tax Exemption Applied")]
        TaxExemptionApplied,
        [Description("Tax Rule Changed")]
        TaxRuleChanged,
        [Description("Document Created")]
        DocumentCreated,
        [Description("Document Modified")]
        DocumentModified,
        [Description("Configuration Changed")]
        ConfigurationChanged
    }

    /// <summary>
    /// Tax violation types
    /// </summary>
    [Description("Tax violation types")]
    public enum TaxViolationType
    {
        [Description("Missing Tax")]
        MissingTax,
        [Description("Incorrect Rate")]
        IncorrectRate,
        [Description("Invalid Exemption")]
        InvalidExemption,
        [Description("Calculation Error")]
        CalculationError,
        [Description("Configuration Issue")]
        ConfigurationIssue,
        [Description("Data Inconsistency")]
        DataInconsistency
    }

    /// <summary>
    /// Violation severity levels
    /// </summary>
    [Description("Violation severity levels")]
    public enum ViolationSeverity
    {
        [Description("Low")]
        Low,
        [Description("Medium")]
        Medium,
        [Description("High")]
        High,
        [Description("Critical")]
        Critical
    }

    /// <summary>
    /// Violation status
    /// </summary>
    [Description("Violation status")]
    public enum ViolationStatus
    {
        [Description("Open")]
        Open,
        [Description("In Progress")]
        InProgress,
        [Description("Resolved")]
        Resolved,
        [Description("Closed")]
        Closed,
        [Description("Ignored")]
        Ignored
    }

    /// <summary>
    /// Recommendation types
    /// </summary>
    [Description("Recommendation types")]
    public enum RecommendationType
    {
        [Description("Process Improvement")]
        ProcessImprovement,
        [Description("Configuration Change")]
        ConfigurationChange,
        [Description("Data Cleanup")]
        DataCleanup,
        [Description("Training")]
        Training,
        [Description("Policy Change")]
        PolicyChange
    }

    /// <summary>
    /// Recommendation priority levels
    /// </summary>
    [Description("Recommendation priority levels")]
    public enum RecommendationPriority
    {
        [Description("Low")]
        Low,
        [Description("Medium")]
        Medium,
        [Description("High")]
        High,
        [Description("Critical")]
        Critical
    }

    // Supporting classes for complex types
    public class TaxRuleCondition
    {
        public string Property { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string? LogicalOperator { get; set; }
    }

    public class TaxRuleAction
    {
        public string ActionType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class TaxDataIntegrityIssue
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string? EntityType { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    public class TaxDataIntegritySummary
    {
        public int TotalIssues { get; set; }
        public int CriticalIssues { get; set; }
        public int HighIssues { get; set; }
        public int MediumIssues { get; set; }
        public int LowIssues { get; set; }
        public Dictionary<string, int> IssuesByType { get; set; } = new();
    }

    public class TaxRuleValidationIssue
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string? Suggestion { get; set; }
    }

    public class TaxRulePerformanceMetrics
    {
        public TimeSpan ExecutionTime { get; set; }
        public int EvaluationCount { get; set; }
        public double AverageExecutionTime { get; set; }
        public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
    }
}
