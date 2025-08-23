using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Parameters for performance metrics calculation
    /// </summary>
    public class PerformanceMetricsParameters
    {
        /// <summary>
        /// Start date for metrics calculation
        /// </summary>
        [Description("Start date for metrics calculation")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for metrics calculation
        /// </summary>
        [Description("End date for metrics calculation")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Location filter
        /// </summary>
        [Description("Location filter")]
        public string? Location { get; set; }

        /// <summary>
        /// Category filter
        /// </summary>
        [Description("Category filter")]
        public string? Category { get; set; }

        /// <summary>
        /// Include KPI calculations
        /// </summary>
        [Description("Include KPI calculations")]
        public bool IncludeKPIs { get; set; } = true;

        /// <summary>
        /// Include trend analysis
        /// </summary>
        [Description("Include trend analysis")]
        public bool IncludeTrends { get; set; } = true;

        /// <summary>
        /// Compare with previous period
        /// </summary>
        [Description("Compare with previous period")]
        public bool ComparePreviousPeriod { get; set; } = true;

        /// <summary>
        /// Group by location
        /// </summary>
        [Description("Group by location")]
        public bool GroupByLocation { get; set; } = false;

        /// <summary>
        /// Group by category
        /// </summary>
        [Description("Group by category")]
        public bool GroupByCategory { get; set; } = false;
    }
}
