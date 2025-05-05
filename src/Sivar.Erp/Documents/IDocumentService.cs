namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Interface for document service operations
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Creates a new document
        /// </summary>
        /// <param name="document">Document to create</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Created document with ID</returns>
        Task<IDocument> CreateDocumentAsync(IDocument document, string userName);

        /// <summary>
        /// Updates an existing document
        /// </summary>
        /// <param name="document">Document with updated values</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Updated document</returns>
        Task<IDocument> UpdateDocumentAsync(IDocument document, string userName);

        /// <summary>
        /// Retrieves a document by ID
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>Document if found, null otherwise</returns>
        Task<IDocument?> GetDocumentByIdAsync(Guid id);

        /// <summary>
        /// Retrieves documents within date range
        /// </summary>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>Collection of documents</returns>
        Task<IEnumerable<IDocument>> GetDocumentsByDateRangeAsync(DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Retrieves documents by type
        /// </summary>
        /// <param name="documentType">Document type</param>
        /// <returns>Collection of documents</returns>
        Task<IEnumerable<IDocument>> GetDocumentsByTypeAsync(DocumentType documentType);
    }
}