using System;
using System.ComponentModel;
using System.Linq;

namespace Sivar.Erp.Documents
{
    public interface IDocumentLine : INotifyPropertyChanged
    {
        double LineNumber { get; set; }
        string Description { get; set; }
        IItem Item { get; set; }
        IList<ITotal> LineTotals { get; set; }
        decimal Amount { get; set; }
    }
}
