using Sivar.Erp.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Core.Application.DTOs.Sales;

// Customer DTOs
public class CreateCustomerDto
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContactPerson { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Mobile { get; set; } = string.Empty;

    public CustomerType CustomerType { get; set; } = CustomerType.Individual;

    [MaxLength(50)]
    public string TaxId { get; set; } = string.Empty;

    // Billing Address
    [MaxLength(200)]
    public string BillingAddress { get; set; } = string.Empty;

    [MaxLength(100)]
    public string BillingCity { get; set; } = string.Empty;

    [MaxLength(50)]
    public string BillingState { get; set; } = string.Empty;

    [MaxLength(20)]
    public string BillingPostalCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string BillingCountry { get; set; } = string.Empty;

    // Shipping Address
    [MaxLength(200)]
    public string ShippingAddress { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ShippingCity { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ShippingState { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ShippingPostalCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ShippingCountry { get; set; } = string.Empty;

    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net30;

    public decimal CreditLimit { get; set; } = 0;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}

public class CustomerDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public string CustomerTypeText { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string BillingCity { get; set; } = string.Empty;
    public string BillingState { get; set; } = string.Empty;
    public string BillingPostalCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public PaymentTerms PaymentTerms { get; set; }
    public string PaymentTermsText { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class CustomerSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public string CustomerTypeText { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
}

// Sales Order DTOs
public class CreateSalesOrderDto
{
    [Required]
    [MaxLength(20)]
    public string OrderNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? RequiredDate { get; set; }

    [MaxLength(100)]
    public string CustomerPO { get; set; } = string.Empty;

    // Shipping Information
    [MaxLength(200)]
    public string ShipToAddress { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ShipToCity { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ShipToState { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ShipToPostalCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ShipToCountry { get; set; } = string.Empty;

    public decimal DiscountPercent { get; set; } = 0;

    public decimal TaxPercent { get; set; } = 0;

    public decimal ShippingAmount { get; set; } = 0;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    [MaxLength(500)]
    public string InternalNotes { get; set; } = string.Empty;

    public List<CreateSalesOrderLineDto> Lines { get; set; } = new List<CreateSalesOrderLineDto>();
}

public class CreateSalesOrderLineDto
{
    public int LineNumber { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; } = 0;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}

public class SalesOrderDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public SalesOrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string CustomerPO { get; set; } = string.Empty;
    public string ShipToAddress { get; set; } = string.Empty;
    public string ShipToCity { get; set; } = string.Empty;
    public string ShipToState { get; set; } = string.Empty;
    public string ShipToPostalCode { get; set; } = string.Empty;
    public string ShipToCountry { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string InternalNotes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public List<SalesOrderLineDto> Lines { get; set; } = new List<SalesOrderLineDto>();
}

public class SalesOrderLineDto
{
    public Guid Id { get; set; }
    public Guid SalesOrderId { get; set; }
    public int LineNumber { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal QuantityShipped { get; set; }
    public decimal QuantityInvoiced { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class SalesOrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public SalesOrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

// Invoice DTOs
public class CreateInvoiceDto
{
    [Required]
    [MaxLength(20)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public Guid? SalesOrderId { get; set; }

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    public DateTime DueDate { get; set; }

    [MaxLength(100)]
    public string CustomerPO { get; set; } = string.Empty;

    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net30;

    public decimal DiscountPercent { get; set; } = 0;

    public decimal TaxPercent { get; set; } = 0;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    [MaxLength(500)]
    public string InternalNotes { get; set; } = string.Empty;

    public List<CreateInvoiceLineDto> Lines { get; set; } = new List<CreateInvoiceLineDto>();
}

public class CreateInvoiceLineDto
{
    public int LineNumber { get; set; }
    public Guid? SalesOrderLineId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; } = 0;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public Guid? SalesOrderId { get; set; }
    public string SalesOrderNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string CustomerPO { get; set; } = string.Empty;
    public PaymentTerms PaymentTerms { get; set; }
    public string PaymentTermsText { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string InternalNotes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public List<InvoiceLineDto> Lines { get; set; } = new List<InvoiceLineDto>();
}

public class InvoiceLineDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public int LineNumber { get; set; }
    public Guid? SalesOrderLineId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class InvoiceSummaryDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal BalanceDue { get; set; }
}
