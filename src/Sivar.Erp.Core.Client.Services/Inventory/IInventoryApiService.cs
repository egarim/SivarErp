using Sivar.Erp.Core.Shared.Dtos.Inventory;

namespace Sivar.Erp.Core.Client.Services.Inventory;

/// <summary>
/// Interface for inventory API operations
/// </summary>
public interface IInventoryApiService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(int page = 1, int pageSize = 50, string? searchTerm = null, bool? isActive = null);
    Task<IEnumerable<StockLevelDto>> GetStockLevelsAsync(Guid? warehouseId = null, Guid? productId = null);
    Task<StockLevelDto?> GetStockLevelAsync(Guid productId, Guid warehouseId);
    Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync(int page = 1, int pageSize = 50, Guid? productId = null, Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<InventoryTransactionDto?> GetTransactionByIdAsync(Guid id);
    Task<InventoryTransactionDto> CreateStockAdjustmentAsync(CreateStockAdjustmentDto adjustmentDto);
    Task<IEnumerable<InventoryTransactionDto>> CreateStockTransferAsync(CreateStockTransferDto transferDto);
    Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync();
    Task<InventoryValuationDto> GetInventoryValuationAsync(DateTime? asOfDate = null, Guid? warehouseId = null);
    Task<IEnumerable<InventoryMovementDto>> GetInventoryMovementReportAsync(Guid productId, Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null);
}

/// <summary>
/// HTTP client implementation for inventory API operations
/// </summary>
public class InventoryApiService : ApiClientBase, IInventoryApiService
{
    public InventoryApiService(HttpClient httpClient, ILogger<InventoryApiService> logger) 
        : base(httpClient, logger)
    {
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(int page = 1, int pageSize = 50, string? searchTerm = null, bool? isActive = null)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(searchTerm))
            queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");

        if (isActive.HasValue)
            queryParams.Add($"isActive={isActive.Value}");

        var query = string.Join("&", queryParams);
        var response = await GetAsync<IEnumerable<ProductDto>>($"api/inventory/products?{query}");
        return response ?? new List<ProductDto>();
    }

    public async Task<IEnumerable<StockLevelDto>> GetStockLevelsAsync(Guid? warehouseId = null, Guid? productId = null)
    {
        var queryParams = new List<string>();

        if (warehouseId.HasValue)
            queryParams.Add($"warehouseId={warehouseId.Value}");

        if (productId.HasValue)
            queryParams.Add($"productId={productId.Value}");

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var response = await GetAsync<IEnumerable<StockLevelDto>>($"api/inventory/stock-levels{query}");
        return response ?? new List<StockLevelDto>();
    }

    public async Task<StockLevelDto?> GetStockLevelAsync(Guid productId, Guid warehouseId)
    {
        return await GetAsync<StockLevelDto>($"api/inventory/stock-levels/{productId}/{warehouseId}");
    }

    public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync(int page = 1, int pageSize = 50, Guid? productId = null, Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (productId.HasValue)
            queryParams.Add($"productId={productId.Value}");

        if (warehouseId.HasValue)
            queryParams.Add($"warehouseId={warehouseId.Value}");

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        var query = string.Join("&", queryParams);
        var response = await GetAsync<IEnumerable<InventoryTransactionDto>>($"api/inventory/transactions?{query}");
        return response ?? new List<InventoryTransactionDto>();
    }

    public async Task<InventoryTransactionDto?> GetTransactionByIdAsync(Guid id)
    {
        return await GetAsync<InventoryTransactionDto>($"api/inventory/transactions/{id}");
    }

    public async Task<InventoryTransactionDto> CreateStockAdjustmentAsync(CreateStockAdjustmentDto adjustmentDto)
    {
        var response = await PostAsync<InventoryTransactionDto>("api/inventory/adjustments", adjustmentDto);
        return response ?? throw new InvalidOperationException("Failed to create stock adjustment");
    }

    public async Task<IEnumerable<InventoryTransactionDto>> CreateStockTransferAsync(CreateStockTransferDto transferDto)
    {
        var response = await PostAsync<IEnumerable<InventoryTransactionDto>>("api/inventory/transfers", transferDto);
        return response ?? throw new InvalidOperationException("Failed to create stock transfer");
    }

    public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync()
    {
        var response = await GetAsync<IEnumerable<LowStockAlertDto>>("api/inventory/low-stock-alerts");
        return response ?? new List<LowStockAlertDto>();
    }

    public async Task<InventoryValuationDto> GetInventoryValuationAsync(DateTime? asOfDate = null, Guid? warehouseId = null)
    {
        var queryParams = new List<string>();

        if (asOfDate.HasValue)
            queryParams.Add($"asOfDate={asOfDate.Value:yyyy-MM-dd}");

        if (warehouseId.HasValue)
            queryParams.Add($"warehouseId={warehouseId.Value}");

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var response = await GetAsync<InventoryValuationDto>($"api/inventory/valuation{query}");
        return response ?? new InventoryValuationDto();
    }

    public async Task<IEnumerable<InventoryMovementDto>> GetInventoryMovementReportAsync(Guid productId, Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var queryParams = new List<string>
        {
            $"productId={productId}"
        };

        if (warehouseId.HasValue)
            queryParams.Add($"warehouseId={warehouseId.Value}");

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        var query = string.Join("&", queryParams);
        var response = await GetAsync<IEnumerable<InventoryMovementDto>>($"api/inventory/movement-report?{query}");
        return response ?? new List<InventoryMovementDto>();
    }
}
