using Sivar.Erp.Core.Domain.Entities.Documents;

namespace Sivar.Erp.Core.Application.Services.Documents
{
    /// <summary>
    /// Service interface for document management operations
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Creates a new document
        /// </summary>
        /// <param name="document">Document to create</param>
        /// <param name="userId">User creating the document</param>
        /// <returns>Created document</returns>
        Task<Document> CreateDocumentAsync(Document document, string userId);

        /// <summary>
        /// Gets a document by ID
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <param name="userId">User requesting the document</param>
        /// <param name="companyId">Company context</param>
        /// <returns>Document if found</returns>
        Task<Document?> GetDocumentAsync(Guid documentId, string userId, Guid companyId);

        /// <summary>
        /// Gets a document by document number
        /// </summary>
        /// <param name="documentNumber">Document number</param>
        /// <param name="companyId">Company context</param>
        /// <returns>Document if found</returns>
        Task<Document?> GetDocumentByNumberAsync(string documentNumber, Guid companyId);

        /// <summary>
        /// Updates an existing document
        /// </summary>
        /// <param name="document">Document to update</param>
        /// <param name="userId">User updating the document</param>
        /// <returns>Updated document</returns>
        Task<Document> UpdateDocumentAsync(Document document, string userId);

        /// <summary>
        /// Deletes a document (soft delete)
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <param name="userId">User deleting the document</param>
        Task DeleteDocumentAsync(Guid documentId, string userId);

        /// <summary>
        /// Gets documents for a company with filtering and pagination
        /// </summary>
        /// <param name="companyId">Company identifier</param>
        /// <param name="documentTypeId">Document type filter (optional)</param>
        /// <param name="status">Status filter (optional)</param>
        /// <param name="fromDate">Date range start (optional)</param>
        /// <param name="toDate">Date range end (optional)</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="pageNumber">Page number</param>
        /// <returns>Paginated list of documents</returns>
        Task<(IEnumerable<Document> Documents, int TotalCount)> GetDocumentsAsync(
            Guid companyId,
            Guid? documentTypeId = null,
            string? status = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            int pageSize = 50,
            int pageNumber = 1);

        /// <summary>
        /// Adds a line to a document
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <param name="line">Line to add</param>
        /// <param name="userId">User adding the line</param>
        /// <returns>Updated document</returns>
        Task<Document> AddLineAsync(Guid documentId, DocumentLine line, string userId);

        /// <summary>
        /// Updates a document line
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <param name="line">Line to update</param>
        /// <param name="userId">User updating the line</param>
        /// <returns>Updated document</returns>
        Task<Document> UpdateLineAsync(Guid documentId, DocumentLine line, string userId);

        /// <summary>
        /// Removes a line from a document
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <param name="lineId">Line identifier</param>
        /// <param name="userId">User removing the line</param>
        /// <returns>Updated document</returns>
        Task<Document> RemoveLineAsync(Guid documentId, Guid lineId, string userId);

        /// <summary>
        /// Posts a document to accounting
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <param name="userId">User posting the document</param>
        /// <returns>Posted document</returns>
        Task<Document> PostDocumentAsync(Guid documentId, string userId);

        /// <summary>
        /// Reverses the posting of a document
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <param name="userId">User reversing the posting</param>
        /// <returns>Unposted document</returns>
        Task<Document> ReversePostingAsync(Guid documentId, string userId);

        /// <summary>
        /// Changes the status of a document
        /// </summary>
        /// <param name="documentId">Document identifier</param>
        /// <param name="newStatus">New status</param>
        /// <param name="userId">User changing the status</param>
        /// <returns>Updated document</returns>
        Task<Document> ChangeStatusAsync(Guid documentId, string newStatus, string userId);

        /// <summary>
        /// Gets document statistics for a company
        /// </summary>
        /// <param name="companyId">Company identifier</param>
        /// <param name="fromDate">Date range start (optional)</param>
        /// <param name="toDate">Date range end (optional)</param>
        /// <returns>Document statistics</returns>
        Task<DocumentStatistics> GetDocumentStatisticsAsync(Guid companyId, DateOnly? fromDate = null, DateOnly? toDate = null);

        /// <summary>
        /// Validates a document before saving
        /// </summary>
        /// <param name="document">Document to validate</param>
        /// <returns>Validation result</returns>
        Task<DocumentValidationResult> ValidateDocumentAsync(Document document);

        /// <summary>
        /// Duplicates an existing document
        /// </summary>
        /// <param name="sourceDocumentId">Source document identifier</param>
        /// <param name="newDocumentTypeId">New document type (optional)</param>
        /// <param name="userId">User creating the duplicate</param>
        /// <returns>New duplicated document</returns>
        Task<Document> DuplicateDocumentAsync(Guid sourceDocumentId, Guid? newDocumentTypeId, string userId);
    }

    /// <summary>
    /// Document statistics DTO
    /// </summary>
    public class DocumentStatistics
    {
        public Guid CompanyId { get; set; }
        public int TotalDocuments { get; set; }
        public int DraftDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public int ApprovedDocuments { get; set; }
        public int PostedDocuments { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public Dictionary<string, int> DocumentsByType { get; set; } = new();
        public Dictionary<string, decimal> AmountsByType { get; set; } = new();
    }

    /// <summary>
    /// Document validation result DTO
    /// </summary>
    public class DocumentValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
