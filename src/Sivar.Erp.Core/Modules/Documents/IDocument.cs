using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a business document
    /// </summary>
    [Description("Represents a business document")]
    public interface IDocument
    {
        /// <summary>
        /// Gets or sets the document type
        /// </summary>
        IDocumentType DocumentType { get; set; }
        
        /// <summary>
        /// Gets or sets the document number
        /// </summary>
        string DocumentNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the document date
        /// </summary>
        DateOnly Date { get; set; }
        
        /// <summary>
        /// Gets or sets the business entity associated with this document
        /// </summary>
        IBusinessEntity BusinessEntity { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of document lines
        /// </summary>
        ICollection<IDocumentLine> Lines { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of document totals
        /// </summary>
        ICollection<ITotal> DocumentTotals { get; set; }
    }
}