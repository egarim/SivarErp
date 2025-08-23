using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Query parameters for outstanding cycle counts
    /// </summary>
    public class OutstandingCountsQuery
    {
        /// <summary>
        /// Location filter
        /// </summary>
        [Description("Location filter")]
        public string? Location { get; set; }

        /// <summary>
        /// Item filter
        /// </summary>
        [Description("Item filter")]
        public Guid? ItemId { get; set; }

        /// <summary>
        /// Category filter
        /// </summary>
        [Description("Category filter")]
        public string? Category { get; set; }

        /// <summary>
        /// Assigned to user
        /// </summary>
        [Description("Assigned to user")]
        public string? AssignedTo { get; set; }

        /// <summary>
        /// Due date from
        /// </summary>
        [Description("Due date from")]
        public DateTime? DueDateFrom { get; set; }

        /// <summary>
        /// Due date to
        /// </summary>
        [Description("Due date to")]
        public DateTime? DueDateTo { get; set; }

        /// <summary>
        /// Priority filter
        /// </summary>
        [Description("Priority filter")]
        public CycleCountPriority? Priority { get; set; }

        /// <summary>
        /// Maximum number of results
        /// </summary>
        [Description("Maximum number of results")]
        public int? MaxResults { get; set; }

        /// <summary>
        /// Skip count for pagination
        /// </summary>
        [Description("Skip count for pagination")]
        public int Skip { get; set; } = 0;
    }

    /// <summary>
    /// Priority levels for cycle counts
    /// </summary>
    public enum CycleCountPriority
    {
        /// <summary>
        /// Low priority
        /// </summary>
        Low = 1,

        /// <summary>
        /// Normal priority
        /// </summary>
        Normal = 2,

        /// <summary>
        /// High priority
        /// </summary>
        High = 3,

        /// <summary>
        /// Urgent priority
        /// </summary>
        Urgent = 4
    }
}
