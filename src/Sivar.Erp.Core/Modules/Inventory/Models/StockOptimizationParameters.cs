using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Parameters for stock optimization analysis
    /// </summary>
    public class StockOptimizationParameters
    {
        /// <summary>
        /// Analysis period start date
        /// </summary>
        [Description("Analysis period start date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Analysis period end date
        /// </summary>
        [Description("Analysis period end date")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Target service level percentage
        /// </summary>
        [Description("Target service level percentage")]
        public decimal TargetServiceLevel { get; set; } = 95m;

        /// <summary>
        /// Lead time in days
        /// </summary>
        [Description("Lead time in days")]
        public int LeadTimeDays { get; set; } = 7;

        /// <summary>
        /// Safety stock percentage
        /// </summary>
        [Description("Safety stock percentage")]
        public decimal SafetyStockPercentage { get; set; } = 10m;

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
        /// Minimum turnover ratio
        /// </summary>
        [Description("Minimum turnover ratio")]
        public decimal? MinTurnoverRatio { get; set; }

        /// <summary>
        /// Maximum turnover ratio
        /// </summary>
        [Description("Maximum turnover ratio")]
        public decimal? MaxTurnoverRatio { get; set; }

        /// <summary>
        /// Include seasonal adjustments
        /// </summary>
        [Description("Include seasonal adjustments")]
        public bool IncludeSeasonality { get; set; } = true;

        /// <summary>
        /// Economic order quantity calculation
        /// </summary>
        [Description("Economic order quantity calculation")]
        public bool CalculateEOQ { get; set; } = true;

        /// <summary>
        /// Carrying cost percentage
        /// </summary>
        [Description("Carrying cost percentage")]
        public decimal CarryingCostPercentage { get; set; } = 20m;

        /// <summary>
        /// Ordering cost amount
        /// </summary>
        [Description("Ordering cost amount")]
        public decimal OrderingCost { get; set; } = 50m;
    }
}
