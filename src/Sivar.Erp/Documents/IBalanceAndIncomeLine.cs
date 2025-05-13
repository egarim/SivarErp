using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.FinancialStatements.BalanceAndIncome
{
    #region Interfaces

    /// <summary>
    /// Interface for balance sheet and income statement lines
    /// </summary>
    public interface IBalanceAndIncomeLine : IEntity, IAuditable
    {
        /// <summary>
        /// Type of line (header or data line, balance or income)
        /// </summary>
        BalanceIncomeLineType LineType { get; set; }

        /// <summary>
        /// Index for controlling display order
        /// </summary>
        int VisibleIndex { get; set; }

        /// <summary>
        /// Number to print in the report (e.g., "I", "II", "A", "1.")
        /// </summary>
        string PrintedNo { get; set; }

        /// <summary>
        /// Text to display for this line
        /// </summary>
        string LineText { get; set; }

        /// <summary>
        /// Whether this line represents a debit or credit balance
        /// </summary>
        FinacialStatementValueType ValueType { get; set; }

        /// <summary>
        /// Left index for nested set model
        /// </summary>
        int LeftIndex { get; set; }

        /// <summary>
        /// Right index for nested set model
        /// </summary>
        int RightIndex { get; set; }
    }
    #endregion

    #region DTOs

    /// <summary>
    /// Implementation of balance sheet and income statement line
    /// </summary>
        #endregion

    #region Service Implementation (Abstract Base)

    /// <summary>
    /// Abstract base implementation of balance and income line service
    /// </summary>
        #endregion
}