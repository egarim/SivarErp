using System;

namespace Sivar.Erp.FinancialStatements.Generation
{
    /// <summary>
    /// DTO for financial performance summary
    /// </summary>
    public class FinancialPerformanceSummaryDto
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
        /// Total revenues
        /// </summary>
        public decimal TotalRevenues { get; set; }

        /// <summary>
        /// Total expenses
        /// </summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>
        /// Net income
        /// </summary>
        public decimal NetIncome { get; set; }

        /// <summary>
        /// Gross profit
        /// </summary>
        public decimal GrossProfit { get; set; }

        /// <summary>
        /// Operating income
        /// </summary>
        public decimal OperatingIncome { get; set; }

        /// <summary>
        /// Net cash flow from operations
        /// </summary>
        public decimal OperatingCashFlow { get; set; }

        /// <summary>
        /// Gross profit margin
        /// </summary>
        public decimal GrossProfitMargin { get; set; }

        /// <summary>
        /// Net profit margin
        /// </summary>
        public decimal NetProfitMargin { get; set; }

        /// <summary>
        /// Operating margin
        /// </summary>
        public decimal OperatingMargin { get; set; }
    }
    /// <summary>
    /// Builder for creating balance sheets
    /// </summary>
}