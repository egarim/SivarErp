using System;
using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Represents transfer request data
    /// </summary>
    public class TransferRequestData : IEntity
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
        /// Item to transfer
        /// </summary>
        [Description("Item to transfer")]
        public Guid ItemId { get; set; }

        /// <summary>
        /// Source location
        /// </summary>
        [Description("Source location")]
        public string FromLocation { get; set; } = string.Empty;

        /// <summary>
        /// Destination location
        /// </summary>
        [Description("Destination location")]
        public string ToLocation { get; set; } = string.Empty;

        /// <summary>
        /// Quantity to transfer
        /// </summary>
        [Description("Quantity to transfer")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Reason for transfer
        /// </summary>
        [Description("Reason for transfer")]
        public string? Reason { get; set; }

        /// <summary>
        /// Requested by user
        /// </summary>
        [Description("Requested by user")]
        public string? RequestedBy { get; set; }

        /// <summary>
        /// Request date
        /// </summary>
        [Description("Request date")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Approved by user
        /// </summary>
        [Description("Approved by user")]
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Approval date
        /// </summary>
        [Description("Approval date")]
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// Priority level
        /// </summary>
        [Description("Priority level")]
        public string Priority { get; set; } = "Normal";

        /// <summary>
        /// Expected transfer date
        /// </summary>
        [Description("Expected transfer date")]
        public DateTime? ExpectedDate { get; set; }

        /// <summary>
        /// Transfer status
        /// </summary>
        [Description("Transfer status")]
        public TransferStatus Status { get; set; } = TransferStatus.Pending;

        /// <summary>
        /// Additional notes
        /// </summary>
        [Description("Additional notes")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Status of inventory transfers
    /// </summary>
    public enum TransferStatus
    {
        /// <summary>
        /// Pending approval
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Approved and ready for execution
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Transfer in progress
        /// </summary>
        InProgress = 3,

        /// <summary>
        /// Transfer completed
        /// </summary>
        Completed = 4,

        /// <summary>
        /// Transfer cancelled
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// Transfer rejected
        /// </summary>
        Rejected = 6
    }
}
