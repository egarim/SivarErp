using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Client.Services.Abstractions;
using Sivar.Erp.Core.Client.Services.Http;
using Sivar.Erp.Core.Shared.DTOs.Identity;
using Sivar.Erp.Core.Shared.Responses;

namespace Sivar.Erp.Core.Client.Services.Http;

/// <summary>
/// HTTP client service for company management operations
/// </summary>
public class CompanyApiService : ApiClientBase, ICompanyApiService
{
    public CompanyApiService(HttpClient httpClient, ILogger<CompanyApiService> logger)
        : base(httpClient, logger)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResponse<CompanyDto>> CreateCompanyAsync(CreateCompanyDto createDto, CancellationToken cancellationToken = default)
    {
        return await PostAsync<CompanyDto>("api/companies", createDto, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<CompanyDto>> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<CompanyDto>($"api/companies/{companyId}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<IEnumerable<CompanyDto>>> GetUserCompaniesAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<IEnumerable<CompanyDto>>("api/companies", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<CompanyDto>> UpdateCompanyAsync(Guid companyId, UpdateCompanyDto updateDto, CancellationToken cancellationToken = default)
    {
        return await PutAsync<CompanyDto>($"api/companies/{companyId}", updateDto, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<bool>> DeleteCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DeleteAsync<bool>($"api/companies/{companyId}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<IEnumerable<BranchDto>>> GetCompanyBranchesAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        SetCompanyId(companyId);
        return await GetAsync<IEnumerable<BranchDto>>($"api/companies/{companyId}/branches", cancellationToken);
    }
}
