using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.Services;
using Microsoft.Extensions.Logging;

namespace Sivar.Erp.ErpSystem.Modules.Security.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IObjectDb _objectDb;
        private readonly ILogger<PermissionService> _logger;
        private readonly Dictionary<string, string[]> _defaultRolePermissions;

        public PermissionService(IObjectDb objectDb, ILogger<PermissionService> logger)
        {
            _objectDb = objectDb;
            _logger = logger;
            _defaultRolePermissions = GetDefaultRolePermissions();
        }

        public async Task<IEnumerable<string>> GetAllPermissionsAsync()
        {
            return await Task.FromResult(BusinessOperations.GetAllOperations());
        }

        public async Task<IEnumerable<string>> GetEffectiveUserPermissionsAsync(string userId)
        {
            var user = _objectDb.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return Enumerable.Empty<string>();

            var permissions = new HashSet<string>(user.DirectPermissions);

            // Add permissions from roles
            foreach (var roleName in user.Roles)
            {
                var rolePermissions = await GetRolePermissionsAsync(roleName);
                foreach (var permission in rolePermissions)
                {
                    permissions.Add(permission);
                }
            }

            return permissions;
        }

        public async Task<bool> UserHasPermissionAsync(string userId, string permission)
        {
            var userPermissions = await GetEffectiveUserPermissionsAsync(userId);
            return userPermissions.Contains(permission);
        }

        public async Task<bool> RoleHasPermissionAsync(string roleName, string permission)
        {
            var rolePermissions = await GetRolePermissionsAsync(roleName);
            return rolePermissions.Contains(permission);
        }

        public async Task<IEnumerable<string>> GetBusinessOperationsAsync()
        {
            return await Task.FromResult(BusinessOperations.GetAllOperations());
        }

        public async Task<Dictionary<string, string[]>> GetRolePermissionMappingsAsync()
        {
            return await Task.FromResult(_defaultRolePermissions);
        }

        public async Task<IEnumerable<string>> GetRolePermissionsAsync(string roleName)
        {
            // First check if role exists in database
            var role = _objectDb.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role != null)
            {
                return await Task.FromResult(role.Permissions);
            }

            // Fall back to default role permissions
            if (_defaultRolePermissions.TryGetValue(roleName, out var permissions))
            {
                return await Task.FromResult(permissions);
            }

            return Enumerable.Empty<string>();
        }

        private Dictionary<string, string[]> GetDefaultRolePermissions()
        {
            return new Dictionary<string, string[]>
            {
                [Roles.SYSTEM_ADMINISTRATOR] = BusinessOperations.GetAllOperations().ToArray(),

                [Roles.ADMINISTRATOR] = new[]
                {
                    BusinessOperations.ACCOUNTING_POST_TRANSACTION,
                    BusinessOperations.ACCOUNTING_REVERSE_TRANSACTION,
                    BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES,
                    BusinessOperations.ACCOUNTING_CREATE_FISCAL_PERIOD,
                    BusinessOperations.ACCOUNTING_CLOSE_FISCAL_PERIOD,
                    BusinessOperations.ACCOUNTING_GENERATE_REPORTS,
                    BusinessOperations.ACCOUNTING_MANAGE_CHART_OF_ACCOUNTS,
                    BusinessOperations.DOCUMENTS_CREATE_SALES_INVOICE,
                    BusinessOperations.DOCUMENTS_CREATE_PURCHASE_INVOICE,
                    BusinessOperations.DOCUMENTS_APPROVE_DOCUMENT,
                    BusinessOperations.DOCUMENTS_CANCEL_DOCUMENT,
                    BusinessOperations.DOCUMENTS_VIEW_SENSITIVE_DATA,
                    BusinessOperations.TAX_CALCULATE_TAXES,
                    BusinessOperations.TAX_MANAGE_TAX_RULES,
                    BusinessOperations.TAX_MANAGE_TAX_PROFILES,
                    BusinessOperations.SYSTEM_IMPORT_DATA,
                    BusinessOperations.SYSTEM_EXPORT_DATA,
                    BusinessOperations.SYSTEM_VIEW_PERFORMANCE_LOGS
                },

                [Roles.SENIOR_ACCOUNTANT] = new[]
                {
                    BusinessOperations.ACCOUNTING_POST_TRANSACTION,
                    BusinessOperations.ACCOUNTING_REVERSE_TRANSACTION,
                    BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES,
                    BusinessOperations.ACCOUNTING_GENERATE_REPORTS,
                    BusinessOperations.ACCOUNTING_MANAGE_CHART_OF_ACCOUNTS,
                    BusinessOperations.DOCUMENTS_CREATE_SALES_INVOICE,
                    BusinessOperations.DOCUMENTS_CREATE_PURCHASE_INVOICE,
                    BusinessOperations.DOCUMENTS_APPROVE_DOCUMENT,
                    BusinessOperations.TAX_CALCULATE_TAXES,
                    BusinessOperations.TAX_MANAGE_TAX_PROFILES
                },

                [Roles.ACCOUNTANT] = new[]
                {
                    BusinessOperations.ACCOUNTING_POST_TRANSACTION,
                    BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES,
                    BusinessOperations.ACCOUNTING_GENERATE_REPORTS,
                    BusinessOperations.DOCUMENTS_CREATE_SALES_INVOICE,
                    BusinessOperations.DOCUMENTS_CREATE_PURCHASE_INVOICE,
                    BusinessOperations.TAX_CALCULATE_TAXES
                },

                [Roles.SALES_MANAGER] = new[]
                {
                    BusinessOperations.DOCUMENTS_CREATE_SALES_INVOICE,
                    BusinessOperations.DOCUMENTS_APPROVE_DOCUMENT,
                    BusinessOperations.TAX_CALCULATE_TAXES,
                    BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES
                },

                [Roles.PURCHASE_MANAGER] = new[]
                {
                    BusinessOperations.DOCUMENTS_CREATE_PURCHASE_INVOICE,
                    BusinessOperations.DOCUMENTS_APPROVE_DOCUMENT,
                    BusinessOperations.TAX_CALCULATE_TAXES,
                    BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES
                },

                [Roles.FINANCIAL_CONTROLLER] = new[]
                {
                    BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES,
                    BusinessOperations.ACCOUNTING_GENERATE_REPORTS,
                    BusinessOperations.ACCOUNTING_CREATE_FISCAL_PERIOD,
                    BusinessOperations.ACCOUNTING_CLOSE_FISCAL_PERIOD,
                    BusinessOperations.DOCUMENTS_VIEW_SENSITIVE_DATA,
                    BusinessOperations.SYSTEM_VIEW_PERFORMANCE_LOGS
                },

                [Roles.USER] = new[]
                {
                    BusinessOperations.DOCUMENTS_CREATE_SALES_INVOICE,
                    BusinessOperations.DOCUMENTS_CREATE_PURCHASE_INVOICE,
                    BusinessOperations.TAX_CALCULATE_TAXES
                },

                [Roles.VIEWER] = new[]
                {
                    BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES,
                    BusinessOperations.ACCOUNTING_GENERATE_REPORTS
                }
            };
        }
    }
}
