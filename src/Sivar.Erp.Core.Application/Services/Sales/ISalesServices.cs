using Sivar.Erp.Core.Shared.Dtos.Sales;
using Sivar.Erp.Core.Domain.Entities.Sales;

namespace Sivar.Erp.Core.Application.Services.Sales;

/// <summary>
/// Interface for sales order business operations
/// </summary>
public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrderDto>> GetSalesOrdersAsync(Guid companyId, int page = 1, int pageSize = 50, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<SalesOrderDto?> GetSalesOrderByIdAsync(Guid id, Guid companyId);
    Task<SalesOrderDto> CreateSalesOrderAsync(CreateSalesOrderDto createDto, Guid companyId, string userId);
    Task<SalesOrderDto> UpdateSalesOrderAsync(Guid id, UpdateSalesOrderDto updateDto, Guid companyId, string userId);
    Task<InvoiceDto?> ConvertToInvoiceAsync(Guid salesOrderId, Guid companyId, string userId);
    Task<bool> DeleteSalesOrderAsync(Guid id, Guid companyId);
    Task<SalesOrderDto> ConfirmSalesOrderAsync(Guid id, Guid companyId, string userId);
    Task<SalesOrderDto> CancelSalesOrderAsync(Guid id, Guid companyId, string userId, string? reason = null);
    Task<IEnumerable<SalesOrderDto>> GetSalesOrdersByCustomerAsync(Guid customerId, Guid companyId);
}

/// <summary>
/// Interface for invoice business operations
/// </summary>
public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDto>> GetInvoicesAsync(Guid companyId, int page = 1, int pageSize = 50, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null);
    Task<InvoiceDto?> GetInvoiceByIdAsync(Guid id, Guid companyId);
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceDto createDto, Guid companyId, string userId);
    Task<InvoiceDto> UpdateInvoiceAsync(Guid id, UpdateInvoiceDto updateDto, Guid companyId, string userId);
    Task<InvoiceDto?> PostInvoiceAsync(Guid id, Guid companyId, string userId);
    Task<InvoiceDto?> CancelInvoiceAsync(Guid id, Guid companyId, string userId, string? reason = null);
    Task<byte[]?> GenerateInvoicePdfAsync(Guid id, Guid companyId);
    Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(Guid companyId, DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> DeleteInvoiceAsync(Guid id, Guid companyId);
}
