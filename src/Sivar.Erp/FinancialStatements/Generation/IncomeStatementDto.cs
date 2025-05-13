using System;

namespace Sivar.Erp.FinancialStatements.Generation
{
    /// <summary>
    /// DTO for income statement
    /// </summary>
    public class IncomeStatementDto
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
        /// Collection of income statement lines
        /// </summary>
        public IEnumerable<IncomeStatementLineDto> Lines { get; set; } = new List<IncomeStatementLineDto>();

        /// <summary>
        /// Net income for the period
        /// </summary>
        public decimal NetIncome { get; set; }

        /// <summary>
        /// Total revenues
        /// </summary>
        public decimal TotalRevenues { get; set; }

        /// <summary>
        /// Total expenses
        /// </summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>
        /// Company information for the header
        /// </summary>
        public CompanyHeaderDto CompanyHeader { get; set; } = new CompanyHeaderDto();

        /// <summary>
        /// Gets the period description
        /// </summary>
        /// <returns>Period description string</returns>
        public string GetPeriodDescription()
        {
            if (StartDate.Month == 1 && StartDate.Day == 1 &&
                EndDate.Month == 12 && EndDate.Day == 31 &&
                StartDate.Year == EndDate.Year)
            {
                return $"Year ended {EndDate:yyyy}";
            }
            return $"Period from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
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