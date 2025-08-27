namespace Sivar.Erp.Core.Application.DTOs.Documents
{
    /// <summary>
    /// Document summary DTO for list views
    /// </summary>
    public class DocumentSummaryDto
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; } = "";
        public DateOnly DocumentDate { get; set; }
        public TimeOnly DocumentTime { get; set; }
        public string? DocumentTypeName { get; set; }
        public string? BusinessEntityName { get; set; }
        public string Status { get; set; } = "";
        public string CurrencyCode { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public bool IsPosted { get; set; }
        public int LineCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "";
    }

    /// <summary>
    /// Complete document DTO with all details
    /// </summary>
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; } = "";
        public DateOnly DocumentDate { get; set; }
        public TimeOnly DocumentTime { get; set; }
        
        public Guid DocumentTypeId { get; set; }
        public string? DocumentTypeName { get; set; }
        
        public Guid? BusinessEntityId { get; set; }
        public string? BusinessEntityName { get; set; }
        
        public string Status { get; set; } = "";
        public string CurrencyCode { get; set; } = "";
        public decimal ExchangeRate { get; set; }
        public string? Remarks { get; set; }
        public DateOnly? DueDate { get; set; }
        
        // Financial totals
        public decimal SubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Posting information
        public bool IsPosted { get; set; }
        public DateTime? PostedAt { get; set; }
        public string? PostedBy { get; set; }
        
        // Metadata
        public int LineCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "";
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public int Version { get; set; }
        
        // Related data
        public List<DocumentLineDto> Lines { get; set; } = new();
        public List<DocumentTotalDto> Totals { get; set; } = new();
    }

    /// <summary>
    /// Document line DTO
    /// </summary>
    public class DocumentLineDto
    {
        public Guid Id { get; set; }
        public int LineNumber { get; set; }
        
        public Guid? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string Description { get; set; } = "";
        
        public decimal Quantity { get; set; }
        public string? UnitOfMeasure { get; set; }
        public decimal UnitPrice { get; set; }
        
        // Discounts
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        
        // Tax
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        
        // Totals
        public decimal LineTotal { get; set; }
        
        // Currency
        public string CurrencyCode { get; set; } = "";
        public decimal ExchangeRate { get; set; }
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Document total DTO
    /// </summary>
    public class DocumentTotalDto
    {
        public Guid Id { get; set; }
        public string TotalType { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal? Rate { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; } = "";
        public decimal ExchangeRate { get; set; }
    }

    /// <summary>
    /// Document type DTO
    /// </summary>
    public class DocumentTypeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string Category { get; set; } = "";
        
        // Number generation
        public string? NumberPrefix { get; set; }
        public string? NumberSuffix { get; set; }
        public long NextNumber { get; set; }
        public string NumberFormat { get; set; } = "";
        
        // Configuration
        public bool IsActive { get; set; }
        public bool AutoGenerateNumbers { get; set; }
        public bool RequiresApproval { get; set; }
        public bool RequiresLines { get; set; }
        public bool RequiresBusinessEntity { get; set; }
        public bool AllowZeroAmount { get; set; }
        public string DefaultCurrencyCode { get; set; } = "";
        
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "";
    }

    /// <summary>
    /// Business entity DTO for document relations
    /// </summary>
    public class BusinessEntityDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string EntityType { get; set; } = "";
        public bool IsActive { get; set; }
        
        // Contact information
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        
        // Tax information
        public string? TaxId { get; set; }
        public string? TaxCategory { get; set; }
        
        // Financial
        public string? DefaultCurrencyCode { get; set; }
        public string? PaymentTerms { get; set; }
        public decimal? CreditLimit { get; set; }
    }

    /// <summary>
    /// Item DTO for document lines
    /// </summary>
    public class ItemDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string ItemType { get; set; } = "";
        public string Category { get; set; } = "";
        public bool IsActive { get; set; }
        
        // Inventory
        public string? UnitOfMeasure { get; set; }
        public decimal? StandardCost { get; set; }
        public decimal? ListPrice { get; set; }
        public bool IsInventoryItem { get; set; }
        
        // Tax
        public string? TaxCategory { get; set; }
        public decimal? DefaultTaxPercent { get; set; }
    }

    /// <summary>
    /// Document statistics DTO
    /// </summary>
    public class DocumentStatisticsDto
    {
        public Guid CompanyId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        
        // Count statistics
        public int TotalDocuments { get; set; }
        public int DraftDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public int ApprovedDocuments { get; set; }
        public int PostedDocuments { get; set; }
        
        // Amount statistics
        public decimal TotalAmount { get; set; }
        
        // By type
        public Dictionary<string, int> DocumentsByType { get; set; } = new();
        public Dictionary<string, decimal> AmountsByType { get; set; } = new();
        
        // Trends (if calculated)
        public List<DailyStatistic>? DailyTrends { get; set; }
        public List<MonthlyStatistic>? MonthlyTrends { get; set; }
    }

    /// <summary>
    /// Daily statistics for trends
    /// </summary>
    public class DailyStatistic
    {
        public DateOnly Date { get; set; }
        public int DocumentCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Monthly statistics for trends
    /// </summary>
    public class MonthlyStatistic
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = "";
        public int DocumentCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Document validation result DTO
    /// </summary>
    public class DocumentValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<ValidationDetail> Details { get; set; } = new();
    }

    /// <summary>
    /// Validation detail for specific issues
    /// </summary>
    public class ValidationDetail
    {
        public string PropertyName { get; set; } = "";
        public string ErrorCode { get; set; } = "";
        public string Message { get; set; } = "";
        public string Severity { get; set; } = ""; // Error, Warning, Info
        public object? Value { get; set; }
    }

    /// <summary>
    /// Document search filters DTO
    /// </summary>
    public class DocumentSearchFiltersDto
    {
        public string? SearchText { get; set; }
        public Guid? DocumentTypeId { get; set; }
        public string? Status { get; set; }
        public Guid? BusinessEntityId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? CurrencyCode { get; set; }
        public bool? IsPosted { get; set; }
        public string? CreatedBy { get; set; }
        
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        
        // Sorting
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "DESC";
    }

    /// <summary>
    /// Document export options DTO
    /// </summary>
    public class DocumentExportOptionsDto
    {
        public string Format { get; set; } = ""; // PDF, Excel, CSV, JSON
        public bool IncludeLines { get; set; } = true;
        public bool IncludeTotals { get; set; } = true;
        public bool IncludeAuditInfo { get; set; } = false;
        public string? Template { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }

    /// <summary>
    /// Bulk operation request DTO
    /// </summary>
    public class BulkOperationRequestDto
    {
        public List<Guid> DocumentIds { get; set; } = new();
        public string Operation { get; set; } = ""; // Post, Delete, ChangeStatus, Export
        public Dictionary<string, object>? Parameters { get; set; }
    }

    /// <summary>
    /// Bulk operation result DTO
    /// </summary>
    public class BulkOperationResultDto
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<BulkOperationError> Errors { get; set; } = new();
        public string? ResultData { get; set; } // For exports or other operations that return data
    }

    /// <summary>
    /// Bulk operation error detail
    /// </summary>
    public class BulkOperationError
    {
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = "";
        public string ErrorCode { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
    }
}
