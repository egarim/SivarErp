using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Application.Services.Sales;
using Sivar.Erp.Core.Shared.Dtos.Sales;

namespace Sivar.Erp.Core.Api.Controllers;

/// <summary>
/// API controller for sales invoice management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(IInvoiceService invoiceService, ILogger<InvoicesController> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    /// <summary>
    /// Get all invoices for the current company
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoices(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var invoices = await _invoiceService.GetInvoicesAsync(companyId, page, pageSize, searchTerm, startDate, endDate, status);
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific invoice by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id, companyId);
            
            if (invoice == null)
                return NotFound();

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice {InvoiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new invoice
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] CreateInvoiceDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var invoice = await _invoiceService.CreateInvoiceAsync(createDto, companyId, userId);
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing invoice
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> UpdateInvoice(Guid id, [FromBody] UpdateInvoiceDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var invoice = await _invoiceService.UpdateInvoiceAsync(id, updateDto, companyId, userId);
            
            if (invoice == null)
                return NotFound();

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {InvoiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Post an invoice (finalize it)
    /// </summary>
    [HttpPost("{id:guid}/post")]
    public async Task<ActionResult<InvoiceDto>> PostInvoice(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var invoice = await _invoiceService.PostInvoiceAsync(id, companyId, userId);
            
            if (invoice == null)
                return NotFound("Invoice not found or cannot be posted");

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting invoice {InvoiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Cancel an invoice
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<InvoiceDto>> CancelInvoice(Guid id, [FromBody] string? reason = null)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var invoice = await _invoiceService.CancelInvoiceAsync(id, companyId, userId, reason);
            
            if (invoice == null)
                return NotFound("Invoice not found or cannot be cancelled");

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invoice {InvoiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Generate invoice PDF
    /// </summary>
    [HttpGet("{id:guid}/pdf")]
    public async Task<ActionResult> GetInvoicePdf(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(id, companyId);
            
            if (pdfBytes == null)
                return NotFound();

            return File(pdfBytes, "application/pdf", $"invoice-{id}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for invoice {InvoiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get invoice statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<InvoiceStatisticsDto>> GetInvoiceStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var statistics = await _invoiceService.GetInvoiceStatisticsAsync(companyId, startDate, endDate);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice statistics");
            return StatusCode(500, "Internal server error");
        }
    }

    private Guid GetCurrentCompanyId()
    {
        var companyIdClaim = HttpContext.Request.Headers["X-Company-Id"].FirstOrDefault();
        return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : Guid.Empty;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst("sub")?.Value ?? "anonymous";
    }
}
