using Sivar.Erp.Core.Domain.Entities.Accounting;
using Sivar.Erp.Core.Domain.Interfaces;

namespace Sivar.Erp.Core.Domain.Interfaces.Repositories.Accounting;

/// <summary>
/// Repository interface for Account entities
/// </summary>
public interface IAccountRepository : ITenantRepository<Account>
{
    /// <summary>
    /// Gets account by code within a company
    /// </summary>
    Task<Account?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts by type
    /// </summary>
    Task<IEnumerable<Account>> GetByTypeAsync(Enums.AccountType type, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts by category
    /// </summary>
    Task<IEnumerable<Account>> GetByCategoryAsync(AccountCategory category, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets chart of accounts in hierarchical structure
    /// </summary>
    Task<IEnumerable<Account>> GetChartOfAccountsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets sub-accounts of a parent account
    /// </summary>
    Task<IEnumerable<Account>> GetSubAccountsAsync(Guid parentAccountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts that have sub-accounts
    /// </summary>
    Task<IEnumerable<Account>> GetHeaderAccountsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets posting accounts (accounts that can have transactions)
    /// </summary>
    Task<IEnumerable<Account>> GetPostingAccountsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if account code exists within company
    /// </summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if account has transactions
    /// </summary>
    Task<bool> HasTransactionsAsync(Guid accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets account balance
    /// </summary>
    Task<decimal> GetBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates account balance
    /// </summary>
    Task UpdateBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches accounts by name or code
    /// </summary>
    Task<IEnumerable<Account>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
