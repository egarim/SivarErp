using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Modules.Taxes.TaxGroup;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Tax Groups
/// </summary>
[Table("TaxGroups")]
[Index(nameof(Code), IsUnique = true, Name = "IX_TaxGroups_Code")]
public class TaxGroup : BaseEntity, ITaxGroup
{
    /// <summary>
    /// Unique code for the tax group
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the tax group
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the tax group
    /// </summary>
    [MaxLength(500)]
    public virtual string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether the tax group is currently active
    /// </summary>
    public virtual bool IsEnabled { get; set; } = true;

    // PropertyChanged implementation (required by interface but not needed for EF)
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}