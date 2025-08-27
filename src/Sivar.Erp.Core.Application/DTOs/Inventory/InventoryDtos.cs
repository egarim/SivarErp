using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Application.DTOs.Inventory;

/// <summary>
/// DTO for creating a new product
/// </summary>
public class CreateProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public ProductType Type { get; set; } = ProductType.Finished;
    public string? Category { get; set; }
    public string UnitOfMeasure { get; set; } = "EA";
    public decimal StandardCost { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
    public decimal? ReorderPoint { get; set; }
    public bool IsStockable { get; set; } = true;
    public bool IsPurchasable { get; set; } = true;
    public bool IsSaleable { get; set; } = true;
}

/// <summary>
/// DTO for product information
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public ProductType Type { get; set; }
    public string? Category { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal StandardCost { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
    public decimal? ReorderPoint { get; set; }
    public bool IsActive { get; set; }
    public bool IsStockable { get; set; }
    public bool IsPurchasable { get; set; }
    public bool IsSaleable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for product summary (list views)
/// </summary>
public class ProductSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ProductType Type { get; set; }
    public string? Category { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal StandardCost { get; set; }
    public bool IsActive { get; set; }
    public decimal? CurrentStock { get; set; }
    public decimal? TotalValue { get; set; }
}

/// <summary>
/// DTO for creating a warehouse
/// </summary>
public class CreateWarehouseDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public bool IsDefault { get; set; } = false;
}

/// <summary>
/// DTO for warehouse information
/// </summary>
public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating inventory transactions
/// </summary>
public class CreateInventoryTransactionDto
{
    public DateTime TransactionDate { get; set; } = DateTime.Today;
    public InventoryTransactionType Type { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for inventory transaction information
/// </summary>
public class InventoryTransactionDto
{
    public Guid Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public InventoryTransactionType Type { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for stock level information
/// </summary>
public class StockLevelDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
    public decimal AverageCost { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// DTO for inventory adjustments
/// </summary>
public class InventoryAdjustmentDto
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public decimal AdjustmentQuantity => NewQuantity - CurrentQuantity;
    public decimal UnitCost { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}
