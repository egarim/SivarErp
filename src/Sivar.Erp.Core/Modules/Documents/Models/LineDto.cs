using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents.Models
{
    /// <summary>
    /// Implementation of the IDocumentLine interface
    /// </summary>
    public class LineDto : Entity, IDocumentLine
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
    }
}