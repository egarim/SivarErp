using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Sequences
/// </summary>
[Table("Sequences")]
public class Sequence : BaseEntity
{
    /// <summary>
    /// Unique code for the sequence
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; set; } = string.Empty;

    /// <summary>
    /// Name of the sequence
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current value of the sequence
    /// </summary>
    public virtual long CurrentValue { get; set; } = 0;

    /// <summary>
    /// Increment value for the sequence
    /// </summary>
    public virtual int IncrementBy { get; set; } = 1;

    /// <summary>
    /// Prefix for the generated numbers
    /// </summary>
    [MaxLength(50)]
    public virtual string? Prefix { get; set; }

    /// <summary>
    /// Suffix for the generated numbers
    /// </summary>
    [MaxLength(50)]
    public virtual string? Suffix { get; set; }

    /// <summary>
    /// Minimum length of the number part (with zero padding)
    /// </summary>
    public virtual int MinLength { get; set; } = 1;

    /// <summary>
    /// Whether the sequence is currently active
    /// </summary>
    public virtual bool IsActive { get; set; } = true;
}
