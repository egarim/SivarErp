using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Modules.Taxes;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Implementation of document processing and management service
    /// </summary>
    [Description("Implementation of document processing and management service")]
    public class DocumentService : IDocumentService
    {
        private readonly IRepository _repository;
        private readonly ITaxService _taxService;
        private readonly ILogger<DocumentService> _logger;

        /// <summary>
        /// Initializes a new instance of the DocumentService
        /// </summary>
        /// <param name="repository">Repository for data access</param>
        /// <param name="taxService">Tax service for tax calculations</param>
        /// <param name="logger">Logger for the service</param>
        public DocumentService(IRepository repository, ITaxService taxService, ILogger<DocumentService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _taxService = taxService ?? throw new ArgumentNullException(nameof(taxService));
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
            if (documentType == null)
                throw new ArgumentNullException(nameof(documentType));
            if (businessEntity == null)
                throw new ArgumentNullException(nameof(businessEntity));

            _logger.LogInformation("Creating document of type {DocumentType} for entity {BusinessEntity}", 
                documentType.Code, businessEntity.Code);

            var document = _repository.CreateObject<DocumentDto>();
            
            // Generate document number
            document.DocumentNumber = await GenerateDocumentNumberAsync(documentType.Code);
            document.Date = DateOnly.FromDateTime(DateTime.Today);
            document.DocumentType = documentType;
            document.BusinessEntity = businessEntity;
            document.Status = DocumentStatus.Draft;

            _logger.LogInformation("Created document {DocumentNumber} for {BusinessEntity}", 
                document.DocumentNumber, businessEntity.Name);

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
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _logger.LogInformation("Calculating taxes for document {DocumentNumber}", document.DocumentNumber);

            // Get applicable taxes
            var taxes = await _taxService.GetApplicableTaxesAsync(document, documentOperation);
            
            // Apply tax rules to document
            await _taxService.ApplyTaxRulesToDocumentAsync(document);

            // Create tax totals
            var taxTotals = await _taxService.CreateTaxTotalsAsync(document, documentOperation);
            
            // Add tax totals to document
            foreach (var taxTotal in taxTotals)
            {
                document.DocumentTotals.Add(taxTotal);
            }

            _logger.LogInformation("Calculated {TaxCount} tax totals for document {DocumentNumber}", 
                taxTotals.Count(), document.DocumentNumber);
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

            if (document == null)
            {
                result.IsValid = false;
                result.Errors.Add("Document cannot be null");
                return result;
            }

            // Validate document number
            if (string.IsNullOrWhiteSpace(document.DocumentNumber))
            {
                result.IsValid = false;
                result.Errors.Add("Document number is required");
            }

            // Validate document type
            if (document.DocumentType == null)
            {
                result.IsValid = false;
                result.Errors.Add("Document type is required");
            }

            // Validate business entity
            if (document.BusinessEntity == null)
            {
                result.IsValid = false;
                result.Errors.Add("Business entity is required");
            }

            // Validate date
            if (document.Date == default(DateOnly))
            {
                result.IsValid = false;
                result.Errors.Add("Document date is required");
            }

            // Check for future dates
            if (document.Date > DateOnly.FromDateTime(DateTime.Today))
            {
                result.Warnings.Add("Document date is in the future");
            }

            // Validate document totals if they exist
            if (document.DocumentTotals.Any())
            {
                var invalidTotals = document.DocumentTotals.Where(dt => dt.Total < 0).ToList();
                if (invalidTotals.Any())
                {
                    result.IsValid = false;
                    result.Errors.Add("Document cannot have negative totals");
                }
            }

            // Check for duplicate document number (only for new documents)
            if (document.Id == Guid.Empty || document.Id == default(Guid))
            {
                var existingDocument = _repository.FindObject<DocumentDto>(d => 
                    d.DocumentNumber == document.DocumentNumber && d.Id != document.Id);

                if (existingDocument != null)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Document number {document.DocumentNumber} already exists");
                }
            }

            _logger.LogDebug("Document validation completed for {DocumentNumber}: {IsValid}", 
                document.DocumentNumber, result.IsValid);

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
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _logger.LogInformation("Processing document {DocumentNumber} for operation {Operation}", 
                document.DocumentNumber, operation);

            // Validate document
            var validationResult = await ValidateDocumentAsync(document);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Cannot process invalid document: {string.Join(", ", validationResult.Errors)}");
            }

            // Calculate taxes
            await CalculateDocumentTaxesAsync(document, operation);

            // Add basic document totals based on operation
            await AddBasicDocumentTotalsAsync(document, operation);

            // Update document status
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
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("User name is required", nameof(userName));

            _logger.LogInformation("Posting document {DocumentNumber} by user {UserName}", 
                document.DocumentNumber, userName);

            // Validate document before posting
            var validationResult = await ValidateDocumentAsync(document);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Cannot post invalid document: {string.Join(", ", validationResult.Errors)}");
            }

            // Check current status
            if (document.Status == DocumentStatus.Posted)
            {
                _logger.LogWarning("Document {DocumentNumber} is already posted", document.DocumentNumber);
                return;
            }

            if (document.Status == DocumentStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot post a cancelled document");
            }

            // Update document status
            if (document is DocumentDto documentDto)
            {
                documentDto.Status = DocumentStatus.Posted;
                documentDto.UpdatedAt = DateTime.UtcNow;
                _repository.MarkAsModified(documentDto);
            }

            await _repository.CommitChanges();

            _logger.LogInformation("Successfully posted document {DocumentNumber} by user {UserName}", 
                document.DocumentNumber, userName);
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
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Cancellation reason is required", nameof(reason));
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("User name is required", nameof(userName));

            _logger.LogInformation("Cancelling document {DocumentNumber} by user {UserName}: {Reason}", 
                document.DocumentNumber, userName, reason);

            // Store original status for audit
            var originalStatus = document.Status;

            // Check if document can be cancelled
            if (document.Status == DocumentStatus.Cancelled)
            {
                _logger.LogWarning("Document {DocumentNumber} is already cancelled", document.DocumentNumber);
                return;
            }

            if (document.Status == DocumentStatus.Posted)
            {
                throw new InvalidOperationException("Cannot cancel a posted document. Use reversal instead.");
            }

            // Update document status
            if (document is DocumentDto documentDto)
            {
                documentDto.Status = DocumentStatus.Cancelled;
                documentDto.UpdatedAt = DateTime.UtcNow;
                _repository.MarkAsModified(documentDto);
            }

            // Log cancellation audit trail (simplified approach)
            _logger.LogInformation("Document cancellation audit: DocumentNumber={DocumentNumber}, " +
                "PreviousStatus={PreviousStatus}, NewStatus={NewStatus}, " +
                "CancelledBy={UserName}, Reason={Reason}, Timestamp={Timestamp}",
                document.DocumentNumber, originalStatus, DocumentStatus.Cancelled, 
                userName, reason, DateTime.UtcNow);

            await _repository.CommitChanges();

            _logger.LogInformation("Successfully cancelled document {DocumentNumber} by user {UserName}", 
                document.DocumentNumber, userName);
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
            _logger.LogDebug("Getting documents for business entity {BusinessEntityId} from {FromDate} to {ToDate}", 
                businessEntityId, fromDate, toDate);

            var query = _repository.GetObjects<DocumentDto>()
                .Where(d => d.BusinessEntity.Id == businessEntityId);

            if (fromDate.HasValue)
            {
                query = query.Where(d => d.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.Date <= toDate.Value);
            }

            var documents = query.OrderByDescending(d => d.Date)
                               .ThenByDescending(d => d.DocumentNumber)
                               .ToList();

            _logger.LogDebug("Found {DocumentCount} documents for business entity {BusinessEntityId}", 
                documents.Count, businessEntityId);

            return await Task.FromResult(documents);
        }

        #region Private Helper Methods

        /// <summary>
        /// Generates a unique document number based on document type
        /// </summary>
        /// <param name="documentTypeCode">Document type code</param>
        /// <returns>Generated document number</returns>
        private async Task<string> GenerateDocumentNumberAsync(string documentTypeCode)
        {
            // Get count of existing documents of this type
            var count = _repository.GetObjects<DocumentDto>()
                .Count(d => d.DocumentType.Code == documentTypeCode);

            var documentNumber = $"{documentTypeCode}-{DateTime.Now:yyyy}-{(count + 1):D3}";

            // Ensure uniqueness
            while (_repository.FindObject<DocumentDto>(d => d.DocumentNumber == documentNumber) != null)
            {
                count++;
                documentNumber = $"{documentTypeCode}-{DateTime.Now:yyyy}-{(count + 1):D3}";
            }

            return await Task.FromResult(documentNumber);
        }

        /// <summary>
        /// Adds basic document totals based on the operation type
        /// </summary>
        /// <param name="document">Document to add totals to</param>
        /// <param name="operation">Operation type</param>
        private async Task AddBasicDocumentTotalsAsync(IDocument document, DocumentOperation operation)
        {
            // Calculate subtotal from existing totals (excluding taxes)
            var subtotal = document.DocumentTotals
                .Where(dt => !dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .Sum(dt => dt.Total);

            if (subtotal <= 0)
            {
                // If no subtotal exists, we might need to calculate from document lines
                // For now, we'll assume the document already has the necessary totals
                return;
            }

            // Add main document total based on operation
            string mainConcept;
            string debitAccount = "";
            string creditAccount = "";

            switch (operation)
            {
                case DocumentOperation.Sale:
                    mainConcept = "Sales";
                    debitAccount = "ACCOUNTS_RECEIVABLE"; // Will be mapped by accounting profiles
                    creditAccount = "SALES_REVENUE";
                    break;

                case DocumentOperation.Purchase:
                    mainConcept = "Purchase";
                    debitAccount = "INVENTORY";
                    creditAccount = "ACCOUNTS_PAYABLE";
                    break;

                case DocumentOperation.Return:
                    mainConcept = "Return";
                    // Reverse of the original operation
                    break;

                default:
                    mainConcept = "Subtotal";
                    break;
            }

            // Check if main concept total already exists
            var existingMainTotal = document.DocumentTotals
                .FirstOrDefault(dt => dt.Concept == mainConcept);

            if (existingMainTotal == null && subtotal > 0)
            {
                var mainTotal = _repository.CreateObject<DocumentTotalDto>();
                mainTotal.Concept = mainConcept;
                mainTotal.Total = subtotal;
                mainTotal.DebitAccountCode = debitAccount;
                mainTotal.CreditAccountCode = creditAccount;
                mainTotal.IncludeInTransaction = true;

                document.DocumentTotals.Add(mainTotal);
            }

            await Task.CompletedTask;
        }

        #endregion
    }
}