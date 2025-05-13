using System;

namespace Sivar.Erp.FinancialStatements.Generation
{
    /// <summary>
    /// DTO for cash flow statement
    /// </summary>
    public class CashFlowStatementDto
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
        /// Collection of cash flow lines with values
        /// </summary>
        public IEnumerable<CashFlowLineValueDto> Lines { get; set; } = new List<CashFlowLineValueDto>();

        /// <summary>
        /// Net cash flow for the period
        /// </summary>
        public decimal NetCashFlow { get; set; }

        /// <summary>
        /// Cash flow from operating activities
        /// </summary>
        public decimal OperatingActivities { get; set; }

        /// <summary>
        /// Cash flow from investing activities
        /// </summary>
        public decimal InvestingActivities { get; set; }

        /// <summary>
        /// Cash flow from financing activities
        /// </summary>
        public decimal FinancingActivities { get; set; }

        /// <summary>
        /// Beginning cash balance
        /// </summary>
        public decimal BeginningCash { get; set; }

        /// <summary>
        /// Ending cash balance
        /// </summary>
        public decimal EndingCash { get; set; }

        /// <summary>
        /// Company information for the header
        /// </summary>
        public CompanyHeaderDto CompanyHeader { get; set; } = new CompanyHeaderDto();

        /// <summary>
        /// Validates that the cash flow statement is balanced
        /// </summary>
        /// <returns>True if the statement balances</returns>
        public bool IsBalanced()
        {
            var calculatedNetCash = OperatingActivities + InvestingActivities + FinancingActivities;
            var calculatedEndingCash = BeginningCash + calculatedNetCash;
            return Math.Abs(calculatedEndingCash - EndingCash) < 0.01m &&
                   Math.Abs(calculatedNetCash - NetCashFlow) < 0.01m;
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