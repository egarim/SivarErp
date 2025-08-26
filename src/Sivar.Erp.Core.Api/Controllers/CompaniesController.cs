using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Application.Services.Identity;
using Sivar.Erp.Core.Shared.DTOs.Identity;
using Sivar.Erp.Core.Shared.Responses;
using System.Security.Claims;

namespace Sivar.Erp.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IUserInvitationService _invitationService;

    public CompaniesController(
        ICompanyService companyService,
        IUserInvitationService invitationService)
    {
        _companyService = companyService;
        _invitationService = invitationService;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(ApiResponse<string>.Success("Companies API is healthy"));
    }

    /// <summary>
    /// Create a new company
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto createDto)
    {
        var keycloakUserId = GetKeycloakUserId();
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return Unauthorized(ApiResponse<CompanyDto>.Failure("User not authenticated"));
        }

        var result = await _companyService.CreateAsync(createDto, keycloakUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get company by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(Guid id)
    {
        var result = await _companyService.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get current user's companies
    /// </summary>
    [HttpGet("my-companies")]
    public async Task<IActionResult> GetMyCompanies()
    {
        var keycloakUserId = GetKeycloakUserId();
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return Unauthorized(ApiResponse<IEnumerable<CompanyDto>>.Failure("User not authenticated"));
        }

        var result = await _companyService.GetUserCompaniesAsync(keycloakUserId);
        return Ok(result);
    }

    /// <summary>
    /// Update company details
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyDto updateDto)
    {
        var keycloakUserId = GetKeycloakUserId();
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return Unauthorized(ApiResponse<CompanyDto>.Failure("User not authenticated"));
        }

        var result = await _companyService.UpdateAsync(id, updateDto, keycloakUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete company
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        var keycloakUserId = GetKeycloakUserId();
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return Unauthorized(ApiResponse<bool>.Failure("User not authenticated"));
        }

        var result = await _companyService.DeleteAsync(id, keycloakUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Invite user to company
    /// </summary>
    [HttpPost("{id}/invite")]
    public async Task<IActionResult> InviteUser(Guid id, [FromBody] InviteUserDto inviteDto)
    {
        var keycloakUserId = GetKeycloakUserId();
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return Unauthorized(ApiResponse<UserInvitationDto>.Failure("User not authenticated"));
        }

        inviteDto.CompanyId = id; // Ensure company ID matches route
        var result = await _invitationService.InviteUserAsync(inviteDto, keycloakUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get company invitations
    /// </summary>
    [HttpGet("{id}/invitations")]
    public async Task<IActionResult> GetCompanyInvitations(Guid id)
    {
        var result = await _invitationService.GetCompanyInvitationsAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Cancel invitation
    /// </summary>
    [HttpDelete("invitations/{invitationId}")]
    public async Task<IActionResult> CancelInvitation(Guid invitationId)
    {
        var keycloakUserId = GetKeycloakUserId();
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return Unauthorized(ApiResponse<bool>.Failure("User not authenticated"));
        }

        var result = await _invitationService.CancelInvitationAsync(invitationId, keycloakUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    private string? GetKeycloakUserId()
    {
        // For development without Keycloak, use a test user ID
        // In production, this would extract from JWT token
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "test-user-id";
    }
}
