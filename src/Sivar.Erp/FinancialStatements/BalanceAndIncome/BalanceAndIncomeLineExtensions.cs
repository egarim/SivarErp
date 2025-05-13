using System;
using Sivar.Erp.FinancialStatements.BalanceAndIncome;
using Sivar.Erp.FinancialStatements;

namespace Sivar.Erp.FinancialStatements.BalanceAndIncome
{
    /// <summary>
    /// Extension methods for IBalanceAndIncomeLine to provide tree operations
    /// </summary>
    public static class BalanceAndIncomeLineExtensions
    {
        /// <summary>
        /// Determines if this line is a parent of another line
        /// </summary>
        /// <param name="parentLine">Potential parent line</param>
        /// <param name="childLine">Potential child line</param>
        /// <returns>True if parentLine is parent of childLine</returns>
        public static bool IsParentOf(this IBalanceAndIncomeLine parentLine, IBalanceAndIncomeLine childLine)
        {
            return parentLine.LeftIndex < childLine.LeftIndex && parentLine.RightIndex > childLine.RightIndex;
        }

        /// <summary>
        /// Determines if this line is a child of another line
        /// </summary>
        /// <param name="childLine">Potential child line</param>
        /// <param name="parentLine">Potential parent line</param>
        /// <returns>True if childLine is child of parentLine</returns>
        public static bool IsChildOf(this IBalanceAndIncomeLine childLine, IBalanceAndIncomeLine parentLine)
        {
            return parentLine.LeftIndex < childLine.LeftIndex && parentLine.RightIndex > childLine.RightIndex;
        }

        /// <summary>
        /// Calculates the depth of this line in the tree
        /// </summary>
        /// <param name="line">Line to calculate depth for</param>
        /// <param name="allLines">All lines in the tree</param>
        /// <returns>Depth level (0 = root)</returns>
        public static int CalculateDepth(this IBalanceAndIncomeLine line, IEnumerable<IBalanceAndIncomeLine> allLines)
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
        /// Determines if this line can have children (not a data line)
        /// </summary>
        /// <param name="line">Line to check</param>
        /// <returns>True if can have children</returns>
        public static bool CanHaveChildren(this IBalanceAndIncomeLine line)
        {
            // Only headers can have children, not data lines
            return line.LineType == BalanceIncomeLineType.BaseHeader ||
                   line.LineType == BalanceIncomeLineType.BalanceHeader ||
                   line.LineType == BalanceIncomeLineType.IncomeHeader ||
                   // Allow intermediate grouping lines
                   line.LineType == BalanceIncomeLineType.BalanceLine && line.RightIndex - line.LeftIndex > 1 ||
                   line.LineType == BalanceIncomeLineType.IncomeLine && line.RightIndex - line.LeftIndex > 1;
        }
    }
    /// <summary>
    /// Implementation of balance sheet and income statement line
    /// </summary>


    /// <summary>
    /// Abstract base implementation of balance and income line service
    /// </summary>
}