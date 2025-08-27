using Sivar.Erp.Core.Domain.Entities;
using Sivar.Erp.Core.Domain.Entities.Inventory;
using Sivar.Erp.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Core.Domain.Entities.Sales;

/// <summary>
/// Customer entity for managing business customers
/// </summary>
public class Customer : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }

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

    // Payment Terms
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net30;

    public decimal CreditLimit { get; set; } = 0;

    public decimal CurrentBalance { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    // Navigation Properties
    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

/// <summary>
/// Sales Order entity for managing customer orders
/// </summary>
public class SalesOrder : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }

    [Required]
    [MaxLength(20)]
    public string OrderNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;

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

    // Totals
    public decimal SubTotal { get; set; } = 0;

    public decimal DiscountPercent { get; set; } = 0;

    public decimal DiscountAmount { get; set; } = 0;

    public decimal TaxPercent { get; set; } = 0;

    public decimal TaxAmount { get; set; } = 0;

    public decimal ShippingAmount { get; set; } = 0;

    public decimal TotalAmount { get; set; } = 0;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    [MaxLength(500)]
    public string InternalNotes { get; set; } = string.Empty;

    // Navigation Properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<SalesOrderLine> SalesOrderLines { get; set; } = new List<SalesOrderLine>();
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

/// <summary>
/// Sales Order Line entity for order line items
/// </summary>
public class SalesOrderLine : BaseEntity
{
    public Guid SalesOrderId { get; set; }

    public int LineNumber { get; set; }

    public Guid ProductId { get; set; }

    [MaxLength(100)]
    public string ProductCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountPercent { get; set; } = 0;

    public decimal DiscountAmount { get; set; } = 0;

    public decimal LineTotal { get; set; } = 0;

    public decimal QuantityShipped { get; set; } = 0;

    public decimal QuantityInvoiced { get; set; } = 0;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    // Navigation Properties
    public virtual SalesOrder SalesOrder { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
}

/// <summary>
/// Invoice entity for customer billing
/// </summary>
public class Invoice : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }

    [Required]
    [MaxLength(20)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public Guid? SalesOrderId { get; set; }

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    public DateTime DueDate { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    [MaxLength(100)]
    public string CustomerPO { get; set; } = string.Empty;

    // Payment Information
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net30;

    public DateTime? PaidDate { get; set; }

    public decimal AmountPaid { get; set; } = 0;

    // Totals
    public decimal SubTotal { get; set; } = 0;

    public decimal DiscountPercent { get; set; } = 0;

    public decimal DiscountAmount { get; set; } = 0;

    public decimal TaxPercent { get; set; } = 0;

    public decimal TaxAmount { get; set; } = 0;

    public decimal TotalAmount { get; set; } = 0;

    public decimal BalanceDue { get; set; } = 0;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    [MaxLength(500)]
    public string InternalNotes { get; set; } = string.Empty;

    // Navigation Properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual SalesOrder? SalesOrder { get; set; }
    public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
}

/// <summary>
/// Invoice Line entity for invoice line items
/// </summary>
public class InvoiceLine : BaseEntity
{
    public Guid InvoiceId { get; set; }

    public int LineNumber { get; set; }

    public Guid? SalesOrderLineId { get; set; }

    public Guid ProductId { get; set; }

    [MaxLength(100)]
    public string ProductCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountPercent { get; set; } = 0;

    public decimal DiscountAmount { get; set; } = 0;

    public decimal LineTotal { get; set; } = 0;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    // Navigation Properties
    public virtual Invoice Invoice { get; set; } = null!;
    public virtual SalesOrderLine? SalesOrderLine { get; set; }
    public virtual Product Product { get; set; } = null!;
}
