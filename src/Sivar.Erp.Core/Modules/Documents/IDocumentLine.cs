using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a line in a document
    /// </summary>
    [Description("Represents a line in a document")]
    public interface IDocumentLine
    {
        /// <summary>
        /// Gets or sets the line number
        /// </summary>
        int LineNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the item associated with this line
        /// </summary>
        IItem? Item { get; set; }
        
        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        decimal Quantity { get; set; }
        
        /// <summary>
        /// Gets or sets the unit price
        /// </summary>
        decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Gets or sets the line amount
        /// </summary>
        decimal Amount { get; set; }
    }
}