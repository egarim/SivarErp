using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.ErpSystem.FiscalPeriods
{
    /// <summary>
    /// Interface for fiscal period service operations
    /// </summary>
    public interface IFiscalPeriodService
    {
        /// <summary>
        /// Creates a new fiscal period
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to create</param>
        /// <param name="userId">User creating the fiscal period</param>
        /// <returns>Created fiscal period with ID</returns>
        Task<IFiscalPeriod> CreateFiscalPeriodAsync(IFiscalPeriod fiscalPeriod, string userId);

        /// <summary>
        /// Updates an existing fiscal period
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to update</param>
        /// <param name="userId">User updating the fiscal period</param>
        /// <returns>Updated fiscal period</returns>
        Task<IFiscalPeriod> UpdateFiscalPeriodAsync(IFiscalPeriod fiscalPeriod, string userId);

        /// <summary>
        /// Gets a fiscal period by ID
        /// </summary>
        /// <param name="id">Fiscal period ID</param>
        /// <returns>Fiscal period if found, null otherwise</returns>
        Task<IFiscalPeriod?> GetFiscalPeriodByIdAsync(Guid id);

        /// <summary>
        /// Gets all fiscal periods
        /// </summary>
        /// <returns>Collection of fiscal periods</returns>
        Task<IEnumerable<IFiscalPeriod>> GetAllFiscalPeriodsAsync();

        /// <summary>
        /// Gets fiscal periods by status
        /// </summary>
        /// <param name="status">Status to filter by</param>
        /// <returns>Collection of fiscal periods with the specified status</returns>
        Task<IEnumerable<IFiscalPeriod>> GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus status);

        /// <summary>
        /// Gets the fiscal period that contains a specific date
        /// </summary>
        /// <param name="date">Date to find fiscal period for</param>
        /// <returns>Fiscal period containing the date, null if none found</returns>
        Task<IFiscalPeriod?> GetFiscalPeriodForDateAsync(DateOnly date);

        /// <summary>
        /// Closes a fiscal period
        /// </summary>
        /// <param name="fiscalPeriodId">ID of the fiscal period to close</param>
        /// <param name="userId">User closing the fiscal period</param>
        /// <returns>Updated fiscal period</returns>
        Task<IFiscalPeriod> CloseFiscalPeriodAsync(Guid fiscalPeriodId, string userId);

        /// <summary>
        /// Opens a fiscal period
        /// </summary>
        /// <param name="fiscalPeriodId">ID of the fiscal period to open</param>
        /// <param name="userId">User opening the fiscal period</param>
        /// <returns>Updated fiscal period</returns>
        Task<IFiscalPeriod> OpenFiscalPeriodAsync(Guid fiscalPeriodId, string userId);        /// <summary>
                                                                                              /// Validates a fiscal period for creation or update
                                                                                              /// </summary>
                                                                                              /// <param name="fiscalPeriod">Fiscal period to validate</param>
                                                                                              /// <returns>True if valid, false otherwise</returns>
        Task<bool> ValidateFiscalPeriodAsync(IFiscalPeriod fiscalPeriod);

        /// <summary>
        /// Validates a fiscal period for creation or update including overlap check
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to validate</param>
        /// <param name="excludeId">Optional ID to exclude from overlap check (for updates)</param>
        /// <returns>True if valid, false otherwise</returns>
        Task<bool> ValidateFiscalPeriodWithOverlapAsync(IFiscalPeriod fiscalPeriod, Guid? excludeId = null);

        /// <summary>
        /// Checks if fiscal periods overlap
        /// </summary>
        /// <param name="startDate">Start date of period to check</param>
        /// <param name="endDate">End date of period to check</param>
        /// <param name="excludeId">Optional ID to exclude from overlap check (for updates)</param>
        /// <returns>True if there is an overlap, false otherwise</returns>
        Task<bool> HasOverlappingPeriodsAsync(DateOnly startDate, DateOnly endDate, Guid? excludeId = null);

        /// <summary>
        /// Clears all fiscal periods (for testing purposes)
        /// </summary>
        void ClearAllFiscalPeriods();
    }
}
