using Sivar.Erp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sivar.Erp.Core.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface with integration to ErpLoggingService and telemetry
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Access to the underlying DbContext for complex operations
    /// </summary>
    DbContext Context { get; }
    
    /// <summary>
    /// Saves all changes made in this unit of work to the underlying database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of entities written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves all changes made in this unit of work with audit information
    /// </summary>
    /// <param name="userId">User making the changes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of entities written to the database</returns>
    Task<int> SaveChangesAsync(string? userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begins a database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction that can be committed or rolled back</returns>
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a repository for the specified entity type
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <returns>Repository instance</returns>
    IRepository<T> Repository<T>() where T : class;
    
    /// <summary>
    /// Gets a tenant-specific repository for the specified entity type
    /// </summary>
    /// <typeparam name="T">Entity type that implements ITenantEntity</typeparam>
    /// <returns>Tenant repository instance</returns>
    ITenantRepository<T> TenantRepository<T>() where T : class, ITenantEntity;
    
    /// <summary>
    /// Discards all changes made in this unit of work
    /// </summary>
    void RejectChanges();
    
    /// <summary>
    /// Detaches all tracked entities
    /// </summary>
    void DetachAll();
    
    /// <summary>
    /// Gets the current company context for tenant isolation
    /// </summary>
    Guid? CurrentCompanyId { get; }
    
    /// <summary>
    /// Sets the current company context for tenant isolation
    /// </summary>
    /// <param name="companyId">Company ID</param>
    void SetCompanyContext(Guid companyId);
    
    /// <summary>
    /// Clears the current company context
    /// </summary>
    void ClearCompanyContext();
}

/// <summary>
/// Database transaction interface
/// </summary>
public interface IDbTransaction : IDisposable
{
    /// <summary>
    /// Commits the transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CommitAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rolls back the transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
