using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Budget entity for financial planning
    /// </summary>
    [Description("Budget entity")]
    public class Budget
    {
        public string Id { get; set; } = string.Empty;
        public string BudgetName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public string BudgetType { get; set; } = string.Empty; // Operating, Capital, Cash Flow
        public BudgetStatus Status { get; set; } = BudgetStatus.Draft;
        public int Version { get; set; } = 1;
        public decimal TotalBudgetAmount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? ApprovalComments { get; set; }
        public List<BudgetLine> BudgetLines { get; set; } = new();
    }

    /// <summary>
    /// Budget status enumeration
    /// </summary>
    [Description("Budget status")]
    public enum BudgetStatus
    {
        Draft,
        Submitted,
        UnderReview,
        Approved,
        Rejected,
        Active,
        Closed
    }

    /// <summary>
    /// Individual budget line item
    /// </summary>
    [Description("Budget line item")]
    public class BudgetLine
    {
        public string Id { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<MonthlyBudgetAmount> MonthlyBreakdown { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Monthly budget amount breakdown
    /// </summary>
    [Description("Monthly budget amount")]
    public class MonthlyBudgetAmount
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
        public bool IsActual { get; set; }
        public bool IsForecast { get; set; }
    }

    /// <summary>
    /// Budget creation request
    /// </summary>
    [Description("Budget creation request")]
    public class BudgetRequest
    {
        public string BudgetName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public string BudgetType { get; set; } = string.Empty;
        public List<BudgetLineRequest> BudgetLines { get; set; } = new();
    }

    /// <summary>
    /// Budget line creation request
    /// </summary>
    [Description("Budget line request")]
    public class BudgetLineRequest
    {
        public string AccountCode { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    /// <summary>
    /// Budget update request
    /// </summary>
    [Description("Budget update request")]
    public class BudgetUpdateRequest
    {
        public string BudgetName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool CreateNewVersion { get; set; }
        public List<BudgetLineUpdate> LineUpdates { get; set; } = new();
    }

    /// <summary>
    /// Budget line update
    /// </summary>
    [Description("Budget line update")]
    public class BudgetLineUpdate
    {
        public string LineId { get; set; } = string.Empty;
        public decimal NewBudgetAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Budget analysis comparing budget vs actual
    /// </summary>
    [Description("Budget analysis")]
    public class BudgetAnalysis
    {
        public string BudgetId { get; set; } = string.Empty;
        public string BudgetName { get; set; } = string.Empty;
        public DateOnly AnalysisPeriodStart { get; set; }
        public DateOnly AnalysisPeriodEnd { get; set; }
        public DateTime GeneratedAt { get; set; }
        public decimal TotalBudgetAmount { get; set; }
        public decimal TotalActualAmount { get; set; }
        public decimal TotalVariance { get; set; }
        public decimal TotalVariancePercentage { get; set; }
        public List<AccountBudgetAnalysis> AccountAnalyses { get; set; } = new();
    }

    /// <summary>
    /// Account-level budget analysis
    /// </summary>
    [Description("Account budget analysis")]
    public class AccountBudgetAnalysis
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public bool IsFavorable { get; set; }
        public string VarianceCategory { get; set; } = string.Empty; // Significant, Normal, Minor
    }

    /// <summary>
    /// Financial forecast entity
    /// </summary>
    [Description("Financial forecast")]
    public class FinancialForecast
    {
        public string Id { get; set; } = string.Empty;
        public string ForecastName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateOnly ForecastPeriodStart { get; set; }
        public DateOnly ForecastPeriodEnd { get; set; }
        public DateOnly BasePeriodStart { get; set; }
        public DateOnly BasePeriodEnd { get; set; }
        public string ForecastMethod { get; set; } = string.Empty; // Linear, Exponential, Seasonal
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<ForecastLine> ForecastLines { get; set; } = new();
        public decimal Accuracy { get; set; } // Forecast accuracy percentage
    }

    /// <summary>
    /// Individual forecast line
    /// </summary>
    [Description("Forecast line")]
    public class ForecastLine
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal ForecastAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Confidence { get; set; } // Confidence level 0-100%
        public string ForecastMethod { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<MonthlyForecastAmount> MonthlyBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Monthly forecast amount
    /// </summary>
    [Description("Monthly forecast amount")]
    public class MonthlyForecastAmount
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal ForecastAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Confidence { get; set; }
    }

    /// <summary>
    /// Forecast creation request
    /// </summary>
    [Description("Forecast creation request")]
    public class ForecastRequest
    {
        public string ForecastName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateOnly ForecastPeriodStart { get; set; }
        public DateOnly ForecastPeriodEnd { get; set; }
        public DateOnly BasePeriodStart { get; set; }
        public DateOnly BasePeriodEnd { get; set; }
        public string ForecastMethod { get; set; } = "Linear";
        public List<string> AccountCodes { get; set; } = new();
    }

    /// <summary>
    /// Forecast update request
    /// </summary>
    [Description("Forecast update request")]
    public class ForecastUpdateRequest
    {
        public List<ForecastLineUpdate> LineUpdates { get; set; } = new();
    }

    /// <summary>
    /// Forecast line update
    /// </summary>
    [Description("Forecast line update")]
    public class ForecastLineUpdate
    {
        public string LineId { get; set; } = string.Empty;
        public decimal NewForecastAmount { get; set; }
        public decimal Confidence { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Budget variance report
    /// </summary>
    [Description("Budget variance report")]
    public class BudgetVarianceReport
    {
        public string BudgetId { get; set; } = string.Empty;
        public string BudgetName { get; set; } = string.Empty;
        public DateOnly ReportDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<VarianceAnalysis> VarianceAnalyses { get; set; } = new();
        public decimal TotalPeriodVariance { get; set; }
        public decimal TotalYearToDateVariance { get; set; }
    }

    /// <summary>
    /// Variance analysis details
    /// </summary>
    [Description("Variance analysis")]
    public class VarianceAnalysis
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal PeriodBudget { get; set; }
        public decimal PeriodActual { get; set; }
        public decimal PeriodVariance { get; set; }
        public decimal YearToDateBudget { get; set; }
        public decimal YearToDateActual { get; set; }
        public decimal YearToDateVariance { get; set; }
        public string VarianceReason { get; set; } = string.Empty;
        public string ActionPlan { get; set; } = string.Empty;
    }

    /// <summary>
    /// Budget approval request
    /// </summary>
    [Description("Budget approval request")]
    public class BudgetApprovalRequest
    {
        public string Action { get; set; } = string.Empty; // Approve, Reject, Send_Back
        public string Comments { get; set; } = string.Empty;
    }

    /// <summary>
    /// Budget approval result
    /// </summary>
    [Description("Budget approval result")]
    public class BudgetApprovalResult
    {
        public string BudgetId { get; set; } = string.Empty;
        public string RequestedAction { get; set; } = string.Empty;
        public string ProcessedBy { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Rolling forecast for continuous planning
    /// </summary>
    [Description("Rolling forecast")]
    public class RollingForecast
    {
        public string Id { get; set; } = string.Empty;
        public string ForecastName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ForecastPeriods { get; set; } // Number of periods to forecast
        public string UpdateFrequency { get; set; } = string.Empty; // Monthly, Quarterly
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
        public List<ForecastPeriod> Periods { get; set; } = new();
    }

    /// <summary>
    /// Rolling forecast creation request
    /// </summary>
    [Description("Rolling forecast request")]
    public class RollingForecastRequest
    {
        public string ForecastName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ForecastPeriods { get; set; } = 12;
        public string UpdateFrequency { get; set; } = "Monthly";
        public List<string> AccountCodes { get; set; } = new();
    }

    /// <summary>
    /// Individual period in rolling forecast
    /// </summary>
    [Description("Forecast period")]
    public class ForecastPeriod
    {
        public int PeriodNumber { get; set; }
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public bool IsActual { get; set; }
        public List<ForecastLine> ForecastLines { get; set; } = new();
        public decimal TotalForecastAmount { get; set; }
        public decimal TotalActualAmount { get; set; }
        public decimal Accuracy { get; set; }
    }

    /// <summary>
    /// Budget scenario for what-if analysis
    /// </summary>
    [Description("Budget scenario")]
    public class BudgetScenario
    {
        public string Id { get; set; } = string.Empty;
        public string ScenarioName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BaseBudgetId { get; set; } = string.Empty;
        public string ScenarioType { get; set; } = string.Empty; // Best Case, Worst Case, Most Likely
        public decimal GrowthRate { get; set; }
        public decimal InflationRate { get; set; }
        public List<ScenarioAdjustment> Adjustments { get; set; } = new();
    }

    /// <summary>
    /// Scenario adjustment to base budget
    /// </summary>
    [Description("Scenario adjustment")]
    public class ScenarioAdjustment
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AdjustmentType { get; set; } = string.Empty; // Percentage, Fixed Amount
        public decimal AdjustmentValue { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Budget performance metrics
    /// </summary>
    [Description("Budget performance metrics")]
    public class BudgetPerformanceMetrics
    {
        public string BudgetId { get; set; } = string.Empty;
        public DateOnly CalculationDate { get; set; }
        public decimal BudgetAccuracy { get; set; }
        public decimal AverageVariance { get; set; }
        public int PeriodsAnalyzed { get; set; }
        public decimal ForecastReliability { get; set; }
        public List<string> TopVarianceAccounts { get; set; } = new();
        public List<string> MostAccurateAccounts { get; set; } = new();
    }
}
