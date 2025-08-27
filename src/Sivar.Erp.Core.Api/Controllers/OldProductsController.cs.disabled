using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Application.DTOs.Inventory;
using Sivar.Erp.Core.Application.Services.Inventory;
using Sivar.Erp.Core.Shared.Responses;

namespace Sivar.Erp.Core.Api.Controllers;

/// <summary>
/// Controller for managing products in the inventory system
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Health check for products API
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(ApiResponse<string>.Success("Products API is healthy"));
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var result = await _productService.GetProductsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        var result = await _productService.CreateProductAsync(createProductDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] CreateProductDto updateProductDto)
    {
        var result = await _productService.UpdateProductAsync(id, updateProductDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
