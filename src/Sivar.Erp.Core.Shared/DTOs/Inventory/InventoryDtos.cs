using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Shared.Dtos.Inventory;

/// <summary>
/// DTO for displaying stock level information
/// </summary>
public class StockLevelDto
{
    public Guid Id { get; set; }
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
    public string UnitOfMeasure { get; set; } = string.Empty;
    
    // Alert information
    public decimal? MinimumStock { get; set; }
    public decimal? ReorderPoint { get; set; }
    public bool IsLowStock => MinimumStock.HasValue && QuantityOnHand <= MinimumStock.Value;
    public bool IsReorderPoint => ReorderPoint.HasValue && QuantityOnHand <= ReorderPoint.Value;
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
    public string TypeDescription { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    
    // For transfers
    public Guid? ToWarehouseId { get; set; }
    public string? ToWarehouseCode { get; set; }
    public string? ToWarehouseName { get; set; }
    
    // Running balance
    public decimal RunningBalance { get; set; }
}

/// <summary>
/// DTO for creating stock adjustment
/// </summary>
public class CreateStockAdjustmentDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public Guid WarehouseId { get; set; }
    
    [Required]
    public decimal NewQuantity { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Reference { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? UnitCost { get; set; }
}

/// <summary>
/// DTO for creating stock transfer
/// </summary>
public class CreateStockTransferDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public Guid FromWarehouseId { get; set; }
    
    [Required]
    public Guid ToWarehouseId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Reference { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for low stock alerts
/// </summary>
public class LowStockAlertDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal ReorderPoint { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public int DaysWithoutMovement { get; set; }
    public AlertLevel AlertLevel { get; set; }
}

/// <summary>
/// DTO for inventory valuation
/// </summary>
public class InventoryValuationDto
{
    public DateTime AsOfDate { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AverageCostPerUnit { get; set; }
    public ICollection<ProductValuationDto> ProductValuations { get; set; } = new List<ProductValuationDto>();
}

/// <summary>
/// DTO for individual product valuation
/// </summary>
public class ProductValuationDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal TotalValue { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
}

/// <summary>
/// DTO for inventory movement report (Kardex)
/// </summary>
public class InventoryMovementDto
{
    public DateTime Date { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public InventoryTransactionType Type { get; set; }
    public string TypeDescription { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal UnitCost { get; set; }
    public decimal RunningQuantity { get; set; }
    public decimal RunningValue { get; set; }
    public decimal AverageCost { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Alert level enumeration
/// </summary>
public enum AlertLevel
{
    Normal = 0,
    Low = 1,
    Critical = 2,
    OutOfStock = 3
}
