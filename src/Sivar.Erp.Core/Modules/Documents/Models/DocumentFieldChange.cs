using System;
using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a field change in a document for version comparison
    /// </summary>
    public class DocumentFieldChange : IEntity
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
        /// Name of the field that changed
        /// </summary>
        [Description("Name of the field that changed")]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the field for user-friendly presentation
        /// </summary>
        [Description("Display name of the field for user-friendly presentation")]
        public string FieldDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Old value of the field
        /// </summary>
        [Description("Old value of the field")]
        public string? OldValue { get; set; }

        /// <summary>
        /// New value of the field
        /// </summary>
        [Description("New value of the field")]
        public string? NewValue { get; set; }

        /// <summary>
        /// Type of change (Added, Modified, Removed)
        /// </summary>
        [Description("Type of change (Added, Modified, Removed)")]
        public string ChangeType { get; set; } = string.Empty;

        /// <summary>
        /// Path to the field for nested objects
        /// </summary>
        [Description("Path to the field for nested objects")]
        public string? FieldPath { get; set; }

        /// <summary>
        /// Data type of the field
        /// </summary>
        [Description("Data type of the field")]
        public string? DataType { get; set; }

        /// <summary>
        /// Whether this change is significant for auditing
        /// </summary>
        [Description("Whether this change is significant for auditing")]
        public bool IsSignificant { get; set; } = true;
    }
}
