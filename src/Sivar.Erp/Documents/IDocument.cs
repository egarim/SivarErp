using System;
using System.ComponentModel;
using System.Collections.Generic;
using Sivar.Erp.BusinessEntities;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Interface for document entities
    /// </summary>
    public interface IDocument : INotifyPropertyChanged
    {

        [BusinessKey()]
        string DocumentNumber { get; set; }
        /// <summary>
        /// Unique identifier for the document
        /// </summary>
        Guid Oid { get; set; }
        
        /// <summary>
        /// Date of the document
        /// </summary>
        DateOnly Date { get; set; }
        
        /// <summary>
        /// Time of the document
        /// </summary>
        TimeOnly Time { get; set; }
        
        /// <summary>
        /// Business entity associated with this document
        /// </summary>
        IBusinessEntity BusinessEntity { get; set; }
        
        /// <summary>
        /// Document type information
        /// </summary>
        IDocumentType DocumentType { get; set; }
        
        /// <summary>
        /// Lines contained in this document
        /// </summary>
        IList<IDocumentLine> Lines { get; set; }
        
        /// <summary>
        /// Totals for this document
        /// </summary>
        IList<ITotal> DocumentTotals { get; set; }


    }
}
