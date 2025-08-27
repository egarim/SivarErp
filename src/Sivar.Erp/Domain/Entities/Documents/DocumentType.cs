using Sivar.Erp.Core.Domain.Attributes;
using Sivar.Erp.Core.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.Core.Domain.Entities.Documents
{
    /// <summary>
    /// Document type entity defining types of documents (Invoice, Purchase Order, etc.)
    /// </summary>
    [Table("DocumentTypes")]
    public class DocumentType : BaseEntity
    {
        /// <summary>
        /// Document type code - business key
        /// </summary>
        [BusinessKey("Document Type Code", BusinessKeyScope.Company)]
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Document type name
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Document type description
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Document category (Sales, Purchase, Inventory, etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Document direction (Inbound, Outbound, Internal)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Direction { get; set; } = DocumentDirection.Outbound;

        /// <summary>
        /// Whether this document type affects inventory
        /// </summary>
        public bool AffectsInventory { get; set; } = false;

        /// <summary>
        /// Whether this document type affects accounting
        /// </summary>
        public bool AffectsAccounting { get; set; } = true;

        /// <summary>
        /// Whether this document type requires approval
        /// </summary>
        public bool RequiresApproval { get; set; } = false;

        /// <summary>
        /// Whether lines are required for this document type
        /// </summary>
        public bool RequiresLines { get; set; } = true;

        /// <summary>
        /// Whether business entity is required
        /// </summary>
        public bool RequiresBusinessEntity { get; set; } = true;

        /// <summary>
        /// Default currency for this document type
        /// </summary>
        [MaxLength(3)]
        public string? DefaultCurrency { get; set; }

        /// <summary>
        /// Number series configuration for auto-generation
        /// </summary>
        [MaxLength(20)]
        public string? NumberSeries { get; set; }

        /// <summary>
        /// Number format pattern (e.g., "INV-{0:000000}")
        /// </summary>
        [MaxLength(50)]
        public string? NumberFormat { get; set; }

        /// <summary>
        /// Current sequence number for auto-generation
        /// </summary>
        public long CurrentSequence { get; set; } = 0;

        /// <summary>
        /// Whether document numbers are auto-generated
        /// </summary>
        public bool AutoGenerateNumbers { get; set; } = true;

        /// <summary>
        /// Document template for printing/display
        /// </summary>
        [MaxLength(100)]
        public string? DocumentTemplate { get; set; }

        /// <summary>
        /// Default terms and conditions
        /// </summary>
        [MaxLength(2000)]
        public string? DefaultTermsAndConditions { get; set; }

        /// <summary>
        /// Sort order for display
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Color code for UI display
        /// </summary>
        [MaxLength(7)]
        public string? ColorCode { get; set; }

        /// <summary>
        /// Icon name for UI display
        /// </summary>
        [MaxLength(50)]
        public string? IconName { get; set; }

        /// <summary>
        /// Navigation property to documents of this type
        /// </summary>
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

        /// <summary>
        /// Generates the next document number for this type
        /// </summary>
        /// <returns>Next document number</returns>
        public string GenerateNextDocumentNumber()
        {
            if (!AutoGenerateNumbers)
                throw new InvalidOperationException("Auto-generation is not enabled for this document type");

            CurrentSequence++;
            
            if (!string.IsNullOrEmpty(NumberFormat))
            {
                return string.Format(NumberFormat, CurrentSequence);
            }

            var prefix = !string.IsNullOrEmpty(NumberSeries) ? NumberSeries : Code;
            return $"{prefix}-{CurrentSequence:000000}";
        }

        /// <summary>
        /// Validates the document type configuration
        /// </summary>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(Code) &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Category) &&
                   !string.IsNullOrWhiteSpace(Direction);
        }

        /// <summary>
        /// Returns a string representation of the document type
        /// </summary>
        public override string ToString()
        {
            return $"{Code} - {Name}";
        }
    }

    /// <summary>
    /// Document direction enumeration
    /// </summary>
    public static class DocumentDirection
    {
        public const string Inbound = "Inbound";   // Documents coming into the company (Purchase Orders, Bills)
        public const string Outbound = "Outbound"; // Documents going out of the company (Invoices, Sales Orders)
        public const string Internal = "Internal"; // Internal documents (Transfers, Adjustments)
    }

    /// <summary>
    /// Document category enumeration
    /// </summary>
    public static class DocumentCategory
    {
        public const string Sales = "Sales";
        public const string Purchase = "Purchase";
        public const string Inventory = "Inventory";
        public const string Accounting = "Accounting";
        public const string Finance = "Finance";
        public const string Logistics = "Logistics";
        public const string Manufacturing = "Manufacturing";
        public const string Service = "Service";
    }
}
