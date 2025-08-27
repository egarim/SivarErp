using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities.Accounting;
using AccountTypeEnum = Sivar.Erp.Core.Domain.Enums.AccountType;

namespace Sivar.Erp.Core.Application.DTOs.Accounting;

/// <summary>
/// DTO for creating a new account
/// </summary>
public class CreateAccountDto
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public AccountTypeEnum Type { get; set; }
    
    [Required]
    public AccountCategory Category { get; set; }
    
    public Guid? ParentAccountId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsHeader { get; set; } = false;
    
    [MaxLength(20)]
    public string? TaxCode { get; set; }
    
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    public decimal OpeningBalance { get; set; } = 0;
    
    public bool RequiresDepartment { get; set; } = false;
    
    public bool RequiresProject { get; set; } = false;
}

/// <summary>
/// DTO for updating an account
/// </summary>
public class UpdateAccountDto
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public AccountTypeEnum Type { get; set; }
    
    [Required]
    public AccountCategory Category { get; set; }
    
    public Guid? ParentAccountId { get; set; }
    
    public bool IsActive { get; set; }
    
    public bool IsHeader { get; set; }
    
    [MaxLength(20)]
    public string? TaxCode { get; set; }
    
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    public bool RequiresDepartment { get; set; }
    
    public bool RequiresProject { get; set; }
}

/// <summary>
/// DTO for account response
/// </summary>
public class AccountDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AccountTypeEnum Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public AccountCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public bool IsActive { get; set; }
    public bool IsHeader { get; set; }
    public string? TaxCode { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal OpeningBalance { get; set; }
    public bool RequiresDepartment { get; set; }
    public bool RequiresProject { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    
    // Hierarchical data
    public int Level { get; set; }
    public List<AccountDto> SubAccounts { get; set; } = new();
    public bool HasSubAccounts { get; set; }
}

/// <summary>
/// DTO for account summary
/// </summary>
public class AccountSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountTypeEnum Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public AccountCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsHeader { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// DTO for chart of accounts query
/// </summary>
public class ChartOfAccountsQueryDto
{
    public AccountTypeEnum? Type { get; set; }
    public AccountCategory? Category { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsHeader { get; set; }
    public string? SearchTerm { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public bool FlatStructure { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
