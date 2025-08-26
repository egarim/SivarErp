using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Domain.Entities.Identity;
using Sivar.Erp.Core.Domain.Enums;
using Sivar.Erp.Core.Domain.Interfaces;
using Sivar.Erp.Core.Infrastructure.Repositories.Identity;
using Sivar.Erp.Core.Shared.DTOs.Identity;
using Sivar.Erp.Core.Shared.Responses;
using System.Security.Cryptography;
using System.Text;

namespace Sivar.Erp.Core.Application.Services.Identity;

/// <summary>
/// Company management service with business logic
/// </summary>
public interface ICompanyService
{
    Task<ApiResponse<CompanyDto>> CreateAsync(CreateCompanyDto createDto, string keycloakUserId);
    Task<ApiResponse<CompanyDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<CompanyDto>>> GetUserCompaniesAsync(string keycloakUserId);
    Task<ApiResponse<CompanyDto>> UpdateAsync(Guid id, UpdateCompanyDto updateDto, string keycloakUserId);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string keycloakUserId);
}

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        ICompanyRepository companyRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<CompanyService> logger)
    {
        _companyRepository = companyRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<CompanyDto>> CreateAsync(CreateCompanyDto createDto, string keycloakUserId)
    {
        try
        {
            // Validate company name uniqueness
            if (await _companyRepository.ExistsByNameAsync(createDto.Name))
            {
                return ApiResponse<CompanyDto>.Failure("Company name already exists.");
            }

            // Get or create user
            var user = await _userRepository.GetByKeycloakIdAsync(keycloakUserId);
            if (user == null)
            {
                return ApiResponse<CompanyDto>.Failure("User not found.");
            }

            var company = new Company
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Industry = createDto.Industry,
                CreatedBy = user.KeycloakUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _companyRepository.AddAsync(company);

            // Add user as owner
            var userCompany = new UserCompany
            {
                UserId = user.Id,
                CompanyId = company.Id,
                Role = CompanyRole.Owner,
                IsOwner = true,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            };

            await _unitOfWork.Context.Set<UserCompany>().AddAsync(userCompany);
            await _unitOfWork.SaveChangesAsync();

            var companyDto = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description,
                Industry = company.Industry,
                CreatedAt = company.CreatedAt,
                UserRole = CompanyRole.Owner
            };

            _logger.LogInformation("Company '{CompanyName}' created by user {UserId}", company.Name, user.Id);
            return ApiResponse<CompanyDto>.Success(companyDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company");
            return ApiResponse<CompanyDto>.Failure("An error occurred while creating the company.");
        }
    }

    public async Task<ApiResponse<CompanyDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                return ApiResponse<CompanyDto>.Failure("Company not found.");
            }

            var companyDto = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description,
                Industry = company.Industry,
                CreatedAt = company.CreatedAt
            };

            return ApiResponse<CompanyDto>.Success(companyDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company {CompanyId}", id);
            return ApiResponse<CompanyDto>.Failure("An error occurred while retrieving the company.");
        }
    }

    public async Task<ApiResponse<IEnumerable<CompanyDto>>> GetUserCompaniesAsync(string keycloakUserId)
    {
        try
        {
            var companies = await _companyRepository.GetUserCompaniesAsync(keycloakUserId);
            var companiesDto = companies.Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Industry = c.Industry,
                CreatedAt = c.CreatedAt
            });

            return ApiResponse<IEnumerable<CompanyDto>>.Success(companiesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user companies for {KeycloakUserId}", keycloakUserId);
            return ApiResponse<IEnumerable<CompanyDto>>.Failure("An error occurred while retrieving user companies.");
        }
    }

    public async Task<ApiResponse<CompanyDto>> UpdateAsync(Guid id, UpdateCompanyDto updateDto, string keycloakUserId)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                return ApiResponse<CompanyDto>.Failure("Company not found.");
            }

            // Check if user is owner
            if (!await _companyRepository.IsUserOwnerAsync(id, keycloakUserId))
            {
                return ApiResponse<CompanyDto>.Failure("Access denied. Only company owners can update company details.");
            }

            // Check name uniqueness if changed
            if (updateDto.Name != company.Name && await _companyRepository.ExistsByNameAsync(updateDto.Name))
            {
                return ApiResponse<CompanyDto>.Failure("Company name already exists.");
            }

            company.Name = updateDto.Name;
            company.Description = updateDto.Description;
            company.Industry = updateDto.Industry;
            company.UpdatedAt = DateTime.UtcNow;

            await _companyRepository.UpdateAsync(company);
            await _unitOfWork.SaveChangesAsync();

            var companyDto = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description,
                Industry = company.Industry,
                CreatedAt = company.CreatedAt,
                UserRole = CompanyRole.Owner
            };

            _logger.LogInformation("Company '{CompanyName}' updated by user {KeycloakUserId}", company.Name, keycloakUserId);
            return ApiResponse<CompanyDto>.Success(companyDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company {CompanyId}", id);
            return ApiResponse<CompanyDto>.Failure("An error occurred while updating the company.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string keycloakUserId)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                return ApiResponse<bool>.Failure("Company not found.");
            }

            // Check if user is owner
            if (!await _companyRepository.IsUserOwnerAsync(id, keycloakUserId))
            {
                return ApiResponse<bool>.Failure("Access denied. Only company owners can delete companies.");
            }

            await _companyRepository.DeleteAsync(company);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Company '{CompanyName}' deleted by user {KeycloakUserId}", company.Name, keycloakUserId);
            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company {CompanyId}", id);
            return ApiResponse<bool>.Failure("An error occurred while deleting the company.");
        }
    }
}

/// <summary>
/// User invitation service with token management
/// </summary>
public interface IUserInvitationService
{
    Task<ApiResponse<UserInvitationDto>> InviteUserAsync(InviteUserDto inviteDto, string inviterKeycloakUserId);
    Task<ApiResponse<bool>> AcceptInvitationAsync(string token, string keycloakUserId);
    Task<ApiResponse<bool>> DeclineInvitationAsync(string token, string keycloakUserId);
    Task<ApiResponse<IEnumerable<UserInvitationDto>>> GetCompanyInvitationsAsync(Guid companyId);
    Task<ApiResponse<bool>> CancelInvitationAsync(Guid invitationId, string keycloakUserId);
}

public class UserInvitationService : IUserInvitationService
{
    private readonly IUserInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserInvitationService> _logger;

    public UserInvitationService(
        IUserInvitationRepository invitationRepository,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserInvitationService> logger)
    {
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<UserInvitationDto>> InviteUserAsync(InviteUserDto inviteDto, string inviterKeycloakUserId)
    {
        try
        {
            // Set company context for tenant operations
            _unitOfWork.SetCompanyContext(inviteDto.CompanyId);

            // Validate inviter permissions
            if (!await _companyRepository.IsUserOwnerAsync(inviteDto.CompanyId, inviterKeycloakUserId))
            {
                return ApiResponse<UserInvitationDto>.Failure("Access denied. Only company owners can invite users.");
            }

            // Check for existing pending invitation
            if (await _invitationRepository.HasPendingInvitationAsync(inviteDto.Email, inviteDto.CompanyId))
            {
                return ApiResponse<UserInvitationDto>.Failure("User already has a pending invitation.");
            }

            var inviter = await _userRepository.GetByKeycloakIdAsync(inviterKeycloakUserId);
            if (inviter == null)
            {
                return ApiResponse<UserInvitationDto>.Failure("Inviter not found.");
            }

            var invitation = new UserInvitation
            {
                CompanyId = inviteDto.CompanyId,
                Email = inviteDto.Email,
                Role = inviteDto.Role,
                InvitedByUserId = inviter.Id,
                Token = GenerateInvitationToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days expiration
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _invitationRepository.AddAsync(invitation);
            await _unitOfWork.SaveChangesAsync();

            var invitationDto = new UserInvitationDto
            {
                Id = invitation.Id,
                CompanyId = invitation.CompanyId,
                Email = invitation.Email,
                Role = invitation.Role,
                Token = invitation.Token,
                ExpiresAt = invitation.ExpiresAt,
                Status = invitation.Status,
                CreatedAt = invitation.CreatedAt
            };

            _logger.LogInformation("User invitation sent to {Email} for company {CompanyId}", inviteDto.Email, inviteDto.CompanyId);
            return ApiResponse<UserInvitationDto>.Success(invitationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting user {Email} to company {CompanyId}", inviteDto.Email, inviteDto.CompanyId);
            return ApiResponse<UserInvitationDto>.Failure("An error occurred while sending the invitation.");
        }
    }

    public async Task<ApiResponse<bool>> AcceptInvitationAsync(string token, string keycloakUserId)
    {
        try
        {
            var invitation = await _invitationRepository.GetByTokenAsync(token);
            if (invitation == null)
            {
                return ApiResponse<bool>.Failure("Invalid invitation token.");
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return ApiResponse<bool>.Failure("Invitation is no longer valid.");
            }

            if (invitation.ExpiresAt <= DateTime.UtcNow)
            {
                return ApiResponse<bool>.Failure("Invitation has expired.");
            }

            var user = await _userRepository.GetByKeycloakIdAsync(keycloakUserId);
            if (user == null)
            {
                return ApiResponse<bool>.Failure("User not found.");
            }

            // Update invitation status
            invitation.Status = InvitationStatus.Accepted;
            invitation.InvitedUserId = user.Id;
            invitation.AcceptedAt = DateTime.UtcNow;

            // Create user-company relationship
            var userCompany = new UserCompany
            {
                UserId = user.Id,
                CompanyId = invitation.CompanyId,
                Role = invitation.Role,
                IsOwner = false,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            };

            await _unitOfWork.Context.Set<UserCompany>().AddAsync(userCompany);
            await _invitationRepository.UpdateAsync(invitation);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} accepted invitation to company {CompanyId}", user.Id, invitation.CompanyId);
            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation with token {Token}", token);
            return ApiResponse<bool>.Failure("An error occurred while accepting the invitation.");
        }
    }

    public async Task<ApiResponse<bool>> DeclineInvitationAsync(string token, string keycloakUserId)
    {
        try
        {
            var invitation = await _invitationRepository.GetByTokenAsync(token);
            if (invitation == null)
            {
                return ApiResponse<bool>.Failure("Invalid invitation token.");
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return ApiResponse<bool>.Failure("Invitation is no longer valid.");
            }

            var user = await _userRepository.GetByKeycloakIdAsync(keycloakUserId);
            if (user == null)
            {
                return ApiResponse<bool>.Failure("User not found.");
            }

            invitation.Status = InvitationStatus.Declined;
            invitation.InvitedUserId = user.Id;
            invitation.DeclinedAt = DateTime.UtcNow;

            await _invitationRepository.UpdateAsync(invitation);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} declined invitation to company {CompanyId}", user.Id, invitation.CompanyId);
            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error declining invitation with token {Token}", token);
            return ApiResponse<bool>.Failure("An error occurred while declining the invitation.");
        }
    }

    public async Task<ApiResponse<IEnumerable<UserInvitationDto>>> GetCompanyInvitationsAsync(Guid companyId)
    {
        try
        {
            _unitOfWork.SetCompanyContext(companyId);
            var invitations = await _invitationRepository.GetPendingInvitationsAsync(companyId);

            var invitationsDto = invitations.Select(i => new UserInvitationDto
            {
                Id = i.Id,
                CompanyId = i.CompanyId,
                Email = i.Email,
                Role = i.Role,
                ExpiresAt = i.ExpiresAt,
                Status = i.Status,
                CreatedAt = i.CreatedAt,
                InviterName = i.InvitedByUser?.FirstName + " " + i.InvitedByUser?.LastName
            });

            return ApiResponse<IEnumerable<UserInvitationDto>>.Success(invitationsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invitations for company {CompanyId}", companyId);
            return ApiResponse<IEnumerable<UserInvitationDto>>.Failure("An error occurred while retrieving invitations.");
        }
    }

    public async Task<ApiResponse<bool>> CancelInvitationAsync(Guid invitationId, string keycloakUserId)
    {
        try
        {
            var invitation = await _invitationRepository.GetByIdAsync(invitationId);
            if (invitation == null)
            {
                return ApiResponse<bool>.Failure("Invitation not found.");
            }

            // Check if user can cancel this invitation
            if (!await _companyRepository.IsUserOwnerAsync(invitation.CompanyId, keycloakUserId))
            {
                return ApiResponse<bool>.Failure("Access denied. Only company owners can cancel invitations.");
            }

            invitation.Status = InvitationStatus.Cancelled;
            invitation.UpdatedAt = DateTime.UtcNow;

            await _invitationRepository.UpdateAsync(invitation);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Invitation {InvitationId} cancelled by user {KeycloakUserId}", invitationId, keycloakUserId);
            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invitation {InvitationId}", invitationId);
            return ApiResponse<bool>.Failure("An error occurred while cancelling the invitation.");
        }
    }

    private static string GenerateInvitationToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-").Replace("=", "");
    }
}
