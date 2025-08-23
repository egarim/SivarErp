using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Parameters for slow moving inventory analysis
    /// </summary>
    public class SlowMovingAnalysisParameters
    {
        /// <summary>
        /// Start date for analysis
        /// </summary>
        [Description("Start date for analysis")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for analysis
        /// </summary>
        [Description("End date for analysis")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Minimum days without movement to consider slow
        /// </summary>
        [Description("Minimum days without movement to consider slow")]
        public int SlowMovingThresholdDays { get; set; } = 90;

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
        /// Minimum value threshold to include
        /// </summary>
        [Description("Minimum value threshold to include")]
        public decimal? MinValue { get; set; }

        /// <summary>
        /// Include zero stock items
        /// </summary>
        [Description("Include zero stock items")]
        public bool IncludeZeroStock { get; set; } = false;

        /// <summary>
        /// Include discontinued items
        /// </summary>
        [Description("Include discontinued items")]
        public bool IncludeDiscontinued { get; set; } = true;

        /// <summary>
        /// Sort order for results
        /// </summary>
        [Description("Sort order for results")]
        public string SortBy { get; set; } = "DaysWithoutMovement";

        /// <summary>
        /// Sort direction (ASC/DESC)
        /// </summary>
        [Description("Sort direction (ASC/DESC)")]
        public string SortDirection { get; set; } = "DESC";
    }
}
