using System;

namespace Sivar.Erp.FinancialStatements.Generation
{
    /// <summary>
    /// DTO for balance sheet
    /// </summary>
    public class BalanceSheetDto
    {
        /// <summary>
        /// Date the balance sheet is prepared as of
        /// </summary>
        public DateOnly AsOfDate { get; set; }

        /// <summary>
        /// Collection of balance sheet lines
        /// </summary>
        public IEnumerable<BalanceSheetLineDto> Lines { get; set; } = new List<BalanceSheetLineDto>();

        /// <summary>
        /// Total assets amount
        /// </summary>
        public decimal TotalAssets { get; set; }

        /// <summary>
        /// Total liabilities and equity amount
        /// </summary>
        public decimal TotalLiabilitiesAndEquity { get; set; }

        /// <summary>
        /// Company information for the header
        /// </summary>
        public CompanyHeaderDto CompanyHeader { get; set; } = new CompanyHeaderDto();

        /// <summary>
        /// Notes and disclosures
        /// </summary>
        public IEnumerable<string> Notes { get; set; } = new List<string>();

        /// <summary>
        /// Checks if the balance sheet is balanced
        /// </summary>
        /// <returns>True if assets equal liabilities and equity</returns>
        public bool IsBalanced()
        {
            return Math.Abs(TotalAssets - TotalLiabilitiesAndEquity) < 0.01m;
        }

        /// <summary>
        /// Gets the balancing difference (should be zero)
        /// </summary>
        /// <returns>Difference between assets and liabilities + equity</returns>
        public decimal GetBalancingDifference()
        {
            return TotalAssets - TotalLiabilitiesAndEquity;
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