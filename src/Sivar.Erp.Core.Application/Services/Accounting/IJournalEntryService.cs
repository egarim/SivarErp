using Sivar.Erp.Core.Application.DTOs.Accounting;
using Sivar.Erp.Core.Shared.Responses;
using Sivar.Erp.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Core.Application.Services.Accounting;

/// <summary>
/// Interface for journal entry operations
/// </summary>
public interface IJournalEntryService
{
    /// <summary>
    /// Creates a new journal entry
    /// </summary>
    Task<ApiResponse<JournalEntryDto>> CreateJournalEntryAsync(
        CreateJournalEntryDto dto, 
        Guid companyId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a journal entry by ID
    /// </summary>
    Task<ApiResponse<JournalEntryDto>> GetJournalEntryByIdAsync(
        Guid id, 
        Guid companyId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journal entries by date range
    /// </summary>
    Task<ApiResponse<IEnumerable<JournalEntryDto>>> GetJournalEntriesByDateRangeAsync(
        DateOnly startDate, 
        DateOnly endDate, 
        Guid companyId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journal entries with pagination
    /// </summary>
    Task<ApiResponse<IEnumerable<JournalEntryDto>>> GetJournalEntriesPagedAsync(
        int pageNumber, 
        int pageSize, 
        Guid companyId, 
        JournalEntryStatus? status = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a journal entry (only if not posted)
    /// </summary>
    Task<ApiResponse<bool>> DeleteJournalEntryAsync(
        Guid id, 
        Guid companyId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a journal entry (makes it permanent and affects balances)
    /// </summary>
    Task<ApiResponse<JournalEntryDto>> PostJournalEntryAsync(
        Guid id, 
        Guid companyId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverses a posted journal entry
    /// </summary>
    Task<ApiResponse<JournalEntryDto>> ReverseJournalEntryAsync(
        Guid id, 
        string reason, 
        Guid companyId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journal entry by reference number
    /// </summary>
    Task<ApiResponse<JournalEntryDto>> GetJournalEntryByReferenceAsync(
        string referenceNumber, 
        Guid companyId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trial balance for a specific date
    /// </summary>
    Task<ApiResponse<TrialBalanceDto>> GetTrialBalanceAsync(
        DateTime asOfDate, 
        Guid companyId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a journal entry before creation
    /// </summary>
    Task<ApiResponse<ValidationResult>> ValidateJournalEntryAsync(
        CreateJournalEntryDto dto, 
        Guid companyId, 
        CancellationToken cancellationToken = default);
}
