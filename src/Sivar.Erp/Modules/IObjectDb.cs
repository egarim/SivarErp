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
using System.Linq;

namespace Sivar.Erp.Services
{
    public interface IObjectDb
    {
        IList<IFiscalPeriod> fiscalPeriods { get; set; }
        IList<IAccount> Accounts { get; set; }
        IList<IBusinessEntity> BusinessEntities { get; set; }
        IList<IDocumentType> DocumentTypes { get; set; }
        IList<IItem> Items { get; set; }
        IList<ITax> Taxes { get; set; }
        IList<ITaxGroup> TaxGroups { get; set; }
        IList<GroupMembershipDto> GroupMemberships { get; set; }
        IList<ActivityRecord> ActivityRecords { get; set; }
        IList<SequenceDto> Sequences { get; set; }

        /// <summary>
        /// Collection of all transactions in the system
        /// </summary>
        IList<ITransaction> Transactions { get; set; }

        /// <summary>
        /// Collection of all ledger entries in the system
        /// </summary>
        IList<ILedgerEntry> LedgerEntries { get; set; }

        /// <summary>
        /// Collection of all transaction batches in the system
        /// </summary>
        IList<ITransactionBatch> TransactionBatches { get; set; }
    }
}