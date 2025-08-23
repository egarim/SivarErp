using System;
using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Represents data for cycle counting operations
    /// </summary>
    public class CycleCountData : IEntity
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
        /// The item being counted
        /// </summary>
        [Description("The item being counted")]
        public Guid ItemId { get; set; }

        /// <summary>
        /// Location of the item
        /// </summary>
        [Description("Location of the item")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Expected quantity in system
        /// </summary>
        [Description("Expected quantity in system")]
        public decimal ExpectedQuantity { get; set; }

        /// <summary>
        /// Actual counted quantity
        /// </summary>
        [Description("Actual counted quantity")]
        public decimal? ActualQuantity { get; set; }

        /// <summary>
        /// Variance between expected and actual
        /// </summary>
        [Description("Variance between expected and actual")]
        public decimal? Variance { get; set; }

        /// <summary>
        /// Person who performed the count
        /// </summary>
        [Description("Person who performed the count")]
        public string? CountedBy { get; set; }

        /// <summary>
        /// Date when the count was performed
        /// </summary>
        [Description("Date when the count was performed")]
        public DateTime? CountDate { get; set; }

        /// <summary>
        /// Status of the count
        /// </summary>
        [Description("Status of the count")]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Notes about the count
        /// </summary>
        [Description("Notes about the count")]
        public string? Notes { get; set; }
    }
}
