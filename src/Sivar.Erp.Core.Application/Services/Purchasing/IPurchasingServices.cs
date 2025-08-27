using Sivar.Erp.Core.Shared.Dtos.Purchasing;

namespace Sivar.Erp.Core.Application.Services.Purchasing;

/// <summary>
/// Interface for purchase order business operations
/// </summary>
public interface IPurchaseOrderService
{
    Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersAsync(Guid companyId, int page = 1, int pageSize = 50, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null);
    Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(Guid id, Guid companyId);
    Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto createDto, Guid companyId, string userId);
    Task<PurchaseOrderDto> UpdatePurchaseOrderAsync(Guid id, UpdatePurchaseOrderDto updateDto, Guid companyId, string userId);
    Task<PurchaseOrderDto?> ApprovePurchaseOrderAsync(Guid id, Guid companyId, string userId);
    Task<PurchaseOrderDto?> ReceiveGoodsAsync(Guid id, ReceiveGoodsDto receiveDto, Guid companyId, string userId);
    Task<PurchaseInvoiceDto?> ConvertToPurchaseInvoiceAsync(Guid purchaseOrderId, Guid companyId, string userId);
    Task<PurchaseOrderDto?> CancelPurchaseOrderAsync(Guid id, Guid companyId, string userId, string? reason = null);
    Task<bool> DeletePurchaseOrderAsync(Guid id, Guid companyId);
    Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersBySupplierAsync(Guid supplierId, Guid companyId);
}

/// <summary>
/// Interface for purchase invoice business operations
/// </summary>
public interface IPurchaseInvoiceService
{
    Task<IEnumerable<PurchaseInvoiceDto>> GetPurchaseInvoicesAsync(Guid companyId, int page = 1, int pageSize = 50, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null);
    Task<PurchaseInvoiceDto?> GetPurchaseInvoiceByIdAsync(Guid id, Guid companyId);
    Task<PurchaseInvoiceDto> CreatePurchaseInvoiceAsync(CreatePurchaseInvoiceDto createDto, Guid companyId, string userId);
    Task<PurchaseInvoiceDto> UpdatePurchaseInvoiceAsync(Guid id, UpdatePurchaseInvoiceDto updateDto, Guid companyId, string userId);
    Task<PurchaseInvoiceDto?> PostPurchaseInvoiceAsync(Guid id, Guid companyId, string userId);
    Task<PurchaseInvoiceDto?> CancelPurchaseInvoiceAsync(Guid id, Guid companyId, string userId, string? reason = null);
    Task<bool> DeletePurchaseInvoiceAsync(Guid id, Guid companyId);
}

/// <summary>
/// Interface for supplier management
/// </summary>
public interface ISupplierService
{
    Task<IEnumerable<SupplierDto>> GetSuppliersAsync(Guid companyId, int page = 1, int pageSize = 50, string? searchTerm = null, bool? isActive = null);
    Task<SupplierDto?> GetSupplierByIdAsync(Guid id, Guid companyId);
    Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto, Guid companyId, string userId);
    Task<SupplierDto> UpdateSupplierAsync(Guid id, UpdateSupplierDto updateDto, Guid companyId, string userId);
    Task<bool> DeleteSupplierAsync(Guid id, Guid companyId);
}
