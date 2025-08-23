using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Parameters for inventory analytics operations
    /// </summary>
    public class InventoryAnalyticsParameters
    {
        /// <summary>
        /// Start date for analytics
        /// </summary>
        [Description("Start date for analytics")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for analytics
        /// </summary>
        [Description("End date for analytics")]
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
        /// Item filter
        /// </summary>
        [Description("Item filter")]
        public Guid? ItemId { get; set; }

        /// <summary>
        /// Include inactive items
        /// </summary>
        [Description("Include inactive items")]
        public bool IncludeInactive { get; set; } = false;

        /// <summary>
        /// Minimum value threshold
        /// </summary>
        [Description("Minimum value threshold")]
        public decimal? MinValue { get; set; }

        /// <summary>
        /// Maximum value threshold
        /// </summary>
        [Description("Maximum value threshold")]
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// Analysis granularity (daily, weekly, monthly)
        /// </summary>
        [Description("Analysis granularity (daily, weekly, monthly)")]
        public string Granularity { get; set; } = "daily";

        /// <summary>
        /// Include trend analysis
        /// </summary>
        [Description("Include trend analysis")]
        public bool IncludeTrends { get; set; } = true;
    }
}
