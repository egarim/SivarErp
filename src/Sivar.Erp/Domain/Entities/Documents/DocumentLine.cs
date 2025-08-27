using Sivar.Erp.Core.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.Core.Domain.Entities.Documents
{
    /// <summary>
    /// Document line entity representing individual items/services in a document
    /// </summary>
    [Table("DocumentLines")]
    public class DocumentLine : BaseEntity
    {
        /// <summary>
        /// Reference to the parent document
        /// </summary>
        [Required]
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Line sequence number within the document
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Item/service identifier
        /// </summary>
        public Guid? ItemId { get; set; }

        /// <summary>
        /// Item code for reference
        /// </summary>
        [MaxLength(50)]
        public string? ItemCode { get; set; }

        /// <summary>
        /// Item/service description
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Quantity
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit of measure
        /// </summary>
        [MaxLength(20)]
        public string? UnitOfMeasure { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Discount percentage
        /// </summary>
        [Column(TypeName = "decimal(5,4)")]
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// Discount amount
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Line subtotal before taxes (Quantity * UnitPrice - DiscountAmount)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Tax percentage
        /// </summary>
        [Column(TypeName = "decimal(5,4)")]
        public decimal TaxPercent { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Line total amount (SubTotal + TaxAmount)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Total { get; set; }

        /// <summary>
        /// Currency code for this line
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "USD";

        /// <summary>
        /// Exchange rate for currency conversion
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRate { get; set; } = 1.0m;

        /// <summary>
        /// Warehouse/location identifier
        /// </summary>
        public Guid? WarehouseId { get; set; }

        /// <summary>
        /// Project identifier for project tracking
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Cost center identifier
        /// </summary>
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Additional line notes
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Navigation property to parent document
        /// </summary>
        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; } = null!;

        /// <summary>
        /// Navigation property to item
        /// </summary>
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        /// <summary>
        /// Navigation property to warehouse
        /// </summary>
        [ForeignKey(nameof(WarehouseId))]
        public virtual Warehouse? Warehouse { get; set; }

        /// <summary>
        /// Calculates line totals based on quantity, price, discount, and tax
        /// </summary>
        public void CalculateTotals()
        {
            var lineAmount = Quantity * UnitPrice;
            
            // Calculate discount amount if percentage is provided
            if (DiscountPercent > 0 && DiscountAmount == 0)
            {
                DiscountAmount = lineAmount * DiscountPercent / 100;
            }

            // Calculate subtotal after discount
            SubTotal = lineAmount - DiscountAmount;

            // Calculate tax amount if percentage is provided
            if (TaxPercent > 0 && TaxAmount == 0)
            {
                TaxAmount = SubTotal * TaxPercent / 100;
            }

            // Calculate final total
            Total = SubTotal + TaxAmount;
        }

        /// <summary>
        /// Sets the item information
        /// </summary>
        /// <param name="itemId">Item identifier</param>
        /// <param name="itemCode">Item code</param>
        /// <param name="description">Item description</param>
        /// <param name="unitPrice">Default unit price</param>
        public void SetItem(Guid itemId, string itemCode, string description, decimal unitPrice)
        {
            ItemId = itemId;
            ItemCode = itemCode;
            Description = description;
            UnitPrice = unitPrice;
        }

        /// <summary>
        /// Sets the quantity and recalculates totals
        /// </summary>
        /// <param name="quantity">New quantity</param>
        public void SetQuantity(decimal quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

            Quantity = quantity;
            CalculateTotals();
        }

        /// <summary>
        /// Sets the unit price and recalculates totals
        /// </summary>
        /// <param name="unitPrice">New unit price</param>
        public void SetUnitPrice(decimal unitPrice)
        {
            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

            UnitPrice = unitPrice;
            CalculateTotals();
        }

        /// <summary>
        /// Applies a discount percentage and recalculates totals
        /// </summary>
        /// <param name="discountPercent">Discount percentage (0-100)</param>
        public void ApplyDiscountPercent(decimal discountPercent)
        {
            if (discountPercent < 0 || discountPercent > 100)
                throw new ArgumentException("Discount percent must be between 0 and 100", nameof(discountPercent));

            DiscountPercent = discountPercent;
            DiscountAmount = 0; // Reset amount so it gets calculated
            CalculateTotals();
        }

        /// <summary>
        /// Applies a discount amount and recalculates totals
        /// </summary>
        /// <param name="discountAmount">Discount amount</param>
        public void ApplyDiscountAmount(decimal discountAmount)
        {
            if (discountAmount < 0)
                throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

            DiscountAmount = discountAmount;
            DiscountPercent = 0; // Reset percentage when amount is set explicitly
            CalculateTotals();
        }

        /// <summary>
        /// Applies a tax percentage and recalculates totals
        /// </summary>
        /// <param name="taxPercent">Tax percentage</param>
        public void ApplyTaxPercent(decimal taxPercent)
        {
            if (taxPercent < 0)
                throw new ArgumentException("Tax percent cannot be negative", nameof(taxPercent));

            TaxPercent = taxPercent;
            TaxAmount = 0; // Reset amount so it gets calculated
            CalculateTotals();
        }

        /// <summary>
        /// Validates the document line
        /// </summary>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   DocumentId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   Quantity >= 0 &&
                   UnitPrice >= 0 &&
                   !string.IsNullOrWhiteSpace(CurrencyCode) &&
                   ExchangeRate > 0;
        }

        /// <summary>
        /// Returns a string representation of the document line
        /// </summary>
        public override string ToString()
        {
            return $"Line {LineNumber}: {Description} - Qty: {Quantity} x {UnitPrice:C} = {Total:C}";
        }
    }
}
