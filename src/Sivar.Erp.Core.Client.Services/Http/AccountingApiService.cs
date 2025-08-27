using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Client.Services.Abstractions;
using Sivar.Erp.Core.Client.Services.Http;
using Sivar.Erp.Core.Shared.DTOs.Accounting;
using Sivar.Erp.Core.Shared.Responses;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Client.Services.Http;

/// <summary>
/// HTTP client service for accounting operations
/// </summary>
public class AccountingApiService : ApiClientBase, IAccountingApiService
{
    public AccountingApiService(HttpClient httpClient, ILogger<AccountingApiService> logger)
        : base(httpClient, logger)
    {
    }

    // Journal Entry Operations
    /// <inheritdoc />
    public async Task<ApiResponse<JournalEntryDto>> CreateJournalEntryAsync(CreateJournalEntryDto createDto, Guid companyId, CancellationToken cancellationToken = default)
    {
        SetCompanyId(companyId);
        return await PostAsync<JournalEntryDto>("api/journalentries", createDto, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<JournalEntryDto>> GetJournalEntryByIdAsync(Guid journalEntryId, Guid companyId, CancellationToken cancellationToken = default)
    {
        SetCompanyId(companyId);
        return await GetAsync<JournalEntryDto>($"api/journalentries/{journalEntryId}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<List<JournalEntryDto>>> GetJournalEntriesAsync(
        Guid companyId,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        JournalEntryStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        SetCompanyId(companyId);
        
        var queryParams = new List<string>();
        if (fromDate.HasValue)
            queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue)
            queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (status.HasValue)
            queryParams.Add($"status={status.Value}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        return await GetAsync<List<JournalEntryDto>>($"api/journalentries{queryString}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<string>> PostJournalEntryAsync(Guid journalEntryId, PostJournalEntryDto postDto, Guid companyId, CancellationToken cancellationToken = default)
    {
        SetCompanyId(companyId);
        return await PostAsync<string>($"api/journalentries/{journalEntryId}/post", postDto, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<string>> UnpostJournalEntryAsync(Guid journalEntryId, Guid companyId, CancellationToken cancellationToken = default)
    {
        SetCompanyId(companyId);
        return await PostAsync<string>($"api/journalentries/{journalEntryId}/unpost", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<JournalEntryDto>> ReverseJournalEntryAsync(Guid journalEntryId, ReverseJournalEntryDto reverseDto, Guid companyId, CancellationToken cancellationToken = default)
    {
        SetCompanyId(companyId);
        return await PostAsync<JournalEntryDto>($"api/journalentries/{journalEntryId}/reverse", reverseDto, cancellationToken);
    }

    // Account Operations
    /// <inheritdoc />
    public async Task<ApiResponse<List<AccountDto>>> GetAccountsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        SetCompanyId(companyId);
        return await GetAsync<List<AccountDto>>("api/accounts", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<AccountDto>> CreateAccountAsync(CreateAccountDto createDto, Guid companyId, CancellationToken cancellationToken = default)
    {
        SetCompanyId(companyId);
        return await PostAsync<AccountDto>("api/accounts", createDto, cancellationToken);
    }

    // Reports
    /// <inheritdoc />
    public async Task<ApiResponse<TrialBalanceDto>> GetTrialBalanceAsync(Guid companyId, DateTime asOfDate, CancellationToken cancellationToken = default)
    {
        SetCompanyId(companyId);
        return await GetAsync<TrialBalanceDto>($"api/journalentries/trial-balance?asOfDate={asOfDate:yyyy-MM-dd}", cancellationToken);
    }
}
