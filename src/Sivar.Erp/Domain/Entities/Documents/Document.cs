using Sivar.Erp.Core.Domain.Attributes;
using Sivar.Erp.Core.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.Core.Domain.Entities.Documents
{
    /// <summary>
    /// Core document entity with enhanced monitoring and multi-tenant support
    /// </summary>
    [Table("Documents")]
    public class Document : BaseEntity
    {
        private readonly List<DocumentLine> _lines = new();
        private readonly List<DocumentTotal> _totals = new();

        /// <summary>
        /// Document number - business key
        /// </summary>
        [BusinessKey("Document Number", BusinessKeyScope.Company)]
        [Required]
        [MaxLength(50)]
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// Document date
        /// </summary>
        [Required]
        public DateOnly DocumentDate { get; set; }

        /// <summary>
        /// Document time
        /// </summary>
        [Required]
        public TimeOnly DocumentTime { get; set; }

        /// <summary>
        /// Document type identifier
        /// </summary>
        [Required]
        public Guid DocumentTypeId { get; set; }

        /// <summary>
        /// Business entity (customer/vendor/etc.) identifier
        /// </summary>
        public Guid? BusinessEntityId { get; set; }

        /// <summary>
        /// Document status
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = DocumentStatus.Draft;

        /// <summary>
        /// Document currency code
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "USD";

        /// <summary>
        /// Exchange rate to base currency
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRate { get; set; } = 1.0m;

        /// <summary>
        /// Document subtotal (before taxes and discounts)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Total tax amount
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalTax { get; set; }

        /// <summary>
        /// Total discount amount
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Final total amount
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Reference to another document (for related documents)
        /// </summary>
        public Guid? ReferenceDocumentId { get; set; }

        /// <summary>
        /// Document remarks/notes
        /// </summary>
        [MaxLength(1000)]
        public string? Remarks { get; set; }

        /// <summary>
        /// Posted to accounting flag
        /// </summary>
        public bool IsPosted { get; set; } = false;

        /// <summary>
        /// Posting date
        /// </summary>
        public DateTime? PostedAt { get; set; }

        /// <summary>
        /// User who posted the document
        /// </summary>
        [MaxLength(100)]
        public string? PostedBy { get; set; }

        /// <summary>
        /// Due date for payment documents
        /// </summary>
        public DateOnly? DueDate { get; set; }

        /// <summary>
        /// Navigation property to document type
        /// </summary>
        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType? DocumentType { get; set; }

        /// <summary>
        /// Navigation property to business entity
        /// </summary>
        [ForeignKey(nameof(BusinessEntityId))]
        public virtual BusinessEntity? BusinessEntity { get; set; }

        /// <summary>
        /// Navigation property to reference document
        /// </summary>
        [ForeignKey(nameof(ReferenceDocumentId))]
        public virtual Document? ReferenceDocument { get; set; }

        /// <summary>
        /// Document lines
        /// </summary>
        public virtual ICollection<DocumentLine> Lines => _lines.AsReadOnly();

        /// <summary>
        /// Document totals breakdown
        /// </summary>
        public virtual ICollection<DocumentTotal> Totals => _totals.AsReadOnly();

        /// <summary>
        /// Documents referencing this document
        /// </summary>
        public virtual ICollection<Document> ReferencingDocuments { get; set; } = new List<Document>();

        /// <summary>
        /// Adds a line to the document
        /// </summary>
        /// <param name="line">Document line to add</param>
        public void AddLine(DocumentLine line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            line.DocumentId = Oid;
            line.SetTenantContext(CompanyId, BranchId);
            line.CreatedBy = LastModifiedBy ?? CreatedBy;
            line.CreatedAt = DateTime.UtcNow;

            _lines.Add(line);
            RecalculateTotals();
        }

        /// <summary>
        /// Removes a line from the document
        /// </summary>
        /// <param name="line">Document line to remove</param>
        public void RemoveLine(DocumentLine line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            _lines.Remove(line);
            RecalculateTotals();
        }

        /// <summary>
        /// Adds a total breakdown to the document
        /// </summary>
        /// <param name="total">Document total to add</param>
        public void AddTotal(DocumentTotal total)
        {
            if (total == null)
                throw new ArgumentNullException(nameof(total));

            total.DocumentId = Oid;
            total.SetTenantContext(CompanyId, BranchId);
            total.CreatedBy = LastModifiedBy ?? CreatedBy;
            total.CreatedAt = DateTime.UtcNow;

            _totals.Add(total);
        }

        /// <summary>
        /// Recalculates document totals based on lines
        /// </summary>
        public void RecalculateTotals()
        {
            SubTotal = _lines.Sum(l => l.SubTotal);
            TotalTax = _lines.Sum(l => l.TaxAmount);
            TotalDiscount = _lines.Sum(l => l.DiscountAmount);
            TotalAmount = SubTotal + TotalTax - TotalDiscount;

            // Clear and rebuild totals collection
            _totals.Clear();

            // Add subtotal
            AddTotal(new DocumentTotal
            {
                TotalType = TotalType.SubTotal,
                Description = "Subtotal",
                Amount = SubTotal,
                CurrencyCode = CurrencyCode
            });

            // Add tax total if applicable
            if (TotalTax != 0)
            {
                AddTotal(new DocumentTotal
                {
                    TotalType = TotalType.Tax,
                    Description = "Total Tax",
                    Amount = TotalTax,
                    CurrencyCode = CurrencyCode
                });
            }

            // Add discount total if applicable
            if (TotalDiscount != 0)
            {
                AddTotal(new DocumentTotal
                {
                    TotalType = TotalType.Discount,
                    Description = "Total Discount",
                    Amount = -TotalDiscount, // Negative for display
                    CurrencyCode = CurrencyCode
                });
            }

            // Add final total
            AddTotal(new DocumentTotal
            {
                TotalType = TotalType.GrandTotal,
                Description = "Total Amount",
                Amount = TotalAmount,
                CurrencyCode = CurrencyCode
            });
        }

        /// <summary>
        /// Posts the document to accounting
        /// </summary>
        /// <param name="userId">User performing the posting</param>
        public void Post(string userId)
        {
            if (IsPosted)
                throw new InvalidOperationException("Document is already posted");

            if (Status != DocumentStatus.Approved)
                throw new InvalidOperationException("Document must be approved before posting");

            if (!_lines.Any())
                throw new InvalidOperationException("Cannot post document without lines");

            IsPosted = true;
            PostedAt = DateTime.UtcNow;
            PostedBy = userId;
            Status = DocumentStatus.Posted;
            MarkAsModified(userId);
        }

        /// <summary>
        /// Reverses the posting of the document
        /// </summary>
        /// <param name="userId">User performing the reversal</param>
        public void ReversePosting(string userId)
        {
            if (!IsPosted)
                throw new InvalidOperationException("Document is not posted");

            IsPosted = false;
            PostedAt = null;
            PostedBy = null;
            Status = DocumentStatus.Approved;
            MarkAsModified(userId);
        }

        /// <summary>
        /// Validates the document state
        /// </summary>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(DocumentNumber) &&
                   DocumentTypeId != Guid.Empty &&
                   DocumentDate != default &&
                   !string.IsNullOrWhiteSpace(CurrencyCode) &&
                   ExchangeRate > 0;
        }

        /// <summary>
        /// Returns a string representation of the document
        /// </summary>
        public override string ToString()
        {
            return $"Document {DocumentNumber} - {DocumentDate} - {TotalAmount:C}";
        }
    }

    /// <summary>
    /// Document status enumeration
    /// </summary>
    public static class DocumentStatus
    {
        public const string Draft = "Draft";
        public const string PendingApproval = "Pending";
        public const string Approved = "Approved";
        public const string Posted = "Posted";
        public const string Cancelled = "Cancelled";
        public const string Void = "Void";
    }

    /// <summary>
    /// Total type enumeration
    /// </summary>
    public enum TotalType
    {
        SubTotal = 1,
        Tax = 2,
        Discount = 3,
        Shipping = 4,
        Handling = 5,
        GrandTotal = 99
    }
}
