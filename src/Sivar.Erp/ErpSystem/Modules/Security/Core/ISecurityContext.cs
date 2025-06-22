using System.Security.Claims;

namespace Sivar.Erp.ErpSystem.Modules.Security.Core
{
    public interface ISecurityContext
    {
        ClaimsPrincipal? CurrentUser { get; }
        string? UserId { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }
        IReadOnlyList<string> Roles { get; }
        IReadOnlyList<string> Permissions { get; }
        DateTime? LoginTime { get; }
        DateTime? LastActivity { get; }

        event EventHandler<SecurityContextChangedEventArgs>? SecurityContextChanged;

        void UpdateLastActivity();
    }

    public class SecurityContextChangedEventArgs : EventArgs
    {
        public string? PreviousUserId { get; set; }
        public string? NewUserId { get; set; }
        public SecurityContextChangeType ChangeType { get; set; }
    }

    public enum SecurityContextChangeType
    {
        Login,
        Logout,
        RoleChanged,
        PermissionChanged
    }
}
