using System;
using System.Linq;

namespace Sivar.Erp.Documents
{
    public interface IItem
    {
        System.Guid Oid { get; set; }
        string Code { get; set; }
        string Type { get; set; }
        string Description { get; set; }
        decimal BasePrice { get; set; }
    }
}
