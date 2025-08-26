using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Sivar.Erp.Core.Domain.Entities;
using Sivar.Erp.Core.Domain.Interfaces;

namespace Sivar.Erp.Core.Infrastructure.Repositories;

/// <summary>
/// Tenant-aware repository implementation for multi-tenant entities
/// </summary>
/// <typeparam name="T">Entity type that implements ITenantEntity</typeparam>
public class TenantRepository<T> : GenericRepository<T>, ITenantRepository<T> where T : class, ITenantEntity
{
    public TenantRepository(DbContext context) : base(context)
    {
    }

    public virtual async Task<IEnumerable<T>> GetAllByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.CompanyId == companyId).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> GetByIdAndCompanyAsync(Guid id, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id && e.CompanyId == companyId, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> FindByCompanyAsync(Guid companyId, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.CompanyId == companyId).Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedByCompanyAsync(
        Guid companyId,
        int pageNumber, 
        int pageSize, 
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.Where(e => e.CompanyId == companyId);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (orderBy != null)
        {
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<int> CountByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(e => e.CompanyId == companyId, cancellationToken);
    }

    public virtual async Task<int> CountByCompanyAsync(Guid companyId, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(e => e.CompanyId == companyId && predicate.Compile()(e), cancellationToken);
    }
}
