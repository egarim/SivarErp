using Microsoft.EntityFrameworkCore;
using Sivar.Erp.EfCore.Entities;

namespace Sivar.Erp.EfCore;

/// <summary>
/// Entity Framework DbContext for the Sivar ERP system
/// </summary>
public class SivarErpDbContext : DbContext
{
    public SivarErpDbContext(DbContextOptions<SivarErpDbContext> options) : base(options)
    {
    }

    // Chart of Accounts and Accounting
    public DbSet<Account> Accounts { get; set; }
    public DbSet<FiscalPeriod> FiscalPeriods { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<LedgerEntry> LedgerEntries { get; set; }
    public DbSet<TransactionBatch> TransactionBatches { get; set; }

    // Business Entities and Documents
    public DbSet<BusinessEntity> BusinessEntities { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<DocumentAccountingProfile> DocumentAccountingProfiles { get; set; }

    // Items and Inventory
    public DbSet<Item> Items { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<StockLevel> StockLevels { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<InventoryReservation> InventoryReservations { get; set; }
    public DbSet<InventoryLayer> InventoryLayers { get; set; }

    // Taxes
    public DbSet<Tax> Taxes { get; set; }
    public DbSet<TaxGroup> TaxGroups { get; set; }
    public DbSet<TaxRule> TaxRules { get; set; }
    public DbSet<GroupMembership> GroupMemberships { get; set; }

    // Payments
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // Security and Users
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<SecurityEvent> SecurityEvents { get; set; }

    // System
    public DbSet<ActivityRecord> ActivityRecords { get; set; }
    public DbSet<Sequence> Sequences { get; set; }
    public DbSet<PerformanceLog> PerformanceLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships and constraints
        ConfigureAccountingEntities(modelBuilder);
        ConfigureBusinessEntities(modelBuilder);
        ConfigureInventoryEntities(modelBuilder);
        ConfigureTaxEntities(modelBuilder);
        ConfigurePaymentEntities(modelBuilder);
        ConfigureSecurityEntities(modelBuilder);
        ConfigureSystemEntities(modelBuilder);
    }

    private void ConfigureAccountingEntities(ModelBuilder modelBuilder)
    {
        // Account entity configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasIndex(e => e.OfficialCode).IsUnique();
            entity.HasIndex(e => e.AccountName);
        });

        // FiscalPeriod entity configuration
        modelBuilder.Entity<FiscalPeriod>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.StartDate, e.EndDate });
        });

        // Transaction entity configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(e => e.TransactionNumber).IsUnique();
            entity.HasIndex(e => e.DocumentNumber);
            entity.HasIndex(e => e.TransactionDate);
            
            entity.HasMany(t => t.LedgerEntries)
                  .WithOne(le => le.Transaction)
                  .HasForeignKey(le => le.TransactionNumber)
                  .HasPrincipalKey(t => t.TransactionNumber);
        });

        // LedgerEntry entity configuration
        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.HasIndex(e => e.LedgerEntryNumber).IsUnique();
            entity.HasIndex(e => e.OfficialCode);
            entity.HasIndex(e => e.TransactionNumber);
        });

        // TransactionBatch entity configuration
        modelBuilder.Entity<TransactionBatch>(entity =>
        {
            entity.HasIndex(e => e.ReferenceCode).IsUnique();
            entity.HasIndex(e => e.BatchDate);
        });
    }

    private void ConfigureBusinessEntities(ModelBuilder modelBuilder)
    {
        // BusinessEntity entity configuration
        modelBuilder.Entity<BusinessEntity>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Email);
        });

        // DocumentType entity configuration
        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.DocumentOperation);
        });

        // DocumentAccountingProfile entity configuration
        modelBuilder.Entity<DocumentAccountingProfile>(entity =>
        {
            entity.HasIndex(e => e.DocumentOperation).IsUnique();
        });
    }

    private void ConfigureInventoryEntities(ModelBuilder modelBuilder)
    {
        // Item entity configuration
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Description);
        });

        // InventoryItem entity configuration
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Description);
        });

        // StockLevel entity configuration
        modelBuilder.Entity<StockLevel>(entity =>
        {
            entity.HasIndex(e => new { e.ItemCode, e.WarehouseCode }).IsUnique();
        });

        // InventoryTransaction entity configuration
        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasIndex(e => e.TransactionNumber).IsUnique();
            entity.HasIndex(e => e.TransactionDate);
            entity.HasIndex(e => e.ReferenceDocumentNumber);
            entity.HasIndex(e => new { e.ItemId, e.TransactionDate });
        });

        // InventoryReservation entity configuration
        modelBuilder.Entity<InventoryReservation>(entity =>
        {
            entity.HasIndex(e => e.ReservationId).IsUnique();
            entity.HasIndex(e => new { e.ItemCode, e.WarehouseCode });
            entity.HasIndex(e => e.SourceDocumentNumber);
        });

        // InventoryLayer entity configuration
        modelBuilder.Entity<InventoryLayer>(entity =>
        {
            entity.HasIndex(e => new { e.ItemCode, e.WarehouseCode, e.CreatedDate });
            entity.HasIndex(e => e.TransactionId);
        });
    }

    private void ConfigureTaxEntities(ModelBuilder modelBuilder)
    {
        // Tax entity configuration
        modelBuilder.Entity<Tax>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
        });

        // TaxGroup entity configuration
        modelBuilder.Entity<TaxGroup>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
        });

        // TaxRule entity configuration
        modelBuilder.Entity<TaxRule>(entity =>
        {
            entity.HasIndex(e => new { e.TaxId, e.DocumentOperation, e.BusinessEntityGroupId, e.ItemGroupId });
            entity.HasIndex(e => e.Priority);
        });

        // GroupMembership entity configuration
        modelBuilder.Entity<GroupMembership>(entity =>
        {
            entity.HasIndex(e => new { e.GroupCode, e.EntityId, e.GroupType }).IsUnique();
        });
    }

    private void ConfigurePaymentEntities(ModelBuilder modelBuilder)
    {
        // PaymentMethod entity configuration
        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Type);
        });

        // Payment entity configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.PaymentId).IsUnique();
            entity.HasIndex(e => e.DocumentNumber);
            entity.HasIndex(e => e.PaymentDate);
            entity.HasIndex(e => e.Status);
            
            entity.HasOne(p => p.PaymentMethod)
                  .WithMany()
                  .HasForeignKey(p => p.PaymentMethodId);
        });
    }

    private void ConfigureSecurityEntities(ModelBuilder modelBuilder)
    {
        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Role entity configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // SecurityEvent entity configuration
        modelBuilder.Entity<SecurityEvent>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.UserId);
        });
    }

    private void ConfigureSystemEntities(ModelBuilder modelBuilder)
    {
        // ActivityRecord entity configuration
        modelBuilder.Entity<ActivityRecord>(entity =>
        {
            entity.HasIndex(e => new { e.Date, e.Time });
            entity.HasIndex(e => new { e.ActorType, e.ActorId });
            entity.HasIndex(e => e.Verb);
            entity.HasIndex(e => new { e.TargetId, e.TargetType });
        });

        // Sequence entity configuration
        modelBuilder.Entity<Sequence>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // PerformanceLog entity configuration
        modelBuilder.Entity<PerformanceLog>(entity =>
        {
            entity.HasIndex(e => e.OperationName);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => e.DurationMs);
        });
    }
}
