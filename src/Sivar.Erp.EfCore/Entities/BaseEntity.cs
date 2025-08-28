using DevExpress.Persistent.BaseImpl.EF;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Base entity class for Entity Framework entities
/// </summary>
public abstract class BaseEntity: BaseObject
{
    ///// <summary>
    ///// Unique identifier for the entity
    ///// </summary>
    //[Key]
    //public virtual Guid Oid { get; set; } = Guid.NewGuid();

    [Browsable(false)]
    /// <summary>
    /// UTC timestamp when the entity was created
    /// </summary>
    public virtual DateTime InsertedAt { get; set; } = DateTime.UtcNow;

    [Browsable(false)]
    /// <summary>
    /// User who created the entity
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string InsertedBy { get; set; } = string.Empty;

    [Browsable(false)]
    /// <summary>
    /// UTC timestamp when the entity was last updated
    /// </summary>
    public virtual DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Browsable(false)]
    /// <summary>
    /// User who last updated the entity
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string UpdatedBy { get; set; } = string.Empty;
}
