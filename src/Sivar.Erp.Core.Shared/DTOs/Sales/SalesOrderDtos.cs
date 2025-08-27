using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Shared.Dtos.Sales;

/// <summary>
/// DTO for displaying sales order information
/// </summary>
public class SalesOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public SalesOrderStatus Status { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public ICollection<SalesOrderLineDto> Lines { get; set; } = new List<SalesOrderLineDto>();
}

/// <summary>
/// DTO for sales order line items
/// </summary>
public class SalesOrderLineDto
{
    public Guid Id { get; set; }
    public int LineNumber { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
}

/// <summary>
/// DTO for creating a new sales order
/// </summary>
public class CreateSalesOrderDto
{
    [Required]
    public Guid CustomerId { get; set; }
    
    [Required]
    public DateTime OrderDate { get; set; }
    
    public DateTime? DeliveryDate { get; set; }
    
    public string? Notes { get; set; }
    
    public string? Terms { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one line item is required")]
    public ICollection<CreateSalesOrderLineDto> Lines { get; set; } = new List<CreateSalesOrderLineDto>();
}

/// <summary>
/// DTO for creating sales order lines
/// </summary>
public class CreateSalesOrderLineDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
    public decimal Quantity { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative")]
    public decimal UnitPrice { get; set; }
    
    [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
    public decimal DiscountPercentage { get; set; } = 0;
    
    [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
    public decimal TaxRate { get; set; } = 0;
}

/// <summary>
/// DTO for updating an existing sales order
/// </summary>
public class UpdateSalesOrderDto
{
    [Required]
    public Guid CustomerId { get; set; }
    
    [Required]
    public DateTime OrderDate { get; set; }
    
    public DateTime? DeliveryDate { get; set; }
    
    public string? Notes { get; set; }
    
    public string? Terms { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one line item is required")]
    public ICollection<UpdateSalesOrderLineDto> Lines { get; set; } = new List<UpdateSalesOrderLineDto>();
}

/// <summary>
/// DTO for updating sales order lines
/// </summary>
public class UpdateSalesOrderLineDto
{
    public Guid? Id { get; set; } // null for new lines
    
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
    public decimal Quantity { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative")]
    public decimal UnitPrice { get; set; }
    
    [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
    public decimal DiscountPercentage { get; set; } = 0;
    
    [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
    public decimal TaxRate { get; set; } = 0;
}
