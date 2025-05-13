using Sivar.Erp.FinancialStatements.Equity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.FinancialStatements.Generation
{
    #region Service Interfaces (Task 5.1)

    /// <summary>
    /// Interface for financial statement generation service
    /// </summary>
    public interface IFinancialStatementService
    {
        /// <summary>
        /// Generates a balance sheet as of a specific date
        /// </summary>
        /// <param name="asOfDate">Date to generate balance sheet for</param>
        /// <returns>Generated balance sheet</returns>
        Task<BalanceSheetDto> GenerateBalanceSheetAsync(DateOnly asOfDate);

        /// <summary>
        /// Generates an income statement for a date range
        /// </summary>
        /// <param name="startDate">Start date of the period</param>
        /// <param name="endDate">End date of the period</param>
        /// <returns>Generated income statement</returns>
        Task<IncomeStatementDto> GenerateIncomeStatementAsync(DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Generates a cash flow statement for a date range
        /// </summary>
        /// <param name="startDate">Start date of the period</param>
        /// <param name="endDate">End date of the period</param>
        /// <returns>Generated cash flow statement</returns>
        Task<CashFlowStatementDto> GenerateCashFlowStatementAsync(DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Generates an equity statement for a date range
        /// </summary>
        /// <param name="startDate">Start date of the period</param>
        /// <param name="endDate">End date of the period</param>
        /// <returns>Generated equity statement</returns>
        Task<EquityStatementDto> GenerateEquityStatementAsync(DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Validates that the balance sheet balances
        /// </summary>
        /// <param name="balanceSheet">Balance sheet to validate</param>
        /// <returns>True if assets equal liabilities and equity</returns>
        bool ValidateBalanceSheet(BalanceSheetDto balanceSheet);

        /// <summary>
        /// Gets the financial position summary
        /// </summary>
        /// <param name="asOfDate">Date for the summary</param>
        /// <returns>Financial position summary</returns>
        Task<FinancialPositionSummaryDto> GetFinancialPositionSummaryAsync(DateOnly asOfDate);

        /// <summary>
        /// Gets the financial performance summary
        /// </summary>
        /// <param name="startDate">Start date of the period</param>
        /// <param name="endDate">End date of the period</param>
        /// <returns>Financial performance summary</returns>
        Task<FinancialPerformanceSummaryDto> GetFinancialPerformanceSummaryAsync(DateOnly startDate, DateOnly endDate);
    }

    #endregion

    #region Statement DTOs (Task 5.2)

    /// <summary>
    /// DTO for balance sheet
    /// </summary>
        #endregion

    #region Statement Line Value DTOs (Task 5.3)

    /// <summary>
    /// DTO for balance sheet line with calculated value
    /// </summary>
        #endregion

    #region Supporting DTOs

    /// <summary>
    /// DTO for company header information in statements
    /// </summary>
        #endregion

    #region Builder Classes

    /// <summary>
    /// Builder for creating balance sheets
    /// </summary>
        #endregion
}