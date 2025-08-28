using DevExpress.ExpressApp.EFCore.Updating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;
using DevExpress.Persistent.BaseImpl.EF.StateMachine;
using Sivar.Erp.Xaf.Module.BusinessObjects;

namespace Sivar.Erp.EfCore.BusinessObjects;

// This code allows our Model Editor to get relevant EF Core metadata at design time.
// For details, please refer to https://supportcenter.devexpress.com/ticket/details/t933891/core-prerequisites-for-design-time-model-editor-with-entity-framework-core-data-model.
public class XafContextInitializer : DbContextTypesInfoInitializerBase {
    protected override DbContext CreateDbContext() {
        var optionsBuilder = new DbContextOptionsBuilder<XafEFCoreDbContext>()
            .UseSqlServer(";")//.UseSqlite(";") wrong for a solution with SqLite, see https://isc.devexpress.com/internal/ticket/details/t1240173
            .UseChangeTrackingProxies()
            .UseObjectSpaceLinkProxies();
        return new XafEFCoreDbContext(optionsBuilder.Options);
    }
}
//This factory creates DbContext for design-time services. For example, it is required for database migration.
public class XafDesignTimeDbContextFactory : IDesignTimeDbContextFactory<XafEFCoreDbContext> {
    public XafEFCoreDbContext CreateDbContext(string[] args) {
        throw new InvalidOperationException("Make sure that the database connection string and connection provider are correct. After that, uncomment the code below and remove this exception.");
        //var optionsBuilder = new DbContextOptionsBuilder<XafEFCoreDbContext>();
        //optionsBuilder.UseSqlServer("Integrated Security=SSPI;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=Sivar.Erp.Xaf");
        //optionsBuilder.UseChangeTrackingProxies();
        //optionsBuilder.UseObjectSpaceLinkProxies();
        //return new XafEFCoreDbContext(optionsBuilder.Options);
    }
}
[TypesInfoInitializer(typeof(XafContextInitializer))]
public class XafEFCoreDbContext : SivarErpDbContext
{
    public XafEFCoreDbContext(DbContextOptions<XafEFCoreDbContext> options) : base(options) {
    }
    //public DbSet<ModuleInfo> ModulesInfo { get; set; }

    public DbSet<ImportFile> ImportFiles { get; set; }
    public DbSet<ModelDifference> ModelDifferences { get; set; }
    public DbSet<ModelDifferenceAspect> ModelDifferenceAspects { get; set; }
    public DbSet<PermissionPolicyRole> Roles { get; set; }
    public DbSet<Sivar.Erp.EfCore.BusinessObjects.ApplicationUser> Users { get; set; }
    public DbSet<Sivar.Erp.EfCore.BusinessObjects.ApplicationUserLoginInfo> UserLoginsInfo { get; set; }
    public DbSet<FileData> FileData { get; set; }
    public DbSet<ReportDataV2> ReportDataV2 { get; set; }
    public DbSet<StateMachine> StateMachines { get; set; }
    public DbSet<StateMachineState> StateMachineStates { get; set; }
    public DbSet<StateMachineTransition> StateMachineTransitions { get; set; }
    public DbSet<StateMachineAppearance> StateMachineAppearances { get; set; }
    public DbSet<DashboardData> DashboardData { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<HCategory> HCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseDeferredDeletion(this);
        modelBuilder.UseOptimisticLock();
        modelBuilder.SetOneToManyAssociationDeleteBehavior(DeleteBehavior.SetNull, DeleteBehavior.Cascade);
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
        modelBuilder.Entity<Sivar.Erp.EfCore.BusinessObjects.ApplicationUserLoginInfo>(b => {
            b.HasIndex(nameof(DevExpress.ExpressApp.Security.ISecurityUserLoginInfo.LoginProviderName), nameof(DevExpress.ExpressApp.Security.ISecurityUserLoginInfo.ProviderUserKey)).IsUnique();
        });
        modelBuilder.Entity<StateMachine>()
            .HasMany(t => t.States)
            .WithOne(t => t.StateMachine)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ModelDifference>()
            .HasMany(t => t.Aspects)
            .WithOne(t => t.Owner)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
