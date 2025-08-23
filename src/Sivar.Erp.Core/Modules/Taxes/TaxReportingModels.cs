using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Tax reporting models
    /// </summary>
    
    /// <summary>
    /// Tax report parameters
    /// </summary>
    [Description("Tax report parameters")]
    public class TaxReportParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? TaxCode { get; set; }
        public TaxType? TaxType { get; set; }
        public Guid? BusinessEntityId { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public TaxReportFormat Format { get; set; } = TaxReportFormat.Summary;
        public bool IncludeExemptions { get; set; }
        public bool IncludeZeroAmounts { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax report
    /// </summary>
    [Description("Tax report")]
    public class TaxReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ReportName { get; set; } = string.Empty;
        public TaxReportParameters Parameters { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public List<TaxReportSection> Sections { get; set; } = new();
        public TaxReportSummary Summary { get; set; } = new();
        public TimeSpan GenerationDuration { get; set; }
        public int RecordCount { get; set; }
    }

    /// <summary>
    /// Tax report section
    /// </summary>
    [Description("Tax report section")]
    public class TaxReportSection
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<TaxReportItem> Items { get; set; } = new();
        public Dictionary<string, decimal> Totals { get; set; } = new();
    }

    /// <summary>
    /// Tax report item
    /// </summary>
    [Description("Tax report item")]
    public class TaxReportItem
    {
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public TaxType TaxType { get; set; }
        public decimal TaxRate { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public int TransactionCount { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Tax report summary
    /// </summary>
    [Description("Tax report summary")]
    public class TaxReportSummary
    {
        public decimal TotalBaseAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public int TotalTransactions { get; set; }
        public Dictionary<TaxType, decimal> TaxByType { get; set; } = new();
        public Dictionary<string, decimal> TaxByCode { get; set; } = new();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    /// <summary>
    /// VAT return parameters
    /// </summary>
    [Description("VAT return parameters")]
    public class VATReturnParameters
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string Country { get; set; } = string.Empty;
        public string TaxRegistrationNumber { get; set; } = string.Empty;
        public VATReturnType ReturnType { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// VAT return report
    /// </summary>
    [Description("VAT return report")]
    public class VATReturnReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public VATReturnParameters Parameters { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public VATReturnData ReturnData { get; set; } = new();
        public List<VATReturnLine> Lines { get; set; } = new();
        public VATReturnSummary Summary { get; set; } = new();
    }

    /// <summary>
    /// VAT return data
    /// </summary>
    [Description("VAT return data")]
    public class VATReturnData
    {
        public decimal OutputVAT { get; set; }
        public decimal InputVAT { get; set; }
        public decimal NetVAT { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal ExemptSales { get; set; }
        public decimal ZeroRatedSales { get; set; }
    }

    /// <summary>
    /// VAT return line
    /// </summary>
    [Description("VAT return line")]
    public class VATReturnLine
    {
        public string LineCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CalculationFormula { get; set; } = string.Empty;
    }

    /// <summary>
    /// VAT return summary
    /// </summary>
    [Description("VAT return summary")]
    public class VATReturnSummary
    {
        public decimal VATPayable { get; set; }
        public decimal VATRefundable { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Withholding tax report parameters
    /// </summary>
    [Description("Withholding tax report parameters")]
    public class WithholdingTaxReportParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? PayeeId { get; set; }
        public string? WithholdingTaxCode { get; set; }
        public decimal? MinAmount { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Withholding tax report
    /// </summary>
    [Description("Withholding tax report")]
    public class WithholdingTaxReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public WithholdingTaxReportParameters Parameters { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public List<WithholdingTaxEntry> Entries { get; set; } = new();
        public WithholdingTaxSummary Summary { get; set; } = new();
    }

    /// <summary>
    /// Withholding tax entry
    /// </summary>
    [Description("Withholding tax entry")]
    public class WithholdingTaxEntry
    {
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }
        public string PayeeId { get; set; } = string.Empty;
        public string PayeeName { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public decimal BaseAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal WithheldAmount { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Withholding tax summary
    /// </summary>
    [Description("Withholding tax summary")]
    public class WithholdingTaxSummary
    {
        public decimal TotalBaseAmount { get; set; }
        public decimal TotalWithheldAmount { get; set; }
        public int TotalTransactions { get; set; }
        public Dictionary<string, decimal> WithholdingByTaxCode { get; set; } = new();
        public Dictionary<string, decimal> WithholdingByPayee { get; set; } = new();
    }

    /// <summary>
    /// Tax analytics parameters
    /// </summary>
    [Description("Tax analytics parameters")]
    public class TaxAnalyticsParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TaxAnalyticsType AnalyticsType { get; set; }
        public string? TaxCode { get; set; }
        public string? BusinessEntityId { get; set; }
        public TaxAnalyticsGrouping Grouping { get; set; }
    }

    /// <summary>
    /// Tax analytics
    /// </summary>
    [Description("Tax analytics")]
    public class TaxAnalytics
    {
        public TaxAnalyticsParameters Parameters { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public List<TaxAnalyticsDataPoint> DataPoints { get; set; } = new();
        public TaxAnalyticsSummary Summary { get; set; } = new();
        public List<TaxTrend> Trends { get; set; } = new();
    }

    /// <summary>
    /// Tax analytics data point
    /// </summary>
    [Description("Tax analytics data point")]
    public class TaxAnalyticsDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Period { get; set; }
        public decimal Value { get; set; }
        public int Count { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Tax analytics summary
    /// </summary>
    [Description("Tax analytics summary")]
    public class TaxAnalyticsSummary
    {
        public decimal AverageAmount { get; set; }
        public decimal MedianAmount { get; set; }
        public decimal StandardDeviation { get; set; }
        public decimal GrowthRate { get; set; }
        public string DominantTaxType { get; set; } = string.Empty;
        public decimal EffectiveTaxRate { get; set; }
    }

    /// <summary>
    /// Tax trend
    /// </summary>
    [Description("Tax trend")]
    public class TaxTrend
    {
        public string TrendType { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax export parameters
    /// </summary>
    [Description("Tax export parameters")]
    public class TaxExportParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TaxExportFormat Format { get; set; }
        public List<string> TaxCodes { get; set; } = new();
        public bool IncludeDocumentDetails { get; set; }
        public bool IncludeExemptions { get; set; }
        public string ExportedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax export data
    /// </summary>
    [Description("Tax export data")]
    public class TaxExportData
    {
        public Guid ExportId { get; set; } = Guid.NewGuid();
        public TaxExportParameters Parameters { get; set; } = new();
        public DateTime ExportedAt { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int RecordCount { get; set; }
        public string ChecksumHash { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax compliance status
    /// </summary>
    [Description("Tax compliance status")]
    public class TaxComplianceStatus
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime CheckedAt { get; set; }
        public List<TaxComplianceIssue> Issues { get; set; } = new();
        public TaxComplianceScore Score { get; set; } = new();
        public List<TaxComplianceRecommendation> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Tax compliance issue
    /// </summary>
    [Description("Tax compliance issue")]
    public class TaxComplianceIssue
    {
        public string IssueType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Guid> AffectedDocuments { get; set; } = new();
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax compliance score
    /// </summary>
    [Description("Tax compliance score")]
    public class TaxComplianceScore
    {
        public decimal OverallScore { get; set; }
        public decimal AccuracyScore { get; set; }
        public decimal CompletenessScore { get; set; }
        public decimal TimelinessScore { get; set; }
        public string Grade { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax compliance recommendation
    /// </summary>
    [Description("Tax compliance recommendation")]
    public class TaxComplianceRecommendation
    {
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
    }

    /// <summary>
    /// Tax reconciliation parameters
    /// </summary>
    [Description("Tax reconciliation parameters")]
    public class TaxReconciliationParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? TaxCode { get; set; }
        public string? AccountCode { get; set; }
        public bool IncludeDiscrepancies { get; set; } = true;
        public string GeneratedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax reconciliation report
    /// </summary>
    [Description("Tax reconciliation report")]
    public class TaxReconciliationReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public TaxReconciliationParameters Parameters { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public List<TaxReconciliationItem> Items { get; set; } = new();
        public List<TaxDiscrepancy> Discrepancies { get; set; } = new();
        public TaxReconciliationSummary Summary { get; set; } = new();
    }

    /// <summary>
    /// Tax reconciliation item
    /// </summary>
    [Description("Tax reconciliation item")]
    public class TaxReconciliationItem
    {
        public string TaxCode { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public decimal BookAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Variance { get; set; }
        public bool IsReconciled { get; set; }
    }

    /// <summary>
    /// Tax discrepancy
    /// </summary>
    [Description("Tax discrepancy")]
    public class TaxDiscrepancy
    {
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string DiscrepancyType { get; set; } = string.Empty;
        public decimal ExpectedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax reconciliation summary
    /// </summary>
    [Description("Tax reconciliation summary")]
    public class TaxReconciliationSummary
    {
        public int TotalItems { get; set; }
        public int ReconciledItems { get; set; }
        public int DiscrepancyCount { get; set; }
        public decimal TotalVariance { get; set; }
        public decimal ReconciliationPercentage { get; set; }
    }

    /// <summary>
    /// Enumerations for tax reporting
    /// </summary>
    public enum TaxReportFormat
    {
        Summary,
        Detailed,
        Analytical
    }

    public enum VATReturnType
    {
        Monthly,
        Quarterly,
        Annual
    }

    public enum TaxAnalyticsType
    {
        Trends,
        Distribution,
        Compliance,
        Performance
    }

    public enum TaxAnalyticsGrouping
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly
    }

    public enum TaxExportFormat
    {
        CSV,
        Excel,
        XML,
        JSON,
        PDF
    }
}
