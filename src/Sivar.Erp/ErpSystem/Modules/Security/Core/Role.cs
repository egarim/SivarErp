namespace Sivar.Erp.ErpSystem.Modules.Security.Core
{
    public class Role : IRole
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> Permissions { get; set; } = new();

        // Interface implementation
        IReadOnlyList<string> IRole.Permissions => Permissions.AsReadOnly();
    }
}
