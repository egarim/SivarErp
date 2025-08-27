using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sivar.Erp.Core.Infrastructure.Repositories.Identity;

namespace Sivar.Erp.Core.Infrastructure.Services.Identity;

/// <summary>
/// Interface for tenant context service
/// </summary>
public interface ITenantContextService
{
    /// <summary>
    /// Gets the current company/tenant ID
    /// </summary>
    Guid? CurrentCompanyId { get; }

    /// <summary>
    /// Gets the current user's Keycloak ID
    /// </summary>
    string? CurrentUserId { get; }

    /// <summary>
    /// Gets the current user's email
    /// </summary>
    string? CurrentUserEmail { get; }

    /// <summary>
    /// Gets the current branch ID if available
    /// </summary>
    Guid? CurrentBranchId { get; }

    /// <summary>
    /// Sets the current tenant context
    /// </summary>
    void SetContext(Guid companyId, string userId, string userEmail, Guid? branchId = null);

    /// <summary>
    /// Clears the current tenant context
    /// </summary>
    void ClearContext();

    /// <summary>
    /// Checks if the current user has access to the specified company
    /// </summary>
    Task<bool> HasAccessToCompanyAsync(Guid companyId);

    /// <summary>
    /// Gets all companies the current user has access to
    /// </summary>
    Task<IEnumerable<Guid>> GetUserCompanyIdsAsync();

    /// <summary>
    /// Validates tenant context and throws exception if invalid
    /// </summary>
    void ValidateContext();
}

/// <summary>
/// Implementation of tenant context service
/// </summary>
public class TenantContextService : ITenantContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;

    public TenantContextService(
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public Guid? CurrentCompanyId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue("CompanyId", out var companyId) == true)
            {
                return companyId as Guid?;
            }

            // Try to get from claims
            var companyIdClaim = context?.User?.FindFirst("company_id")?.Value;
            if (Guid.TryParse(companyIdClaim, out var parsedCompanyId))
            {
                return parsedCompanyId;
            }

            return null;
        }
    }

    public string? CurrentUserId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue("UserId", out var userId) == true)
            {
                return userId as string;
            }

            // Try to get from claims (Keycloak sub claim)
            return context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   context?.User?.FindFirst("sub")?.Value;
        }
    }

    public string? CurrentUserEmail
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue("UserEmail", out var email) == true)
            {
                return email as string;
            }

            // Try to get from claims
            return context?.User?.FindFirst(ClaimTypes.Email)?.Value ??
                   context?.User?.FindFirst("email")?.Value;
        }
    }

    public Guid? CurrentBranchId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue("BranchId", out var branchId) == true)
            {
                return branchId as Guid?;
            }

            // Try to get from claims
            var branchIdClaim = context?.User?.FindFirst("branch_id")?.Value;
            if (Guid.TryParse(branchIdClaim, out var parsedBranchId))
            {
                return parsedBranchId;
            }

            return null;
        }
    }

    public void SetContext(Guid companyId, string userId, string userEmail, Guid? branchId = null)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            context.Items["CompanyId"] = companyId;
            context.Items["UserId"] = userId;
            context.Items["UserEmail"] = userEmail;
            
            if (branchId.HasValue)
            {
                context.Items["BranchId"] = branchId.Value;
            }
        }
    }

    public void ClearContext()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            context.Items.Remove("CompanyId");
            context.Items.Remove("UserId");
            context.Items.Remove("UserEmail");
            context.Items.Remove("BranchId");
        }
    }

    public async Task<bool> HasAccessToCompanyAsync(Guid companyId)
    {
        if (CurrentUserId == null) return false;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var companyRepository = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
            
            return await companyRepository.IsUserOwnerAsync(companyId, CurrentUserId);
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<Guid>> GetUserCompanyIdsAsync()
    {
        if (CurrentUserId == null) return Enumerable.Empty<Guid>();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var companyRepository = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
            
            var companies = await companyRepository.GetUserCompaniesAsync(CurrentUserId);
            return companies.Select(c => c.Id);
        }
        catch
        {
            return Enumerable.Empty<Guid>();
        }
    }

    public void ValidateContext()
    {
        if (CurrentCompanyId == null)
        {
            throw new UnauthorizedAccessException("No company context available");
        }

        if (CurrentUserId == null)
        {
            throw new UnauthorizedAccessException("No user context available");
        }
    }
}
