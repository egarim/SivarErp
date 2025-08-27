using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Shared.DTOs;

namespace Sivar.Erp.Core.Shared.DTOs;

/// <summary>
/// Product DTO for API responses
/// </summary>
public class ProductDto : BaseDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal StandardCost { get; set; }

    [Range(0, double.MaxValue)]
    public decimal SalePrice { get; set; }

    public bool IsActive { get; set; } = true;

    // Category information
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // Inventory information
    public decimal CurrentStock { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }

    // Multi-tenant support
    public Guid TenantId { get; set; }
}

/// <summary>
/// DTO for creating a new product
/// </summary>
public class CreateProductDto : CreateBaseDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal StandardCost { get; set; }

    [Range(0, double.MaxValue)]
    public decimal SalePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid? CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinimumStock { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MaximumStock { get; set; }

    public Guid TenantId { get; set; }
}

/// <summary>
/// DTO for updating an existing product
/// </summary>
public class UpdateProductDto : UpdateBaseDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StandardCost { get; set; }

    [Range(0, double.MaxValue)]
    public decimal SalePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid? CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinimumStock { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MaximumStock { get; set; }
}
