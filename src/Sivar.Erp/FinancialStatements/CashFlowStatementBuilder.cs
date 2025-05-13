using System;
using System.Linq;

namespace Sivar.Erp.FinancialStatements
{
    /// <summary>
    /// Builder class for creating cash flow statements
    /// </summary>
    public class CashFlowStatementBuilder
    {
        private readonly List<ICashFlowLine> _lines = new List<ICashFlowLine>();
        private readonly List<ICashFlowLineAssignment> _assignments = new List<ICashFlowLineAssignment>();
        private int _currentDepth = 0;

        /// <summary>
        /// Adds a header line to the statement
        /// </summary>
        /// <param name="text">Header text</param>
        /// <param name="printedNo">Printed number</param>
        /// <returns>Builder for fluent interface</returns>
        public CashFlowStatementBuilder AddHeader(string text, string printedNo = "")
        {
            var line = new CashFlowLineDto
            {
                Id = Guid.NewGuid(),
                LineType = CashFlowLineType.Header,
                LineText = text,
                PrintedNo = printedNo,
                VisibleIndex = _lines.Count,
                LeftIndex = GetNextLeftIndex(),
                RightIndex = GetNextLeftIndex() + 1
            };

            _lines.Add(line);
            return this;
        }

        /// <summary>
        /// Adds a data line to the statement
        /// </summary>
        /// <param name="text">Line text</param>
        /// <param name="valueType">Value type</param>
        /// <param name="balanceType">Balance type</param>
        /// <param name="printedNo">Printed number</param>
        /// <param name="isNetIncome">Whether this is the net income line</param>
        /// <returns>Builder for fluent interface</returns>
        public CashFlowStatementBuilder AddLine(
            string text,
            FinacialStatementValueType valueType,
            BalanceType balanceType,
            string printedNo = "",
            bool isNetIncome = false)
        {
            // Validate net income constraint
            if (isNetIncome && _lines.Any(l => l.IsNetIncome))
            {
                throw new InvalidOperationException("Only one line can be marked as net income");
            }

            var line = new CashFlowLineDto
            {
                Id = Guid.NewGuid(),
                LineType = CashFlowLineType.Line,
                LineText = text,
                PrintedNo = printedNo,
                ValueType = valueType,
                BalanceType = balanceType,
                IsNetIncome = isNetIncome,
                VisibleIndex = _lines.Count,
                LeftIndex = GetNextLeftIndex(),
                RightIndex = GetNextLeftIndex() + 1
            };

            _lines.Add(line);
            return this;
        }

        /// <summary>
        /// Assigns an account to the last added line
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Builder for fluent interface</returns>
        public CashFlowStatementBuilder AssignAccount(Guid accountId)
        {
            if (_lines.Count == 0)
            {
                throw new InvalidOperationException("No lines available to assign account to");
            }

            var lastLine = _lines.Last();

            // Cannot assign accounts to headers
            if (lastLine.LineType == CashFlowLineType.Header)
            {
                throw new InvalidOperationException("Cannot assign accounts to header lines");
            }

            var assignment = new CashFlowLineAssignmentDto
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                CashFlowLineId = lastLine.Id
            };

            _assignments.Add(assignment);
            return this;
        }

        /// <summary>
        /// Assigns multiple accounts to the last added line
        /// </summary>
        /// <param name="accountIds">Account IDs</param>
        /// <returns>Builder for fluent interface</returns>
        public CashFlowStatementBuilder AssignAccounts(params Guid[] accountIds)
        {
            foreach (var accountId in accountIds)
            {
                AssignAccount(accountId);
            }
            return this;
        }

        /// <summary>
        /// Builds the cash flow statement
        /// </summary>
        /// <returns>Tuple of lines and assignments</returns>
        public (IEnumerable<ICashFlowLine> Lines, IEnumerable<ICashFlowLineAssignment> Assignments) Build()
        {
            // Validate that we have a net income line
            if (!_lines.Any(l => l.IsNetIncome))
            {
                throw new InvalidOperationException("Cash flow statement must have one net income line");
            }

            // Update nested set indexes
            UpdateNestedSetIndexes();

            return (_lines, _assignments);
        }

        /// <summary>
        /// Gets the next left index for a new line
        /// </summary>
        /// <returns>Next left index</returns>
        private int GetNextLeftIndex()
        {
            if (_lines.Count == 0)
            {
                return 1;
            }

            return _lines.Max(l => l.RightIndex) + 1;
        }

        /// <summary>
        /// Updates nested set indexes for proper tree structure
        /// </summary>
        private void UpdateNestedSetIndexes()
        {
            // Simple implementation - each line is a sibling
            for (int i = 0; i < _lines.Count; i++)
            {
                var line = (CashFlowLineDto)_lines[i];
                line.LeftIndex = i * 2 + 1;
                line.RightIndex = i * 2 + 2;
            }
        }

        /// <summary>
        /// Creates a standard cash flow statement structure using indirect method
        /// </summary>
        /// <returns>Builder with standard structure</returns>
        public static CashFlowStatementBuilder CreateStandardIndirectStructure()
        {
            return new CashFlowStatementBuilder()
                .AddHeader("CASH FLOWS FROM OPERATING ACTIVITIES", "I")
                .AddLine("Net Income", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "1", true)
                .AddLine("Adjustments to reconcile net income to net cash:", FinacialStatementValueType.Debit, BalanceType.PerPeriod)
                .AddLine("Depreciation and Amortization", FinacialStatementValueType.Debit, BalanceType.PerPeriod, "2")
                .AddLine("Changes in Operating Assets and Liabilities:", FinacialStatementValueType.Debit, BalanceType.PerPeriod)
                .AddLine("(Increase) Decrease in Accounts Receivable", FinacialStatementValueType.Debit, BalanceType.PerPeriod, "3")
                .AddLine("(Increase) Decrease in Inventory", FinacialStatementValueType.Debit, BalanceType.PerPeriod, "4")
                .AddLine("Increase (Decrease) in Accounts Payable", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "5")
                .AddHeader("CASH FLOWS FROM INVESTING ACTIVITIES", "II")
                .AddLine("Purchase of Property, Plant, and Equipment", FinacialStatementValueType.Debit, BalanceType.Debit, "1")
                .AddLine("Sale of Property, Plant, and Equipment", FinacialStatementValueType.Credit, BalanceType.Credit, "2")
                .AddLine("Purchase of Investments", FinacialStatementValueType.Debit, BalanceType.Debit, "3")
                .AddLine("Sale of Investments", FinacialStatementValueType.Credit, BalanceType.Credit, "4")
                .AddHeader("CASH FLOWS FROM FINANCING ACTIVITIES", "III")
                .AddLine("Proceeds from Borrowing", FinacialStatementValueType.Credit, BalanceType.Credit, "1")
                .AddLine("Repayment of Borrowing", FinacialStatementValueType.Debit, BalanceType.Debit, "2")
                .AddLine("Payment of Dividends", FinacialStatementValueType.Debit, BalanceType.Debit, "3")
                .AddLine("Proceeds from Issuance of Stock", FinacialStatementValueType.Credit, BalanceType.Credit, "4");
        }

        /// <summary>
        /// Resets the builder to empty state
        /// </summary>
        /// <returns>Reset builder</returns>
        public CashFlowStatementBuilder Reset()
        {
            _lines.Clear();
            _assignments.Clear();
            _currentDepth = 0;
            return this;
        }
    }
}