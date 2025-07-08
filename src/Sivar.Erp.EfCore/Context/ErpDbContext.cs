using Microsoft.EntityFrameworkCore;
using Sivar.Erp.EfCore.Entities.Core;
using Sivar.Erp.EfCore.Entities.Accounting;
using Sivar.Erp.EfCore.Entities.Tax;
using Sivar.Erp.EfCore.Entities.System;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.Services.Taxes.TaxGroup;
using Sivar.Erp.Services.Taxes.TaxRule;
using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.Modules.Payments.Models;
using Sivar.Erp.Modules.Inventory;

namespace Sivar.Erp.EfCore.Context
{
    public class ErpDbContext : DbContext
    {
        public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options)
        {
        }

        // Core entities
        public DbSet<Account> Accounts { get; set; }
        public DbSet<BusinessEntity> BusinessEntities { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<Item> Items { get; set; }

        // Accounting entities
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<LedgerEntry> LedgerEntries { get; set; }
        public DbSet<FiscalPeriod> FiscalPeriods { get; set; }

        // Tax entities
        public DbSet<Entities.Tax.Tax> Taxes { get; set; }
        public DbSet<TaxGroup> TaxGroups { get; set; }

        // System entities
        public DbSet<ActivityRecord> ActivityRecords { get; set; }
        public DbSet<PerformanceLog> PerformanceLogs { get; set; }

        // Legacy entities (mapped to DTOs)
        public DbSet<SequenceDto> Sequences { get; set; }
        public DbSet<GroupMembershipDto> GroupMemberships { get; set; }
        public DbSet<TaxRuleDto> TaxRules { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<SecurityEvent> SecurityEvents { get; set; }
        public DbSet<PaymentMethodDto> PaymentMethods { get; set; }
        public DbSet<PaymentDto> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureAccountingEntities(modelBuilder);
            ConfigureTaxEntities(modelBuilder);
            ConfigureSystemEntities(modelBuilder);
            ConfigureLegacyEntities(modelBuilder);
        }

        private void ConfigureAccountingEntities(ModelBuilder modelBuilder)
        {
            // Account configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.Property(e => e.OfficialCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AccountName).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.OfficialCode).IsUnique();
            });

            // Transaction configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.Property(e => e.TransactionNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.TransactionNumber).IsUnique();

                // Configure relationship with LedgerEntries
                entity.HasMany(t => t.LedgerEntries)
                      .WithOne(le => le.Transaction)
                      .HasForeignKey(le => le.TransactionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // LedgerEntry configuration
            modelBuilder.Entity<LedgerEntry>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.Property(e => e.LedgerEntryNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.LedgerEntryNumber).IsUnique();
            });

            // FiscalPeriod configuration
            modelBuilder.Entity<FiscalPeriod>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Code).IsUnique();
            });
        }

        private void ConfigureTaxEntities(ModelBuilder modelBuilder)
        {
            // Tax configuration
            modelBuilder.Entity<Entities.Tax.Tax>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Percentage).HasColumnType("decimal(5,4)");
                entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // TaxGroup configuration
            modelBuilder.Entity<TaxGroup>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Code).IsUnique();
            });
        }

        private void ConfigureSystemEntities(ModelBuilder modelBuilder)
        {
            // ActivityRecord configuration
            modelBuilder.Entity<ActivityRecord>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.Property(e => e.Verb).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Actor).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Target).IsRequired().HasMaxLength(100);
            });

            // PerformanceLog configuration
            modelBuilder.Entity<PerformanceLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Method).IsRequired().HasMaxLength(200);
            });
        }

        private void ConfigureLegacyEntities(ModelBuilder modelBuilder)
        {
            // Configure DTOs that don't have entity counterparts yet
            modelBuilder.Entity<SequenceDto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Sequences");
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            modelBuilder.Entity<GroupMembershipDto>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.ToTable("GroupMemberships");
                entity.Property(e => e.GroupId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<TaxRuleDto>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.ToTable("TaxRules");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Users");
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Username).IsUnique();

                // Configure Properties as JSON (for SQL Server, SQLite, PostgreSQL)
                entity.Property(e => e.Properties)
                      .HasConversion(
                          v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                          v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                      .HasColumnType("TEXT"); // Use TEXT for JSON storage

                // Configure Collections as JSON
                entity.Property(e => e.Roles)
                      .HasConversion(
                          v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                          v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                      .HasColumnType("TEXT");

                entity.Property(e => e.DirectPermissions)
                      .HasConversion(
                          v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                          v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                      .HasColumnType("TEXT");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Name);
                entity.ToTable("Roles");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<SecurityEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("SecurityEvents");
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);

                // Configure AdditionalData as JSON (for SQL Server, SQLite, PostgreSQL)
                // or ignore it if you don't want to store it in the database
                entity.Property(e => e.AdditionalData)
                      .HasConversion(
                          v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                          v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                      .HasColumnType("TEXT"); // Use TEXT for JSON storage

                // Alternative: If you don't want to store AdditionalData, uncomment the line below and comment out the property configuration above
                // entity.Ignore(e => e.AdditionalData);
            });

            modelBuilder.Entity<PaymentMethodDto>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.ToTable("PaymentMethods");
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

                // Configure AdditionalProperties as JSON (for SQL Server, SQLite, PostgreSQL)
                entity.Property(e => e.AdditionalProperties)
                      .HasConversion(
                          v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                          v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
                      .HasColumnType("TEXT"); // Use TEXT for JSON storage
            });

            modelBuilder.Entity<PaymentDto>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.ToTable("Payments");

                // Configure AdditionalData as JSON (for SQL Server, SQLite, PostgreSQL)
                entity.Property(e => e.AdditionalData)
                      .HasConversion(
                          v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                          v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
                      .HasColumnType("TEXT"); // Use TEXT for JSON storage
            });
        }
    }
}
