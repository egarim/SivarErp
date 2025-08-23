using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Core.Infrastructure.Repository;

namespace Sivar.Erp.Core.Infrastructure.Repository
{
    public interface IBusinessEntityRepository : IRepository<IBusinessEntity>
    {
        // Business Entity specific operations
        Task<IBusinessEntity?> GetByBusinessKeyAsync(string businessKey);
        Task<IEnumerable<IBusinessEntity>> GetByTypeAsync(string entityType);
        Task<IEnumerable<IBusinessEntity>> SearchAsync(string searchTerm);
        Task<bool> IsBusinessKeyUniqueAsync(string businessKey, Guid? excludeId = null);
        
        // Hierarchy operations
        Task<IEnumerable<IBusinessEntity>> GetChildrenAsync(Guid parentId);
        Task<IBusinessEntity?> GetParentAsync(Guid childId);
        Task<IEnumerable<IBusinessEntity>> GetHierarchyAsync(Guid rootId);
        
        // Relationship operations
        Task<IEnumerable<IBusinessEntity>> GetRelatedEntitiesAsync(Guid entityId, string relationType);
        Task AddRelationshipAsync(Guid entityId, Guid relatedEntityId, string relationType);
        Task RemoveRelationshipAsync(Guid entityId, Guid relatedEntityId, string relationType);
        
        // Audit and tracking
        Task<IEnumerable<IBusinessEntity>> GetModifiedSinceAsync(DateTime since);
        Task<IEnumerable<IBusinessEntity>> GetCreatedByUserAsync(string userId);
        Task<IEnumerable<IBusinessEntity>> GetModifiedByUserAsync(string userId);
        
        // Advanced search
        Task<(IEnumerable<IBusinessEntity> Items, int TotalCount)> SearchPagedAsync(
            string searchTerm,
            int pageNumber,
            int pageSize,
            string? entityType = null,
            DateTime? modifiedSince = null);
    }

    // Business entity interface - simplified version to resolve dependencies
    public interface IBusinessEntity : Sivar.Erp.Core.Core.IEntity
    {
        string BusinessKey { get; set; }
        string EntityType { get; set; }
        string DisplayName { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime ModifiedAt { get; set; }
        string? CreatedBy { get; set; }
        string? ModifiedBy { get; set; }
        bool IsActive { get; set; }
        Dictionary<string, object> Properties { get; set; }
    }
}
