using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Service for document processing and management
    /// </summary>
    [Description("Service for document processing and management")]
    public interface IDocumentService
    {
        /// <summary>
        /// Creates a new document
        /// </summary>
        /// <param name="documentType">The document type</param>
        /// <param name="businessEntity">The business entity associated with the document</param>
        /// <returns>The created document</returns>
        [Description("Creates a new document")]
        Task<IDocument> CreateDocumentAsync(IDocumentType documentType, IBusinessEntity businessEntity);
        
        /// <summary>
        /// Calculates taxes for a document
        /// </summary>
        /// <param name="document">The document to calculate taxes for</param>
        /// <param name="documentOperation">The operation being performed</param>
        [Description("Calculates taxes for a document")]
        Task CalculateDocumentTaxesAsync(IDocument document, string documentOperation);
        
        /// <summary>
        /// Validates document before processing
        /// </summary>
        /// <param name="document">The document to validate</param>
        /// <returns>The validation result</returns>
        [Description("Validates document before processing")]
        Task<ValidationResult> ValidateDocumentAsync(IDocument document);
    }
}