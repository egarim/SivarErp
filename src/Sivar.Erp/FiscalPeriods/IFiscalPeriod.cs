using System;

namespace Sivar.Erp.FiscalPeriods
{
    /// <summary>
    /// Interface for fiscal period entities
    /// </summary>
    public interface IFiscalPeriod : IEntity, IAuditable
    {
        /// <summary>
        /// Start date of the fiscal period
        /// </summary>
        DateOnly StartDate { get; set; }

        /// <summary>
        /// End date of the fiscal period
        /// </summary>
        DateOnly EndDate { get; set; }

        /// <summary>
        /// Status of the fiscal period (Open or Closed)
        /// </summary>
        FiscalPeriodStatus Status { get; set; }

        /// <summary>
        /// Name of the fiscal period
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Description of the fiscal period
        /// </summary>
        string Description { get; set; }
    }
}
