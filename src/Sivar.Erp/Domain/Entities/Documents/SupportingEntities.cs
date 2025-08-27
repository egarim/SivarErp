using Sivar.Erp.Core.Domain.Attributes;
using Sivar.Erp.Core.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.Core.Domain.Entities.Documents
{
    /// <summary>
    /// Business entity (customer, vendor, employee, etc.)
    /// </summary>
    [Table("BusinessEntities")]
    public class BusinessEntity : BaseEntity
    {
        /// <summary>
        /// Business entity code - business key
        /// </summary>
        [BusinessKey("Business Entity Code", BusinessKeyScope.Company)]
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Business entity name
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Business entity type (Customer, Vendor, Employee, etc.)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Tax identification number
        /// </summary>
        [MaxLength(50)]
        public string? TaxId { get; set; }

        /// <summary>
        /// Default currency
        /// </summary>
        [MaxLength(3)]
        public string? DefaultCurrency { get; set; }

        /// <summary>
        /// Default payment terms (days)
        /// </summary>
        public int? DefaultPaymentTerms { get; set; }

        /// <summary>
        /// Credit limit
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? CreditLimit { get; set; }

        /// <summary>
        /// Navigation property to documents
        /// </summary>
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

        public override string ToString()
        {
            return $"{Code} - {Name}";
        }
    }

    /// <summary>
    /// Item/product/service entity
    /// </summary>
    [Table("Items")]
    public class Item : BaseEntity
    {
        /// <summary>
        /// Item code - business key
        /// </summary>
        [BusinessKey("Item Code", BusinessKeyScope.Company)]
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Item name
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Item description
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Item type (Product, Service, etc.)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ItemType { get; set; } = "Product";

        /// <summary>
        /// Base unit of measure
        /// </summary>
        [MaxLength(20)]
        public string? BaseUnitOfMeasure { get; set; }

        /// <summary>
        /// Standard cost
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? StandardCost { get; set; }

        /// <summary>
        /// List price
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? ListPrice { get; set; }

        /// <summary>
        /// Whether this item affects inventory
        /// </summary>
        public bool AffectsInventory { get; set; } = true;

        /// <summary>
        /// Navigation property to document lines
        /// </summary>
        public virtual ICollection<DocumentLine> DocumentLines { get; set; } = new List<DocumentLine>();

        public override string ToString()
        {
            return $"{Code} - {Name}";
        }
    }

    /// <summary>
    /// Warehouse/location entity
    /// </summary>
    [Table("Warehouses")]
    public class Warehouse : BaseEntity
    {
        /// <summary>
        /// Warehouse code - business key
        /// </summary>
        [BusinessKey("Warehouse Code", BusinessKeyScope.Company)]
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Warehouse name
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Warehouse description
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Warehouse type (Main, Transit, etc.)
        /// </summary>
        [MaxLength(20)]
        public string? WarehouseType { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Manager identifier
        /// </summary>
        public Guid? ManagerId { get; set; }

        /// <summary>
        /// Whether this is the default warehouse
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Navigation property to document lines
        /// </summary>
        public virtual ICollection<DocumentLine> DocumentLines { get; set; } = new List<DocumentLine>();

        public override string ToString()
        {
            return $"{Code} - {Name}";
        }
    }
}
