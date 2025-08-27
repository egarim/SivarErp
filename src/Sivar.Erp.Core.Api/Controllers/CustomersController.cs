using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Application.DTOs.Sales;
using Sivar.Erp.Core.Application.Services.Sales;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Api.Controllers;

/// <summary>
/// API Controller for Customer management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(CustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint for Customers API
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "customers", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Get all customers for the company
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] bool activeOnly = true)
    {
        try
        {
            // TODO: Get company ID from authenticated user context
            var companyId = Guid.Parse("12345678-1234-5678-9abc-123456789012"); // Temporary hardcoded

            var result = await _customerService.GetCustomersAsync(companyId, activeOnly);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customers by type
    /// </summary>
    [HttpGet("by-type/{customerType}")]
    public async Task<IActionResult> GetCustomersByType(CustomerType customerType)
    {
        try
        {
            // TODO: Get company ID from authenticated user context
            var companyId = Guid.Parse("12345678-1234-5678-9abc-123456789012"); // Temporary hardcoded

            var result = await _customerService.GetCustomersByTypeAsync(companyId, customerType);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers by type: {CustomerType}", customerType);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(Guid id)
    {
        try
        {
            // TODO: Get company ID from authenticated user context
            var companyId = Guid.Parse("12345678-1234-5678-9abc-123456789012"); // Temporary hardcoded

            var result = await _customerService.GetCustomerByIdAsync(companyId, id);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer: {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customer by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetCustomerByCode(string code)
    {
        try
        {
            // TODO: Get company ID from authenticated user context
            var companyId = Guid.Parse("12345678-1234-5678-9abc-123456789012"); // Temporary hardcoded

            var result = await _customerService.GetCustomerByCodeAsync(companyId, code);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer by code: {CustomerCode}", code);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customer balance
    /// </summary>
    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetCustomerBalance(Guid id)
    {
        try
        {
            // TODO: Get company ID from authenticated user context
            var companyId = Guid.Parse("12345678-1234-5678-9abc-123456789012"); // Temporary hardcoded

            var result = await _customerService.GetCustomerBalanceAsync(companyId, id);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer balance: {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // TODO: Get company ID from authenticated user context
            var companyId = Guid.Parse("12345678-1234-5678-9abc-123456789012"); // Temporary hardcoded

            var result = await _customerService.CreateCustomerAsync(companyId, dto);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetCustomer), 
                    new { id = result.Data!.Id }, 
                    result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer: {CustomerCode}", dto.Code);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // TODO: Get company ID from authenticated user context
            var companyId = Guid.Parse("12345678-1234-5678-9abc-123456789012"); // Temporary hardcoded

            var result = await _customerService.UpdateCustomerAsync(companyId, id, dto);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer: {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a customer (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        try
        {
            // TODO: Get company ID from authenticated user context
            var companyId = Guid.Parse("12345678-1234-5678-9abc-123456789012"); // Temporary hardcoded

            var result = await _customerService.DeleteCustomerAsync(companyId, id);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer: {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
