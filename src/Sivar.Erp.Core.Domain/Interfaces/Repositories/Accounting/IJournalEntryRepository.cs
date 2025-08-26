using Sivar.Erp.Core.Domain.Entities.Accounting;
using Sivar.Erp.Core.Domain.Interfaces;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Domain.Interfaces.Repositories.Accounting;

/// <summary>
/// Repository interface for Journal Entry entities
/// </summary>
public interface IJournalEntryRepository : ITenantRepository<JournalEntry>
{
    /// <summary>
    /// Gets journal entry by number within a company
    /// </summary>
    Task<JournalEntry?> GetByNumberAsync(string number, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets journal entries by status
    /// </summary>
    Task<IEnumerable<JournalEntry>> GetByStatusAsync(JournalEntryStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets journal entries by date range
    /// </summary>
    Task<IEnumerable<JournalEntry>> GetByDateRangeAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets journal entries by accounting period
    /// </summary>
    Task<IEnumerable<JournalEntry>> GetByPeriodAsync(Guid periodId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets journal entries for a specific account
    /// </summary>
    Task<IEnumerable<JournalEntry>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets journal entries by approval status
    /// </summary>
    Task<IEnumerable<JournalEntry>> GetByApprovalStatusAsync(ApprovalStatus approvalStatus, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets journal entries by source document
    /// </summary>
    Task<IEnumerable<JournalEntry>> GetBySourceAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets journal entries with full details (including lines)
    /// </summary>
    Task<JournalEntry?> GetWithLinesAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets journal entries with full details by query parameters
    /// </summary>
    Task<IEnumerable<JournalEntry>> GetWithLinesAsync(
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        JournalEntryStatus? status = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if journal entry number exists within company
    /// </summary>
    Task<bool> NumberExistsAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets next available journal entry number
    /// </summary>
    Task<string> GetNextNumberAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Posts a journal entry (changes status to Posted)
    /// </summary>
    Task PostAsync(Guid id, string postedBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reverses a posted journal entry
    /// </summary>
    Task ReverseAsync(Guid id, string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Approves a journal entry
    /// </summary>
    Task ApproveAsync(Guid id, string approvedBy, string? notes = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rejects a journal entry
    /// </summary>
    Task RejectAsync(Guid id, string rejectedBy, string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches journal entries by description or reference
    /// </summary>
    Task<IEnumerable<JournalEntry>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets journal entry statistics
    /// </summary>
    Task<Dictionary<JournalEntryStatus, int>> GetStatusStatisticsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets total debit and credit amounts for a period
    /// </summary>
    Task<(decimal TotalDebit, decimal TotalCredit)> GetPeriodTotalsAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
}
