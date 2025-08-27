using Sivar.Erp.Core.Domain.Entities.BusinessEntities;
using Sivar.Erp.Core.Domain.Interfaces;

namespace Sivar.Erp.Core.Domain.Interfaces.Repositories.BusinessEntities;

/// <summary>
/// Repository interface for Business Entity operations
/// </summary>
public interface IBusinessEntityRepository : ITenantRepository<BusinessEntity>
{
    /// <summary>
    /// Get business entities by type
    /// </summary>
    Task<IEnumerable<BusinessEntity>> GetByTypeAsync(BusinessEntityType entityType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active business entities
    /// </summary>
    Task<IEnumerable<BusinessEntity>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business entities by branch
    /// </summary>
    Task<IEnumerable<BusinessEntity>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search business entities by name or code
    /// </summary>
    Task<IEnumerable<BusinessEntity>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a business entity code exists for the company
    /// </summary>
    Task<bool> ExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business entity by code
    /// </summary>
    Task<BusinessEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customers with credit limits
    /// </summary>
    Task<IEnumerable<BusinessEntity>> GetCustomersWithCreditLimitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue customers (those who exceed payment terms)
    /// </summary>
    Task<IEnumerable<BusinessEntity>> GetOverdueCustomersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business entities created within a date range
    /// </summary>
    Task<IEnumerable<BusinessEntity>> GetCreatedInRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business entities by email domain (for grouping by organization)
    /// </summary>
    Task<IEnumerable<BusinessEntity>> GetByEmailDomainAsync(string domain, CancellationToken cancellationToken = default);
}
