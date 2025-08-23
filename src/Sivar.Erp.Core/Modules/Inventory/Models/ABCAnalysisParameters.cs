using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Parameters for ABC analysis
    /// </summary>
    public class ABCAnalysisParameters
    {
        /// <summary>
        /// Start date for analysis period
        /// </summary>
        [Description("Start date for analysis period")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for analysis period
        /// </summary>
        [Description("End date for analysis period")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Analysis method (Value, Quantity, Margin)
        /// </summary>
        [Description("Analysis method (Value, Quantity, Margin)")]
        public ABCClassificationMethod Method { get; set; } = ABCClassificationMethod.Value;

        /// <summary>
        /// Percentage threshold for A items
        /// </summary>
        [Description("Percentage threshold for A items")]
        public decimal AItemsThreshold { get; set; } = 80m;

        /// <summary>
        /// Percentage threshold for B items
        /// </summary>
        [Description("Percentage threshold for B items")]
        public decimal BItemsThreshold { get; set; } = 15m;

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
        /// Include inactive items
        /// </summary>
        [Description("Include inactive items")]
        public bool IncludeInactive { get; set; } = false;
    }

    /// <summary>
    /// Methods for ABC classification
    /// </summary>
    public enum ABCClassificationMethod
    {
        /// <summary>
        /// Based on item value
        /// </summary>
        Value,

        /// <summary>
        /// Based on quantity sold
        /// </summary>
        Quantity,

        /// <summary>
        /// Based on profit margin
        /// </summary>
        Margin,

        /// <summary>
        /// Based on frequency of sales
        /// </summary>
        Frequency
    }

    /// <summary>
    /// ABC classification categories
    /// </summary>
    [Description("ABC classification categories")]
    public enum ABCClassification
    {
        /// <summary>
        /// Class A - High value items
        /// </summary>
        [Description("Class A - High value items")]
        A,

        /// <summary>
        /// Class B - Medium value items
        /// </summary>
        [Description("Class B - Medium value items")]
        B,

        /// <summary>
        /// Class C - Low value items
        /// </summary>
        [Description("Class C - Low value items")]
        C
    }
}
