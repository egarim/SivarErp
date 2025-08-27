using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.Core.Domain.Entities
{
    /// <summary>
    /// Base entity for all domain entities with enhanced monitoring support
    /// </summary>
    public abstract class BaseEntity : IEntity, IAuditableEntity, ITenantEntity
    {
        /// <summary>
        /// Primary key identifier
        /// </summary>
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Company identifier for multi-tenant isolation
        /// </summary>
        [Required]
        public Guid CompanyId { get; set; }

        /// <summary>
        /// Branch identifier for multi-branch operations
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who created the entity (Keycloak user ID)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Last modification timestamp
        /// </summary>
        public DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// User who last modified the entity (Keycloak user ID)
        /// </summary>
        [MaxLength(100)]
        public string? LastModifiedBy { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Deletion timestamp
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// User who deleted the entity (Keycloak user ID)
        /// </summary>
        [MaxLength(100)]
        public string? DeletedBy { get; set; }

        /// <summary>
        /// Entity is active (business rule)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Version for optimistic concurrency control
        /// </summary>
        [Timestamp]
        public byte[] Version { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Additional metadata for the entity
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? Metadata { get; set; }

        /// <summary>
        /// Marks the entity as modified by setting audit fields
        /// </summary>
        /// <param name="userId">Keycloak user ID</param>
        public virtual void MarkAsModified(string userId)
        {
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = userId;
        }

        /// <summary>
        /// Marks the entity as deleted (soft delete)
        /// </summary>
        /// <param name="userId">Keycloak user ID</param>
        public virtual void MarkAsDeleted(string userId)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = userId;
            IsActive = false;
        }

        /// <summary>
        /// Restores a soft-deleted entity
        /// </summary>
        /// <param name="userId">Keycloak user ID</param>
        public virtual void Restore(string userId)
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            IsActive = true;
            MarkAsModified(userId);
        }

        /// <summary>
        /// Sets the tenant context for the entity
        /// </summary>
        /// <param name="companyId">Company identifier</param>
        /// <param name="branchId">Branch identifier (optional)</param>
        public virtual void SetTenantContext(Guid companyId, Guid? branchId = null)
        {
            CompanyId = companyId;
            BranchId = branchId;
        }

        /// <summary>
        /// Validates the entity state
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public virtual bool IsValid()
        {
            return CompanyId != Guid.Empty && 
                   !string.IsNullOrWhiteSpace(CreatedBy) &&
                   CreatedAt != default;
        }

        /// <summary>
        /// Returns a string representation of the entity
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name} - {Oid}";
        }

        /// <summary>
        /// Equality comparison based on ID
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not BaseEntity other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            if (Oid == Guid.Empty || other.Oid == Guid.Empty)
                return false;

            return Oid == other.Oid;
        }

        /// <summary>
        /// Hash code based on ID
        /// </summary>
        public override int GetHashCode()
        {
            return Oid.GetHashCode();
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(BaseEntity? left, BaseEntity? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(BaseEntity? left, BaseEntity? right)
        {
            return !Equals(left, right);
        }
    }
}
