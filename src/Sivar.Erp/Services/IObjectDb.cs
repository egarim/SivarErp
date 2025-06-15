using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.Taxes;
using Sivar.Erp.Services.Taxes.TaxGroup;
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
        IList<ActivityRecord> ActivityRecords { get; set; }
        IList<SequenceDto> Sequences { get; set; }
    }
}
