using System;
using System.ComponentModel;

namespace Sivar.Erp.Modules.Documents.Core.Entities
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
        /// Category or operation of the document type (e.g., "SalesInvoice", "PurchaseOrder")
        /// </summary>
        string DocumentOperation { get; set; }
    }
}
