namespace Sivar.Erp.Core.Domain.Enums;

/// <summary>
/// Enumeration for customer types
/// </summary>
public enum CustomerType
{
    Individual = 1,
    Corporate = 2,
    Distributor = 3,
    Retailer = 4,
    Wholesale = 5
}

/// <summary>
/// Enumeration for sales order status
/// </summary>
public enum SalesOrderStatus
{
    Draft = 1,
    Confirmed = 2,
    InProgress = 3,
    Shipped = 4,
    Invoiced = 5,
    Completed = 6,
    Cancelled = 7
}

/// <summary>
/// Enumeration for invoice status
/// </summary>
public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    Paid = 3,
    PartiallyPaid = 4,
    Overdue = 5,
    Cancelled = 6,
    Voided = 7
}

/// <summary>
/// Enumeration for payment terms
/// </summary>
public enum PaymentTerms
{
    Net15 = 15,
    Net30 = 30,
    Net45 = 45,
    Net60 = 60,
    Net90 = 90,
    COD = 0,        // Cash on Delivery
    Prepaid = -1    // Payment before delivery
}

/// <summary>
/// Enumeration for sales document types
/// </summary>
public enum SalesDocumentType
{
    Quote = 1,
    SalesOrder = 2,
    Invoice = 3,
    CreditNote = 4,
    DebitNote = 5
}
