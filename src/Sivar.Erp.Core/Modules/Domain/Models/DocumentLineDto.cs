using System.ComponentModel;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Domain.Models
{
    /// <summary>
    /// Data transfer object implementation of IDocumentLine  
    /// </summary>
    [Description("Data transfer object implementation of IDocumentLine")]
    public class DocumentLineDto : IDocumentLine
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Description("Entity identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        [Description("Last update date")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the document identifier
        /// </summary>
        [Description("Document identifier")]
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the item identifier
        /// </summary>
        [Description("Item identifier")]
        public Guid? ItemId { get; set; }

        /// <summary>
        /// Gets or sets the item code
        /// </summary>
        [Description("Item code")]
        public string? ItemCode { get; set; }

        /// <summary>
        /// Gets or sets the item description
        /// </summary>
        [Description("Item description")]
        public string? ItemDescription { get; set; }

        /// <summary>
        /// Gets or sets the line number
        /// </summary>
        [Description("Line number")]
        public int LineNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the item associated with this line
        /// </summary>
        [Description("Item associated with this line")]
        public IItem? Item { get; set; }
        
        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        [Description("Quantity")]
        public decimal Quantity { get; set; }
        
        /// <summary>
        /// Gets or sets the unit price
        /// </summary>
        [Description("Unit price")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the line total
        /// </summary>
        [Description("Line total")]
        public decimal LineTotal { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure
        /// </summary>
        [Description("Unit of measure")]
        public string? UnitOfMeasure { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage
        /// </summary>
        [Description("Discount percentage")]
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount
        /// </summary>
        [Description("Discount amount")]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the net amount
        /// </summary>
        [Description("Net amount")]
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Gets or sets the tax amount
        /// </summary>
        [Description("Tax amount")]
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// Gets or sets the line amount
        /// </summary>
        [Description("Line amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the notes
        /// </summary>
        [Description("Notes")]
        public string? Notes { get; set; }
        
        /// <summary>
        /// Description of the line item
        /// </summary>
        [Description("Description of the line item")]
        public string Description { get; set; } = string.Empty;
    }
}
