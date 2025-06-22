using Sivar.Erp.ErpSystem.Modules.Security.Core;

namespace Sivar.Erp.ErpSystem.Modules.Security
{
    public interface ISecurityModule
    {
        ISecurityContext SecurityContext { get; }
        IAuthorizationService AuthorizationService { get; }
        IUserService UserService { get; }
        IRoleService RoleService { get; }
        IPermissionService PermissionService { get; }
        IAuditService AuditService { get; }

        Task InitializeAsync();
        Task<AuthenticationResult> AuthenticateAsync(string username, string password);
        Task LogoutAsync();
        Task<SecurityResult> CheckBusinessOperationAsync(string operation, object? context = null);
    }
}
