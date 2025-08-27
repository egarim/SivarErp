using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Core.Domain.Entities.Sales;
using Sivar.Erp.Core.Domain.Enums;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.Sales;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Infrastructure.Repositories;

namespace Sivar.Erp.Core.Infrastructure.Repositories.Sales;

public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    private readonly ErpDbContext _erpContext;

    public CustomerRepository(ErpDbContext context) : base(context)
    {
        _erpContext = context;
    }

    public async Task<Customer?> GetByCodeAsync(Guid companyId, string code)
    {
        return await _erpContext.Customers
            .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Code == code && c.IsActive);
    }

    public async Task<bool> IsCodeUniqueAsync(Guid companyId, string code, Guid? excludeId = null)
    {
        var query = _erpContext.Customers
            .Where(c => c.CompanyId == companyId && c.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync(Guid companyId)
    {
        return await _erpContext.Customers
            .Where(c => c.CompanyId == companyId && c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetCustomersByTypeAsync(Guid companyId, CustomerType customerType)
    {
        return await _erpContext.Customers
            .Where(c => c.CompanyId == companyId && c.CustomerType == customerType && c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<Customer?> GetCustomerWithOrdersAsync(Guid companyId, Guid customerId)
    {
        return await _erpContext.Customers
            .Include(c => c.SalesOrders.Where(so => so.CompanyId == companyId))
            .ThenInclude(so => so.SalesOrderLines)
            .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == customerId);
    }

    public async Task<decimal> GetCustomerBalanceAsync(Guid companyId, Guid customerId)
    {
        var totalInvoiced = await _erpContext.Invoices
            .Where(i => i.CompanyId == companyId && i.CustomerId == customerId && 
                       i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Voided)
            .SumAsync(i => i.TotalAmount);

        var totalPaid = await _erpContext.Invoices
            .Where(i => i.CompanyId == companyId && i.CustomerId == customerId && 
                       i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Voided)
            .SumAsync(i => i.AmountPaid);

        return totalInvoiced - totalPaid;
    }

    public async Task<IEnumerable<Customer>> GetByCompanyAsync(Guid companyId)
    {
        return await _erpContext.Customers
            .Where(c => c.CompanyId == companyId)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }
}

public class SalesOrderRepository : GenericRepository<SalesOrder>, ISalesOrderRepository
{
    private readonly ErpDbContext _erpContext;

    public SalesOrderRepository(ErpDbContext context) : base(context)
    {
        _erpContext = context;
    }

    public async Task<SalesOrder?> GetByOrderNumberAsync(Guid companyId, string orderNumber)
    {
        return await _erpContext.SalesOrders
            .Include(so => so.Customer)
            .FirstOrDefaultAsync(so => so.CompanyId == companyId && so.OrderNumber == orderNumber);
    }

    public async Task<bool> IsOrderNumberUniqueAsync(Guid companyId, string orderNumber, Guid? excludeId = null)
    {
        var query = _erpContext.SalesOrders
            .Where(so => so.CompanyId == companyId && so.OrderNumber == orderNumber);

        if (excludeId.HasValue)
        {
            query = query.Where(so => so.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<SalesOrder?> GetWithLinesAsync(Guid companyId, Guid salesOrderId)
    {
        return await _erpContext.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.SalesOrderLines)
                .ThenInclude(sol => sol.Product)
            .FirstOrDefaultAsync(so => so.CompanyId == companyId && so.Id == salesOrderId);
    }

    public async Task<IEnumerable<SalesOrder>> GetByCustomerAsync(Guid companyId, Guid customerId)
    {
        return await _erpContext.SalesOrders
            .Include(so => so.Customer)
            .Where(so => so.CompanyId == companyId && so.CustomerId == customerId)
            .OrderByDescending(so => so.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SalesOrder>> GetByStatusAsync(Guid companyId, SalesOrderStatus status)
    {
        return await _erpContext.SalesOrders
            .Include(so => so.Customer)
            .Where(so => so.CompanyId == companyId && so.Status == status)
            .OrderBy(so => so.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SalesOrder>> GetByDateRangeAsync(Guid companyId, DateTime fromDate, DateTime toDate)
    {
        return await _erpContext.SalesOrders
            .Include(so => so.Customer)
            .Where(so => so.CompanyId == companyId && 
                        so.OrderDate >= fromDate && so.OrderDate <= toDate)
            .OrderBy(so => so.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SalesOrder>> GetPendingOrdersAsync(Guid companyId)
    {
        return await _erpContext.SalesOrders
            .Include(so => so.Customer)
            .Where(so => so.CompanyId == companyId && 
                        (so.Status == SalesOrderStatus.Confirmed || 
                         so.Status == SalesOrderStatus.InProgress))
            .OrderBy(so => so.RequiredDate ?? so.OrderDate)
            .ToListAsync();
    }

    public async Task<string> GetNextOrderNumberAsync(Guid companyId)
    {
        var currentYear = DateTime.UtcNow.Year;
        var prefix = $"SO{currentYear}";
        
        var lastOrder = await _erpContext.SalesOrders
            .Where(so => so.CompanyId == companyId && so.OrderNumber.StartsWith(prefix))
            .OrderByDescending(so => so.OrderNumber)
            .FirstOrDefaultAsync();

        if (lastOrder == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = lastOrder.OrderNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out int number))
        {
            return $"{prefix}{(number + 1):D4}";
        }

        return $"{prefix}0001";
    }
}

public class SalesOrderLineRepository : GenericRepository<SalesOrderLine>, ISalesOrderLineRepository
{
    private readonly ErpDbContext _erpContext;

    public SalesOrderLineRepository(ErpDbContext context) : base(context)
    {
        _erpContext = context;
    }

    public async Task<IEnumerable<SalesOrderLine>> GetByOrderAsync(Guid salesOrderId)
    {
        return await _erpContext.SalesOrderLines
            .Include(sol => sol.Product)
            .Where(sol => sol.SalesOrderId == salesOrderId)
            .OrderBy(sol => sol.LineNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<SalesOrderLine>> GetByProductAsync(Guid companyId, Guid productId)
    {
        return await _erpContext.SalesOrderLines
            .Include(sol => sol.SalesOrder)
                .ThenInclude(so => so.Customer)
            .Where(sol => sol.SalesOrder.CompanyId == companyId && sol.ProductId == productId)
            .OrderByDescending(sol => sol.SalesOrder.OrderDate)
            .ToListAsync();
    }

    public async Task<SalesOrderLine?> GetLineWithProductAsync(Guid salesOrderLineId)
    {
        return await _erpContext.SalesOrderLines
            .Include(sol => sol.Product)
            .Include(sol => sol.SalesOrder)
            .FirstOrDefaultAsync(sol => sol.Id == salesOrderLineId);
    }

    public async Task<decimal> GetTotalQuantityByProductAsync(Guid companyId, Guid productId)
    {
        return await _erpContext.SalesOrderLines
            .Where(sol => sol.SalesOrder.CompanyId == companyId && 
                         sol.ProductId == productId &&
                         sol.SalesOrder.Status != SalesOrderStatus.Cancelled)
            .SumAsync(sol => sol.Quantity);
    }
}

public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
{
    private readonly ErpDbContext _erpContext;

    public InvoiceRepository(ErpDbContext context) : base(context)
    {
        _erpContext = context;
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(Guid companyId, string invoiceNumber)
    {
        return await _erpContext.Invoices
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.CompanyId == companyId && i.InvoiceNumber == invoiceNumber);
    }

    public async Task<bool> IsInvoiceNumberUniqueAsync(Guid companyId, string invoiceNumber, Guid? excludeId = null)
    {
        var query = _erpContext.Invoices
            .Where(i => i.CompanyId == companyId && i.InvoiceNumber == invoiceNumber);

        if (excludeId.HasValue)
        {
            query = query.Where(i => i.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<Invoice?> GetWithLinesAsync(Guid companyId, Guid invoiceId)
    {
        return await _erpContext.Invoices
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .Include(i => i.InvoiceLines)
                .ThenInclude(il => il.Product)
            .FirstOrDefaultAsync(i => i.CompanyId == companyId && i.Id == invoiceId);
    }

    public async Task<IEnumerable<Invoice>> GetByCustomerAsync(Guid companyId, Guid customerId)
    {
        return await _erpContext.Invoices
            .Include(i => i.Customer)
            .Where(i => i.CompanyId == companyId && i.CustomerId == customerId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(Guid companyId, InvoiceStatus status)
    {
        return await _erpContext.Invoices
            .Include(i => i.Customer)
            .Where(i => i.CompanyId == companyId && i.Status == status)
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByDateRangeAsync(Guid companyId, DateTime fromDate, DateTime toDate)
    {
        return await _erpContext.Invoices
            .Include(i => i.Customer)
            .Where(i => i.CompanyId == companyId && 
                       i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(Guid companyId)
    {
        var today = DateTime.UtcNow.Date;
        return await _erpContext.Invoices
            .Include(i => i.Customer)
            .Where(i => i.CompanyId == companyId && 
                       i.DueDate < today && 
                       i.BalanceDue > 0 &&
                       i.Status != InvoiceStatus.Paid && 
                       i.Status != InvoiceStatus.Cancelled && 
                       i.Status != InvoiceStatus.Voided)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetUnpaidInvoicesAsync(Guid companyId)
    {
        return await _erpContext.Invoices
            .Include(i => i.Customer)
            .Where(i => i.CompanyId == companyId && 
                       i.BalanceDue > 0 &&
                       i.Status != InvoiceStatus.Paid && 
                       i.Status != InvoiceStatus.Cancelled && 
                       i.Status != InvoiceStatus.Voided)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }

    public async Task<string> GetNextInvoiceNumberAsync(Guid companyId)
    {
        var currentYear = DateTime.UtcNow.Year;
        var prefix = $"INV{currentYear}";
        
        var lastInvoice = await _erpContext.Invoices
            .Where(i => i.CompanyId == companyId && i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        if (lastInvoice == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = lastInvoice.InvoiceNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out int number))
        {
            return $"{prefix}{(number + 1):D4}";
        }

        return $"{prefix}0001";
    }

    public async Task<decimal> GetTotalSalesAsync(Guid companyId, DateTime fromDate, DateTime toDate)
    {
        return await _erpContext.Invoices
            .Where(i => i.CompanyId == companyId && 
                       i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate &&
                       i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Voided)
            .SumAsync(i => i.TotalAmount);
    }
}

public class InvoiceLineRepository : GenericRepository<InvoiceLine>, IInvoiceLineRepository
{
    private readonly ErpDbContext _erpContext;

    public InvoiceLineRepository(ErpDbContext context) : base(context)
    {
        _erpContext = context;
    }

    public async Task<IEnumerable<InvoiceLine>> GetByInvoiceAsync(Guid invoiceId)
    {
        return await _erpContext.InvoiceLines
            .Include(il => il.Product)
            .Where(il => il.InvoiceId == invoiceId)
            .OrderBy(il => il.LineNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<InvoiceLine>> GetByProductAsync(Guid companyId, Guid productId)
    {
        return await _erpContext.InvoiceLines
            .Include(il => il.Invoice)
                .ThenInclude(i => i.Customer)
            .Where(il => il.Invoice.CompanyId == companyId && il.ProductId == productId)
            .OrderByDescending(il => il.Invoice.InvoiceDate)
            .ToListAsync();
    }

    public async Task<InvoiceLine?> GetLineWithProductAsync(Guid invoiceLineId)
    {
        return await _erpContext.InvoiceLines
            .Include(il => il.Product)
            .Include(il => il.Invoice)
            .FirstOrDefaultAsync(il => il.Id == invoiceLineId);
    }

    public async Task<decimal> GetTotalSalesQuantityByProductAsync(Guid companyId, Guid productId, DateTime fromDate, DateTime toDate)
    {
        return await _erpContext.InvoiceLines
            .Where(il => il.Invoice.CompanyId == companyId && 
                        il.ProductId == productId &&
                        il.Invoice.InvoiceDate >= fromDate && il.Invoice.InvoiceDate <= toDate &&
                        il.Invoice.Status != InvoiceStatus.Cancelled && il.Invoice.Status != InvoiceStatus.Voided)
            .SumAsync(il => il.Quantity);
    }
}
