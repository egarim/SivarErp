using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Logging;
using Sivar.Erp.Core.Infrastructure.Repositories;
using Sivar.Erp.Core.Infrastructure.Validation;
using Sivar.Erp.Core.Modules.Domain;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Document workflow service implementation
    /// </summary>
    [Description("Document workflow service")]
    public class DocumentWorkflowService : IDocumentWorkflowService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IBusinessEntityRepository _businessEntityRepository;
        private readonly ILogger<DocumentWorkflowService> _logger;
        private readonly IValidationService _validationService;

        public DocumentWorkflowService(
            IDocumentRepository documentRepository,
            IBusinessEntityRepository businessEntityRepository,
            ILogger<DocumentWorkflowService> logger,
            IValidationService validationService)
        {
            _documentRepository = documentRepository;
            _businessEntityRepository = businessEntityRepository;
            _logger = logger;
            _validationService = validationService;
        }

        public async Task SubmitForApprovalAsync(IDocument document, string submittedBy, string? comments = null)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentWorkflow.SubmitForApproval", document.Id);
            
            try
            {
                if (document.Status != DocumentStatus.Draft)
                    throw new InvalidOperationException($"Document must be in Draft status to submit for approval. Current status: {document.Status}");

                // Create workflow step
                var workflowStep = new DocumentWorkflowStep
                {
                    DocumentId = document.Id,
                    StepName = "Submit for Approval",
                    FromStatus = document.Status,
                    ToStatus = DocumentStatus.Pending,
                    PerformedBy = submittedBy,
                    PerformedAt = DateTime.UtcNow,
                    Comments = comments,
                    IsSystemAction = false
                };

                // Update document status
                document.Status = DocumentStatus.Pending;
                await _documentRepository.UpdateAsync(document);

                // Save workflow step
                await SaveWorkflowStepAsync(workflowStep);

                _logger.LogInformation("Document {DocumentId} submitted for approval by {SubmittedBy}", document.Id, submittedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting document {DocumentId} for approval", document.Id);
                throw;
            }
        }

        public async Task ApproveDocumentAsync(IDocument document, string approvedBy, string? comments = null)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentWorkflow.Approve", document.Id);
            
            try
            {
                if (document.Status != DocumentStatus.Pending)
                    throw new InvalidOperationException($"Document must be in Pending status to approve. Current status: {document.Status}");

                var workflowStep = new DocumentWorkflowStep
                {
                    DocumentId = document.Id,
                    StepName = "Approve",
                    FromStatus = document.Status,
                    ToStatus = DocumentStatus.Approved,
                    PerformedBy = approvedBy,
                    PerformedAt = DateTime.UtcNow,
                    Comments = comments,
                    IsSystemAction = false
                };

                document.Status = DocumentStatus.Approved;
                await _documentRepository.UpdateAsync(document);
                await SaveWorkflowStepAsync(workflowStep);

                _logger.LogInformation("Document {DocumentId} approved by {ApprovedBy}", document.Id, approvedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving document {DocumentId}", document.Id);
                throw;
            }
        }

        public async Task RejectDocumentAsync(IDocument document, string rejectedBy, string reason)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentWorkflow.Reject", document.Id);
            
            try
            {
                if (document.Status != DocumentStatus.Pending)
                    throw new InvalidOperationException($"Document must be in Pending status to reject. Current status: {document.Status}");

                var workflowStep = new DocumentWorkflowStep
                {
                    DocumentId = document.Id,
                    StepName = "Reject",
                    FromStatus = document.Status,
                    ToStatus = DocumentStatus.Draft,
                    PerformedBy = rejectedBy,
                    PerformedAt = DateTime.UtcNow,
                    Comments = reason,
                    IsSystemAction = false
                };

                document.Status = DocumentStatus.Draft;
                await _documentRepository.UpdateAsync(document);
                await SaveWorkflowStepAsync(workflowStep);

                _logger.LogInformation("Document {DocumentId} rejected by {RejectedBy}: {Reason}", document.Id, rejectedBy, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting document {DocumentId}", document.Id);
                throw;
            }
        }

        public async Task<DocumentWorkflowStep> FinalizeAsync(Guid documentId, string finalizedBy, string comments = "")
        {
            using var activity = LoggingExtensions.StartActivity("DocumentWorkflow.Finalize", documentId);
            
            try
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                    throw new InvalidOperationException($"Document {documentId} not found");

                if (document.Status != DocumentStatus.Approved)
                    throw new InvalidOperationException($"Document must be in Approved status to finalize. Current status: {document.Status}");

                var workflowStep = new DocumentWorkflowStep
                {
                    DocumentId = documentId,
                    StepName = "Finalize",
                    FromStatus = document.Status,
                    ToStatus = DocumentStatus.Finalized,
                    PerformedBy = finalizedBy,
                    PerformedAt = DateTime.UtcNow,
                    Comments = comments,
                    IsSystemAction = false
                };

                document.Status = DocumentStatus.Finalized;
                await _documentRepository.UpdateAsync(document);
                await SaveWorkflowStepAsync(workflowStep);

                _logger.LogInformation("Document {DocumentId} finalized by {FinalizedBy}", documentId, finalizedBy);
                return workflowStep;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<DocumentWorkflowStep> ReviseAsync(Guid documentId, string revisedBy, string revisionReason)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentWorkflow.Revise", documentId);
            
            try
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                    throw new InvalidOperationException($"Document {documentId} not found");

                if (document.Status == DocumentStatus.Cancelled || document.Status == DocumentStatus.Finalized)
                    throw new InvalidOperationException($"Cannot revise document with status {document.Status}");

                var workflowStep = new DocumentWorkflowStep
                {
                    DocumentId = documentId,
                    StepName = "Revise",
                    FromStatus = document.Status,
                    ToStatus = DocumentStatus.Draft,
                    PerformedBy = revisedBy,
                    PerformedAt = DateTime.UtcNow,
                    Comments = revisionReason,
                    IsSystemAction = false
                };

                document.Status = DocumentStatus.Draft;
                await _documentRepository.UpdateAsync(document);
                await SaveWorkflowStepAsync(workflowStep);

                _logger.LogInformation("Document {DocumentId} revised by {RevisedBy}: {Reason}", documentId, revisedBy, revisionReason);
                return workflowStep;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revising document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentWorkflowStep>> GetWorkflowHistoryAsync(Guid documentId)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentWorkflow.GetHistory", documentId);
            
            try
            {
                // This would typically query a separate workflow table
                // For now, return empty list as placeholder
                return new List<DocumentWorkflowStep>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow history for document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<IEnumerable<IDocument>> GetPendingApprovalsAsync(string userId)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentWorkflow.GetPendingApprovals", userId);
            
            try
            {
                var documents = await _documentRepository.GetByStatusAsync(DocumentStatus.Pending);
                
                // Filter by approver if specified
                if (!string.IsNullOrEmpty(userId))
                {
                    // This would typically check approval permissions
                    // For now, return all pending approvals
                }

                return documents.Cast<IDocument>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending approvals for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CanPerformActionAsync(Guid documentId, string userId, string action)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentWorkflow.CanPerformAction", documentId);
            
            try
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                    return false;

                // Basic permission check - in a real system this would check user roles and permissions
                switch (action.ToUpperInvariant())
                {
                    case "SUBMIT":
                        return document.Status == DocumentStatus.Draft;
                    case "APPROVE":
                        return document.Status == DocumentStatus.PendingApproval;
                    case "REJECT":
                        return document.Status == DocumentStatus.PendingApproval;
                    case "FINALIZE":
                        return document.Status == DocumentStatus.Approved;
                    case "REVISE":
                        return document.Status != DocumentStatus.Cancelled && document.Status != DocumentStatus.Finalized;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking action permission for document {DocumentId}", documentId);
                return false;
            }
        }

        public async Task<Dictionary<string, int>> GetWorkflowStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentWorkflow.GetStatistics");
            
            try
            {
                var stats = new Dictionary<string, int>();
                
                // Get documents by status
                var allDocuments = await _documentRepository.GetAllAsync();
                
                if (startDate.HasValue || endDate.HasValue)
                {
                    allDocuments = allDocuments.Where(d => 
                        (!startDate.HasValue || d.CreatedAt >= startDate.Value) &&
                        (!endDate.HasValue || d.CreatedAt <= endDate.Value));
                }

                stats["Draft"] = allDocuments.Count(d => d.Status == DocumentStatus.Draft);
                stats["PendingApproval"] = allDocuments.Count(d => d.Status == DocumentStatus.PendingApproval);
                stats["Approved"] = allDocuments.Count(d => d.Status == DocumentStatus.Approved);
                stats["Finalized"] = allDocuments.Count(d => d.Status == DocumentStatus.Finalized);
                stats["Cancelled"] = allDocuments.Count(d => d.Status == DocumentStatus.Cancelled);
                stats["Total"] = allDocuments.Count();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow statistics");
                throw;
            }
        }

        private async Task SaveWorkflowStepAsync(DocumentWorkflowStep workflowStep)
        {
            // In a real implementation, this would save to a workflow history table
            // For now, this is a placeholder
            await Task.CompletedTask;
        }
    }
}
