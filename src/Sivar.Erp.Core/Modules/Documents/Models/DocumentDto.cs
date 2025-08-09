using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents.Models
{
    /// <summary>
    /// Implementation of the IDocument interface
    /// </summary>
    public class DocumentDto : Entity, IDocument
    {
        /// <summary>
        /// Gets or sets the document type
        /// </summary>
        public IDocumentType DocumentType { get; set; } = new DocumentTypeDto();
        
        /// <summary>
        /// Gets or sets the document number
        /// </summary>
        public string DocumentNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the document date
        /// </summary>
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        
        /// <summary>
        /// Gets or sets the business entity associated with this document
        /// </summary>
        public IBusinessEntity BusinessEntity { get; set; } = new BusinessEntityDto();
        
        /// <summary>
        /// Gets or sets the collection of document lines
        /// </summary>
        public ICollection<IDocumentLine> Lines { get; set; } = new List<IDocumentLine>();
        
        /// <summary>
        /// Gets or sets the collection of document totals
        /// </summary>
        public ICollection<ITotal> DocumentTotals { get; set; } = new List<ITotal>();
    }
}