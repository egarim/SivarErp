using System;
using System.ComponentModel;

namespace Sivar.Erp.Documents
{

    /// <summary>
    /// Interface for document type entities
    /// </summary>
    public interface IDocumentType : INotifyPropertyChanged
    {
        /// <summary>
        /// Unique identifier for the document type
        /// </summary>
        Guid Oid { get; set; }

        /// <summary>
        /// Unique code for the document type (e.g., "INV", "PO")
        /// </summary>
        string Code { get; set; }

        /// <summary>
        /// Display name of the document type
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Whether this document type is currently active
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// The document operation
        /// </summary>
        DocumentOperation DocumentOperation { get; set; }
    }
}