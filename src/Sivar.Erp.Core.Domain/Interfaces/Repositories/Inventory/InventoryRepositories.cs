using Sivar.Erp.Core.Domain.Entities.Inventory;
using Sivar.Erp.Core.Domain.Interfaces;

namespace Sivar.Erp.Core.Domain.Interfaces.Repositories.Inventory;

/// <summary>
/// Repository interface for product operations
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByTypeAsync(Enums.ProductType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for warehouse operations
/// </summary>
public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default);
    Task<Warehouse?> GetDefaultWarehouseAsync(CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for inventory transaction operations
/// </summary>
public interface IInventoryTransactionRepository : IRepository<InventoryTransaction>
{
    Task<IEnumerable<InventoryTransaction>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryTransaction>> GetByWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryTransaction>> GetByTypeAsync(Enums.InventoryTransactionType type, CancellationToken cancellationToken = default);
    Task<string> GenerateTransactionNumberAsync(Enums.InventoryTransactionType type, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for stock level operations
/// </summary>
public interface IStockLevelRepository : IRepository<StockLevel>
{
    Task<StockLevel?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockLevel>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockLevel>> GetByWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockLevel>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<StockLevel>> GetNegativeStockAsync(CancellationToken cancellationToken = default);
    Task UpdateStockLevelAsync(Guid productId, Guid warehouseId, decimal quantityChange, decimal newAverageCost, CancellationToken cancellationToken = default);
}
