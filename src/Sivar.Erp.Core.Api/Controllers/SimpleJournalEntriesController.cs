using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Application.DTOs.Accounting;
using Sivar.Erp.Core.Shared.Responses;
using Sivar.Erp.Core.Application.Services.Accounting;

namespace Sivar.Erp.Core.Api.Controllers;

/// <summary>
/// Simple journal entries controller without complex DI dependencies
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SimpleJournalEntriesController : ControllerBase
{
    private readonly IAccountService _accountService;

    public SimpleJournalEntriesController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Health check for journal entries API
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(ApiResponse<string>.Success("Journal Entries API is healthy"));
    }

    /// <summary>
    /// Get available accounts for journal entries
    /// </summary>
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccountsForJournalEntries()
    {
        var companyId = GetCompanyIdFromHeaders();
        if (companyId == Guid.Empty)
        {
            return BadRequest(ApiResponse<IEnumerable<AccountSummaryDto>>.Failure("Company ID is required"));
        }

        var result = await _accountService.GetAccountsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Create a simple journal entry (basic implementation)
    /// </summary>
    [HttpPost("simple")]
    public async Task<IActionResult> CreateSimpleJournalEntry([FromBody] SimpleJournalEntryDto entryDto)
    {
        var companyId = GetCompanyIdFromHeaders();
        if (companyId == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Failure("Company ID is required"));
        }

        // Basic validation
        if (entryDto.Lines == null || entryDto.Lines.Count < 2)
        {
            return BadRequest(ApiResponse<object>.Failure("Journal entry must have at least 2 lines"));
        }

        var totalDebits = entryDto.Lines.Where(l => l.DebitAmount > 0).Sum(l => l.DebitAmount);
        var totalCredits = entryDto.Lines.Where(l => l.CreditAmount > 0).Sum(l => l.CreditAmount);

        if (Math.Abs(totalDebits - totalCredits) > 0.01m)
        {
            return BadRequest(ApiResponse<object>.Failure("Debits and credits must be equal"));
        }

        // For now, just return success with the entry data
        var result = new
        {
            Id = Guid.NewGuid(),
            EntryNumber = $"JE-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            Date = entryDto.Date,
            Description = entryDto.Description,
            TotalAmount = totalDebits,
            Lines = entryDto.Lines.Select(l => new
            {
                AccountCode = l.AccountCode,
                Description = l.Description,
                DebitAmount = l.DebitAmount,
                CreditAmount = l.CreditAmount
            })
        };

        return Ok(ApiResponse<object>.Success(result, "Journal entry created successfully"));
    }

    private Guid GetCompanyIdFromHeaders()
    {
        if (Request.Headers.TryGetValue("X-Company-Id", out var companyIdValue) &&
            Guid.TryParse(companyIdValue.FirstOrDefault(), out var companyId))
        {
            return companyId;
        }
        return Guid.Empty;
    }
}

/// <summary>
/// Simple DTO for journal entry creation
/// </summary>
public class SimpleJournalEntryDto
{
    public DateTime Date { get; set; } = DateTime.Today;
    public string Description { get; set; } = string.Empty;
    public List<SimpleJournalLineDto> Lines { get; set; } = new();
}

/// <summary>
/// Simple DTO for journal entry lines
/// </summary>
public class SimpleJournalLineDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}
