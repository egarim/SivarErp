using Sivar.Erp.Core.Domain.Entities.Documents;

namespace Sivar.Erp.Core.Domain.Interfaces
{
    /// <summary>
    /// Unit of Work pattern interface for managing database transactions
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Save all pending changes to the database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begin a new database transaction
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get repository for document entities
        /// </summary>
        IDocumentRepository Documents { get; }

        /// <summary>
        /// Get repository for document type entities
        /// </summary>
        IDocumentTypeRepository DocumentTypes { get; }

        /// <summary>
        /// Get repository for business entity entities
        /// </summary>
        IBusinessEntityRepository BusinessEntities { get; }

        /// <summary>
        /// Get repository for item entities
        /// </summary>
        IItemRepository Items { get; }
    }

    /// <summary>
    /// Generic repository interface for entity operations
    /// </summary>
    public interface IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Get entity by ID
        /// </summary>
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all entities
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Add new entity
        /// </summary>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update existing entity
        /// </summary>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete entity (soft delete)
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find entities matching predicate
        /// </summary>
        Task<IEnumerable<T>> FindAsync(
            System.Linq.Expressions.Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if entity exists
        /// </summary>
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Document repository interface with specialized operations
    /// </summary>
    public interface IDocumentRepository : IRepository<Document>
    {
        /// <summary>
        /// Get document by document number
        /// </summary>
        Task<Document?> GetByDocumentNumberAsync(
            string documentNumber,
            Guid companyId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get documents with pagination and filtering
        /// </summary>
        Task<(IEnumerable<Document> Documents, int TotalCount)> GetDocumentsAsync(
            Guid companyId,
            Guid? documentTypeId = null,
            string? status = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            int pageSize = 50,
            int pageNumber = 1,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get document with all related data
        /// </summary>
        Task<Document?> GetDocumentWithDetailsAsync(
            Guid documentId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get documents by business entity
        /// </summary>
        Task<IEnumerable<Document>> GetDocumentsByBusinessEntityAsync(
            Guid businessEntityId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get document statistics
        /// </summary>
        Task<DocumentStatistics> GetStatisticsAsync(
            Guid companyId,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if document number exists
        /// </summary>
        Task<bool> DocumentNumberExistsAsync(
            string documentNumber,
            Guid companyId,
            Guid? excludeDocumentId = null,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Document type repository interface
    /// </summary>
    public interface IDocumentTypeRepository : IRepository<DocumentType>
    {
        /// <summary>
        /// Get document type by code
        /// </summary>
        Task<DocumentType?> GetByCodeAsync(
            string code,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get active document types
        /// </summary>
        Task<IEnumerable<DocumentType>> GetActiveAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get document types by category
        /// </summary>
        Task<IEnumerable<DocumentType>> GetByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get next document number for type
        /// </summary>
        Task<string> GetNextDocumentNumberAsync(
            Guid documentTypeId,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Business entity repository interface
    /// </summary>
    public interface IBusinessEntityRepository : IRepository<BusinessEntity>
    {
        /// <summary>
        /// Get business entity by code
        /// </summary>
        Task<BusinessEntity?> GetByCodeAsync(
            string code,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get active business entities
        /// </summary>
        Task<IEnumerable<BusinessEntity>> GetActiveAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get business entities by type
        /// </summary>
        Task<IEnumerable<BusinessEntity>> GetByTypeAsync(
            string entityType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Search business entities
        /// </summary>
        Task<IEnumerable<BusinessEntity>> SearchAsync(
            string searchTerm,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Item repository interface
    /// </summary>
    public interface IItemRepository : IRepository<Item>
    {
        /// <summary>
        /// Get item by code
        /// </summary>
        Task<Item?> GetByCodeAsync(
            string code,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get active items
        /// </summary>
        Task<IEnumerable<Item>> GetActiveAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get items by category
        /// </summary>
        Task<IEnumerable<Item>> GetByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Search items
        /// </summary>
        Task<IEnumerable<Item>> SearchAsync(
            string searchTerm,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get inventory items only
        /// </summary>
        Task<IEnumerable<Item>> GetInventoryItemsAsync(
            CancellationToken cancellationToken = default);
    }
}
