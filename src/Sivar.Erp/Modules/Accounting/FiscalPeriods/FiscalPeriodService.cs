using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.Services;

namespace Sivar.Erp.Services.Accounting.FiscalPeriods
{
    /// <summary>
    /// Implementation of fiscal period service
    /// </summary>
    public class FiscalPeriodService : IFiscalPeriodService
    {
        private readonly PerformanceLogger<FiscalPeriodService> _performanceLogger;
        private readonly FiscalPeriodValidator _validator;
        private readonly IObjectDb _objectDb; public FiscalPeriodService(ILogger<FiscalPeriodService> logger, IObjectDb objectDb, IPerformanceContextProvider? contextProvider = null)
        {
            _objectDb = objectDb;
            _validator = new FiscalPeriodValidator();
            _performanceLogger = new PerformanceLogger<FiscalPeriodService>(logger, PerformanceLogMode.All, 50, 5_000_000, objectDb, contextProvider);
        }

        /// <summary>
        /// Creates a new fiscal period
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to create</param>
        /// <param name="userId">User creating the fiscal period</param>
        /// <returns>Created fiscal period with ID</returns>
        public async Task<IFiscalPeriod> CreateFiscalPeriodAsync(IFiscalPeriod fiscalPeriod, string userId)
        {
            return await _performanceLogger.Track(nameof(CreateFiscalPeriodAsync), async () =>
            {
                if (fiscalPeriod == null)
                    throw new ArgumentNullException(nameof(fiscalPeriod));

                if (string.IsNullOrWhiteSpace(userId))
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

                // Validate the fiscal period including overlap check
                var isValid = await ValidateFiscalPeriodWithOverlapAsync(fiscalPeriod);
                if (!isValid)
                    throw new InvalidOperationException("Invalid fiscal period or fiscal period overlaps with existing period");

                // Store the fiscal period (in a real implementation, this would be saved to database)
                _objectDb.fiscalPeriods.Add(fiscalPeriod);

                return fiscalPeriod;
            });
        }

        /// <summary>
        /// Gets a fiscal period by ID
        /// </summary>
        /// <param name="Code">Fiscal period ID</param>
        /// <returns>Fiscal period if found, null otherwise</returns>
        public Task<IFiscalPeriod?> GetFiscalPeriodByIdAsync(string Code)
        {
            return _performanceLogger.Track(nameof(GetFiscalPeriodByIdAsync), () =>
            {
                var fiscalPeriod = _objectDb.fiscalPeriods.FirstOrDefault(fp => fp.Code == Code);
                return Task.FromResult(fiscalPeriod);
            });
        }

        /// <summary>
        /// Gets fiscal periods by status
        /// </summary>
        /// <param name="status">Status to filter by</param>
        /// <returns>Collection of fiscal periods with the specified status</returns>
        public Task<IEnumerable<IFiscalPeriod>> GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus status)
        {
            return _performanceLogger.Track(nameof(GetFiscalPeriodsByStatusAsync), () =>
            {
                var filteredPeriods = _objectDb.fiscalPeriods.Where(fp => fp.Status == status);
                return Task.FromResult(filteredPeriods);
            });
        }

        /// <summary>
        /// Gets the fiscal period that contains a specific date
        /// </summary>
        /// <param name="date">Date to find fiscal period for</param>
        /// <returns>Fiscal period containing the date, null if none found</returns>
        public Task<IFiscalPeriod?> GetFiscalPeriodForDateAsync(DateOnly date)
        {
            return _performanceLogger.Track(nameof(GetFiscalPeriodForDateAsync), () =>
            {
                var fiscalPeriod = _objectDb.fiscalPeriods.FirstOrDefault(fp => date >= fp.StartDate && date <= fp.EndDate);
                return Task.FromResult(fiscalPeriod);
            });
        }

        /// <summary>
        /// Validates a fiscal period for creation or update
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public Task<bool> ValidateFiscalPeriodAsync(IFiscalPeriod fiscalPeriod)
        {
            return _performanceLogger.Track(nameof(ValidateFiscalPeriodAsync), () =>
            {
                if (fiscalPeriod == null)
                    return Task.FromResult(false);

                // Use the validator for comprehensive validation
                var isValid = _validator.ValidateFiscalPeriod(fiscalPeriod);
                return Task.FromResult(isValid);
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
            return _performanceLogger.Track(nameof(ValidateFiscalPeriodWithOverlapAsync), () =>
            {
                if (fiscalPeriod == null)
                    return Task.FromResult(false);

                // Use the validator for comprehensive validation including overlap check
                var isValid = _validator.ValidateFiscalPeriodWithOverlapCheck(fiscalPeriod, _objectDb.fiscalPeriods, excludeId);
                return Task.FromResult(isValid);
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
            return _performanceLogger.Track(nameof(HasOverlappingPeriodsAsync), () =>
            {
                var periodsToCheck = _objectDb.fiscalPeriods.Where(fp => excludeId == null || fp.Code != excludeId);

                var hasOverlap = periodsToCheck.Any(fp =>
                    startDate >= fp.StartDate && startDate <= fp.EndDate ||
                    endDate >= fp.StartDate && endDate <= fp.EndDate ||
                    startDate <= fp.StartDate && endDate >= fp.EndDate);

                return Task.FromResult(hasOverlap);
            });
        }

        /// <summary>
        /// Clears all fiscal periods (for testing purposes)
        /// </summary>
        public void ClearAllFiscalPeriods()
        {
            _performanceLogger.Track(nameof(ClearAllFiscalPeriods), () =>
            {
                _objectDb.fiscalPeriods.Clear();
            });
        }
    }
}
