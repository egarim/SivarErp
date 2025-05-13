using System;

namespace Sivar.Erp.FinancialStatements.Generation
{
    /// <summary>
    /// Builder for creating balance sheets
    /// </summary>
    public class BalanceSheetBuilder
    {
        private readonly BalanceSheetDto _balanceSheet;
        private readonly List<BalanceSheetLineDto> _lines = new List<BalanceSheetLineDto>();

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
        /// Builds the balance sheet
        /// </summary>
        /// <returns>Completed balance sheet</returns>
        public BalanceSheetDto Build()
        {
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
    }
}