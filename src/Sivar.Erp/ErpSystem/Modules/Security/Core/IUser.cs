namespace Sivar.Erp.ErpSystem.Modules.Security.Core
{
    /// <summary>
    /// Minimal interface for user representation
    /// </summary>
    public interface IUser
    {
        string Id { get; }
        string Username { get; }
        bool IsActive { get; }
        IReadOnlyList<string> Roles { get; }
        IReadOnlyList<string> DirectPermissions { get; }
    }
}
