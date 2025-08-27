using System;
using System.Collections.Generic;

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Interface representing a document in the ERP system
    /// (.NET 9 modernized with Core contracts)
    /// </summary>
    public interface IDocument
    {
        /// <summary>
        /// Gets or sets the document number
        /// </summary>
        string DocumentNumber { get; set; }

        /// <summary>
        /// Gets or sets the document date
        /// </summary>
        DateOnly Date { get; set; }

        /// <summary>
        /// Gets or sets the business entity (customer, supplier, etc.)
        /// </summary>
        IBusinessEntity BusinessEntity { get; set; }

        /// <summary>
        /// Gets or sets the document type
        /// </summary>
        IDocumentType DocumentType { get; set; }

        /// <summary>
        /// Gets or sets the document lines
        /// </summary>
        IList<IDocumentLine> Lines { get; set; }

        /// <summary>
        /// Gets or sets the document totals
        /// </summary>
        IList<ITotal> DocumentTotals { get; set; }

        /// <summary>
        /// Gets or sets when the document was created
        /// </summary>
        DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets who created the document
        /// </summary>
        string CreatedBy { get; set; }
    }
}