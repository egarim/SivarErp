namespace Sivar.Erp.ErpSystem.Modules.Security.Core
{
    public interface IRoleService
    {
        Task<IEnumerable<IRole>> GetAllRolesAsync();
        Task<IRole?> GetRoleByNameAsync(string roleName);
        Task<IRole> CreateRoleAsync(CreateRoleRequest request);
        Task<IRole> UpdateRoleAsync(string roleName, UpdateRoleRequest request);
        Task DeleteRoleAsync(string roleName);
        Task<IEnumerable<string>> GetRolePermissionsAsync(string roleName);
        Task AddPermissionToRoleAsync(string roleName, string permission);
        Task RemovePermissionFromRoleAsync(string roleName, string permission);
        Task<IEnumerable<IUser>> GetUsersInRoleAsync(string roleName);
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }

    public class UpdateRoleRequest
    {
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public List<string>? Permissions { get; set; }
    }
}
