using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Parameters for inventory aging analysis
    /// </summary>
    public class InventoryAgingParameters
    {
        /// <summary>
        /// As of date for the aging analysis
        /// </summary>
        [Description("As of date for the aging analysis")]
        public DateTime AsOfDate { get; set; } = DateTime.Today;

        /// <summary>
        /// First aging bucket in days
        /// </summary>
        [Description("First aging bucket in days")]
        public int FirstBucket { get; set; } = 30;

        /// <summary>
        /// Second aging bucket in days
        /// </summary>
        [Description("Second aging bucket in days")]
        public int SecondBucket { get; set; } = 60;

        /// <summary>
        /// Third aging bucket in days
        /// </summary>
        [Description("Third aging bucket in days")]
        public int ThirdBucket { get; set; } = 90;

        /// <summary>
        /// Fourth aging bucket in days
        /// </summary>
        [Description("Fourth aging bucket in days")]
        public int FourthBucket { get; set; } = 180;

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
        /// Include zero value items
        /// </summary>
        [Description("Include zero value items")]
        public bool IncludeZeroValue { get; set; } = false;

        /// <summary>
        /// Include discontinued items
        /// </summary>
        [Description("Include discontinued items")]
        public bool IncludeDiscontinued { get; set; } = true;

        /// <summary>
        /// Group by category
        /// </summary>
        [Description("Group by category")]
        public bool GroupByCategory { get; set; } = false;

        /// <summary>
        /// Group by location
        /// </summary>
        [Description("Group by location")]
        public bool GroupByLocation { get; set; } = false;
    }
}
