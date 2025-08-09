using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a document type
    /// </summary>
    [Description("Represents a document type")]
    public interface IDocumentType
    {
        /// <summary>
        /// Gets or sets the document type code
        /// </summary>
        string Code { get; set; }
        
        /// <summary>
        /// Gets or sets the document type name
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the document type description
        /// </summary>
        string? Description { get; set; }
    }
}