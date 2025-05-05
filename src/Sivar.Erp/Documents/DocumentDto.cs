namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Implementation of the document entity
    /// </summary>
    public class DocumentDto : IDocument
    {
        /// <summary>
        /// Unique identifier for the document
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Date of the document
        /// </summary>
        public DateOnly DocumentDate { get; set; }

        /// <summary>
        /// Document number or reference
        /// </summary>
        public string DocumentNo { get; set; } = string.Empty;

        /// <summary>
        /// Short description of the document
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Comments that appear on the document itself
        /// </summary>
        public string DocumentComments { get; set; } = string.Empty;

        /// <summary>
        /// Comments for internal use only
        /// </summary>
        public string InternalComments { get; set; } = string.Empty;

        /// <summary>
        /// Type of document
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// ID of extended document type (if from extension)
        /// </summary>
        public Guid? ExtendedDocumentTypeId { get; set; }

        /// <summary>
        /// External identifier for the document (if from external system)
        /// </summary>
        public string ExternalId { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the document was created
        /// </summary>
        public DateTime InsertedAt { get; set; }

        /// <summary>
        /// User who created the document
        /// </summary>
        public string InsertedBy { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the document was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who last updated the document
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;
    }
}