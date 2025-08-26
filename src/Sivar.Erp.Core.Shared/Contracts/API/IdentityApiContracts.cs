using Sivar.Erp.Core.Shared.DTOs.Identity;
using Sivar.Erp.Core.Shared.Models;

namespace Sivar.Erp.Core.Shared.Contracts.API;

/// <summary>
/// Company management API contract
/// </summary>
public interface ICompanyApi
{
    /// <summary>
    /// Gets all companies for the current user
    /// </summary>
    Task<ApiResponse<List<CompanyDto>>> GetUserCompaniesAsync();
    
    /// <summary>
    /// Gets a specific company by ID
    /// </summary>
    Task<ApiResponse<CompanyDto>> GetCompanyAsync(Guid companyId);
    
    /// <summary>
    /// Creates a new company
    /// </summary>
    Task<ApiResponse<CompanyDto>> CreateCompanyAsync(CreateCompanyRequest request);
    
    /// <summary>
    /// Updates an existing company
    /// </summary>
    Task<ApiResponse<CompanyDto>> UpdateCompanyAsync(Guid companyId, UpdateCompanyRequest request);
    
    /// <summary>
    /// Deactivates a company
    /// </summary>
    Task<ApiResponse> DeactivateCompanyAsync(Guid companyId);
    
    /// <summary>
    /// Gets company users with their roles
    /// </summary>
    Task<ApiResponse<PaginatedResult<UserCompanyDto>>> GetCompanyUsersAsync(Guid companyId, int pageNumber = 1, int pageSize = 10);
    
    /// <summary>
    /// Updates a user's role in the company
    /// </summary>
    Task<ApiResponse> UpdateUserRoleAsync(Guid companyId, Guid userId, string newRole);
    
    /// <summary>
    /// Removes a user from the company
    /// </summary>
    Task<ApiResponse> RemoveUserFromCompanyAsync(Guid companyId, Guid userId);
}

/// <summary>
/// Branch management API contract
/// </summary>
public interface IBranchApi
{
    /// <summary>
    /// Gets all branches for a company
    /// </summary>
    Task<ApiResponse<List<BranchDto>>> GetCompanyBranchesAsync(Guid companyId);
    
    /// <summary>
    /// Gets a specific branch by ID
    /// </summary>
    Task<ApiResponse<BranchDto>> GetBranchAsync(Guid branchId);
    
    /// <summary>
    /// Creates a new branch
    /// </summary>
    Task<ApiResponse<BranchDto>> CreateBranchAsync(CreateBranchRequest request);
    
    /// <summary>
    /// Updates an existing branch
    /// </summary>
    Task<ApiResponse<BranchDto>> UpdateBranchAsync(Guid branchId, UpdateBranchRequest request);
    
    /// <summary>
    /// Deactivates a branch
    /// </summary>
    Task<ApiResponse> DeactivateBranchAsync(Guid branchId);
}

/// <summary>
/// User invitation API contract
/// </summary>
public interface IUserInvitationApi
{
    /// <summary>
    /// Sends an invitation to a user
    /// </summary>
    Task<ApiResponse<UserInvitationDto>> SendInvitationAsync(InvitationRequest request);
    
    /// <summary>
    /// Gets pending invitations for a company
    /// </summary>
    Task<ApiResponse<PaginatedResult<UserInvitationDto>>> GetCompanyInvitationsAsync(Guid companyId, int pageNumber = 1, int pageSize = 10);
    
    /// <summary>
    /// Accepts an invitation
    /// </summary>
    Task<ApiResponse> AcceptInvitationAsync(string token);
    
    /// <summary>
    /// Declines an invitation
    /// </summary>
    Task<ApiResponse> DeclineInvitationAsync(string token);
    
    /// <summary>
    /// Cancels an invitation
    /// </summary>
    Task<ApiResponse> CancelInvitationAsync(Guid invitationId);
    
    /// <summary>
    /// Resends an invitation
    /// </summary>
    Task<ApiResponse> ResendInvitationAsync(Guid invitationId);
    
    /// <summary>
    /// Gets invitation details by token
    /// </summary>
    Task<ApiResponse<UserInvitationDto>> GetInvitationByTokenAsync(string token);
}

// Request models for the APIs
public class CreateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string Currency { get; set; } = "USD";
    public string DefaultLanguage { get; set; } = "en-US";
    public string TimeZone { get; set; } = "UTC";
}

public class UpdateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? LogoUrl { get; set; }
    public string Currency { get; set; } = "USD";
    public string DefaultLanguage { get; set; } = "en-US";
    public string TimeZone { get; set; } = "UTC";
}

public class CreateBranchRequest
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public bool IsHeadquarters { get; set; }
    public Guid? ManagerUserId { get; set; }
}

public class UpdateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public bool IsHeadquarters { get; set; }
    public Guid? ManagerUserId { get; set; }
}
