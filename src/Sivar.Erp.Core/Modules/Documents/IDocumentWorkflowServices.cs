using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Service for document workflow management and approval processes
    /// </summary>
    [Description("Service for document workflow management")]
    public interface IDocumentWorkflowService
    {
        /// <summary>
        /// Submits a document for approval
        /// </summary>
        /// <param name="document">Document to submit</param>
        /// <param name="submittedBy">User submitting the document</param>
        /// <param name="comments">Optional submission comments</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Submits a document for approval")]
        Task SubmitForApprovalAsync(IDocument document, string submittedBy, string? comments = null);

        /// <summary>
        /// Approves a document
        /// </summary>
        /// <param name="document">Document to approve</param>
        /// <param name="approvedBy">User approving the document</param>
        /// <param name="comments">Optional approval comments</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Approves a document")]
        Task ApproveDocumentAsync(IDocument document, string approvedBy, string? comments = null);

        /// <summary>
        /// Rejects a document
        /// </summary>
        /// <param name="document">Document to reject</param>
        /// <param name="rejectedBy">User rejecting the document</param>
        /// <param name="reason">Reason for rejection</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Rejects a document")]
        Task RejectDocumentAsync(IDocument document, string rejectedBy, string reason);

        /// <summary>
        /// Gets approval workflow history for a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Collection of workflow steps</returns>
        [Description("Gets approval workflow history")]
        Task<IEnumerable<DocumentWorkflowStep>> GetWorkflowHistoryAsync(Guid documentId);

        /// <summary>
        /// Gets pending approvals for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Collection of documents pending approval</returns>
        [Description("Gets pending approvals for a user")]
        Task<IEnumerable<IDocument>> GetPendingApprovalsAsync(string userId);
    }

    /// <summary>
    /// Service for document attachments and file management
    /// </summary>
    [Description("Service for document attachments")]
    public interface IDocumentAttachmentService
    {
        /// <summary>
        /// Attaches a file to a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileContent">File content</param>
        /// <param name="contentType">MIME content type</param>
        /// <param name="attachedBy">User attaching the file</param>
        /// <returns>The created attachment</returns>
        [Description("Attaches a file to a document")]
        Task<DocumentAttachment> AttachFileAsync(Guid documentId, string fileName, byte[] fileContent, string contentType, string attachedBy);

        /// <summary>
        /// Gets all attachments for a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Collection of attachments</returns>
        [Description("Gets all attachments for a document")]
        Task<IEnumerable<DocumentAttachment>> GetDocumentAttachmentsAsync(Guid documentId);

        /// <summary>
        /// Downloads an attachment
        /// </summary>
        /// <param name="attachmentId">Attachment ID</param>
        /// <returns>Attachment file data</returns>
        [Description("Downloads an attachment")]
        Task<AttachmentFileData> DownloadAttachmentAsync(Guid attachmentId);

        /// <summary>
        /// Removes an attachment from a document
        /// </summary>
        /// <param name="attachmentId">Attachment ID</param>
        /// <param name="removedBy">User removing the attachment</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Removes an attachment")]
        Task RemoveAttachmentAsync(Guid attachmentId, string removedBy);
    }

    /// <summary>
    /// Service for document search and reporting
    /// </summary>
    [Description("Service for document search and reporting")]
    public interface IDocumentSearchService
    {
        /// <summary>
        /// Searches documents based on criteria
        /// </summary>
        /// <param name="criteria">Search criteria</param>
        /// <returns>Search results</returns>
        [Description("Searches documents based on criteria")]
        Task<DocumentSearchResult> SearchDocumentsAsync(DocumentSearchCriteria criteria);

        /// <summary>
        /// Generates a document report
        /// </summary>
        /// <param name="reportType">Type of report to generate</param>
        /// <param name="parameters">Report parameters</param>
        /// <returns>Generated report</returns>
        [Description("Generates a document report")]
        Task<DocumentReport> GenerateReportAsync(DocumentReportType reportType, DocumentReportParameters parameters);

        /// <summary>
        /// Gets document summary statistics
        /// </summary>
        /// <param name="filter">Optional filter criteria</param>
        /// <returns>Document statistics</returns>
        [Description("Gets document summary statistics")]
        Task<DocumentSummaryStatistics> GetDocumentStatisticsAsync(DocumentStatisticsFilter? filter = null);
    }

    /// <summary>
    /// Service for document templates
    /// </summary>
    [Description("Service for document templates")]
    public interface IDocumentTemplateService
    {
        /// <summary>
        /// Creates a document from a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="businessEntity">Business entity for the document</param>
        /// <param name="templateData">Data to populate the template</param>
        /// <returns>Created document</returns>
        [Description("Creates a document from a template")]
        Task<IDocument> CreateFromTemplateAsync(Guid templateId, IBusinessEntity businessEntity, IDictionary<string, object>? templateData = null);

        /// <summary>
        /// Gets available templates for a document type
        /// </summary>
        /// <param name="documentTypeCode">Document type code</param>
        /// <returns>Collection of available templates</returns>
        [Description("Gets available templates for a document type")]
        Task<IEnumerable<DocumentTemplate>> GetTemplatesForDocumentTypeAsync(string documentTypeCode);

        /// <summary>
        /// Creates or updates a document template
        /// </summary>
        /// <param name="template">Template to save</param>
        /// <param name="savedBy">User saving the template</param>
        /// <returns>The saved template</returns>
        [Description("Creates or updates a document template")]
        Task<DocumentTemplate> SaveTemplateAsync(DocumentTemplate template, string savedBy);

        /// <summary>
        /// Deletes a document template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="deletedBy">User deleting the template</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Deletes a document template")]
        Task DeleteTemplateAsync(Guid templateId, string deletedBy);
    }
}
