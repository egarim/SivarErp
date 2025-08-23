using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Request for scheduling cycle counts
    /// </summary>
    public class CycleCountScheduleRequest
    {
        /// <summary>
        /// Location for the cycle count
        /// </summary>
        [Description("Location for the cycle count")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Category to count
        /// </summary>
        [Description("Category to count")]
        public string? Category { get; set; }

        /// <summary>
        /// Specific items to count
        /// </summary>
        [Description("Specific items to count")]
        public Guid[]? ItemIds { get; set; }

        /// <summary>
        /// Scheduled date for the count
        /// </summary>
        [Description("Scheduled date for the count")]
        public DateTime ScheduledDate { get; set; }

        /// <summary>
        /// Priority of the count
        /// </summary>
        [Description("Priority of the count")]
        public CycleCountPriority Priority { get; set; } = CycleCountPriority.Normal;

        /// <summary>
        /// Assigned user for the count
        /// </summary>
        [Description("Assigned user for the count")]
        public string? AssignedTo { get; set; }

        /// <summary>
        /// Count frequency
        /// </summary>
        [Description("Count frequency")]
        public CycleCountFrequency Frequency { get; set; } = CycleCountFrequency.Manual;

        /// <summary>
        /// Notes for the count
        /// </summary>
        [Description("Notes for the count")]
        public string? Notes { get; set; }

        /// <summary>
        /// Include ABC A items
        /// </summary>
        [Description("Include ABC A items")]
        public bool IncludeAItems { get; set; } = true;

        /// <summary>
        /// Include ABC B items
        /// </summary>
        [Description("Include ABC B items")]
        public bool IncludeBItems { get; set; } = true;

        /// <summary>
        /// Include ABC C items
        /// </summary>
        [Description("Include ABC C items")]
        public bool IncludeCItems { get; set; } = true;
    }

    /// <summary>
    /// Frequency options for cycle counts
    /// </summary>
    public enum CycleCountFrequency
    {
        /// <summary>
        /// Manual count
        /// </summary>
        Manual,

        /// <summary>
        /// Daily count
        /// </summary>
        Daily,

        /// <summary>
        /// Weekly count
        /// </summary>
        Weekly,

        /// <summary>
        /// Monthly count
        /// </summary>
        Monthly,

        /// <summary>
        /// Quarterly count
        /// </summary>
        Quarterly,

        /// <summary>
        /// Annual count
        /// </summary>
        Annual
    }
}
