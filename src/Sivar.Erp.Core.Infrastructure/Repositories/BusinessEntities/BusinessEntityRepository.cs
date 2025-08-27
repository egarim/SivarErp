using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Core.Domain.Entities.BusinessEntities;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.BusinessEntities;
using Sivar.Erp.Core.Infrastructure.Data;

namespace Sivar.Erp.Core.Infrastructure.Repositories.BusinessEntities;

/// <summary>
/// Repository implementation for BusinessEntity with tenant isolation and performance optimization
/// </summary>
public class BusinessEntityRepository : TenantRepository<BusinessEntity>, IBusinessEntityRepository
{
    public BusinessEntityRepository(ErpDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get business entities by type
    /// </summary>
    public async Task<IEnumerable<BusinessEntity>> GetByTypeAsync(BusinessEntityType entityType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(be => be.EntityType == entityType)
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .Include(be => be.CreatedByUser)
            .OrderBy(be => be.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get active business entities
    /// </summary>
    public async Task<IEnumerable<BusinessEntity>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(be => be.IsActive)
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .OrderBy(be => be.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get business entities by branch
    /// </summary>
    public async Task<IEnumerable<BusinessEntity>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(be => be.BranchId == branchId)
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .Include(be => be.CreatedByUser)
            .OrderBy(be => be.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Search business entities by name or code
    /// </summary>
    public async Task<IEnumerable<BusinessEntity>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(be => be.Name.ToLower().Contains(lowerSearchTerm) || 
                        be.Code.ToLower().Contains(lowerSearchTerm) ||
                        (be.Email != null && be.Email.ToLower().Contains(lowerSearchTerm)))
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .OrderBy(be => be.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Check if a business entity with the given code exists
    /// </summary>
    public async Task<bool> ExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(be => be.Code == code);
        
        if (excludeId.HasValue)
        {
            query = query.Where(be => be.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Get business entity by code
    /// </summary>
    public async Task<BusinessEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(be => be.Code == code)
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .Include(be => be.CreatedByUser)
            .Include(be => be.UpdatedByUser)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get customers with credit limit greater than or equal to specified amount
    /// </summary>
    public async Task<IEnumerable<BusinessEntity>> GetCustomersWithCreditLimitAsync(decimal minCreditLimit, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(be => be.EntityType == BusinessEntityType.Customer && 
                        be.CreditLimit.HasValue && 
                        be.CreditLimit.Value >= minCreditLimit)
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .OrderBy(be => be.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get customers with credit limits (simplified interface version)
    /// </summary>
    public async Task<IEnumerable<BusinessEntity>> GetCustomersWithCreditLimitAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(be => be.EntityType == BusinessEntityType.Customer && be.CreditLimit.HasValue)
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .OrderBy(be => be.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get overdue customers (those who exceed payment terms)
    /// </summary>
    public async Task<IEnumerable<BusinessEntity>> GetOverdueCustomersAsync(CancellationToken cancellationToken = default)
    {
        // For now, return customers - this would need integration with invoice/payment data
        return await _dbSet
            .Where(be => be.EntityType == BusinessEntityType.Customer && be.IsActive)
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .OrderBy(be => be.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get business entities created within a date range
    /// </summary>
    public async Task<IEnumerable<BusinessEntity>> GetCreatedInRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(be => be.CreatedAt >= startDate && be.CreatedAt <= endDate)
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .OrderBy(be => be.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get business entities by email domain (for grouping by organization)
    /// </summary>
    public async Task<IEnumerable<BusinessEntity>> GetByEmailDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(be => be.Email != null && be.Email.Contains($"@{domain}"))
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .OrderBy(be => be.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get all business entities with navigation properties included
    /// </summary>
    public override async Task<IEnumerable<BusinessEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .Include(be => be.CreatedByUser)
            .Include(be => be.UpdatedByUser)
            .OrderBy(be => be.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get business entity by ID with navigation properties included
    /// </summary>
    public override async Task<BusinessEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(be => be.Id == id)
            .Include(be => be.Company)
            .Include(be => be.Branch)
            .Include(be => be.CreatedByUser)
            .Include(be => be.UpdatedByUser)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
