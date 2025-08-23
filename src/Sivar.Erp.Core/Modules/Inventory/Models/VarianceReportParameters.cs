using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Parameters for variance reporting
    /// </summary>
    public class VarianceReportParameters
    {
        /// <summary>
        /// Start date for the report
        /// </summary>
        [Description("Start date for the report")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for the report
        /// </summary>
        [Description("End date for the report")]
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
        /// Minimum variance threshold
        /// </summary>
        [Description("Minimum variance threshold")]
        public decimal? MinVariance { get; set; }

        /// <summary>
        /// Maximum variance threshold
        /// </summary>
        [Description("Maximum variance threshold")]
        public decimal? MaxVariance { get; set; }

        /// <summary>
        /// Include positive variances
        /// </summary>
        [Description("Include positive variances")]
        public bool IncludePositive { get; set; } = true;

        /// <summary>
        /// Include negative variances
        /// </summary>
        [Description("Include negative variances")]
        public bool IncludeNegative { get; set; } = true;

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

        /// <summary>
        /// Sort order
        /// </summary>
        [Description("Sort order")]
        public string SortBy { get; set; } = "Variance";

        /// <summary>
        /// Sort direction
        /// </summary>
        [Description("Sort direction")]
        public string SortDirection { get; set; } = "DESC";
    }
}
