using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Accounting.FiscalPeriods
{
    /// <summary>
    /// Implementation of fiscal period service
    /// </summary>
    public class FiscalPeriodService : IFiscalPeriodService
    {
  
        private readonly FiscalPeriodValidator _validator;
        private static readonly List<IFiscalPeriod> _fiscalPeriods = new List<IFiscalPeriod>();

        public FiscalPeriodService()
        {
           
            _validator = new FiscalPeriodValidator();
        }

        /// <summary>
        /// Creates a new fiscal period
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to create</param>
        /// <param name="userId">User creating the fiscal period</param>
        /// <returns>Created fiscal period with ID</returns>
        public async Task<IFiscalPeriod> CreateFiscalPeriodAsync(IFiscalPeriod fiscalPeriod, string userId)
        {
            if (fiscalPeriod == null)
                throw new ArgumentNullException(nameof(fiscalPeriod));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            // Validate the fiscal period including overlap check
            var isValid = await ValidateFiscalPeriodWithOverlapAsync(fiscalPeriod);
            if (!isValid)
                throw new InvalidOperationException("Invalid fiscal period or fiscal period overlaps with existing period");

            // Generate ID if not set
            if (fiscalPeriod.Id == Guid.Empty)
                fiscalPeriod.Id = Guid.NewGuid();

          

            // Store the fiscal period (in a real implementation, this would be saved to database)
            _fiscalPeriods.Add(fiscalPeriod);

            return fiscalPeriod;
        }

        /// <summary>
        /// Updates an existing fiscal period
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to update</param>
        /// <param name="userId">User updating the fiscal period</param>
        /// <returns>Updated fiscal period</returns>
        public async Task<IFiscalPeriod> UpdateFiscalPeriodAsync(IFiscalPeriod fiscalPeriod, string userId)
        {
            if (fiscalPeriod == null)
                throw new ArgumentNullException(nameof(fiscalPeriod));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            // Find existing fiscal period
            var existingPeriod = _fiscalPeriods.FirstOrDefault(fp => fp.Id == fiscalPeriod.Id);
            if (existingPeriod == null)
                throw new InvalidOperationException("Fiscal period not found");

            // Validate the fiscal period including overlap check (excluding current period)
            var isValid = await ValidateFiscalPeriodWithOverlapAsync(fiscalPeriod, fiscalPeriod.Id);
            if (!isValid)
                throw new InvalidOperationException("Invalid fiscal period or fiscal period overlaps with existing period");

            // Update properties
            existingPeriod.StartDate = fiscalPeriod.StartDate;
            existingPeriod.EndDate = fiscalPeriod.EndDate;
            existingPeriod.Status = fiscalPeriod.Status;
            existingPeriod.Name = fiscalPeriod.Name;
            existingPeriod.Description = fiscalPeriod.Description;

           

            return existingPeriod;
        }

        /// <summary>
        /// Gets a fiscal period by ID
        /// </summary>
        /// <param name="id">Fiscal period ID</param>
        /// <returns>Fiscal period if found, null otherwise</returns>
        public Task<IFiscalPeriod?> GetFiscalPeriodByIdAsync(Guid id)
        {
            var fiscalPeriod = _fiscalPeriods.FirstOrDefault(fp => fp.Id == id);
            return Task.FromResult(fiscalPeriod);
        }

        /// <summary>
        /// Gets all fiscal periods
        /// </summary>
        /// <returns>Collection of fiscal periods</returns>
        public Task<IEnumerable<IFiscalPeriod>> GetAllFiscalPeriodsAsync()
        {
            return Task.FromResult(_fiscalPeriods.AsEnumerable());
        }

        /// <summary>
        /// Gets fiscal periods by status
        /// </summary>
        /// <param name="status">Status to filter by</param>
        /// <returns>Collection of fiscal periods with the specified status</returns>
        public Task<IEnumerable<IFiscalPeriod>> GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus status)
        {
            var filteredPeriods = _fiscalPeriods.Where(fp => fp.Status == status);
            return Task.FromResult(filteredPeriods);
        }

        /// <summary>
        /// Gets the fiscal period that contains a specific date
        /// </summary>
        /// <param name="date">Date to find fiscal period for</param>
        /// <returns>Fiscal period containing the date, null if none found</returns>
        public Task<IFiscalPeriod?> GetFiscalPeriodForDateAsync(DateOnly date)
        {
            var fiscalPeriod = _fiscalPeriods.FirstOrDefault(fp => date >= fp.StartDate && date <= fp.EndDate);
            return Task.FromResult(fiscalPeriod);
        }

        /// <summary>
        /// Closes a fiscal period
        /// </summary>
        /// <param name="fiscalPeriodId">ID of the fiscal period to close</param>
        /// <param name="userId">User closing the fiscal period</param>
        /// <returns>Updated fiscal period</returns>
        public async Task<IFiscalPeriod> CloseFiscalPeriodAsync(Guid fiscalPeriodId, string userId)
        {
            var fiscalPeriod = await GetFiscalPeriodByIdAsync(fiscalPeriodId);
            if (fiscalPeriod == null)
                throw new InvalidOperationException("Fiscal period not found");

            fiscalPeriod.Status = FiscalPeriodStatus.Closed;
        
            return fiscalPeriod;
        }

        /// <summary>
        /// Opens a fiscal period
        /// </summary>
        /// <param name="fiscalPeriodId">ID of the fiscal period to open</param>
        /// <param name="userId">User opening the fiscal period</param>
        /// <returns>Updated fiscal period</returns>
        public async Task<IFiscalPeriod> OpenFiscalPeriodAsync(Guid fiscalPeriodId, string userId)
        {
            var fiscalPeriod = await GetFiscalPeriodByIdAsync(fiscalPeriodId);
            if (fiscalPeriod == null)
                throw new InvalidOperationException("Fiscal period not found");

            fiscalPeriod.Status = FiscalPeriodStatus.Open;
          

            return fiscalPeriod;
        }

        /// <summary>
        /// Validates a fiscal period for creation or update
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public Task<bool> ValidateFiscalPeriodAsync(IFiscalPeriod fiscalPeriod)
        {
            if (fiscalPeriod == null)
                return Task.FromResult(false);

            // Use the validator for comprehensive validation
            var isValid = _validator.ValidateFiscalPeriod(fiscalPeriod);
            return Task.FromResult(isValid);
        }

        /// <summary>
        /// Validates a fiscal period for creation or update including overlap check
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to validate</param>
        /// <param name="excludeId">Optional ID to exclude from overlap check (for updates)</param>
        /// <returns>True if valid, false otherwise</returns>
        public Task<bool> ValidateFiscalPeriodWithOverlapAsync(IFiscalPeriod fiscalPeriod, Guid? excludeId = null)
        {
            if (fiscalPeriod == null)
                return Task.FromResult(false);

            // Use the validator for comprehensive validation including overlap check
            var isValid = _validator.ValidateFiscalPeriodWithOverlapCheck(fiscalPeriod, _fiscalPeriods, excludeId);
            return Task.FromResult(isValid);
        }

        /// <summary>
        /// Checks if fiscal periods overlap
        /// </summary>
        /// <param name="startDate">Start date of period to check</param>
        /// <param name="endDate">End date of period to check</param>
        /// <param name="excludeId">Optional ID to exclude from overlap check (for updates)</param>
        /// <returns>True if there is an overlap, false otherwise</returns>
        public Task<bool> HasOverlappingPeriodsAsync(DateOnly startDate, DateOnly endDate, Guid? excludeId = null)
        {
            var periodsToCheck = _fiscalPeriods.Where(fp => excludeId == null || fp.Id != excludeId);

            var hasOverlap = periodsToCheck.Any(fp =>
                startDate >= fp.StartDate && startDate <= fp.EndDate ||
                endDate >= fp.StartDate && endDate <= fp.EndDate ||
                startDate <= fp.StartDate && endDate >= fp.EndDate);

            return Task.FromResult(hasOverlap);
        }

        /// <summary>
        /// Clears all fiscal periods (for testing purposes)
        /// </summary>
        public void ClearAllFiscalPeriods()
        {
            _fiscalPeriods.Clear();
        }
    }
}
