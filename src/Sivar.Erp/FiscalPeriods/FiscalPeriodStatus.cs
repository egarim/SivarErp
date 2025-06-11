using System;

namespace Sivar.Erp.FiscalPeriods
{
    /// <summary>
    /// Enumeration for fiscal period status
    /// </summary>
    public enum FiscalPeriodStatus
    {
        /// <summary>
        /// Fiscal period is open and accepts transactions
        /// </summary>
        Open = 1,

        /// <summary>
        /// Fiscal period is closed and no longer accepts transactions
        /// </summary>
        Closed = 2
    }
}
