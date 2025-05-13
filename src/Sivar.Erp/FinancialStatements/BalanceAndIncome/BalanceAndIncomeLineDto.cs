using System;

namespace Sivar.Erp.FinancialStatements.BalanceAndIncome
{
    /// <summary>
    /// Implementation of balance sheet and income statement line
    /// </summary>
    public class BalanceAndIncomeLineDto : IBalanceAndIncomeLine
    {
        /// <summary>
        /// Unique identifier for the line
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Type of line (header or data line, balance or income)
        /// </summary>
        public BalanceIncomeLineType LineType { get; set; }

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
        /// Whether this line represents a debit or credit balance
        /// </summary>
        public FinacialStatementValueType ValueType { get; set; }

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

            return true;
        }

        /// <summary>
        /// Calculates the depth of this line in the tree
        /// </summary>
        /// <param name="allLines">All lines in the tree</param>
        /// <returns>Depth level (0 = root)</returns>
        public int CalculateDepth(IEnumerable<IBalanceAndIncomeLine> allLines)
        {
            return CalculateDepth(allLines);
        }

        /// <summary>
        /// Determines if this line is a parent of another line
        /// </summary>
        /// <param name="childLine">Potential child line</param>
        /// <returns>True if this line is parent of childLine</returns>
        public bool IsParentOf(IBalanceAndIncomeLine childLine)
        {
            return IsParentOf(childLine);
        }

        /// <summary>
        /// Determines if this line is a child of another line
        /// </summary>
        /// <param name="parentLine">Potential parent line</param>
        /// <returns>True if this line is child of parentLine</returns>
        public bool IsChildOf(IBalanceAndIncomeLine parentLine)
        {
            return IsChildOf(parentLine);
        }

        /// <summary>
        /// Determines if this line can have children (not a data line)
        /// </summary>
        /// <returns>True if can have children</returns>
        public bool CanHaveChildren()
        {
            return CanHaveChildren();
        }
    }
    /// <summary>
    /// Abstract base implementation of balance and income line service
    /// </summary>
}