using Sivar.Erp.Core.Domain.Entities.Accounting;
using Sivar.Erp.Core.Domain.Interfaces;

namespace Sivar.Erp.Core.Domain.Interfaces.Repositories.Accounting;

/// <summary>
/// Repository interface for Accounting Period entities
/// </summary>
public interface IAccountingPeriodRepository : ITenantRepository<AccountingPeriod>
{
    /// <summary>
    /// Gets accounting period by name within a company
    /// </summary>
    Task<AccountingPeriod?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounting periods by fiscal year
    /// </summary>
    Task<IEnumerable<AccountingPeriod>> GetByFiscalYearAsync(Guid fiscalYearId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounting period by date
    /// </summary>
    Task<AccountingPeriod?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets current open accounting period
    /// </summary>
    Task<AccountingPeriod?> GetCurrentPeriodAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounting periods by status
    /// </summary>
    Task<IEnumerable<AccountingPeriod>> GetByStatusAsync(PeriodStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounting periods by type
    /// </summary>
    Task<IEnumerable<AccountingPeriod>> GetByTypeAsync(PeriodType type, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Closes an accounting period
    /// </summary>
    Task CloseAsync(Guid id, string closedBy, string? notes = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reopens a closed accounting period
    /// </summary>
    Task ReopenAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Locks an accounting period
    /// </summary>
    Task LockAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if period name exists within fiscal year
    /// </summary>
    Task<bool> NameExistsAsync(Guid fiscalYearId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if date range overlaps with existing periods
    /// </summary>
    Task<bool> DateRangeOverlapsAsync(Guid fiscalYearId, DateOnly startDate, DateOnly endDate, Guid? excludeId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets overlapping periods for validation
    /// </summary>
    Task<IEnumerable<AccountingPeriod>> GetOverlappingPeriodsAsync(DateOnly startDate, DateOnly endDate, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Fiscal Year entities
/// </summary>
public interface IFiscalYearRepository : ITenantRepository<FiscalYear>
{
    /// <summary>
    /// Gets fiscal year by name within a company
    /// </summary>
    Task<FiscalYear?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets fiscal year by date
    /// </summary>
    Task<FiscalYear?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets current fiscal year
    /// </summary>
    Task<FiscalYear?> GetCurrentAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets fiscal years by status
    /// </summary>
    Task<IEnumerable<FiscalYear>> GetByStatusAsync(FiscalYearStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets fiscal year with periods
    /// </summary>
    Task<FiscalYear?> GetWithPeriodsAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sets current fiscal year
    /// </summary>
    Task SetCurrentAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Closes a fiscal year
    /// </summary>
    Task CloseAsync(Guid id, string closedBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Locks a fiscal year
    /// </summary>
    Task LockAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if fiscal year name exists within company
    /// </summary>
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if date range overlaps with existing fiscal years
    /// </summary>
    Task<bool> DateRangeOverlapsAsync(DateOnly startDate, DateOnly endDate, Guid? excludeId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets overlapping fiscal years for validation
    /// </summary>
    Task<IEnumerable<FiscalYear>> GetOverlappingFiscalYearsAsync(DateOnly startDate, DateOnly endDate, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
