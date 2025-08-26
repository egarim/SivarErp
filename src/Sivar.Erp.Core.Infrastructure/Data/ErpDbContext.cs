using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Core.Domain.Entities;
using Sivar.Erp.Core.Domain.Entities.Identity;
using Sivar.Erp.Core.Domain.Entities.Accounting;

namespace Sivar.Erp.Core.Infrastructure.Data;

/// <summary>
/// Main ERP DbContext with enhanced telemetry and performance monitoring
/// </summary>
public class ErpDbContext : DbContext
{
    private readonly string? _currentCompanyId;

    public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options)
    {
    }

    public ErpDbContext(DbContextOptions<ErpDbContext> options, string? companyId) : base(options)
    {
        _currentCompanyId = companyId;
    }

    // Identity entities
    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<UserCompany> UserCompanies { get; set; }
    public DbSet<UserInvitation> UserInvitations { get; set; }

    // Accounting entities
    public DbSet<Account> Accounts { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
    public DbSet<AccountingPeriod> AccountingPeriods { get; set; }
    public DbSet<FiscalYear> FiscalYears { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships and constraints
        ConfigureIdentityEntities(modelBuilder);
        ConfigureAccountingEntities(modelBuilder);

        // Apply global query filters for soft delete
        ConfigureGlobalQueryFilters(modelBuilder);

        // Apply tenant isolation if company context is available
        if (!string.IsNullOrEmpty(_currentCompanyId) && Guid.TryParse(_currentCompanyId, out var companyId))
        {
            ConfigureTenantFilters(modelBuilder, companyId);
        }
    }

    private static void ConfigureIdentityEntities(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.KeycloakUserId).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Version).IsRowVersion();
        });

        // Company configuration
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.Name);
            entity.Property(c => c.Version).IsRowVersion();
        });

        // Branch configuration
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => new { b.CompanyId, b.Code }).IsUnique();
            entity.Property(b => b.Version).IsRowVersion();

            entity.HasOne(b => b.Company)
                .WithMany(c => c.Branches)
                .HasForeignKey(b => b.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Manager)
                .WithMany()
                .HasForeignKey(b => b.ManagerUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // UserCompany configuration
        modelBuilder.Entity<UserCompany>(entity =>
        {
            entity.HasKey(uc => uc.Id);
            entity.HasIndex(uc => new { uc.UserId, uc.CompanyId }).IsUnique();
            entity.Property(uc => uc.Version).IsRowVersion();

            entity.HasOne(uc => uc.User)
                .WithMany(u => u.UserCompanies)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(uc => uc.Company)
                .WithMany(c => c.UserCompanies)
                .HasForeignKey(uc => uc.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(uc => uc.InvitedByUser)
                .WithMany()
                .HasForeignKey(uc => uc.InvitedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // UserInvitation configuration
        modelBuilder.Entity<UserInvitation>(entity =>
        {
            entity.HasKey(ui => ui.Id);
            entity.HasIndex(ui => ui.Token).IsUnique();
            entity.HasIndex(ui => new { ui.Email, ui.CompanyId, ui.Status });
            entity.Property(ui => ui.Version).IsRowVersion();

            entity.HasOne(ui => ui.Company)
                .WithMany(c => c.Invitations)
                .HasForeignKey(ui => ui.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ui => ui.InvitedByUser)
                .WithMany(u => u.SentInvitations)
                .HasForeignKey(ui => ui.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ui => ui.InvitedUser)
                .WithMany(u => u.ReceivedInvitations)
                .HasForeignKey(ui => ui.InvitedUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Apply soft delete filter to all entities that inherit from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    private static System.Linq.Expressions.LambdaExpression CreateSoftDeleteFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(condition, parameter);
    }

    private static void ConfigureTenantFilters(ModelBuilder modelBuilder, Guid companyId)
    {
        // Apply tenant filter to all entities that implement ITenantEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateTenantFilter(entityType.ClrType, companyId));
            }
        }
    }

    private static System.Linq.Expressions.LambdaExpression CreateTenantFilter(Type entityType, Guid companyId)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ITenantEntity.CompanyId));
        var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(companyId));
        return System.Linq.Expressions.Expression.Lambda(condition, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(string? userId, CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(userId);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields(string? userId = null)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.SetModified(userId);
            }
        }
    }

    private static void ConfigureAccountingEntities(ModelBuilder modelBuilder)
    {
        // Account configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.TaxCode).HasMaxLength(20);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);

            // Unique constraint on Code within a company
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();

            // Self-referencing relationship for account hierarchy
            entity.HasOne(e => e.ParentAccount)
                  .WithMany(e => e.SubAccounts)
                  .HasForeignKey(e => e.ParentAccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Journal Entry configuration
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 6);
            entity.Property(e => e.TotalDebit).HasPrecision(18, 2);
            entity.Property(e => e.TotalCredit).HasPrecision(18, 2);
            entity.Property(e => e.SourceType).HasMaxLength(50);
            entity.Property(e => e.PostedBy).HasMaxLength(255);
            entity.Property(e => e.ApprovedBy).HasMaxLength(255);
            entity.Property(e => e.ApprovalNotes).HasMaxLength(1000);

            // Unique constraint on Number within a company
            entity.HasIndex(e => new { e.CompanyId, e.Number }).IsUnique();

            // Relationship with accounting period
            entity.HasOne(e => e.AccountingPeriod)
                  .WithMany(e => e.JournalEntries)
                  .HasForeignKey(e => e.AccountingPeriodId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Journal Entry Line configuration
        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.DebitAmount).HasPrecision(18, 2);
            entity.Property(e => e.CreditAmount).HasPrecision(18, 2);
            entity.Property(e => e.DepartmentCode).HasMaxLength(20);
            entity.Property(e => e.ProjectCode).HasMaxLength(20);
            entity.Property(e => e.CustomerVendorRef).HasMaxLength(100);
            entity.Property(e => e.Reference).HasMaxLength(255);
            entity.Property(e => e.TaxCode).HasMaxLength(20);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.BaseAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 6);
            entity.Property(e => e.ForeignAmount).HasPrecision(18, 2);
            entity.Property(e => e.ReconciliationRef).HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.JournalEntry)
                  .WithMany(e => e.Lines)
                  .HasForeignKey(e => e.JournalEntryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                  .WithMany(e => e.JournalEntryLines)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Accounting Period configuration
        modelBuilder.Entity<AccountingPeriod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClosedBy).HasMaxLength(255);
            entity.Property(e => e.ClosureNotes).HasMaxLength(1000);

            // Unique constraint on period within fiscal year
            entity.HasIndex(e => new { e.FiscalYearId, e.PeriodNumber }).IsUnique();

            // Relationship with fiscal year
            entity.HasOne(e => e.FiscalYear)
                  .WithMany(e => e.Periods)
                  .HasForeignKey(e => e.FiscalYearId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Fiscal Year configuration
        modelBuilder.Entity<FiscalYear>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ClosedBy).HasMaxLength(255);

            // Unique constraint on fiscal year name within company
            entity.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();
        });
    }
}
