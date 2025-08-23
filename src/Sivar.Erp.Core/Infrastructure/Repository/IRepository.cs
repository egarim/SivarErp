using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Infrastructure.Repository
{
    public interface IRepository<T> where T : class, IEntity
    {
        // Basic CRUD operations
        Task<T> CreateAsync(T entity);
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(T entity);

        // Query operations
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        // Bulk operations
        Task<IEnumerable<T>> CreateManyAsync(IEnumerable<T> entities);
        Task<IEnumerable<T>> UpdateManyAsync(IEnumerable<T> entities);
        Task DeleteManyAsync(IEnumerable<T> entities);
        Task DeleteManyAsync(Expression<Func<T, bool>> predicate);

        // Transaction support
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation);
        Task ExecuteInTransactionAsync(Func<Task> operation);

        // Paging support
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true);
    }

    // Non-generic base interface
    public interface IRepository
    {
        Task SaveChangesAsync();
        Task<bool> ExistsAsync(Type entityType, Guid id);
        Task<object?> GetByIdAsync(Type entityType, Guid id);
        Task DeleteAsync(Type entityType, Guid id);
    }
}
