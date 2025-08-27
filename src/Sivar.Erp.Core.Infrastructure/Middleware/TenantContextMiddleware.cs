using Microsoft.AspNetCore.Http;
using Sivar.Erp.Core.Infrastructure.Services.Identity;
using Sivar.Erp.Core.Infrastructure.Logging;
using System.Security.Claims;

namespace Sivar.Erp.Core.Infrastructure.Middleware;

/// <summary>
/// Middleware to automatically set tenant context from JWT claims
/// </summary>
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IErpLoggingService _logger;

    public TenantContextMiddleware(RequestDelegate next, IErpLoggingService logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContextService tenantContext)
    {
        try
        {
            // Extract tenant information from JWT claims
            var user = context.User;
            
            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                           user.FindFirst("sub")?.Value;
                
                var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? 
                              user.FindFirst("email")?.Value;
                
                var companyIdClaim = user.FindFirst("company_id")?.Value;
                var branchIdClaim = user.FindFirst("branch_id")?.Value;

                if (!string.IsNullOrEmpty(userId) && 
                    !string.IsNullOrEmpty(userEmail) && 
                    Guid.TryParse(companyIdClaim, out var companyId))
                {
                    Guid? branchId = null;
                    if (Guid.TryParse(branchIdClaim, out var parsedBranchId))
                    {
                        branchId = parsedBranchId;
                    }

                    // Set tenant context
                    tenantContext.SetContext(companyId, userId, userEmail, branchId);

                    _logger.LogSecurityEvent(
                        "TenantContextSet", 
                        userId, 
                        $"Company: {companyId}, Branch: {branchId}", 
                        true);
                }
                else
                {
                    _logger.LogSecurityEvent(
                        "TenantContextMissing", 
                        userId ?? "unknown", 
                        "Missing company_id or user info in JWT", 
                        false);
                }
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogBusinessEntityOperation(
                "TenantContextError",
                "Middleware",
                "TenantContext", 
                "System",
                new { Error = ex.Message, Path = context.Request.Path });

            // Continue processing even if tenant context setup fails
            await _next(context);
        }
        finally
        {
            // Clear context after request
            tenantContext.ClearContext();
        }
    }
}
