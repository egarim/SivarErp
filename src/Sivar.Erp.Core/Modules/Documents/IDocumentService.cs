using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;

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
        Task CalculateDocumentTaxesAsync(IDocument document, DocumentOperation documentOperation);
        
        /// <summary>
        /// Validates document before processing
        /// </summary>
        /// <param name="document">The document to validate</param>
        /// <returns>The validation result</returns>
        [Description("Validates document before processing")]
        Task<ValidationResult> ValidateDocumentAsync(IDocument document);

        /// <summary>
        /// Processes a document (validates, calculates taxes, and prepares for posting)
        /// </summary>
        /// <param name="document">The document to process</param>
        /// <param name="operation">The operation being performed</param>
        /// <returns>The processed document</returns>
        [Description("Processes a document")]
        Task<IDocument> ProcessDocumentAsync(IDocument document, DocumentOperation operation);

        /// <summary>
        /// Posts a document to make it final
        /// </summary>
        /// <param name="document">The document to post</param>
        /// <param name="userName">User posting the document</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Posts a document to make it final")]
        Task PostDocumentAsync(IDocument document, string userName);

        /// <summary>
        /// Cancels a document
        /// </summary>
        /// <param name="document">The document to cancel</param>
        /// <param name="reason">Reason for cancellation</param>
        /// <param name="userName">User cancelling the document</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Cancels a document")]
        Task CancelDocumentAsync(IDocument document, string reason, string userName);

        /// <summary>
        /// Gets documents by business entity
        /// </summary>
        /// <param name="businessEntityId">Business entity ID</param>
        /// <param name="fromDate">Start date filter</param>
        /// <param name="toDate">End date filter</param>
        /// <returns>Collection of documents</returns>
        [Description("Gets documents by business entity")]
        Task<IEnumerable<IDocument>> GetDocumentsByBusinessEntityAsync(Guid businessEntityId, DateOnly? fromDate = null, DateOnly? toDate = null);
    }
}