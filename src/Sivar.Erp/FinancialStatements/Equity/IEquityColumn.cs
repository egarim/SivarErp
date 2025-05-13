using System;

namespace Sivar.Erp.FinancialStatements.Equity
{
    /// <summary>
    /// Interface for equity statement column entities
    /// </summary>
    public interface IEquityColumn : IEntity, IAuditable
    {
        /// <summary>
        /// Text to display in the column header
        /// </summary>
        string ColumnText { get; set; }

        /// <summary>
        /// Index for controlling display order of columns
        /// </summary>
        int VisibleIndex { get; set; }
    }
    /// <summary>
    /// Implementation of equity line entity
    /// </summary>


    /// <summary>
    /// Interface for equity line service operations
    /// </summary>


    /// <summary>
    /// Validation result for equity lines
    /// </summary>


    /// <summary>
    /// Builder class for creating equity statement structure
    /// </summary>
}