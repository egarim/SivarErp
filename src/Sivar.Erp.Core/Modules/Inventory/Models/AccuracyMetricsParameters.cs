using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Parameters for calculating accuracy metrics
    /// </summary>
    public class AccuracyMetricsParameters
    {
        /// <summary>
        /// Start date for the metrics calculation
        /// </summary>
        [Description("Start date for the metrics calculation")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for the metrics calculation
        /// </summary>
        [Description("End date for the metrics calculation")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Location filter for metrics
        /// </summary>
        [Description("Location filter for metrics")]
        public string? Location { get; set; }

        /// <summary>
        /// Item category filter
        /// </summary>
        [Description("Item category filter")]
        public string? Category { get; set; }

        /// <summary>
        /// Minimum variance threshold to include
        /// </summary>
        [Description("Minimum variance threshold to include")]
        public decimal? MinVarianceThreshold { get; set; }

        /// <summary>
        /// Maximum variance threshold to include
        /// </summary>
        [Description("Maximum variance threshold to include")]
        public decimal? MaxVarianceThreshold { get; set; }

        /// <summary>
        /// Whether to include zero variance items
        /// </summary>
        [Description("Whether to include zero variance items")]
        public bool IncludeZeroVariance { get; set; } = true;

        /// <summary>
        /// Group results by location
        /// </summary>
        [Description("Group results by location")]
        public bool GroupByLocation { get; set; } = false;

        /// <summary>
        /// Group results by category
        /// </summary>
        [Description("Group results by category")]
        public bool GroupByCategory { get; set; } = false;
    }
}
