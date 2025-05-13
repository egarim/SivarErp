using System;

namespace Sivar.Erp.FinancialStatements.Equity
{
    /// <summary>
    /// Implementation of equity column entity
    /// </summary>
    public class EquityColumnDto : IEquityColumn
    {
        /// <summary>
        /// Unique identifier for the equity column
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Text to display in the column header
        /// </summary>
        public string ColumnText { get; set; } = string.Empty;

        /// <summary>
        /// Index for controlling display order of columns
        /// </summary>
        public int VisibleIndex { get; set; }

        /// <summary>
        /// UTC timestamp when the column was created
        /// </summary>
        public DateTime InsertedAt { get; set; }

        /// <summary>
        /// User who created the column
        /// </summary>
        public string InsertedBy { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the column was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who last updated the column
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Validates the equity column according to business rules
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // Column text is required
            if (string.IsNullOrWhiteSpace(ColumnText))
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
        /// Determines if this is a total column
        /// </summary>
        /// <returns>True if this column represents totals</returns>
        public bool IsTotalColumn()
        {
            return ColumnText.ToUpper().Contains("TOTAL") ||
                   ColumnText.ToUpper().Contains("SUM");
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