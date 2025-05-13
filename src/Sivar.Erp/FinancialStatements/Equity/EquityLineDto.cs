using System;

namespace Sivar.Erp.FinancialStatements.Equity
{
    /// <summary>
    /// Implementation of equity line entity
    /// </summary>
    public class EquityLineDto : IEquityLine
    {
        /// <summary>
        /// Unique identifier for the equity line
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Index for controlling display order
        /// </summary>
        public int VisibleIndex { get; set; }

        /// <summary>
        /// Number to print in the report (e.g., "1", "2", "A", "B")
        /// </summary>
        public string PrintedNo { get; set; } = string.Empty;

        /// <summary>
        /// Text to display for this line
        /// </summary>
        public string LineText { get; set; } = string.Empty;

        /// <summary>
        /// Type of the equity line that defines overall structure
        /// </summary>
        public EquityLineType LineType { get; set; }

        /// <summary>
        /// UTC timestamp when the line was created
        /// </summary>
        public DateTime InsertedAt { get; set; }

        /// <summary>
        /// User who created the line
        /// </summary>
        public string InsertedBy { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the line was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who last updated the line
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Validates the equity line according to business rules
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // Line text is required
            if (string.IsNullOrWhiteSpace(LineText))
            {
                return false;
            }

            // Visible index cannot be negative
            if (VisibleIndex < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if this line is a first period delta type
        /// </summary>
        /// <returns>True if represents first period change</returns>
        public bool IsFirstPeriodDelta()
        {
            return LineType == EquityLineType.FirstDelta;
        }

        /// <summary>
        /// Determines if this line is a second period delta type
        /// </summary>
        /// <returns>True if represents second period change</returns>
        public bool IsSecondPeriodDelta()
        {
            return LineType == EquityLineType.SecondDelta;
        }

        /// <summary>
        /// Determines if this line represents a balance (not a change)
        /// </summary>
        /// <returns>True if represents a balance</returns>
        public bool IsBalanceLine()
        {
            return LineType == EquityLineType.InitialBalance ||
                   LineType == EquityLineType.ZeroBalance ||
                   LineType == EquityLineType.FirstBalance ||
                   LineType == EquityLineType.SecondBalance;
        }
    }
    /// <summary>
    /// Interface for equity line service operations
    /// </summary>


    /// <summary>
    /// Validation result for equity lines
    /// </summary>


    /// <summary>
    /// Builder class for creating equity statement structure
    /// </summary>
}