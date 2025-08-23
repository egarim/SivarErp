using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Parameters for demand forecasting
    /// </summary>
    public class DemandForecastParameters
    {
        /// <summary>
        /// Start date for historical data analysis
        /// </summary>
        [Description("Start date for historical data analysis")]
        public DateTime HistoryStartDate { get; set; }

        /// <summary>
        /// End date for historical data analysis
        /// </summary>
        [Description("End date for historical data analysis")]
        public DateTime HistoryEndDate { get; set; }

        /// <summary>
        /// Forecast period in months
        /// </summary>
        [Description("Forecast period in months")]
        public int ForecastPeriodMonths { get; set; } = 3;

        /// <summary>
        /// Forecasting method
        /// </summary>
        [Description("Forecasting method")]
        public string Method { get; set; } = "MovingAverage";

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
        /// Seasonality adjustment factor
        /// </summary>
        [Description("Seasonality adjustment factor")]
        public decimal SeasonalityFactor { get; set; } = 1.0m;

        /// <summary>
        /// Trend adjustment factor
        /// </summary>
        [Description("Trend adjustment factor")]
        public decimal TrendFactor { get; set; } = 1.0m;

        /// <summary>
        /// Confidence level percentage
        /// </summary>
        [Description("Confidence level percentage")]
        public decimal ConfidenceLevel { get; set; } = 95m;

        /// <summary>
        /// Include outliers in analysis
        /// </summary>
        [Description("Include outliers in analysis")]
        public bool IncludeOutliers { get; set; } = false;
    }
}
