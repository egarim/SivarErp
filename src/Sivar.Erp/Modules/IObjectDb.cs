using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Services.Taxes;
using Sivar.Erp.Services.Taxes.TaxGroup;
using Sivar.Erp.Services.Taxes.TaxRule;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sivar.Erp.Services
{
    public interface IObjectDb
    {
        IList<PerformanceLog> PerformanceLogs { get; set; }
        IList<IFiscalPeriod> fiscalPeriods { get; set; }
        IList<IAccount> Accounts { get; set; }
        IList<IBusinessEntity> BusinessEntities { get; set; }
        IList<IDocumentType> DocumentTypes { get; set; }
        IList<IItem> Items { get; set; }
        IList<ITax> Taxes { get; set; }
        IList<ITaxGroup> TaxGroups { get; set; }

        IList<ITaxRule> TaxRules { get; set; }
        IList<GroupMembershipDto> GroupMemberships { get; set; }
        IList<ActivityRecord> ActivityRecords { get; set; }
        IList<SequenceDto> Sequences { get; set; }

        /// <summary>
        /// Collection of document accounting profiles used for transaction generation
        /// </summary>
        IList<IDocumentAccountingProfile> DocumentAccountingProfiles { get; set; }

        /// <summary>
        /// Collection of all transactions in the system
        /// </summary>
        IList<ITransaction> Transactions { get; set; }

        /// <summary>
        /// Collection of all ledger entries in the system
        /// </summary>
        IList<ILedgerEntry> LedgerEntries { get; set; }        /// <summary>
                                                               /// Collection of all transaction batches in the system
                                                               /// </summary>
        IList<ITransactionBatch> TransactionBatches { get; set; }

        /// <summary>
        /// Collection of users in the system
        /// </summary>
        IList<User> Users { get; set; }

        /// <summary>
        /// Collection of roles in the system
        /// </summary>
        IList<Role> Roles { get; set; }

        /// <summary>
        /// Collection of security events and audit logs
        /// </summary>
        IList<SecurityEvent> SecurityEvents { get; set; }
    }
}