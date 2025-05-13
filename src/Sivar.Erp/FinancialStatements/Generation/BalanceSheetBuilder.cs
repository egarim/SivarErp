using System;
using System.Collections.Generic;
using System.Linq;

namespace Sivar.Erp.FinancialStatements.Generation
{
    /// <summary>
    /// Builder for creating balance sheets
    /// </summary>
    public class BalanceSheetBuilder
    {
        private readonly BalanceSheetDto _balanceSheet;
        private readonly List<BalanceSheetLineDto> _lines = new List<BalanceSheetLineDto>();
        private readonly Dictionary<string, List<Guid>> _accountGroupings = new Dictionary<string, List<Guid>>();
        private readonly Dictionary<string, decimal> _accountBalances = new Dictionary<string, decimal>();

        /// <summary>
        /// Initializes a new instance of the builder
        /// </summary>
        /// <param name="asOfDate">Date for the balance sheet</param>
        public BalanceSheetBuilder(DateOnly asOfDate)
        {
            _balanceSheet = new BalanceSheetDto { AsOfDate = asOfDate };
        }

        /// <summary>
        /// Adds a line to the balance sheet
        /// </summary>
        /// <param name="line">Line to add</param>
        /// <returns>Builder for fluent interface</returns>
        public BalanceSheetBuilder AddLine(BalanceSheetLineDto line)
        {
            _lines.Add(line);
            return this;
        }

        /// <summary>
        /// Adds an account grouping for aggregation
        /// </summary>
        /// <param name="groupKey">Key for the group</param>
        /// <param name="accountIds">Account IDs in this group</param>
        /// <returns>Builder for fluent interface</returns>
        public BalanceSheetBuilder AddAccountGrouping(string groupKey, IEnumerable<Guid> accountIds)
        {
            _accountGroupings[groupKey] = accountIds.ToList();
            return this;
        }

        /// <summary>
        /// Adds account balances from the general ledger
        /// </summary>
        /// <param name="accountBalances">Dictionary of account balances by account ID</param>
        /// <returns>Builder for fluent interface</returns>
        public BalanceSheetBuilder AddAccountBalances(Dictionary<string, decimal> accountBalances)
        {
            foreach (var kvp in accountBalances)
            {
                _accountBalances[kvp.Key] = kvp.Value;
            }
            return this;
        }

        /// <summary>
        /// Sets company header information
        /// </summary>
        /// <param name="companyHeader">Company header</param>
        /// <returns>Builder for fluent interface</returns>
        public BalanceSheetBuilder SetCompanyHeader(CompanyHeaderDto companyHeader)
        {
            _balanceSheet.CompanyHeader = companyHeader;
            return this;
        }

        /// <summary>
        /// Adds notes to the balance sheet
        /// </summary>
        /// <param name="notes">Notes to add</param>
        /// <returns>Builder for fluent interface</returns>
        public BalanceSheetBuilder AddNotes(IEnumerable<string> notes)
        {
            if (_balanceSheet.Notes == null)
                _balanceSheet.Notes = new List<string>();

            ((List<string>)_balanceSheet.Notes).AddRange(notes);
            return this;
        }

        /// <summary>
        /// Sets retained earnings based on income statement results
        /// </summary>
        /// <param name="retainedEarningsAmount">Retained earnings amount</param>
        /// <param name="lineKey">Line identifier to update</param>
        /// <returns>Builder for fluent interface</returns>
        public BalanceSheetBuilder SetRetainedEarnings(decimal retainedEarningsAmount, string lineKey)
        {
            var retainedEarningsLine = _lines.FirstOrDefault(l => l.PrintedNo == lineKey);
            if (retainedEarningsLine != null)
            {
                retainedEarningsLine.Amount = retainedEarningsAmount;
            }
            return this;
        }

        /// <summary>
        /// Builds the balance sheet
        /// </summary>
        /// <returns>Completed balance sheet</returns>
        public BalanceSheetDto Build()
        {
            AggregateAccountBalances();
            CalculateHierarchy();
            _balanceSheet.Lines = _lines;
            CalculateTotals();
            return _balanceSheet;
        }

        /// <summary>
        /// Calculates total assets and liabilities + equity
        /// </summary>
        private void CalculateTotals()
        {
            _balanceSheet.TotalAssets = _lines
                .Where(l => l.LineType == BalanceIncomeLineType.BalanceLine && l.Amount > 0)
                .Sum(l => l.Amount);

            _balanceSheet.TotalLiabilitiesAndEquity = _lines
                .Where(l => l.LineType == BalanceIncomeLineType.BalanceLine && l.Amount < 0)
                .Sum(l => -l.Amount);
        }

        /// <summary>
        /// Aggregates account balances into line items
        /// </summary>
        private void AggregateAccountBalances()
        {
            foreach (var line in _lines.Where(l => l.AccountIds != null && l.AccountIds.Any()))
            {
                decimal lineSum = 0;
                foreach (var accountId in line.AccountIds)
                {
                    if (_accountBalances.TryGetValue(accountId.ToString(), out decimal balance))
                    {
                        lineSum += balance;
                    }
                }
                line.Amount = lineSum;
            }

            // Handle account groupings
            foreach (var line in _lines)
            {
                if (string.IsNullOrEmpty(line.PrintedNo) || !_accountGroupings.ContainsKey(line.PrintedNo))
                    continue;

                decimal groupSum = 0;
                foreach (var accountId in _accountGroupings[line.PrintedNo])
                {
                    if (_accountBalances.TryGetValue(accountId.ToString(), out decimal balance))
                    {
                        groupSum += balance;
                    }
                }
                line.Amount = groupSum;
            }
        }

        /// <summary>
        /// Calculates hierarchical totals for header items
        /// </summary>
        private void CalculateHierarchy()
        {
            // Track indent levels and calculate sums for headers
            var levelTotals = new Dictionary<int, decimal>();

            // Process lines bottom-up
            for (int i = _lines.Count - 1; i >= 0; i--)
            {
                var line = _lines[i];

                if (!line.IsHeader)
                {
                    // Track running sum for current level
                    if (!levelTotals.ContainsKey(line.IndentLevel))
                    {
                        levelTotals[line.IndentLevel] = 0;
                    }
                    levelTotals[line.IndentLevel] += line.Amount;
                }
                else if (line.IsHeader)
                {
                    // For headers, sum all lines at higher indentation levels
                    int headerLevel = line.IndentLevel;
                    int childLevel = headerLevel + 1;

                    if (levelTotals.ContainsKey(childLevel))
                    {
                        line.Amount = levelTotals[childLevel];

                        // Add header amount to its level's running total
                        if (!levelTotals.ContainsKey(headerLevel))
                        {
                            levelTotals[headerLevel] = 0;
                        }
                        levelTotals[headerLevel] += line.Amount;
                    }
                }
            }
        }
    }
}