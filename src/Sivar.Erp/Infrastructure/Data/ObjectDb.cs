using Sivar.Erp.Core.Contracts;
using System;
using System.Collections.Generic;

// Explicit Core namespace imports to avoid ambiguity
using CoreIStockLevel = Sivar.Erp.Core.Contracts.IStockLevel;
using CoreIInventoryTransaction = Sivar.Erp.Core.Contracts.IInventoryTransaction;
using CoreIInventoryReservation = Sivar.Erp.Core.Contracts.IInventoryReservation;

// Legacy imports that still need to be referenced until full migration
using Sivar.Erp.Infrastructure.Diagnostics;
using Sivar.Erp.Infrastructure.Sequencers;
using Sivar.Erp.Infrastructure.ActivityStream;
using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.Modules.Payments.Models;
using Sivar.Erp.Modules.Inventory;

namespace Sivar.Erp.Infrastructure.Data
{
    /// <summary>
    /// Infrastructure layer ObjectDb implementation (.NET 9 optimized)
    /// Uses Core.Contracts interfaces for clean architecture
    /// </summary>
    public class ObjectDb : IObjectDb
    {
        // Infrastructure.Diagnostics
        public IList<PerformanceLog> PerformanceLogs { get; set; } = new List<PerformanceLog>();
        public IList<ActivityRecord> ActivityRecords { get; set; } = new List<ActivityRecord>();
        public IList<SequenceDto> Sequences { get; set; } = new List<SequenceDto>();

        // Core.Contracts - Accounting
        public IList<IFiscalPeriod> fiscalPeriods { get; set; } = new List<IFiscalPeriod>();
        public IList<IAccount> Accounts { get; set; } = new List<IAccount>();
        public IList<ITransaction> Transactions { get; set; } = new List<ITransaction>();
        public IList<ILedgerEntry> LedgerEntries { get; set; } = new List<ILedgerEntry>();
        public IList<ITransactionBatch> TransactionBatches { get; set; } = new List<ITransactionBatch>();
        public IList<IDocumentAccountingProfile> DocumentAccountingProfiles { get; set; } = new List<IDocumentAccountingProfile>();

        // Core.Contracts - Business Entities & Documents
        public IList<IBusinessEntity> BusinessEntities { get; set; } = new List<IBusinessEntity>();
        public IList<IDocumentType> DocumentTypes { get; set; } = new List<IDocumentType>();
        public IList<IItem> Items { get; set; } = new List<IItem>();

        // Core.Contracts - Taxes
        public IList<ITax> Taxes { get; set; } = new List<ITax>();
        public IList<ITaxGroup> TaxGroups { get; set; } = new List<ITaxGroup>();
        public IList<ITaxRule> TaxRules { get; set; } = new List<ITaxRule>();
        public IList<IGroupMembership> GroupMemberships { get; set; } = new List<IGroupMembership>();

        // Core.Contracts - Inventory (explicit types to avoid ambiguity)
        public IList<IInventoryItem> InventoryItems { get; set; } = new List<IInventoryItem>();
        public IList<CoreIStockLevel> StockLevels { get; set; } = new List<CoreIStockLevel>();
        public IList<CoreIInventoryTransaction> InventoryTransactions { get; set; } = new List<CoreIInventoryTransaction>();
        public IList<CoreIInventoryReservation> InventoryReservations { get; set; } = new List<CoreIInventoryReservation>();
        public IList<InventoryLayerDto> InventoryLayers { get; set; } = new List<InventoryLayerDto>();

        // Payment System
        public IList<PaymentMethodDto> PaymentMethods { get; set; } = new List<PaymentMethodDto>();
        public IList<PaymentDto> Payments { get; set; } = new List<PaymentDto>();

        // Security System
        public IList<User> Users { get; set; } = new List<User>();
        public IList<Role> Roles { get; set; } = new List<Role>();
        public IList<SecurityEvent> SecurityEvents { get; set; } = new List<SecurityEvent>();

        /// <summary>
        /// Initializes a new instance of ObjectDb for Infrastructure layer
        /// </summary>
        public ObjectDb()
        {
            // Initialize collections to prevent null reference exceptions
            // Collections are already initialized above with new List<T>()
        }
    }
}