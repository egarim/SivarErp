using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Interface for document entities
    /// </summary>
    public interface IDocument : IEntity, IAuditable
    {
        /// <summary>
        /// Date of the document
        /// </summary>
        DateOnly DocumentDate { get; set; }

        /// <summary>
        /// Document number or reference
        /// </summary>
        string DocumentNo { get; set; }

        /// <summary>
        /// Short description of the document
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Comments that appear on the document itself
        /// </summary>
        string DocumentComments { get; set; }

        /// <summary>
        /// Comments for internal use only
        /// </summary>
        string InternalComments { get; set; }

        /// <summary>
        /// Type of document
        /// </summary>
        DocumentType DocumentType { get; set; }

        /// <summary>
        /// ID of extended document type (if from extension)
        /// </summary>
        Guid? ExtendedDocumentTypeId { get; set; }

        /// <summary>
        /// External identifier for the document (if from external system)
        /// </summary>
        string ExternalId { get; set; }
    }
}
