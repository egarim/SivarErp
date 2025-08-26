using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities;

namespace Sivar.Erp.Core.Domain.Entities.Accounting;

/// <summary>
/// Represents a line item in a journal entry
/// </summary>
public class JournalEntryLine : BaseEntity
{
    /// <summary>
    /// Journal entry this line belongs to
    /// </summary>
    public Guid JournalEntryId { get; set; }
    
    /// <summary>
    /// Account this line affects
    /// </summary>
    public Guid AccountId { get; set; }
    
    /// <summary>
    /// Line sequence number within the journal entry
    /// </summary>
    public int LineNumber { get; set; }
    
    /// <summary>
    /// Line description
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Debit amount
    /// </summary>
    public decimal DebitAmount { get; set; } = 0;
    
    /// <summary>
    /// Credit amount
    /// </summary>
    public decimal CreditAmount { get; set; } = 0;
    
    /// <summary>
    /// Department code for cost center tracking
    /// </summary>
    [MaxLength(20)]
    public string? DepartmentCode { get; set; }
    
    /// <summary>
    /// Project code for project tracking
    /// </summary>
    [MaxLength(20)]
    public string? ProjectCode { get; set; }
    
    /// <summary>
    /// Customer/vendor reference
    /// </summary>
    [MaxLength(100)]
    public string? CustomerVendorRef { get; set; }
    
    /// <summary>
    /// Additional reference information
    /// </summary>
    [MaxLength(255)]
    public string? Reference { get; set; }
    
    /// <summary>
    /// Tax code for this line
    /// </summary>
    [MaxLength(20)]
    public string? TaxCode { get; set; }
    
    /// <summary>
    /// Tax amount for this line
    /// </summary>
    public decimal TaxAmount { get; set; } = 0;
    
    /// <summary>
    /// Base amount before tax
    /// </summary>
    public decimal BaseAmount { get; set; } = 0;
    
    /// <summary>
    /// Currency code for this line
    /// </summary>
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Exchange rate for foreign currency
    /// </summary>
    public decimal ExchangeRate { get; set; } = 1.0m;
    
    /// <summary>
    /// Amount in foreign currency
    /// </summary>
    public decimal ForeignAmount { get; set; } = 0;
    
    /// <summary>
    /// Whether this line is reconciled
    /// </summary>
    public bool IsReconciled { get; set; } = false;
    
    /// <summary>
    /// Reconciliation reference
    /// </summary>
    [MaxLength(100)]
    public string? ReconciliationRef { get; set; }
    
    /// <summary>
    /// When this line was reconciled
    /// </summary>
    public DateTime? ReconciledAt { get; set; }
    
    // Navigation properties
    public virtual JournalEntry JournalEntry { get; set; } = null!;
    public virtual Account Account { get; set; } = null!;
    
    /// <summary>
    /// Calculated property to get the net amount (Debit - Credit)
    /// </summary>
    public decimal NetAmount => DebitAmount - CreditAmount;
    
    /// <summary>
    /// Validates that either debit or credit is specified, but not both
    /// </summary>
    public bool IsValid()
    {
        // Either debit or credit must be specified, but not both
        return (DebitAmount > 0 && CreditAmount == 0) || 
               (CreditAmount > 0 && DebitAmount == 0);
    }
}
