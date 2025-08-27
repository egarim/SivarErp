using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Application.Services.Purchasing;
using Sivar.Erp.Core.Shared.Dtos.Purchasing;

namespace Sivar.Erp.Core.Api.Controllers;

/// <summary>
/// API controller for purchase order management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly ILogger<PurchaseOrdersController> _logger;

    public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService, ILogger<PurchaseOrdersController> logger)
    {
        _purchaseOrderService = purchaseOrderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all purchase orders for the current company
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetPurchaseOrders(
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
            var purchaseOrders = await _purchaseOrderService.GetPurchaseOrdersAsync(
                companyId, page, pageSize, searchTerm, startDate, endDate, status);
            return Ok(purchaseOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific purchase order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseOrderDto>> GetPurchaseOrder(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var purchaseOrder = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id, companyId);
            
            if (purchaseOrder == null)
                return NotFound();

            return Ok(purchaseOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase order {PurchaseOrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new purchase order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PurchaseOrderDto>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var purchaseOrder = await _purchaseOrderService.CreatePurchaseOrderAsync(createDto, companyId, userId);
            return CreatedAtAction(nameof(GetPurchaseOrder), new { id = purchaseOrder.Id }, purchaseOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase order");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing purchase order
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PurchaseOrderDto>> UpdatePurchaseOrder(Guid id, [FromBody] UpdatePurchaseOrderDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var purchaseOrder = await _purchaseOrderService.UpdatePurchaseOrderAsync(id, updateDto, companyId, userId);
            
            if (purchaseOrder == null)
                return NotFound();

            return Ok(purchaseOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating purchase order {PurchaseOrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Approve a purchase order
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<PurchaseOrderDto>> ApprovePurchaseOrder(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var purchaseOrder = await _purchaseOrderService.ApprovePurchaseOrderAsync(id, companyId, userId);
            
            if (purchaseOrder == null)
                return NotFound("Purchase order not found or cannot be approved");

            return Ok(purchaseOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving purchase order {PurchaseOrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Receive goods from a purchase order
    /// </summary>
    [HttpPost("{id:guid}/receive")]
    public async Task<ActionResult<PurchaseOrderDto>> ReceiveGoods(Guid id, [FromBody] ReceiveGoodsDto receiveDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var purchaseOrder = await _purchaseOrderService.ReceiveGoodsAsync(id, receiveDto, companyId, userId);
            
            if (purchaseOrder == null)
                return NotFound("Purchase order not found or cannot receive goods");

            return Ok(purchaseOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving goods for purchase order {PurchaseOrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Convert purchase order to purchase invoice
    /// </summary>
    [HttpPost("{id:guid}/convert-to-invoice")]
    public async Task<ActionResult<PurchaseInvoiceDto>> ConvertToInvoice(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var invoice = await _purchaseOrderService.ConvertToPurchaseInvoiceAsync(id, companyId, userId);
            
            if (invoice == null)
                return NotFound("Purchase order not found or cannot be converted");

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting purchase order {PurchaseOrderId} to invoice", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Cancel a purchase order
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<PurchaseOrderDto>> CancelPurchaseOrder(Guid id, [FromBody] string? reason = null)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            
            var purchaseOrder = await _purchaseOrderService.CancelPurchaseOrderAsync(id, companyId, userId, reason);
            
            if (purchaseOrder == null)
                return NotFound("Purchase order not found or cannot be cancelled");

            return Ok(purchaseOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling purchase order {PurchaseOrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a purchase order
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeletePurchaseOrder(Guid id)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var result = await _purchaseOrderService.DeletePurchaseOrderAsync(id, companyId);
            
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting purchase order {PurchaseOrderId}", id);
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
