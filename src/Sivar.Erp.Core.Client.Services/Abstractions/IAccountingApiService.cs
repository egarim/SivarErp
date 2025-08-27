using Sivar.Erp.Core.Shared.DTOs.Accounting;
using Sivar.Erp.Core.Shared.Responses;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Client.Services.Abstractions;

/// <summary>
/// Accounting API service interface for client applications
/// </summary>
public interface IAccountingApiService
{
    // Journal Entry Operations
    /// <summary>
    /// Creates a new journal entry
    /// </summary>
    Task<ApiResponse<JournalEntryDto>> CreateJournalEntryAsync(CreateJournalEntryDto createDto, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a journal entry by ID
    /// </summary>
    Task<ApiResponse<JournalEntryDto>> GetJournalEntryByIdAsync(Guid journalEntryId, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journal entries with filtering
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
    Task<ApiResponse<string>> PostJournalEntryAsync(Guid journalEntryId, PostJournalEntryDto postDto, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unposts a journal entry
    /// </summary>
    Task<ApiResponse<string>> UnpostJournalEntryAsync(Guid journalEntryId, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverses a journal entry
    /// </summary>
    Task<ApiResponse<JournalEntryDto>> ReverseJournalEntryAsync(Guid journalEntryId, ReverseJournalEntryDto reverseDto, Guid companyId, CancellationToken cancellationToken = default);

    // Account Operations
    /// <summary>
    /// Gets all accounts for a company
    /// </summary>
    Task<ApiResponse<List<AccountDto>>> GetAccountsAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new account
    /// </summary>
    Task<ApiResponse<AccountDto>> CreateAccountAsync(CreateAccountDto createDto, Guid companyId, CancellationToken cancellationToken = default);

    // Reports
    /// <summary>
    /// Gets trial balance report
    /// </summary>
    Task<ApiResponse<TrialBalanceDto>> GetTrialBalanceAsync(Guid companyId, DateTime asOfDate, CancellationToken cancellationToken = default);
}
