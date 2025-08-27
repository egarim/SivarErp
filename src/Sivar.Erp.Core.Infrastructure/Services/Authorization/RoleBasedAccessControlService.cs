using Sivar.Erp.Core.Domain.Entities.Identity;
using Sivar.Erp.Core.Infrastructure.Services.Identity;
using Sivar.Erp.Core.Infrastructure.Repositories.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Sivar.Erp.Core.Infrastructure.Services.Authorization;

/// <summary>
/// Available roles in the ERP system
/// </summary>
public static class ErpRoles
{
    public const string SystemAdmin = "system_admin";
    public const string CompanyOwner = "company_owner";
    public const string CompanyAdmin = "company_admin";
    public const string BranchManager = "branch_manager";
    public const string Accountant = "accountant";
    public const string SalesManager = "sales_manager";
    public const string SalesUser = "sales_user";
    public const string InventoryManager = "inventory_manager";
    public const string InventoryUser = "inventory_user";
    public const string Employee = "employee";
    public const string ReadOnly = "read_only";

    /// <summary>
    /// Gets all available roles
    /// </summary>
    public static readonly string[] AllRoles = {
        SystemAdmin, CompanyOwner, CompanyAdmin, BranchManager,
        Accountant, SalesManager, SalesUser, InventoryManager,
        InventoryUser, Employee, ReadOnly
    };

    /// <summary>
    /// Gets roles that have administrative privileges
    /// </summary>
    public static readonly string[] AdminRoles = {
        SystemAdmin, CompanyOwner, CompanyAdmin, BranchManager
    };

    /// <summary>
    /// Gets roles that can manage financial data
    /// </summary>
    public static readonly string[] FinancialRoles = {
        SystemAdmin, CompanyOwner, CompanyAdmin, Accountant
    };

    /// <summary>
    /// Gets roles that can manage sales
    /// </summary>
    public static readonly string[] SalesRoles = {
        SystemAdmin, CompanyOwner, CompanyAdmin, BranchManager, 
        SalesManager, SalesUser
    };

    /// <summary>
    /// Gets roles that can manage inventory
    /// </summary>
    public static readonly string[] InventoryRoles = {
        SystemAdmin, CompanyOwner, CompanyAdmin, BranchManager,
        InventoryManager, InventoryUser
    };
}

/// <summary>
/// Available permissions in the ERP system
/// </summary>
public static class ErpPermissions
{
    // Company Management
    public const string ManageCompanies = "manage_companies";
    public const string ViewCompanies = "view_companies";
    
    // User Management
    public const string ManageUsers = "manage_users";
    public const string ViewUsers = "view_users";
    public const string InviteUsers = "invite_users";
    
    // Branch Management
    public const string ManageBranches = "manage_branches";
    public const string ViewBranches = "view_branches";
    
    // Financial/Accounting
    public const string ManageAccounts = "manage_accounts";
    public const string ViewAccounts = "view_accounts";
    public const string CreateJournalEntries = "create_journal_entries";
    public const string ApproveJournalEntries = "approve_journal_entries";
    public const string ViewFinancialReports = "view_financial_reports";
    
    // Sales
    public const string ManageCustomers = "manage_customers";
    public const string ViewCustomers = "view_customers";
    public const string CreateSalesOrders = "create_sales_orders";
    public const string ManageSalesOrders = "manage_sales_orders";
    public const string CreateInvoices = "create_invoices";
    public const string ManageInvoices = "manage_invoices";
    
    // Inventory
    public const string ManageProducts = "manage_products";
    public const string ViewProducts = "view_products";
    public const string ManageWarehouses = "manage_warehouses";
    public const string ViewWarehouses = "view_warehouses";
    public const string ManageInventory = "manage_inventory";
    public const string ViewInventory = "view_inventory";
}

/// <summary>
/// Interface for role-based access control service
/// </summary>
public interface IRoleBasedAccessControlService
{
    /// <summary>
    /// Checks if the current user has the specified role
    /// </summary>
    Task<bool> HasRoleAsync(string role);

    /// <summary>
    /// Checks if the current user has any of the specified roles
    /// </summary>
    Task<bool> HasAnyRoleAsync(params string[] roles);

    /// <summary>
    /// Checks if the current user has all of the specified roles
    /// </summary>
    Task<bool> HasAllRolesAsync(params string[] roles);

    /// <summary>
    /// Checks if the current user has the specified permission
    /// </summary>
    Task<bool> HasPermissionAsync(string permission);

    /// <summary>
    /// Gets all roles for the current user in the current company
    /// </summary>
    Task<IEnumerable<string>> GetUserRolesAsync();

    /// <summary>
    /// Gets all permissions for the current user in the current company
    /// </summary>
    Task<IEnumerable<string>> GetUserPermissionsAsync();

    /// <summary>
    /// Checks if the current user can access the specified company
    /// </summary>
    Task<bool> CanAccessCompanyAsync(Guid companyId);

    /// <summary>
    /// Checks if the current user can access the specified branch
    /// </summary>
    Task<bool> CanAccessBranchAsync(Guid branchId);

    /// <summary>
    /// Validates access and throws exception if unauthorized
    /// </summary>
    Task ValidateAccessAsync(string permission);

    /// <summary>
    /// Validates role and throws exception if unauthorized
    /// </summary>
    Task ValidateRoleAsync(params string[] roles);
}

/// <summary>
/// Implementation of role-based access control service
/// </summary>
public class RoleBasedAccessControlService : IRoleBasedAccessControlService
{
    private readonly ITenantContextService _tenantContext;
    private readonly IServiceProvider _serviceProvider;

    // Role to permissions mapping
    private static readonly Dictionary<string, string[]> RolePermissions = new()
    {
        {
            ErpRoles.SystemAdmin,
            new[] { "*" } // System admin has all permissions
        },
        {
            ErpRoles.CompanyOwner,
            new[]
            {
                ErpPermissions.ManageCompanies, ErpPermissions.ViewCompanies,
                ErpPermissions.ManageUsers, ErpPermissions.ViewUsers, ErpPermissions.InviteUsers,
                ErpPermissions.ManageBranches, ErpPermissions.ViewBranches,
                ErpPermissions.ManageAccounts, ErpPermissions.ViewAccounts,
                ErpPermissions.CreateJournalEntries, ErpPermissions.ApproveJournalEntries,
                ErpPermissions.ViewFinancialReports,
                ErpPermissions.ManageCustomers, ErpPermissions.ViewCustomers,
                ErpPermissions.CreateSalesOrders, ErpPermissions.ManageSalesOrders,
                ErpPermissions.CreateInvoices, ErpPermissions.ManageInvoices,
                ErpPermissions.ManageProducts, ErpPermissions.ViewProducts,
                ErpPermissions.ManageWarehouses, ErpPermissions.ViewWarehouses,
                ErpPermissions.ManageInventory, ErpPermissions.ViewInventory
            }
        },
        {
            ErpRoles.CompanyAdmin,
            new[]
            {
                ErpPermissions.ViewCompanies,
                ErpPermissions.ManageUsers, ErpPermissions.ViewUsers, ErpPermissions.InviteUsers,
                ErpPermissions.ManageBranches, ErpPermissions.ViewBranches,
                ErpPermissions.ManageAccounts, ErpPermissions.ViewAccounts,
                ErpPermissions.CreateJournalEntries, ErpPermissions.ApproveJournalEntries,
                ErpPermissions.ViewFinancialReports,
                ErpPermissions.ManageCustomers, ErpPermissions.ViewCustomers,
                ErpPermissions.CreateSalesOrders, ErpPermissions.ManageSalesOrders,
                ErpPermissions.CreateInvoices, ErpPermissions.ManageInvoices,
                ErpPermissions.ManageProducts, ErpPermissions.ViewProducts,
                ErpPermissions.ManageWarehouses, ErpPermissions.ViewWarehouses,
                ErpPermissions.ManageInventory, ErpPermissions.ViewInventory
            }
        },
        {
            ErpRoles.BranchManager,
            new[]
            {
                ErpPermissions.ViewCompanies, ErpPermissions.ViewBranches,
                ErpPermissions.ViewUsers, ErpPermissions.InviteUsers,
                ErpPermissions.ViewAccounts, ErpPermissions.CreateJournalEntries,
                ErpPermissions.ViewFinancialReports,
                ErpPermissions.ManageCustomers, ErpPermissions.ViewCustomers,
                ErpPermissions.CreateSalesOrders, ErpPermissions.ManageSalesOrders,
                ErpPermissions.CreateInvoices, ErpPermissions.ManageInvoices,
                ErpPermissions.ManageProducts, ErpPermissions.ViewProducts,
                ErpPermissions.ViewWarehouses, ErpPermissions.ManageInventory, ErpPermissions.ViewInventory
            }
        },
        {
            ErpRoles.Accountant,
            new[]
            {
                ErpPermissions.ViewCompanies, ErpPermissions.ViewBranches,
                ErpPermissions.ManageAccounts, ErpPermissions.ViewAccounts,
                ErpPermissions.CreateJournalEntries, ErpPermissions.ApproveJournalEntries,
                ErpPermissions.ViewFinancialReports
            }
        },
        {
            ErpRoles.SalesManager,
            new[]
            {
                ErpPermissions.ViewCompanies, ErpPermissions.ViewBranches,
                ErpPermissions.ManageCustomers, ErpPermissions.ViewCustomers,
                ErpPermissions.CreateSalesOrders, ErpPermissions.ManageSalesOrders,
                ErpPermissions.CreateInvoices, ErpPermissions.ManageInvoices,
                ErpPermissions.ViewProducts, ErpPermissions.ViewInventory
            }
        },
        {
            ErpRoles.SalesUser,
            new[]
            {
                ErpPermissions.ViewCompanies, ErpPermissions.ViewBranches,
                ErpPermissions.ViewCustomers, ErpPermissions.CreateSalesOrders,
                ErpPermissions.CreateInvoices, ErpPermissions.ViewProducts, ErpPermissions.ViewInventory
            }
        },
        {
            ErpRoles.InventoryManager,
            new[]
            {
                ErpPermissions.ViewCompanies, ErpPermissions.ViewBranches,
                ErpPermissions.ManageProducts, ErpPermissions.ViewProducts,
                ErpPermissions.ManageWarehouses, ErpPermissions.ViewWarehouses,
                ErpPermissions.ManageInventory, ErpPermissions.ViewInventory
            }
        },
        {
            ErpRoles.InventoryUser,
            new[]
            {
                ErpPermissions.ViewCompanies, ErpPermissions.ViewBranches,
                ErpPermissions.ViewProducts, ErpPermissions.ViewWarehouses,
                ErpPermissions.ViewInventory
            }
        },
        {
            ErpRoles.Employee,
            new[]
            {
                ErpPermissions.ViewCompanies, ErpPermissions.ViewBranches,
                ErpPermissions.ViewCustomers, ErpPermissions.ViewProducts,
                ErpPermissions.ViewInventory
            }
        },
        {
            ErpRoles.ReadOnly,
            new[]
            {
                ErpPermissions.ViewCompanies, ErpPermissions.ViewBranches,
                ErpPermissions.ViewUsers, ErpPermissions.ViewAccounts,
                ErpPermissions.ViewFinancialReports, ErpPermissions.ViewCustomers,
                ErpPermissions.ViewProducts, ErpPermissions.ViewWarehouses,
                ErpPermissions.ViewInventory
            }
        }
    };

    public RoleBasedAccessControlService(
        ITenantContextService tenantContext,
        IServiceProvider serviceProvider)
    {
        _tenantContext = tenantContext;
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> HasRoleAsync(string role)
    {
        var userRoles = await GetUserRolesAsync();
        return userRoles.Contains(role);
    }

    public async Task<bool> HasAnyRoleAsync(params string[] roles)
    {
        var userRoles = await GetUserRolesAsync();
        return roles.Any(role => userRoles.Contains(role));
    }

    public async Task<bool> HasAllRolesAsync(params string[] roles)
    {
        var userRoles = await GetUserRolesAsync();
        return roles.All(role => userRoles.Contains(role));
    }

    public async Task<bool> HasPermissionAsync(string permission)
    {
        var userPermissions = await GetUserPermissionsAsync();
        return userPermissions.Contains(permission) || userPermissions.Contains("*");
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync()
    {
        if (_tenantContext.CurrentUserId == null || _tenantContext.CurrentCompanyId == null)
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            
            var user = await userRepository.GetByKeycloakIdAsync(_tenantContext.CurrentUserId);
            if (user == null) return Enumerable.Empty<string>();

            // Get roles for the current company and optionally current branch
            var roles = user.UserRoles
                .Where(ur => ur.CompanyId == _tenantContext.CurrentCompanyId && 
                           ur.IsValid() &&
                           (ur.BranchId == null || ur.BranchId == _tenantContext.CurrentBranchId))
                .Select(ur => ur.RoleName)
                .Distinct();

            return roles;
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync()
    {
        var userRoles = await GetUserRolesAsync();
        var permissions = new HashSet<string>();

        foreach (var role in userRoles)
        {
            if (RolePermissions.TryGetValue(role, out var rolePermissions))
            {
                foreach (var permission in rolePermissions)
                {
                    permissions.Add(permission);
                }
            }
        }

        return permissions;
    }

    public async Task<bool> CanAccessCompanyAsync(Guid companyId)
    {
        return await _tenantContext.HasAccessToCompanyAsync(companyId);
    }

    public async Task<bool> CanAccessBranchAsync(Guid branchId)
    {
        // Implementation would check if the branch belongs to a company the user has access to
        return await Task.FromResult(true); // Placeholder
    }

    public async Task ValidateAccessAsync(string permission)
    {
        _tenantContext.ValidateContext();
        
        if (!await HasPermissionAsync(permission))
        {
            throw new UnauthorizedAccessException($"Access denied. Required permission: {permission}");
        }
    }

    public async Task ValidateRoleAsync(params string[] roles)
    {
        _tenantContext.ValidateContext();
        
        if (!await HasAnyRoleAsync(roles))
        {
            throw new UnauthorizedAccessException($"Access denied. Required roles: {string.Join(", ", roles)}");
        }
    }
}
