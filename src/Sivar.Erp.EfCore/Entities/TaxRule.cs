using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Documents.Core.Enums;
using Sivar.Erp.Modules.Taxes.TaxRule;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Tax Rules
/// </summary>
[Table("TaxRules")]
public class TaxRule : BaseEntity, ITaxRule
{
    /// <summary>
    /// Reference to the tax that should be applied
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string TaxId { get; set; } = string.Empty;

    /// <summary>
    /// Document operation this rule applies to (null means any document operation)
    /// </summary>
    public virtual DocumentOperation? DocumentOperation { get; set; }

    /// <summary>
    /// Business entity group ID this rule applies to (null means any entity)
    /// </summary>
    [MaxLength(50)]
    public virtual string? BusinessEntityGroupId { get; set; }

    /// <summary>
    /// Item group ID this rule applies to (null means any item)
    /// </summary>
    [MaxLength(50)]
    public virtual string? ItemGroupId { get; set; }

    /// <summary>
    /// Whether this tax rule is currently active
    /// </summary>
    public virtual bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Priority of the rule (lower numbers = higher priority)
    /// </summary>
    public virtual int Priority { get; set; } = 1;
}
