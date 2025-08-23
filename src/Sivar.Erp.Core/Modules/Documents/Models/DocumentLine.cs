using System;
using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Document line entity implementation for the Core project
    /// </summary>
    [Description("Document line entity implementation for the Core project")]
    public class DocumentLine : IDocumentLine
    {
        /// <summary>
        /// Gets or sets the line number
        /// </summary>
        public int LineNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the item associated with this line
        /// </summary>
        public IItem? Item { get; set; }
        
        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public decimal Quantity { get; set; }
        
        /// <summary>
        /// Gets or sets the unit price
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Gets or sets the line amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Description of the line item
        /// </summary>
        [Description("Description of the line item")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Product or service code
        /// </summary>
        [Description("Product or service code")]
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// Unit of measure
        /// </summary>
        [Description("Unit of measure")]
        public string? UnitOfMeasure { get; set; }

        /// <summary>
        /// Discount percentage applied to this line
        /// </summary>
        [Description("Discount percentage applied to this line")]
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// Discount amount applied to this line
        /// </summary>
        [Description("Discount amount applied to this line")]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Tax amount for this line
        /// </summary>
        [Description("Tax amount for this line")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Net amount after discounts and taxes
        /// </summary>
        [Description("Net amount after discounts and taxes")]
        public decimal NetAmount => Amount - DiscountAmount + TaxAmount;
    }
}
