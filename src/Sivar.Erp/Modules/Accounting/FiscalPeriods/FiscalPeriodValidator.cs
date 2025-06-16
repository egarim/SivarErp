using System;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.Accounting.FiscalPeriods
{
    /// <summary>
    /// Validator for fiscal period entities
    /// </summary>
    public class FiscalPeriodValidator
    {
        /// <summary>
        /// Validates a fiscal period
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateFiscalPeriod(IFiscalPeriod fiscalPeriod)
        {
            if (fiscalPeriod == null)
                return false;

            if (string.IsNullOrWhiteSpace(fiscalPeriod.Name))
                return false;

            if (fiscalPeriod.EndDate < fiscalPeriod.StartDate)
                return false;

            return true;
        }

        /// <summary>
        /// Validates fiscal period dates
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>True if dates are valid, false otherwise</returns>
        public bool ValidateDateRange(DateOnly startDate, DateOnly endDate)
        {
            return endDate >= startDate;
        }

        /// <summary>
        /// Validates fiscal period name
        /// </summary>
        /// <param name="name">Name to validate</param>
        /// <returns>True if name is valid, false otherwise</returns>
        public bool ValidateName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length <= 100;
        }

        /// <summary>
        /// Validates fiscal period description
        /// </summary>
        /// <param name="description">Description to validate</param>
        /// <returns>True if description is valid, false otherwise</returns>
        public bool ValidateDescription(string description)
        {
            // Description is optional but if provided, should not exceed reasonable length
            return description == null || description.Length <= 500;
        }

        /// <summary>
        /// Validates that a fiscal period is not too long (e.g., more than 2 years)
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>True if period length is reasonable, false otherwise</returns>
        public bool ValidatePeriodLength(DateOnly startDate, DateOnly endDate)
        {
            var duration = endDate.DayNumber - startDate.DayNumber + 1;
            // Allow up to 2 years (approximately 730 days)
            return duration <= 730;
        }        /// <summary>
                 /// Validates that a fiscal period is not too short (e.g., less than 1 day)
                 /// </summary>
                 /// <param name="startDate">Start date</param>
                 /// <param name="endDate">End date</param>
                 /// <returns>True if period length is reasonable, false otherwise</returns>
        public bool ValidateMinimumPeriodLength(DateOnly startDate, DateOnly endDate)
        {
            return endDate >= startDate; // At least one day
        }

        /// <summary>
        /// Validates that a fiscal period does not overlap with existing periods
        /// </summary>
        /// <param name="startDate">Start date of the period to validate</param>
        /// <param name="endDate">End date of the period to validate</param>
        /// <param name="existingPeriods">Collection of existing fiscal periods</param>
        /// <param name="excludeId">Optional ID to exclude from overlap check (for updates)</param>
        /// <returns>True if no overlap, false if overlaps with existing periods</returns>
        public bool ValidateNoOverlap(DateOnly startDate, DateOnly endDate, IEnumerable<IFiscalPeriod> existingPeriods, Guid? excludeId = null)
        {
            if (existingPeriods == null)
                return true;

            var periodsToCheck = existingPeriods.Where(fp => excludeId == null || fp.Oid != excludeId);

            var hasOverlap = periodsToCheck.Any(fp =>
                startDate >= fp.StartDate && startDate <= fp.EndDate ||
                endDate >= fp.StartDate && endDate <= fp.EndDate ||
                startDate <= fp.StartDate && endDate >= fp.EndDate);

            return !hasOverlap;
        }

        /// <summary>
        /// Comprehensive validation of a fiscal period including overlap check
        /// </summary>
        /// <param name="fiscalPeriod">Fiscal period to validate</param>
        /// <param name="existingPeriods">Collection of existing fiscal periods for overlap check</param>
        /// <param name="excludeId">Optional ID to exclude from overlap check (for updates)</param>
        /// <returns>True if all validations pass, false otherwise</returns>
        public bool ValidateFiscalPeriodWithOverlapCheck(IFiscalPeriod fiscalPeriod, IEnumerable<IFiscalPeriod> existingPeriods, Guid? excludeId = null)
        {
            if (fiscalPeriod == null)
                return false;

            // Basic validation
            if (!ValidateFiscalPeriod(fiscalPeriod))
                return false;

            // Overlap validation
            if (!ValidateNoOverlap(fiscalPeriod.StartDate, fiscalPeriod.EndDate, existingPeriods, excludeId))
                return false;

            return true;
        }
    }
}
