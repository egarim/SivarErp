using System;

namespace Sivar.Erp.ErpSystem.FiscalPeriods
{
    /// <summary>
    /// Implementation of a fiscal period
    /// </summary>
    public class FiscalPeriodDto : IFiscalPeriod
    {
        /// <summary>
        /// Unique identifier for the fiscal period
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Start date of the fiscal period
        /// </summary>
        public required DateOnly StartDate { get; set; }

        /// <summary>
        /// End date of the fiscal period
        /// </summary>
        public required DateOnly EndDate { get; set; }

        /// <summary>
        /// Status of the fiscal period (Open or Closed)
        /// </summary>
        public FiscalPeriodStatus Status { get; set; } = FiscalPeriodStatus.Open;

        /// <summary>
        /// Name of the fiscal period
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Description of the fiscal period
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the fiscal period was created
        /// </summary>
        public DateTime InsertedAt { get; set; }

        /// <summary>
        /// User who created the fiscal period
        /// </summary>
        public required string InsertedBy { get; set; }

        /// <summary>
        /// UTC timestamp when the fiscal period was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who last updated the fiscal period
        /// </summary>
        public required string UpdatedBy { get; set; }

        /// <summary>
        /// Initializes a new instance of the FiscalPeriodDto class
        /// </summary>
        public FiscalPeriodDto()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Determines if the fiscal period is currently open
        /// </summary>
        /// <returns>True if open, false if closed</returns>
        public bool IsOpen()
        {
            return Status == FiscalPeriodStatus.Open;
        }

        /// <summary>
        /// Determines if the fiscal period is currently closed
        /// </summary>
        /// <returns>True if closed, false if open</returns>
        public bool IsClosed()
        {
            return Status == FiscalPeriodStatus.Closed;
        }

        /// <summary>
        /// Closes the fiscal period
        /// </summary>
        public void Close()
        {
            Status = FiscalPeriodStatus.Closed;
        }

        /// <summary>
        /// Opens the fiscal period
        /// </summary>
        public void Open()
        {
            Status = FiscalPeriodStatus.Open;
        }

        /// <summary>
        /// Determines if a given date falls within this fiscal period
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if the date falls within the period, false otherwise</returns>
        public bool ContainsDate(DateOnly date)
        {
            return date >= StartDate && date <= EndDate;
        }

        /// <summary>
        /// Gets the duration of the fiscal period in days
        /// </summary>
        /// <returns>Number of days in the period</returns>
        public int GetDurationInDays()
        {
            return EndDate.DayNumber - StartDate.DayNumber + 1;
        }

        /// <summary>
        /// Validates the fiscal period
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            if (EndDate < StartDate)
                return false;

            return true;
        }
    }
}
