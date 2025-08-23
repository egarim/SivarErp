using System;
using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a file attachment to a document
    /// </summary>
    public class DocumentAttachment : IEntity
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
        /// Document ID this attachment belongs to
        /// </summary>
        [Description("Document ID this attachment belongs to")]
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Original filename of the attachment
        /// </summary>
        [Description("Original filename of the attachment")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// MIME content type of the file
        /// </summary>
        [Description("MIME content type of the file")]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        [Description("Size of the file in bytes")]
        public long FileSize { get; set; }

        /// <summary>
        /// File content as byte array
        /// </summary>
        [Description("File content as byte array")]
        public byte[] FileContent { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// User who attached the file
        /// </summary>
        [Description("User who attached the file")]
        public string AttachedBy { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the file was attached
        /// </summary>
        [Description("Date and time when the file was attached")]
        public DateTime AttachedAt { get; set; }

        /// <summary>
        /// Description or notes about the attachment
        /// </summary>
        [Description("Description or notes about the attachment")]
        public string? Description { get; set; }

        /// <summary>
        /// Whether the attachment is still active
        /// </summary>
        [Description("Whether the attachment is still active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Hash of the file content for integrity verification
        /// </summary>
        [Description("Hash of the file content for integrity verification")]
        public string? FileHash { get; set; }
    }
}
