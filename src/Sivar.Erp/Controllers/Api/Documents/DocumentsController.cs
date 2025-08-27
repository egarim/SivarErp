using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Application.Services.Documents;
using Sivar.Erp.Core.Domain.Entities.Documents;
using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Controllers.Api.Documents
{
    /// <summary>
    /// API controller for document management operations
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(
            IDocumentService documentService,
            ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        /// <summary>
        /// Get documents with optional filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<DocumentSummaryDto>>> GetDocuments(
            [FromQuery] GetDocumentsQuery query)
        {
            try
            {
                var companyId = GetCompanyId();
                var (documents, totalCount) = await _documentService.GetDocumentsAsync(
                    companyId,
                    query.DocumentTypeId,
                    query.Status,
                    query.FromDate,
                    query.ToDate,
                    query.PageSize,
                    query.PageNumber);

                var documentDtos = documents.Select(d => new DocumentSummaryDto
                {
                    Id = d.Oid,
                    DocumentNumber = d.DocumentNumber,
                    DocumentDate = d.DocumentDate,
                    DocumentTime = d.DocumentTime,
                    DocumentTypeName = d.DocumentType?.Name,
                    BusinessEntityName = d.BusinessEntity?.Name,
                    Status = d.Status,
                    CurrencyCode = d.CurrencyCode,
                    TotalAmount = d.TotalAmount,
                    IsPosted = d.IsPosted,
                    LineCount = d.LineCount,
                    CreatedAt = d.CreatedAt,
                    CreatedBy = d.CreatedBy
                }).ToList();

                var response = new PagedResponse<DocumentSummaryDto>
                {
                    Data = documentDtos,
                    TotalCount = totalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents");
                return StatusCode(500, new { message = "An error occurred while retrieving documents" });
            }
        }

        /// <summary>
        /// Get a specific document by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DocumentDto>> GetDocument(Guid id)
        {
            try
            {
                var companyId = GetCompanyId();
                var userId = GetUserId();
                var document = await _documentService.GetDocumentAsync(id, userId, companyId);

                if (document == null)
                    return NotFound(new { message = "Document not found" });

                var documentDto = MapToDocumentDto(document);
                return Ok(documentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document {DocumentId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the document" });
            }
        }

        /// <summary>
        /// Get a document by document number
        /// </summary>
        [HttpGet("by-number/{documentNumber}")]
        public async Task<ActionResult<DocumentDto>> GetDocumentByNumber(string documentNumber)
        {
            try
            {
                var companyId = GetCompanyId();
                var document = await _documentService.GetDocumentByNumberAsync(documentNumber, companyId);

                if (document == null)
                    return NotFound(new { message = "Document not found" });

                var documentDto = MapToDocumentDto(document);
                return Ok(documentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document by number {DocumentNumber}", documentNumber);
                return StatusCode(500, new { message = "An error occurred while retrieving the document" });
            }
        }

        /// <summary>
        /// Create a new document
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DocumentDto>> CreateDocument([FromBody] CreateDocumentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetUserId();
                var companyId = GetCompanyId();
                var branchId = GetBranchId();

                var document = new Document
                {
                    DocumentTypeId = request.DocumentTypeId,
                    DocumentDate = request.DocumentDate,
                    DocumentTime = request.DocumentTime ?? TimeOnly.FromDateTime(DateTime.Now),
                    BusinessEntityId = request.BusinessEntityId,
                    Status = request.Status ?? DocumentStatus.Draft,
                    CurrencyCode = request.CurrencyCode ?? "USD",
                    ExchangeRate = request.ExchangeRate ?? 1.0m,
                    Remarks = request.Remarks,
                    DueDate = request.DueDate,
                    CompanyId = companyId,
                    BranchId = branchId
                };

                // Add lines if provided
                if (request.Lines?.Any() == true)
                {
                    foreach (var lineRequest in request.Lines)
                    {
                        var line = new DocumentLine
                        {
                            LineNumber = lineRequest.LineNumber,
                            ItemId = lineRequest.ItemId,
                            ItemCode = lineRequest.ItemCode,
                            Description = lineRequest.Description,
                            Quantity = lineRequest.Quantity,
                            UnitOfMeasure = lineRequest.UnitOfMeasure,
                            UnitPrice = lineRequest.UnitPrice,
                            DiscountPercent = lineRequest.DiscountPercent ?? 0,
                            TaxPercent = lineRequest.TaxPercent ?? 0,
                            CurrencyCode = lineRequest.CurrencyCode ?? document.CurrencyCode,
                            ExchangeRate = lineRequest.ExchangeRate ?? document.ExchangeRate,
                            Notes = lineRequest.Notes
                        };

                        document.AddLine(line);
                    }
                }

                var createdDocument = await _documentService.CreateDocumentAsync(document, userId);
                var documentDto = MapToDocumentDto(createdDocument);

                return CreatedAtAction(nameof(GetDocument), new { id = documentDto.Id }, documentDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document");
                return StatusCode(500, new { message = "An error occurred while creating the document" });
            }
        }

        /// <summary>
        /// Update an existing document
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<DocumentDto>> UpdateDocument(Guid id, [FromBody] UpdateDocumentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetUserId();
                var companyId = GetCompanyId();

                // Get existing document
                var existingDocument = await _documentService.GetDocumentAsync(id, userId, companyId);
                if (existingDocument == null)
                    return NotFound(new { message = "Document not found" });

                // Update properties
                existingDocument.DocumentDate = request.DocumentDate;
                existingDocument.DocumentTime = request.DocumentTime ?? existingDocument.DocumentTime;
                existingDocument.BusinessEntityId = request.BusinessEntityId;
                existingDocument.Status = request.Status ?? existingDocument.Status;
                existingDocument.CurrencyCode = request.CurrencyCode ?? existingDocument.CurrencyCode;
                existingDocument.ExchangeRate = request.ExchangeRate ?? existingDocument.ExchangeRate;
                existingDocument.Remarks = request.Remarks;
                existingDocument.DueDate = request.DueDate;

                // Update lines if provided
                if (request.Lines?.Any() == true)
                {
                    existingDocument.Lines.Clear();
                    foreach (var lineRequest in request.Lines)
                    {
                        var line = new DocumentLine
                        {
                            LineNumber = lineRequest.LineNumber,
                            ItemId = lineRequest.ItemId,
                            ItemCode = lineRequest.ItemCode,
                            Description = lineRequest.Description,
                            Quantity = lineRequest.Quantity,
                            UnitOfMeasure = lineRequest.UnitOfMeasure,
                            UnitPrice = lineRequest.UnitPrice,
                            DiscountPercent = lineRequest.DiscountPercent ?? 0,
                            TaxPercent = lineRequest.TaxPercent ?? 0,
                            CurrencyCode = lineRequest.CurrencyCode ?? existingDocument.CurrencyCode,
                            ExchangeRate = lineRequest.ExchangeRate ?? existingDocument.ExchangeRate,
                            Notes = lineRequest.Notes
                        };

                        existingDocument.AddLine(line);
                    }
                }

                var updatedDocument = await _documentService.UpdateDocumentAsync(existingDocument, userId);
                var documentDto = MapToDocumentDto(updatedDocument);

                return Ok(documentDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document {DocumentId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the document" });
            }
        }

        /// <summary>
        /// Delete a document
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteDocument(Guid id)
        {
            try
            {
                var userId = GetUserId();
                await _documentService.DeleteDocumentAsync(id, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the document" });
            }
        }

        /// <summary>
        /// Post a document
        /// </summary>
        [HttpPost("{id:guid}/post")]
        public async Task<ActionResult<DocumentDto>> PostDocument(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var document = await _documentService.PostDocumentAsync(id, userId);
                var documentDto = MapToDocumentDto(document);
                return Ok(documentDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting document {DocumentId}", id);
                return StatusCode(500, new { message = "An error occurred while posting the document" });
            }
        }

        /// <summary>
        /// Reverse posting of a document
        /// </summary>
        [HttpPost("{id:guid}/reverse-posting")]
        public async Task<ActionResult<DocumentDto>> ReversePosting(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var document = await _documentService.ReversePostingAsync(id, userId);
                var documentDto = MapToDocumentDto(document);
                return Ok(documentDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reversing document posting {DocumentId}", id);
                return StatusCode(500, new { message = "An error occurred while reversing the document posting" });
            }
        }

        /// <summary>
        /// Change document status
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        public async Task<ActionResult<DocumentDto>> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetUserId();
                var document = await _documentService.ChangeStatusAsync(id, request.Status, userId);
                var documentDto = MapToDocumentDto(document);
                return Ok(documentDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing document status {DocumentId}", id);
                return StatusCode(500, new { message = "An error occurred while changing the document status" });
            }
        }

        /// <summary>
        /// Duplicate a document
        /// </summary>
        [HttpPost("{id:guid}/duplicate")]
        public async Task<ActionResult<DocumentDto>> DuplicateDocument(Guid id, [FromBody] DuplicateDocumentRequest? request = null)
        {
            try
            {
                var userId = GetUserId();
                var document = await _documentService.DuplicateDocumentAsync(id, request?.NewDocumentTypeId, userId);
                var documentDto = MapToDocumentDto(document);
                return CreatedAtAction(nameof(GetDocument), new { id = documentDto.Id }, documentDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating document {DocumentId}", id);
                return StatusCode(500, new { message = "An error occurred while duplicating the document" });
            }
        }

        /// <summary>
        /// Get document statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<DocumentStatistics>> GetStatistics([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            try
            {
                var companyId = GetCompanyId();
                var statistics = await _documentService.GetDocumentStatisticsAsync(companyId, fromDate, toDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// Validate a document
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<DocumentValidationResult>> ValidateDocument([FromBody] Document document)
        {
            try
            {
                var result = await _documentService.ValidateDocumentAsync(document);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating document");
                return StatusCode(500, new { message = "An error occurred while validating the document" });
            }
        }

        #region Private Helper Methods

        private Guid GetCompanyId()
        {
            var companyIdClaim = User.FindFirst("company_id")?.Value;
            return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : Guid.Empty;
        }

        private Guid GetBranchId()
        {
            var branchIdClaim = User.FindFirst("branch_id")?.Value;
            return Guid.TryParse(branchIdClaim, out var branchId) ? branchId : Guid.Empty;
        }

        private string GetUserId()
        {
            return User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "system";
        }

        private DocumentDto MapToDocumentDto(Document document)
        {
            return new DocumentDto
            {
                Id = document.Oid,
                DocumentNumber = document.DocumentNumber,
                DocumentDate = document.DocumentDate,
                DocumentTime = document.DocumentTime,
                DocumentTypeId = document.DocumentTypeId,
                DocumentTypeName = document.DocumentType?.Name,
                BusinessEntityId = document.BusinessEntityId,
                BusinessEntityName = document.BusinessEntity?.Name,
                Status = document.Status,
                CurrencyCode = document.CurrencyCode,
                ExchangeRate = document.ExchangeRate,
                Remarks = document.Remarks,
                DueDate = document.DueDate,
                SubTotal = document.SubTotal,
                TotalDiscount = document.TotalDiscount,
                TotalTax = document.TotalTax,
                TotalAmount = document.TotalAmount,
                IsPosted = document.IsPosted,
                PostedAt = document.PostedAt,
                PostedBy = document.PostedBy,
                LineCount = document.LineCount,
                Lines = document.Lines.Select(l => new DocumentLineDto
                {
                    Id = l.Oid,
                    LineNumber = l.LineNumber,
                    ItemId = l.ItemId,
                    ItemCode = l.ItemCode,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    UnitOfMeasure = l.UnitOfMeasure,
                    UnitPrice = l.UnitPrice,
                    DiscountPercent = l.DiscountPercent,
                    DiscountAmount = l.DiscountAmount,
                    TaxPercent = l.TaxPercent,
                    TaxAmount = l.TaxAmount,
                    LineTotal = l.LineTotal,
                    CurrencyCode = l.CurrencyCode,
                    ExchangeRate = l.ExchangeRate,
                    Notes = l.Notes
                }).ToList(),
                Totals = document.Totals.Select(t => new DocumentTotalDto
                {
                    Id = t.Oid,
                    TotalType = t.TotalType,
                    Description = t.Description,
                    Rate = t.Rate,
                    BaseAmount = t.BaseAmount,
                    TotalAmount = t.TotalAmount,
                    CurrencyCode = t.CurrencyCode,
                    ExchangeRate = t.ExchangeRate
                }).ToList(),
                CreatedAt = document.CreatedAt,
                CreatedBy = document.CreatedBy,
                ModifiedAt = document.ModifiedAt,
                ModifiedBy = document.ModifiedBy,
                Version = document.Version
            };
        }

        #endregion
    }

    #region Request/Response DTOs

    public class GetDocumentsQuery
    {
        public Guid? DocumentTypeId { get; set; }
        public string? Status { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public int PageSize { get; set; } = 50;
        public int PageNumber { get; set; } = 1;
    }

    public class CreateDocumentRequest
    {
        [Required]
        public Guid DocumentTypeId { get; set; }
        
        [Required]
        public DateOnly DocumentDate { get; set; }
        
        public TimeOnly? DocumentTime { get; set; }
        public Guid? BusinessEntityId { get; set; }
        public string? Status { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? Remarks { get; set; }
        public DateOnly? DueDate { get; set; }
        public List<CreateDocumentLineRequest>? Lines { get; set; }
    }

    public class CreateDocumentLineRequest
    {
        [Required]
        public int LineNumber { get; set; }
        
        public Guid? ItemId { get; set; }
        public string? ItemCode { get; set; }
        
        [Required]
        public string Description { get; set; } = "";
        
        [Required]
        public decimal Quantity { get; set; }
        
        public string? UnitOfMeasure { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        public decimal? DiscountPercent { get; set; }
        public decimal? TaxPercent { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateDocumentRequest
    {
        [Required]
        public DateOnly DocumentDate { get; set; }
        
        public TimeOnly? DocumentTime { get; set; }
        public Guid? BusinessEntityId { get; set; }
        public string? Status { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? Remarks { get; set; }
        public DateOnly? DueDate { get; set; }
        public List<CreateDocumentLineRequest>? Lines { get; set; }
    }

    public class ChangeStatusRequest
    {
        [Required]
        public string Status { get; set; } = "";
    }

    public class DuplicateDocumentRequest
    {
        public Guid? NewDocumentTypeId { get; set; }
    }

    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    #endregion
}
