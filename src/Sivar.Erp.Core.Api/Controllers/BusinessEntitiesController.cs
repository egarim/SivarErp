using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sivar.Erp.Core.Shared.DTOs.BusinessEntities;
using Sivar.Erp.Core.Application.Services.BusinessEntities;
using Sivar.Erp.Core.Domain.Entities.BusinessEntities;
using Sivar.Erp.Core.Shared.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Core.API.Controllers;

/// <summary>
/// REST API controller for managing business entities (customers, suppliers, vendors, etc.)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessEntitiesController : ControllerBase
{
    private readonly IBusinessEntityService _businessEntityService;
    private readonly ILogger<BusinessEntitiesController> _logger;

    public BusinessEntitiesController(
        IBusinessEntityService businessEntityService,
        ILogger<BusinessEntitiesController> logger)
    {
        _businessEntityService = businessEntityService ?? throw new ArgumentNullException(nameof(businessEntityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all business entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of business entities</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BusinessEntityDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _businessEntityService.GetAllAsync(cancellationToken);
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all business entities");
            return StatusCode(500, "An error occurred while retrieving business entities");
        }
    }

    /// <summary>
    /// Get business entity by ID
    /// </summary>
    /// <param name="id">Business entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Business entity details</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BusinessEntityDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _businessEntityService.GetByIdAsync(id, cancellationToken);
            return Ok(entity);
        }
        catch (NotFoundException)
        {
            return NotFound($"Business entity with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business entity with ID: {BusinessEntityId}", id);
            return StatusCode(500, "An error occurred while retrieving the business entity");
        }
    }

    /// <summary>
    /// Get business entity by code
    /// </summary>
    /// <param name="code">Business entity code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Business entity details</returns>
    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<BusinessEntityDto>> GetByCode(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _businessEntityService.GetByCodeAsync(code, cancellationToken);
            if (entity == null)
            {
                return NotFound($"Business entity with code '{code}' not found");
            }
            return Ok(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business entity with code: {Code}", code);
            return StatusCode(500, "An error occurred while retrieving the business entity");
        }
    }

    /// <summary>
    /// Get business entities by type
    /// </summary>
    /// <param name="entityType">Type of business entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of business entities of the specified type</returns>
    [HttpGet("by-type/{entityType}")]
    public async Task<ActionResult<IEnumerable<BusinessEntityDto>>> GetByType(BusinessEntityType entityType, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _businessEntityService.GetByTypeAsync(entityType, cancellationToken);
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business entities of type: {EntityType}", entityType);
            return StatusCode(500, "An error occurred while retrieving business entities");
        }
    }

    /// <summary>
    /// Search business entities by name, code, or email
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching business entities</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<BusinessEntityDto>>> Search([FromQuery] string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _businessEntityService.SearchAsync(searchTerm, cancellationToken);
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching business entities with term: {SearchTerm}", searchTerm);
            return StatusCode(500, "An error occurred while searching business entities");
        }
    }

    /// <summary>
    /// Get customers with credit limit above specified amount
    /// </summary>
    /// <param name="minCreditLimit">Minimum credit limit</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customers with sufficient credit limit</returns>
    [HttpGet("customers/with-credit-limit")]
    public async Task<ActionResult<IEnumerable<BusinessEntityDto>>> GetCustomersWithCreditLimit(
        [FromQuery] decimal minCreditLimit, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _businessEntityService.GetCustomersWithCreditLimitAsync(cancellationToken);
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers with credit limit >= {MinCreditLimit}", minCreditLimit);
            return StatusCode(500, "An error occurred while retrieving customers");
        }
    }

    /// <summary>
    /// Create a new business entity
    /// </summary>
    /// <param name="createDto">Business entity data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created business entity</returns>
    [HttpPost]
    public async Task<ActionResult<BusinessEntityDto>> Create([FromBody] CreateBusinessEntityDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _businessEntityService.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }
        catch (BusinessValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating business entity: {Name}", createDto.Name);
            return StatusCode(500, "An error occurred while creating the business entity");
        }
    }

    /// <summary>
    /// Update an existing business entity
    /// </summary>
    /// <param name="id">Business entity ID</param>
    /// <param name="updateDto">Updated business entity data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated business entity</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BusinessEntityDto>> Update(Guid id, [FromBody] UpdateBusinessEntityDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _businessEntityService.UpdateAsync(id, updateDto, cancellationToken);
            return Ok(entity);
        }
        catch (NotFoundException)
        {
            return NotFound($"Business entity with ID {id} not found");
        }
        catch (BusinessValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business entity: {BusinessEntityId}", id);
            return StatusCode(500, "An error occurred while updating the business entity");
        }
    }

    /// <summary>
    /// Delete a business entity (soft delete)
    /// </summary>
    /// <param name="id">Business entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _businessEntityService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound($"Business entity with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business entity: {BusinessEntityId}", id);
            return StatusCode(500, "An error occurred while deleting the business entity");
        }
    }

    /// <summary>
    /// Check if a business entity code exists
    /// </summary>
    /// <param name="code">Business entity code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    [HttpGet("exists/{code}")]
    public async Task<ActionResult<bool>> Exists(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _businessEntityService.IsCodeAvailableAsync(code, null, cancellationToken);
            return Ok(!exists); // IsCodeAvailableAsync returns true if code is available (not exists), so we negate it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if business entity exists: {Code}", code);
            return StatusCode(500, "An error occurred while checking business entity existence");
        }
    }

    /// <summary>
    /// Get business entity statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Business entity statistics</returns>
    [HttpGet("statistics")]
    public async Task<ActionResult<BusinessEntityStatsDto>> GetStatistics(CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = await _businessEntityService.GetStatsAsync(cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business entity statistics");
            return StatusCode(500, "An error occurred while retrieving statistics");
        }
    }

    /// <summary>
    /// Import business entities from CSV file
    /// </summary>
    /// <param name="file">CSV file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import results</returns>
    [HttpPost("import/csv")]
    public async Task<ActionResult<ImportResultDto>> ImportFromCsv(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded or file is empty");
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("File must be a CSV file");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _businessEntityService.ImportFromCsvAsync(stream, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing business entities from CSV: {FileName}", file.FileName);
            return StatusCode(500, "An error occurred while importing business entities");
        }
    }

    /// <summary>
    /// Export business entities to CSV file
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CSV file</returns>
    [HttpGet("export/csv")]
    public async Task<ActionResult> ExportToCsv(CancellationToken cancellationToken = default)
    {
        try
        {
            var csvData = await _businessEntityService.ExportToCsvAsync(null, cancellationToken);
            var fileName = $"business-entities-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
            
            return File(csvData, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting business entities to CSV");
            return StatusCode(500, "An error occurred while exporting business entities");
        }
    }

    /// <summary>
    /// Duplicate a business entity with new code and name
    /// </summary>
    /// <param name="sourceId">Source business entity ID</param>
    /// <param name="newCode">New entity code</param>
    /// <param name="newName">New entity name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Duplicated business entity</returns>
    [HttpPost("{sourceId:guid}/duplicate")]
    public async Task<ActionResult<BusinessEntityDto>> Duplicate(
        Guid sourceId, 
        [FromQuery, Required] string newCode, 
        [FromQuery, Required] string newName, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _businessEntityService.DuplicateAsync(sourceId, newCode, newName, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BusinessValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating business entity: {SourceId}", sourceId);
            return StatusCode(500, "An error occurred while duplicating the business entity");
        }
    }

    /// <summary>
    /// Merge two business entities (secondary entity data merged into primary, secondary is soft deleted)
    /// </summary>
    /// <param name="primaryId">Primary business entity ID (target)</param>
    /// <param name="secondaryId">Secondary business entity ID (source)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Merged business entity</returns>
    [HttpPost("{primaryId:guid}/merge/{secondaryId:guid}")]
    public async Task<ActionResult<BusinessEntityDto>> Merge(
        Guid primaryId, 
        Guid secondaryId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _businessEntityService.MergeAsync(primaryId, secondaryId, cancellationToken);
            return Ok(entity);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging business entities: {PrimaryId} <- {SecondaryId}", primaryId, secondaryId);
            return StatusCode(500, "An error occurred while merging business entities");
        }
    }
}
