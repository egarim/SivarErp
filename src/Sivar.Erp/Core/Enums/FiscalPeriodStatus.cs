using System;

namespace Sivar.Erp.Core.Enums
{
    /// <summary>
    /// Enumeration of fiscal period statuses (.NET 9)
    /// </summary>
    public enum FiscalPeriodStatus
    {
        /// <summary>
        /// Fiscal period is in draft state
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Fiscal period is open for transaction posting
        /// </summary>
        Open = 1,

        /// <summary>
        /// Fiscal period is closed, no more transactions allowed
        /// </summary>
        Closed = 2,

        /// <summary>
        /// Fiscal period is locked, typically for audit purposes
        /// </summary>
        Locked = 3
    }
}