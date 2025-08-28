using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Taxes;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Taxes
/// </summary>
[Table("Taxes")]
public class Tax : BaseEntity, ITax
{
    /// <summary>
    /// Display name of the tax
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short code for the tax (e.g., VAT, GST)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; set; } = string.Empty;

    /// <summary>
    /// Type of tax calculation (by percentage, fixed amount, or per quantity)
    /// </summary>
    public virtual TaxType TaxType { get; set; }

    /// <summary>
    /// Level at which the tax should be applied (line or document)
    /// </summary>
    public virtual TaxApplicationLevel ApplicationLevel { get; set; }

    /// <summary>
    /// Fixed amount to apply when TaxType is FixedAmount or AmountPerUnit
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public virtual decimal Amount { get; set; }

    /// <summary>
    /// Percentage to apply when TaxType is Percentage
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public virtual decimal Percentage { get; set; }

    /// <summary>
    /// Whether this tax is currently active
    /// </summary>
    public virtual bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Whether the tax is already included in the line price
    /// </summary>
    public virtual bool IsIncludedInPrice { get; set; }
}
