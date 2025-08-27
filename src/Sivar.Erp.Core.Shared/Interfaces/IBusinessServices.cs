using Sivar.Erp.Core.Shared.DTOs;

namespace Sivar.Erp.Core.Shared.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(ProductDto productDto);
        Task<ProductDto> UpdateProductAsync(ProductDto productDto);
        Task<bool> DeleteProductAsync(int id);
    }

    public interface IInventoryTransactionService
    {
        Task<bool> ProcessTransactionAsync(int productId, int quantity, string transactionType);
    }

    public interface IStockLevelService
    {
        Task<int> GetStockLevelAsync(int productId);
        Task<bool> UpdateStockLevelAsync(int productId, int quantity);
    }

    public interface IPurchaseOrderService
    {
        Task<bool> CreatePurchaseOrderAsync(int productId, int quantity, decimal price);
    }

    public interface ISupplierService
    {
        Task<IEnumerable<object>> GetAllSuppliersAsync();
    }

    public interface IInvoiceService
    {
        Task<bool> CreateInvoiceAsync(int productId, int quantity, decimal price);
    }

    public interface ISalesOrderService
    {
        Task<IEnumerable<object>> GetAllSalesOrdersAsync(Guid companyId);
        Task<object?> GetSalesOrderByIdAsync(Guid id);
        Task<object> CreateSalesOrderAsync(object createDto);
        Task<object> UpdateSalesOrderAsync(Guid id, object updateDto);
        Task<bool> DeleteSalesOrderAsync(Guid id, Guid companyId);
    }
}
