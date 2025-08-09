using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Implementation of document service for processing and managing documents
    /// </summary>
    [Description("Implementation of document service")]
    public class DocumentService : IDocumentService
    {
        private readonly IRepository _repository;
        private readonly ILogger<DocumentService> _logger;

        /// <summary>
        /// Initializes a new instance of the DocumentService
        /// </summary>
        /// <param name="repository">Repository for data access</param>
        /// <param name="logger">Logger for the service</param>
        public DocumentService(IRepository repository, ILogger<DocumentService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new document
        /// </summary>
        /// <param name="documentType">The document type</param>
        /// <param name="businessEntity">The business entity associated with the document</param>
        /// <returns>The created document</returns>
        [Description("Creates a new document")]
        public async Task<IDocument> CreateDocumentAsync(IDocumentType documentType, IBusinessEntity businessEntity)
        {
            _logger.LogInformation("Creating document of type {DocumentTypeCode} for entity {EntityCode}", 
                documentType.Code, businessEntity.Code);

            var document = _repository.CreateObject<DocumentDto>();
            document.DocumentNumber = await GenerateDocumentNumber(documentType.Code);
            document.Date = DateOnly.FromDateTime(DateTime.Today);
            document.DocumentType = documentType;
            document.BusinessEntity = businessEntity;
            document.Status = DocumentStatus.Draft;

            _logger.LogInformation("Created document {DocumentNumber}", document.DocumentNumber);

            return document;
        }

        /// <summary>
        /// Calculates taxes for a document
        /// </summary>
        /// <param name="document">The document to calculate taxes for</param>
        /// <param name="documentOperation">The operation being performed</param>
        [Description("Calculates taxes for a document")]
        public async Task CalculateDocumentTaxesAsync(IDocument document, DocumentOperation documentOperation)
        {
            _logger.LogInformation("Calculating taxes for document {DocumentNumber}", document.DocumentNumber);

            // Placeholder implementation - would integrate with tax service
            // For now, just log the operation
            await Task.CompletedTask;

            _logger.LogInformation("Completed tax calculation for document {DocumentNumber}", document.DocumentNumber);
        }

        /// <summary>
        /// Validates document before processing
        /// </summary>
        /// <param name="document">The document to validate</param>
        /// <returns>The validation result</returns>
        [Description("Validates document before processing")]
        public async Task<ValidationResult> ValidateDocumentAsync(IDocument document)
        {
            var result = new ValidationResult { IsValid = true };

            // Basic validation
            if (string.IsNullOrEmpty(document.DocumentNumber))
            {
                result.IsValid = false;
                result.Errors.Add("Document number is required");
            }

            if (document.DocumentType == null)
            {
                result.IsValid = false;
                result.Errors.Add("Document type is required");
            }

            if (document.BusinessEntity == null)
            {
                result.IsValid = false;
                result.Errors.Add("Business entity is required");
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Processes a document (validates, calculates taxes, and prepares for posting)
        /// </summary>
        /// <param name="document">The document to process</param>
        /// <param name="operation">The operation being performed</param>
        /// <returns>The processed document</returns>
        [Description("Processes a document")]
        public async Task<IDocument> ProcessDocumentAsync(IDocument document, DocumentOperation operation)
        {
            _logger.LogInformation("Processing document {DocumentNumber} for operation {Operation}", 
                document.DocumentNumber, operation);

            // Validate document
            var validationResult = await ValidateDocumentAsync(document);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Document validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Calculate taxes
            await CalculateDocumentTaxesAsync(document, operation);

            // Update status
            if (document is DocumentDto documentDto)
            {
                documentDto.Status = DocumentStatus.Pending;
                _repository.MarkAsModified(documentDto);
            }

            await _repository.CommitChanges();

            _logger.LogInformation("Successfully processed document {DocumentNumber}", document.DocumentNumber);

            return document;
        }

        /// <summary>
        /// Posts a document to make it final
        /// </summary>
        /// <param name="document">The document to post</param>
        /// <param name="userName">User posting the document</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Posts a document to make it final")]
        public async Task PostDocumentAsync(IDocument document, string userName)
        {
            _logger.LogInformation("Posting document {DocumentNumber} by user {UserName}", 
                document.DocumentNumber, userName);

            if (document.Status != DocumentStatus.Approved)
            {
                throw new InvalidOperationException("Document must be approved before posting");
            }

            // Update status
            if (document is DocumentDto documentDto)
            {
                documentDto.Status = DocumentStatus.Posted;
                _repository.MarkAsModified(documentDto);
            }

            await _repository.CommitChanges();

            _logger.LogInformation("Successfully posted document {DocumentNumber}", document.DocumentNumber);
        }

        /// <summary>
        /// Cancels a document
        /// </summary>
        /// <param name="document">The document to cancel</param>
        /// <param name="reason">Reason for cancellation</param>
        /// <param name="userName">User cancelling the document</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Cancels a document")]
        public async Task CancelDocumentAsync(IDocument document, string reason, string userName)
        {
            _logger.LogInformation("Cancelling document {DocumentNumber} by user {UserName}: {Reason}", 
                document.DocumentNumber, userName, reason);

            if (document.Status == DocumentStatus.Posted)
            {
                throw new InvalidOperationException("Cannot cancel a posted document");
            }

            // Update status
            if (document is DocumentDto documentDto)
            {
                documentDto.Status = DocumentStatus.Cancelled;
                _repository.MarkAsModified(documentDto);
            }

            await _repository.CommitChanges();

            _logger.LogInformation("Successfully cancelled document {DocumentNumber}", document.DocumentNumber);
        }

        /// <summary>
        /// Gets documents by business entity
        /// </summary>
        /// <param name="businessEntityId">Business entity ID</param>
        /// <param name="fromDate">Start date filter</param>
        /// <param name="toDate">End date filter</param>
        /// <returns>Collection of documents</returns>
        [Description("Gets documents by business entity")]
        public async Task<IEnumerable<IDocument>> GetDocumentsByBusinessEntityAsync(Guid businessEntityId, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            var query = _repository.GetObjects<DocumentDto>()
                .Where(d => d.BusinessEntity.Id == businessEntityId);

            if (fromDate.HasValue)
                query = query.Where(d => d.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(d => d.Date <= toDate.Value);

            var documents = query.OrderBy(d => d.Date).ThenBy(d => d.DocumentNumber).ToList();

            return await Task.FromResult(documents);
        }

        /// <summary>
        /// Generates a unique document number
        /// </summary>
        /// <param name="documentTypeCode">Document type code for prefix</param>
        /// <returns>Document number</returns>
        private async Task<string> GenerateDocumentNumber(string documentTypeCode)
        {
            var count = _repository.GetObjects<DocumentDto>()
                .Count(d => d.DocumentType.Code == documentTypeCode);
            
            var documentNumber = $"{documentTypeCode}{DateTime.Now:yyyyMM}{(count + 1):D6}";
            
            return await Task.FromResult(documentNumber);
        }
    }
}