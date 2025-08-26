using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities.Accounting;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Application.DTOs.Accounting;

/// <summary>
/// DTO for creating a new journal entry
/// </summary>
public class CreateJournalEntryDto
{
    [Required]
    public DateOnly TransactionDate { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Reference { get; set; }
    
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    public decimal ExchangeRate { get; set; } = 1.0m;
    
    [MaxLength(50)]
    public string? SourceType { get; set; }
    
    public Guid? SourceId { get; set; }
    
    public Guid? AccountingPeriodId { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "At least 2 lines are required")]
    public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
}

/// <summary>
/// DTO for updating a journal entry
/// </summary>
public class UpdateJournalEntryDto
{
    [Required]
    public DateOnly TransactionDate { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Reference { get; set; }
    
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    public decimal ExchangeRate { get; set; } = 1.0m;
    
    [MaxLength(50)]
    public string? SourceType { get; set; }
    
    public Guid? SourceId { get; set; }
    
    public Guid? AccountingPeriodId { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "At least 2 lines are required")]
    public List<UpdateJournalEntryLineDto> Lines { get; set; } = new();
}

/// <summary>
/// DTO for creating a journal entry line
/// </summary>
public class CreateJournalEntryLineDto
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public decimal DebitAmount { get; set; } = 0;
    
    public decimal CreditAmount { get; set; } = 0;
    
    [MaxLength(20)]
    public string? DepartmentCode { get; set; }
    
    [MaxLength(20)]
    public string? ProjectCode { get; set; }
    
    [MaxLength(100)]
    public string? CustomerVendorRef { get; set; }
    
    [MaxLength(255)]
    public string? Reference { get; set; }
    
    [MaxLength(20)]
    public string? TaxCode { get; set; }
    
    public decimal TaxAmount { get; set; } = 0;
    
    public decimal BaseAmount { get; set; } = 0;
    
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    public decimal ExchangeRate { get; set; } = 1.0m;
    
    public decimal ForeignAmount { get; set; } = 0;
}

/// <summary>
/// DTO for updating a journal entry line
/// </summary>
public class UpdateJournalEntryLineDto
{
    public Guid? Id { get; set; } // Null for new lines
    
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public decimal DebitAmount { get; set; } = 0;
    
    public decimal CreditAmount { get; set; } = 0;
    
    [MaxLength(20)]
    public string? DepartmentCode { get; set; }
    
    [MaxLength(20)]
    public string? ProjectCode { get; set; }
    
    [MaxLength(100)]
    public string? CustomerVendorRef { get; set; }
    
    [MaxLength(255)]
    public string? Reference { get; set; }
    
    [MaxLength(20)]
    public string? TaxCode { get; set; }
    
    public decimal TaxAmount { get; set; } = 0;
    
    public decimal BaseAmount { get; set; } = 0;
    
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    public decimal ExchangeRate { get; set; } = 1.0m;
    
    public decimal ForeignAmount { get; set; } = 0;
}

/// <summary>
/// DTO for journal entry response
/// </summary>
public class JournalEntryDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateOnly TransactionDate { get; set; }
    public DateOnly? PostingDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public JournalEntryStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public Guid? AccountingPeriodId { get; set; }
    public string? AccountingPeriodName { get; set; }
    public string? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public string ApprovalStatusName { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    
    public List<JournalEntryLineDto> Lines { get; set; } = new();
    public bool IsBalanced { get; set; }
}

/// <summary>
/// DTO for journal entry line response
/// </summary>
public class JournalEntryLineDto
{
    public Guid Id { get; set; }
    public Guid JournalEntryId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? DepartmentCode { get; set; }
    public string? ProjectCode { get; set; }
    public string? CustomerVendorRef { get; set; }
    public string? Reference { get; set; }
    public string? TaxCode { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal BaseAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal ForeignAmount { get; set; }
    public bool IsReconciled { get; set; }
    public string? ReconciliationRef { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public decimal NetAmount { get; set; }
}

/// <summary>
/// DTO for journal entry summary
/// </summary>
public class JournalEntrySummaryDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateOnly TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public JournalEntryStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string Currency { get; set; } = string.Empty;
    public ApprovalStatus ApprovalStatus { get; set; }
    public string ApprovalStatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsBalanced { get; set; }
}

/// <summary>
/// DTO for journal entry query
/// </summary>
public class JournalEntryQueryDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public JournalEntryStatus? Status { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
    public Guid? AccountingPeriodId { get; set; }
    public Guid? AccountId { get; set; }
    public string? Reference { get; set; }
    public string? SearchTerm { get; set; }
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
