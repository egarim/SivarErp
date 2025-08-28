using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Base entity class for Entity Framework entities
/// </summary>
public abstract class BaseEntity: DevExpress.Persistent.BaseImpl.EF.BaseObject
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    [Key]
    public Guid Oid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// UTC timestamp when the entity was created
    /// </summary>
    public DateTime InsertedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created the entity
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string InsertedBy { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who last updated the entity
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
}
