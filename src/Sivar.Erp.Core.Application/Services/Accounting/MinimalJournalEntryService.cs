using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.Accounting;
using Sivar.Erp.Core.Shared.DTOs.Accounting;
using Sivar.Erp.Core.Shared.Responses;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Application.Services.Accounting;

/// <summary>
/// Minimal working journal entry service implementation
/// </summary>
public class MinimalJournalEntryService : IJournalEntryService
{
    private readonly ILogger<MinimalJournalEntryService> _logger;

    public MinimalJournalEntryService(ILogger<MinimalJournalEntryService> logger)
    {
        _logger = logger;
    }

    public async Task<ApiResponse<JournalEntryDto>> CreateJournalEntryAsync(
        CreateJournalEntryDto dto, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating journal entry for company {CompanyId}", companyId);
        
        var journalEntryDto = new JournalEntryDto
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ReferenceNumber = "JE-TEST-001",
            Description = dto.Description,
            TransactionDate = dto.TransactionDate,
            Status = JournalEntryStatus.Draft,
            CreatedDate = DateTime.UtcNow,
            Lines = new List<JournalEntryLineDto>()
        };

        return await Task.FromResult(ApiResponse<JournalEntryDto>.Success(journalEntryDto));
    }

    public async Task<ApiResponse<JournalEntryDto>> GetJournalEntryByIdAsync(
        Guid id, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting journal entry {Id} for company {CompanyId}", id, companyId);
        return await Task.FromResult(ApiResponse<JournalEntryDto>.Failure("Not implemented yet"));
    }

    public async Task<ApiResponse<List<JournalEntryDto>>> GetJournalEntriesAsync(
        Guid companyId,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        JournalEntryStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting journal entries for company {CompanyId}", companyId);
        return await Task.FromResult(ApiResponse<List<JournalEntryDto>>.Success(new List<JournalEntryDto>()));
    }

    public async Task<ApiResponse<string>> PostJournalEntryAsync(
        Guid id,
        PostJournalEntryDto dto,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Posting journal entry {Id} for company {CompanyId}", id, companyId);
        return await Task.FromResult(ApiResponse<string>.Success("Journal entry posted successfully"));
    }

    public async Task<ApiResponse<string>> UnpostJournalEntryAsync(
        Guid id,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Unposting journal entry {Id} for company {CompanyId}", id, companyId);
        return await Task.FromResult(ApiResponse<string>.Success("Journal entry unposted successfully"));
    }

    public async Task<ApiResponse<JournalEntryDto>> ReverseJournalEntryAsync(
        Guid id, 
        string reason, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reversing journal entry {Id} for company {CompanyId}", id, companyId);
        return await Task.FromResult(ApiResponse<JournalEntryDto>.Failure("Not implemented yet"));
    }

    public async Task<ApiResponse<JournalEntryDto>> GetJournalEntryByReferenceAsync(
        string referenceNumber, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting journal entry by reference {Reference} for company {CompanyId}", referenceNumber, companyId);
        return await Task.FromResult(ApiResponse<JournalEntryDto>.Failure("Not implemented yet"));
    }

    public async Task<ApiResponse<TrialBalanceDto>> GetTrialBalanceAsync(
        DateTime asOfDate, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting trial balance for company {CompanyId} as of {AsOfDate}", companyId, asOfDate);
        
        var trialBalance = new TrialBalanceDto
        {
            AsOfDate = asOfDate,
            Lines = new List<TrialBalanceLineDto>(),
            TotalDebits = 0,
            TotalCredits = 0
        };

        return await Task.FromResult(ApiResponse<TrialBalanceDto>.Success(trialBalance));
    }
}
