using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.TimeService;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sivar.Erp.Services.Documents
{
    ///// <summary>
    ///// Service for adding accounting totals to documents
    ///// </summary>
    //public class DocumentTotalsService : IDocumentTotalsService
    //{
    //    private readonly IObjectDb _objectDb;
    //    private readonly IDateTimeZoneService _dateTimeService;
    //    private readonly ILogger<DocumentTotalsService> _logger;

    //    /// <summary>
    //    /// Initializes a new instance of the DocumentTotalsService class
    //    /// </summary>
    //    /// <param name="objectDb">The object database</param>
    //    /// <param name="dateTimeService">The date/time service</param>
    //    /// <param name="logger">The logger</param>
    //    public DocumentTotalsService(
    //        IObjectDb objectDb,
    //        IDateTimeZoneService dateTimeService,
    //        ILogger<DocumentTotalsService> logger)
    //    {
    //        _objectDb = objectDb ?? throw new ArgumentNullException(nameof(objectDb));
    //        _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
    //        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    //        // Initialize the collection if it's null
    //        _objectDb.DocumentAccountingProfiles ??= new System.Collections.Generic.List<IDocumentAccountingProfile>();
    //    }

    //    /// <summary>
    //    /// Adds accounting totals to a document based on document operation
    //    /// </summary>
    //    public bool AddDocumentAccountingTotals(IDocument document, string documentOperation)
    //    {
    //        if (document == null) throw new ArgumentNullException(nameof(document));
    //        if (string.IsNullOrWhiteSpace(documentOperation)) throw new ArgumentException("Document operation must be specified", nameof(documentOperation));

    //        try
    //        {
    //            _logger.LogInformation("Adding accounting totals for document {DocumentNumber}, operation {Operation}",
    //                document.DocumentNumber, documentOperation);

    //            // Get the profile for this document operation
    //            var profile = GetDocumentAccountingProfile(documentOperation);

    //            if (profile == null)
    //            {
    //                _logger.LogWarning("No accounting profile found for operation {Operation}", documentOperation);
    //                return false;
    //            }

    //            switch (documentOperation.ToUpperInvariant())
    //            {
    //                case "SALESINVOICE":
    //                    return AddSalesInvoiceTotals(document, profile);
    //                case "PURCHASEINVOICE":
    //                    return AddPurchaseInvoiceTotals(document, profile);
    //                default:
    //                    _logger.LogWarning("Unsupported document operation: {Operation}", documentOperation);
    //                    return false;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error adding accounting totals to document {DocumentNumber}", document.DocumentNumber);
    //            return false;
    //        }
    //    }

    //    /// <summary>
    //    /// Creates a document accounting profile
    //    /// </summary>
    //    public async Task<bool> CreateDocumentAccountingProfileAsync(IDocumentAccountingProfile profile, string userName)
    //    {
    //        if (profile == null) throw new ArgumentNullException(nameof(profile));
    //        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Username must be specified", nameof(userName));

    //        try
    //        {
    //            // Check if profile already exists
    //            var existingProfile = _objectDb.DocumentAccountingProfiles
    //                .FirstOrDefault(p => p.DocumentOperation.Equals(profile.DocumentOperation, StringComparison.OrdinalIgnoreCase));

    //            if (existingProfile != null)
    //            {
    //                _logger.LogWarning("Accounting profile already exists for operation {Operation}", profile.DocumentOperation);
    //                return false;
    //            }

    //            // Set created info
    //            profile.CreatedBy = userName;
    //            profile.CreatedDate = _dateTimeService.Now;

    //            // Add to collection
    //            _objectDb.DocumentAccountingProfiles.Add(profile);
    //            _logger.LogInformation("Created accounting profile for operation {Operation}", profile.DocumentOperation);

    //            // In a real app, we might persist to database here
    //            await Task.CompletedTask;
    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error creating accounting profile for operation {Operation}", profile.DocumentOperation);
    //            return false;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets a document accounting profile by document operation
    //    /// </summary>
    //    public IDocumentAccountingProfile GetDocumentAccountingProfile(string documentOperation)
    //    {
    //        if (string.IsNullOrWhiteSpace(documentOperation)) throw new ArgumentException("Document operation must be specified", nameof(documentOperation));

    //        return _objectDb.DocumentAccountingProfiles
    //            .FirstOrDefault(p => p.DocumentOperation.Equals(documentOperation, StringComparison.OrdinalIgnoreCase));
    //    }

    //    #region Private Methods

    //    private bool AddSalesInvoiceTotals(IDocument document, IDocumentAccountingProfile profile)
    //    {
    //        try
    //        {
    //            // Calculate subtotal from document lines
    //            var subtotal = document.Lines.Sum(l => l.Amount);

    //            // Create subtotal entry (credit to sales account)
    //            var subtotalDto = new TotalDto
    //            {
    //                Oid = Guid.NewGuid(),
    //                Concept = "Subtotal",
    //                Total = subtotal,
    //                CreditAccountCode = profile.SalesAccountCode,
    //                IncludeInTransaction = true
    //            };

    //            // Add subtotal at the beginning
    //            document.DocumentTotals.Insert(0, subtotalDto);

    //            // Calculate total amount including taxes
    //            var totalAmount = document.DocumentTotals.Sum(t => t.Total);

    //            // Add accounts receivable (debit)
    //            var accountsReceivableDto = new TotalDto
    //            {
    //                Oid = Guid.NewGuid(),
    //                Concept = "Accounts Receivable",
    //                Total = totalAmount,
    //                DebitAccountCode = profile.AccountsReceivableCode,
    //                IncludeInTransaction = true
    //            };

    //            document.DocumentTotals.Add(accountsReceivableDto);

    //            // Add cost of goods sold and inventory reduction if configured
    //            if (profile.CostRatio > 0 &&
    //                !string.IsNullOrWhiteSpace(profile.CostOfGoodsSoldAccountCode) &&
    //                !string.IsNullOrWhiteSpace(profile.InventoryAccountCode))
    //            {
    //                var costOfGoodsSold = subtotal * profile.CostRatio;

    //                var cogsDto = new TotalDto
    //                {
    //                    Oid = Guid.NewGuid(),
    //                    Concept = "Cost of Goods Sold",
    //                    Total = costOfGoodsSold,
    //                    DebitAccountCode = profile.CostOfGoodsSoldAccountCode,
    //                    IncludeInTransaction = true
    //                };

    //                var inventoryReductionDto = new TotalDto
    //                {
    //                    Oid = Guid.NewGuid(),
    //                    Concept = "Inventory Reduction",
    //                    Total = costOfGoodsSold,
    //                    CreditAccountCode = profile.InventoryAccountCode,
    //                    IncludeInTransaction = true
    //                };

    //                document.DocumentTotals.Add(cogsDto);
    //                document.DocumentTotals.Add(inventoryReductionDto);
    //            }

    //            _logger.LogInformation("Added sales invoice totals to document {DocumentNumber}", document.DocumentNumber);
    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error adding sales invoice totals to document {DocumentNumber}", document.DocumentNumber);
    //            return false;
    //        }
    //    }

    //    private bool AddPurchaseInvoiceTotals(IDocument document, IDocumentAccountingProfile profile)
    //    {
    //        // Implementation for purchase invoice
    //        // This would follow a similar pattern but with different accounts
    //        // For now, return false as not implemented
    //        _logger.LogWarning("Purchase invoice totals not yet implemented");
    //        return false;
    //    }

    //    #endregion
    //}
}