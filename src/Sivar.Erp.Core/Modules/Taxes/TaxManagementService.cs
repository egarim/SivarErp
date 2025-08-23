using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Logging;
using System.ComponentModel;
using Sivar.Erp.Core.Infrastructure.Repository;
using Sivar.Erp.Core.Infrastructure.Validation;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Tax management service implementation
    /// </summary>
    [Description("Tax management service")]
    public class TaxManagementService : ITaxManagementService
    {
        private readonly IRepository _repository;
        private readonly ILogger<TaxManagementService> _logger;
        private readonly IValidationService _validationService;

        public TaxManagementService(
            IRepository repository,
            ILogger<TaxManagementService> logger,
            IValidationService validationService)
        {
            _repository = repository;
            _logger = logger;
            _validationService = validationService;
        }

        public async Task<ITax> CreateOrUpdateTaxAsync(TaxConfiguration taxConfig, string userId)
        {
            using var activity = LoggingExtensions.StartActivity("TaxManagement.CreateOrUpdate", taxConfig.Code);

            try
            {
                // Validate configuration
                var validationResult = await ValidateTaxConfigurationAsync(taxConfig);
                if (!validationResult.IsValid)
                    throw new InvalidOperationException($"Tax configuration validation failed: {string.Join(", ", validationResult.Errors)}");

                var existingTax = await GetTaxByCodeAsync(taxConfig.Code);
                
                ITax tax;
                if (existingTax != null)
                {
                    // Update existing
                    tax = UpdateTaxFromConfiguration(existingTax, taxConfig, userId);
                    await _repository.UpdateAsync(tax);
                    _logger.LogInformation("Updated tax configuration {TaxCode} by {UserId}", taxConfig.Code, userId);
                }
                else
                {
                    // Create new
                    tax = CreateTaxFromConfiguration(taxConfig, userId);
                    await _repository.CreateAsync(tax);
                    _logger.LogInformation("Created tax configuration {TaxCode} by {UserId}", taxConfig.Code, userId);
                }

                return tax;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating tax configuration {TaxCode}", taxConfig.Code);
                throw;
            }
        }

        public async Task DeactivateTaxAsync(string taxCode, string userId)
        {
            using var activity = LoggingExtensions.StartActivity("TaxManagement.Deactivate", taxCode);

            try
            {
                var tax = await GetTaxByCodeAsync(taxCode);
                if (tax == null)
                    throw new InvalidOperationException($"Tax {taxCode} not found");

                tax.IsActive = false;
                await _repository.UpdateAsync(tax);

                _logger.LogInformation("Deactivated tax {TaxCode} by {UserId}", taxCode, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating tax {TaxCode}", taxCode);
                throw;
            }
        }

        public async Task<ITax?> GetTaxByCodeAsync(string taxCode)
        {
            using var activity = LoggingExtensions.StartActivity("TaxManagement.GetByCode", taxCode);

            try
            {
                var taxes = _repository.GetObjects<TaxDto>();
                return taxes.FirstOrDefault(t => t.Code == taxCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tax by code {TaxCode}", taxCode);
                throw;
            }
        }

        public async Task<List<ITax>> GetTaxConfigurationsAsync(TaxConfigurationFilter? filter = null)
        {
            using var activity = LoggingExtensions.StartActivity("TaxManagement.GetConfigurations");

            try
            {
                var query = _repository.GetObjects<TaxDto>().AsQueryable();

                if (filter != null)
                {
                    if (!string.IsNullOrEmpty(filter.TaxCode))
                        query = query.Where(t => t.Code.Contains(filter.TaxCode));

                    if (filter.TaxType.HasValue)
                        query = query.Where(t => t.TaxType == filter.TaxType.Value);

                    if (filter.IsActive.HasValue)
                        query = query.Where(t => t.IsActive == filter.IsActive.Value);

                    if (!string.IsNullOrEmpty(filter.Country))
                        query = query.Where(t => t.Country == filter.Country);

                    if (!string.IsNullOrEmpty(filter.Region))
                        query = query.Where(t => t.Region == filter.Region);

                    if (filter.EffectiveDate.HasValue)
                        query = query.Where(t => t.EffectiveDate <= filter.EffectiveDate.Value);

                    if (filter.ExpiryDate.HasValue)
                        query = query.Where(t => !t.ExpiryDate.HasValue || t.ExpiryDate >= filter.ExpiryDate.Value);
                }

                return await Task.FromResult(query.OrderBy(t => t.Code).ToList<ITax>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tax configurations");
                throw;
            }
        }

        public async Task<ValidationResult> ValidateTaxConfigurationAsync(TaxConfiguration taxConfig)
        {
            using var activity = LoggingExtensions.StartActivity("TaxManagement.ValidateConfiguration", taxConfig.Code);

            try
            {
                var result = new ValidationResult { IsValid = true };

                // Basic validation
                if (string.IsNullOrWhiteSpace(taxConfig.Code))
                    result.AddError("Tax code is required");

                if (string.IsNullOrWhiteSpace(taxConfig.Name))
                    result.AddError("Tax name is required");

                if (taxConfig.Rate < 0 || taxConfig.Rate > 100)
                    result.AddError("Tax rate must be between 0 and 100");

                if (taxConfig.EffectiveDate > DateTime.Now.AddYears(1))
                    result.AddError("Effective date cannot be more than 1 year in the future");

                if (taxConfig.ExpiryDate.HasValue && taxConfig.ExpiryDate <= taxConfig.EffectiveDate)
                    result.AddError("Expiry date must be after effective date");

                // Check for duplicate codes
                var existingTax = await GetTaxByCodeAsync(taxConfig.Code);
                if (existingTax != null && existingTax.Code != taxConfig.Code)
                    result.AddError($"Tax code {taxConfig.Code} already exists");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tax configuration {TaxCode}", taxConfig.Code);
                throw;
            }
        }

        public async Task<TaxUsageStatistics> GetTaxUsageStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var activity = LoggingExtensions.StartActivity("TaxManagement.GetUsageStatistics");

            try
            {
                // This would typically query transaction data
                var statistics = new TaxUsageStatistics
                {
                    ReportGeneratedAt = DateTime.UtcNow,
                    ReportPeriod = $"{fromDate?.ToString("yyyy-MM-dd") ?? "All"} to {toDate?.ToString("yyyy-MM-dd") ?? "All"}",
                    TotalDocumentsProcessed = 0,
                    TotalTaxAmount = 0m
                };

                return await Task.FromResult(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tax usage statistics");
                throw;
            }
        }

        public async Task<int> ArchiveOldTaxConfigurationsAsync(DateTime cutoffDate, string userId)
        {
            using var activity = LoggingExtensions.StartActivity("TaxManagement.ArchiveOld");

            try
            {
                var oldTaxes = _repository.GetObjects<TaxDto>()
                    .Where(t => t.ExpiryDate.HasValue && t.ExpiryDate < cutoffDate && t.IsActive)
                    .ToList();

                foreach (var tax in oldTaxes)
                {
                    tax.IsActive = false;
                    await _repository.UpdateAsync(tax);
                }

                _logger.LogInformation("Archived {Count} old tax configurations by {UserId}", oldTaxes.Count, userId);
                return oldTaxes.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving old tax configurations");
                throw;
            }
        }

        public async Task<ITax> DuplicateTaxConfigurationAsync(string sourceTaxCode, string newTaxCode, string userId)
        {
            using var activity = LoggingExtensions.StartActivity("TaxManagement.Duplicate", sourceTaxCode);

            try
            {
                var sourceTax = await GetTaxByCodeAsync(sourceTaxCode);
                if (sourceTax == null)
                    throw new InvalidOperationException($"Source tax {sourceTaxCode} not found");

                var newTaxConfig = new TaxConfiguration
                {
                    Code = newTaxCode,
                    Name = $"{sourceTax.Name} (Copy)",
                    Description = sourceTax.Description,
                    TaxType = sourceTax.TaxType,
                    Rate = sourceTax.Rate,
                    EffectiveDate = DateTime.Now,
                    IsActive = true,
                    Country = sourceTax.Country,
                    Region = sourceTax.Region,
                    CalculationMethod = TaxCalculationMethod.Percentage,
                    RoundingMethod = TaxRoundingMethod.Round,
                    DecimalPlaces = 2,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                return await CreateOrUpdateTaxAsync(newTaxConfig, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating tax configuration {SourceTaxCode}", sourceTaxCode);
                throw;
            }
        }

        private ITax CreateTaxFromConfiguration(TaxConfiguration config, string userId)
        {
            var tax = _repository.CreateObject<TaxDto>();
            tax.Code = config.Code;
            tax.Name = config.Name;
            tax.Description = config.Description;
            tax.TaxType = config.TaxType;
            tax.Rate = config.Rate;
            tax.EffectiveDate = config.EffectiveDate;
            tax.ExpiryDate = config.ExpiryDate;
            tax.IsActive = config.IsActive;
            tax.Country = config.Country;
            tax.Region = config.Region;
            tax.CreatedBy = userId;
            tax.CreatedAt = DateTime.UtcNow;
            return tax;
        }

        private ITax UpdateTaxFromConfiguration(ITax existingTax, TaxConfiguration config, string userId)
        {
            existingTax.Name = config.Name;
            existingTax.Description = config.Description;
            existingTax.TaxType = config.TaxType;
            existingTax.Rate = config.Rate;
            existingTax.EffectiveDate = config.EffectiveDate;
            existingTax.ExpiryDate = config.ExpiryDate;
            existingTax.IsActive = config.IsActive;
            existingTax.Country = config.Country;
            existingTax.Region = config.Region;
            existingTax.ModifiedBy = userId;
            existingTax.ModifiedAt = DateTime.UtcNow;
            return existingTax;
        }
    }
}
