using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Payments.Models;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Payment Methods
/// </summary>
[Table("PaymentMethods")]
public class PaymentMethod : BaseEntity
{
    /// <summary>
    /// Unique code for the payment method
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Name of the payment method
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of payment method
    /// </summary>
    public PaymentMethodType Type { get; set; }

    /// <summary>
    /// GL Account for this payment method
    /// </summary>
    [MaxLength(50)]
    public string? AccountCode { get; set; }

    /// <summary>
    /// Whether this payment method requires a bank account
    /// </summary>
    public bool RequiresBankAccount { get; set; }

    /// <summary>
    /// Whether this payment method requires a reference
    /// </summary>
    public bool RequiresReference { get; set; }

    /// <summary>
    /// Whether this payment method is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Additional properties (stored as JSON)
    /// </summary>
    public string? AdditionalProperties { get; set; }
}
