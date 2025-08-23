using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Advanced tax management service interface
    /// </summary>
    [Description("Advanced tax management service")]
    public interface ITaxManagementService
    {
        /// <summary>
        /// Creates or updates a tax configuration
        /// </summary>
        Task<ITax> CreateOrUpdateTaxAsync(TaxConfiguration taxConfig, string userId);

        /// <summary>
        /// Deactivates a tax configuration
        /// </summary>
        Task DeactivateTaxAsync(string taxCode, string userId);

        /// <summary>
        /// Gets tax configuration by code
        /// </summary>
        Task<ITax?> GetTaxByCodeAsync(string taxCode);

        /// <summary>
        /// Gets all tax configurations with optional filtering
        /// </summary>
        Task<List<ITax>> GetTaxConfigurationsAsync(TaxConfigurationFilter? filter = null);

        /// <summary>
        /// Validates tax configuration before saving
        /// </summary>
        Task<ValidationResult> ValidateTaxConfigurationAsync(TaxConfiguration taxConfig);

        /// <summary>
        /// Gets tax usage statistics
        /// </summary>
        Task<TaxUsageStatistics> GetTaxUsageStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Archives old tax configurations
        /// </summary>
        Task<int> ArchiveOldTaxConfigurationsAsync(DateTime cutoffDate, string userId);

        /// <summary>
        /// Duplicates an existing tax configuration
        /// </summary>
        Task<ITax> DuplicateTaxConfigurationAsync(string sourceTaxCode, string newTaxCode, string userId);
    }

    /// <summary>
    /// Tax exemption management service interface
    /// </summary>
    [Description("Tax exemption management service")]
    public interface ITaxExemptionService
    {
        /// <summary>
        /// Creates a tax exemption
        /// </summary>
        Task<TaxExemption> CreateExemptionAsync(TaxExemption exemption, string userId);

        /// <summary>
        /// Gets exemptions for a business entity
        /// </summary>
        Task<List<TaxExemption>> GetExemptionsForEntityAsync(Guid businessEntityId);

        /// <summary>
        /// Gets exemptions for an item/product
        /// </summary>
        Task<List<TaxExemption>> GetExemptionsForItemAsync(string itemCode);

        /// <summary>
        /// Checks if tax exemption applies
        /// </summary>
        Task<bool> IsExemptFromTaxAsync(Guid businessEntityId, string itemCode, string taxCode);

        /// <summary>
        /// Updates exemption status
        /// </summary>
        Task UpdateExemptionAsync(Guid exemptionId, TaxExemption exemption, string userId);

        /// <summary>
        /// Deactivates an exemption
        /// </summary>
        Task DeactivateExemptionAsync(Guid exemptionId, string userId);

        /// <summary>
        /// Gets exemption audit trail
        /// </summary>
        Task<List<TaxExemptionAudit>> GetExemptionAuditTrailAsync(Guid exemptionId);

        /// <summary>
        /// Validates exemption configuration
        /// </summary>
        Task<ValidationResult> ValidateExemptionAsync(TaxExemption exemption);
    }

    /// <summary>
    /// Tax reporting service interface
    /// </summary>
    [Description("Tax reporting service")]
    public interface ITaxReportingService
    {
        /// <summary>
        /// Generates tax summary report
        /// </summary>
        Task<TaxReport> GenerateTaxSummaryReportAsync(TaxReportParameters parameters);

        /// <summary>
        /// Generates detailed tax report
        /// </summary>
        Task<TaxReport> GenerateDetailedTaxReportAsync(TaxReportParameters parameters);

        /// <summary>
        /// Generates VAT return report
        /// </summary>
        Task<VATReturnReport> GenerateVATReturnAsync(VATReturnParameters parameters);

        /// <summary>
        /// Generates withholding tax report
        /// </summary>
        Task<WithholdingTaxReport> GenerateWithholdingTaxReportAsync(WithholdingTaxReportParameters parameters);

        /// <summary>
        /// Gets tax analytics
        /// </summary>
        Task<TaxAnalytics> GetTaxAnalyticsAsync(TaxAnalyticsParameters parameters);

        /// <summary>
        /// Exports tax data for compliance
        /// </summary>
        Task<TaxExportData> ExportTaxDataAsync(TaxExportParameters parameters);

        /// <summary>
        /// Gets tax compliance status
        /// </summary>
        Task<TaxComplianceStatus> GetTaxComplianceStatusAsync(DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Generates tax reconciliation report
        /// </summary>
        Task<TaxReconciliationReport> GenerateTaxReconciliationReportAsync(TaxReconciliationParameters parameters);
    }

    /// <summary>
    /// Tax rules engine service interface
    /// </summary>
    [Description("Tax rules engine service")]
    public interface ITaxRulesEngineService
    {
        /// <summary>
        /// Creates or updates a tax rule
        /// </summary>
        Task<TaxRule> CreateOrUpdateRuleAsync(TaxRule rule, string userId);

        /// <summary>
        /// Gets all active tax rules
        /// </summary>
        Task<List<TaxRule>> GetActiveRulesAsync();

        /// <summary>
        /// Gets rules for specific criteria
        /// </summary>
        Task<List<TaxRule>> GetRulesAsync(TaxRuleFilter filter);

        /// <summary>
        /// Evaluates rules for a document
        /// </summary>
        Task<TaxRuleEvaluationResult> EvaluateRulesAsync(IDocument document, DocumentOperation operation);

        /// <summary>
        /// Tests a rule against sample data
        /// </summary>
        Task<TaxRuleTestResult> TestRuleAsync(TaxRule rule, TaxRuleTestData testData);

        /// <summary>
        /// Deactivates a tax rule
        /// </summary>
        Task DeactivateRuleAsync(Guid ruleId, string userId);

        /// <summary>
        /// Gets rule evaluation history
        /// </summary>
        Task<List<TaxRuleEvaluationHistory>> GetRuleEvaluationHistoryAsync(Guid ruleId, DateTime? fromDate = null);

        /// <summary>
        /// Validates rule configuration
        /// </summary>
        Task<ValidationResult> ValidateRuleAsync(TaxRule rule);
    }

    /// <summary>
    /// Tax audit service interface
    /// </summary>
    [Description("Tax audit service")]
    public interface ITaxAuditService
    {
        /// <summary>
        /// Records tax calculation audit entry
        /// </summary>
        Task RecordTaxCalculationAuditAsync(TaxCalculationAudit audit);

        /// <summary>
        /// Gets tax audit trail for document
        /// </summary>
        Task<List<TaxCalculationAudit>> GetDocumentTaxAuditTrailAsync(Guid documentId);

        /// <summary>
        /// Gets audit trail for business entity
        /// </summary>
        Task<List<TaxCalculationAudit>> GetEntityTaxAuditTrailAsync(Guid businessEntityId, DateTime? fromDate = null);

        /// <summary>
        /// Gets tax changes audit
        /// </summary>
        Task<List<TaxChangeAudit>> GetTaxChangesAuditAsync(string? taxCode = null, DateTime? fromDate = null);

        /// <summary>
        /// Generates audit report
        /// </summary>
        Task<TaxAuditReport> GenerateAuditReportAsync(TaxAuditReportParameters parameters);

        /// <summary>
        /// Verifies tax calculations integrity
        /// </summary>
        Task<TaxIntegrityCheckResult> VerifyTaxCalculationsIntegrityAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Gets suspicious tax patterns
        /// </summary>
        Task<List<SuspiciousTaxPattern>> GetSuspiciousTaxPatternsAsync(SuspiciousTaxPatternFilter filter);

        /// <summary>
        /// Archives old audit data
        /// </summary>
        Task<int> ArchiveOldAuditDataAsync(DateTime cutoffDate);
    }

    /// <summary>
    /// Tax configuration model
    /// </summary>
    [Description("Tax configuration")]
    public class TaxConfiguration
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaxType TaxType { get; set; }
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string Country { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public TaxCalculationMethod CalculationMethod { get; set; }
        public TaxRoundingMethod RoundingMethod { get; set; }
        public int DecimalPlaces { get; set; } = 2;
        public bool IsCompoundTax { get; set; }
        public List<string> DependsOnTaxCodes { get; set; } = new();
        public Dictionary<string, string> AccountingMappings { get; set; } = new();
        public List<TaxBracket> TaxBrackets { get; set; } = new();
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    /// <summary>
    /// Tax configuration filter
    /// </summary>
    [Description("Tax configuration filter")]
    public class TaxConfigurationFilter
    {
        public string? TaxCode { get; set; }
        public TaxType? TaxType { get; set; }
        public bool? IsActive { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    /// <summary>
    /// Tax usage statistics
    /// </summary>
    [Description("Tax usage statistics")]
    public class TaxUsageStatistics
    {
        public Dictionary<string, int> TaxUsageCount { get; set; } = new();
        public Dictionary<string, decimal> TaxAmountTotals { get; set; } = new();
        public Dictionary<TaxType, int> UsageByType { get; set; } = new();
        public DateTime ReportGeneratedAt { get; set; }
        public string ReportPeriod { get; set; } = string.Empty;
        public int TotalDocumentsProcessed { get; set; }
        public decimal TotalTaxAmount { get; set; }
    }

    /// <summary>
    /// Tax exemption
    /// </summary>
    [Description("Tax exemption")]
    public class TaxExemption
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? BusinessEntityId { get; set; }
        public string? ItemCode { get; set; }
        public string TaxCode { get; set; } = string.Empty;
        public TaxExemptionType ExemptionType { get; set; }
        public string ExemptionReason { get; set; } = string.Empty;
        public string? CertificateNumber { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<TaxExemptionDocument> SupportingDocuments { get; set; } = new();
    }

    /// <summary>
    /// Tax exemption types
    /// </summary>
    [Description("Tax exemption type")]
    public enum TaxExemptionType
    {
        Full,
        Partial,
        Conditional,
        Temporary
    }

    /// <summary>
    /// Tax calculation methods
    /// </summary>
    [Description("Tax calculation method")]
    public enum TaxCalculationMethod
    {
        Percentage,
        FixedAmount,
        Progressive,
        Compound
    }

    /// <summary>
    /// Tax rounding methods
    /// </summary>
    [Description("Tax rounding method")]
    public enum TaxRoundingMethod
    {
        Round,
        RoundUp,
        RoundDown,
        Truncate
    }

    /// <summary>
    /// Tax bracket for progressive taxation
    /// </summary>
    [Description("Tax bracket")]
    public class TaxBracket
    {
        public decimal MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public decimal Rate { get; set; }
        public decimal FixedAmount { get; set; }
    }

    /// <summary>
    /// Tax exemption document
    /// </summary>
    [Description("Tax exemption document")]
    public class TaxExemptionDocument
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string IssuingAuthority { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tax exemption audit entry
    /// </summary>
    [Description("Tax exemption audit")]
    public class TaxExemptionAudit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ExemptionId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }
        public string? PreviousValue { get; set; }
        public string? NewValue { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
