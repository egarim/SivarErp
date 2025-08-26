using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Application.DTOs.Accounting;
using Sivar.Erp.Core.Application.Services.Accounting;
using Sivar.Erp.Core.Domain.Entities.Accounting;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Api.Controllers;

/// <summary>
/// Controller for managing chart of accounts and account operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(
        IAccountService accountService,
        ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new account
    /// </summary>
    /// <param name="createAccountDto">Account creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created account details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAccount(
        [FromBody] CreateAccountDto createAccountDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _accountService.CreateAccountAsync(createAccountDto, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(
                nameof(GetAccountById),
                new { id = result.Data!.Id },
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            return StatusCode(500, "An error occurred while creating the account");
        }
    }

    /// <summary>
    /// Gets an account by ID
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccountById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _accountService.GetAccountByIdAsync(id, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the account");
        }
    }

    /// <summary>
    /// Gets all accounts for the current company
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of accounts</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccounts(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _accountService.GetAccountsAsync(cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts");
            return StatusCode(500, "An error occurred while retrieving accounts");
        }
    }

    /// <summary>
    /// Gets accounts by type
    /// </summary>
    /// <param name="type">Account type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of accounts of the specified type</returns>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<AccountSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccountsByType(
        AccountType type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, we'll use the general GetAccounts method
            // In the future, we can add filtering by type
            var result = await _accountService.GetAccountsAsync(cancellationToken);
            
            if (result.IsSuccess && result.Data != null)
            {
                var filteredAccounts = result.Data.Where(a => a.Type == type);
                var filteredResult = new Sivar.Erp.Core.Shared.Responses.ApiResponse<IEnumerable<AccountSummaryDto>>
                {
                    IsSuccess = true,
                    Data = filteredAccounts,
                    Message = "Accounts retrieved successfully"
                };
                return Ok(filteredResult);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts by type {Type}", type);
            return StatusCode(500, "An error occurred while retrieving accounts");
        }
    }

    /// <summary>
    /// Gets accounts by category
    /// </summary>
    /// <param name="category">Account category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of accounts of the specified category</returns>
    [HttpGet("by-category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<AccountSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccountsByCategory(
        AccountCategory category,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _accountService.GetAccountsAsync(cancellationToken);
            
            if (result.IsSuccess && result.Data != null)
            {
                var filteredAccounts = result.Data.Where(a => a.Category == category);
                var filteredResult = new Sivar.Erp.Core.Shared.Responses.ApiResponse<IEnumerable<AccountSummaryDto>>
                {
                    IsSuccess = true,
                    Data = filteredAccounts,
                    Message = "Accounts retrieved successfully"
                };
                return Ok(filteredResult);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts by category {Category}", category);
            return StatusCode(500, "An error occurred while retrieving accounts");
        }
    }

    /// <summary>
    /// Gets the chart of accounts in hierarchical structure
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive accounts</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chart of accounts</returns>
    [HttpGet("chart")]
    [ProducesResponseType(typeof(IEnumerable<AccountSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChartOfAccounts(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _accountService.GetAccountsAsync(cancellationToken);
            
            if (result.IsSuccess && result.Data != null)
            {
                var accounts = includeInactive 
                    ? result.Data 
                    : result.Data.Where(a => a.IsActive);
                
                // Sort by type and then by code for chart of accounts view
                var sortedAccounts = accounts
                    .OrderBy(a => a.Type)
                    .ThenBy(a => a.Code);
                
                var chartResult = new Sivar.Erp.Core.Shared.Responses.ApiResponse<IEnumerable<AccountSummaryDto>>
                {
                    IsSuccess = true,
                    Data = sortedAccounts,
                    Message = "Chart of accounts retrieved successfully"
                };
                return Ok(chartResult);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chart of accounts");
            return StatusCode(500, "An error occurred while retrieving chart of accounts");
        }
    }

    /// <summary>
    /// Gets account types for dropdown/selection purposes
    /// </summary>
    /// <returns>List of account types</returns>
    [HttpGet("types")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public IActionResult GetAccountTypes()
    {
        try
        {
            var accountTypes = Enum.GetValues<AccountType>()
                .Select(type => new
                {
                    Value = (int)type,
                    Name = type.ToString(),
                    DisplayName = GetAccountTypeDisplayName(type)
                });

            return Ok(new Sivar.Erp.Core.Shared.Responses.ApiResponse<IEnumerable<object>>
            {
                IsSuccess = true,
                Data = accountTypes,
                Message = "Account types retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account types");
            return StatusCode(500, "An error occurred while retrieving account types");
        }
    }

    /// <summary>
    /// Gets account categories for dropdown/selection purposes
    /// </summary>
    /// <returns>List of account categories</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public IActionResult GetAccountCategories()
    {
        try
        {
            var accountCategories = Enum.GetValues<AccountCategory>()
                .Select(category => new
                {
                    Value = (int)category,
                    Name = category.ToString(),
                    DisplayName = GetAccountCategoryDisplayName(category),
                    Type = GetCategoryAccountType(category)
                });

            return Ok(new Sivar.Erp.Core.Shared.Responses.ApiResponse<IEnumerable<object>>
            {
                IsSuccess = true,
                Data = accountCategories,
                Message = "Account categories retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account categories");
            return StatusCode(500, "An error occurred while retrieving account categories");
        }
    }

    private static string GetAccountTypeDisplayName(AccountType type)
    {
        return type switch
        {
            AccountType.Asset => "Assets",
            AccountType.Liability => "Liabilities",
            AccountType.Equity => "Equity",
            AccountType.Revenue => "Revenue",
            AccountType.Expense => "Expenses",
            _ => type.ToString()
        };
    }

    private static string GetAccountCategoryDisplayName(AccountCategory category)
    {
        return category switch
        {
            AccountCategory.CurrentAssets => "Current Assets",
            AccountCategory.FixedAssets => "Fixed Assets",
            AccountCategory.IntangibleAssets => "Intangible Assets",
            AccountCategory.Investments => "Investments",
            AccountCategory.CurrentLiabilities => "Current Liabilities",
            AccountCategory.LongTermLiabilities => "Long-term Liabilities",
            AccountCategory.Capital => "Capital",
            AccountCategory.RetainedEarnings => "Retained Earnings",
            AccountCategory.OperatingRevenue => "Operating Revenue",
            AccountCategory.NonOperatingRevenue => "Non-operating Revenue",
            AccountCategory.CostOfGoodsSold => "Cost of Goods Sold",
            AccountCategory.OperatingExpenses => "Operating Expenses",
            AccountCategory.NonOperatingExpenses => "Non-operating Expenses",
            AccountCategory.TaxExpenses => "Tax Expenses",
            _ => category.ToString()
        };
    }

    private static AccountType GetCategoryAccountType(AccountCategory category)
    {
        return category switch
        {
            AccountCategory.CurrentAssets => AccountType.Asset,
            AccountCategory.FixedAssets => AccountType.Asset,
            AccountCategory.IntangibleAssets => AccountType.Asset,
            AccountCategory.Investments => AccountType.Asset,
            AccountCategory.CurrentLiabilities => AccountType.Liability,
            AccountCategory.LongTermLiabilities => AccountType.Liability,
            AccountCategory.Capital => AccountType.Equity,
            AccountCategory.RetainedEarnings => AccountType.Equity,
            AccountCategory.OperatingRevenue => AccountType.Revenue,
            AccountCategory.NonOperatingRevenue => AccountType.Revenue,
            AccountCategory.CostOfGoodsSold => AccountType.Expense,
            AccountCategory.OperatingExpenses => AccountType.Expense,
            AccountCategory.NonOperatingExpenses => AccountType.Expense,
            AccountCategory.TaxExpenses => AccountType.Expense,
            _ => AccountType.Asset
        };
    }
}
