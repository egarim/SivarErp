using System;
using System.Collections.Generic;
using Sivar.Erp.Documents;

namespace Sivar.Erp.FinancialStatements.Equity
{
    #region Interfaces

    /// <summary>
    /// Interface for equity statement line entities
    /// </summary>
    public interface IEquityLine : IEntity, IAuditable
    {
        /// <summary>
        /// Index for controlling display order
        /// </summary>
        int VisibleIndex { get; set; }

        /// <summary>
        /// Number to print in the report (e.g., "1", "2", "A", "B")
        /// </summary>
        string PrintedNo { get; set; }

        /// <summary>
        /// Text to display for this line
        /// </summary>
        string LineText { get; set; }

        /// <summary>
        /// Type of the equity line that defines overall structure
        /// </summary>
        EquityLineType LineType { get; set; }
    }
    #endregion

    #region DTOs

    /// <summary>
    /// Implementation of equity line entity
    /// </summary>
        #endregion

    #region Service Interfaces

    /// <summary>
    /// Interface for equity line service operations
    /// </summary>
        #endregion

    #region Validation Results

    /// <summary>
    /// Validation result for equity lines
    /// </summary>
        #endregion

    #region Helper Classes

    /// <summary>
    /// Builder class for creating equity statement structure
    /// </summary>
        #endregion
}