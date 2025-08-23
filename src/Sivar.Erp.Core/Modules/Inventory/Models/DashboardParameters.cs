using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Parameters for dashboard data
    /// </summary>
    public class DashboardParameters
    {
        /// <summary>
        /// Start date for dashboard data
        /// </summary>
        [Description("Start date for dashboard data")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);

        /// <summary>
        /// End date for dashboard data
        /// </summary>
        [Description("End date for dashboard data")]
        public DateTime EndDate { get; set; } = DateTime.Today;

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
        /// Include charts data
        /// </summary>
        [Description("Include charts data")]
        public bool IncludeCharts { get; set; } = true;

        /// <summary>
        /// Include summary statistics
        /// </summary>
        [Description("Include summary statistics")]
        public bool IncludeSummary { get; set; } = true;

        /// <summary>
        /// Include alerts and notifications
        /// </summary>
        [Description("Include alerts and notifications")]
        public bool IncludeAlerts { get; set; } = true;

        /// <summary>
        /// Refresh interval in minutes
        /// </summary>
        [Description("Refresh interval in minutes")]
        public int RefreshInterval { get; set; } = 15;

        /// <summary>
        /// Maximum number of items to show in lists
        /// </summary>
        [Description("Maximum number of items to show in lists")]
        public int MaxItems { get; set; } = 10;
    }
}
