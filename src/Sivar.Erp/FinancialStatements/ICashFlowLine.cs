using Sivar.Erp.Documents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.FinancialStatements
{
    #region Core Interfaces

    /// <summary>
    /// Interface for cash flow statement lines
    /// </summary>
    public interface ICashFlowLine : IEntity, IAuditable
    {
        /// <summary>
        /// Type of line (header or data line)
        /// </summary>
        CashFlowLineType LineType { get; set; }

        /// <summary>
        /// Indicates if this line represents net income (only one line can have this flag)
        /// </summary>
        bool IsNetIncome { get; set; }

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
        /// Whether this line represents a debit or credit value
        /// </summary>
        FinacialStatementValueType ValueType { get; set; }

        /// <summary>
        /// How the balance is calculated for this line
        /// </summary>
        BalanceType BalanceType { get; set; }

        /// <summary>
        /// Left index for nested set model
        /// </summary>
        int LeftIndex { get; set; }

        /// <summary>
        /// Right index for nested set model
        /// </summary>
        int RightIndex { get; set; }
    }

    /// <summary>
    /// Interface for cash flow line assignment to accounts
    /// </summary>
    public interface ICashFlowLineAssignment : IEntity
    {
        /// <summary>
        /// Reference to the account
        /// </summary>
        Guid AccountId { get; set; }

        /// <summary>
        /// Reference to the cash flow line
        /// </summary>
        Guid CashFlowLineId { get; set; }
    }

    /// <summary>
    /// Interface for cash flow adjustments
    /// </summary>
    public interface ICashFlowAdjustment : IEntity
    {
        /// <summary>
        /// Reference to the transaction being adjusted
        /// </summary>
        Guid TransactionId { get; set; }

        /// <summary>
        /// Reference to the account being adjusted
        /// </summary>
        Guid AccountId { get; set; }

        /// <summary>
        /// Type of adjustment (debit or credit)
        /// </summary>
        EntryType EntryType { get; set; }

        /// <summary>
        /// Amount of the adjustment
        /// </summary>
        decimal Amount { get; set; }
    }

    #endregion

    #region Extension Methods

    /// <summary>
    /// Extension methods for cash flow lines
    /// </summary>
    public static class CashFlowLineExtensions
    {
        /// <summary>
        /// Determines if this line is a parent of another line
        /// </summary>
        /// <param name="parentLine">Potential parent line</param>
        /// <param name="childLine">Potential child line</param>
        /// <returns>True if parentLine is parent of childLine</returns>
        public static bool IsParentOf(this ICashFlowLine parentLine, ICashFlowLine childLine)
        {
            return parentLine.LeftIndex < childLine.LeftIndex && parentLine.RightIndex > childLine.RightIndex;
        }

        /// <summary>
        /// Determines if this line is a child of another line
        /// </summary>
        /// <param name="childLine">Potential child line</param>
        /// <param name="parentLine">Potential parent line</param>
        /// <returns>True if childLine is child of parentLine</returns>
        public static bool IsChildOf(this ICashFlowLine childLine, ICashFlowLine parentLine)
        {
            return parentLine.LeftIndex < childLine.LeftIndex && parentLine.RightIndex > childLine.RightIndex;
        }

        /// <summary>
        /// Calculates the depth of this line in the tree
        /// </summary>
        /// <param name="line">Line to calculate depth for</param>
        /// <param name="allLines">All lines in the tree</param>
        /// <returns>Depth level (0 = root)</returns>
        public static int CalculateDepth(this ICashFlowLine line, IEnumerable<ICashFlowLine> allLines)
        {
            int depth = 0;
            foreach (var otherLine in allLines)
            {
                if (otherLine.IsParentOf(line))
                {
                    depth++;
                }
            }
            return depth;
        }

        /// <summary>
        /// Determines if this line can have children
        /// </summary>
        /// <param name="line">Line to check</param>
        /// <returns>True if can have children</returns>
        public static bool CanHaveChildren(this ICashFlowLine line)
        {
            // Only headers can have children
            return line.LineType == CashFlowLineType.Header;
        }
    }

    #endregion
}