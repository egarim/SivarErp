using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Domain.Entities.Accounting;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.Accounting;
using Sivar.Erp.Core.Application.DTOs.Accounting;
using Sivar.Erp.Core.Shared.Responses;
using Sivar.Erp.Core.Domain.Enums;
using AccountTypeEnum = Sivar.Erp.Core.Domain.Enums.AccountType;

namespace Sivar.Erp.Core.Application.Services.Accounting;

/// <summary>
/// Service implementation for journal entry operations
/// </summary>
public class JournalEntryServiceImpl : IJournalEntryService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<JournalEntryServiceImpl> _logger;

    public JournalEntryServiceImpl(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        ILogger<JournalEntryServiceImpl> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<JournalEntryDto>> CreateJournalEntryAsync(
        CreateJournalEntryDto dto, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create the journal entry entity
            var journalEntry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Number = await _journalEntryRepository.GetNextNumberAsync(cancellationToken),
                Reference = dto.ReferenceNumber,
                TransactionDate = dto.EntryDate,
                Description = dto.Description,
                TotalDebit = dto.Lines.Sum(l => l.DebitAmount),
                TotalCredit = dto.Lines.Sum(l => l.CreditAmount),
                Status = JournalEntryStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            // Create journal entry lines
            foreach (var lineDto in dto.Lines)
            {
                var line = new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = lineDto.AccountId,
                    Description = lineDto.Description,
                    DebitAmount = lineDto.DebitAmount,
                    CreditAmount = lineDto.CreditAmount,
                    Reference = lineDto.Reference
                };
                journalEntry.Lines.Add(line);
            }

            // Save to repository
            await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
            
            _logger.LogInformation("Created journal entry {JournalEntryId} for company {CompanyId}", 
                journalEntry.Id, companyId);

            // Convert to DTO and return
            var resultDto = await MapToJournalEntryDto(journalEntry, cancellationToken);
            return ApiResponse<JournalEntryDto>.Success(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating journal entry for company {CompanyId}", companyId);
            return ApiResponse<JournalEntryDto>.Failure("An error occurred while creating the journal entry");
        }
    }

    public async Task<ApiResponse<JournalEntryDto>> GetJournalEntryByIdAsync(
        Guid id, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var journalEntry = await _journalEntryRepository.GetWithLinesAsync(id, cancellationToken);
            
            if (journalEntry == null || journalEntry.CompanyId != companyId)
            {
                return ApiResponse<JournalEntryDto>.Failure("Journal entry not found");
            }

            var dto = await MapToJournalEntryDto(journalEntry, cancellationToken);
            return ApiResponse<JournalEntryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entry {JournalEntryId}", id);
            return ApiResponse<JournalEntryDto>.Failure("An error occurred while retrieving the journal entry");
        }
    }

    public async Task<ApiResponse<IEnumerable<JournalEntryDto>>> GetJournalEntriesByDateRangeAsync(
        DateOnly startDate, 
        DateOnly endDate, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var journalEntries = await _journalEntryRepository.GetByDateRangeAsync(
                startDate, endDate, cancellationToken);

            var dtos = new List<JournalEntryDto>();
            foreach (var entry in journalEntries.Where(e => e.CompanyId == companyId))
            {
                var dto = await MapToJournalEntryDto(entry, cancellationToken);
                dtos.Add(dto);
            }

            return ApiResponse<IEnumerable<JournalEntryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entries by date range for company {CompanyId}", companyId);
            return ApiResponse<IEnumerable<JournalEntryDto>>.Failure("An error occurred while retrieving journal entries");
        }
    }

    public async Task<ApiResponse<IEnumerable<JournalEntryDto>>> GetJournalEntriesPagedAsync(
        int pageNumber, 
        int pageSize, 
        Guid companyId, 
        JournalEntryStatus? status = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the GetWithLinesAsync method for filtering
            var journalEntries = await _journalEntryRepository.GetWithLinesAsync(
                null, null, status, cancellationToken);

            // Filter by company and apply pagination
            var filteredEntries = journalEntries
                .Where(e => e.CompanyId == companyId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var dtos = new List<JournalEntryDto>();
            foreach (var entry in filteredEntries)
            {
                var dto = await MapToJournalEntryDto(entry, cancellationToken);
                dtos.Add(dto);
            }

            return ApiResponse<IEnumerable<JournalEntryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged journal entries for company {CompanyId}", companyId);
            return ApiResponse<IEnumerable<JournalEntryDto>>.Failure("An error occurred while retrieving journal entries");
        }
    }

    public async Task<ApiResponse<bool>> DeleteJournalEntryAsync(
        Guid id, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var journalEntry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            
            if (journalEntry == null || journalEntry.CompanyId != companyId)
            {
                return ApiResponse<bool>.Failure("Journal entry not found");
            }

            if (journalEntry.Status == JournalEntryStatus.Posted)
            {
                return ApiResponse<bool>.Failure("Cannot delete a posted journal entry");
            }

            await _journalEntryRepository.DeleteAsync(id, cancellationToken);
            
            _logger.LogInformation("Deleted journal entry {JournalEntryId} for company {CompanyId}", 
                id, companyId);

            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting journal entry {JournalEntryId}", id);
            return ApiResponse<bool>.Failure("An error occurred while deleting the journal entry");
        }
    }

    public async Task<ApiResponse<JournalEntryDto>> PostJournalEntryAsync(
        Guid id, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var journalEntry = await _journalEntryRepository.GetWithLinesAsync(id, cancellationToken);
            
            if (journalEntry == null || journalEntry.CompanyId != companyId)
            {
                return ApiResponse<JournalEntryDto>.Failure("Journal entry not found");
            }

            if (journalEntry.Status == JournalEntryStatus.Posted)
            {
                return ApiResponse<JournalEntryDto>.Failure("Journal entry is already posted");
            }

            // Validate balancing
            if (journalEntry.TotalDebit != journalEntry.TotalCredit)
            {
                return ApiResponse<JournalEntryDto>.Failure("Journal entry is not balanced");
            }

            // Use the repository's PostAsync method
            await _journalEntryRepository.PostAsync(id, "System", cancellationToken);
            
            // Re-fetch the updated entry
            var updatedEntry = await _journalEntryRepository.GetWithLinesAsync(id, cancellationToken);
            
            _logger.LogInformation("Posted journal entry {JournalEntryId} for company {CompanyId}", 
                id, companyId);

            var dto = await MapToJournalEntryDto(updatedEntry!, cancellationToken);
            return ApiResponse<JournalEntryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting journal entry {JournalEntryId}", id);
            return ApiResponse<JournalEntryDto>.Failure("An error occurred while posting the journal entry");
        }
    }

    public async Task<ApiResponse<JournalEntryDto>> ReverseJournalEntryAsync(
        Guid id, 
        string reason, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the repository's ReverseAsync method
            await _journalEntryRepository.ReverseAsync(id, reason, cancellationToken);
            
            _logger.LogInformation("Reversed journal entry {JournalEntryId} for company {CompanyId}", 
                id, companyId);

            // For now, return the original entry - in practice, the reverse method might create a new entry
            var journalEntry = await _journalEntryRepository.GetWithLinesAsync(id, cancellationToken);
            var dto = await MapToJournalEntryDto(journalEntry!, cancellationToken);
            return ApiResponse<JournalEntryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reversing journal entry {JournalEntryId}", id);
            return ApiResponse<JournalEntryDto>.Failure("An error occurred while reversing the journal entry");
        }
    }

    public async Task<ApiResponse<JournalEntryDto>> GetJournalEntryByReferenceAsync(
        string referenceNumber, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use search functionality since there's no direct GetByReference method
            var entries = await _journalEntryRepository.SearchAsync(referenceNumber, cancellationToken);
            var journalEntry = entries.FirstOrDefault(e => e.CompanyId == companyId && e.Reference == referenceNumber);
            
            if (journalEntry == null)
            {
                return ApiResponse<JournalEntryDto>.Failure("Journal entry not found");
            }

            var dto = await MapToJournalEntryDto(journalEntry, cancellationToken);
            return ApiResponse<JournalEntryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entry by reference {ReferenceNumber}", referenceNumber);
            return ApiResponse<JournalEntryDto>.Failure("An error occurred while retrieving the journal entry");
        }
    }

    public async Task<ApiResponse<TrialBalanceDto>> GetTrialBalanceAsync(
        DateTime asOfDate, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all accounts for the company using search or getAllAsync
            var allAccounts = await _accountRepository.GetAllAsync(cancellationToken);
            var accounts = allAccounts.Where(a => a.CompanyId == companyId);
            
            // Get all posted journal entries up to the as-of date
            var asOfDateOnly = DateOnly.FromDateTime(asOfDate);
            var postedEntries = await _journalEntryRepository.GetWithLinesAsync(null, asOfDateOnly, JournalEntryStatus.Posted, cancellationToken);
            var companyEntries = postedEntries.Where(e => e.CompanyId == companyId);
            
            var trialBalanceLines = new List<TrialBalanceLineDto>();
            decimal totalDebits = 0;
            decimal totalCredits = 0;

            foreach (var account in accounts)
            {
                decimal debitBalance = 0;
                decimal creditBalance = 0;

                // Calculate balances for this account
                var accountLines = companyEntries
                    .SelectMany(je => je.Lines)
                    .Where(line => line.AccountId == account.Id);

                var totalDebitsForAccount = accountLines.Sum(line => line.DebitAmount);
                var totalCreditsForAccount = accountLines.Sum(line => line.CreditAmount);

                // Determine normal balance based on account type
                var netAmount = totalDebitsForAccount - totalCreditsForAccount;
                
                if (account.AccountType == AccountTypeEnum.Asset || 
                    account.AccountType == AccountTypeEnum.Expense)
                {
                    // Debit normal accounts
                    if (netAmount > 0)
                        debitBalance = netAmount;
                    else if (netAmount < 0)
                        creditBalance = Math.Abs(netAmount);
                }
                else
                {
                    // Credit normal accounts (Liability, Equity, Revenue)
                    if (netAmount < 0)
                        creditBalance = Math.Abs(netAmount);
                    else if (netAmount > 0)
                        debitBalance = netAmount;
                }

                // Only include accounts with balances
                if (debitBalance != 0 || creditBalance != 0)
                {
                    trialBalanceLines.Add(new TrialBalanceLineDto
                    {
                        AccountId = account.Id,
                        AccountCode = account.AccountCode,
                        AccountName = account.AccountName,
                        AccountType = account.AccountType,
                        DebitBalance = debitBalance,
                        CreditBalance = creditBalance
                    });

                    totalDebits += debitBalance;
                    totalCredits += creditBalance;
                }
            }

            var trialBalance = new TrialBalanceDto
            {
                AsOfDate = asOfDate,
                Lines = trialBalanceLines.OrderBy(l => l.AccountCode).ToList(),
                TotalDebits = totalDebits,
                TotalCredits = totalCredits
            };

            return ApiResponse<TrialBalanceDto>.Success(trialBalance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating trial balance for company {CompanyId} as of {AsOfDate}", companyId, asOfDate);
            return ApiResponse<TrialBalanceDto>.Failure("An error occurred while generating the trial balance");
        }
    }

    public async Task<ApiResponse<ValidationResult>> ValidateJournalEntryAsync(
        CreateJournalEntryDto dto, 
        Guid companyId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = new ValidationResult(true, null);

            // Check if journal entry balances
            var totalDebits = dto.Lines.Sum(l => l.DebitAmount);
            var totalCredits = dto.Lines.Sum(l => l.CreditAmount);

            if (totalDebits != totalCredits)
            {
                var errorMessage = $"Journal entry is not balanced. Debits: {totalDebits:C}, Credits: {totalCredits:C}";
                return ApiResponse<ValidationResult>.Failure(errorMessage);
            }

            // Validate that each line has either a debit or credit (but not both)
            foreach (var line in dto.Lines)
            {
                if (line.DebitAmount > 0 && line.CreditAmount > 0)
                {
                    return ApiResponse<ValidationResult>.Failure("A line cannot have both debit and credit amounts");
                }

                if (line.DebitAmount == 0 && line.CreditAmount == 0)
                {
                    return ApiResponse<ValidationResult>.Failure("A line must have either a debit or credit amount");
                }
            }

            // Validate that all accounts exist and belong to the company
            var accountIds = dto.Lines.Select(l => l.AccountId).Distinct();
            foreach (var accountId in accountIds)
            {
                var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
                if (account == null || account.CompanyId != companyId)
                {
                    return ApiResponse<ValidationResult>.Failure($"Account {accountId} not found or does not belong to this company");
                }
            }

            return ApiResponse<ValidationResult>.Success(validationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating journal entry for company {CompanyId}", companyId);
            return ApiResponse<ValidationResult>.Failure("An error occurred while validating the journal entry");
        }
    }

    private async Task<JournalEntryDto> MapToJournalEntryDto(JournalEntry journalEntry, CancellationToken cancellationToken)
    {
        var lines = new List<JournalEntryLineDto>();
        
        foreach (var line in journalEntry.Lines)
        {
            var account = await _accountRepository.GetByIdAsync(line.AccountId, cancellationToken);
            
            lines.Add(new JournalEntryLineDto
            {
                Id = line.Id,
                AccountId = line.AccountId,
                AccountCode = account?.AccountCode ?? "UNKNOWN",
                AccountName = account?.AccountName ?? "Unknown Account",
                Description = line.Description,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                Reference = line.Reference
            });
        }

        return new JournalEntryDto
        {
            Id = journalEntry.Id,
            JournalEntryNumber = journalEntry.Number,
            ReferenceNumber = journalEntry.Reference,
            EntryDate = journalEntry.TransactionDate,
            Description = journalEntry.Description,
            TotalDebitAmount = journalEntry.TotalDebit,
            TotalCreditAmount = journalEntry.TotalCredit,
            Status = journalEntry.Status,
            PostedDate = journalEntry.PostedAt,
            CreatedAt = journalEntry.CreatedAt,
            Lines = lines
        };
    }
}
