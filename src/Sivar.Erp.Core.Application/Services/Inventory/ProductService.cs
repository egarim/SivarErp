using Sivar.Erp.Core.Application.DTOs.Inventory;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.Inventory;
using Sivar.Erp.Core.Domain.Entities.Inventory;
using Sivar.Erp.Core.Domain.Enums;
using Sivar.Erp.Core.Domain.Interfaces;
using Sivar.Erp.Core.Shared.Responses;

namespace Sivar.Erp.Core.Application.Services.Inventory;

/// <summary>
/// Service interface for product operations
/// </summary>
public interface IProductService
{
    Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createProductDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<ProductSummaryDto>>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDto>> UpdateProductAsync(Guid id, CreateProductDto updateProductDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for warehouse operations
/// </summary>
public interface IWarehouseService
{
    Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto createWarehouseDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<WarehouseDto>>> GetWarehousesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(Guid id, CreateWarehouseDto updateWarehouseDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteWarehouseAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for inventory transaction operations
/// </summary>
public interface IInventoryTransactionService
{
    Task<ApiResponse<InventoryTransactionDto>> CreateTransactionAsync(CreateInventoryTransactionDto createTransactionDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<InventoryTransactionDto>>> GetTransactionsAsync(Guid? productId = null, Guid? warehouseId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<InventoryTransactionDto>> AdjustInventoryAsync(InventoryAdjustmentDto adjustmentDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ReverseTransactionAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for stock level operations
/// </summary>
public interface IStockLevelService
{
    Task<ApiResponse<IEnumerable<StockLevelDto>>> GetStockLevelsAsync(Guid? productId = null, Guid? warehouseId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<StockLevelDto>> GetStockLevelAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<StockLevelDto>>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of product service
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createProductDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if code already exists
            if (await _productRepository.CodeExistsAsync(createProductDto.Code, cancellationToken: cancellationToken))
            {
                return ApiResponse<ProductDto>.Failure($"Product code '{createProductDto.Code}' already exists");
            }

            var product = new Product
            {
                Code = createProductDto.Code,
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Barcode = createProductDto.Barcode,
                Type = createProductDto.Type,
                Category = createProductDto.Category,
                UnitOfMeasure = createProductDto.UnitOfMeasure,
                StandardCost = createProductDto.StandardCost,
                MinimumStock = createProductDto.MinimumStock,
                MaximumStock = createProductDto.MaximumStock,
                ReorderPoint = createProductDto.ReorderPoint,
                IsStockable = createProductDto.IsStockable,
                IsPurchasable = createProductDto.IsPurchasable,
                IsSaleable = createProductDto.IsSaleable
            };

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var productDto = MapToProductDto(product);
            return ApiResponse<ProductDto>.Success(productDto, "Product created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDto>.Failure($"Failed to create product: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return ApiResponse<ProductDto>.Failure("Product not found");
            }

            var productDto = MapToProductDto(product);
            return ApiResponse<ProductDto>.Success(productDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDto>.Failure($"Failed to get product: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<ProductSummaryDto>>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _productRepository.GetActiveProductsAsync(cancellationToken);
            var productSummaries = products.Select(MapToProductSummaryDto);
            return ApiResponse<IEnumerable<ProductSummaryDto>>.Success(productSummaries);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<ProductSummaryDto>>.Failure($"Failed to get products: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProductDto>> UpdateProductAsync(Guid id, CreateProductDto updateProductDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return ApiResponse<ProductDto>.Failure("Product not found");
            }

            // Check if code already exists (excluding current product)
            if (await _productRepository.CodeExistsAsync(updateProductDto.Code, id, cancellationToken))
            {
                return ApiResponse<ProductDto>.Failure($"Product code '{updateProductDto.Code}' already exists");
            }

            // Update properties
            product.Code = updateProductDto.Code;
            product.Name = updateProductDto.Name;
            product.Description = updateProductDto.Description;
            product.Barcode = updateProductDto.Barcode;
            product.Type = updateProductDto.Type;
            product.Category = updateProductDto.Category;
            product.UnitOfMeasure = updateProductDto.UnitOfMeasure;
            product.StandardCost = updateProductDto.StandardCost;
            product.MinimumStock = updateProductDto.MinimumStock;
            product.MaximumStock = updateProductDto.MaximumStock;
            product.ReorderPoint = updateProductDto.ReorderPoint;
            product.IsStockable = updateProductDto.IsStockable;
            product.IsPurchasable = updateProductDto.IsPurchasable;
            product.IsSaleable = updateProductDto.IsSaleable;

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var productDto = MapToProductDto(product);
            return ApiResponse<ProductDto>.Success(productDto, "Product updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDto>.Failure($"Failed to update product: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return ApiResponse<bool>.Failure("Product not found");
            }

            // Soft delete by marking as inactive
            product.IsActive = false;
            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Product deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Failed to delete product: {ex.Message}");
        }
    }

    private static ProductDto MapToProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            Barcode = product.Barcode,
            Type = product.Type,
            Category = product.Category,
            UnitOfMeasure = product.UnitOfMeasure,
            StandardCost = product.StandardCost,
            MinimumStock = product.MinimumStock,
            MaximumStock = product.MaximumStock,
            ReorderPoint = product.ReorderPoint,
            IsActive = product.IsActive,
            IsStockable = product.IsStockable,
            IsPurchasable = product.IsPurchasable,
            IsSaleable = product.IsSaleable,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt ?? product.CreatedAt
        };
    }

    private static ProductSummaryDto MapToProductSummaryDto(Product product)
    {
        return new ProductSummaryDto
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Type = product.Type,
            Category = product.Category,
            UnitOfMeasure = product.UnitOfMeasure,
            StandardCost = product.StandardCost,
            IsActive = product.IsActive
        };
    }
}
