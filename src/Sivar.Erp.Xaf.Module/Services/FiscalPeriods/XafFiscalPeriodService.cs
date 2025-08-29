using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Accounting.FiscalPeriods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.FiscalPeriods
{
    /// <summary>
    /// XAF implementation of fiscal period service using IObjectSpace
    /// </summary>
    public class XafFiscalPeriodService : IFiscalPeriodService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly FiscalPeriodValidator _fiscalPeriodValidator;
        private readonly ILogger<XafFiscalPeriodService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafFiscalPeriodService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafFiscalPeriodService(IObjectSpace objectSpace, ILogger<XafFiscalPeriodService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _fiscalPeriodValidator = new FiscalPeriodValidator();
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the XafFiscalPeriodService class with a custom validator
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="fiscalPeriodValidator">Custom fiscal period validator</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafFiscalPeriodService(IObjectSpace objectSpace, FiscalPeriodValidator fiscalPeriodValidator, ILogger<XafFiscalPeriodService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _fiscalPeriodValidator = fiscalPeriodValidator ?? new FiscalPeriodValidator();
            _logger = logger;
        }

        /// <summary>
        /// Creates a new fiscal period using XAF ObjectSpace
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to create</param>
        /// <param name="userId">User creating the fiscal period</param>
        /// <returns>Created fiscal period with ID</returns>
        public Task<IFiscalPeriod> CreateFiscalPeriodAsync(IFiscalPeriod fiscalPeriod, string userId)
        {
            return Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Creating fiscal period with code: {Code} for user: {UserId}", 
                        fiscalPeriod?.Code, userId);

                    if (fiscalPeriod == null)
                        throw new ArgumentNullException(nameof(fiscalPeriod));

                    if (string.IsNullOrWhiteSpace(userId))
                        throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

                    // Convert interface to DTO for validation compatibility
                    var fiscalPeriodDto = CreateFiscalPeriodDtoFromInterface(fiscalPeriod);

                    // Validate the fiscal period using the validator
                    if (!_fiscalPeriodValidator.ValidateFiscalPeriod(fiscalPeriodDto))
                    {
                        throw new InvalidOperationException("Fiscal period validation failed");
                    }

                    // Check for duplicate fiscal periods by Code in the database
                    var existingFiscalPeriod = _objectSpace.FindObject<FiscalPeriod>(CriteriaOperator.Parse("Code = ?", fiscalPeriod.Code));
                    if (existingFiscalPeriod != null)
                    {
                        throw new InvalidOperationException($"Fiscal period with code '{fiscalPeriod.Code}' already exists");
                    }

                    // Check for overlapping periods
                    var allFiscalPeriods = _objectSpace.GetObjects<FiscalPeriod>().Cast<IFiscalPeriod>();
                    if (!_fiscalPeriodValidator.ValidateNoOverlap(fiscalPeriod.StartDate, fiscalPeriod.EndDate, allFiscalPeriods))
                    {
                        throw new InvalidOperationException("Fiscal period overlaps with existing period");
                    }

                    // Create the XAF entity from the validated data
                    var xafFiscalPeriod = CreateXafFiscalPeriodFromInterface(fiscalPeriod, userId);

                    // Commit the changes
                    _objectSpace.CommitChanges();
                    
                    _logger?.LogInformation("Successfully created fiscal period with code: {Code}", fiscalPeriod.Code);

                    return (IFiscalPeriod)xafFiscalPeriod;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error creating fiscal period with code: {Code}", fiscalPeriod?.Code);
                    throw;
                }
            });
        }

        /// <summary>
        /// Gets fiscal periods by status
        /// </summary>
        /// <param name="status">Status to filter by</param>
        /// <returns>Collection of fiscal periods with the specified status</returns>
        public Task<IEnumerable<IFiscalPeriod>> GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus status)
        {
            return Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Getting fiscal periods by status: {Status}", status);

                    var fiscalPeriods = _objectSpace.GetObjects<FiscalPeriod>(CriteriaOperator.Parse("Status = ?", status))
                        .Cast<IFiscalPeriod>()
                        .ToList();

                    _logger?.LogInformation("Found {Count} fiscal periods with status: {Status}", fiscalPeriods.Count, status);

                    return (IEnumerable<IFiscalPeriod>)fiscalPeriods;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error getting fiscal periods by status: {Status}", status);
                    throw;
                }
            });
        }

        /// <summary>
        /// Gets the fiscal period that contains a specific date
        /// </summary>
        /// <param name="date">Date to find fiscal period for</param>
        /// <returns>Fiscal period containing the date, null if none found</returns>
        public Task<IFiscalPeriod?> GetFiscalPeriodForDateAsync(DateOnly date)
        {
            return Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Getting fiscal period for date: {Date}", date);

                    var fiscalPeriod = _objectSpace.FindObject<FiscalPeriod>(
                        CriteriaOperator.Parse("StartDate <= ? AND EndDate >= ?", date, date));

                    _logger?.LogInformation("Found fiscal period: {Code} for date: {Date}", 
                        fiscalPeriod?.Code ?? "None", date);

                    return (IFiscalPeriod?)fiscalPeriod;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error getting fiscal period for date: {Date}", date);
                    throw;
                }
            });
        }

        /// <summary>
        /// Gets a fiscal period by ID
        /// </summary>
        /// <param name="code">Fiscal period code</param>
        /// <returns>Fiscal period if found, null otherwise</returns>
        public Task<IFiscalPeriod?> GetFiscalPeriodByIdAsync(string code)
        {
            return Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Getting fiscal period by code: {Code}", code);

                    var fiscalPeriod = _objectSpace.FindObject<FiscalPeriod>(CriteriaOperator.Parse("Code = ?", code));

                    _logger?.LogInformation("Found fiscal period: {Found} for code: {Code}", 
                        fiscalPeriod != null, code);

                    return (IFiscalPeriod?)fiscalPeriod;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error getting fiscal period by code: {Code}", code);
                    throw;
                }
            });
        }

        /// <summary>
        /// Validates a fiscal period for creation or update
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public Task<bool> ValidateFiscalPeriodAsync(IFiscalPeriod fiscalPeriod)
        {
            return Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Validating fiscal period with code: {Code}", fiscalPeriod?.Code);

                    if (fiscalPeriod == null)
                        return false;

                    // Convert interface to DTO for validation compatibility
                    var fiscalPeriodDto = CreateFiscalPeriodDtoFromInterface(fiscalPeriod);

                    var isValid = _fiscalPeriodValidator.ValidateFiscalPeriod(fiscalPeriodDto);

                    _logger?.LogInformation("Fiscal period validation result: {IsValid} for code: {Code}", 
                        isValid, fiscalPeriod.Code);

                    return isValid;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error validating fiscal period with code: {Code}", fiscalPeriod?.Code);
                    return false;
                }
            });
        }

        /// <summary>
        /// Validates a fiscal period for creation or update including overlap check
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to validate</param>
        /// <param name="excludeId">Optional ID to exclude from overlap check (for updates)</param>
        /// <returns>True if valid, false otherwise</returns>
        public Task<bool> ValidateFiscalPeriodWithOverlapAsync(IFiscalPeriod fiscalPeriod, string? excludeId = null)
        {
            return Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Validating fiscal period with overlap check. Code: {Code}, ExcludeId: {ExcludeId}", 
                        fiscalPeriod?.Code, excludeId);

                    if (fiscalPeriod == null)
                        return false;

                    // Convert interface to DTO for validation compatibility
                    var fiscalPeriodDto = CreateFiscalPeriodDtoFromInterface(fiscalPeriod);

                    // Get all existing fiscal periods for overlap check
                    var allFiscalPeriods = _objectSpace.GetObjects<FiscalPeriod>().Cast<IFiscalPeriod>();

                    var isValid = _fiscalPeriodValidator.ValidateFiscalPeriodWithOverlapCheck(
                        fiscalPeriodDto, allFiscalPeriods, excludeId);

                    _logger?.LogInformation("Fiscal period validation with overlap result: {IsValid} for code: {Code}", 
                        isValid, fiscalPeriod.Code);

                    return isValid;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error validating fiscal period with overlap check. Code: {Code}", fiscalPeriod?.Code);
                    return false;
                }
            });
        }

        /// <summary>
        /// Checks if fiscal periods overlap
        /// </summary>
        /// <param name="startDate">Start date of period to check</param>
        /// <param name="endDate">End date of period to check</param>
        /// <param name="excludeId">Optional ID to exclude from overlap check (for updates)</param>
        /// <returns>True if there is an overlap, false otherwise</returns>
        public Task<bool> HasOverlappingPeriodsAsync(DateOnly startDate, DateOnly endDate, string? excludeId = null)
        {
            return Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Checking for overlapping periods. StartDate: {StartDate}, EndDate: {EndDate}, ExcludeId: {ExcludeId}", 
                        startDate, endDate, excludeId);

                    // Get all existing fiscal periods
                    var allFiscalPeriods = _objectSpace.GetObjects<FiscalPeriod>().Cast<IFiscalPeriod>();

                    var hasOverlap = !_fiscalPeriodValidator.ValidateNoOverlap(startDate, endDate, allFiscalPeriods, excludeId);

                    _logger?.LogInformation("Overlap check result: {HasOverlap} for period {StartDate} to {EndDate}", 
                        hasOverlap, startDate, endDate);

                    return hasOverlap;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error checking for overlapping periods. StartDate: {StartDate}, EndDate: {EndDate}", 
                        startDate, endDate);
                    throw;
                }
            });
        }

        /// <summary>
        /// Clears all fiscal periods (for testing purposes)
        /// </summary>
        public void ClearAllFiscalPeriods()
        {
            try
            {
                _logger?.LogInformation("Clearing all fiscal periods");

                var allFiscalPeriods = _objectSpace.GetObjects<FiscalPeriod>();
                foreach (var fiscalPeriod in allFiscalPeriods.ToList())
                {
                    _objectSpace.Delete(fiscalPeriod);
                }

                _objectSpace.CommitChanges();

                _logger?.LogInformation("Successfully cleared all fiscal periods");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error clearing all fiscal periods");
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Creates a FiscalPeriodDto from an IFiscalPeriod interface for validation compatibility
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period interface</param>
        /// <returns>New FiscalPeriodDto with populated properties</returns>
        private FiscalPeriodDto CreateFiscalPeriodDtoFromInterface(IFiscalPeriod fiscalPeriod)
        {
            return new FiscalPeriodDto
            {
                Code = fiscalPeriod.Code,
                Name = fiscalPeriod.Name,
                Description = fiscalPeriod.Description,
                StartDate = fiscalPeriod.StartDate,
                EndDate = fiscalPeriod.EndDate,
                Status = fiscalPeriod.Status,
                InsertedAt = fiscalPeriod.InsertedAt,
                InsertedBy = fiscalPeriod.InsertedBy,
                UpdatedAt = fiscalPeriod.UpdatedAt,
                UpdatedBy = fiscalPeriod.UpdatedBy
            };
        }

        /// <summary>
        /// Creates a XAF FiscalPeriod entity from an IFiscalPeriod interface
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period interface</param>
        /// <param name="userId">User performing the operation</param>
        /// <returns>New XAF FiscalPeriod with populated properties</returns>
        private FiscalPeriod CreateXafFiscalPeriodFromInterface(IFiscalPeriod fiscalPeriod, string userId)
        {
            var xafFiscalPeriod = _objectSpace.CreateObject<FiscalPeriod>();
            var currentTime = DateTime.UtcNow;

            // Copy data from interface to XAF entity
            xafFiscalPeriod.Code = fiscalPeriod.Code;
            xafFiscalPeriod.Name = fiscalPeriod.Name;
            xafFiscalPeriod.Description = fiscalPeriod.Description;
            xafFiscalPeriod.StartDate = fiscalPeriod.StartDate;
            xafFiscalPeriod.EndDate = fiscalPeriod.EndDate;
            xafFiscalPeriod.Status = fiscalPeriod.Status;

            // Set audit fields
            xafFiscalPeriod.InsertedBy = userId;
            xafFiscalPeriod.InsertedAt = currentTime;
            xafFiscalPeriod.UpdatedBy = userId;
            xafFiscalPeriod.UpdatedAt = currentTime;

            // Set XAF-specific fields based on status
            xafFiscalPeriod.IsOpen = fiscalPeriod.Status == FiscalPeriodStatus.Open;

            return xafFiscalPeriod;
        }

        #endregion
    }
}
