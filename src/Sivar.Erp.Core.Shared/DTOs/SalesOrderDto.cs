using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Shared.DTOs;

namespace Sivar.Erp.Core.Shared.DTOs;

/// <summary>
/// Sales Order DTO
/// </summary>
public class SalesOrderDto : BaseDto
{
    [Required]
    [StringLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public DateTime OrderDate { get; set; }

    public DateTime? DeliveryDate { get; set; }

    [Required]
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal SubTotal { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Draft";

    [StringLength(500)]
    public string? Notes { get; set; }

    public List<SalesOrderLineDto> Lines { get; set; } = new();

    // Multi-tenant support
    public Guid TenantId { get; set; }
}

/// <summary>
/// Sales Order Line DTO
/// </summary>
public class SalesOrderLineDto : BaseDto
{
    public Guid SalesOrderId { get; set; }

    [Required]
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercentage { get; set; }

    [Range(0, double.MaxValue)]
    public decimal LineTotal { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating a new sales order
/// </summary>
public class CreateSalesOrderDto : CreateBaseDto
{
    [Required]
    public DateTime OrderDate { get; set; }

    public DateTime? DeliveryDate { get; set; }

    [Required]
    public Guid CustomerId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public List<CreateSalesOrderLineDto> Lines { get; set; } = new();

    public Guid TenantId { get; set; }
}

/// <summary>
/// DTO for creating a sales order line
/// </summary>
public class CreateSalesOrderLineDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercentage { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating a sales order
/// </summary>
public class UpdateSalesOrderDto : UpdateBaseDto
{
    public DateTime? DeliveryDate { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Draft";

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Sales Order Statistics DTO
/// </summary>
public class SalesOrderStatisticsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalValue { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
}
