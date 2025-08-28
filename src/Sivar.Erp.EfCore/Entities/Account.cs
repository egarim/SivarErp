using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.Persistent.Base;
using Sivar.Erp.Core.Enums;
using Sivar.Erp.Modules.Accounting.ChartOfAccounts;

namespace Sivar.Erp.EfCore.Entities;

[DefaultClassOptions()]
/// <summary>
/// Entity Framework entity for Chart of Accounts
/// </summary>
[Table("Accounts")]
public class Account : BaseEntity, IAccount
{
    /// <summary>
    /// Optional reference to balance sheet or income statement line
    /// </summary>
    public virtual Guid? BalanceAndIncomeLineId { get; set; }

    /// <summary>
    /// Name of the account
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Type of account (asset, liability, etc.)
    /// </summary>
    public virtual AccountType AccountType { get; set; }

    /// <summary>
    /// Official code/identifier for the account (e.g., for SAF-T reporting)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string OfficialCode { get; set; } = string.Empty;

    /// <summary>
    /// Official code/identifier for the parent account 
    /// </summary>
    [MaxLength(50)]
    public virtual string? ParentOfficialCode { get; set; }
}
