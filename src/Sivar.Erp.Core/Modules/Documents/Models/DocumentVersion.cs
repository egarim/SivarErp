using System;
using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a document version
    /// </summary>
    public class DocumentVersion : IEntity
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
        /// Document ID this version belongs to
        /// </summary>
        [Description("Document ID this version belongs to")]
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Version number
        /// </summary>
        [Description("Version number")]
        public int VersionNumber { get; set; }

        /// <summary>
        /// Serialized document data
        /// </summary>
        [Description("Serialized document data")]
        public string DocumentData { get; set; } = string.Empty;

        /// <summary>
        /// Comment for this version
        /// </summary>
        [Description("Comment for this version")]
        public string? VersionComment { get; set; }

        /// <summary>
        /// User who created this version
        /// </summary>
        [Description("User who created this version")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Hash of the document data for integrity verification
        /// </summary>
        [Description("Hash of the document data for integrity verification")]
        public string DataHash { get; set; } = string.Empty;

        /// <summary>
        /// Size of the version data in bytes
        /// </summary>
        [Description("Size of the version data in bytes")]
        public long DataSize { get; set; }
    }
}
