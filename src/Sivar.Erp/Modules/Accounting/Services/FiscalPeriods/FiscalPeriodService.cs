using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Contracts;
using Sivar.Erp.Core.Enums; // Add FiscalPeriodStatus enum
using Sivar.Erp.ErpSystem.Diagnostics; // Use legacy PerformanceLogger for now

// Explicit import to resolve ambiguity
using LegacyIPerformanceContextProvider = Sivar.Erp.ErpSystem.Diagnostics.IPerformanceContextProvider;

namespace Sivar.Erp.Modules.Accounting.Services.FiscalPeriods
{
    /// <summary>
    /// Implementation of fiscal period service (.NET 9 modernized)
    /// </summary>
    public class FiscalPeriodService : IFiscalPeriodService
    {
        private readonly PerformanceLogger<FiscalPeriodService> _performanceLogger;
        private readonly IObjectDb _objectDb;

        public FiscalPeriodService(ILogger<FiscalPeriodService> logger, IObjectDb objectDb, LegacyIPerformanceContextProvider? contextProvider = null)
        {
            _objectDb = objectDb ?? throw new ArgumentNullException(nameof(objectDb));
            // Convert Core.Contracts.IObjectDb to legacy IObjectDb for PerformanceLogger
            _performanceLogger = new PerformanceLogger<FiscalPeriodService>(logger, PerformanceLogMode.All, 50, 5_000_000, null, contextProvider);
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

                // Basic validation
                if (fiscalPeriod.StartDate >= fiscalPeriod.EndDate)
                    throw new InvalidOperationException("Start date must be before end date");

                // Check for overlapping periods
                var hasOverlap = await HasOverlappingPeriodsAsync(fiscalPeriod.StartDate, fiscalPeriod.EndDate);
                if (hasOverlap)
                    throw new InvalidOperationException("Fiscal period overlaps with existing period");

                // Set creation info
                fiscalPeriod.CreatedDate = DateTime.UtcNow;
                fiscalPeriod.CreatedBy = userId;

                // Store the fiscal period
                _objectDb.fiscalPeriods.Add(fiscalPeriod);

                return fiscalPeriod;
            });
        }

        /// <summary>
        /// Gets a fiscal period by ID
        /// </summary>
        /// <param name="code">Fiscal period code</param>
        /// <returns>Fiscal period if found, null otherwise</returns>
        public Task<IFiscalPeriod?> GetFiscalPeriodByIdAsync(string code)
        {
            return _performanceLogger.Track(nameof(GetFiscalPeriodByIdAsync), () =>
            {
                var fiscalPeriod = _objectDb.fiscalPeriods.FirstOrDefault(fp => fp.Code == code);
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
        /// Checks if fiscal periods overlap
        /// </summary>
        /// <param name="startDate">Start date of period to check</param>
        /// <param name="endDate">End date of period to check</param>
        /// <param name="excludeCode">Optional code to exclude from overlap check (for updates)</param>
        /// <returns>True if there is an overlap, false otherwise</returns>
        public Task<bool> HasOverlappingPeriodsAsync(DateOnly startDate, DateOnly endDate, string? excludeCode = null)
        {
            return _performanceLogger.Track(nameof(HasOverlappingPeriodsAsync), () =>
            {
                var periodsToCheck = _objectDb.fiscalPeriods.Where(fp => excludeCode == null || fp.Code != excludeCode);

                var hasOverlap = periodsToCheck.Any(fp =>
                    startDate >= fp.StartDate && startDate <= fp.EndDate ||
                    endDate >= fp.StartDate && endDate <= fp.EndDate ||
                    startDate <= fp.StartDate && endDate >= fp.EndDate);

                return Task.FromResult(hasOverlap);
            });
        }
    }
}