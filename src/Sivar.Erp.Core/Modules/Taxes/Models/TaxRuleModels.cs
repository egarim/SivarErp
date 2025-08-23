using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Taxes.Models
{
    /// <summary>
    /// Tax rule operator types
    /// </summary>
    [Description("Tax rule operator types")]
    public enum TaxRuleOperator
    {
        [Description("Equals")]
        Equals,
        [Description("Not Equals")]
        NotEquals,
        [Description("Greater Than")]
        GreaterThan,
        [Description("Greater Than or Equal")]
        GreaterThanOrEqual,
        [Description("Less Than")]
        LessThan,
        [Description("Less Than or Equal")]
        LessThanOrEqual,
        [Description("Contains")]
        Contains,
        [Description("Starts With")]
        StartsWith,
        [Description("Ends With")]
        EndsWith,
        [Description("In")]
        In,
        [Description("Not In")]
        NotIn,
        [Description("Between")]
        Between,
        [Description("Is Null")]
        IsNull,
        [Description("Is Not Null")]
        IsNotNull,
        [Description("Matches Regex")]
        MatchesRegex
    }

    /// <summary>
    /// Tax rule set evaluation result
    /// </summary>
    [Description("Tax rule set evaluation result")]
    public class TaxRuleSetEvaluationResult : IEntity
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
        /// Gets or sets the rule set name
        /// </summary>
        [Description("Rule set name")]
        public string RuleSetName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the evaluation was successful
        /// </summary>
        [Description("Whether the evaluation was successful")]
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the evaluation timestamp
        /// </summary>
        [Description("Evaluation timestamp")]
        public DateTime EvaluationTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the execution time
        /// </summary>
        [Description("Execution time")]
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the rule results
        /// </summary>
        [Description("Rule results")]
        public List<TaxRuleEvaluationResult> RuleResults { get; set; } = new();

        /// <summary>
        /// Gets or sets the applied actions
        /// </summary>
        [Description("Applied actions")]
        public List<TaxRuleActionResult> AppliedActions { get; set; } = new();

        /// <summary>
        /// Gets or sets the errors
        /// </summary>
        [Description("Errors")]
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Gets or sets the metadata
        /// </summary>
        [Description("Metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Tax rule condition result
    /// </summary>
    [Description("Tax rule condition result")]
    public class TaxRuleConditionResult : IEntity
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
        /// Gets or sets the condition
        /// </summary>
        [Description("Condition")]
        public TaxRuleCondition Condition { get; set; } = new();

        /// <summary>
        /// Gets or sets whether the condition was met
        /// </summary>
        [Description("Whether the condition was met")]
        public bool IsMet { get; set; }

        /// <summary>
        /// Gets or sets the evaluated value
        /// </summary>
        [Description("Evaluated value")]
        public object? EvaluatedValue { get; set; }

        /// <summary>
        /// Gets or sets the expected value
        /// </summary>
        [Description("Expected value")]
        public object? ExpectedValue { get; set; }

        /// <summary>
        /// Gets or sets the evaluation details
        /// </summary>
        [Description("Evaluation details")]
        public string? EvaluationDetails { get; set; }

        /// <summary>
        /// Gets or sets the evaluation errors
        /// </summary>
        [Description("Evaluation errors")]
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Tax rule evaluation result
    /// </summary>
    [Description("Tax rule evaluation result")]
    public class TaxRuleEvaluationResult : IEntity
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
        /// Gets or sets whether the rule was applied
        /// </summary>
        [Description("Whether the rule was applied")]
        public bool WasApplied { get; set; }

        /// <summary>
        /// Gets or sets the condition results
        /// </summary>
        [Description("Condition results")]
        public List<TaxRuleConditionResult> ConditionResults { get; set; } = new();

        /// <summary>
        /// Gets or sets the action results
        /// </summary>
        [Description("Action results")]
        public List<TaxRuleActionResult> ActionResults { get; set; } = new();

        /// <summary>
        /// Gets or sets the execution time
        /// </summary>
        [Description("Execution time")]
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the errors
        /// </summary>
        [Description("Errors")]
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Tax rule action result
    /// </summary>
    [Description("Tax rule action result")]
    public class TaxRuleActionResult : IEntity
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
        /// Gets or sets the action type
        /// </summary>
        [Description("Action type")]
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the action was successful
        /// </summary>
        [Description("Whether the action was successful")]
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the result value
        /// </summary>
        [Description("Result value")]
        public object? ResultValue { get; set; }

        /// <summary>
        /// Gets or sets the execution time
        /// </summary>
        [Description("Execution time")]
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        [Description("Error message")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the details
        /// </summary>
        [Description("Details")]
        public Dictionary<string, object> Details { get; set; } = new();
    }

    /// <summary>
    /// Tax type enumeration
    /// </summary>
    [Description("Tax type enumeration")]
    public enum TaxType
    {
        [Description("Sales Tax")]
        SalesTax,
        [Description("VAT")]
        VAT,
        [Description("GST")]
        GST,
        [Description("Income Tax")]
        IncomeTax,
        [Description("Property Tax")]
        PropertyTax,
        [Description("Excise Tax")]
        ExciseTax,
        [Description("Import Duty")]
        ImportDuty,
        [Description("Export Duty")]
        ExportDuty,
        [Description("Withholding Tax")]
        WithholdingTax,
        [Description("Custom")]
        Custom
    }
}
