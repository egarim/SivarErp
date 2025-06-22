using Microsoft.Extensions.Logging;
using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.Services;

namespace Sivar.Erp.ErpSystem.Modules.Security
{
    public class SecurityModule : ISecurityModule
    {
        protected readonly ILogger<SecurityModule> _logger;
        protected readonly PerformanceLogger<SecurityModule> _performanceLogger;
        protected readonly IObjectDb _objectDb;

        public virtual ISecurityContext SecurityContext { get; protected set; }
        public virtual IAuthorizationService AuthorizationService { get; protected set; }
        public virtual IUserService UserService { get; protected set; }
        public virtual IRoleService RoleService { get; protected set; }
        public virtual IPermissionService PermissionService { get; protected set; }
        public virtual IAuditService AuditService { get; protected set; }

        public SecurityModule(
            ILogger<SecurityModule> logger,
            IObjectDb objectDb,
            ISecurityContext securityContext,
            IAuthorizationService authorizationService,
            IUserService userService,
            IRoleService roleService,
            IPermissionService permissionService,
            IAuditService auditService)
        {
            _logger = logger;
            _objectDb = objectDb;
            _performanceLogger = new PerformanceLogger<SecurityModule>(logger, objectDb: objectDb);

            SecurityContext = securityContext;
            AuthorizationService = authorizationService;
            UserService = userService;
            RoleService = roleService;
            PermissionService = permissionService;
            AuditService = auditService;
        }

        public virtual async Task InitializeAsync()
        {
            await _performanceLogger.Track(nameof(InitializeAsync), async () =>
            {
                _logger.LogInformation("Initializing Security Module");

                await InitializeDefaultRolesAndPermissions();
                await InitializeDefaultUsers();

                _logger.LogInformation("Security Module initialized successfully");
            });
        }

        public virtual async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
        {
            return await _performanceLogger.Track(nameof(AuthenticateAsync), async () =>
            {
                _logger.LogInformation("Attempting authentication for user: {Username}", username);

                var result = await UserService.AuthenticateAsync(username, password);

                if (result.IsSuccessful && result.User != null)
                {
                    // Set the authenticated user in the security context
                    if (SecurityContext is Platform.GenericSecurityContext genericContext)
                    {
                        genericContext.SetCurrentUser(result.User);
                    }

                    _logger.LogInformation("Authentication successful for user: {Username}", username);
                    await AuditService.LogAsync("Authentication", "Success", $"User {username} authenticated successfully");
                }
                else
                {
                    _logger.LogWarning("Authentication failed for user: {Username}. Reason: {Reason}", username, result.ErrorMessage);
                    await AuditService.LogAsync("Authentication", "Failed", $"Authentication failed for user {username}: {result.ErrorMessage}");
                }

                return result;
            });
        }

        public virtual async Task LogoutAsync()
        {
            await _performanceLogger.Track(nameof(LogoutAsync), async () =>
            {
                if (SecurityContext.IsAuthenticated)
                {
                    var username = SecurityContext.UserName;
                    _logger.LogInformation("User {Username} logging out", username);

                    // Log out from security context
                    if (SecurityContext is Platform.GenericSecurityContext genericContext)
                    {
                        genericContext.Logout();
                    }

                    await AuditService.LogAsync("Authentication", "Logout", $"User {username} logged out");
                }
            });
        }

        public virtual async Task<SecurityResult> CheckBusinessOperationAsync(string operation, object? context = null)
        {
            return await _performanceLogger.Track(nameof(CheckBusinessOperationAsync), async () =>
            {
                var result = await AuthorizationService.CheckBusinessOperationAsync(operation, context);

                if (!result.IsAuthorized)
                {
                    await AuditService.LogAsync("Authorization", "Denied",
                        $"Business operation '{operation}' denied for user {SecurityContext.UserName}. Reason: {result.DenialReason}");
                }

                return result;
            });
        }

        protected virtual async Task InitializeDefaultRolesAndPermissions()
        {
            _logger.LogInformation("Initializing default roles and permissions");

            // Create default roles with their permissions
            var defaultRoles = GetDefaultRoleDefinitions();

            foreach (var roleDefinition in defaultRoles)
            {
                var existingRole = await RoleService.GetRoleByNameAsync(roleDefinition.Name);
                if (existingRole == null)
                {
                    await RoleService.CreateRoleAsync(new CreateRoleRequest
                    {
                        Name = roleDefinition.Name,
                        DisplayName = roleDefinition.DisplayName,
                        Description = roleDefinition.Description,
                        Permissions = roleDefinition.Permissions.ToList()
                    });

                    _logger.LogInformation("Created default role: {RoleName} with {PermissionCount} permissions",
                        roleDefinition.Name, roleDefinition.Permissions.Length);
                }
            }
        }

        protected virtual async Task InitializeDefaultUsers()
        {
            _logger.LogInformation("Checking for default administrator user");

            var adminUser = await UserService.GetUserByUsernameAsync("admin");
            if (adminUser == null)
            {
                await UserService.CreateUserAsync(new CreateUserRequest
                {
                    Username = "admin",
                    Password = "admin123", // Should be changed on first login
                    Email = "admin@company.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    Roles = new List<string> { Roles.SYSTEM_ADMINISTRATOR },
                    IsActive = true
                });

                _logger.LogInformation("Created default administrator user");
            }
        }

        protected virtual RoleDefinition[] GetDefaultRoleDefinitions()
        {
            var allOperations = BusinessOperations.GetAllOperations().ToArray();

            return new[]
            {
                new RoleDefinition
                {
                    Name = Roles.SYSTEM_ADMINISTRATOR,
                    DisplayName = "System Administrator",
                    Description = "Full system access with all permissions",
                    Permissions = allOperations
                },
                new RoleDefinition
                {
                    Name = Roles.ADMINISTRATOR,
                    DisplayName = "Administrator",
                    Description = "Administrative access to most system functions",
                    Permissions = new[]
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
                    }
                },
                new RoleDefinition
                {
                    Name = Roles.SENIOR_ACCOUNTANT,
                    DisplayName = "Senior Accountant",
                    Description = "Advanced accounting operations with approval rights",
                    Permissions = new[]
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
                    }
                },
                new RoleDefinition
                {
                    Name = Roles.ACCOUNTANT,
                    DisplayName = "Accountant",
                    Description = "Standard accounting operations",
                    Permissions = new[]
                    {
                        BusinessOperations.ACCOUNTING_POST_TRANSACTION,
                        BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES,
                        BusinessOperations.ACCOUNTING_GENERATE_REPORTS,
                        BusinessOperations.DOCUMENTS_CREATE_SALES_INVOICE,
                        BusinessOperations.DOCUMENTS_CREATE_PURCHASE_INVOICE,
                        BusinessOperations.TAX_CALCULATE_TAXES
                    }
                },
                new RoleDefinition
                {
                    Name = Roles.SALES_MANAGER,
                    DisplayName = "Sales Manager",
                    Description = "Sales document creation and approval",
                    Permissions = new[]
                    {
                        BusinessOperations.DOCUMENTS_CREATE_SALES_INVOICE,
                        BusinessOperations.DOCUMENTS_APPROVE_DOCUMENT,
                        BusinessOperations.TAX_CALCULATE_TAXES,
                        BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES
                    }
                },
                new RoleDefinition
                {
                    Name = Roles.PURCHASE_MANAGER,
                    DisplayName = "Purchase Manager",
                    Description = "Purchase document creation and approval",
                    Permissions = new[]
                    {
                        BusinessOperations.DOCUMENTS_CREATE_PURCHASE_INVOICE,
                        BusinessOperations.DOCUMENTS_APPROVE_DOCUMENT,
                        BusinessOperations.TAX_CALCULATE_TAXES,
                        BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES
                    }
                },
                new RoleDefinition
                {
                    Name = Roles.FINANCIAL_CONTROLLER,
                    DisplayName = "Financial Controller",
                    Description = "Financial oversight and reporting",
                    Permissions = new[]
                    {
                        BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES,
                        BusinessOperations.ACCOUNTING_GENERATE_REPORTS,
                        BusinessOperations.ACCOUNTING_CREATE_FISCAL_PERIOD,
                        BusinessOperations.ACCOUNTING_CLOSE_FISCAL_PERIOD,
                        BusinessOperations.DOCUMENTS_VIEW_SENSITIVE_DATA,
                        BusinessOperations.SYSTEM_VIEW_PERFORMANCE_LOGS
                    }
                },
                new RoleDefinition
                {
                    Name = Roles.USER,
                    DisplayName = "User",
                    Description = "Basic document creation operations",
                    Permissions = new[]
                    {
                        BusinessOperations.DOCUMENTS_CREATE_SALES_INVOICE,
                        BusinessOperations.DOCUMENTS_CREATE_PURCHASE_INVOICE,
                        BusinessOperations.TAX_CALCULATE_TAXES
                    }
                },
                new RoleDefinition
                {
                    Name = Roles.VIEWER,
                    DisplayName = "Viewer",
                    Description = "Read-only access to reports and journal entries",
                    Permissions = new[]
                    {
                        BusinessOperations.ACCOUNTING_VIEW_JOURNAL_ENTRIES,
                        BusinessOperations.ACCOUNTING_GENERATE_REPORTS
                    }
                }
            };
        }

        protected class RoleDefinition
        {
            public string Name { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string[] Permissions { get; set; } = Array.Empty<string>();
        }
    }
}
