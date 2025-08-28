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
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Name of the sequence
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current value of the sequence
    /// </summary>
    public long CurrentValue { get; set; } = 0;

    /// <summary>
    /// Increment value for the sequence
    /// </summary>
    public int IncrementBy { get; set; } = 1;

    /// <summary>
    /// Prefix for the generated numbers
    /// </summary>
    [MaxLength(50)]
    public string? Prefix { get; set; }

    /// <summary>
    /// Suffix for the generated numbers
    /// </summary>
    [MaxLength(50)]
    public string? Suffix { get; set; }

    /// <summary>
    /// Minimum length of the number part (with zero padding)
    /// </summary>
    public int MinLength { get; set; } = 1;

    /// <summary>
    /// Whether the sequence is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
