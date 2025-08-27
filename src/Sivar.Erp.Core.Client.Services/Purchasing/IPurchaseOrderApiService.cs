using Sivar.Erp.Core.Shared.Dtos.Purchasing;

namespace Sivar.Erp.Core.Client.Services.Purchasing;

/// <summary>
/// Interface for purchase order API operations
/// </summary>
public interface IPurchaseOrderApiService
{
    Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersAsync(int page = 1, int pageSize = 50, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null);
    Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(Guid id);
    Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto createDto);
    Task<PurchaseOrderDto> UpdatePurchaseOrderAsync(Guid id, UpdatePurchaseOrderDto updateDto);
    Task<PurchaseOrderDto> ApprovePurchaseOrderAsync(Guid id);
    Task<PurchaseOrderDto> ReceiveGoodsAsync(Guid id, ReceiveGoodsDto receiveDto);
    Task<PurchaseInvoiceDto> ConvertToPurchaseInvoiceAsync(Guid id);
    Task<PurchaseOrderDto> CancelPurchaseOrderAsync(Guid id, string? reason = null);
    Task<bool> DeletePurchaseOrderAsync(Guid id);
}

/// <summary>
/// HTTP client implementation for purchase order API operations
/// </summary>
public class PurchaseOrderApiService : ApiClientBase, IPurchaseOrderApiService
{
    public PurchaseOrderApiService(HttpClient httpClient, ILogger<PurchaseOrderApiService> logger) 
        : base(httpClient, logger)
    {
    }

    public async Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersAsync(int page = 1, int pageSize = 50, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(searchTerm))
            queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        if (!string.IsNullOrEmpty(status))
            queryParams.Add($"status={Uri.EscapeDataString(status)}");

        var query = string.Join("&", queryParams);
        var response = await GetAsync<IEnumerable<PurchaseOrderDto>>($"api/purchaseorders?{query}");
        return response ?? new List<PurchaseOrderDto>();
    }

    public async Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(Guid id)
    {
        return await GetAsync<PurchaseOrderDto>($"api/purchaseorders/{id}");
    }

    public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto createDto)
    {
        var response = await PostAsync<PurchaseOrderDto>("api/purchaseorders", createDto);
        return response ?? throw new InvalidOperationException("Failed to create purchase order");
    }

    public async Task<PurchaseOrderDto> UpdatePurchaseOrderAsync(Guid id, UpdatePurchaseOrderDto updateDto)
    {
        var response = await PutAsync<PurchaseOrderDto>($"api/purchaseorders/{id}", updateDto);
        return response ?? throw new InvalidOperationException("Failed to update purchase order");
    }

    public async Task<PurchaseOrderDto> ApprovePurchaseOrderAsync(Guid id)
    {
        var response = await PostAsync<PurchaseOrderDto>($"api/purchaseorders/{id}/approve", null);
        return response ?? throw new InvalidOperationException("Failed to approve purchase order");
    }

    public async Task<PurchaseOrderDto> ReceiveGoodsAsync(Guid id, ReceiveGoodsDto receiveDto)
    {
        var response = await PostAsync<PurchaseOrderDto>($"api/purchaseorders/{id}/receive", receiveDto);
        return response ?? throw new InvalidOperationException("Failed to receive goods");
    }

    public async Task<PurchaseInvoiceDto> ConvertToPurchaseInvoiceAsync(Guid id)
    {
        var response = await PostAsync<PurchaseInvoiceDto>($"api/purchaseorders/{id}/convert-to-invoice", null);
        return response ?? throw new InvalidOperationException("Failed to convert purchase order to invoice");
    }

    public async Task<PurchaseOrderDto> CancelPurchaseOrderAsync(Guid id, string? reason = null)
    {
        var response = await PostAsync<PurchaseOrderDto>($"api/purchaseorders/{id}/cancel", reason);
        return response ?? throw new InvalidOperationException("Failed to cancel purchase order");
    }

    public async Task<bool> DeletePurchaseOrderAsync(Guid id)
    {
        return await DeleteAsync($"api/purchaseorders/{id}");
    }
}
