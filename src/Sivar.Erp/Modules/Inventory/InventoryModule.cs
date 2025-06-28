using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.ErpSystem.Modules;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.Services;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Modules.Inventory.Reports;
using Sivar.Erp.Services;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Implementation of the inventory module that manages inventory operations
    /// </summary>
    public class InventoryModule : ErpModuleBase, IInventoryModule
    {
        private readonly IObjectDb _objectDb;
        private readonly ILogger<InventoryModule> _logger;
        private readonly PerformanceLogger<InventoryModule> _performanceLogger;

        private const string INVENTORY_TRANSACTION_SEQUENCE_CODE = "INV_TRANS";
        private const string INVENTORY_RESERVATION_SEQUENCE_CODE = "INV_RESV";
        private const string KARDEX_REPORT_SEQUENCE_CODE = "KRX_REPORT";

        /// <summary>
        /// Gets the inventory service instance
        /// </summary>
        public IInventoryService InventoryService { get; }

        /// <summary>
        /// Gets the inventory reservation service instance
        /// </summary>
        public IInventoryReservationService ReservationService { get; }

        /// <summary>
        /// Gets the kardex service instance
        /// </summary>
        public IKardexService KardexService { get; }

        /// <summary>
        /// Initializes a new instance of the InventoryModule class
        /// </summary>
        /// <param name="optionService">Option service instance</param>
        /// <param name="activityStreamService">Activity stream service instance</param>
        /// <param name="dateTimeZoneService">Date time zone service instance</param>
        /// <param name="sequencerService">Sequencer service instance</param>
        /// <param name="inventoryService">Inventory service instance</param>
        /// <param name="reservationService">Reservation service instance</param>
        /// <param name="kardexService">Kardex service instance</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="objectDb">Object database instance</param>
        /// <param name="contextProvider">Performance context provider</param>
        public InventoryModule(
            IOptionService optionService,
            IActivityStreamService activityStreamService,
            IDateTimeZoneService dateTimeZoneService,
            ISequencerService sequencerService,
            IInventoryService inventoryService,
            IInventoryReservationService reservationService,
            IKardexService kardexService,
            ILogger<InventoryModule> logger,
            IObjectDb objectDb = null,
            IPerformanceContextProvider contextProvider = null)
            : base(optionService, activityStreamService, dateTimeZoneService, sequencerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _objectDb = objectDb;
            InventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            ReservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
            KardexService = kardexService ?? throw new ArgumentNullException(nameof(kardexService));
            _performanceLogger = new PerformanceLogger<InventoryModule>(logger, PerformanceLogMode.All, 100, 10_000_000, objectDb, contextProvider);
        }

        /// <summary>
        /// Initializes the inventory module and creates necessary sequences
        /// </summary>
        public async Task InitializeAsync()
        {
            await _performanceLogger.Track(nameof(InitializeAsync), async () =>
            {
                // Initialize collections in ObjectDb if needed
                if (_objectDb != null)
                {
                    _objectDb.InventoryItems ??= new List<IInventoryItem>();
                    _objectDb.StockLevels ??= new List<IStockLevel>();
                    _objectDb.InventoryTransactions ??= new List<IInventoryTransaction>();
                    _objectDb.InventoryReservations ??= new List<IInventoryReservation>();
                }

                // Register sequences if not already registered
                if (_objectDb?.Sequences != null)
                {
                    if (!_objectDb.Sequences.Any(s => s.Code == INVENTORY_TRANSACTION_SEQUENCE_CODE) ||
                        !_objectDb.Sequences.Any(s => s.Code == INVENTORY_RESERVATION_SEQUENCE_CODE) ||
                        !_objectDb.Sequences.Any(s => s.Code == KARDEX_REPORT_SEQUENCE_CODE))
                    {
                        RegisterSequence(_objectDb.Sequences);
                    }
                }

                _logger.LogInformation("Inventory module initialized successfully");
            });
        }

        /// <summary>
        /// Registers inventory sequences in the system
        /// </summary>
        /// <param name="sequenceDtos">Collection of sequence DTOs to register with</param>
        public override void RegisterSequence(IEnumerable<SequenceDto> sequenceDtos)
        {
            _performanceLogger.Track(nameof(RegisterSequence), () =>
            {
                // Transaction sequence
                var transactionSequence = new SequenceDto
                {
                    Code = INVENTORY_TRANSACTION_SEQUENCE_CODE,
                    CurrentNumber = 1,
                    Name = "Inventory Transactions",
                    Prefix = "INV",
                    Suffix = ""
                };

                // Reservation sequence
                var reservationSequence = new SequenceDto
                {
                    Code = INVENTORY_RESERVATION_SEQUENCE_CODE,
                    CurrentNumber = 1,
                    Name = "Inventory Reservations",
                    Prefix = "RES",
                    Suffix = ""
                };

                // Kardex report sequence
                var kardexReportSequence = new SequenceDto
                {
                    Code = KARDEX_REPORT_SEQUENCE_CODE,
                    CurrentNumber = 1,
                    Name = "Kardex Reports",
                    Prefix = "KRX",
                    Suffix = ""
                };

                // Register sequences
                sequencerService.CreateSequenceAsync(transactionSequence);
                sequencerService.CreateSequenceAsync(reservationSequence);
                sequencerService.CreateSequenceAsync(kardexReportSequence);

                _logger.LogInformation("Inventory sequences registered successfully");
            });
        }

        /// <summary>
        /// Gets an inventory item by code
        /// </summary>
        public async Task<IInventoryItem> GetInventoryItemAsync(string itemCode)
        {
            return await InventoryService.GetInventoryItemAsync(itemCode);
        }

        /// <summary>
        /// Creates a new inventory item
        /// </summary>
        public async Task<IInventoryItem> CreateInventoryItemAsync(IInventoryItem item, string userName)
        {
            return await InventoryService.CreateInventoryItemAsync(item, userName);
        }

        /// <summary>
        /// Gets the stock level for an item
        /// </summary>
        public async Task<IStockLevel> GetStockLevelAsync(string itemCode, string warehouseCode = null)
        {
            if (string.IsNullOrEmpty(warehouseCode))
            {
                var stockLevels = await InventoryService.GetStockLevelsAsync(itemCode);
                return stockLevels.FirstOrDefault();
            }
            
            return await InventoryService.GetStockLevelAsync(itemCode, warehouseCode);
        }

        /// <summary>
        /// Processes a receipt of inventory
        /// </summary>
        public async Task<IInventoryTransaction> ReceiveInventoryAsync(
            IInventoryItem item, 
            decimal quantity, 
            string warehouseCode,
            InventoryTransactionType transactionType, 
            string referenceDocument, 
            decimal unitCost,
            string userName, 
            string notes = null)
        {
            return await _performanceLogger.Track(nameof(ReceiveInventoryAsync), async () =>
            {
                var transaction = await InventoryService.RecordInventoryReceiptAsync(
                    item, quantity, warehouseCode, transactionType, 
                    referenceDocument, unitCost, userName, notes);

                // Record activity
                var systemActor = CreateSystemStreamObject();
                var itemTarget = CreateStreamObject(
                    "InventoryItem",
                    item.Code,
                    $"Item {item.Code} - {item.Description}");

                var contextData = new Dictionary<string, object>
                {
                    { "quantity", quantity },
                    { "warehouse", warehouseCode },
                    { "reference", referenceDocument },
                    { "transactionId", transaction.TransactionId }
                };
                
                string contextJson = JsonSerializer.Serialize(contextData);

                await RecordActivityAsync(
                    systemActor,
                    "Received",
                    itemTarget,
                    contextJson);

                return transaction;
            });
        }

        /// <summary>
        /// Processes an issue of inventory
        /// </summary>
        public async Task<IInventoryTransaction> IssueInventoryAsync(
            IInventoryItem item, 
            decimal quantity, 
            string warehouseCode,
            InventoryTransactionType transactionType, 
            string referenceDocument,
            string userName, 
            string notes = null)
        {
            return await _performanceLogger.Track(nameof(IssueInventoryAsync), async () =>
            {
                var transaction = await InventoryService.RecordInventoryIssueAsync(
                    item, quantity, warehouseCode, transactionType, 
                    referenceDocument, userName, notes);

                // Record activity
                var systemActor = CreateSystemStreamObject();
                var itemTarget = CreateStreamObject(
                    "InventoryItem",
                    item.Code,
                    $"Item {item.Code} - {item.Description}");
                    
                var contextData = new Dictionary<string, object>
                {
                    { "quantity", quantity },
                    { "warehouse", warehouseCode },
                    { "reference", referenceDocument },
                    { "transactionId", transaction.TransactionId }
                };
                
                string contextJson = JsonSerializer.Serialize(contextData);

                await RecordActivityAsync(
                    systemActor,
                    "Issued",
                    itemTarget,
                    contextJson);

                return transaction;
            });
        }

        /// <summary>
        /// Creates a reservation for an item
        /// </summary>
        public async Task<IInventoryReservation> ReserveInventoryAsync(
            IInventoryItem item, 
            decimal quantity, 
            string warehouseCode,
            string sourceDocumentNumber, 
            int expiryMinutes, 
            string userName)
        {
            return await _performanceLogger.Track(nameof(ReserveInventoryAsync), async () =>
            {
                var reservation = await ReservationService.CreateReservationAsync(
                    item, quantity, warehouseCode, sourceDocumentNumber, 
                    expiryMinutes, userName);

                if (reservation != null)
                {
                    // Record activity
                    var userActor = CreateStreamObject(
                        "User",
                        userName,
                        $"User {userName}");

                    var itemTarget = CreateStreamObject(
                        "InventoryItem",
                        item.Code,
                        $"Item {item.Code} - {item.Description}");

                    var contextData = new Dictionary<string, object>
                    {
                        { "quantity", quantity },
                        { "warehouse", warehouseCode },
                        { "document", sourceDocumentNumber },
                        { "reservationId", reservation.ReservationId }
                    };
                    
                    string contextJson = JsonSerializer.Serialize(contextData);

                    await RecordActivityAsync(
                        userActor,
                        "Reserved",
                        itemTarget,
                        contextJson);
                }

                return reservation;
            });
        }

        /// <summary>
        /// Cancels an inventory reservation
        /// </summary>
        public async Task<bool> CancelReservationAsync(string reservationId, string userName)
        {
            return await _performanceLogger.Track(nameof(CancelReservationAsync), async () =>
            {
                // Get the reservation first to record activity properly
                var reservation = await ReservationService.GetReservationAsync(reservationId);
                if (reservation == null)
                    return false;

                var result = await ReservationService.CancelReservationAsync(reservationId, userName);

                if (result)
                {
                    // Record activity
                    var userActor = CreateStreamObject(
                        "User",
                        userName,
                        $"User {userName}");

                    var itemTarget = CreateStreamObject(
                        "InventoryItem",
                        reservation.Item.Code,
                        $"Item {reservation.Item.Code} - {reservation.Item.Description}");

                    var contextData = new Dictionary<string, object>
                    {
                        { "quantity", reservation.Quantity },
                        { "warehouse", reservation.WarehouseCode },
                        { "document", reservation.SourceDocumentNumber },
                        { "reservationId", reservation.ReservationId }
                    };
                    
                    string contextJson = JsonSerializer.Serialize(contextData);

                    await RecordActivityAsync(
                        userActor,
                        "Cancelled Reservation",
                        itemTarget,
                        contextJson);
                }

                return result;
            });
        }

        /// <summary>
        /// Fulfills an inventory reservation by issuing the stock
        /// </summary>
        public async Task<IInventoryTransaction> FulfillReservationAsync(
            string reservationId, 
            decimal actualQuantity, 
            string userName)
        {
            return await _performanceLogger.Track(nameof(FulfillReservationAsync), async () =>
            {
                // Get the reservation first to record activity properly
                var reservation = await ReservationService.GetReservationAsync(reservationId);
                if (reservation == null)
                    return null;

                var transaction = await ReservationService.FulfillReservationAsync(
                    reservationId, actualQuantity, userName);

                // Record activity
                var userActor = CreateStreamObject(
                    "User",
                    userName,
                    $"User {userName}");

                var itemTarget = CreateStreamObject(
                    "InventoryItem",
                    reservation.Item.Code,
                    $"Item {reservation.Item.Code} - {reservation.Item.Description}");
                    
                var contextData = new Dictionary<string, object>
                {
                    { "quantity", actualQuantity },
                    { "warehouse", reservation.WarehouseCode },
                    { "document", reservation.SourceDocumentNumber },
                    { "reservationId", reservation.ReservationId },
                    { "transactionId", transaction.TransactionId }
                };
                
                string contextJson = JsonSerializer.Serialize(contextData);

                await RecordActivityAsync(
                    userActor,
                    "Fulfilled Reservation",
                    itemTarget,
                    contextJson);

                return transaction;
            });
        }

        /// <summary>
        /// Generates a kardex report for an item
        /// </summary>
        public async Task<KardexReportDto> GenerateKardexReportAsync(
            string itemCode, 
            DateOnly startDate, 
            DateOnly endDate,
            string warehouseCode = null)
        {
            return await _performanceLogger.Track(nameof(GenerateKardexReportAsync), async () =>
            {
                var report = await KardexService.GenerateKardexReportAsync(
                    itemCode, startDate, endDate, warehouseCode);

                // Record activity
                var systemActor = CreateSystemStreamObject();
                
                var itemTarget = CreateStreamObject(
                    "InventoryItem",
                    itemCode,
                    $"Item {itemCode}");
                    
                var contextData = new Dictionary<string, object>
                {
                    { "reportId", report.ReportId },
                    { "startDate", startDate.ToString() },
                    { "endDate", endDate.ToString() },
                    { "warehouse", warehouseCode ?? "All" }
                };
                
                string contextJson = JsonSerializer.Serialize(contextData);

                await RecordActivityAsync(
                    systemActor,
                    "Generated Kardex",
                    itemTarget,
                    contextJson);

                return report;
            });
        }

        /// <summary>
        /// Gets inventory valuation as of a specific date
        /// </summary>
        public async Task<InventoryValuationReportDto> GetInventoryValuationAsync(
            DateOnly asOfDate, 
            string warehouseCode = null)
        {
            return await KardexService.GetInventoryValuationAsync(asOfDate, warehouseCode);
        }
    }
}