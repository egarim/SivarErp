using System;
using System.Linq;

namespace Sivar.Erp.Modules.Inventory
{
    public interface IInventoryLayerDto
    {
        string CreatedBy { get; set; }
        DateTime CreatedDate { get; set; }
        string ItemCode { get; set; }
        Guid Oid { get; set; }
        decimal Quantity { get; set; }
        decimal RemainingQuantity { get; set; }
        string TransactionId { get; set; }
        decimal UnitCost { get; set; }
        string WarehouseCode { get; set; }
    }
}
