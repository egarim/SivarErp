using DevExpress.ExpressApp;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Services.Documents;
using Sivar.Erp.Xaf.Module.BusinessObjects.Documents;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Xaf.Module.Services.Documents
{
    /// <summary>
    /// XAF-aware implementation of IDocumentTotalsService using object space pattern
    /// </summary>
    public class XafDocumentTotalsService : IDocumentTotalsService
    {
        private readonly IObjectSpaceProvider _objectSpaceProvider;
        private readonly IDateTimeZoneService _dateTimeService;
        private readonly ILogger<XafDocumentTotalsService> _logger;

        /// <summary>
        /// Initializes a new instance of the XafDocumentTotalsService class
        /// </summary>
        /// <param name="objectSpaceProvider">XAF object space provider</param>
        /// <param name="dateTimeService">Date/time service</param>
        /// <param name="logger">Logger</param>
        public XafDocumentTotalsService(
            IObjectSpaceProvider objectSpaceProvider,
            IDateTimeZoneService dateTimeService,
            ILogger<XafDocumentTotalsService> logger)
        {
            _objectSpaceProvider = objectSpaceProvider ?? throw new ArgumentNullException(nameof(objectSpaceProvider));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds accounting totals to a document based on document operation
        /// </summary>
        public bool AddDocumentAccountingTotals(IDocument document, string documentOperation)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrWhiteSpace(documentOperation)) throw new ArgumentException("Document operation must be specified", nameof(documentOperation));

            try
            {
                _logger.LogInformation("Adding accounting totals for document {DocumentNumber}, operation {Operation}",
                    document.DocumentNumber, documentOperation);

                // Get the profile for this document operation
                var profile = GetDocumentAccountingProfile(documentOperation);

                if (profile == null)
                {
                    _logger.LogWarning("No accounting profile found for operation {Operation}", documentOperation);
                    return false;
                }

                switch (documentOperation.ToUpperInvariant())
                {
                    case "SALESINVOICE":
                        return AddSalesInvoiceTotals(document, profile);
                    case "PURCHASEINVOICE":
                        return AddPurchaseInvoiceTotals(document, profile);
                    default:
                        _logger.LogWarning("Unsupported document operation: {Operation}", documentOperation);
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding accounting totals to document {DocumentNumber}", document.DocumentNumber);
                return false;
            }
        }

        /// <summary>
        /// Creates a document accounting profile
        /// </summary>
        public async Task<bool> CreateDocumentAccountingProfileAsync(IDocumentAccountingProfile profile, string userName)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Username must be specified", nameof(userName));

            try
            {
                using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

                // Check if profile already exists
                var existingProfile = objectSpace.FindObject<DocumentAccountingProfile>(
                    CriteriaOperator.Parse("DocumentOperation == ?", profile.DocumentOperation));

                if (existingProfile != null)
                {
                    _logger.LogWarning("Accounting profile already exists for operation {Operation}", profile.DocumentOperation);
                    return false;
                }

                // Create new XAF persistent profile
                var xafProfile = objectSpace.CreateObject<DocumentAccountingProfile>();
                xafProfile.DocumentOperation = profile.DocumentOperation;
                xafProfile.SalesAccountCode = profile.SalesAccountCode;
                xafProfile.AccountsReceivableCode = profile.AccountsReceivableCode;
                xafProfile.CostOfGoodsSoldAccountCode = profile.CostOfGoodsSoldAccountCode;
                xafProfile.InventoryAccountCode = profile.InventoryAccountCode;
                xafProfile.CostRatio = profile.CostRatio;

                objectSpace.CommitChanges();
                _logger.LogInformation("Created accounting profile for operation {Operation}", profile.DocumentOperation);

                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating accounting profile for operation {Operation}", profile.DocumentOperation);
                return false;
            }
        }

        /// <summary>
        /// Gets a document accounting profile by document operation
        /// </summary>
        public IDocumentAccountingProfile GetDocumentAccountingProfile(string documentOperation)
        {
            if (string.IsNullOrWhiteSpace(documentOperation)) throw new ArgumentException("Document operation must be specified", nameof(documentOperation));

            try
            {
                using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
                return objectSpace.FindObject<DocumentAccountingProfile>(
                    CriteriaOperator.Parse("DocumentOperation == ?", documentOperation.ToUpperInvariant()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounting profile for operation {Operation}", documentOperation);
                return null;
            }
        }

        #region Private Methods

        private bool AddSalesInvoiceTotals(IDocument document, IDocumentAccountingProfile profile)
        {
            try
            {
                // Calculate subtotal from document lines
                var subtotal = document.Lines.Sum(l => l.Amount);

                // If document is a XAF document, work with it directly
                if (document is Document xafDocument)
                {
                    return AddSalesInvoiceTotalsToXafDocument(xafDocument, profile, subtotal);
                }

                // Otherwise, work with interface (fallback for DTO compatibility)
                return AddSalesInvoiceTotalsToInterface(document, profile, subtotal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding sales invoice totals to document {DocumentNumber}", document.DocumentNumber);
                return false;
            }
        }

        private bool AddSalesInvoiceTotalsToXafDocument(Document document, IDocumentAccountingProfile profile, decimal subtotal)
        {
            var objectSpace = _objectSpaceProvider.CreateObjectSpace();

            // Create subtotal entry (credit to sales account)
            var subtotalTotal = objectSpace.CreateObject<Total>();
            subtotalTotal.Concept = "Subtotal";
            subtotalTotal.Amount = subtotal;
            subtotalTotal.CreditAccountCode = profile.SalesAccountCode;
            subtotalTotal.IncludeInTransaction = true;
            subtotalTotal.Document = document;

            // Calculate total amount including taxes
            var totalAmount = document.DocumentTotals.Sum(t => t.Amount) + subtotal;

            // Add accounts receivable (debit)
            var accountsReceivableTotal = objectSpace.CreateObject<Total>();
            accountsReceivableTotal.Concept = "Accounts Receivable";
            accountsReceivableTotal.Amount = totalAmount;
            accountsReceivableTotal.DebitAccountCode = profile.AccountsReceivableCode;
            accountsReceivableTotal.IncludeInTransaction = true;
            accountsReceivableTotal.Document = document;

            // Add cost of goods sold and inventory reduction if configured
            if (profile.CostRatio > 0 &&
                !string.IsNullOrWhiteSpace(profile.CostOfGoodsSoldAccountCode) &&
                !string.IsNullOrWhiteSpace(profile.InventoryAccountCode))
            {
                var costOfGoodsSold = subtotal * profile.CostRatio;

                var cogsTotal = objectSpace.CreateObject<Total>();
                cogsTotal.Concept = "Cost of Goods Sold";
                cogsTotal.Amount = costOfGoodsSold;
                cogsTotal.DebitAccountCode = profile.CostOfGoodsSoldAccountCode;
                cogsTotal.IncludeInTransaction = true;
                cogsTotal.Document = document;

                var inventoryReductionTotal = objectSpace.CreateObject<Total>();
                inventoryReductionTotal.Concept = "Inventory Reduction";
                inventoryReductionTotal.Amount = costOfGoodsSold;
                inventoryReductionTotal.CreditAccountCode = profile.InventoryAccountCode;
                inventoryReductionTotal.IncludeInTransaction = true;
                inventoryReductionTotal.Document = document;
            }

            objectSpace.CommitChanges();
            _logger.LogInformation("Added sales invoice totals to document {DocumentNumber}", document.DocumentNumber);
            return true;
        }

        private bool AddSalesInvoiceTotalsToInterface(IDocument document, IDocumentAccountingProfile profile, decimal subtotal)
        {
            // Create subtotal entry (credit to sales account)
            var subtotalDto = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Subtotal",
                Total = subtotal,
                CreditAccountCode = profile.SalesAccountCode,
                IncludeInTransaction = true
            };

            // Add subtotal at the beginning
            document.DocumentTotals.Insert(0, subtotalDto);

            // Calculate total amount including taxes
            var totalAmount = document.DocumentTotals.Sum(t => t.Total);

            // Add accounts receivable (debit)
            var accountsReceivableDto = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Accounts Receivable",
                Total = totalAmount,
                DebitAccountCode = profile.AccountsReceivableCode,
                IncludeInTransaction = true
            };

            document.DocumentTotals.Add(accountsReceivableDto);

            // Add cost of goods sold and inventory reduction if configured
            if (profile.CostRatio > 0 &&
                !string.IsNullOrWhiteSpace(profile.CostOfGoodsSoldAccountCode) &&
                !string.IsNullOrWhiteSpace(profile.InventoryAccountCode))
            {
                var costOfGoodsSold = subtotal * profile.CostRatio;

                var cogsDto = new TotalDto
                {
                    Oid = Guid.NewGuid(),
                    Concept = "Cost of Goods Sold",
                    Total = costOfGoodsSold,
                    DebitAccountCode = profile.CostOfGoodsSoldAccountCode,
                    IncludeInTransaction = true
                };

                var inventoryReductionDto = new TotalDto
                {
                    Oid = Guid.NewGuid(),
                    Concept = "Inventory Reduction",
                    Total = costOfGoodsSold,
                    CreditAccountCode = profile.InventoryAccountCode,
                    IncludeInTransaction = true
                };

                document.DocumentTotals.Add(cogsDto);
                document.DocumentTotals.Add(inventoryReductionDto);
            }

            _logger.LogInformation("Added sales invoice totals to document {DocumentNumber}", document.DocumentNumber);
            return true;
        }

        private bool AddPurchaseInvoiceTotals(IDocument document, IDocumentAccountingProfile profile)
        {
            try
            {
                // Calculate subtotal from document lines
                var subtotal = document.Lines.Sum(l => l.Amount);

                // If document is a XAF document, work with it directly
                if (document is Document xafDocument)
                {
                    return AddPurchaseInvoiceTotalsToXafDocument(xafDocument, profile, subtotal);
                }

                // Otherwise, work with interface (fallback for DTO compatibility)
                return AddPurchaseInvoiceTotalsToInterface(document, profile, subtotal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding purchase invoice totals to document {DocumentNumber}", document.DocumentNumber);
                return false;
            }
        }

        private bool AddPurchaseInvoiceTotalsToXafDocument(Document document, IDocumentAccountingProfile profile, decimal subtotal)
        {
            var objectSpace = _objectSpaceProvider.CreateObjectSpace();

            // Create inventory entry (debit to inventory account)
            var inventoryTotal = objectSpace.CreateObject<Total>();
            inventoryTotal.Concept = "Inventory Purchase";
            inventoryTotal.Amount = subtotal;
            inventoryTotal.DebitAccountCode = profile.InventoryAccountCode;
            inventoryTotal.IncludeInTransaction = true;
            inventoryTotal.Document = document;

            // Calculate total amount including taxes
            var totalAmount = document.DocumentTotals.Sum(t => t.Amount) + subtotal;

            // Add accounts payable (credit)
            var accountsPayableTotal = objectSpace.CreateObject<Total>();
            accountsPayableTotal.Concept = "Accounts Payable";
            accountsPayableTotal.Amount = totalAmount;
            accountsPayableTotal.CreditAccountCode = "ACCOUNTS_PAYABLE"; // Use mapping key
            accountsPayableTotal.IncludeInTransaction = true;
            accountsPayableTotal.Document = document;

            objectSpace.CommitChanges();
            _logger.LogInformation("Added purchase invoice totals to document {DocumentNumber}", document.DocumentNumber);
            return true;
        }

        private bool AddPurchaseInvoiceTotalsToInterface(IDocument document, IDocumentAccountingProfile profile, decimal subtotal)
        {
            // Create inventory entry (debit to inventory account)
            var inventoryDto = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Inventory Purchase",
                Total = subtotal,
                DebitAccountCode = profile.InventoryAccountCode,
                IncludeInTransaction = true
            };

            document.DocumentTotals.Add(inventoryDto);

            // Calculate total amount including taxes
            var totalAmount = document.DocumentTotals.Sum(t => t.Total);

            // Add accounts payable (credit)
            var accountsPayableDto = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Accounts Payable",
                Total = totalAmount,
                CreditAccountCode = "ACCOUNTS_PAYABLE", // Use mapping key
                IncludeInTransaction = true
            };

            document.DocumentTotals.Add(accountsPayableDto);

            _logger.LogInformation("Added purchase invoice totals to document {DocumentNumber}", document.DocumentNumber);
            return true;
        }

        #endregion
    }
}