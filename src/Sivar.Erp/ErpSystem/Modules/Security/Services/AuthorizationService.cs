using Microsoft.Extensions.Logging;
using Sivar.Erp.ErpSystem.Modules.Security.Core;

namespace Sivar.Erp.ErpSystem.Modules.Security.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ISecurityContext _securityContext;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            ISecurityContext securityContext,
            IPermissionService permissionService,
            ILogger<AuthorizationService> logger)
        {
            _securityContext = securityContext;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(string permission)
        {
            var result = await CheckPermissionAsync(permission);
            return result.IsAuthorized;
        }

        public async Task<bool> HasAnyPermissionAsync(params string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (await HasPermissionAsync(permission))
                    return true;
            }
            return false;
        }

        public async Task<bool> HasAllPermissionsAsync(params string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (!await HasPermissionAsync(permission))
                    return false;
            }
            return true;
        }

        public async Task<bool> IsInRoleAsync(string role)
        {
            return await Task.FromResult(_securityContext.Roles.Contains(role));
        }

        public async Task<SecurityResult> CheckPermissionAsync(string permission)
        {
            if (!_securityContext.IsAuthenticated)
            {
                _logger.LogWarning("Permission check failed: User not authenticated. Permission: {Permission}", permission);
                return SecurityResult.Denied("User not authenticated", permission);
            }

            if (_securityContext.UserId == null)
            {
                return SecurityResult.Denied("Invalid user context", permission);
            }

            // Check if user has permission (either directly or through roles)
            var hasPermission = await _permissionService.UserHasPermissionAsync(_securityContext.UserId, permission);

            if (hasPermission)
            {
                _logger.LogDebug("Permission granted: {Permission} for user {User}", permission, _securityContext.UserName);
                return SecurityResult.Success();
            }

            _logger.LogWarning("Permission denied: {Permission} for user {User}. User roles: {Roles}",
                permission, _securityContext.UserName, string.Join(", ", _securityContext.Roles));

            return SecurityResult.Denied($"Insufficient permissions", permission);
        }

        public async Task<SecurityResult> CheckBusinessOperationAsync(string operation, object? context = null)
        {
            var result = await CheckPermissionAsync(operation);

            if (!result.IsAuthorized)
            {
                _logger.LogWarning("Business operation denied: {Operation} for user {User}",
                    operation, _securityContext.UserName);
            }
            else
            {
                _logger.LogInformation("Business operation authorized: {Operation} for user {User}",
                    operation, _securityContext.UserName);
            }

            return result;
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
        {
            return await _permissionService.GetEffectiveUserPermissionsAsync(userId);
        }
        public async Task<IEnumerable<string>> GetRolePermissionsAsync(string role)
        {
            return await _permissionService.GetRolePermissionsAsync(role);
        }
    }
}
