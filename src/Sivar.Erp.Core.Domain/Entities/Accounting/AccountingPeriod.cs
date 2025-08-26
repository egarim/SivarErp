using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities;
using Sivar.Erp.Core.Domain.Interfaces;

namespace Sivar.Erp.Core.Domain.Entities.Accounting;

/// <summary>
/// Represents an accounting period for financial reporting
/// </summary>
public class AccountingPeriod : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Company/tenant this period belongs to
    /// </summary>
    public Guid CompanyId { get; set; }
    
    /// <summary>
    /// Fiscal year this period belongs to
    /// </summary>
    public Guid FiscalYearId { get; set; }
    
    /// <summary>
    /// Period name (e.g., "January 2024", "Q1 2024")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Period start date
    /// </summary>
    public DateOnly StartDate { get; set; }
    
    /// <summary>
    /// Period end date
    /// </summary>
    public DateOnly EndDate { get; set; }
    
    /// <summary>
    /// Period status
    /// </summary>
    public PeriodStatus Status { get; set; } = PeriodStatus.Open;
    
    /// <summary>
    /// Period type (Monthly, Quarterly, Yearly, etc.)
    /// </summary>
    public PeriodType Type { get; set; }
    
    /// <summary>
    /// Period number within the fiscal year
    /// </summary>
    public int PeriodNumber { get; set; }
    
    /// <summary>
    /// Whether this is an adjustment period
    /// </summary>
    public bool IsAdjustmentPeriod { get; set; } = false;
    
    /// <summary>
    /// When this period was closed
    /// </summary>
    public DateTime? ClosedAt { get; set; }
    
    /// <summary>
    /// Who closed this period
    /// </summary>
    [MaxLength(255)]
    public string? ClosedBy { get; set; }
    
    /// <summary>
    /// Notes about period closure
    /// </summary>
    [MaxLength(1000)]
    public string? ClosureNotes { get; set; }
    
    // Navigation properties
    public virtual FiscalYear FiscalYear { get; set; } = null!;
    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}

/// <summary>
/// Represents a fiscal year
/// </summary>
public class FiscalYear : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Company/tenant this fiscal year belongs to
    /// </summary>
    public Guid CompanyId { get; set; }
    
    /// <summary>
    /// Fiscal year name (e.g., "FY 2024")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Fiscal year start date
    /// </summary>
    public DateOnly StartDate { get; set; }
    
    /// <summary>
    /// Fiscal year end date
    /// </summary>
    public DateOnly EndDate { get; set; }
    
    /// <summary>
    /// Fiscal year status
    /// </summary>
    public FiscalYearStatus Status { get; set; } = FiscalYearStatus.Open;
    
    /// <summary>
    /// Whether this is the current active fiscal year
    /// </summary>
    public bool IsCurrent { get; set; } = false;
    
    /// <summary>
    /// When this fiscal year was closed
    /// </summary>
    public DateTime? ClosedAt { get; set; }
    
    /// <summary>
    /// Who closed this fiscal year
    /// </summary>
    [MaxLength(255)]
    public string? ClosedBy { get; set; }
    
    // Navigation properties
    public virtual ICollection<AccountingPeriod> Periods { get; set; } = new List<AccountingPeriod>();
}

/// <summary>
/// Accounting period status
/// </summary>
public enum PeriodStatus
{
    Future = 1,
    Open = 2,
    Closed = 3,
    Locked = 4
}

/// <summary>
/// Period type enumeration
/// </summary>
public enum PeriodType
{
    Monthly = 1,
    Quarterly = 2,
    SemiAnnual = 3,
    Annual = 4,
    Adjustment = 5
}

/// <summary>
/// Fiscal year status
/// </summary>
public enum FiscalYearStatus
{
    Future = 1,
    Open = 2,
    Closed = 3,
    Locked = 4
}
