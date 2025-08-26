using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities;
using Sivar.Erp.Core.Domain.Interfaces;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Domain.Entities.Accounting;

/// <summary>
/// Represents an account in the Chart of Accounts
/// </summary>
public class Account : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Company/tenant this account belongs to
    /// </summary>
    public Guid CompanyId { get; set; }
    
    /// <summary>
    /// Account code (e.g., "1000", "2100")
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Account name
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Account description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Account type (Asset, Liability, Equity, Revenue, Expense)
    /// </summary>
    public AccountType Type { get; set; }
    
    /// <summary>
    /// Account category for grouping
    /// </summary>
    public AccountCategory Category { get; set; }
    
    /// <summary>
    /// Parent account ID for hierarchical structure
    /// </summary>
    public Guid? ParentAccountId { get; set; }
    
    /// <summary>
    /// Whether this account is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether this account is a header account (no transactions allowed)
    /// </summary>
    public bool IsHeader { get; set; } = false;
    
    /// <summary>
    /// Tax code associated with this account
    /// </summary>
    [MaxLength(20)]
    public string? TaxCode { get; set; }
    
    /// <summary>
    /// Currency code for this account
    /// </summary>
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Current balance of the account
    /// </summary>
    public decimal Balance { get; set; } = 0;
    
    /// <summary>
    /// Opening balance for the current period
    /// </summary>
    public decimal OpeningBalance { get; set; } = 0;
    
    /// <summary>
    /// Whether this account requires department tracking
    /// </summary>
    public bool RequiresDepartment { get; set; } = false;
    
    /// <summary>
    /// Whether this account requires project tracking
    /// </summary>
    public bool RequiresProject { get; set; } = false;
    
    // Navigation properties
    public virtual Account? ParentAccount { get; set; }
    public virtual ICollection<Account> SubAccounts { get; set; } = new List<Account>();
    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}

/// <summary>
/// Account categories for more detailed classification
/// </summary>
public enum AccountCategory
{
    // Assets
    CurrentAssets = 101,
    FixedAssets = 102,
    IntangibleAssets = 103,
    Investments = 104,
    
    // Liabilities
    CurrentLiabilities = 201,
    LongTermLiabilities = 202,
    
    // Equity
    Capital = 301,
    RetainedEarnings = 302,
    
    // Revenue
    OperatingRevenue = 401,
    NonOperatingRevenue = 402,
    
    // Expenses
    CostOfGoodsSold = 501,
    OperatingExpenses = 502,
    NonOperatingExpenses = 503,
    TaxExpenses = 504
}
