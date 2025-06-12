using System;
using System.Linq;

namespace Sivar.Erp.Documents
{
    public interface ITotal
    {
        string Concept { get; set; }
        decimal Total { get; set; }



    }
}
