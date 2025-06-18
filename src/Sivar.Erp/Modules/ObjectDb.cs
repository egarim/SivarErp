using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Services.Taxes;
using Sivar.Erp.Services.Taxes.TaxGroup;
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
        public IList<GroupMembershipDto> GroupMemberships { get; set; } = new List<GroupMembershipDto>();
        public IList<ActivityRecord> ActivityRecords { get; set; } = new List<ActivityRecord>();
        public IList<SequenceDto> Sequences { get; set; } = new List<SequenceDto>();
        public IList<IFiscalPeriod> fiscalPeriods { get; set; } = new List<IFiscalPeriod>();

        /// <summary>
        /// Collection of all transactions in the system
        /// </summary>
        public IList<ITransaction> Transactions { get; set; } = new List<ITransaction>();

        /// <summary>
        /// Collection of all ledger entries in the system
        /// </summary>
        public IList<ILedgerEntry> LedgerEntries { get; set; } = new List<ILedgerEntry>();

        /// <summary>
        /// Collection of all transaction batches in the system
        /// </summary>
        public IList<ITransactionBatch> TransactionBatches { get; set; } = new List<ITransactionBatch>();

        public ObjectDb()
        {
        }

       
    }
}