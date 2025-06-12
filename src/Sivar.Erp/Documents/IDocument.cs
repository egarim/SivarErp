using System;
using System.ComponentModel;
using System.Linq;

namespace Sivar.Erp.Documents
{
    public interface IDocument : INotifyPropertyChanged
    {
        System.Guid Oid { get; set; }
        DateOnly Date { get; set; }
        TimeOnly Time { get; set; }
        IBusinessEntity BusinessEntity { get; set; }
        IList<IDocumentLine> Lines { get; set; }
        IList<ITotal> DocumentTotals { get; set; }
    }
}
