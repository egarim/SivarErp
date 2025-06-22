namespace Sivar.Erp.ErpSystem.Modules.Security.Core
{
    public interface IAuthorizationService
    {
        Task<bool> HasPermissionAsync(string permission);
        Task<bool> HasAnyPermissionAsync(params string[] permissions);
        Task<bool> HasAllPermissionsAsync(params string[] permissions);
        Task<bool> IsInRoleAsync(string role);
        Task<SecurityResult> CheckPermissionAsync(string permission);
        Task<SecurityResult> CheckBusinessOperationAsync(string operation, object? context = null);
        Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);
        Task<IEnumerable<string>> GetRolePermissionsAsync(string role);
    }

    public class SecurityResult
    {
        public bool IsAuthorized { get; set; }
        public string? DenialReason { get; set; }
        public IReadOnlyList<string> RequiredPermissions { get; set; } = new List<string>();
        public Dictionary<string, object> Context { get; set; } = new();

        public static SecurityResult Success() => new() { IsAuthorized = true };
        public static SecurityResult Denied(string reason, params string[] requiredPermissions) =>
            new() { IsAuthorized = false, DenialReason = reason, RequiredPermissions = requiredPermissions };
    }

    public class SecurityResult<T> : SecurityResult
    {
        public T? Data { get; set; }

        public static SecurityResult<T> Success(T data) => new() { IsAuthorized = true, Data = data };
        public static new SecurityResult<T> Denied(string reason, params string[] requiredPermissions) =>
            new() { IsAuthorized = false, DenialReason = reason, RequiredPermissions = requiredPermissions };
    }
}
