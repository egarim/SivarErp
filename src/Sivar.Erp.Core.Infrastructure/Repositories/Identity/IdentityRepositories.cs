using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Core.Domain.Entities.Identity;
using Sivar.Erp.Core.Domain.Interfaces;
using Sivar.Erp.Core.Infrastructure.Data;

namespace Sivar.Erp.Core.Infrastructure.Repositories.Identity;

/// <summary>
/// Company repository with business-specific operations
/// </summary>
public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Company>> GetUserCompaniesAsync(string keycloakUserId, CancellationToken cancellationToken = default);
    Task<bool> IsUserOwnerAsync(Guid companyId, string keycloakUserId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}

public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
{
    public CompanyRepository(ErpDbContext context) : base(context)
    {
    }

    public async Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Company>> GetUserCompaniesAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserCompany>()
            .Where(uc => uc.User.KeycloakUserId == keycloakUserId && uc.IsActive)
            .Include(uc => uc.Company)
            .Select(uc => uc.Company)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsUserOwnerAsync(Guid companyId, string keycloakUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserCompany>()
            .AnyAsync(uc => uc.CompanyId == companyId && 
                           uc.User.KeycloakUserId == keycloakUserId && 
                           uc.IsOwner && 
                           uc.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Name == name, cancellationToken);
    }
}

/// <summary>
/// User repository with Keycloak integration
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByKeycloakIdAsync(string keycloakUserId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetCompanyUsersAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByKeycloakIdAsync(string keycloakUserId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ErpDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByKeycloakIdAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.KeycloakUserId == keycloakUserId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetCompanyUsersAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserCompany>()
            .Where(uc => uc.CompanyId == companyId && uc.IsActive)
            .Include(uc => uc.User)
            .Select(uc => uc.User)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByKeycloakIdAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.KeycloakUserId == keycloakUserId, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
    }
}

/// <summary>
/// User invitation repository with token management
/// </summary>
public interface IUserInvitationRepository : ITenantRepository<UserInvitation>
{
    Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserInvitation>> GetPendingInvitationsAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserInvitation>> GetExpiredInvitationsAsync(CancellationToken cancellationToken = default);
    Task<bool> HasPendingInvitationAsync(string email, Guid companyId, CancellationToken cancellationToken = default);
}

public class UserInvitationRepository : TenantRepository<UserInvitation>, IUserInvitationRepository
{
    public UserInvitationRepository(ErpDbContext context) : base(context)
    {
    }

    public async Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ui => ui.Company)
            .Include(ui => ui.InvitedByUser)
            .FirstOrDefaultAsync(ui => ui.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<UserInvitation>> GetPendingInvitationsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ui => ui.InvitedByUser)
            .Include(ui => ui.InvitedUser)
            .Where(ui => ui.CompanyId == companyId && ui.Status == Domain.Enums.InvitationStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserInvitation>> GetExpiredInvitationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(ui => ui.Status == Domain.Enums.InvitationStatus.Pending && ui.ExpiresAt <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPendingInvitationAsync(string email, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(ui => ui.Email == email && 
                                          ui.CompanyId == companyId && 
                                          ui.Status == Domain.Enums.InvitationStatus.Pending,
                                    cancellationToken);
    }
}
