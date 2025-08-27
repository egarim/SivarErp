using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Shared.Interfaces;

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
    /// Get all sales orders for a company
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetSalesOrders([FromQuery] Guid companyId)
    {
        try
        {
            if (companyId == Guid.Empty)
            {
                return BadRequest("CompanyId is required");
            }

            var salesOrders = await _salesOrderService.GetAllSalesOrdersAsync(companyId);
            return Ok(salesOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales orders for company {CompanyId}", companyId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get sales order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetSalesOrder(Guid id)
    {
        try
        {
            var salesOrder = await _salesOrderService.GetSalesOrderByIdAsync(id);
            if (salesOrder == null)
            {
                return NotFound($"Sales order with ID {id} not found");
            }
            return Ok(salesOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales order {SalesOrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create new sales order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<object>> CreateSalesOrder([FromBody] CreateSalesOrderRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Sales order data is required");
            }

            var createdSalesOrder = await _salesOrderService.CreateSalesOrderAsync(request);
            return CreatedAtAction(nameof(GetSalesOrder), new { id = Guid.NewGuid() }, createdSalesOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sales order");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update existing sales order
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateSalesOrder(Guid id, [FromBody] UpdateSalesOrderRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Sales order data is required");
            }

            var updatedSalesOrder = await _salesOrderService.UpdateSalesOrderAsync(id, request);
            return Ok(updatedSalesOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sales order {SalesOrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete sales order
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSalesOrder(Guid id, [FromQuery] Guid companyId)
    {
        try
        {
            if (companyId == Guid.Empty)
            {
                return BadRequest("CompanyId is required");
            }

            var success = await _salesOrderService.DeleteSalesOrderAsync(id, companyId);
            if (!success)
            {
                return NotFound($"Sales order with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sales order {SalesOrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

/// <summary>
/// Request model for creating sales orders
/// </summary>
public class CreateSalesOrderRequest
{
    public Guid CompanyId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
}

/// <summary>
/// Request model for updating sales orders
/// </summary>
public class UpdateSalesOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal Amount { get; set; }
}
