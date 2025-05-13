using Sivar.Erp.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sivar.Erp.FinancialStatements
{
    #region Helper Classes

    /// <summary>
    /// Helper class for cash flow calculations
    /// </summary>
    public static class CashFlowCalculationHelper
    {
        /// <summary>
        /// Calculates the cash flow value for a line based on account balances
        /// </summary>
        /// <param name="line">Cash flow line</param>
        /// <param name="accountBalances">Account balances</param>
        /// <param name="adjustments">Cash flow adjustments</param>
        /// <returns>Calculated value</returns>
        public static decimal CalculateLineValue(
            ICashFlowLine line,
            Dictionary<Guid, (decimal DebitTotal, decimal CreditTotal)> accountBalances,
            IEnumerable<ICashFlowAdjustment> adjustments)
        {
            decimal totalValue = 0;

            // Calculate base value from account balances
            foreach (var (accountId, balances) in accountBalances)
            {
                decimal accountValue = CalculateAccountValueForLine(line, balances);
                totalValue += accountValue;
            }

            // Apply adjustments
            foreach (var adjustment in adjustments)
            {
                if (accountBalances.ContainsKey(adjustment.AccountId))
                {
                    decimal adjustmentValue = CalculateAdjustmentValue(line, adjustment);
                    totalValue += adjustmentValue;
                }
            }

            return totalValue;
        }

        /// <summary>
        /// Calculates the value contribution of an account to a cash flow line
        /// </summary>
        /// <param name="line">Cash flow line</param>
        /// <param name="balances">Account balances</param>
        /// <returns>Value contribution</returns>
        public static decimal CalculateAccountValueForLine(
            ICashFlowLine line,
            (decimal DebitTotal, decimal CreditTotal) balances)
        {
            decimal value = 0;

            switch (line.BalanceType)
            {
                case BalanceType.Total:
                    // Use net balance (debit - credit)
                    value = balances.DebitTotal - balances.CreditTotal;
                    break;

                case BalanceType.PerPeriod:
                    // Use net balance for the period
                    value = balances.DebitTotal - balances.CreditTotal;
                    break;

                case BalanceType.Debit:
                    // Use only debit movements
                    value = balances.DebitTotal;
                    break;

                case BalanceType.Credit:
                    // Use only credit movements (make positive)
                    value = -balances.CreditTotal;
                    break;
            }

            // Apply value type adjustment
            if (line.ValueType == FinacialStatementValueType.Credit)
            {
                value = -value;
            }

            return value;
        }

        /// <summary>
        /// Calculates the impact of an adjustment
        /// </summary>
        /// <param name="line">Cash flow line</param>
        /// <param name="adjustment">Cash flow adjustment</param>
        /// <returns>Adjustment impact</returns>
        public static decimal CalculateAdjustmentValue(ICashFlowLine line, ICashFlowAdjustment adjustment)
        {
            decimal value = adjustment.Amount;

            // Apply adjustment entry type
            if (adjustment.EntryType == EntryType.Credit)
            {
                value = -value;
            }

            // Apply line value type
            if (line.ValueType == FinacialStatementValueType.Credit)
            {
                value = -value;
            }

            return value;
        }

        /// <summary>
        /// Validates that cash flow statement is balanced
        /// </summary>
        /// <param name="lines">Cash flow lines with calculated values</param>
        /// <param name="tolerance">Tolerance for rounding differences</param>
        /// <returns>True if balanced within tolerance</returns>
        public static bool ValidateCashFlowBalance(
            Dictionary<ICashFlowLine, decimal> lines,
            decimal tolerance = 0.01M)
        {
            decimal totalOperating = 0;
            decimal totalInvesting = 0;
            decimal totalFinancing = 0;

            // Categorize activities based on line context
            foreach (var (line, value) in lines)
            {
                if (line.LineType == CashFlowLineType.Line)
                {
                    var lineText = line.LineText.ToUpper();

                    if (lineText.Contains("OPERATING"))
                    {
                        totalOperating += value;
                    }
                    else if (lineText.Contains("INVESTING"))
                    {
                        totalInvesting += value;
                    }
                    else if (lineText.Contains("FINANCING"))
                    {
                        totalFinancing += value;
                    }
                }
            }

            // Total cash flow should be the sum of all activities
            decimal totalCashFlow = totalOperating + totalInvesting + totalFinancing;

            // The total should equal the net change in cash (ideally zero for balanced statement)
            return Math.Abs(totalCashFlow) <= tolerance;
        }

        /// <summary>
        /// Calculates net income from account balances for indirect method
        /// </summary>
        /// <param name="revenueAccounts">Revenue account balances</param>
        /// <param name="expenseAccounts">Expense account balances</param>
        /// <returns>Net income</returns>
        public static decimal CalculateNetIncome(
            Dictionary<Guid, decimal> revenueAccounts,
            Dictionary<Guid, decimal> expenseAccounts)
        {
            decimal totalRevenue = revenueAccounts.Values.Sum();
            decimal totalExpenses = expenseAccounts.Values.Sum();

            // Revenues are typically credit balances (negative in our system)
            // Expenses are typically debit balances (positive in our system)
            return -totalRevenue - totalExpenses;
        }
    }
    #endregion
}