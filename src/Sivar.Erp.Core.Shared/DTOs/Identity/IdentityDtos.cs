namespace Sivar.Erp.Core.Shared.DTOs.Identity;

using Sivar.Erp.Core.Domain.Enums;

/// <summary>
/// Data Transfer Object for User information
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string KeycloakUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string PreferredLanguage { get; set; } = "en-US";
    public DateTime? LastActiveAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Data Transfer Object for Company information
/// </summary>
public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Industry { get; set; }
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
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public CompanyRole? UserRole { get; set; }
}

/// <summary>
/// Data Transfer Object for Branch information
/// </summary>
public class BranchDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsHeadquarters { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties for DTOs
    public CompanyDto? Company { get; set; }
}

/// <summary>
/// Data Transfer Object for User Invitation information
/// </summary>
public class UserInvitationDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Email { get; set; } = string.Empty;
    public CompanyRole Role { get; set; }
    public InvitationStatus Status { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DeclinedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Additional display properties
    public string? InviterName { get; set; }
    public string? CompanyName { get; set; }
    
    // Computed properties
    public bool IsValid => Status == InvitationStatus.Pending && ExpiresAt > DateTime.UtcNow;
}

/// <summary>
/// Data Transfer Object for Role information
/// </summary>
public class RoleDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Data Transfer Object for Permission information
/// </summary>
public class PermissionDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

// === Create/Update DTOs ===

/// <summary>
/// DTO for creating a new company
/// </summary>
public class CreateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Industry { get; set; }
}

/// <summary>
/// DTO for updating company information
/// </summary>
public class UpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Industry { get; set; }
}

/// <summary>
/// DTO for inviting a user to a company
/// </summary>
public class InviteUserDto
{
    public Guid CompanyId { get; set; }
    public string Email { get; set; } = string.Empty;
    public CompanyRole Role { get; set; }
}

/// <summary>
/// DTO for user-company relationship
/// </summary>
public class UserCompanyDto
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public CompanyRole Role { get; set; }
    public bool IsOwner { get; set; }
    public bool IsActive { get; set; }
    public DateTime JoinedAt { get; set; }
    public UserDto? User { get; set; }
    public CompanyDto? Company { get; set; }
}
