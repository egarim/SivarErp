using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Document entity implementation for the Core project
    /// </summary>
    [Description("Document entity implementation for the Core project")]
    public class Document : IDocument
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
        /// Document number for identification
        /// </summary>
        [Description("Document number for identification")]
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// Date of the document
        /// </summary>
        [Description("Date of the document")]
        public DateOnly Date { get; set; }

        /// <summary>
        /// Type of document
        /// </summary>
        [Description("Type of document")]
        public IDocumentType DocumentType { get; set; } = null!;

        /// <summary>
        /// Business entity associated with this document
        /// </summary>
        [Description("Business entity associated with this document")]
        public IBusinessEntity BusinessEntity { get; set; } = null!;

        /// <summary>
        /// Collection of document totals for accounting
        /// </summary>
        [Description("Collection of document totals for accounting")]
        public ICollection<IDocumentTotal> DocumentTotals { get; set; } = new List<IDocumentTotal>();

        /// <summary>
        /// Total amount of the document
        /// </summary>
        [Description("Total amount of the document")]
        public decimal TotalAmount => DocumentTotals?.Sum(dt => dt.Total) ?? 0;

        /// <summary>
        /// Status of the document
        /// </summary>
        [Description("Status of the document")]
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

        /// <summary>
        /// Notes or comments for the document
        /// </summary>
        [Description("Notes or comments for the document")]
        public string? Notes { get; set; }

        /// <summary>
        /// User who created the document
        /// </summary>
        [Description("User who created the document")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// User who last modified the document
        /// </summary>
        [Description("User who last modified the document")]
        public string? ModifiedBy { get; set; }
    }
}
