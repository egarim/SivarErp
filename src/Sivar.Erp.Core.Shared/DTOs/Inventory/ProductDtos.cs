using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Shared.Dtos.Inventory;

/// <summary>
/// DTO for displaying product information
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
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    
    // Current stock information (aggregated)
    public decimal CurrentStock { get; set; }
    public decimal AvailableStock { get; set; }
    public decimal ReservedStock { get; set; }
    public decimal AverageCost { get; set; }
    public decimal TotalValue { get; set; }
}

/// <summary>
/// DTO for creating a new product
/// </summary>
public class CreateProductDto
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Barcode { get; set; }
    
    [Required]
    public ProductType Type { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    [Required]
    [StringLength(10)]
    public string UnitOfMeasure { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public decimal StandardCost { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MinimumStock { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MaximumStock { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? ReorderPoint { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsStockable { get; set; } = true;
    public bool IsPurchasable { get; set; } = true;
    public bool IsSaleable { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing product
/// </summary>
public class UpdateProductDto
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Barcode { get; set; }
    
    [Required]
    public ProductType Type { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    [Required]
    [StringLength(10)]
    public string UnitOfMeasure { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public decimal StandardCost { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MinimumStock { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MaximumStock { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? ReorderPoint { get; set; }
    
    public bool IsActive { get; set; }
    public bool IsStockable { get; set; }
    public bool IsPurchasable { get; set; }
    public bool IsSaleable { get; set; }
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
    public string CreatedBy { get; set; } = string.Empty;
}
