using System;
using System.ComponentModel;
using System.Collections.Generic;
using Sivar.Erp.Modules.Inventory.Core.Interfaces;

namespace Sivar.Erp.Modules.Documents.Core.Interfaces
{
    /// <summary>
    /// Interface for document line entities
    /// </summary>
    public interface IDocumentLine : INotifyPropertyChanged
    {
        /// <summary>
        /// Line number within the document
        /// </summary>
        double LineNumber { get; set; }

        /// <summary>
        /// Description of the line item
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// The item associated with this line
        /// </summary>
        IItem Item { get; set; }

        /// <summary>
        /// Collection of totals for this line
        /// </summary>
        IList<ITotal> LineTotals { get; set; }

        /// <summary>
        /// Total amount for this line
        /// </summary>
        decimal Amount { get; set; }
        
        /// <summary>
        /// Gets or sets the quantity of the line item
        /// </summary>
        decimal Quantity { get; set; }
        
        /// <summary>
        /// Gets or sets the unit price of the line item
        /// </summary>
        decimal UnitPrice { get; set; }
    }
}
