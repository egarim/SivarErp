using System;
using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents file data for an attachment download
    /// </summary>
    public class AttachmentFileData
    {
        /// <summary>
        /// Original filename
        /// </summary>
        [Description("Original filename")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// MIME content type
        /// </summary>
        [Description("MIME content type")]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// File content as byte array
        /// </summary>
        [Description("File content as byte array")]
        public byte[] Content { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        [Description("Size of the file in bytes")]
        public long Size { get; set; }

        /// <summary>
        /// Date when the file was last modified
        /// </summary>
        [Description("Date when the file was last modified")]
        public DateTime LastModified { get; set; }
    }
}
