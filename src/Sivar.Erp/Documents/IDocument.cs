using System;
using System.ComponentModel;
using System.Linq;

namespace Sivar.Erp.Documents
{
    internal interface IDocument : INotifyPropertyChanged
    {
        DateOnly Date { get; set; }
        TimeOnly Time { get; set; }
        IBusinessEntity BusinessEntity { get; set; }
        IList<IDocumentLine> Lines { get; set; }
        IList<ITotal> DocumentTotals { get; set; }
    }
}
