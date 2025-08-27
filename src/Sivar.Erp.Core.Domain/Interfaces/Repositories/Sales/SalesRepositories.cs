using Sivar.Erp.Core.Domain.Interfaces;
using Sivar.Erp.Core.Domain.Entities.Sales;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Domain.Interfaces.Repositories.Sales;

/// <summary>
/// Repository interface for Customer entity
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByCodeAsync(Guid companyId, string code);
    Task<bool> IsCodeUniqueAsync(Guid companyId, string code, Guid? excludeId = null);
    Task<IEnumerable<Customer>> GetActiveCustomersAsync(Guid companyId);
    Task<IEnumerable<Customer>> GetCustomersByTypeAsync(Guid companyId, CustomerType customerType);
    Task<Customer?> GetCustomerWithOrdersAsync(Guid companyId, Guid customerId);
    Task<decimal> GetCustomerBalanceAsync(Guid companyId, Guid customerId);
    Task<IEnumerable<Customer>> GetByCompanyAsync(Guid companyId);
}

/// <summary>
/// Repository interface for SalesOrder entity
/// </summary>
public interface ISalesOrderRepository : IRepository<SalesOrder>
{
    Task<SalesOrder?> GetByOrderNumberAsync(Guid companyId, string orderNumber);
    Task<bool> IsOrderNumberUniqueAsync(Guid companyId, string orderNumber, Guid? excludeId = null);
    Task<SalesOrder?> GetWithLinesAsync(Guid companyId, Guid salesOrderId);
    Task<IEnumerable<SalesOrder>> GetByCustomerAsync(Guid companyId, Guid customerId);
    Task<IEnumerable<SalesOrder>> GetByStatusAsync(Guid companyId, SalesOrderStatus status);
    Task<IEnumerable<SalesOrder>> GetByDateRangeAsync(Guid companyId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<SalesOrder>> GetPendingOrdersAsync(Guid companyId);
    Task<string> GetNextOrderNumberAsync(Guid companyId);
}

/// <summary>
/// Repository interface for SalesOrderLine entity
/// </summary>
public interface ISalesOrderLineRepository : IRepository<SalesOrderLine>
{
    Task<IEnumerable<SalesOrderLine>> GetByOrderAsync(Guid salesOrderId);
    Task<IEnumerable<SalesOrderLine>> GetByProductAsync(Guid companyId, Guid productId);
    Task<SalesOrderLine?> GetLineWithProductAsync(Guid salesOrderLineId);
    Task<decimal> GetTotalQuantityByProductAsync(Guid companyId, Guid productId);
}

/// <summary>
/// Repository interface for Invoice entity
/// </summary>
public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetByInvoiceNumberAsync(Guid companyId, string invoiceNumber);
    Task<bool> IsInvoiceNumberUniqueAsync(Guid companyId, string invoiceNumber, Guid? excludeId = null);
    Task<Invoice?> GetWithLinesAsync(Guid companyId, Guid invoiceId);
    Task<IEnumerable<Invoice>> GetByCustomerAsync(Guid companyId, Guid customerId);
    Task<IEnumerable<Invoice>> GetByStatusAsync(Guid companyId, InvoiceStatus status);
    Task<IEnumerable<Invoice>> GetByDateRangeAsync(Guid companyId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(Guid companyId);
    Task<IEnumerable<Invoice>> GetUnpaidInvoicesAsync(Guid companyId);
    Task<string> GetNextInvoiceNumberAsync(Guid companyId);
    Task<decimal> GetTotalSalesAsync(Guid companyId, DateTime fromDate, DateTime toDate);
}

/// <summary>
/// Repository interface for InvoiceLine entity
/// </summary>
public interface IInvoiceLineRepository : IRepository<InvoiceLine>
{
    Task<IEnumerable<InvoiceLine>> GetByInvoiceAsync(Guid invoiceId);
    Task<IEnumerable<InvoiceLine>> GetByProductAsync(Guid companyId, Guid productId);
    Task<InvoiceLine?> GetLineWithProductAsync(Guid invoiceLineId);
    Task<decimal> GetTotalSalesQuantityByProductAsync(Guid companyId, Guid productId, DateTime fromDate, DateTime toDate);
}
