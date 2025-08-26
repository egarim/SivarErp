using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Application.Services.Identity;
using Sivar.Erp.Core.Shared.Responses;
using System.Security.Claims;

namespace Sivar.Erp.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationsController : ControllerBase
{
    private readonly IUserInvitationService _invitationService;

    public InvitationsController(IUserInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    /// <summary>
    /// Accept an invitation by token
    /// </summary>
    [HttpPost("accept/{token}")]
    public async Task<IActionResult> AcceptInvitation(string token)
    {
        var keycloakUserId = GetKeycloakUserId();
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return Unauthorized(ApiResponse<bool>.Failure("User not authenticated"));
        }

        var result = await _invitationService.AcceptInvitationAsync(token, keycloakUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Decline an invitation by token
    /// </summary>
    [HttpPost("decline/{token}")]
    public async Task<IActionResult> DeclineInvitation(string token)
    {
        var keycloakUserId = GetKeycloakUserId();
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return Unauthorized(ApiResponse<bool>.Failure("User not authenticated"));
        }

        var result = await _invitationService.DeclineInvitationAsync(token, keycloakUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get invitation details by token (for invitation preview)
    /// </summary>
    [HttpGet("preview/{token}")]
    public async Task<IActionResult> GetInvitationPreview(string token)
    {
        // This would typically show invitation details without requiring authentication
        // Implementation would call a service method to get invitation details
        return Ok(ApiResponse<object>.Success(new { Message = "Invitation preview endpoint - to be implemented" }));
    }

    private string? GetKeycloakUserId()
    {
        // For development without Keycloak, use a test user ID
        // In production, this would extract from JWT token
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "test-user-id";
    }
}
