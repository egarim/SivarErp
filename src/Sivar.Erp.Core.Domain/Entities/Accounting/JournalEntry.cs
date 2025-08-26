using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities;
using Sivar.Erp.Core.Domain.Interfaces;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Domain.Entities.Accounting;

/// <summary>
/// Represents a journal entry in the accounting system
/// </summary>
public class JournalEntry : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Company/tenant this journal entry belongs to
    /// </summary>
    public Guid CompanyId { get; set; }
    
    /// <summary>
    /// Journal entry number (auto-generated)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;
    
    /// <summary>
    /// Transaction date
    /// </summary>
    public DateOnly TransactionDate { get; set; }
    
    /// <summary>
    /// Posting date (when entry is posted to ledger)
    /// </summary>
    public DateOnly? PostingDate { get; set; }
    
    /// <summary>
    /// Journal entry description
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Reference number (invoice, receipt, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? Reference { get; set; }
    
    /// <summary>
    /// Journal entry status
    /// </summary>
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;
    
    /// <summary>
    /// Currency code
    /// </summary>
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Exchange rate if foreign currency
    /// </summary>
    public decimal ExchangeRate { get; set; } = 1.0m;
    
    /// <summary>
    /// Total debit amount
    /// </summary>
    public decimal TotalDebit { get; set; } = 0;
    
    /// <summary>
    /// Total credit amount
    /// </summary>
    public decimal TotalCredit { get; set; } = 0;
    
    /// <summary>
    /// Source document type
    /// </summary>
    [MaxLength(50)]
    public string? SourceType { get; set; }
    
    /// <summary>
    /// Source document ID
    /// </summary>
    public Guid? SourceId { get; set; }
    
    /// <summary>
    /// Accounting period this entry belongs to
    /// </summary>
    public Guid? AccountingPeriodId { get; set; }
    
    /// <summary>
    /// Who posted this entry
    /// </summary>
    [MaxLength(255)]
    public string? PostedBy { get; set; }
    
    /// <summary>
    /// When this entry was posted
    /// </summary>
    public DateTime? PostedAt { get; set; }
    
    /// <summary>
    /// Approval status
    /// </summary>
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    
    /// <summary>
    /// Who approved this entry
    /// </summary>
    [MaxLength(255)]
    public string? ApprovedBy { get; set; }
    
    /// <summary>
    /// When this entry was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    /// <summary>
    /// Approval notes
    /// </summary>
    [MaxLength(1000)]
    public string? ApprovalNotes { get; set; }
    
    // Navigation properties
    public virtual ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    public virtual AccountingPeriod? AccountingPeriod { get; set; }
}
