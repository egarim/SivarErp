using System;
using System.Collections.Generic;

namespace Sivar.Erp.FinancialStatements
{
    #region DTOs

    /// <summary>
    /// Implementation of cash flow statement line
    /// </summary>
    public class CashFlowLineDto : ICashFlowLine
    {
        /// <summary>
        /// Unique identifier for the line
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Type of line (header or data line)
        /// </summary>
        public CashFlowLineType LineType { get; set; }

        /// <summary>
        /// Indicates if this line represents net income (only one line can have this flag)
        /// </summary>
        public bool IsNetIncome { get; set; }

        /// <summary>
        /// Index for controlling display order
        /// </summary>
        public int VisibleIndex { get; set; }

        /// <summary>
        /// Number to print in the report (e.g., "I", "II", "A", "1.")
        /// </summary>
        public string PrintedNo { get; set; } = string.Empty;

        /// <summary>
        /// Text to display for this line
        /// </summary>
        public string LineText { get; set; } = string.Empty;

        /// <summary>
        /// Whether this line represents a debit or credit value
        /// </summary>
        public FinacialStatementValueType ValueType { get; set; }

        /// <summary>
        /// How the balance is calculated for this line
        /// </summary>
        public BalanceType BalanceType { get; set; }

        /// <summary>
        /// Left index for nested set model
        /// </summary>
        public int LeftIndex { get; set; }

        /// <summary>
        /// Right index for nested set model
        /// </summary>
        public int RightIndex { get; set; }

        /// <summary>
        /// UTC timestamp when the line was created
        /// </summary>
        public DateTime InsertedAt { get; set; }

        /// <summary>
        /// User who created the line
        /// </summary>
        public string InsertedBy { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the line was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who last updated the line
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Validates the line according to business rules
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // Line text is required
            if (string.IsNullOrWhiteSpace(LineText))
            {
                return false;
            }

            // Left index must be less than right index
            if (LeftIndex >= RightIndex)
            {
                return false;
            }

            // Visible index cannot be negative
            if (VisibleIndex < 0)
            {
                return false;
            }

            // Net income lines must be of type Line, not Header
            if (IsNetIncome && LineType != CashFlowLineType.Line)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if this line can be moved to a new parent
        /// </summary>
        /// <param name="newParent">Potential new parent</param>
        /// <returns>True if move is valid</returns>
        public bool CanMoveTo(ICashFlowLine newParent)
        {
            // Cannot move to self
            if (newParent.Id == this.Id)
            {
                return false;
            }

            // Cannot move to a child of itself
            if (this.IsParentOf(newParent))
            {
                return false;
            }

            // Can only move under headers
            return newParent.LineType == CashFlowLineType.Header;
        }
    }
    #endregion

    #region Validation Results

    /// <summary>
    /// Validation result for cash flow lines
    /// </summary>
        #endregion
}