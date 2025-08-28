using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Payments.Models;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Payments
/// </summary>
[Table("Payments")]
public class Payment : BaseEntity
{
    /// <summary>
    /// Unique payment identifier
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string PaymentId { get; set; } = string.Empty;

    /// <summary>
    /// Document number associated with this payment
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to PaymentMethod
    /// </summary>
    public virtual Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Navigation property to PaymentMethod
    /// </summary>
    [ForeignKey(nameof(PaymentMethodId))]
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    /// <summary>
    /// Payment amount
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public virtual decimal Amount { get; set; }

    /// <summary>
    /// Date when the payment was made
    /// </summary>
    public virtual DateOnly PaymentDate { get; set; }

    /// <summary>
    /// Reference number for the payment
    /// </summary>
    [MaxLength(200)]
    public virtual string? Reference { get; set; }

    /// <summary>
    /// Bank account used for the payment
    /// </summary>
    [MaxLength(100)]
    public virtual string? BankAccount { get; set; }

    /// <summary>
    /// Payment status
    /// </summary>
    public virtual PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Additional notes about the payment
    /// </summary>
    [MaxLength(1000)]
    public virtual string? Notes { get; set; }

    /// <summary>
    /// Additional data (stored as JSON)
    /// </summary>
    public virtual string? AdditionalData { get; set; }
}
