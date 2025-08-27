using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sivar.Erp.Core.Domain.Entities.Documents;

namespace Sivar.Erp.Core.Infrastructure.Data.Configurations.Documents
{
    /// <summary>
    /// Entity Framework configuration for Document entity
    /// </summary>
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            // Primary key
            builder.HasKey(x => x.Oid);

            // Table name
            builder.ToTable("Documents", "docs");

            // Properties
            builder.Property(x => x.DocumentNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.DocumentDate)
                .IsRequired();

            builder.Property(x => x.DocumentTime)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CurrencyCode)
                .HasMaxLength(3)
                .IsRequired()
                .HasDefaultValue("USD");

            builder.Property(x => x.ExchangeRate)
                .HasPrecision(18, 8)
                .HasDefaultValue(1.0m);

            builder.Property(x => x.Remarks)
                .HasMaxLength(1000);

            // Financial amounts with precision
            builder.Property(x => x.SubTotal)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalDiscount)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalTax)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            // Computed columns
            builder.Property(x => x.LineCount)
                .HasComputedColumnSql("(SELECT COUNT(*) FROM docs.DocumentLines WHERE DocumentId = Oid)", stored: false);

            // Indexes
            builder.HasIndex(x => x.DocumentNumber)
                .IsUnique()
                .HasDatabaseName("IX_Documents_DocumentNumber");

            builder.HasIndex(x => new { x.CompanyId, x.DocumentDate })
                .HasDatabaseName("IX_Documents_Company_Date");

            builder.HasIndex(x => new { x.DocumentTypeId, x.Status })
                .HasDatabaseName("IX_Documents_Type_Status");

            builder.HasIndex(x => x.BusinessEntityId)
                .HasDatabaseName("IX_Documents_BusinessEntity");

            builder.HasIndex(x => x.IsPosted)
                .HasDatabaseName("IX_Documents_Posted");

            // Global query filter for soft delete and tenant isolation
            builder.HasQueryFilter(x => !x.IsDeleted);

            // Relationships
            builder.HasOne<DocumentType>()
                .WithMany()
                .HasForeignKey(x => x.DocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Documents_DocumentType");

            builder.HasOne<BusinessEntity>()
                .WithMany()
                .HasForeignKey(x => x.BusinessEntityId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Documents_BusinessEntity");

            builder.HasMany(x => x.Lines)
                .WithOne(x => x.Document)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DocumentLines_Document");

            builder.HasMany(x => x.Totals)
                .WithOne(x => x.Document)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DocumentTotals_Document");

            // Base entity configuration
            ConfigureBaseEntity(builder);
        }

        private void ConfigureBaseEntity<T>(EntityTypeBuilder<T> builder) where T : BaseEntity
        {
            // Tenant fields
            builder.Property(x => x.CompanyId)
                .IsRequired();

            builder.Property(x => x.BranchId)
                .IsRequired();

            // Audit fields
            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(100);

            builder.Property(x => x.ModifiedAt);

            // Soft delete
            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(x => x.DeletedBy)
                .HasMaxLength(100);

            builder.Property(x => x.DeletedAt);

            // Version tracking
            builder.Property(x => x.Version)
                .HasDefaultValue(1);

            // Row version for optimistic concurrency
            builder.Property(x => x.RowVersion)
                .IsRowVersion();

            // Indexes for audit and tenant isolation
            builder.HasIndex(x => new { x.CompanyId, x.BranchId })
                .HasDatabaseName($"IX_{typeof(T).Name}_Tenant");

            builder.HasIndex(x => x.CreatedAt)
                .HasDatabaseName($"IX_{typeof(T).Name}_CreatedAt");

            builder.HasIndex(x => x.IsDeleted)
                .HasDatabaseName($"IX_{typeof(T).Name}_IsDeleted");
        }
    }

    /// <summary>
    /// Entity Framework configuration for DocumentLine entity
    /// </summary>
    public class DocumentLineConfiguration : IEntityTypeConfiguration<DocumentLine>
    {
        public void Configure(EntityTypeBuilder<DocumentLine> builder)
        {
            // Primary key
            builder.HasKey(x => x.Oid);

            // Table name
            builder.ToTable("DocumentLines", "docs");

            // Properties
            builder.Property(x => x.LineNumber)
                .IsRequired();

            builder.Property(x => x.ItemCode)
                .HasMaxLength(50);

            builder.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .HasPrecision(18, 4)
                .IsRequired();

            builder.Property(x => x.UnitOfMeasure)
                .HasMaxLength(10);

            builder.Property(x => x.UnitPrice)
                .HasPrecision(18, 4)
                .IsRequired();

            builder.Property(x => x.DiscountPercent)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            builder.Property(x => x.DiscountAmount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(x => x.TaxPercent)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            builder.Property(x => x.TaxAmount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(x => x.LineTotal)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.CurrencyCode)
                .HasMaxLength(3)
                .IsRequired()
                .HasDefaultValue("USD");

            builder.Property(x => x.ExchangeRate)
                .HasPrecision(18, 8)
                .HasDefaultValue(1.0m);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(x => new { x.DocumentId, x.LineNumber })
                .IsUnique()
                .HasDatabaseName("IX_DocumentLines_Document_LineNumber");

            builder.HasIndex(x => x.ItemId)
                .HasDatabaseName("IX_DocumentLines_Item");

            builder.HasIndex(x => x.ItemCode)
                .HasDatabaseName("IX_DocumentLines_ItemCode");

            // Global query filter
            builder.HasQueryFilter(x => !x.IsDeleted);

            // Relationships
            builder.HasOne(x => x.Document)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DocumentLines_Document");

            builder.HasOne<Item>()
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DocumentLines_Item");

            // Base entity configuration (simplified version)
            ConfigureBaseEntitySimple(builder);
        }

        private void ConfigureBaseEntitySimple<T>(EntityTypeBuilder<T> builder) where T : BaseEntity
        {
            builder.Property(x => x.CompanyId).IsRequired();
            builder.Property(x => x.BranchId).IsRequired();
            builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.ModifiedBy).HasMaxLength(100);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.DeletedBy).HasMaxLength(100);
            builder.Property(x => x.Version).HasDefaultValue(1);
            builder.Property(x => x.RowVersion).IsRowVersion();
        }
    }

    /// <summary>
    /// Entity Framework configuration for DocumentTotal entity
    /// </summary>
    public class DocumentTotalConfiguration : IEntityTypeConfiguration<DocumentTotal>
    {
        public void Configure(EntityTypeBuilder<DocumentTotal> builder)
        {
            // Primary key
            builder.HasKey(x => x.Oid);

            // Table name
            builder.ToTable("DocumentTotals", "docs");

            // Properties
            builder.Property(x => x.TotalType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Rate)
                .HasPrecision(5, 2);

            builder.Property(x => x.BaseAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.CurrencyCode)
                .HasMaxLength(3)
                .IsRequired()
                .HasDefaultValue("USD");

            builder.Property(x => x.ExchangeRate)
                .HasPrecision(18, 8)
                .HasDefaultValue(1.0m);

            // Indexes
            builder.HasIndex(x => new { x.DocumentId, x.TotalType })
                .HasDatabaseName("IX_DocumentTotals_Document_Type");

            // Global query filter
            builder.HasQueryFilter(x => !x.IsDeleted);

            // Relationships
            builder.HasOne(x => x.Document)
                .WithMany(x => x.Totals)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DocumentTotals_Document");

            // Base entity configuration
            ConfigureBaseEntitySimple(builder);
        }

        private void ConfigureBaseEntitySimple<T>(EntityTypeBuilder<T> builder) where T : BaseEntity
        {
            builder.Property(x => x.CompanyId).IsRequired();
            builder.Property(x => x.BranchId).IsRequired();
            builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.ModifiedBy).HasMaxLength(100);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.DeletedBy).HasMaxLength(100);
            builder.Property(x => x.Version).HasDefaultValue(1);
            builder.Property(x => x.RowVersion).IsRowVersion();
        }
    }

    /// <summary>
    /// Entity Framework configuration for DocumentType entity
    /// </summary>
    public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
    {
        public void Configure(EntityTypeBuilder<DocumentType> builder)
        {
            // Primary key
            builder.HasKey(x => x.Oid);

            // Table name
            builder.ToTable("DocumentTypes", "docs");

            // Properties
            builder.Property(x => x.Code)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.Category)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.NumberPrefix)
                .HasMaxLength(10);

            builder.Property(x => x.NumberSuffix)
                .HasMaxLength(10);

            builder.Property(x => x.NextNumber)
                .HasDefaultValue(1);

            builder.Property(x => x.NumberFormat)
                .HasMaxLength(50)
                .HasDefaultValue("{Prefix}{Number:D6}{Suffix}");

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.AutoGenerateNumbers)
                .HasDefaultValue(true);

            builder.Property(x => x.RequiresApproval)
                .HasDefaultValue(false);

            builder.Property(x => x.RequiresLines)
                .HasDefaultValue(true);

            builder.Property(x => x.RequiresBusinessEntity)
                .HasDefaultValue(false);

            builder.Property(x => x.AllowZeroAmount)
                .HasDefaultValue(false);

            builder.Property(x => x.DefaultCurrencyCode)
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            // Indexes
            builder.HasIndex(x => x.Code)
                .IsUnique()
                .HasDatabaseName("IX_DocumentTypes_Code");

            builder.HasIndex(x => x.Name)
                .HasDatabaseName("IX_DocumentTypes_Name");

            builder.HasIndex(x => new { x.Category, x.IsActive })
                .HasDatabaseName("IX_DocumentTypes_Category_Active");

            // Global query filter
            builder.HasQueryFilter(x => !x.IsDeleted);

            // Business key configuration
            builder.Property(x => x.Code)
                .HasAnnotation("BusinessKey", true);

            // Base entity configuration
            ConfigureBaseEntitySimple(builder);
        }

        private void ConfigureBaseEntitySimple<T>(EntityTypeBuilder<T> builder) where T : BaseEntity
        {
            builder.Property(x => x.CompanyId).IsRequired();
            builder.Property(x => x.BranchId).IsRequired();
            builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.ModifiedBy).HasMaxLength(100);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.DeletedBy).HasMaxLength(100);
            builder.Property(x => x.Version).HasDefaultValue(1);
            builder.Property(x => x.RowVersion).IsRowVersion();
        }
    }
}
