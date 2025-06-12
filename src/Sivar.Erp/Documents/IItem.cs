using System;
using System.Linq;

namespace Sivar.Erp.Documents
{
    public interface IItem
    {
        string Code { get; set; }
        string Type { get; set; }
        string Description { get; set; }
        decimal Price { get; set; }
    }
}
