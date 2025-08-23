using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a step in the document workflow process
    /// </summary>
    [Description("Document workflow step")]
    public class DocumentWorkflowStep
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DocumentId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public DocumentStatus FromStatus { get; set; }
        public DocumentStatus ToStatus { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }
        public string Comments { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public bool IsSystemAction { get; set; }
    }

    /// <summary>
    /// Document version information
    /// </summary>
    [Description("Document version")]
    public class DocumentVersion
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DocumentId { get; set; }
        public int VersionNumber { get; set; }
        public string VersionComments { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string DocumentSnapshot { get; set; } = string.Empty; // JSON serialized document state
        public long FileSize { get; set; }
        public string ChecksumHash { get; set; } = string.Empty;
    }

    /// <summary>
    /// Document version comparison result
    /// </summary>
    [Description("Document version comparison")]
    public class DocumentVersionComparison
    {
        public Guid DocumentId { get; set; }
        public int FromVersion { get; set; }
        public int ToVersion { get; set; }
        public List<DocumentFieldChange> FieldChanges { get; set; } = new();
        public List<DocumentLineChange> LineChanges { get; set; } = new();
        public DateTime ComparedAt { get; set; }
        public string ComparedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Individual field change between versions
    /// </summary>
    [Description("Document field change")]
    public class DocumentFieldChange
    {
        public string FieldName { get; set; } = string.Empty;
        public string FieldDisplayName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string ChangeType { get; set; } = string.Empty; // Added, Modified, Removed
    }

    /// <summary>
    /// Document line change between versions
    /// </summary>
    [Description("Document line change")]
    public class DocumentLineChange
    {
        public int? LineNumber { get; set; }
        public string ChangeType { get; set; } = string.Empty; // Added, Modified, Removed
        public string? OldLineData { get; set; }
        public string? NewLineData { get; set; }
        public List<DocumentFieldChange> FieldChanges { get; set; } = new();
    }

    /// <summary>
    /// Document attachment information
    /// </summary>
    [Description("Document attachment")]
    public class DocumentAttachment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string AttachedBy { get; set; } = string.Empty;
        public DateTime AttachedAt { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string ChecksumHash { get; set; } = string.Empty;
    }

    /// <summary>
    /// Attachment file data for download
    /// </summary>
    [Description("Attachment file data")]
    public class AttachmentFileData
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
    }

    /// <summary>
    /// Document search criteria
    /// </summary>
    [Description("Document search criteria")]
    public class DocumentSearchCriteria
    {
        public string? DocumentNumber { get; set; }
        public string? DocumentTypeCode { get; set; }
        public Guid? BusinessEntityId { get; set; }
        public DocumentStatus? Status { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public string? SearchText { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public List<string> Tags { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? SortField { get; set; }
        public bool SortDescending { get; set; } = true;
    }

    /// <summary>
    /// Document search result
    /// </summary>
    [Description("Document search result")]
    public class DocumentSearchResult
    {
        public List<DocumentSearchItem> Documents { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public TimeSpan SearchDuration { get; set; }
        public DateTime SearchPerformedAt { get; set; }
        public string SearchPerformedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Individual document in search results
    /// </summary>
    [Description("Document search item")]
    public class DocumentSearchItem
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentTypeName { get; set; } = string.Empty;
        public string BusinessEntityName { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }
        public DateOnly Date { get; set; }
        public decimal TotalAmount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> MatchHighlights { get; set; } = new();
        public int AttachmentCount { get; set; }
        public bool HasWorkflowHistory { get; set; }
    }

    /// <summary>
    /// Document report types
    /// </summary>
    [Description("Document report type")]
    public enum DocumentReportType
    {
        Summary,
        Detailed,
        StatusReport,
        AuditTrail,
        WorkflowReport,
        DocumentsByBusinessEntity,
        DocumentsByType,
        PendingApprovals,
        OverdueDocuments
    }

    /// <summary>
    /// Document report parameters
    /// </summary>
    [Description("Document report parameters")]
    public class DocumentReportParameters
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? DocumentTypeCode { get; set; }
        public Guid? BusinessEntityId { get; set; }
        public DocumentStatus? Status { get; set; }
        public string? CreatedBy { get; set; }
        public List<string> IncludeFields { get; set; } = new();
        public bool IncludeWorkflowHistory { get; set; }
        public bool IncludeAttachments { get; set; }
        public string ReportFormat { get; set; } = "JSON"; // JSON, CSV, PDF
        public string? FilterExpression { get; set; }
    }

    /// <summary>
    /// Generated document report
    /// </summary>
    [Description("Document report")]
    public class DocumentReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DocumentReportType ReportType { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public TimeSpan GenerationDuration { get; set; }
        public int RecordCount { get; set; }
        public string ReportData { get; set; } = string.Empty; // JSON data
        public List<DocumentReportSection> Sections { get; set; } = new();
        public Dictionary<string, object> Summary { get; set; } = new();
    }

    /// <summary>
    /// Document report section
    /// </summary>
    [Description("Document report section")]
    public class DocumentReportSection
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<Dictionary<string, object>> Data { get; set; } = new();
        public Dictionary<string, object> SectionSummary { get; set; } = new();
    }

    /// <summary>
    /// Document summary statistics
    /// </summary>
    [Description("Document summary statistics")]
    public class DocumentSummaryStatistics
    {
        public int TotalDocuments { get; set; }
        public decimal TotalAmount { get; set; }
        public Dictionary<string, int> DocumentsByStatus { get; set; } = new();
        public Dictionary<string, int> DocumentsByType { get; set; } = new();
        public Dictionary<string, decimal> AmountsByType { get; set; } = new();
        public int DocumentsThisMonth { get; set; }
        public int DocumentsThisYear { get; set; }
        public decimal AverageDocumentAmount { get; set; }
        public DocumentTrendData TrendData { get; set; } = new();
        public DateTime StatisticsGeneratedAt { get; set; }
    }

    /// <summary>
    /// Document trend data
    /// </summary>
    [Description("Document trend data")]
    public class DocumentTrendData
    {
        public List<MonthlyDocumentCount> MonthlyTrends { get; set; } = new();
        public List<DailyDocumentCount> DailyTrends { get; set; } = new();
        public decimal GrowthRate { get; set; }
        public string TrendDirection { get; set; } = string.Empty; // Up, Down, Stable
    }

    /// <summary>
    /// Monthly document count for trends
    /// </summary>
    [Description("Monthly document count")]
    public class MonthlyDocumentCount
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Daily document count for trends
    /// </summary>
    [Description("Daily document count")]
    public class DailyDocumentCount
    {
        public DateOnly Date { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Document statistics filter
    /// </summary>
    [Description("Document statistics filter")]
    public class DocumentStatisticsFilter
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? DocumentTypeCode { get; set; }
        public Guid? BusinessEntityId { get; set; }
        public DocumentStatus? Status { get; set; }
        public bool IncludeTrends { get; set; } = true;
        public int TrendPeriodDays { get; set; } = 30;
    }

    /// <summary>
    /// Document template definition
    /// </summary>
    [Description("Document template")]
    public class DocumentTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DocumentTypeCode { get; set; } = string.Empty;
        public string TemplateData { get; set; } = string.Empty; // JSON template structure
        public List<TemplateField> Fields { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int UsageCount { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// Template field definition
    /// </summary>
    [Description("Template field")]
    public class TemplateField
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty; // Text, Number, Date, Boolean, List
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public List<string> ValidValues { get; set; } = new();
        public string ValidationExpression { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
