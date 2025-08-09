using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a document total
    /// </summary>
    [Description("Represents a document total")]
    public interface ITotal
    {
        /// <summary>
        /// Gets or sets the concept name
        /// </summary>
        string Concept { get; set; }
        
        /// <summary>
        /// Gets or sets the total amount
        /// </summary>
        decimal Total { get; set; }
        
        /// <summary>
        /// Gets or sets whether this total is included in the document total
        /// </summary>
        bool IncludeInDocumentTotal { get; set; }
    }
}