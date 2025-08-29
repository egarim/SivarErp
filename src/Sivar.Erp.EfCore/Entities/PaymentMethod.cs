using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Payments.Models;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Payment Methods
/// </summary>
[Table("PaymentMethods")]
public class PaymentMethod : BaseEntity, IPaymentMethod
{
    /// <summary>
    /// Unique code for the payment method
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; set; } = string.Empty;

    /// <summary>
    /// Name of the payment method
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of payment method
    /// </summary>
    public virtual PaymentMethodType Type { get; set; }

    /// <summary>
    /// GL Account for this payment method
    /// </summary>
    [MaxLength(50)]
    public virtual string? AccountCode { get; set; }

    /// <summary>
    /// Whether this payment method requires a bank account
    /// </summary>
    public virtual bool RequiresBankAccount { get; set; }

    /// <summary>
    /// Whether this payment method requires a reference
    /// </summary>
    public virtual bool RequiresReference { get; set; }

    /// <summary>
    /// Whether this payment method is currently active
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    /// <summary>
    /// Additional properties (stored as JSON)
    /// </summary>
    public virtual string? AdditionalProperties { get; set; }
    [NotMapped]
    Dictionary<string, string> IPaymentMethod.AdditionalProperties { get; set; }
}
