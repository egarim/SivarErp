using System;

namespace Sivar.Erp.FinancialStatements.Generation
{
    /// <summary>
    /// DTO for income statement line with calculated value
    /// </summary>
    public class IncomeStatementLineDto
    {
        /// <summary>
        /// Printed number for the line
        /// </summary>
        public string PrintedNo { get; set; } = string.Empty;

        /// <summary>
        /// Text to display for the line
        /// </summary>
        public string LineText { get; set; } = string.Empty;

        /// <summary>
        /// Calculated amount for the line
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Whether this is a header line
        /// </summary>
        public bool IsHeader { get; set; }

        /// <summary>
        /// Indentation level for display
        /// </summary>
        public int IndentLevel { get; set; }

        /// <summary>
        /// Type of the income line
        /// </summary>
        public BalanceIncomeLineType LineType { get; set; }

        /// <summary>
        /// Associated account IDs
        /// </summary>
        public IEnumerable<Guid> AccountIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Determines if this line should be bold
        /// </summary>
        /// <returns>True if line should be bold</returns>
        public bool ShouldBeBold()
        {
            return IsHeader || LineText.ToUpper().Contains("TOTAL") || LineText.ToUpper().Contains("NET");
        }

        /// <summary>
        /// Gets the formatted amount string
        /// </summary>
        /// <param name="showCurrency">Whether to show currency symbol</param>
        /// <returns>Formatted amount</returns>
        public string GetFormattedAmount(bool showCurrency = true)
        {
            if (Amount == 0m && IsHeader)
                return string.Empty;
            return showCurrency ? $"${Amount:N2}" : Amount.ToString("N2");
        }
    }
    /// <summary>
    /// DTO for company header information in statements
    /// </summary>


    /// <summary>
    /// Builder for creating balance sheets
    /// </summary>
}