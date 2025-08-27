using Sivar.Erp.Core.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.Core.Domain.Entities.Documents
{
    /// <summary>
    /// Document total breakdown entity for different types of totals
    /// </summary>
    [Table("DocumentTotals")]
    public class DocumentTotal : BaseEntity
    {
        /// <summary>
        /// Reference to the parent document
        /// </summary>
        [Required]
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Type of total (SubTotal, Tax, Discount, etc.)
        /// </summary>
        [Required]
        public TotalType TotalType { get; set; }

        /// <summary>
        /// Description of the total
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Total amount
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency code for this total
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
        /// Tax rate/percentage if applicable
        /// </summary>
        [Column(TypeName = "decimal(5,4)")]
        public decimal? Rate { get; set; }

        /// <summary>
        /// Tax code or reference if applicable
        /// </summary>
        [MaxLength(20)]
        public string? TaxCode { get; set; }

        /// <summary>
        /// Base amount on which this total is calculated
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? BaseAmount { get; set; }

        /// <summary>
        /// Sort order for display
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Whether this total should be included in the grand total
        /// </summary>
        public bool IncludeInGrandTotal { get; set; } = true;

        /// <summary>
        /// Navigation property to parent document
        /// </summary>
        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; } = null!;

        /// <summary>
        /// Calculates the amount based on base amount and rate
        /// </summary>
        public void CalculateAmount()
        {
            if (BaseAmount.HasValue && Rate.HasValue)
            {
                Amount = BaseAmount.Value * Rate.Value / 100;
            }
        }

        /// <summary>
        /// Sets the total as a tax total
        /// </summary>
        /// <param name="baseAmount">Base amount for tax calculation</param>
        /// <param name="taxRate">Tax rate percentage</param>
        /// <param name="taxCode">Tax code reference</param>
        public void SetAsTax(decimal baseAmount, decimal taxRate, string? taxCode = null)
        {
            TotalType = TotalType.Tax;
            BaseAmount = baseAmount;
            Rate = taxRate;
            TaxCode = taxCode;
            Description = string.IsNullOrEmpty(taxCode) ? $"Tax ({taxRate:F2}%)" : $"{taxCode} ({taxRate:F2}%)";
            SortOrder = 2;
            CalculateAmount();
        }

        /// <summary>
        /// Sets the total as a discount total
        /// </summary>
        /// <param name="baseAmount">Base amount for discount calculation</param>
        /// <param name="discountRate">Discount rate percentage</param>
        /// <param name="discountCode">Discount code reference</param>
        public void SetAsDiscount(decimal baseAmount, decimal discountRate, string? discountCode = null)
        {
            TotalType = TotalType.Discount;
            BaseAmount = baseAmount;
            Rate = discountRate;
            Description = string.IsNullOrEmpty(discountCode) ? $"Discount ({discountRate:F2}%)" : $"{discountCode} ({discountRate:F2}%)";
            Amount = -(baseAmount * discountRate / 100); // Negative for discounts
            SortOrder = 3;
        }

        /// <summary>
        /// Sets the total as a subtotal
        /// </summary>
        /// <param name="amount">Subtotal amount</param>
        public void SetAsSubTotal(decimal amount)
        {
            TotalType = TotalType.SubTotal;
            Amount = amount;
            Description = "Subtotal";
            SortOrder = 1;
        }

        /// <summary>
        /// Sets the total as a grand total
        /// </summary>
        /// <param name="amount">Grand total amount</param>
        public void SetAsGrandTotal(decimal amount)
        {
            TotalType = TotalType.GrandTotal;
            Amount = amount;
            Description = "Total";
            SortOrder = 99;
            IncludeInGrandTotal = false; // Grand total doesn't include itself
        }

        /// <summary>
        /// Validates the document total
        /// </summary>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   DocumentId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   !string.IsNullOrWhiteSpace(CurrencyCode) &&
                   ExchangeRate > 0;
        }

        /// <summary>
        /// Returns a string representation of the document total
        /// </summary>
        public override string ToString()
        {
            return $"{Description}: {Amount:C} {CurrencyCode}";
        }
    }
}
