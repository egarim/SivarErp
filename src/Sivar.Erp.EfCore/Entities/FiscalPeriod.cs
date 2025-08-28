using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Core.Enums;

using Sivar.Erp.Modules.Accounting.FiscalPeriods;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Fiscal Periods
/// </summary>
[Table("FiscalPeriods")]
public class FiscalPeriod : BaseEntity, IFiscalPeriod
{
    /// <summary>
    /// Unique code for the fiscal period
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; set; } = string.Empty;

    /// <summary>
    /// Start date of the fiscal period
    /// </summary>
    public virtual DateOnly StartDate { get; set; }

    /// <summary>
    /// End date of the fiscal period
    /// </summary>
    public virtual DateOnly EndDate { get; set; }

    /// <summary>
    /// Name of the fiscal period
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the fiscal period is currently open
    /// </summary>
    public virtual bool IsOpen { get; set; } = true;

    /// <summary>
    /// Whether this is the current active fiscal period
    /// </summary>
    public virtual bool IsCurrent { get; set; } = false;
    public virtual FiscalPeriodStatus Status { get; set; }
    public virtual string Description { get; set; } = string.Empty;
 
}
