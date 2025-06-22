namespace Sivar.Erp.ErpSystem.Modules.Security.Core
{
    /// <summary>
    /// Minimal interface for role representation
    /// </summary>
    public interface IRole
    {
        string Name { get; }
        string DisplayName { get; }
        IReadOnlyList<string> Permissions { get; }
        bool IsSystemRole { get; }
    }
}
