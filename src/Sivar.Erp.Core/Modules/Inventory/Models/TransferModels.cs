using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Inventory.Models
{
    /// <summary>
    /// Data for executing inventory transfers
    /// </summary>
    [Description("Data for executing inventory transfers")]
    public class TransferExecutionData : IEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Description("Entity identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        [Description("Last update date")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the transfer request ID
        /// </summary>
        [Description("Transfer request ID")]
        public Guid TransferRequestId { get; set; }

        /// <summary>
        /// Gets or sets the execution date
        /// </summary>
        [Description("Execution date")]
        public DateTime ExecutionDate { get; set; }

        /// <summary>
        /// Gets or sets the executor user ID
        /// </summary>
        [Description("Executor user ID")]
        public string ExecutorUserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution notes
        /// </summary>
        [Description("Execution notes")]
        public string? ExecutionNotes { get; set; }

        /// <summary>
        /// Gets or sets the execution method
        /// </summary>
        [Description("Execution method")]
        public TransferExecutionMethod ExecutionMethod { get; set; }

        /// <summary>
        /// Gets or sets the batch number
        /// </summary>
        [Description("Batch number")]
        public string? BatchNumber { get; set; }

        /// <summary>
        /// Gets or sets the shipping reference
        /// </summary>
        [Description("Shipping reference")]
        public string? ShippingReference { get; set; }

        /// <summary>
        /// Gets or sets whether to force execution
        /// </summary>
        [Description("Whether to force execution")]
        public bool ForceExecution { get; set; }

        /// <summary>
        /// Gets or sets the items to transfer with specific quantities
        /// </summary>
        [Description("Items to transfer with specific quantities")]
        public List<TransferExecutionItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Query parameters for transfer history
    /// </summary>
    [Description("Query parameters for transfer history")]
    public class TransferHistoryQuery : IEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Description("Entity identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        [Description("Last update date")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the source location ID
        /// </summary>
        [Description("Source location ID")]
        public Guid? SourceLocationId { get; set; }

        /// <summary>
        /// Gets or sets the destination location ID
        /// </summary>
        [Description("Destination location ID")]
        public Guid? DestinationLocationId { get; set; }

        /// <summary>
        /// Gets or sets the item ID
        /// </summary>
        [Description("Item ID")]
        public Guid? ItemId { get; set; }

        /// <summary>
        /// Gets or sets the start date
        /// </summary>
        [Description("Start date")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date
        /// </summary>
        [Description("End date")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the transfer status
        /// </summary>
        [Description("Transfer status")]
        public TransferStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        [Description("User ID")]
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the page number
        /// </summary>
        [Description("Page number")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        [Description("Page size")]
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Query parameters for pending transfers
    /// </summary>
    [Description("Query parameters for pending transfers")]
    public class PendingTransfersQuery : IEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Description("Entity identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        [Description("Last update date")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the location ID
        /// </summary>
        [Description("Location ID")]
        public Guid? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        [Description("User ID")]
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the priority
        /// </summary>
        [Description("Priority")]
        public TransferPriority? Priority { get; set; }

        /// <summary>
        /// Gets or sets the due date
        /// </summary>
        [Description("Due date")]
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets whether to include overdue only
        /// </summary>
        [Description("Whether to include overdue only")]
        public bool OverdueOnly { get; set; }

        /// <summary>
        /// Gets or sets the page number
        /// </summary>
        [Description("Page number")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        [Description("Page size")]
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Request for bulk transfer operations
    /// </summary>
    [Description("Request for bulk transfer operations")]
    public class BulkTransferRequest : IEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Description("Entity identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        [Description("Last update date")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the source location ID
        /// </summary>
        [Description("Source location ID")]
        public Guid SourceLocationId { get; set; }

        /// <summary>
        /// Gets or sets the destination location ID
        /// </summary>
        [Description("Destination location ID")]
        public Guid DestinationLocationId { get; set; }

        /// <summary>
        /// Gets or sets the transfer items
        /// </summary>
        [Description("Transfer items")]
        public List<BulkTransferItem> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the requested by user ID
        /// </summary>
        [Description("Requested by user ID")]
        public string RequestedBy { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reason
        /// </summary>
        [Description("Reason")]
        public string? Reason { get; set; }

        /// <summary>
        /// Gets or sets the priority
        /// </summary>
        [Description("Priority")]
        public TransferPriority Priority { get; set; } = TransferPriority.Normal;

        /// <summary>
        /// Gets or sets the due date
        /// </summary>
        [Description("Due date")]
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets whether to auto-approve
        /// </summary>
        [Description("Whether to auto-approve")]
        public bool AutoApprove { get; set; }
    }

    /// <summary>
    /// Individual item for transfer execution
    /// </summary>
    [Description("Individual item for transfer execution")]
    public class TransferExecutionItem
    {
        /// <summary>
        /// Gets or sets the item ID
        /// </summary>
        [Description("Item ID")]
        public Guid ItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity to transfer
        /// </summary>
        [Description("Quantity to transfer")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the lot number
        /// </summary>
        [Description("Lot number")]
        public string? LotNumber { get; set; }

        /// <summary>
        /// Gets or sets the serial number
        /// </summary>
        [Description("Serial number")]
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the expiration date
        /// </summary>
        [Description("Expiration date")]
        public DateTime? ExpirationDate { get; set; }
    }

    /// <summary>
    /// Individual item for bulk transfer
    /// </summary>
    [Description("Individual item for bulk transfer")]
    public class BulkTransferItem
    {
        /// <summary>
        /// Gets or sets the item ID
        /// </summary>
        [Description("Item ID")]
        public Guid ItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        [Description("Quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure
        /// </summary>
        [Description("Unit of measure")]
        public string UnitOfMeasure { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the notes
        /// </summary>
        [Description("Notes")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Transfer execution method
    /// </summary>
    [Description("Transfer execution method")]
    public enum TransferExecutionMethod
    {
        [Description("Manual")]
        Manual,
        [Description("Automatic")]
        Automatic,
        [Description("Batch")]
        Batch,
        [Description("Scheduled")]
        Scheduled
    }

    /// <summary>
    /// Transfer priority levels
    /// </summary>
    [Description("Transfer priority levels")]
    public enum TransferPriority
    {
        [Description("Low")]
        Low,
        [Description("Normal")]
        Normal,
        [Description("High")]
        High,
        [Description("Urgent")]
        Urgent,
        [Description("Critical")]
        Critical
    }
}
