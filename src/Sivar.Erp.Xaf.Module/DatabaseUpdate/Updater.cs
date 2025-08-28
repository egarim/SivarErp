using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.MultiTenancy;
using DevExpress.ExpressApp.EF;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.MultiTenancy;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Sivar.Erp.EfCore.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Sivar.Erp.EfCore.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion) {
    }
    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();
        //string name = "MyName";
        //EntityObject1 theObject = ObjectSpace.FirstOrDefault<EntityObject1>(u => u.Name == name);
        //if(theObject == null) {
        //    theObject = ObjectSpace.CreateObject<EntityObject1>();
        //    theObject.Name = name;
        //}
        if (!ObjectSpace.CanInstantiate(typeof(ApplicationUser))) {
            return;
        }

#if !RELEASE
        if (TenantName == null) {
            _ = CreateTenant("company1.com", "Sivar.Erp.Xaf_company1");
            _ = CreateTenant("company2.com", "Sivar.Erp.Xaf_company2");
            ObjectSpace.CommitChanges();
        }
#endif

        // The code below creates users and roles for testing purposes only.
        // In production code, you can create users and assign roles to them automatically, as described in the following help topic:
        // https://docs.devexpress.com/eXpressAppFramework/119064/data-security-and-safety/security-system/authentication
#if !RELEASE
        // If a role doesn't exist in the database, create this role
        var adminRole = CreateAdminRole();

        UserManager userManager = ObjectSpace.ServiceProvider.GetRequiredService<UserManager>();

        if (TenantName != null) {
            var defaultRole = CreateDefaultRole();

            string userName = $"User@{TenantName}";
            // If a user named 'userName' doesn't exist in the database, create this user
            if(userManager.FindUserByName<ApplicationUser>(ObjectSpace, userName) == null) {
                // Set a password if the standard authentication type is used
                string EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, userName, EmptyPassword, (user) => {
                    // Add the Users role to the user
                    user.Roles.Add(defaultRole);
                });
            }
        }

        string adminUserName = TenantName != null ? $"Admin@{TenantName}" : "Admin";
        if(userManager.FindUserByName<ApplicationUser>(ObjectSpace, adminUserName) == null) {
            // Set a password if the standard authentication type is used
            string EmptyPassword = "";
            _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, adminUserName, EmptyPassword, (user) => {
                // Add the Administrators role to the user
                user.Roles.Add(adminRole);
            });
        }

        ObjectSpace.CommitChanges(); //This line persists created object(s).
#endif
    }
    public override void UpdateDatabaseBeforeUpdateSchema() {
        base.UpdateDatabaseBeforeUpdateSchema();
    }
    private Tenant CreateTenant(string tenantName, string databaseName) {
        var tenant = ObjectSpace.FirstOrDefault<Tenant>(t => t.Name == tenantName);
        if (tenant == null) {
            tenant = ObjectSpace.CreateObject<Tenant>();
            tenant.Name = tenantName;
            tenant.ConnectionString = $"Integrated Security=SSPI;MultipleActiveResultSets=True;Data Source=(localdb)\\mssqllocaldb;Initial Catalog={databaseName}";
        }
        return tenant;
    }
    private PermissionPolicyRole CreateAdminRole() {
        PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
        if(adminRole == null) {
            adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            adminRole.Name = "Administrators";
            adminRole.IsAdministrative = true;
        }
        return adminRole;
    }
    private PermissionPolicyRole CreateDefaultRole() {
        PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
        if(defaultRole == null) {
            defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            defaultRole.Name = "Default";

            defaultRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
            defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
            defaultRole.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess, "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
            defaultRole.AddObjectPermission<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, "Owner.UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
        }
        return defaultRole;
    }

    Guid? TenantId {
        get {
            return ObjectSpace.ServiceProvider.GetRequiredService<ITenantProvider>().TenantId;
        }
    }
    string TenantName {
        get {
            return ObjectSpace.ServiceProvider.GetRequiredService<ITenantProvider>().TenantName;
        }
    }
}
