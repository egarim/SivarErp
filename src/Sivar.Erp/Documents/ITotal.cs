using System;
using System.Linq;

namespace Sivar.Erp.Documents
{
    public interface ITotal
    {
        System.Guid Oid { get; set; }
        string Concept { get; set; }
        decimal Total { get; set; }



    }
}
