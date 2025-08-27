using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Application.Services.Sales;
using Sivar.Erp.Core.Shared.Dtos.Sales;

namespace Sivar.Erp.Core.Api.Controllers;

/// <summary>
/// API controller for sales order management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrderService _salesOrderService;
    private readonly ILogger<SalesOrdersController> _logger;

    public SalesOrdersController(ISalesOrderService salesOrderService, ILogger<SalesOrdersController> logger)
    {
        _salesOrderService = salesOrderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all sales orders for the current company
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesOrderDto>>> GetSalesOrders(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var salesOrders = await _salesOrderService.GetSalesOrdersAsync(companyId, page, pageSize, searchTerm, startDate, endDate);
            return Ok(salesOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific sales order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalesOrderDto>> GetSalesOrder(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var salesOrder = await _salesOrderService.GetSalesOrderByIdAsync(id, companyId);
            
            if (salesOrder == null)
                return NotFound();

            return Ok(salesOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales order {SalesOrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new sales order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SalesOrderDto>> CreateSalesOrder([FromBody] CreateSalesOrderDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var salesOrder = await _salesOrderService.CreateSalesOrderAsync(createDto, companyId, userId);
            return CreatedAtAction(nameof(GetSalesOrder), new { id = salesOrder.Id }, salesOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sales order");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing sales order
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SalesOrderDto>> UpdateSalesOrder(Guid id, [FromBody] UpdateSalesOrderDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var salesOrder = await _salesOrderService.UpdateSalesOrderAsync(id, updateDto, companyId, userId);
            
            if (salesOrder == null)
                return NotFound();

            return Ok(salesOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sales order {SalesOrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Convert sales order to invoice
    /// </summary>
    [HttpPost("{id:guid}/convert-to-invoice")]
    public async Task<ActionResult<InvoiceDto>> ConvertToInvoice(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var invoice = await _salesOrderService.ConvertToInvoiceAsync(id, companyId, userId);
            
            if (invoice == null)
                return NotFound("Sales order not found or cannot be converted");

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting sales order {SalesOrderId} to invoice", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a sales order
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteSalesOrder(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var result = await _salesOrderService.DeleteSalesOrderAsync(id, companyId);
            
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sales order {SalesOrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private Guid GetCurrentCompanyId()
    {
        // Extract company ID from JWT token or request headers
        var companyIdClaim = HttpContext.Request.Headers["X-Company-Id"].FirstOrDefault();
        return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : Guid.Empty;
    }

    private string GetCurrentUserId()
    {
        // Extract user ID from JWT token
        return User.FindFirst("sub")?.Value ?? "anonymous";
    }
}
