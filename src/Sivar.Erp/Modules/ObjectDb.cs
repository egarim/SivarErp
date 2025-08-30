using Sivar.Erp.Modules.BusinessEntities.Application.DTOs;

using Sivar.Erp.Modules.Inventory.Core.Interfaces;
using Sivar.Erp.Modules.Documents.Core.Interfaces;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.Modules.Accounting.ChartOfAccounts;
using Sivar.Erp.Modules.Accounting.FiscalPeriods;
using Sivar.Erp.Modules.Accounting.Transactions;
using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.Modules.Payments.Models;
using Sivar.Erp.Modules.Taxes;
using Sivar.Erp.Modules.Taxes.TaxGroup;
using Sivar.Erp.Modules.Taxes.TaxRule;
using Sivar.Erp.Modules.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sivar.Erp.Modules.BusinessEntities.Core.Interfaces;
using Sivar.Erp.Infrastructure.Diagnostics;


namespace Sivar.Erp.Modules
{
    public class ObjectDb : IObjectDb
    {
        public IList<IAccount> Accounts { get; set; } = new List<IAccount>();
        public IList<IBusinessEntity> BusinessEntities { get; set; } = new List<IBusinessEntity>();
        public IList<IDocumentType> DocumentTypes { get; set; } = new List<IDocumentType>();
        public IList<ITaxGroup> TaxGroups { get; set; } = new List<ITaxGroup>();
        public IList<ITax> Taxes { get; set; } = new List<ITax>();
        public IList<IItem> Items { get; set; } = new List<IItem>();
        public IList<IGroupMembership> GroupMemberships { get; set; } = new List<IGroupMembership>();
   
        public IList<ISequence> Sequences { get; set; } = new List<ISequence>();
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

        public IList<ITaxRule> TaxRules { get; set; } = new List<ITaxRule>();
        public IList<PerformanceLogDto> PerformanceLogs { get; set; } = new List<PerformanceLogDto>();
        public IList<IDocumentAccountingProfile> DocumentAccountingProfiles { get; set; } = new List<IDocumentAccountingProfile>();

        /// <summary>
        /// Collection of payment methods in the system
        /// </summary>
        public IList<PaymentMethodDto> PaymentMethods { get; set; } = new List<PaymentMethodDto>();

        /// <summary>
        /// Collection of payments in the system
        /// </summary>
        public IList<PaymentDto> Payments { get; set; } = new List<PaymentDto>();

        /// <summary>
        /// Collection of users in the system
        /// </summary>
        public IList<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Collection of roles in the system
        /// </summary>
        public IList<Role> Roles { get; set; } = new List<Role>();

        /// <summary>
        /// Collection of security events and audit logs
        /// </summary>
        public IList<SecurityEvent> SecurityEvents { get; set; } = new List<SecurityEvent>();

        /// <summary>
        /// Collection of inventory items in the system
        /// </summary>
        public IList<IInventoryItem> InventoryItems { get; set; } = new List<IInventoryItem>();

        /// <summary>
        /// Collection of stock levels for inventory items
        /// </summary>
        public IList<IStockLevel> StockLevels { get; set; } = new List<IStockLevel>();

        /// <summary>
        /// Collection of inventory transactions (stock movements)
        /// </summary>
        public IList<IInventoryTransaction> InventoryTransactions { get; set; } = new List<IInventoryTransaction>();

        /// <summary>
        /// Collection of inventory reservations
        /// </summary>
        public IList<IInventoryReservation> InventoryReservations { get; set; } = new List<IInventoryReservation>();

        /// <summary>
        /// Collection of inventory layers/batches for FIFO and LIFO costing
        /// </summary>
        public IList<InventoryLayerDto> InventoryLayers { get; set; } = new List<InventoryLayerDto>();
   

        public ObjectDb()
        {
        }
    }
}
