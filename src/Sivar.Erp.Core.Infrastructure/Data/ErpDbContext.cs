using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Core.Domain.Entities;
using Sivar.Erp.Core.Domain.Entities.Identity;
using Sivar.Erp.Core.Domain.Entities.Accounting;
using Sivar.Erp.Core.Domain.Entities.Inventory;
using Sivar.Erp.Core.Domain.Entities.Sales;
using Sivar.Erp.Core.Domain.Entities.BusinessEntities;

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
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }

    // Accounting entities
    public DbSet<Account> Accounts { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
    public DbSet<AccountingPeriod> AccountingPeriods { get; set; }
    public DbSet<FiscalYear> FiscalYears { get; set; }

    // Inventory entities
    public DbSet<Product> Products { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<StockLevel> StockLevels { get; set; }

    // Sales entities
    public DbSet<Customer> Customers { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }

    // Business Entities
    public DbSet<BusinessEntity> BusinessEntities { get; set; }
    public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLine> InvoiceLines { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships and constraints
        ConfigureIdentityEntities(modelBuilder);
        ConfigureAccountingEntities(modelBuilder);
        ConfigureInventoryEntities(modelBuilder);
        ConfigureSalesEntities(modelBuilder);
        ConfigureBusinessEntities(modelBuilder);

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

        // UserRole configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => ur.Id);
            entity.HasIndex(ur => new { ur.UserId, ur.CompanyId, ur.BranchId, ur.RoleName }).IsUnique();
            entity.HasIndex(ur => new { ur.CompanyId, ur.RoleName });
            entity.Property(ur => ur.Version).IsRowVersion();

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Company)
                .WithMany()
                .HasForeignKey(ur => ur.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Branch)
                .WithMany()
                .HasForeignKey(ur => ur.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserPermission configuration
        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.HasKey(up => up.Id);
            entity.HasIndex(up => new { up.UserId, up.CompanyId, up.BranchId, up.PermissionName }).IsUnique();
            entity.HasIndex(up => new { up.CompanyId, up.PermissionName });
            entity.Property(up => up.Version).IsRowVersion();

            entity.HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(up => up.Company)
                .WithMany()
                .HasForeignKey(up => up.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(up => up.Branch)
                .WithMany()
                .HasForeignKey(up => up.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(up => up.GrantedByUser)
                .WithMany()
                .HasForeignKey(up => up.GrantedByUserId)
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

    /// <summary>
    /// Configure inventory entity relationships and constraints
    /// </summary>
    private static void ConfigureInventoryEntities(ModelBuilder modelBuilder)
    {
        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.UnitOfMeasure).IsRequired().HasMaxLength(20);
            entity.Property(e => e.StandardCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MinimumStock).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaximumStock).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ReorderPoint).HasColumnType("decimal(18,2)");

            // Unique constraint on product code within company
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
            entity.HasIndex(e => e.Barcode);
        });

        // Warehouse configuration
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Address).HasMaxLength(500);

            // Unique constraint on warehouse code within company
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
        });

        // Inventory Transaction configuration
        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            // Relationships
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.InventoryTransactions)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Warehouse)
                  .WithMany(e => e.InventoryTransactions)
                  .HasForeignKey(e => e.WarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing relationship for reversals
            entity.HasOne(e => e.RelatedTransaction)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedTransactionId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.TransactionNumber).IsUnique();
            entity.HasIndex(e => e.TransactionDate);
            entity.HasIndex(e => new { e.ProductId, e.TransactionDate });
        });

        // Stock Level configuration
        modelBuilder.Entity<StockLevel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuantityOnHand).HasColumnType("decimal(18,4)");
            entity.Property(e => e.QuantityReserved).HasColumnType("decimal(18,4)");
            entity.Property(e => e.AverageCost).HasColumnType("decimal(18,2)");

            // Relationships
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.StockLevels)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Warehouse)
                  .WithMany(e => e.StockLevels)
                  .HasForeignKey(e => e.WarehouseId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: one stock level per product per warehouse
            entity.HasIndex(e => new { e.ProductId, e.WarehouseId }).IsUnique();
        });
    }

    /// <summary>
    /// Configure Sales entities relationships and constraints
    /// </summary>
    private void ConfigureSalesEntities(ModelBuilder modelBuilder)
    {
        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CurrentBalance).HasColumnType("decimal(18,2)");

            // Unique constraint on Code per Company
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CustomerType);
        });

        // Sales Order configuration
        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxPercent).HasColumnType("decimal(5,2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ShippingAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

            // Relationships
            entity.HasOne(e => e.Customer)
                  .WithMany(e => e.SalesOrders)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint on OrderNumber per Company
            entity.HasIndex(e => new { e.CompanyId, e.OrderNumber }).IsUnique();
            entity.HasIndex(e => e.OrderDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.CustomerId, e.OrderDate });
        });

        // Sales Order Line configuration
        modelBuilder.Entity<SalesOrderLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.QuantityShipped).HasColumnType("decimal(18,4)");
            entity.Property(e => e.QuantityInvoiced).HasColumnType("decimal(18,4)");

            // Relationships
            entity.HasOne(e => e.SalesOrder)
                  .WithMany(e => e.SalesOrderLines)
                  .HasForeignKey(e => e.SalesOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint on LineNumber per SalesOrder
            entity.HasIndex(e => new { e.SalesOrderId, e.LineNumber }).IsUnique();
            entity.HasIndex(e => e.ProductId);
        });

        // Invoice configuration
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxPercent).HasColumnType("decimal(5,2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BalanceDue).HasColumnType("decimal(18,2)");

            // Relationships
            entity.HasOne(e => e.Customer)
                  .WithMany(e => e.Invoices)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SalesOrder)
                  .WithMany(e => e.Invoices)
                  .HasForeignKey(e => e.SalesOrderId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Unique constraint on InvoiceNumber per Company
            entity.HasIndex(e => new { e.CompanyId, e.InvoiceNumber }).IsUnique();
            entity.HasIndex(e => e.InvoiceDate);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.CustomerId, e.InvoiceDate });
        });

        // Invoice Line configuration
        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");

            // Relationships
            entity.HasOne(e => e.Invoice)
                  .WithMany(e => e.InvoiceLines)
                  .HasForeignKey(e => e.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SalesOrderLine)
                  .WithMany(e => e.InvoiceLines)
                  .HasForeignKey(e => e.SalesOrderLineId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Unique constraint on LineNumber per Invoice
            entity.HasIndex(e => new { e.InvoiceId, e.LineNumber }).IsUnique();
            entity.HasIndex(e => e.ProductId);
        });
    }

    /// <summary>
    /// Configure Business Entities relationships and constraints
    /// </summary>
    private static void ConfigureBusinessEntities(ModelBuilder modelBuilder)
    {
        // Business Entity configuration
        modelBuilder.Entity<BusinessEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // String properties
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            // Address properties
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.ZipCode).HasMaxLength(20);
            
            // Decimal properties with precision
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(18,2)");
            
            // Enum property
            entity.Property(e => e.EntityType).HasConversion<string>().HasMaxLength(50);
            
            // Relationships
            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.UpdatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique()
                  .HasDatabaseName("IX_BusinessEntity_CompanyId_Code");
            
            entity.HasIndex(e => new { e.CompanyId, e.TaxId })
                  .HasDatabaseName("IX_BusinessEntity_CompanyId_TaxId");
            
            entity.HasIndex(e => new { e.CompanyId, e.EntityType })
                  .HasDatabaseName("IX_BusinessEntity_CompanyId_EntityType");
            
            entity.HasIndex(e => new { e.CompanyId, e.Name })
                  .HasDatabaseName("IX_BusinessEntity_CompanyId_Name");
            
            entity.HasIndex(e => e.Email)
                  .HasDatabaseName("IX_BusinessEntity_Email");
            
            entity.HasIndex(e => new { e.CompanyId, e.IsActive })
                  .HasDatabaseName("IX_BusinessEntity_CompanyId_IsActive");
            
            entity.HasIndex(e => new { e.EntityType, e.IsActive })
                  .HasDatabaseName("IX_BusinessEntity_EntityType_IsActive");
        });
    }
}
