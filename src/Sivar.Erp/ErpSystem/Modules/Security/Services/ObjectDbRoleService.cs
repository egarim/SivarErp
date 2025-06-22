using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.Services;
using Microsoft.Extensions.Logging;

namespace Sivar.Erp.ErpSystem.Modules.Security.Services
{
    public class ObjectDbRoleService : IRoleService
    {
        private readonly IObjectDb _objectDb;
        private readonly ILogger<ObjectDbRoleService> _logger;

        public ObjectDbRoleService(IObjectDb objectDb, ILogger<ObjectDbRoleService> logger)
        {
            _objectDb = objectDb;
            _logger = logger;
        }

        public async Task<IEnumerable<IRole>> GetAllRolesAsync()
        {
            return await Task.FromResult(_objectDb.Roles.Cast<IRole>());
        }

        public async Task<IRole?> GetRoleByNameAsync(string roleName)
        {
            return await Task.FromResult(_objectDb.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<IRole> CreateRoleAsync(CreateRoleRequest request)
        {
            // Validate role name is unique
            var existingRole = await GetRoleByNameAsync(request.Name);
            if (existingRole != null)
            {
                throw new InvalidOperationException($"Role '{request.Name}' already exists");
            }

            var role = new Role
            {
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description,
                CreatedDate = DateTime.UtcNow,
                IsSystemRole = false,
                Permissions = new List<string>(request.Permissions)
            };

            _objectDb.Roles.Add(role);
            _logger.LogInformation("Created role: {RoleName}", role.Name);

            return await Task.FromResult(role);
        }

        public async Task<IRole> UpdateRoleAsync(string roleName, UpdateRoleRequest request)
        {
            var role = _objectDb.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                throw new InvalidOperationException($"Role '{roleName}' not found");
            }

            if (role.IsSystemRole)
            {
                throw new InvalidOperationException($"Cannot modify system role '{roleName}'");
            }

            if (request.DisplayName != null) role.DisplayName = request.DisplayName;
            if (request.Description != null) role.Description = request.Description;

            if (request.Permissions != null)
            {
                role.Permissions.Clear();
                role.Permissions.AddRange(request.Permissions);
            }

            _logger.LogInformation("Updated role: {RoleName}", role.Name);
            return await Task.FromResult(role);
        }

        public async Task DeleteRoleAsync(string roleName)
        {
            var role = _objectDb.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role != null)
            {
                if (role.IsSystemRole)
                {
                    throw new InvalidOperationException($"Cannot delete system role '{roleName}'");
                }

                // Check if any users have this role
                var usersWithRole = _objectDb.Users.Where(u => u.Roles.Contains(roleName)).ToList();
                if (usersWithRole.Any())
                {
                    throw new InvalidOperationException($"Cannot delete role '{roleName}' - it is assigned to {usersWithRole.Count} user(s)");
                }

                _objectDb.Roles.Remove(role);
                _logger.LogInformation("Deleted role: {RoleName}", role.Name);
            }

            await Task.CompletedTask;
        }

        public async Task<IEnumerable<string>> GetRolePermissionsAsync(string roleName)
        {
            var role = await GetRoleByNameAsync(roleName);
            return role?.Permissions ?? Enumerable.Empty<string>();
        }

        public async Task AddPermissionToRoleAsync(string roleName, string permission)
        {
            var role = _objectDb.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                throw new InvalidOperationException($"Role '{roleName}' not found");
            }

            if (!role.Permissions.Contains(permission))
            {
                role.Permissions.Add(permission);
                _logger.LogInformation("Added permission {Permission} to role {RoleName}", permission, roleName);
            }

            await Task.CompletedTask;
        }

        public async Task RemovePermissionFromRoleAsync(string roleName, string permission)
        {
            var role = _objectDb.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                throw new InvalidOperationException($"Role '{roleName}' not found");
            }

            if (role.Permissions.Remove(permission))
            {
                _logger.LogInformation("Removed permission {Permission} from role {RoleName}", permission, roleName);
            }

            await Task.CompletedTask;
        }

        public async Task<IEnumerable<IUser>> GetUsersInRoleAsync(string roleName)
        {
            var users = _objectDb.Users.Where(u => u.Roles.Contains(roleName)).Cast<IUser>();
            return await Task.FromResult(users);
        }
    }
}
