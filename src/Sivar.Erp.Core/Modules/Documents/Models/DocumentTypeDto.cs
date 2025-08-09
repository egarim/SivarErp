using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents.Models
{
    /// <summary>
    /// Implementation of the IDocumentType interface
    /// </summary>
    public class DocumentTypeDto : Entity, IDocumentType
    {
        /// <summary>
        /// Gets or sets the document type code
        /// </summary>
        public string Code { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the document type name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the document type description
        /// </summary>
        public string? Description { get; set; }
    }
}