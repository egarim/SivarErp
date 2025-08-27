using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Shared.Dtos.Sales;

/// <summary>
/// DTO for displaying invoice information
/// </summary>
public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public Guid? SalesOrderId { get; set; }
    public string? SalesOrderNumber { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public string? PostedBy { get; set; }
    public ICollection<InvoiceLineDto> Lines { get; set; } = new List<InvoiceLineDto>();
}

/// <summary>
/// DTO for invoice line items
/// </summary>
public class InvoiceLineDto
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
/// DTO for creating a new invoice
/// </summary>
public class CreateInvoiceDto
{
    [Required]
    public Guid CustomerId { get; set; }
    
    [Required]
    public DateTime InvoiceDate { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public Guid? SalesOrderId { get; set; }
    
    public string? Notes { get; set; }
    
    public string? Terms { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one line item is required")]
    public ICollection<CreateInvoiceLineDto> Lines { get; set; } = new List<CreateInvoiceLineDto>();
}

/// <summary>
/// DTO for creating invoice lines
/// </summary>
public class CreateInvoiceLineDto
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
/// DTO for updating an existing invoice
/// </summary>
public class UpdateInvoiceDto
{
    [Required]
    public Guid CustomerId { get; set; }
    
    [Required]
    public DateTime InvoiceDate { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public string? Notes { get; set; }
    
    public string? Terms { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one line item is required")]
    public ICollection<UpdateInvoiceLineDto> Lines { get; set; } = new List<UpdateInvoiceLineDto>();
}

/// <summary>
/// DTO for updating invoice lines
/// </summary>
public class UpdateInvoiceLineDto
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

/// <summary>
/// DTO for invoice statistics
/// </summary>
public class InvoiceStatisticsDto
{
    public int TotalInvoices { get; set; }
    public decimal TotalSales { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal OverdueAmount { get; set; }
    public decimal AverageDaysToPayment { get; set; }
    public ICollection<MonthlySalesDto> MonthlySales { get; set; } = new List<MonthlySalesDto>();
}

/// <summary>
/// DTO for monthly sales data
/// </summary>
public class MonthlySalesDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int InvoiceCount { get; set; }
}
