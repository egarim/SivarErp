using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Domain
{
    /// <summary>
    /// Interface for document lines
    /// </summary>
    [Description("Interface for document lines")]
    public interface IDocumentLine : IEntity
    {
        /// <summary>
        /// Gets or sets the document ID
        /// </summary>
        [Description("Document ID")]
        Guid DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the line number
        /// </summary>
        [Description("Line number")]
        int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the item ID
        /// </summary>
        [Description("Item ID")]
        Guid? ItemId { get; set; }

        /// <summary>
        /// Gets or sets the item code
        /// </summary>
        [Description("Item code")]
        string? ItemCode { get; set; }

        /// <summary>
        /// Gets or sets the item description
        /// </summary>
        [Description("Item description")]
        string? ItemDescription { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        [Description("Quantity")]
        decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price
        /// </summary>
        [Description("Unit price")]
        decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the line total
        /// </summary>
        [Description("Line total")]
        decimal LineTotal { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure
        /// </summary>
        [Description("Unit of measure")]
        string? UnitOfMeasure { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage
        /// </summary>
        [Description("Discount percentage")]
        decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount
        /// </summary>
        [Description("Discount amount")]
        decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the net amount
        /// </summary>
        [Description("Net amount")]
        decimal NetAmount { get; set; }

        /// <summary>
        /// Gets or sets the tax amount
        /// </summary>
        [Description("Tax amount")]
        decimal TaxAmount { get; set; }

        /// <summary>
        /// Gets or sets the notes
        /// </summary>
        [Description("Notes")]
        string? Notes { get; set; }
    }
}
