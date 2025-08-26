using System.Linq.Expressions;
using Sivar.Erp.Core.Domain.Entities;

namespace Sivar.Erp.Core.Domain.Interfaces;

/// <summary>
/// Generic repository interface with enhanced performance tracking integration
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    // Query operations
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    // Paging operations
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);
    
    // Count operations
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    // Modification operations
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    // Soft delete operations (for entities that support it)
    Task SoftDeleteAsync(Guid id, string? deletedBy = null, CancellationToken cancellationToken = default);
    Task SoftDeleteRangeAsync(IEnumerable<Guid> ids, string? deletedBy = null, CancellationToken cancellationToken = default);
    Task RestoreAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Queryable for complex operations
    IQueryable<T> Query { get; }
    IQueryable<T> QueryAsNoTracking { get; }
}

/// <summary>
/// Repository interface for tenant-specific entities
/// </summary>
/// <typeparam name="T">Entity type that implements ITenantEntity</typeparam>
public interface ITenantRepository<T> : IRepository<T> where T : class, ITenantEntity
{
    // Tenant-specific operations
    Task<IEnumerable<T>> GetAllByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAndCompanyAsync(Guid id, Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindByCompanyAsync(Guid companyId, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedByCompanyAsync(
        Guid companyId,
        int pageNumber, 
        int pageSize, 
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);
    Task<int> CountByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<int> CountByCompanyAsync(Guid companyId, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
