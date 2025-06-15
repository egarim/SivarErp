using Sivar.Erp.Accounting.ChartOfAccounts;
using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using Sivar.Erp.Taxes;
using Sivar.Erp.Taxes.TaxGroup;
using System;
using System.Linq;

namespace Sivar.Erp.Services
{
    public interface IObjectDb
    {
        IList<IAccount> Accounts { get; set; }
        IList<IBusinessEntity> BusinessEntities { get; set; }
        IList<IDocumentType> DocumentTypes { get; set; }
        IList<IItem> Items { get; set; }
        IList<ITax> Taxes { get; set; }
        IList<ITaxGroup> TaxGroups { get; set; }
    }
}
