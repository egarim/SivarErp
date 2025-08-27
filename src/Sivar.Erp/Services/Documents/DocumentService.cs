using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Core.Application.Services.Documents;
using Sivar.Erp.Core.Domain.Entities.Documents;
using Sivar.Erp.Core.Domain.Interfaces;
using Sivar.Erp.Core.Infrastructure.Data;

namespace Sivar.Erp.Core.Infrastructure.Services.Documents
{
    /// <summary>
    /// Document service implementation with comprehensive business logic
    /// </summary>
    public class DocumentService : IDocumentService
    {
        private readonly ErpDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            ErpDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<DocumentService> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Document> CreateDocumentAsync(Document document, string userId)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            // Validate the document
            var validationResult = await ValidateDocumentAsync(document);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Document validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Set audit fields
            document.CreatedBy = userId;
            document.CreatedAt = DateTime.UtcNow;

            // Generate document number if needed
            if (string.IsNullOrWhiteSpace(document.DocumentNumber))
            {
                var documentType = await _context.DocumentTypes.FindAsync(document.DocumentTypeId);
                if (documentType != null && documentType.AutoGenerateNumbers)
                {
                    document.DocumentNumber = documentType.GenerateNextDocumentNumber();
                    _context.DocumentTypes.Update(documentType);
                }
            }

            // Set tenant context for lines
            foreach (var line in document.Lines)
            {
                line.SetTenantContext(document.CompanyId, document.BranchId);
                line.CreatedBy = userId;
                line.CreatedAt = DateTime.UtcNow;
                line.CalculateTotals();
            }

            // Recalculate document totals
            document.RecalculateTotals();

            _context.Documents.Add(document);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Document created: {DocumentNumber} by user {UserId}", 
                document.DocumentNumber, userId);

            return document;
        }

        public async Task<Document?> GetDocumentAsync(Guid documentId, string userId, Guid companyId)
        {
            var document = await _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.BusinessEntity)
                .Include(d => d.Lines)
                    .ThenInclude(l => l.Item)
                .Include(d => d.Totals)
                .Where(d => d.Oid == documentId && d.CompanyId == companyId && !d.IsDeleted)
                .FirstOrDefaultAsync();

            return document;
        }

        public async Task<Document?> GetDocumentByNumberAsync(string documentNumber, Guid companyId)
        {
            if (string.IsNullOrWhiteSpace(documentNumber))
                return null;

            var document = await _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.BusinessEntity)
                .Include(d => d.Lines)
                    .ThenInclude(l => l.Item)
                .Include(d => d.Totals)
                .Where(d => d.DocumentNumber == documentNumber && d.CompanyId == companyId && !d.IsDeleted)
                .FirstOrDefaultAsync();

            return document;
        }

        public async Task<Document> UpdateDocumentAsync(Document document, string userId)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var existingDocument = await _context.Documents
                .Include(d => d.Lines)
                .Include(d => d.Totals)
                .FirstOrDefaultAsync(d => d.Oid == document.Oid);

            if (existingDocument == null)
                throw new InvalidOperationException("Document not found");

            if (existingDocument.IsPosted)
                throw new InvalidOperationException("Cannot modify posted documents");

            // Validate the updated document
            var validationResult = await ValidateDocumentAsync(document);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Document validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Update document properties
            existingDocument.DocumentDate = document.DocumentDate;
            existingDocument.DocumentTime = document.DocumentTime;
            existingDocument.BusinessEntityId = document.BusinessEntityId;
            existingDocument.Status = document.Status;
            existingDocument.CurrencyCode = document.CurrencyCode;
            existingDocument.ExchangeRate = document.ExchangeRate;
            existingDocument.Remarks = document.Remarks;
            existingDocument.DueDate = document.DueDate;
            existingDocument.MarkAsModified(userId);

            // Update lines (simplified - in production, you'd want more sophisticated change tracking)
            _context.DocumentLines.RemoveRange(existingDocument.Lines);
            foreach (var line in document.Lines)
            {
                line.DocumentId = existingDocument.Oid;
                line.SetTenantContext(existingDocument.CompanyId, existingDocument.BranchId);
                line.CreatedBy = userId;
                line.CreatedAt = DateTime.UtcNow;
                line.CalculateTotals();
                existingDocument.AddLine(line);
            }

            // Recalculate totals
            existingDocument.RecalculateTotals();

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Document updated: {DocumentNumber} by user {UserId}", 
                existingDocument.DocumentNumber, userId);

            return existingDocument;
        }

        public async Task DeleteDocumentAsync(Guid documentId, string userId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                throw new InvalidOperationException("Document not found");

            if (document.IsPosted)
                throw new InvalidOperationException("Cannot delete posted documents");

            document.MarkAsDeleted(userId);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Document deleted: {DocumentNumber} by user {UserId}", 
                document.DocumentNumber, userId);
        }

        public async Task<(IEnumerable<Document> Documents, int TotalCount)> GetDocumentsAsync(
            Guid companyId,
            Guid? documentTypeId = null,
            string? status = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            int pageSize = 50,
            int pageNumber = 1)
        {
            var query = _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.BusinessEntity)
                .Where(d => d.CompanyId == companyId && !d.IsDeleted);

            if (documentTypeId.HasValue)
                query = query.Where(d => d.DocumentTypeId == documentTypeId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(d => d.Status == status);

            if (fromDate.HasValue)
                query = query.Where(d => d.DocumentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(d => d.DocumentDate <= toDate.Value);

            var totalCount = await query.CountAsync();

            var documents = await query
                .OrderByDescending(d => d.DocumentDate)
                .ThenByDescending(d => d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (documents, totalCount);
        }

        public async Task<Document> AddLineAsync(Guid documentId, DocumentLine line, string userId)
        {
            var document = await _context.Documents
                .Include(d => d.Lines)
                .FirstOrDefaultAsync(d => d.Oid == documentId);

            if (document == null)
                throw new InvalidOperationException("Document not found");

            if (document.IsPosted)
                throw new InvalidOperationException("Cannot modify posted documents");

            line.SetTenantContext(document.CompanyId, document.BranchId);
            line.CreatedBy = userId;
            line.CreatedAt = DateTime.UtcNow;
            line.CalculateTotals();

            document.AddLine(line);
            document.MarkAsModified(userId);

            await _unitOfWork.SaveChangesAsync();

            return document;
        }

        public async Task<Document> UpdateLineAsync(Guid documentId, DocumentLine line, string userId)
        {
            var document = await _context.Documents
                .Include(d => d.Lines)
                .FirstOrDefaultAsync(d => d.Oid == documentId);

            if (document == null)
                throw new InvalidOperationException("Document not found");

            if (document.IsPosted)
                throw new InvalidOperationException("Cannot modify posted documents");

            var existingLine = document.Lines.FirstOrDefault(l => l.Oid == line.Oid);
            if (existingLine == null)
                throw new InvalidOperationException("Document line not found");

            // Update line properties
            existingLine.ItemId = line.ItemId;
            existingLine.ItemCode = line.ItemCode;
            existingLine.Description = line.Description;
            existingLine.Quantity = line.Quantity;
            existingLine.UnitOfMeasure = line.UnitOfMeasure;
            existingLine.UnitPrice = line.UnitPrice;
            existingLine.DiscountPercent = line.DiscountPercent;
            existingLine.DiscountAmount = line.DiscountAmount;
            existingLine.TaxPercent = line.TaxPercent;
            existingLine.Notes = line.Notes;
            existingLine.MarkAsModified(userId);
            existingLine.CalculateTotals();

            document.RecalculateTotals();
            document.MarkAsModified(userId);

            await _unitOfWork.SaveChangesAsync();

            return document;
        }

        public async Task<Document> RemoveLineAsync(Guid documentId, Guid lineId, string userId)
        {
            var document = await _context.Documents
                .Include(d => d.Lines)
                .FirstOrDefaultAsync(d => d.Oid == documentId);

            if (document == null)
                throw new InvalidOperationException("Document not found");

            if (document.IsPosted)
                throw new InvalidOperationException("Cannot modify posted documents");

            var line = document.Lines.FirstOrDefault(l => l.Oid == lineId);
            if (line == null)
                throw new InvalidOperationException("Document line not found");

            document.RemoveLine(line);
            document.MarkAsModified(userId);

            await _unitOfWork.SaveChangesAsync();

            return document;
        }

        public async Task<Document> PostDocumentAsync(Guid documentId, string userId)
        {
            var document = await _context.Documents
                .Include(d => d.Lines)
                .FirstOrDefaultAsync(d => d.Oid == documentId);

            if (document == null)
                throw new InvalidOperationException("Document not found");

            document.Post(userId);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Document posted: {DocumentNumber} by user {UserId}", 
                document.DocumentNumber, userId);

            return document;
        }

        public async Task<Document> ReversePostingAsync(Guid documentId, string userId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                throw new InvalidOperationException("Document not found");

            document.ReversePosting(userId);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Document posting reversed: {DocumentNumber} by user {UserId}", 
                document.DocumentNumber, userId);

            return document;
        }

        public async Task<Document> ChangeStatusAsync(Guid documentId, string newStatus, string userId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                throw new InvalidOperationException("Document not found");

            document.Status = newStatus;
            document.MarkAsModified(userId);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Document status changed: {DocumentNumber} to {Status} by user {UserId}", 
                document.DocumentNumber, newStatus, userId);

            return document;
        }

        public async Task<DocumentStatistics> GetDocumentStatisticsAsync(Guid companyId, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            var query = _context.Documents.Where(d => d.CompanyId == companyId && !d.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(d => d.DocumentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(d => d.DocumentDate <= toDate.Value);

            var stats = new DocumentStatistics
            {
                CompanyId = companyId,
                TotalDocuments = await query.CountAsync(),
                DraftDocuments = await query.CountAsync(d => d.Status == DocumentStatus.Draft),
                PendingDocuments = await query.CountAsync(d => d.Status == DocumentStatus.PendingApproval),
                ApprovedDocuments = await query.CountAsync(d => d.Status == DocumentStatus.Approved),
                PostedDocuments = await query.CountAsync(d => d.Status == DocumentStatus.Posted),
                TotalAmount = await query.SumAsync(d => d.TotalAmount)
            };

            // Get statistics by document type
            var typeStats = await query
                .Include(d => d.DocumentType)
                .GroupBy(d => d.DocumentType!.Name)
                .Select(g => new { Type = g.Key, Count = g.Count(), Amount = g.Sum(d => d.TotalAmount) })
                .ToListAsync();

            foreach (var stat in typeStats)
            {
                stats.DocumentsByType[stat.Type] = stat.Count;
                stats.AmountsByType[stat.Type] = stat.Amount;
            }

            return stats;
        }

        public async Task<DocumentValidationResult> ValidateDocumentAsync(Document document)
        {
            var result = new DocumentValidationResult { IsValid = true };

            // Basic validation
            if (!document.IsValid())
            {
                result.IsValid = false;
                result.Errors.Add("Document basic validation failed");
            }

            // Check document type exists
            var documentType = await _context.DocumentTypes.FindAsync(document.DocumentTypeId);
            if (documentType == null)
            {
                result.IsValid = false;
                result.Errors.Add("Invalid document type");
            }
            else
            {
                // Check document type requirements
                if (documentType.RequiresLines && !document.Lines.Any())
                {
                    result.IsValid = false;
                    result.Errors.Add("Document type requires lines");
                }

                if (documentType.RequiresBusinessEntity && !document.BusinessEntityId.HasValue)
                {
                    result.IsValid = false;
                    result.Errors.Add("Document type requires a business entity");
                }
            }

            // Check business entity exists if specified
            if (document.BusinessEntityId.HasValue)
            {
                var businessEntity = await _context.BusinessEntities.FindAsync(document.BusinessEntityId.Value);
                if (businessEntity == null)
                {
                    result.IsValid = false;
                    result.Errors.Add("Invalid business entity");
                }
            }

            // Validate lines
            foreach (var line in document.Lines)
            {
                if (!line.IsValid())
                {
                    result.IsValid = false;
                    result.Errors.Add($"Line {line.LineNumber} validation failed");
                }

                if (line.ItemId.HasValue)
                {
                    var item = await _context.Items.FindAsync(line.ItemId.Value);
                    if (item == null)
                    {
                        result.Warnings.Add($"Line {line.LineNumber} references invalid item");
                    }
                }
            }

            // Check for duplicate document number
            if (!string.IsNullOrWhiteSpace(document.DocumentNumber))
            {
                var existingDoc = await _context.Documents
                    .Where(d => d.DocumentNumber == document.DocumentNumber && 
                               d.CompanyId == document.CompanyId && 
                               d.Oid != document.Oid && 
                               !d.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingDoc != null)
                {
                    result.IsValid = false;
                    result.Errors.Add("Document number already exists");
                }
            }

            return result;
        }

        public async Task<Document> DuplicateDocumentAsync(Guid sourceDocumentId, Guid? newDocumentTypeId, string userId)
        {
            var sourceDocument = await _context.Documents
                .Include(d => d.Lines)
                    .ThenInclude(l => l.Item)
                .FirstOrDefaultAsync(d => d.Oid == sourceDocumentId);

            if (sourceDocument == null)
                throw new InvalidOperationException("Source document not found");

            var newDocument = new Document
            {
                DocumentTypeId = newDocumentTypeId ?? sourceDocument.DocumentTypeId,
                DocumentDate = DateOnly.FromDateTime(DateTime.Today),
                DocumentTime = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntityId = sourceDocument.BusinessEntityId,
                Status = DocumentStatus.Draft,
                CurrencyCode = sourceDocument.CurrencyCode,
                ExchangeRate = sourceDocument.ExchangeRate,
                Remarks = sourceDocument.Remarks,
                DueDate = sourceDocument.DueDate,
                CompanyId = sourceDocument.CompanyId,
                BranchId = sourceDocument.BranchId
            };

            // Copy lines
            foreach (var sourceLine in sourceDocument.Lines)
            {
                var newLine = new DocumentLine
                {
                    LineNumber = sourceLine.LineNumber,
                    ItemId = sourceLine.ItemId,
                    ItemCode = sourceLine.ItemCode,
                    Description = sourceLine.Description,
                    Quantity = sourceLine.Quantity,
                    UnitOfMeasure = sourceLine.UnitOfMeasure,
                    UnitPrice = sourceLine.UnitPrice,
                    DiscountPercent = sourceLine.DiscountPercent,
                    TaxPercent = sourceLine.TaxPercent,
                    CurrencyCode = sourceLine.CurrencyCode,
                    ExchangeRate = sourceLine.ExchangeRate,
                    Notes = sourceLine.Notes
                };

                newLine.CalculateTotals();
                newDocument.AddLine(newLine);
            }

            return await CreateDocumentAsync(newDocument, userId);
        }
    }
}
