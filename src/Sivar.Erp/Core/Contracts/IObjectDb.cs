using Sivar.Erp.Core.Enums;
using System;
using System.Collections.Generic;

// Infrastructure layer imports
using Sivar.Erp.Infrastructure.Diagnostics;
using Sivar.Erp.Infrastructure.ActivityStream;
using Sivar.Erp.Infrastructure.Sequencers;

// Legacy namespace imports (to be phased out)
using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.Modules.Payments.Models;
using Sivar.Erp.Modules.Inventory;
using Sivar.Erp.Modules.Accounting.Transactions;

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Main data access interface for the ERP system
    /// (.NET 9 modernized with Core contracts)
    /// </summary>
    public interface IObjectDb
    {
        // Infrastructure layer types
        IList<IPerformanceLog> PerformanceLogs { get; set; }
        IList<IActivityRecord> ActivityRecords { get; set; }
        IList<ISequenceDto> Sequences { get; set; }

        // Core.Contracts - Accounting
        IList<IFiscalPeriod> fiscalPeriods { get; set; }
        IList<IAccount> Accounts { get; set; }
        IList<ITransaction> Transactions { get; set; }
        IList<ILedgerEntry> LedgerEntries { get; set; }
        IList<ITransactionBatch> TransactionBatches { get; set; }
        IList<IDocumentAccountingProfile> DocumentAccountingProfiles { get; set; }

        // Core.Contracts - Business Entities & Documents
        IList<IBusinessEntity> BusinessEntities { get; set; }
        IList<IDocumentType> DocumentTypes { get; set; }
        IList<IItem> Items { get; set; }

        // Core.Contracts - Taxes
        IList<ITax> Taxes { get; set; }
        IList<ITaxGroup> TaxGroups { get; set; }
        IList<ITaxRule> TaxRules { get; set; }
        IList<IGroupMembership> GroupMemberships { get; set; }

        // Core.Contracts - Inventory (using explicit Core types)
        IList<IInventoryItem> InventoryItems { get; set; }
        IList<IStockLevel> StockLevels { get; set; }
        IList<IInventoryTransaction> InventoryTransactions { get; set; }
        IList<IInventoryReservation> InventoryReservations { get; set; }
        IList<IInventoryLayerDto> InventoryLayers { get; set; }

        // Payment System
        IList<IPaymentMethodDto> PaymentMethods { get; set; }
        IList<IPaymentDto> Payments { get; set; }

        // Security System
        IList<IUser> Users { get; set; }
        IList<IRole> Roles { get; set; }
        IList<ISecurityEvent> SecurityEvents { get; set; }
    }
}
