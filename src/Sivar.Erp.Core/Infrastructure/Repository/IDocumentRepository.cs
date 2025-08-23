using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Core.Infrastructure.Repository;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Infrastructure.Repository
{
    public interface IDocumentRepository : IRepository<IDocument>
    {
        // Document specific operations
        Task<IDocument?> GetByDocumentNumberAsync(string documentNumber);
        Task<IEnumerable<IDocument>> GetByDocumentTypeAsync(string documentType);
        Task<IEnumerable<IDocument>> GetByStatusAsync(DocumentStatus status);
        Task<IEnumerable<IDocument>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // Business entity related documents
        Task<IEnumerable<IDocument>> GetByBusinessEntityAsync(Guid businessEntityId);
        Task<IEnumerable<IDocument>> GetByBusinessEntityTypeAsync(string entityType);
        
        // Document relationships
        Task<IEnumerable<IDocument>> GetRelatedDocumentsAsync(Guid documentId);
        Task<IDocument?> GetParentDocumentAsync(Guid documentId);
        Task<IEnumerable<IDocument>> GetChildDocumentsAsync(Guid documentId);
        
        // Document search and filtering
        Task<IEnumerable<IDocument>> SearchDocumentsAsync(string searchTerm);
        Task<(IEnumerable<IDocument> Items, int TotalCount)> SearchPagedAsync(
            string? searchTerm = null,
            string? documentType = null,
            DocumentStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            Guid? businessEntityId = null,
            int pageNumber = 1,
            int pageSize = 20);
        
        // Document workflow
        Task<IEnumerable<IDocument>> GetDocumentsByWorkflowStateAsync(string workflowState);
        Task<IEnumerable<IDocument>> GetPendingApprovalsAsync(string userId);
        Task<IEnumerable<IDocument>> GetDocumentsByApproverAsync(string userId);
        
        // Document totals and aggregations
        Task<decimal> GetDocumentTotalAsync(Guid documentId);
        Task<Dictionary<string, decimal>> GetDocumentTotalsByTypeAsync(string documentType, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<DocumentStatus, int>> GetDocumentCountsByStatusAsync(string? documentType = null);
        
        // Document versions
        Task<IEnumerable<IDocument>> GetDocumentVersionsAsync(Guid documentId);
        Task<IDocument?> GetDocumentVersionAsync(Guid documentId, int version);
        Task<int> GetLatestVersionNumberAsync(Guid documentId);
        
        // Document attachments
        Task<bool> HasAttachmentsAsync(Guid documentId);
        Task<int> GetAttachmentCountAsync(Guid documentId);
        
        // Bulk operations
        Task<IEnumerable<IDocument>> BulkUpdateStatusAsync(IEnumerable<Guid> documentIds, DocumentStatus newStatus, string? updatedBy = null);
        Task<int> BulkDeleteAsync(IEnumerable<Guid> documentIds);
        
        // Performance and analytics
        Task<Dictionary<string, object>> GetDocumentStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<IDocument>> GetRecentDocumentsAsync(int count = 10, string? userId = null);
        Task<IEnumerable<IDocument>> GetMostViewedDocumentsAsync(int count = 10, DateTime? since = null);
    }
}
