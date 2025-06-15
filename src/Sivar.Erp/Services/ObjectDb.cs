using Sivar.Erp.Accounting.ChartOfAccounts;
using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using Sivar.Erp.Taxes;
using Sivar.Erp.Taxes.TaxGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Services
{
    public class ObjectDb : IObjectDb
    {
        public IList<IAccount> Accounts { get; set; } = new List<IAccount>();
        public IList<IBusinessEntity> BusinessEntities { get; set; } = new List<IBusinessEntity>();
        public IList<IDocumentType> DocumentTypes { get; set; } = new List<IDocumentType>();
        public IList<ITaxGroup> TaxGroups { get; set; } = new List<ITaxGroup>();
        public IList<ITax> Taxes { get; set; } = new List<ITax>();
        public IList<IItem> Items { get; set; } = new List<IItem>();

        public ObjectDb()
        {
        }
    }
}
