using Sivar.Erp.Core.Domain.Entities.BusinessEntities;
using Sivar.Erp.Core.Shared.DTOs.BusinessEntities;

namespace Sivar.Erp.Core.Application.Services.BusinessEntities;

/// <summary>
/// Application service interface for Business Entity operations
/// </summary>
public interface IBusinessEntityService
{
    /// <summary>
    /// Get all business entities for the current company
    /// </summary>
    Task<IEnumerable<BusinessEntityDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business entity by ID
    /// </summary>
    Task<BusinessEntityDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business entity by code
    /// </summary>
    Task<BusinessEntityDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business entities by type
    /// </summary>
    Task<IEnumerable<BusinessEntityDto>> GetByTypeAsync(BusinessEntityType entityType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active business entities
    /// </summary>
    Task<IEnumerable<BusinessEntityDto>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search business entities by name or code
    /// </summary>
    Task<IEnumerable<BusinessEntityDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new business entity
    /// </summary>
    Task<BusinessEntityDto> CreateAsync(CreateBusinessEntityDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing business entity
    /// </summary>
    Task<BusinessEntityDto> UpdateAsync(Guid id, UpdateBusinessEntityDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a business entity (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a business entity code is available
    /// </summary>
    Task<bool> IsCodeAvailableAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customers with credit limits
    /// </summary>
    Task<IEnumerable<BusinessEntityDto>> GetCustomersWithCreditLimitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business entities for a specific branch
    /// </summary>
    Task<IEnumerable<BusinessEntityDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business entity statistics for dashboard
    /// </summary>
    Task<BusinessEntityStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Export business entities to CSV
    /// </summary>
    Task<byte[]> ExportToCsvAsync(BusinessEntityType? entityType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import business entities from CSV
    /// </summary>
    Task<ImportResultDto> ImportFromCsvAsync(Stream csvStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate business entity data
    /// </summary>
    Task<ValidationResultDto> ValidateAsync(CreateBusinessEntityDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Duplicate an existing business entity
    /// </summary>
    Task<BusinessEntityDto> DuplicateAsync(Guid sourceId, string newCode, string newName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Merge two business entities
    /// </summary>
    Task<BusinessEntityDto> MergeAsync(Guid primaryId, Guid secondaryId, CancellationToken cancellationToken = default);
}
