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

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Main data access interface for the ERP system
    /// (.NET 9 modernized with Core contracts)
    /// </summary>
    public interface IObjectDb
    {
        // Infrastructure layer types
        IList<PerformanceLog> PerformanceLogs { get; set; }
        IList<ActivityRecord> ActivityRecords { get; set; }
        IList<SequenceDto> Sequences { get; set; }

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
        IList<InventoryLayerDto> InventoryLayers { get; set; }

        // Payment System
        IList<PaymentMethodDto> PaymentMethods { get; set; }
        IList<PaymentDto> Payments { get; set; }

        // Security System
        IList<User> Users { get; set; }
        IList<Role> Roles { get; set; }
        IList<SecurityEvent> SecurityEvents { get; set; }
    }
}