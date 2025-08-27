using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Shared.Interfaces;
using Sivar.Erp.Core.Shared.DTOs;

namespace Sivar.Erp.Core.Api.Controllers;

/// <summary>
/// API controller for inventory management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IStockLevelService _stockLevelService;
    private readonly IInventoryTransactionService _transactionService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        IInventoryService inventoryService,
        IStockLevelService stockLevelService,
        IInventoryTransactionService transactionService,
        ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _stockLevelService = stockLevelService;
        _transactionService = transactionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        try
        {
            var products = await _inventoryService.GetAllProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("products/{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            var product = await _inventoryService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost("products")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] ProductDto productDto)
    {
        try
        {
            if (productDto == null)
            {
                return BadRequest("Product data is required");
            }

            var createdProduct = await _inventoryService.CreateProductAsync(productDto);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update existing product
    /// </summary>
    [HttpPut("products/{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] ProductDto productDto)
    {
        try
        {
            if (productDto == null)
            {
                return BadRequest("Product data is required");
            }

            if (id != productDto.Id)
            {
                return BadRequest("Product ID mismatch");
            }

            var updatedProduct = await _inventoryService.UpdateProductAsync(productDto);
            return Ok(updatedProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete product
    /// </summary>
    [HttpDelete("products/{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        try
        {
            var success = await _inventoryService.DeleteProductAsync(id);
            if (!success)
            {
                return NotFound($"Product with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get stock level for product
    /// </summary>
    [HttpGet("products/{productId}/stock")]
    public async Task<ActionResult<int>> GetStockLevel(int productId)
    {
        try
        {
            var stockLevel = await _stockLevelService.GetStockLevelAsync(productId);
            return Ok(stockLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock level for product {ProductId}", productId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update stock level for product
    /// </summary>
    [HttpPut("products/{productId}/stock")]
    public async Task<ActionResult> UpdateStockLevel(int productId, [FromBody] UpdateStockRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Stock update data is required");
            }

            var success = await _stockLevelService.UpdateStockLevelAsync(productId, request.Quantity);
            if (!success)
            {
                return NotFound($"Product with ID {productId} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock level for product {ProductId}", productId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Process inventory transaction
    /// </summary>
    [HttpPost("transactions")]
    public async Task<ActionResult> ProcessTransaction([FromBody] ProcessTransactionRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Transaction data is required");
            }

            var success = await _transactionService.ProcessTransactionAsync(
                request.ProductId, 
                request.Quantity, 
                request.TransactionType);

            if (!success)
            {
                return BadRequest("Failed to process transaction");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transaction");
            return StatusCode(500, "Internal server error");
        }
    }
}

/// <summary>
/// Request model for stock updates
/// </summary>
public class UpdateStockRequest
{
    public int Quantity { get; set; }
}

/// <summary>
/// Request model for processing transactions
/// </summary>
public class ProcessTransactionRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string TransactionType { get; set; } = string.Empty;
}
