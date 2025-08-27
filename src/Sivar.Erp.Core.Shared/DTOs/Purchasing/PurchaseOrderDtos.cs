using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Core.Shared.Dtos.Purchasing;

/// <summary>
/// DTO for displaying purchase order information
/// </summary>
public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? PromisedDate { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal ExchangeRate { get; set; } = 1;
    public string? Notes { get; set; }
    public Guid? DeliveryWarehouseId { get; set; }
    public string? DeliveryWarehouseName { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    
    public ICollection<PurchaseOrderLineDto> Lines { get; set; } = new List<PurchaseOrderLineDto>();
}

/// <summary>
/// DTO for purchase order line items
/// </summary>
public class PurchaseOrderLineDto
{
    public Guid Id { get; set; }
    public int LineNumber { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? TaxAmount { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string? Notes { get; set; }
    
    // Receiving information
    public decimal QuantityReceived { get; set; }
    public decimal QuantityInvoiced { get; set; }
    public decimal QuantityPending => Quantity - QuantityReceived;
    public bool IsFullyReceived => QuantityReceived >= Quantity;
}

/// <summary>
/// DTO for creating purchase orders
/// </summary>
public class CreatePurchaseOrderDto
{
    [Required]
    public Guid SupplierId { get; set; }
    
    [Required]
    public DateTime OrderDate { get; set; }
    
    public DateTime? RequiredDate { get; set; }
    
    public DateTime? PromisedDate { get; set; }
    
    [StringLength(3)]
    public string Currency { get; set; } = "USD";
    
    [Range(0.01, double.MaxValue)]
    public decimal ExchangeRate { get; set; } = 1;
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    public Guid? DeliveryWarehouseId { get; set; }
    
    [StringLength(500)]
    public string? DeliveryAddress { get; set; }
    
    [StringLength(100)]
    public string? ContactPerson { get; set; }
    
    [StringLength(20)]
    public string? ContactPhone { get; set; }
    
    [EmailAddress]
    [StringLength(100)]
    public string? ContactEmail { get; set; }
    
    [Required]
    [MinLength(1)]
    public ICollection<CreatePurchaseOrderLineDto> Lines { get; set; } = new List<CreatePurchaseOrderLineDto>();
}

/// <summary>
/// DTO for creating purchase order lines
/// </summary>
public class CreatePurchaseOrderLineDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
    
    [Range(0, 100)]
    public decimal Discount { get; set; } = 0;
    
    [Range(0, 100)]
    public decimal? TaxRate { get; set; }
    
    public DateTime? RequiredDate { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating purchase orders
/// </summary>
public class UpdatePurchaseOrderDto
{
    public DateTime? RequiredDate { get; set; }
    public DateTime? PromisedDate { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    public Guid? DeliveryWarehouseId { get; set; }
    
    [StringLength(500)]
    public string? DeliveryAddress { get; set; }
    
    [StringLength(100)]
    public string? ContactPerson { get; set; }
    
    [StringLength(20)]
    public string? ContactPhone { get; set; }
    
    [EmailAddress]
    [StringLength(100)]
    public string? ContactEmail { get; set; }
}

/// <summary>
/// DTO for supplier information
/// </summary>
public class SupplierDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string? TaxId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ContactPerson { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string Currency { get; set; } = "USD";
    public int PaymentTermsDays { get; set; } = 30;
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public decimal TotalPurchases { get; set; }
    public int PurchaseOrderCount { get; set; }
}

/// <summary>
/// DTO for creating suppliers
/// </summary>
public class CreateSupplierDto
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? TradeName { get; set; }
    
    [StringLength(20)]
    public string? TaxId { get; set; }
    
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(100)]
    public string? ContactPerson { get; set; }
    
    [Url]
    [StringLength(200)]
    public string? Website { get; set; }
    
    [StringLength(300)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(100)]
    public string? State { get; set; }
    
    [StringLength(20)]
    public string? PostalCode { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    [StringLength(3)]
    public string Currency { get; set; } = "USD";
    
    [Range(1, 365)]
    public int PaymentTermsDays { get; set; } = 30;
    
    [Range(0, double.MaxValue)]
    public decimal CreditLimit { get; set; } = 0;
}

/// <summary>
/// DTO for updating suppliers
/// </summary>
public class UpdateSupplierDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? TradeName { get; set; }
    
    [StringLength(20)]
    public string? TaxId { get; set; }
    
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(100)]
    public string? ContactPerson { get; set; }
    
    [Url]
    [StringLength(200)]
    public string? Website { get; set; }
    
    [StringLength(300)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(100)]
    public string? State { get; set; }
    
    [StringLength(20)]
    public string? PostalCode { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    [StringLength(3)]
    public string Currency { get; set; } = "USD";
    
    [Range(1, 365)]
    public int PaymentTermsDays { get; set; } = 30;
    
    [Range(0, double.MaxValue)]
    public decimal CreditLimit { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for purchase order statistics
/// </summary>
public class PurchaseOrderStatisticsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ApprovedOrders { get; set; }
    public int PartiallyReceivedOrders { get; set; }
    public int CompletelyReceivedOrders { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal AverageLeadTimeDays { get; set; }
}

/// <summary>
/// DTO for receiving goods
/// </summary>
public class ReceiveGoodsDto
{
    [Required]
    public Guid PurchaseOrderId { get; set; }
    
    [Required]
    public DateTime ReceiveDate { get; set; }
    
    [Required]
    [StringLength(50)]
    public string ReceiptNumber { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [Required]
    [MinLength(1)]
    public ICollection<ReceiveGoodsLineDto> Lines { get; set; } = new List<ReceiveGoodsLineDto>();
}

/// <summary>
/// DTO for received goods line items
/// </summary>
public class ReceiveGoodsLineDto
{
    [Required]
    public Guid PurchaseOrderLineId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal QuantityReceived { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}
