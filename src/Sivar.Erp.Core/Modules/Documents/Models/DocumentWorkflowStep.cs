using System;
using System.ComponentModel;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a step in the document workflow process
    /// </summary>
    public class DocumentWorkflowStep : IEntity
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
        /// Document ID this workflow step belongs to
        /// </summary>
        [Description("Document ID this workflow step belongs to")]
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Name of the workflow step
        /// </summary>
        [Description("Name of the workflow step")]
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// Status before this workflow step
        /// </summary>
        [Description("Status before this workflow step")]
        public DocumentStatus FromStatus { get; set; }

        /// <summary>
        /// Status after this workflow step
        /// </summary>
        [Description("Status after this workflow step")]
        public DocumentStatus ToStatus { get; set; }

        /// <summary>
        /// User who performed this workflow step
        /// </summary>
        [Description("User who performed this workflow step")]
        public string PerformedBy { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when this step was performed
        /// </summary>
        [Description("Date and time when this step was performed")]
        public DateTime PerformedAt { get; set; }

        /// <summary>
        /// Comments or notes for this workflow step
        /// </summary>
        [Description("Comments or notes for this workflow step")]
        public string? Comments { get; set; }

        /// <summary>
        /// Whether this was a system-initiated action
        /// </summary>
        [Description("Whether this was a system-initiated action")]
        public bool IsSystemAction { get; set; }

        /// <summary>
        /// Workflow step sequence number
        /// </summary>
        [Description("Workflow step sequence number")]
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Additional metadata as JSON
        /// </summary>
        [Description("Additional metadata as JSON")]
        public string? Metadata { get; set; }
    }
}
