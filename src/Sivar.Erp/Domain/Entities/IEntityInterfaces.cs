namespace Sivar.Erp.Core.Domain.Entities
{
    /// <summary>
    /// Base interface for all domain entities
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        Guid Oid { get; set; }

        /// <summary>
        /// Indicates if the entity is valid
        /// </summary>
        bool IsValid();
    }

    /// <summary>
    /// Interface for entities that support auditing
    /// </summary>
    public interface IAuditableEntity
    {
        /// <summary>
        /// Creation timestamp
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// User who created the entity
        /// </summary>
        string CreatedBy { get; set; }

        /// <summary>
        /// Last modification timestamp
        /// </summary>
        DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// User who last modified the entity
        /// </summary>
        string? LastModifiedBy { get; set; }

        /// <summary>
        /// Marks the entity as modified
        /// </summary>
        void MarkAsModified(string userId);
    }

    /// <summary>
    /// Interface for entities that support soft deletion
    /// </summary>
    public interface ISoftDeletable
    {
        /// <summary>
        /// Soft delete flag
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Deletion timestamp
        /// </summary>
        DateTime? DeletedAt { get; set; }

        /// <summary>
        /// User who deleted the entity
        /// </summary>
        string? DeletedBy { get; set; }

        /// <summary>
        /// Marks the entity as deleted
        /// </summary>
        void MarkAsDeleted(string userId);

        /// <summary>
        /// Restores a soft-deleted entity
        /// </summary>
        void Restore(string userId);
    }

    /// <summary>
    /// Interface for entities that belong to a tenant (company/branch)
    /// </summary>
    public interface ITenantEntity
    {
        /// <summary>
        /// Company identifier for multi-tenant isolation
        /// </summary>
        Guid CompanyId { get; set; }

        /// <summary>
        /// Branch identifier for multi-branch operations
        /// </summary>
        Guid? BranchId { get; set; }

        /// <summary>
        /// Sets the tenant context for the entity
        /// </summary>
        void SetTenantContext(Guid companyId, Guid? branchId = null);
    }

    /// <summary>
    /// Interface for entities that support business activation state
    /// </summary>
    public interface IActivatable
    {
        /// <summary>
        /// Entity is active (business rule)
        /// </summary>
        bool IsActive { get; set; }
    }

    /// <summary>
    /// Interface for entities that support versioning
    /// </summary>
    public interface IVersionable
    {
        /// <summary>
        /// Version for optimistic concurrency control
        /// </summary>
        byte[] Version { get; set; }
    }

    /// <summary>
    /// Interface for entities that support metadata
    /// </summary>
    public interface IMetadataEntity
    {
        /// <summary>
        /// Additional metadata for the entity
        /// </summary>
        string? Metadata { get; set; }
    }
}
