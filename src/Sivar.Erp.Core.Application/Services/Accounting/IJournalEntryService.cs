using Sivar.Erp.Core.Shared.DTOs.Accounting;
using Sivar.Erp.Core.Shared.Responses;
using Sivar.Erp.Core.Domain.Enums;

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
    Task<ApiResponse<List<JournalEntryDto>>> GetJournalEntriesAsync(
        Guid companyId,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        JournalEntryStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a journal entry
    /// </summary>
    Task<ApiResponse<string>> PostJournalEntryAsync(
        Guid id,
        PostJournalEntryDto dto,
        Guid companyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unposts a journal entry
    /// </summary>
    Task<ApiResponse<string>> UnpostJournalEntryAsync(
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
}
