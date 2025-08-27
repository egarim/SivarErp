using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Application.DTOs.Accounting;
using Sivar.Erp.Core.Shared.Responses;
using Sivar.Erp.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Core.Application.Services.Accounting;

/// <summary>
/// Stub implementation for journal entry service
/// </summary>
public class JournalEntryServiceStub : IJournalEntryService
{
    private readonly ILogger<JournalEntryServiceStub> _logger;

    public JournalEntryServiceStub(ILogger<JournalEntryServiceStub> logger)
    {
        _logger = logger;
    }

    public async Task<ApiResponse<JournalEntryDto>> CreateJournalEntryAsync(
        CreateJournalEntryDto dto, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        _logger.LogInformation("Creating journal entry for company {CompanyId}", companyId);
        
        var result = new JournalEntryDto
        {
            Id = Guid.NewGuid(),
            Description = dto.Description,
            Status = JournalEntryStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            Lines = new List<JournalEntryLineDto>()
        };
        
        return ApiResponse<JournalEntryDto>.Success(result);
    }

    public async Task<ApiResponse<JournalEntryDto>> GetJournalEntryByIdAsync(
        Guid id, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return ApiResponse<JournalEntryDto>.Failure("Journal entry not found");
    }

    public async Task<ApiResponse<IEnumerable<JournalEntryDto>>> GetJournalEntriesByDateRangeAsync(
        DateOnly startDate, 
        DateOnly endDate, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return ApiResponse<IEnumerable<JournalEntryDto>>.Success(new List<JournalEntryDto>());
    }

    public async Task<ApiResponse<IEnumerable<JournalEntryDto>>> GetJournalEntriesPagedAsync(
        int pageNumber, 
        int pageSize, 
        Guid companyId, 
        JournalEntryStatus? status = null, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return ApiResponse<IEnumerable<JournalEntryDto>>.Success(new List<JournalEntryDto>());
    }

    public async Task<ApiResponse<bool>> DeleteJournalEntryAsync(
        Guid id, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<JournalEntryDto>> PostJournalEntryAsync(
        Guid id, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return ApiResponse<JournalEntryDto>.Failure("Journal entry not found");
    }

    public async Task<ApiResponse<JournalEntryDto>> ReverseJournalEntryAsync(
        Guid id, 
        string reason, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return ApiResponse<JournalEntryDto>.Failure("Journal entry not found");
    }

    public async Task<ApiResponse<JournalEntryDto>> GetJournalEntryByReferenceAsync(
        string referenceNumber, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return ApiResponse<JournalEntryDto>.Failure("Journal entry not found");
    }

    public async Task<ApiResponse<TrialBalanceDto>> GetTrialBalanceAsync(
        DateTime asOfDate, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        
        var trialBalance = new TrialBalanceDto
        {
            AsOfDate = asOfDate,
            Lines = new List<TrialBalanceLineDto>(),
            TotalDebits = 0,
            TotalCredits = 0
        };
        
        return ApiResponse<TrialBalanceDto>.Success(trialBalance);
    }

    public async Task<ApiResponse<ValidationResult>> ValidateJournalEntryAsync(
        CreateJournalEntryDto dto, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        
        var validationResult = new ValidationResult("Valid");
        return ApiResponse<ValidationResult>.Success(validationResult);
    }
}
