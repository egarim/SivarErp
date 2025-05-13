using System;

namespace Sivar.Erp.FinancialStatements.Generation
{
    /// <summary>
    /// DTO for equity line with calculated values for each column
    /// </summary>
    public class EquityLineValueDto
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
        /// Dictionary of column values (column ID -> amount)
        /// </summary>
        public Dictionary<Guid, decimal> ColumnValues { get; set; } = new Dictionary<Guid, decimal>();

        /// <summary>
        /// Type of the equity line
        /// </summary>
        public EquityLineType LineType { get; set; }

        /// <summary>
        /// Gets the total value across all columns
        /// </summary>
        /// <returns>Sum of all column values</returns>
        public decimal GetTotalValue()
        {
            return ColumnValues.Values.Sum();
        }

        /// <summary>
        /// Gets the value for a specific column
        /// </summary>
        /// <param name="columnId">Column ID</param>
        /// <returns>Value for the column, or 0 if not found</returns>
        public decimal GetColumnValue(Guid columnId)
        {
            return ColumnValues.TryGetValue(columnId, out var value) ? value : 0m;
        }

        /// <summary>
        /// Sets the value for a specific column
        /// </summary>
        /// <param name="columnId">Column ID</param>
        /// <param name="value">Value to set</param>
        public void SetColumnValue(Guid columnId, decimal value)
        {
            ColumnValues[columnId] = value;
        }
    }
    /// <summary>
    /// DTO for company header information in statements
    /// </summary>


    /// <summary>
    /// Builder for creating balance sheets
    /// </summary>
}