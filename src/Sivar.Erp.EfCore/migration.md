# Entity Framework Core Migration Guide

## Overview

This guide provides step-by-step instructions for implementing Entity Framework Core as the data backend for the Sivar ERP system while maintaining full backward compatibility and ensuring the `CompleteAccountingWorkflowTest` continues to pass without modifications.

## Prerequisites

- Visual Studio 2022 with .NET 9 SDK
- Understanding of Entity Framework Core concepts
- Access to the existing Sivar.Erp solution
- SQL Server or SQLite for database (SQLite recommended for testing)

## Phase 1: Project Setup and Foundation

### Step 1.1: Verify Project Structure

Ensure you have the following project structure:
```
Solution/
├── Sivar.Erp/                    (Core project - .NET 9)
├── Sivar.Erp.EfCore/             (New EF Core project - .NET 9)
├── Tests/                        (Test project - .NET 9)
```

### Step 1.2: Update NuGet Packages

Add the following packages to `Sivar.Erp.EfCore.csproj`:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
```

## Phase 2: Create Entity Models

### Step 2.1: Create Entities Directory Structure

Create the following directory structure in `Sivar.Erp.EfCore`:
```
Sivar.Erp.EfCore/
├── Entities/
│   ├── Core/
│   ├── Accounting/
│   ├── Tax/
│   ├── Inventory/
│   ├── Security/
│   └── System/
├── Context/
├── Repositories/
├── UnitOfWork/
├── Adapters/
├── Extensions/
└── Configuration/
```

### Step 2.2: Core Entity Models

Create `Entities/Core/Account.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;

namespace Sivar.Erp.EfCore.Entities.Core
{
    [Table("Accounts")]
    public class Account : IAccount
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(20)]
        public string AccountCode { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string OfficialCode { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string AccountName { get; set; } = string.Empty;
        
        public AccountType AccountType { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [MaxLength(20)]
        public string? ParentAccountCode { get; set; }
        
        public int Level { get; set; }
        
        public bool IsDebitNormal { get; set; }
        
        public decimal Balance { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
```

Create `Entities/Core/BusinessEntity.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.BusinessEntities;

namespace Sivar.Erp.EfCore.Entities.Core
{
    [Table("BusinessEntities")]
    public class BusinessEntity : IBusinessEntity
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        public BusinessEntityType EntityType { get; set; }
        
        [MaxLength(50)]
        public string? TaxId { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(100)]
        public string? Email { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
```

Create `Entities/Core/DocumentType.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Documents;

namespace Sivar.Erp.EfCore.Entities.Core
{
    [Table("DocumentTypes")]
    public class DocumentType : IDocumentType
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
```

Create `Entities/Core/Item.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Documents;

namespace Sivar.Erp.EfCore.Entities.Core
{
    [Table("Items")]
    public class Item : IItem
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }
        
        [MaxLength(20)]
        public string? Unit { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
```

### Step 2.3: Accounting Entity Models

Create `Entities/Accounting/Transaction.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Services.Accounting.Transactions;

namespace Sivar.Erp.EfCore.Entities.Accounting
{
    [Table("Transactions")]
    public class Transaction : ITransaction
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;
        
        public DateOnly Date { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string DocumentNumber { get; set; } = string.Empty;
        
        public bool IsPosted { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        
        public DateTime? PostedDate { get; set; }
        
        [MaxLength(100)]
        public string? PostedBy { get; set; }
        
        // Navigation properties
        public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
        
        // Interface implementation
        IList<ILedgerEntry> ITransaction.LedgerEntries 
        { 
            get => LedgerEntries.Cast<ILedgerEntry>().ToList();
            set => LedgerEntries = value.Cast<LedgerEntry>().ToList();
        }
    }
}
```

Create `Entities/Accounting/LedgerEntry.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Core.Enums;

namespace Sivar.Erp.EfCore.Entities.Accounting
{
    [Table("LedgerEntries")]
    public class LedgerEntry : ILedgerEntry
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(50)]
        public string LedgerEntryNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string OfficialCode { get; set; } = string.Empty;
        
        public EntryType EntryType { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        
        // Foreign Key
        public Guid TransactionId { get; set; }
        
        // Navigation property
        [ForeignKey(nameof(TransactionId))]
        public virtual Transaction Transaction { get; set; } = null!;
    }
}
```

Create `Entities/Accounting/FiscalPeriod.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Services.Accounting.FiscalPeriods;

namespace Sivar.Erp.EfCore.Entities.Accounting
{
    [Table("FiscalPeriods")]
    public class FiscalPeriod : IFiscalPeriod
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public DateOnly StartDate { get; set; }
        
        public DateOnly EndDate { get; set; }
        
        public FiscalPeriodStatus Status { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
```

### Step 2.4: Tax Entity Models

Create `Entities/Tax/Tax.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Services.Taxes;

namespace Sivar.Erp.EfCore.Entities.Tax
{
    [Table("Taxes")]
    public class Tax : ITax
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string TaxCode { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string TaxName { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(5,4)")]
        public decimal Rate { get; set; }
        
        public TaxType TaxType { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
```

Create `Entities/Tax/TaxGroup.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Services.Taxes.TaxGroup;

namespace Sivar.Erp.EfCore.Entities.Tax
{
    [Table("TaxGroups")]
    public class TaxGroup : ITaxGroup
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
```

### Step 2.5: System Entity Models

Create `Entities/System/ActivityRecord.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.ErpSystem.ActivityStream;

namespace Sivar.Erp.EfCore.Entities.System
{
    [Table("ActivityRecords")]
    public class ActivityRecord
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(100)]
        public string Actor { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Verb { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Target { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? TimeZoneId { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [MaxLength(1000)]
        public string? SerializedActivity { get; set; }
    }
}
```

Create `Entities/System/PerformanceLog.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.ErpSystem.Diagnostics;

namespace Sivar.Erp.EfCore.Entities.System
{
    [Table("PerformanceLogs")]
    public class PerformanceLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(200)]
        public string Method { get; set; } = string.Empty;
        
        public long ExecutionTimeMs { get; set; }
        
        public long MemoryDeltaBytes { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? UserName { get; set; }
        
        [MaxLength(100)]
        public string? InstanceId { get; set; }
        
        [MaxLength(200)]
        public string? Context { get; set; }
        
        public bool IsSlow { get; set; }
        
        public bool IsMemoryIntensive { get; set; }
    }
}
```

## Phase 3: Create DbContext

### Step 3.1: Create ErpDbContext

Create `Context/ErpDbContext.cs`:

```csharp
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
        public DbSet<Tax> Taxes { get; set; }
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
                entity.Property(e => e.AccountCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.OfficialCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AccountName).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.AccountCode).IsUnique();
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
            });
        }

        private void ConfigureTaxEntities(ModelBuilder modelBuilder)
        {
            // Tax configuration
            modelBuilder.Entity<Tax>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TaxCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Rate).HasColumnType("decimal(5,4)");
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.TaxCode).IsUnique();
            });

            // TaxGroup configuration
            modelBuilder.Entity<TaxGroup>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
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
                entity.HasKey(e => e.Oid);
                entity.ToTable("Sequences");
            });

            modelBuilder.Entity<GroupMembershipDto>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.ToTable("GroupMemberships");
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
                entity.HasIndex(e => e.Username).IsUnique();
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Roles");
            });

            modelBuilder.Entity<SecurityEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("SecurityEvents");
            });

            modelBuilder.Entity<PaymentMethodDto>(entity =>
            {
                entity.HasKey(e => e.Oid);
                entity.ToTable("PaymentMethods");
            });

            modelBuilder.Entity<PaymentDto>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.ToTable("Payments");
            });
        }
    }
}
```

## Phase 4: Repository and Unit of Work Pattern

### Step 4.1: Create Repository Interface

Create `Repositories/IRepository.cs`:

```csharp
using System.Linq.Expressions;

namespace Sivar.Erp.EfCore.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> CountAsync();
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        IQueryable<T> Query();
    }
}
```

### Step 4.2: Create Repository Implementation

Create `Repositories/Repository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Sivar.Erp.EfCore.Context;
using System.Linq.Expressions;

namespace Sivar.Erp.EfCore.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ErpDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ErpDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }
    }
}
```

### Step 4.3: Create Unit of Work Interface

Create `UnitOfWork/IUnitOfWork.cs`:

```csharp
using Sivar.Erp.EfCore.Repositories;

namespace Sivar.Erp.EfCore.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
```

### Step 4.4: Create Unit of Work Implementation

Create `UnitOfWork/UnitOfWork.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sivar.Erp.EfCore.Context;
using Sivar.Erp.EfCore.Repositories;

namespace Sivar.Erp.EfCore.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ErpDbContext _context;
        private readonly Dictionary<Type, object> _repositories;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ErpDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return (IRepository<T>)_repositories[typeof(T)];
            }

            var repository = new Repository<T>(_context);
            _repositories.Add(typeof(T), repository);
            return repository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
```

## Phase 5: Backward Compatibility Adapter

### Step 5.1: Create IObjectDb Adapter

Create `Adapters/DbContextObjectDbAdapter.cs`:

```csharp
using Sivar.Erp.Services;
using Sivar.Erp.EfCore.Context;
using Sivar.Erp.EfCore.UnitOfWork;
using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Services.Taxes;
using Sivar.Erp.Services.Taxes.TaxGroup;
using Sivar.Erp.Services.Taxes.TaxRule;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.Modules.Payments.Models;
using Sivar.Erp.Modules.Inventory;

namespace Sivar.Erp.EfCore.Adapters
{
    /// <summary>
    /// Adapter that makes DbContext implement IObjectDb interface for backward compatibility
    /// </summary>
    public class DbContextObjectDbAdapter : IObjectDb
    {
        private readonly ErpDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;

        public DbContextObjectDbAdapter(ErpDbContext dbContext, IUnitOfWork unitOfWork)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
        }

        // Core entities
        public IList<IAccount> Accounts => 
            new EntityFrameworkCollection<IAccount>(_dbContext.Accounts.Cast<IAccount>().AsQueryable());

        public IList<IBusinessEntity> BusinessEntities => 
            new EntityFrameworkCollection<IBusinessEntity>(_dbContext.BusinessEntities.Cast<IBusinessEntity>().AsQueryable());

        public IList<IDocumentType> DocumentTypes => 
            new EntityFrameworkCollection<IDocumentType>(_dbContext.DocumentTypes.Cast<IDocumentType>().AsQueryable());

        public IList<IItem> Items => 
            new EntityFrameworkCollection<IItem>(_dbContext.Items.Cast<IItem>().AsQueryable());

        // Accounting entities
        public IList<IFiscalPeriod> fiscalPeriods => 
            new EntityFrameworkCollection<IFiscalPeriod>(_dbContext.FiscalPeriods.Cast<IFiscalPeriod>().AsQueryable());

        public IList<ITransaction> Transactions => 
            new EntityFrameworkCollection<ITransaction>(_dbContext.Transactions.Cast<ITransaction>().AsQueryable());

        public IList<ILedgerEntry> LedgerEntries => 
            new EntityFrameworkCollection<ILedgerEntry>(_dbContext.LedgerEntries.Cast<ILedgerEntry>().AsQueryable());

        public IList<ITransactionBatch> TransactionBatches { get; set; } = new List<ITransactionBatch>();

        public IList<IDocumentAccountingProfile> DocumentAccountingProfiles { get; set; } = new List<IDocumentAccountingProfile>();

        // Tax entities
        public IList<ITax> Taxes => 
            new EntityFrameworkCollection<ITax>(_dbContext.Taxes.Cast<ITax>().AsQueryable());

        public IList<ITaxGroup> TaxGroups => 
            new EntityFrameworkCollection<ITaxGroup>(_dbContext.TaxGroups.Cast<ITaxGroup>().AsQueryable());

        public IList<ITaxRule> TaxRules => 
            new EntityFrameworkCollection<ITaxRule>(_dbContext.TaxRules.Cast<ITaxRule>().AsQueryable());

        // System entities
        public IList<ActivityRecord> ActivityRecords => 
            new EntityFrameworkCollection<ActivityRecord>(_dbContext.ActivityRecords.AsQueryable());

        public IList<PerformanceLog> PerformanceLogs => 
            new EntityFrameworkCollection<PerformanceLog>(_dbContext.PerformanceLogs.AsQueryable());

        public IList<SequenceDto> Sequences => 
            new EntityFrameworkCollection<SequenceDto>(_dbContext.Sequences.AsQueryable());

        public IList<GroupMembershipDto> GroupMemberships => 
            new EntityFrameworkCollection<GroupMembershipDto>(_dbContext.GroupMemberships.AsQueryable());

        // Security entities
        public IList<User> Users => 
            new EntityFrameworkCollection<User>(_dbContext.Users.AsQueryable());

        public IList<Role> Roles => 
            new EntityFrameworkCollection<Role>(_dbContext.Roles.AsQueryable());

        public IList<SecurityEvent> SecurityEvents => 
            new EntityFrameworkCollection<SecurityEvent>(_dbContext.SecurityEvents.AsQueryable());

        // Payment entities
        public IList<PaymentMethodDto> PaymentMethods => 
            new EntityFrameworkCollection<PaymentMethodDto>(_dbContext.PaymentMethods.AsQueryable());

        public IList<PaymentDto> Payments => 
            new EntityFrameworkCollection<PaymentDto>(_dbContext.Payments.AsQueryable());

        // Inventory entities (placeholder implementations)
        public IList<IInventoryItem> InventoryItems { get; set; } = new List<IInventoryItem>();
        public IList<IStockLevel> StockLevels { get; set; } = new List<IStockLevel>();
        public IList<IInventoryTransaction> InventoryTransactions { get; set; } = new List<IInventoryTransaction>();
        public IList<IInventoryReservation> InventoryReservations { get; set; } = new List<IInventoryReservation>();
        public IList<InventoryLayerDto> InventoryLayers { get; set; } = new List<InventoryLayerDto>();
    }

    /// <summary>
    /// Helper class to make IQueryable behave like IList for backward compatibility
    /// </summary>
    public class EntityFrameworkCollection<T> : IList<T>
    {
        private readonly IQueryable<T> _queryable;
        private List<T>? _cachedList;

        public EntityFrameworkCollection(IQueryable<T> queryable)
        {
            _queryable = queryable;
        }

        private List<T> GetList()
        {
            return _cachedList ??= _queryable.ToList();
        }

        public T this[int index] 
        { 
            get => GetList()[index]; 
            set => GetList()[index] = value; 
        }

        public int Count => GetList().Count;
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            GetList().Add(item);
            _cachedList = null; // Invalidate cache
        }

        public void Clear()
        {
            GetList().Clear();
            _cachedList = null;
        }

        public bool Contains(T item) => GetList().Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => GetList().CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => GetList().GetEnumerator();
        public int IndexOf(T item) => GetList().IndexOf(item);
        public void Insert(int index, T item) => GetList().Insert(index, item);
        public bool Remove(T item) => GetList().Remove(item);
        public void RemoveAt(int index) => GetList().RemoveAt(index);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
```

## Phase 6: Service Collection Extensions

### Step 6.1: Create EF Core Service Extensions

Create `Extensions/ServiceCollectionExtensions.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Sivar.Erp.EfCore.Context;
using Sivar.Erp.EfCore.UnitOfWork;
using Sivar.Erp.EfCore.Repositories;
using Sivar.Erp.EfCore.Adapters;
using Sivar.Erp.Services;

namespace Sivar.Erp.EfCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds ERP modules with Entity Framework Core support while maintaining backward compatibility
        /// </summary>
        public static IServiceCollection AddErpModulesWithEfCore(
            this IServiceCollection services, 
            Action<DbContextOptionsBuilder> configureDbContext)
        {
            // Add DbContext
            services.AddDbContext<ErpDbContext>(configureDbContext);
            
            // Add Repository Pattern
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            
            // Add backward compatibility adapter - THIS IS CRITICAL
            services.AddScoped<IObjectDb, DbContextObjectDbAdapter>();
            
            return services;
        }

        /// <summary>
        /// Adds ERP modules with SQLite for testing (recommended for CompleteAccountingWorkflowTest)
        /// </summary>
        public static IServiceCollection AddErpModulesWithSqlite(
            this IServiceCollection services,
            string connectionString = "Data Source=:memory:")
        {
            return services.AddErpModulesWithEfCore(options =>
            {
                options.UseSqlite(connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
        }

        /// <summary>
        /// Adds ERP modules with SQL Server
        /// </summary>
        public static IServiceCollection AddErpModulesWithSqlServer(
            this IServiceCollection services,
            string connectionString)
        {
            return services.AddErpModulesWithEfCore(options =>
            {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
        }

        /// <summary>
        /// Adds ERP modules with In-Memory database (for unit testing)
        /// </summary>
        public static IServiceCollection AddErpModulesWithInMemoryDatabase(
            this IServiceCollection services,
            string databaseName = "TestErpDatabase")
        {
            return services.AddErpModulesWithEfCore(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
        }
    }
}
```

## Phase 7: Modify CompleteAccountingWorkflowTest

### Step 7.1: Update Test to Use EF Core

Create a modified version of the test setup method. The key is to replace the ObjectDb registration with EF Core while keeping everything else identical:

In your test `Setup()` method, replace:

```csharp
// OLD CODE:
_objectDb = new ObjectDb();
_serviceProvider = AccountingTestServiceFactory.CreateServiceProvider(services =>
{
    services.AddSingleton<IObjectDb>(_objectDb);
    // ... rest of configuration
});
```

With:

```csharp
// NEW CODE:
_serviceProvider = AccountingTestServiceFactory.CreateServiceProvider(services =>
{
    // Add EF Core with In-Memory database for testing
    services.AddErpModulesWithInMemoryDatabase("CompleteAccountingWorkflowTestDb");
    
    // The IObjectDb will be automatically provided by DbContextObjectDbAdapter
    // Remove the manual ObjectDb registration
    
    // Add ERP Security Module
    services.AddErpSecurityModule();

    // Configure test user context
    services.AddSingleton<GenericSecurityContext>(provider =>
    {
        var context = new GenericSecurityContext();
        var testUser = new User
        {
            Id = "test-user-id",
            Username = "TestUser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Roles = new List<string> { Roles.SYSTEM_ADMINISTRATOR },
            DirectPermissions = new List<string>(),
            IsActive = true
        };
        context.SetCurrentUser(testUser);
        return context;
    });

    // Register performance context provider for testing
    services.AddSingleton<IPerformanceContextProvider>(provider =>
    {
        var securityContext = provider.GetService<GenericSecurityContext>();
        return new DefaultPerformanceContextProvider(
            userId: securityContext?.UserId ?? "test-user-id",
            userName: securityContext?.UserName ?? "TestUser",
            sessionId: "test-session-12345",
            context: "CompleteAccountingWorkflowTest"
        );
    });
});

// Get the IObjectDb from the service provider (it will be the adapter)
_objectDb = _serviceProvider.GetRequiredService<IObjectDb>();

// Initialize the database
await InitializeDatabaseAsync();
```

### Step 7.2: Add Database Initialization Method

Add this method to your test class:

```csharp
private async Task InitializeDatabaseAsync()
{
    // Get the DbContext and ensure database is created
    var dbContext = _serviceProvider.GetRequiredService<ErpDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    
    // The adapter will handle the rest automatically
}
```

### Step 7.3: Update TearDown Method

Update the TearDown method to properly dispose of the database:

```csharp
[TearDown]
public async Task TearDown()
{
    // Clean up database
    if (_serviceProvider != null)
    {
        var dbContext = _serviceProvider.GetService<ErpDbContext>();
        if (dbContext != null)
        {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.DisposeAsync();
        }
    }
    
    // Dispose service provider if it implements IDisposable
    if (_serviceProvider is IDisposable disposableProvider)
    {
        disposableProvider.Dispose();
    }
}
```

## Phase 8: Testing and Validation

### Step 8.1: Run the Test

1. Build the solution to ensure all dependencies are resolved
2. Run the `CompleteAccountingWorkflowTest.ExecuteCompleteWorkflowTest()` method
3. Verify that all assertions pass and no exceptions are thrown

### Step 8.2: Verify Data Persistence

Add a verification method to ensure data is being persisted correctly:

```csharp
[Test]
public async Task VerifyDataPersistenceWithEfCore()
{
    // Setup (same as main test)
    await SetupDataAndServices();
    
    // Get DbContext directly
    var dbContext = _serviceProvider.GetRequiredService<ErpDbContext>();
    
    // Verify data exists in database
    var accountCount = await dbContext.Accounts.CountAsync();
    var transactionCount = await dbContext.Transactions.CountAsync();
    
    Assert.That(accountCount, Is.GreaterThan(0), "Accounts should be persisted");
    Console.WriteLine($"✓ Found {accountCount} accounts in database");
    Console.WriteLine($"✓ Found {transactionCount} transactions in database");
}
```

## Phase 9: Performance Monitoring

### Step 9.1: Add EF Core Performance Logging

Update the DbContext to include query logging:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        // Fallback configuration
        optionsBuilder.UseInMemoryDatabase("DefaultErpDb");
    }
    
    // Enable query logging for performance monitoring
    optionsBuilder.LogTo(
        message => System.Diagnostics.Debug.WriteLine(message),
        Microsoft.Extensions.Logging.LogLevel.Information);
}
```

## Phase 10: Troubleshooting Guide

### Common Issues and Solutions:

1. **"Entity type not found" errors**:
   - Ensure all entity classes implement the correct interfaces
   - Verify DbSet properties are correctly named
   - Check that all entities are configured in OnModelCreating

2. **"Cannot insert explicit value for identity column" errors**:
   - Review primary key configurations
   - Ensure Guid properties use `Guid.NewGuid()` default values

3. **Performance issues**:
   - Enable query logging to identify slow queries
   - Consider adding appropriate database indexes
   - Use async methods consistently

4. **Test failures**:
   - Verify that the adapter correctly maps between interfaces and entities
   - Check that all required data is being seeded
   - Ensure proper transaction handling

## Success Criteria

✅ The `CompleteAccountingWorkflowTest` passes without any code changes to the test itself
✅ All existing functionality works exactly as before
✅ Data is properly persisted to the database
✅ Performance is acceptable (no significant degradation)
✅ No breaking changes to existing interfaces

## Next Steps

After successful implementation:

1. **Gradual Migration**: Start creating new modules that use DbContext directly
2. **Performance Optimization**: Add indexes and optimize queries
3. **Migration Tools**: Build tools to migrate from in-memory ObjectDb to database
4. **Feature Flags**: Implement feature flags to switch between implementations

This migration provides a solid foundation for moving from in-memory collections to a proper database while maintaining complete backward compatibility.