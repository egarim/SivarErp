using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Logging;
using System.ComponentModel;
using Sivar.Erp.Core.Infrastructure.Repository;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Tax exemption service implementation
    /// </summary>
    [Description("Tax exemption service")]
    public class TaxExemptionService : ITaxExemptionService
    {
        private readonly IRepository _repository;
        private readonly ILogger<TaxExemptionService> _logger;

        public TaxExemptionService(
            IRepository repository,
            ILogger<TaxExemptionService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<TaxExemption> CreateExemptionAsync(TaxExemption exemption, string userId)
        {
            using var activity = LoggingExtensions.StartActivity("TaxExemption.Create");

            try
            {
                var validationResult = await ValidateExemptionAsync(exemption);
                if (!validationResult.IsValid)
                    throw new InvalidOperationException($"Exemption validation failed: {string.Join(", ", validationResult.Errors)}");

                exemption.Id = Guid.NewGuid();
                exemption.CreatedBy = userId;
                exemption.CreatedAt = DateTime.UtcNow;
                exemption.IsActive = true;

                // Save exemption (placeholder implementation)
                await SaveExemptionAsync(exemption);

                // Record audit trail
                await RecordExemptionAuditAsync(exemption.Id, "Created", userId, null, exemption.ToString());

                _logger.LogInformation("Created tax exemption {ExemptionId} for tax {TaxCode} by {UserId}", 
                    exemption.Id, exemption.TaxCode, userId);

                return exemption;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax exemption for tax {TaxCode}", exemption.TaxCode);
                throw;
            }
        }

        public async Task<List<TaxExemption>> GetExemptionsForEntityAsync(Guid businessEntityId)
        {
            using var activity = LoggingExtensions.StartActivity("TaxExemption.GetForEntity", businessEntityId);

            try
            {
                // Placeholder implementation - would query exemptions table
                return new List<TaxExemption>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exemptions for entity {BusinessEntityId}", businessEntityId);
                throw;
            }
        }

        public async Task<List<TaxExemption>> GetExemptionsForItemAsync(string itemCode)
        {
            using var activity = LoggingExtensions.StartActivity("TaxExemption.GetForItem", itemCode);

            try
            {
                // Placeholder implementation - would query exemptions table
                return new List<TaxExemption>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exemptions for item {ItemCode}", itemCode);
                throw;
            }
        }

        public async Task<bool> IsExemptFromTaxAsync(Guid businessEntityId, string itemCode, string taxCode)
        {
            using var activity = LoggingExtensions.StartActivity("TaxExemption.IsExempt");

            try
            {
                var entityExemptions = await GetExemptionsForEntityAsync(businessEntityId);
                var itemExemptions = await GetExemptionsForItemAsync(itemCode);

                var allExemptions = entityExemptions.Concat(itemExemptions);
                
                var exemption = allExemptions.FirstOrDefault(e => 
                    e.TaxCode == taxCode && 
                    e.IsActive && 
                    e.EffectiveDate <= DateTime.Now &&
                    (!e.ExpiryDate.HasValue || e.ExpiryDate > DateTime.Now));

                var isExempt = exemption != null;

                _logger.LogDebug("Tax exemption check for entity {EntityId}, item {ItemCode}, tax {TaxCode}: {IsExempt}", 
                    businessEntityId, itemCode, taxCode, isExempt);

                return isExempt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking tax exemption for entity {EntityId}, item {ItemCode}, tax {TaxCode}", 
                    businessEntityId, itemCode, taxCode);
                return false;
            }
        }

        public async Task UpdateExemptionAsync(Guid exemptionId, TaxExemption exemption, string userId)
        {
            using var activity = LoggingExtensions.StartActivity("TaxExemption.Update", exemptionId);

            try
            {
                var validationResult = await ValidateExemptionAsync(exemption);
                if (!validationResult.IsValid)
                    throw new InvalidOperationException($"Exemption validation failed: {string.Join(", ", validationResult.Errors)}");

                var existingExemption = await GetExemptionByIdAsync(exemptionId);
                if (existingExemption == null)
                    throw new InvalidOperationException($"Exemption {exemptionId} not found");

                var oldValue = existingExemption.ToString();
                
                // Update exemption
                exemption.Id = exemptionId;
                exemption.ModifiedBy = userId;
                exemption.ModifiedAt = DateTime.UtcNow;

                await SaveExemptionAsync(exemption);

                // Record audit trail
                await RecordExemptionAuditAsync(exemptionId, "Updated", userId, oldValue, exemption.ToString());

                _logger.LogInformation("Updated tax exemption {ExemptionId} by {UserId}", exemptionId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tax exemption {ExemptionId}", exemptionId);
                throw;
            }
        }

        public async Task DeactivateExemptionAsync(Guid exemptionId, string userId)
        {
            using var activity = LoggingExtensions.StartActivity("TaxExemption.Deactivate", exemptionId);

            try
            {
                var exemption = await GetExemptionByIdAsync(exemptionId);
                if (exemption == null)
                    throw new InvalidOperationException($"Exemption {exemptionId} not found");

                exemption.IsActive = false;
                exemption.ModifiedBy = userId;
                exemption.ModifiedAt = DateTime.UtcNow;

                await SaveExemptionAsync(exemption);

                // Record audit trail
                await RecordExemptionAuditAsync(exemptionId, "Deactivated", userId, "Active", "Inactive");

                _logger.LogInformation("Deactivated tax exemption {ExemptionId} by {UserId}", exemptionId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating tax exemption {ExemptionId}", exemptionId);
                throw;
            }
        }

        public async Task<List<TaxExemptionAudit>> GetExemptionAuditTrailAsync(Guid exemptionId)
        {
            using var activity = LoggingExtensions.StartActivity("TaxExemption.GetAuditTrail", exemptionId);

            try
            {
                // Placeholder implementation - would query audit table
                return new List<TaxExemptionAudit>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit trail for exemption {ExemptionId}", exemptionId);
                throw;
            }
        }

        public async Task<ValidationResult> ValidateExemptionAsync(TaxExemption exemption)
        {
            using var activity = LoggingExtensions.StartActivity("TaxExemption.Validate");

            try
            {
                var result = new ValidationResult { IsValid = true };

                if (string.IsNullOrWhiteSpace(exemption.TaxCode))
                    result.AddError("Tax code is required");

                if (!exemption.BusinessEntityId.HasValue && string.IsNullOrWhiteSpace(exemption.ItemCode))
                    result.AddError("Either business entity or item code must be specified");

                if (string.IsNullOrWhiteSpace(exemption.ExemptionReason))
                    result.AddError("Exemption reason is required");

                if (exemption.EffectiveDate > DateTime.Now.AddYears(1))
                    result.AddError("Effective date cannot be more than 1 year in the future");

                if (exemption.ExpiryDate.HasValue && exemption.ExpiryDate <= exemption.EffectiveDate)
                    result.AddError("Expiry date must be after effective date");

                // Check for overlapping exemptions
                var existingExemptions = new List<TaxExemption>(); // Would query database
                var overlapping = existingExemptions.Any(e => 
                    e.Id != exemption.Id &&
                    e.TaxCode == exemption.TaxCode &&
                    e.BusinessEntityId == exemption.BusinessEntityId &&
                    e.ItemCode == exemption.ItemCode &&
                    e.IsActive &&
                    e.EffectiveDate <= exemption.EffectiveDate &&
                    (!e.ExpiryDate.HasValue || e.ExpiryDate >= exemption.EffectiveDate));

                if (overlapping)
                    result.AddError("Overlapping exemption already exists");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tax exemption");
                throw;
            }
        }

        private async Task<TaxExemption?> GetExemptionByIdAsync(Guid exemptionId)
        {
            // Placeholder implementation - would query exemptions table
            await Task.CompletedTask;
            return null;
        }

        private async Task SaveExemptionAsync(TaxExemption exemption)
        {
            // Placeholder implementation - would save to exemptions table
            await Task.CompletedTask;
        }

        private async Task RecordExemptionAuditAsync(Guid exemptionId, string action, string userId, string? oldValue, string? newValue)
        {
            var audit = new TaxExemptionAudit
            {
                ExemptionId = exemptionId,
                Action = action,
                PerformedBy = userId,
                PerformedAt = DateTime.UtcNow,
                PreviousValue = oldValue,
                NewValue = newValue
            };

            // Placeholder implementation - would save to audit table
            await Task.CompletedTask;
        }
    }
}
