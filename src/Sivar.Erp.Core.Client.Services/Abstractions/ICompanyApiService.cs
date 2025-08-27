using Sivar.Erp.Core.Shared.DTOs.Identity;
using Sivar.Erp.Core.Shared.Responses;

namespace Sivar.Erp.Core.Client.Services.Abstractions;

/// <summary>
/// Company management API service interface for client applications
/// </summary>
public interface ICompanyApiService
{
    /// <summary>
    /// Creates a new company
    /// </summary>
    Task<ApiResponse<CompanyDto>> CreateCompanyAsync(CreateCompanyDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a company by ID
    /// </summary>
    Task<ApiResponse<CompanyDto>> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all companies for the current user
    /// </summary>
    Task<ApiResponse<IEnumerable<CompanyDto>>> GetUserCompaniesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing company
    /// </summary>
    Task<ApiResponse<CompanyDto>> UpdateCompanyAsync(Guid companyId, UpdateCompanyDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a company
    /// </summary>
    Task<ApiResponse<bool>> DeleteCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets company branches
    /// </summary>
    Task<ApiResponse<IEnumerable<BranchDto>>> GetCompanyBranchesAsync(Guid companyId, CancellationToken cancellationToken = default);
}
