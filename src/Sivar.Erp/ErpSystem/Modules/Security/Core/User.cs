namespace Sivar.Erp.ErpSystem.Modules.Security.Core
{
    public class User : IUser
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> DirectPermissions { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();

        // Interface implementations
        IReadOnlyList<string> IUser.Roles => Roles.AsReadOnly();
        IReadOnlyList<string> IUser.DirectPermissions => DirectPermissions.AsReadOnly();
    }
}
