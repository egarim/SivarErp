using System;
using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Represents data for resolving discrepancies in inventory
    /// </summary>
    public class DiscrepancyResolutionData : IEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// The item with discrepancy
        /// </summary>
        [Description("The item with discrepancy")]
        public Guid ItemId { get; set; }

        /// <summary>
        /// Location of the discrepancy
        /// </summary>
        [Description("Location of the discrepancy")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Expected quantity
        /// </summary>
        [Description("Expected quantity")]
        public decimal ExpectedQuantity { get; set; }

        /// <summary>
        /// Actual quantity found
        /// </summary>
        [Description("Actual quantity found")]
        public decimal ActualQuantity { get; set; }

        /// <summary>
        /// Variance amount
        /// </summary>
        [Description("Variance amount")]
        public decimal Variance { get; set; }

        /// <summary>
        /// Resolution action taken
        /// </summary>
        [Description("Resolution action taken")]
        public string ResolutionAction { get; set; } = string.Empty;

        /// <summary>
        /// Reason for the discrepancy
        /// </summary>
        [Description("Reason for the discrepancy")]
        public string? Reason { get; set; }

        /// <summary>
        /// Person who resolved the discrepancy
        /// </summary>
        [Description("Person who resolved the discrepancy")]
        public string? ResolvedBy { get; set; }

        /// <summary>
        /// Date when resolved
        /// </summary>
        [Description("Date when resolved")]
        public DateTime? ResolvedDate { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        [Description("Additional notes")]
        public string? Notes { get; set; }
    }
}
