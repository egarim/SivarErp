using Sivar.Erp.Core.Shared.Dtos.Sales;

namespace Sivar.Erp.Core.Client.Services.Sales;

/// <summary>
/// Interface for invoice API operations
/// </summary>
public interface IInvoiceApiService
{
    Task<IEnumerable<InvoiceDto>> GetInvoicesAsync(int page = 1, int pageSize = 50, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null);
    Task<InvoiceDto?> GetInvoiceByIdAsync(Guid id);
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceDto createDto);
    Task<InvoiceDto> UpdateInvoiceAsync(Guid id, UpdateInvoiceDto updateDto);
    Task<InvoiceDto> PostInvoiceAsync(Guid id);
    Task<InvoiceDto> CancelInvoiceAsync(Guid id, string? reason = null);
    Task<byte[]?> GetInvoicePdfAsync(Guid id);
    Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

/// <summary>
/// HTTP client implementation for invoice API operations
/// </summary>
public class InvoiceApiService : ApiClientBase, IInvoiceApiService
{
    public InvoiceApiService(HttpClient httpClient, ILogger<InvoiceApiService> logger) 
        : base(httpClient, logger)
    {
    }

    public async Task<IEnumerable<InvoiceDto>> GetInvoicesAsync(int page = 1, int pageSize = 50, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null)
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
        var response = await GetAsync<IEnumerable<InvoiceDto>>($"api/invoices?{query}");
        return response ?? new List<InvoiceDto>();
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid id)
    {
        return await GetAsync<InvoiceDto>($"api/invoices/{id}");
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceDto createDto)
    {
        var response = await PostAsync<InvoiceDto>("api/invoices", createDto);
        return response ?? throw new InvalidOperationException("Failed to create invoice");
    }

    public async Task<InvoiceDto> UpdateInvoiceAsync(Guid id, UpdateInvoiceDto updateDto)
    {
        var response = await PutAsync<InvoiceDto>($"api/invoices/{id}", updateDto);
        return response ?? throw new InvalidOperationException("Failed to update invoice");
    }

    public async Task<InvoiceDto> PostInvoiceAsync(Guid id)
    {
        var response = await PostAsync<InvoiceDto>($"api/invoices/{id}/post", null);
        return response ?? throw new InvalidOperationException("Failed to post invoice");
    }

    public async Task<InvoiceDto> CancelInvoiceAsync(Guid id, string? reason = null)
    {
        var response = await PostAsync<InvoiceDto>($"api/invoices/{id}/cancel", reason);
        return response ?? throw new InvalidOperationException("Failed to cancel invoice");
    }

    public async Task<byte[]?> GetInvoicePdfAsync(Guid id)
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/invoices/{id}/pdf");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting invoice PDF for {InvoiceId}", id);
            return null;
        }
    }

    public async Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var queryParams = new List<string>();

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var response = await GetAsync<InvoiceStatisticsDto>($"api/invoices/statistics{query}");
        return response ?? new InvoiceStatisticsDto();
    }
}
