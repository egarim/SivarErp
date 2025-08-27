using Sivar.Erp.Core.Shared.Dtos.Inventory;

namespace Sivar.Erp.Core.Application.Services.Inventory;

/// <summary>
/// Interface for inventory management operations
/// </summary>
public interface IInventoryService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(Guid companyId, int page = 1, int pageSize = 50, string? searchTerm = null, bool? isActive = null);
    Task<ProductDto?> GetProductByIdAsync(Guid id, Guid companyId);
    Task<ProductDto> CreateProductAsync(CreateProductDto createDto, Guid companyId, string userId);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto updateDto, Guid companyId, string userId);
    Task<bool> DeleteProductAsync(Guid id, Guid companyId);
    Task<InventoryValuationDto> GetInventoryValuationAsync(Guid companyId, DateTime? asOfDate = null, Guid? warehouseId = null);
    Task<IEnumerable<InventoryMovementDto>> GetInventoryMovementReportAsync(Guid companyId, Guid productId, Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null);
}

/// <summary>
/// Interface for stock level management
/// </summary>
public interface IStockLevelService
{
    Task<IEnumerable<StockLevelDto>> GetStockLevelsAsync(Guid companyId, Guid? warehouseId = null, Guid? productId = null);
    Task<StockLevelDto?> GetStockLevelAsync(Guid companyId, Guid productId, Guid warehouseId);
    Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(Guid companyId);
    Task<StockLevelDto> UpdateStockLevelAsync(Guid companyId, Guid productId, Guid warehouseId, decimal newQuantity, string userId, string reason);
}

/// <summary>
/// Interface for inventory transaction management
/// </summary>
public interface IInventoryTransactionService
{
    Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync(Guid companyId, int page = 1, int pageSize = 50, Guid? productId = null, Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<InventoryTransactionDto?> GetTransactionByIdAsync(Guid id, Guid companyId);
    Task<InventoryTransactionDto> CreateStockAdjustmentAsync(CreateStockAdjustmentDto adjustmentDto, Guid companyId, string userId);
    Task<IEnumerable<InventoryTransactionDto>> CreateStockTransferAsync(CreateStockTransferDto transferDto, Guid companyId, string userId);
    Task<InventoryTransactionDto> CreateReceiptTransactionAsync(Guid productId, Guid warehouseId, decimal quantity, decimal unitCost, string reference, Guid companyId, string userId);
    Task<InventoryTransactionDto> CreateIssueTransactionAsync(Guid productId, Guid warehouseId, decimal quantity, string reference, Guid companyId, string userId);
}
