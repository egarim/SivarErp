using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Core.Domain.Entities;

/// <summary>
/// Base entity with enhanced telemetry tracking integration
/// </summary>
public abstract class BaseEntity : IEntity
{
    [Key]
    public virtual Guid Id { get; set; } = Guid.NewGuid();
    
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual DateTime? UpdatedAt { get; set; }
    
    public virtual string? CreatedBy { get; set; }
    
    public virtual string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Version for optimistic concurrency control
    /// </summary>
    public virtual byte[] Version { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Marks entity as soft deleted
    /// </summary>
    public virtual bool IsDeleted { get; set; }
    
    public virtual DateTime? DeletedAt { get; set; }
    
    public virtual string? DeletedBy { get; set; }

    /// <summary>
    /// Updates the audit fields for modification
    /// </summary>
    /// <param name="userId">The user making the modification</param>
    public virtual void SetModified(string? userId = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    /// <summary>
    /// Marks entity as soft deleted
    /// </summary>
    /// <param name="userId">The user performing the deletion</param>
    public virtual void SetDeleted(string? userId = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
    }

    /// <summary>
    /// Restores a soft deleted entity
    /// </summary>
    public virtual void SetRestored()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
