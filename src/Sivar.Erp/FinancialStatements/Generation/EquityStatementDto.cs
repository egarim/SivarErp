using Sivar.Erp.FinancialStatements.Equity;
using System;

namespace Sivar.Erp.FinancialStatements.Generation
{
    /// <summary>
    /// DTO for equity statement
    /// </summary>
    public class EquityStatementDto
    {
        /// <summary>
        /// Start date of the period
        /// </summary>
        public DateOnly StartDate { get; set; }

        /// <summary>
        /// End date of the period
        /// </summary>
        public DateOnly EndDate { get; set; }

        /// <summary>
        /// Collection of equity lines with values
        /// </summary>
        public IEnumerable<EquityLineValueDto> Lines { get; set; } = new List<EquityLineValueDto>();

        /// <summary>
        /// Collection of equity columns
        /// </summary>
        public IEnumerable<EquityColumnDto> Columns { get; set; } = new List<EquityColumnDto>();

        /// <summary>
        /// Company information for the header
        /// </summary>
        public CompanyHeaderDto CompanyHeader { get; set; } = new CompanyHeaderDto();

        /// <summary>
        /// Gets total equity at the beginning of the period
        /// </summary>
        /// <returns>Beginning total equity</returns>
        public decimal GetBeginningTotalEquity()
        {
            var initialBalanceLine = Lines.FirstOrDefault(l => l.LineType == EquityLineType.InitialBalance);
            return initialBalanceLine?.ColumnValues.Values.Sum() ?? 0m;
        }

        /// <summary>
        /// Gets total equity at the end of the period
        /// </summary>
        /// <returns>Ending total equity</returns>
        public decimal GetEndingTotalEquity()
        {
            var secondBalanceLine = Lines.FirstOrDefault(l => l.LineType == EquityLineType.SecondBalance);
            return secondBalanceLine?.ColumnValues.Values.Sum() ?? 0m;
        }
    }
    /// <summary>
    /// DTO for balance sheet line with calculated value
    /// </summary>


    /// <summary>
    /// DTO for company header information in statements
    /// </summary>


    /// <summary>
    /// Builder for creating balance sheets
    /// </summary>
}