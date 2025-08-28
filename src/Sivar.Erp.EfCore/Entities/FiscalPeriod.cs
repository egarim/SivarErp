using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Core.Enums;
using Sivar.Erp.Modules.Accounting.Domain.FiscalPeriods;

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
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Start date of the fiscal period
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// End date of the fiscal period
    /// </summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Name of the fiscal period
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the fiscal period is currently open
    /// </summary>
    public bool IsOpen { get; set; } = true;

    /// <summary>
    /// Whether this is the current active fiscal period
    /// </summary>
    public bool IsCurrent { get; set; } = false;
    public FiscalPeriodStatus Status { get; set; }
    public string Description { get; set; }
}
