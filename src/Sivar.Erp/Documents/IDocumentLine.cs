using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using Sivar.Erp.Services.Taxes;


namespace Sivar.Erp.Documents
{
    public interface IDocumentLine : INotifyPropertyChanged
    {
        double LineNumber { get; set; }
        string Description { get; set; }
        IItem Item { get; set; }
        IList<ITotal> LineTotals { get; set; }
        decimal Amount { get; set; }
        
        /// <summary>
        /// Gets or sets the quantity of the line item
        /// </summary>
        decimal Quantity { get; set; }
        
        /// <summary>
        /// Gets or sets the unit price of the line item
        /// </summary>
        decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of taxes that apply to this line
        /// </summary>
        IList<TaxDto> Taxes { get; set; }
    }
}
