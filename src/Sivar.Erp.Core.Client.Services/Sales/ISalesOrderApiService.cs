using Sivar.Erp.Core.Shared.Dtos.Sales;
using System.Text.Json;

namespace Sivar.Erp.Core.Client.Services.Sales;

/// <summary>
/// Interface for sales order API operations
/// </summary>
public interface ISalesOrderApiService
{
    Task<IEnumerable<SalesOrderDto>> GetSalesOrdersAsync(int page = 1, int pageSize = 50, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<SalesOrderDto?> GetSalesOrderByIdAsync(Guid id);
    Task<SalesOrderDto> CreateSalesOrderAsync(CreateSalesOrderDto createDto);
    Task<SalesOrderDto> UpdateSalesOrderAsync(Guid id, UpdateSalesOrderDto updateDto);
    Task<InvoiceDto> ConvertToInvoiceAsync(Guid id);
    Task<bool> DeleteSalesOrderAsync(Guid id);
}

/// <summary>
/// HTTP client implementation for sales order API operations
/// </summary>
public class SalesOrderApiService : ApiClientBase, ISalesOrderApiService
{
    public SalesOrderApiService(HttpClient httpClient, ILogger<SalesOrderApiService> logger) 
        : base(httpClient, logger)
    {
    }

    public async Task<IEnumerable<SalesOrderDto>> GetSalesOrdersAsync(int page = 1, int pageSize = 50, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null)
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

        var query = string.Join("&", queryParams);
        var response = await GetAsync<IEnumerable<SalesOrderDto>>($"api/salesorders?{query}");
        return response ?? new List<SalesOrderDto>();
    }

    public async Task<SalesOrderDto?> GetSalesOrderByIdAsync(Guid id)
    {
        return await GetAsync<SalesOrderDto>($"api/salesorders/{id}");
    }

    public async Task<SalesOrderDto> CreateSalesOrderAsync(CreateSalesOrderDto createDto)
    {
        var response = await PostAsync<SalesOrderDto>("api/salesorders", createDto);
        return response ?? throw new InvalidOperationException("Failed to create sales order");
    }

    public async Task<SalesOrderDto> UpdateSalesOrderAsync(Guid id, UpdateSalesOrderDto updateDto)
    {
        var response = await PutAsync<SalesOrderDto>($"api/salesorders/{id}", updateDto);
        return response ?? throw new InvalidOperationException("Failed to update sales order");
    }

    public async Task<InvoiceDto> ConvertToInvoiceAsync(Guid id)
    {
        var response = await PostAsync<InvoiceDto>($"api/salesorders/{id}/convert-to-invoice", null);
        return response ?? throw new InvalidOperationException("Failed to convert sales order to invoice");
    }

    public async Task<bool> DeleteSalesOrderAsync(Guid id)
    {
        return await DeleteAsync($"api/salesorders/{id}");
    }
}
