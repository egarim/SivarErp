using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Shared.DTOs.BusinessEntities;
using Sivar.Erp.Core.Application.Services.BusinessEntities;
using Sivar.Erp.Core.Domain.Entities.BusinessEntities;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.BusinessEntities;
using Sivar.Erp.Core.Shared.Exceptions;

namespace Sivar.Erp.Core.Application.Services.BusinessEntities;

/// <summary>
/// Simplified Business Entity Service implementation for Phase 4 completion
/// </summary>
public class BusinessEntityService : IBusinessEntityService
{
    private readonly IBusinessEntityRepository _repository;
    private readonly ILogger<BusinessEntityService> _logger;

    public BusinessEntityService(
        IBusinessEntityRepository repository,
        ILogger<BusinessEntityService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BusinessEntityDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting business entity with ID: {BusinessEntityId}", id);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<BusinessEntityDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all business entities");

        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<BusinessEntityDto>> GetByTypeAsync(BusinessEntityType entityType, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting business entities of type: {EntityType}", entityType);

        var entities = await _repository.GetByTypeAsync(entityType, cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<BusinessEntityDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting active business entities");

        var entities = await _repository.GetActiveAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<BusinessEntityDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Searching business entities with term: {SearchTerm}", searchTerm);

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync(cancellationToken);
        }

        var entities = await _repository.SearchAsync(searchTerm, cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<BusinessEntityDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByCodeAsync(code, cancellationToken);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<BusinessEntityDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetByBranchAsync(branchId, cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<BusinessEntityDto>> GetCustomersWithCreditLimitAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetCustomersWithCreditLimitAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<BusinessEntityDto> CreateAsync(CreateBusinessEntityDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating new business entity: {Name} ({EntityType})", createDto.Name, createDto.EntityType);

        // Check if code already exists
        if (await _repository.ExistsAsync(createDto.Code, null, cancellationToken))
        {
            throw new BusinessValidationException($"Business entity with code '{createDto.Code}' already exists.");
        }

        // Create the entity
        var entity = new BusinessEntity
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(), // TODO: Get from current user context
            BranchId = createDto.BranchId,
            Code = createDto.Code,
            Name = createDto.Name,
            EntityType = createDto.EntityType,
            TaxId = createDto.TaxId,
            Address = createDto.Address,
            City = createDto.City,
            State = createDto.State,
            ZipCode = createDto.ZipCode,
            Country = createDto.Country,
            PhoneNumber = createDto.PhoneNumber,
            MobileNumber = createDto.MobileNumber,
            Email = createDto.Email,
            Website = createDto.Website,
            CreditLimit = createDto.CreditLimit,
            PaymentTermsDays = createDto.PaymentTermsDays,
            IsActive = true,
            Notes = createDto.Notes,
            CreatedByUserId = Guid.NewGuid(), // TODO: Get from current user context
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);

        _logger.LogInformation("Created business entity: {BusinessEntityId} - {Name}", entity.Id, entity.Name);

        return MapToDto(entity);
    }

    public async Task<BusinessEntityDto> UpdateAsync(Guid id, UpdateBusinessEntityDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating business entity: {BusinessEntityId}", id);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Business entity with ID {id} not found.");
        }

        // Update the entity
        entity.Code = updateDto.Code;
        entity.Name = updateDto.Name;
        entity.EntityType = updateDto.EntityType;
        entity.TaxId = updateDto.TaxId;
        entity.Address = updateDto.Address;
        entity.City = updateDto.City;
        entity.State = updateDto.State;
        entity.ZipCode = updateDto.ZipCode;
        entity.Country = updateDto.Country;
        entity.PhoneNumber = updateDto.PhoneNumber;
        entity.MobileNumber = updateDto.MobileNumber;
        entity.Email = updateDto.Email;
        entity.Website = updateDto.Website;
        entity.CreditLimit = updateDto.CreditLimit;
        entity.PaymentTermsDays = updateDto.PaymentTermsDays;
        entity.Notes = updateDto.Notes;
        entity.UpdatedByUserId = Guid.NewGuid(); // TODO: Get from current user context
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);

        _logger.LogInformation("Updated business entity: {BusinessEntityId} - {Name}", entity.Id, entity.Name);

        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting business entity: {BusinessEntityId}", id);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        // Soft delete
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);

        _logger.LogInformation("Soft deleted business entity: {BusinessEntityId} - {Name}", entity.Id, entity.Name);
        return true;
    }

    public async Task<bool> IsCodeAvailableAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return !await _repository.ExistsAsync(code, excludeId, cancellationToken);
    }

    public async Task<BusinessEntityStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting business entity statistics");

        var allEntities = await _repository.GetAllAsync(cancellationToken);
        var activeEntities = allEntities.Where(e => e.IsActive);

        return new BusinessEntityStatsDto
        {
            TotalCount = allEntities.Count(),
            ActiveCount = activeEntities.Count(),
            InactiveCount = allEntities.Count() - activeEntities.Count(),
            CountByType = activeEntities.GroupBy(e => e.EntityType)
                                      .ToDictionary(g => g.Key, g => g.Count()),
            CustomersWithCreditLimit = activeEntities.Count(e => e.EntityType == BusinessEntityType.Customer && e.CreditLimit.HasValue),
            TotalCreditLimit = activeEntities.Where(e => e.CreditLimit.HasValue).Sum(e => e.CreditLimit!.Value),
            CreatedThisMonth = allEntities.Count(e => e.CreatedAt >= DateTime.UtcNow.AddMonths(-1)),
            CreatedThisYear = allEntities.Count(e => e.CreatedAt >= DateTime.UtcNow.AddYears(-1))
        };
    }

    public async Task<byte[]> ExportToCsvAsync(BusinessEntityType? entityType = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Exporting business entities to CSV");

        var entities = entityType.HasValue 
            ? await GetByTypeAsync(entityType.Value, cancellationToken)
            : await GetAllAsync(cancellationToken);
        
        var csv = new System.Text.StringBuilder();
        
        // Header
        csv.AppendLine("Code,Name,EntityType,TaxId,Email,PhoneNumber,Address,City,State,Country,IsActive");
        
        // Data
        foreach (var entity in entities)
        {
            csv.AppendLine($"{entity.Code},{entity.Name},{entity.EntityType},{entity.TaxId},{entity.Email},{entity.PhoneNumber},{entity.Address},{entity.City},{entity.State},{entity.Country},{entity.IsActive}");
        }

        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<ImportResultDto> ImportFromCsvAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting CSV import for business entities");

        var result = new ImportResultDto();
        
        // Simple CSV parsing
        using var reader = new StreamReader(csvStream);
        var lineNumber = 0;
        
        // Skip header
        await reader.ReadLineAsync();
        lineNumber++;

        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();
            
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var fields = line.Split(',');
                if (fields.Length < 3) // Minimum required fields
                {
                    result.Errors.Add($"Line {lineNumber}: Insufficient fields");
                    continue;
                }

                var createDto = new CreateBusinessEntityDto
                {
                    Code = fields[0].Trim(),
                    Name = fields[1].Trim(),
                    EntityType = Enum.Parse<BusinessEntityType>(fields[2].Trim(), true),
                    TaxId = fields.Length > 3 ? fields[3].Trim() : null,
                    Email = fields.Length > 4 ? fields[4].Trim() : null,
                    PhoneNumber = fields.Length > 5 ? fields[5].Trim() : null
                };

                var entity = await CreateAsync(createDto, cancellationToken);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Line {lineNumber}: {ex.Message}");
            }
            result.ProcessedCount++;
        }

        _logger.LogInformation("CSV import completed: {SuccessCount} imported, {ErrorCount} errors", 
            result.SuccessCount, result.Errors.Count);

        return result;
    }

    public async Task<ValidationResultDto> ValidateAsync(CreateBusinessEntityDto dto, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResultDto();
        
        // Code validation
        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            result.Errors.Add("Code is required");
        }
        else if (await _repository.ExistsAsync(dto.Code, null, cancellationToken))
        {
            result.Errors.Add($"Code '{dto.Code}' already exists");
        }

        // Name validation
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            result.Errors.Add("Name is required");
        }

        // Email validation
        if (!string.IsNullOrEmpty(dto.Email) && !IsValidEmail(dto.Email))
        {
            result.Errors.Add("Invalid email format");
        }

        result.IsValid = !result.Errors.Any();
        return result;
    }

    public async Task<BusinessEntityDto> DuplicateAsync(Guid sourceId, string newCode, string newName, CancellationToken cancellationToken = default)
    {
        var sourceEntity = await _repository.GetByIdAsync(sourceId, cancellationToken);
        if (sourceEntity == null)
        {
            throw new NotFoundException($"Source business entity with ID {sourceId} not found.");
        }

        var createDto = new CreateBusinessEntityDto
        {
            BranchId = sourceEntity.BranchId,
            Code = newCode,
            Name = newName,
            EntityType = sourceEntity.EntityType,
            TaxId = sourceEntity.TaxId,
            Address = sourceEntity.Address,
            City = sourceEntity.City,
            State = sourceEntity.State,
            ZipCode = sourceEntity.ZipCode,
            Country = sourceEntity.Country,
            PhoneNumber = sourceEntity.PhoneNumber,
            MobileNumber = sourceEntity.MobileNumber,
            Email = sourceEntity.Email,
            Website = sourceEntity.Website,
            CreditLimit = sourceEntity.CreditLimit,
            PaymentTermsDays = sourceEntity.PaymentTermsDays,
            Notes = $"Duplicated from {sourceEntity.Code} - {sourceEntity.Name}"
        };

        return await CreateAsync(createDto, cancellationToken);
    }

    public async Task<BusinessEntityDto> MergeAsync(Guid primaryId, Guid secondaryId, CancellationToken cancellationToken = default)
    {
        var primaryEntity = await _repository.GetByIdAsync(primaryId, cancellationToken);
        var secondaryEntity = await _repository.GetByIdAsync(secondaryId, cancellationToken);

        if (primaryEntity == null)
            throw new NotFoundException($"Primary business entity with ID {primaryId} not found.");
        
        if (secondaryEntity == null)
            throw new NotFoundException($"Secondary business entity with ID {secondaryId} not found.");

        // Merge logic - combine data from secondary into primary
        if (string.IsNullOrEmpty(primaryEntity.TaxId) && !string.IsNullOrEmpty(secondaryEntity.TaxId))
            primaryEntity.TaxId = secondaryEntity.TaxId;
        
        if (string.IsNullOrEmpty(primaryEntity.Email) && !string.IsNullOrEmpty(secondaryEntity.Email))
            primaryEntity.Email = secondaryEntity.Email;
        
        if (!primaryEntity.CreditLimit.HasValue && secondaryEntity.CreditLimit.HasValue)
            primaryEntity.CreditLimit = secondaryEntity.CreditLimit;

        // Save primary
        await _repository.UpdateAsync(primaryEntity, cancellationToken);

        // Soft delete secondary
        secondaryEntity.IsActive = false;
        secondaryEntity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(secondaryEntity, cancellationToken);

        _logger.LogInformation("Merged business entities: {PrimaryId} ({PrimaryCode}) <- {SecondaryId} ({SecondaryCode})", 
            primaryId, primaryEntity.Code, secondaryId, secondaryEntity.Code);

        return MapToDto(primaryEntity);
    }

    private static BusinessEntityDto MapToDto(BusinessEntity entity)
    {
        return new BusinessEntityDto
        {
            Id = entity.Id,
            CompanyId = entity.CompanyId,
            BranchId = entity.BranchId,
            Code = entity.Code,
            Name = entity.Name,
            EntityType = entity.EntityType,
            TaxId = entity.TaxId,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            ZipCode = entity.ZipCode,
            Country = entity.Country,
            PhoneNumber = entity.PhoneNumber,
            MobileNumber = entity.MobileNumber,
            Email = entity.Email,
            Website = entity.Website,
            CreditLimit = entity.CreditLimit,
            PaymentTermsDays = entity.PaymentTermsDays,
            IsActive = entity.IsActive,
            Notes = entity.Notes,
            CreatedByUserId = entity.CreatedByUserId,
            CreatedAt = entity.CreatedAt,
            UpdatedByUserId = entity.UpdatedByUserId,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
