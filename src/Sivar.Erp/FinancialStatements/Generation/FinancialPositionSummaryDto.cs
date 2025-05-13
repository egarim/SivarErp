using System;

namespace Sivar.Erp.FinancialStatements.Generation
{
    /// <summary>
    /// DTO for financial position summary
    /// </summary>
    public class FinancialPositionSummaryDto
    {
        /// <summary>
        /// Date of the summary
        /// </summary>
        public DateOnly AsOfDate { get; set; }

        /// <summary>
        /// Total assets
        /// </summary>
        public decimal TotalAssets { get; set; }

        /// <summary>
        /// Current assets
        /// </summary>
        public decimal CurrentAssets { get; set; }

        /// <summary>
        /// Non-current assets
        /// </summary>
        public decimal NonCurrentAssets { get; set; }

        /// <summary>
        /// Total liabilities
        /// </summary>
        public decimal TotalLiabilities { get; set; }

        /// <summary>
        /// Current liabilities
        /// </summary>
        public decimal CurrentLiabilities { get; set; }

        /// <summary>
        /// Non-current liabilities
        /// </summary>
        public decimal NonCurrentLiabilities { get; set; }

        /// <summary>
        /// Total equity
        /// </summary>
        public decimal TotalEquity { get; set; }

        /// <summary>
        /// Working capital (current assets - current liabilities)
        /// </summary>
        public decimal WorkingCapital { get; set; }

        /// <summary>
        /// Current ratio
        /// </summary>
        public decimal CurrentRatio { get; set; }

        /// <summary>
        /// Debt to equity ratio
        /// </summary>
        public decimal DebtToEquityRatio { get; set; }
    }
    /// <summary>
    /// Builder for creating balance sheets
    /// </summary>
}