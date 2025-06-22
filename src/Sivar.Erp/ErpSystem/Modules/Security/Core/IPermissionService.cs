namespace Sivar.Erp.ErpSystem.Modules.Security.Core
{
    public interface IPermissionService
    {
        Task<IEnumerable<string>> GetAllPermissionsAsync();
        Task<IEnumerable<string>> GetEffectiveUserPermissionsAsync(string userId);
        Task<bool> UserHasPermissionAsync(string userId, string permission);
        Task<bool> RoleHasPermissionAsync(string roleName, string permission);
        Task<IEnumerable<string>> GetBusinessOperationsAsync();
        Task<Dictionary<string, string[]>> GetRolePermissionMappingsAsync();
        Task<IEnumerable<string>> GetRolePermissionsAsync(string roleName);
    }

    public interface IAuditService
    {
        Task LogAsync(string action, string result, string details, string? userId = null);
        Task LogSecurityEventAsync(SecurityEvent securityEvent);
        Task<IEnumerable<SecurityEvent>> GetSecurityEventsAsync(DateTime fromDate, DateTime toDate, string? userId = null);
    }

    public class SecurityEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Action { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
}
