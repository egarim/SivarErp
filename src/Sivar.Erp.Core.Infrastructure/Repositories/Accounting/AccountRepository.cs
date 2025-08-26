using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Core.Domain.Entities.Accounting;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.Accounting;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Infrastructure.Repositories;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Infrastructure.Repositories.Accounting;

/// <summary>
/// Repository implementation for Account entities
/// </summary>
public class AccountRepository : TenantRepository<Account>, IAccountRepository
{
    private readonly ErpDbContext _erpContext;

    public AccountRepository(ErpDbContext context) : base(context)
    {
        _erpContext = context;
    }

    public async Task<Account?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetByTypeAsync(AccountType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Type == type)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetByCategoryAsync(AccountCategory category, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Category == category)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetChartOfAccountsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(a => a.IsActive);
        }

        return await query
            .Include(a => a.ParentAccount)
            .Include(a => a.SubAccounts)
            .OrderBy(a => a.Type)
            .ThenBy(a => a.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetSubAccountsAsync(Guid parentAccountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.ParentAccountId == parentAccountId)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetHeaderAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.IsHeader)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetPostingAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => !a.IsHeader && a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(a => a.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasTransactionsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _erpContext.JournalEntryLines
            .AnyAsync(jel => jel.AccountId == accountId, cancellationToken);
    }

    public async Task<decimal> GetBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await GetByIdAsync(accountId, cancellationToken);
        return account?.Balance ?? 0;
    }

    public async Task UpdateBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        var account = await GetByIdAsync(accountId, cancellationToken);
        if (account != null)
        {
            account.Balance += amount;
            await _erpContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<Account>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(a => a.Code.ToLower().Contains(normalizedSearchTerm) ||
                       a.Name.ToLower().Contains(normalizedSearchTerm) ||
                       (a.Description != null && a.Description.ToLower().Contains(normalizedSearchTerm)))
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);
    }
}
