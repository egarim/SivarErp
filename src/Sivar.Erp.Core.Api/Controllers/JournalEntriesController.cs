using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Application.Services.Accounting;
using Sivar.Erp.Core.Application.DTOs.Accounting;
using Sivar.Erp.Core.Shared.Responses;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JournalEntriesController : ControllerBase
{
    private readonly IJournalEntryService _journalEntryService;
    private readonly ILogger<JournalEntriesController> _logger;

    public JournalEntriesController(
        IJournalEntryService journalEntryService,
        ILogger<JournalEntriesController> logger)
    {
        _journalEntryService = journalEntryService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new journal entry
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<JournalEntryDto>>> CreateJournalEntry(
        [FromBody] CreateJournalEntryDto dto,
        [FromHeader] Guid companyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _journalEntryService.CreateJournalEntryAsync(dto, companyId, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating journal entry");
            return StatusCode(500, ApiResponse<JournalEntryDto>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Gets journal entries with filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<JournalEntryDto>>>> GetJournalEntries(
        [FromHeader] Guid companyId,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] JournalEntryStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ApiResponse<IEnumerable<JournalEntryDto>> result;
            
            if (fromDate.HasValue && toDate.HasValue)
            {
                result = await _journalEntryService.GetJournalEntriesByDateRangeAsync(fromDate.Value, toDate.Value, companyId, cancellationToken);
            }
            else
            {
                result = await _journalEntryService.GetJournalEntriesPagedAsync(pageNumber, pageSize, companyId, status, cancellationToken);
            }
            
            var listResult = ApiResponse<List<JournalEntryDto>>.Success(result.Data?.ToList() ?? new List<JournalEntryDto>());
            return result.IsSuccess ? Ok(listResult) : BadRequest(listResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entries");
            return StatusCode(500, ApiResponse<List<JournalEntryDto>>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Gets a specific journal entry by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<JournalEntryDto>>> GetJournalEntry(
        Guid id,
        [FromHeader] Guid companyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _journalEntryService.GetJournalEntryByIdAsync(id, companyId, cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entry {Id}", id);
            return StatusCode(500, ApiResponse<JournalEntryDto>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Posts a journal entry to make it permanent
    /// </summary>
    [HttpPost("{id:guid}/post")]
    public async Task<ActionResult<ApiResponse<JournalEntryDto>>> PostJournalEntry(
        Guid id,
        [FromHeader] Guid companyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _journalEntryService.PostJournalEntryAsync(id, companyId, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting journal entry {Id}", id);
            return StatusCode(500, ApiResponse<JournalEntryDto>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Reverses a journal entry with a reason
    /// </summary>
    [HttpPost("{id:guid}/reverse")]
    public async Task<ActionResult<ApiResponse<JournalEntryDto>>> ReverseJournalEntry(
        Guid id,
        [FromBody] PostJournalEntryDto dto,
        [FromHeader] Guid companyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _journalEntryService.ReverseJournalEntryAsync(id, dto.Reason, companyId, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reversing journal entry {Id}", id);
            return StatusCode(500, ApiResponse<JournalEntryDto>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Gets trial balance report
    /// </summary>
    [HttpGet("trial-balance")]
    public async Task<ActionResult<ApiResponse<TrialBalanceDto>>> GetTrialBalance(
        [FromHeader] Guid companyId,
        [FromQuery] DateOnly asOfDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _journalEntryService.GetTrialBalanceAsync(asOfDate.ToDateTime(TimeOnly.MinValue), companyId, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating trial balance");
            return StatusCode(500, ApiResponse<TrialBalanceDto>.Failure("Internal server error"));
        }
    }
}
