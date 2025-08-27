using System;
using Sivar.Erp.Core.Contracts;
using Sivar.Erp.Core.Enums;

namespace Sivar.Erp.Modules.Accounting.Domain.FiscalPeriods
{
    /// <summary>
    /// Interface for fiscal period entities
    /// </summary>
    public interface IFiscalPeriod : IEntity
    {
        [BusinessKey]
        string Code { get; set; }
        /// <summary>
        /// UTC timestamp when the entity was created
        /// </summary>
        DateTime InsertedAt { get; set; }

        /// <summary>
        /// User who created the entity
        /// </summary>
        string InsertedBy { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was last updated
        /// </summary>
        DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who last updated the entity
        /// </summary>
        string UpdatedBy { get; set; }
        
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