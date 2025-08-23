using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Taxes.Models
{
    /// <summary>
    /// Parameters for tax compliance checks
    /// </summary>
    [Description("Parameters for tax compliance checks")]
    public class TaxComplianceCheckParameters : IEntity
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
        /// Gets or sets the entity types to check
        /// </summary>
        [Description("Entity types to check")]
        public List<string> EntityTypes { get; set; } = new();

        /// <summary>
        /// Gets or sets the check types
        /// </summary>
        [Description("Check types")]
        public List<TaxComplianceCheckType> CheckTypes { get; set; } = new();

        /// <summary>
        /// Gets or sets the severity threshold
        /// </summary>
        [Description("Severity threshold")]
        public ViolationSeverity SeverityThreshold { get; set; } = ViolationSeverity.Low;

        /// <summary>
        /// Gets or sets whether to include resolved violations
        /// </summary>
        [Description("Whether to include resolved violations")]
        public bool IncludeResolved { get; set; } = false;

        /// <summary>
        /// Gets or sets additional filters
        /// </summary>
        [Description("Additional filters")]
        public Dictionary<string, object> Filters { get; set; } = new();
    }

    /// <summary>
    /// Parameters for tax data integrity checks
    /// </summary>
    [Description("Parameters for tax data integrity checks")]
    public class TaxDataIntegrityCheckParameters : IEntity
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
        /// Gets or sets the check scope
        /// </summary>
        [Description("Check scope")]
        public TaxDataIntegrityScope Scope { get; set; } = TaxDataIntegrityScope.All;

        /// <summary>
        /// Gets or sets the maximum issues to return
        /// </summary>
        [Description("Maximum issues to return")]
        public int MaxIssues { get; set; } = 1000;

        /// <summary>
        /// Gets or sets whether to perform deep checks
        /// </summary>
        [Description("Whether to perform deep checks")]
        public bool PerformDeepChecks { get; set; } = false;

        /// <summary>
        /// Gets or sets the specific entity IDs to check
        /// </summary>
        [Description("Specific entity IDs to check")]
        public List<Guid> EntityIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the check options
        /// </summary>
        [Description("Check options")]
        public Dictionary<string, object> Options { get; set; } = new();
    }

    /// <summary>
    /// Query parameters for tax audit trail
    /// </summary>
    [Description("Query parameters for tax audit trail")]
    public class TaxAuditTrailQuery : IEntity
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
        /// Gets or sets the entity ID
        /// </summary>
        [Description("Entity ID")]
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity type
        /// </summary>
        [Description("Entity type")]
        public string? EntityType { get; set; }

        /// <summary>
        /// Gets or sets the start date
        /// </summary>
        [Description("Start date")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date
        /// </summary>
        [Description("End date")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        [Description("User ID")]
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the event types
        /// </summary>
        [Description("Event types")]
        public List<TaxAuditEventType> EventTypes { get; set; } = new();

        /// <summary>
        /// Gets or sets the page number
        /// </summary>
        [Description("Page number")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        [Description("Page size")]
        public int PageSize { get; set; } = 50;
    }

    /// <summary>
    /// Request for creating or updating tax rule sets
    /// </summary>
    [Description("Request for creating or updating tax rule sets")]
    public class TaxRuleSetRequest : IEntity
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
        public List<TaxRuleConditionRequest> Conditions { get; set; } = new();

        /// <summary>
        /// Gets or sets the actions
        /// </summary>
        [Description("Actions")]
        public List<TaxRuleActionRequest> Actions { get; set; } = new();

        /// <summary>
        /// Gets or sets the requested by user ID
        /// </summary>
        [Description("Requested by user ID")]
        public string RequestedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request for creating or updating tax rules
    /// </summary>
    [Description("Request for creating or updating tax rules")]
    public class TaxRuleRequest : IEntity
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
        /// Gets or sets the rule set ID
        /// </summary>
        [Description("Rule set ID")]
        public Guid RuleSetId { get; set; }

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
        /// Gets or sets the priority
        /// </summary>
        [Description("Priority")]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the condition
        /// </summary>
        [Description("Condition")]
        public TaxRuleConditionRequest Condition { get; set; } = new();

        /// <summary>
        /// Gets or sets the action
        /// </summary>
        [Description("Action")]
        public TaxRuleActionRequest Action { get; set; } = new();

        /// <summary>
        /// Gets or sets whether the rule is active
        /// </summary>
        [Description("Whether the rule is active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the requested by user ID
        /// </summary>
        [Description("Requested by user ID")]
        public string RequestedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Context for tax rule evaluation
    /// </summary>
    [Description("Context for tax rule evaluation")]
    public class TaxRuleEvaluationContext : IEntity
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
        /// Gets or sets the document
        /// </summary>
        [Description("Document")]
        public object? Document { get; set; }

        /// <summary>
        /// Gets or sets the business entity
        /// </summary>
        [Description("Business entity")]
        public object? BusinessEntity { get; set; }

        /// <summary>
        /// Gets or sets the operation type
        /// </summary>
        [Description("Operation type")]
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the evaluation date
        /// </summary>
        [Description("Evaluation date")]
        public DateTime EvaluationDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        [Description("User ID")]
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the context properties
        /// </summary>
        [Description("Context properties")]
        public Dictionary<string, object> Properties { get; set; } = new();

        /// <summary>
        /// Gets or sets the configuration
        /// </summary>
        [Description("Configuration")]
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    // Supporting enums and classes

    /// <summary>
    /// Tax compliance check types
    /// </summary>
    [Description("Tax compliance check types")]
    public enum TaxComplianceCheckType
    {
        [Description("Rate Validation")]
        RateValidation,
        [Description("Exemption Validation")]
        ExemptionValidation,
        [Description("Calculation Accuracy")]
        CalculationAccuracy,
        [Description("Configuration Consistency")]
        ConfigurationConsistency,
        [Description("Data Completeness")]
        DataCompleteness,
        [Description("Business Rule Compliance")]
        BusinessRuleCompliance
    }

    /// <summary>
    /// Tax data integrity scope
    /// </summary>
    [Description("Tax data integrity scope")]
    public enum TaxDataIntegrityScope
    {
        [Description("All")]
        All,
        [Description("Calculations")]
        Calculations,
        [Description("Configurations")]
        Configurations,
        [Description("Documents")]
        Documents,
        [Description("Business Entities")]
        BusinessEntities,
        [Description("Audit Trail")]
        AuditTrail
    }

    /// <summary>
    /// Tax data integrity issue types
    /// </summary>
    [Description("Tax data integrity issue types")]
    public enum TaxDataIntegrityIssueType
    {
        [Description("Missing Data")]
        MissingData,
        [Description("Invalid Data")]
        InvalidData,
        [Description("Inconsistent Data")]
        InconsistentData,
        [Description("Orphaned Record")]
        OrphanedRecord,
        [Description("Duplicate Record")]
        DuplicateRecord,
        [Description("Calculation Mismatch")]
        CalculationMismatch,
        [Description("Configuration Error")]
        ConfigurationError
    }

    // Supporting request classes
    public class TaxRuleConditionRequest
    {
        public string Property { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string? LogicalOperator { get; set; }
    }

    public class TaxRuleActionRequest
    {
        public string ActionType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}
