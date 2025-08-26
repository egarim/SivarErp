using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Application.DTOs.Accounting;
using Sivar.Erp.Core.Domain.Entities.Accounting;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.Accounting;
using Sivar.Erp.Core.Domain.Interfaces;
using Sivar.Erp.Core.Shared.Responses;

namespace Sivar.Erp.Core.Application.Services.Accounting;

/// <summary>
/// Service for managing chart of accounts and account operations
/// </summary>
public interface IAccountService
{
    Task<ApiResponse<AccountDto>> CreateAccountAsync(CreateAccountDto createAccountDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<AccountDto>> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<AccountSummaryDto>>> GetAccountsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of account management service
/// </summary>
public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<AccountDto>> CreateAccountAsync(CreateAccountDto createAccountDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if code already exists
            if (await _accountRepository.CodeExistsAsync(createAccountDto.Code, cancellationToken: cancellationToken))
            {
                return ApiResponse<AccountDto>.Failure("Account code already exists");
            }

            var account = new Account
            {
                Code = createAccountDto.Code,
                Name = createAccountDto.Name,
                Description = createAccountDto.Description,
                Type = createAccountDto.Type,
                Category = createAccountDto.Category,
                ParentAccountId = createAccountDto.ParentAccountId,
                IsActive = createAccountDto.IsActive,
                IsHeader = createAccountDto.IsHeader,
                TaxCode = createAccountDto.TaxCode,
                Currency = createAccountDto.Currency,
                OpeningBalance = createAccountDto.OpeningBalance,
                RequiresDepartment = createAccountDto.RequiresDepartment,
                RequiresProject = createAccountDto.RequiresProject,
                CompanyId = GetCurrentCompanyId()
            };

            await _accountRepository.AddAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var accountDto = MapToAccountDto(account);
            return ApiResponse<AccountDto>.Success(accountDto, "Account created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account with code {Code}", createAccountDto.Code);
            return ApiResponse<AccountDto>.Failure("An error occurred while creating the account");
        }
    }

    public async Task<ApiResponse<AccountDto>> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
            if (account == null)
            {
                return ApiResponse<AccountDto>.Failure("Account not found");
            }

            var accountDto = MapToAccountDto(account);
            return ApiResponse<AccountDto>.Success(accountDto, "Account retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account {Id}", id);
            return ApiResponse<AccountDto>.Failure("An error occurred while retrieving the account");
        }
    }

    public async Task<ApiResponse<IEnumerable<AccountSummaryDto>>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = await _accountRepository.GetAllAsync(cancellationToken);
            var accountDtos = accounts.Select(MapToAccountSummaryDto).ToList();
            return ApiResponse<IEnumerable<AccountSummaryDto>>.Success(accountDtos, "Accounts retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts");
            return ApiResponse<IEnumerable<AccountSummaryDto>>.Failure("An error occurred while retrieving accounts");
        }
    }

    private Guid GetCurrentCompanyId()
    {
        // This would normally come from the current user context
        // For now, we'll use a placeholder implementation
        return Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
    }

    private AccountDto MapToAccountDto(Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            CompanyId = account.CompanyId,
            Code = account.Code,
            Name = account.Name,
            Description = account.Description,
            Type = account.Type,
            TypeName = account.Type.ToString(),
            Category = account.Category,
            CategoryName = account.Category.ToString(),
            ParentAccountId = account.ParentAccountId,
            IsActive = account.IsActive,
            IsHeader = account.IsHeader,
            TaxCode = account.TaxCode,
            Currency = account.Currency,
            Balance = account.Balance,
            OpeningBalance = account.OpeningBalance,
            RequiresDepartment = account.RequiresDepartment,
            RequiresProject = account.RequiresProject,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            CreatedBy = account.CreatedBy ?? string.Empty,
            UpdatedBy = account.UpdatedBy
        };
    }

    private AccountSummaryDto MapToAccountSummaryDto(Account account)
    {
        return new AccountSummaryDto
        {
            Id = account.Id,
            Code = account.Code,
            Name = account.Name,
            Type = account.Type,
            TypeName = account.Type.ToString(),
            Category = account.Category,
            CategoryName = account.Category.ToString(),
            IsActive = account.IsActive,
            IsHeader = account.IsHeader,
            Balance = account.Balance,
            Currency = account.Currency
        };
    }
}
