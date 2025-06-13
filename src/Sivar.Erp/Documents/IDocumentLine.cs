using System;
using System.ComponentModel;
using System.Linq;

namespace Sivar.Erp.Documents
{
    public interface IDocumentLine : INotifyPropertyChanged
    {
        IItem Item { get; set; }
        IList<ITotal> LineTotals { get; set; }
        decimal Amount { get; set; }
    }
}
