using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Rules for automatic adjustment of inventory discrepancies
    /// </summary>
    public class AutoAdjustmentRules
    {
        /// <summary>
        /// Maximum variance percentage for automatic adjustment
        /// </summary>
        [Description("Maximum variance percentage for automatic adjustment")]
        public decimal MaxVariancePercentage { get; set; } = 5m;

        /// <summary>
        /// Maximum absolute variance amount for automatic adjustment
        /// </summary>
        [Description("Maximum absolute variance amount for automatic adjustment")]
        public decimal MaxVarianceAmount { get; set; } = 100m;

        /// <summary>
        /// Enable automatic adjustments
        /// </summary>
        [Description("Enable automatic adjustments")]
        public bool EnableAutoAdjustments { get; set; } = false;

        /// <summary>
        /// Require approval for adjustments above threshold
        /// </summary>
        [Description("Require approval for adjustments above threshold")]
        public bool RequireApproval { get; set; } = true;

        /// <summary>
        /// Approval threshold amount
        /// </summary>
        [Description("Approval threshold amount")]
        public decimal ApprovalThreshold { get; set; } = 50m;

        /// <summary>
        /// Categories excluded from auto adjustment
        /// </summary>
        [Description("Categories excluded from auto adjustment")]
        public string[]? ExcludedCategories { get; set; }

        /// <summary>
        /// Locations excluded from auto adjustment
        /// </summary>
        [Description("Locations excluded from auto adjustment")]
        public string[]? ExcludedLocations { get; set; }

        /// <summary>
        /// Reason code for automatic adjustments
        /// </summary>
        [Description("Reason code for automatic adjustments")]
        public string DefaultReasonCode { get; set; } = "AUTO_ADJUSTMENT";

        /// <summary>
        /// User ID for automatic adjustments
        /// </summary>
        [Description("User ID for automatic adjustments")]
        public string SystemUserId { get; set; } = "SYSTEM";
    }
}
