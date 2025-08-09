using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Service for tax calculations and management
    /// </summary>
    [Description("Service for tax calculations and management")]
    public interface ITaxService
    {
        /// <summary>
        /// Calculates applicable taxes for a document based on the operation type
        /// </summary>
        /// <param name="document">The document to calculate taxes for</param>
        /// <param name="operation">The operation being performed (Sale, Purchase, Return)</param>
        /// <returns>Collection of applicable taxes</returns>
        [Description("Calculates applicable taxes for a document")]
        Task<IEnumerable<ITax>> GetApplicableTaxesAsync(IDocument document, DocumentOperation operation);

        /// <summary>
        /// Applies tax rules to document lines and calculates totals
        /// </summary>
        /// <param name="document">The document to apply taxes to</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Applies tax rules to document lines")]
        Task ApplyTaxRulesToDocumentAsync(IDocument document);

        /// <summary>
        /// Calculates tax amount for a specific base amount and tax
        /// </summary>
        /// <param name="baseAmount">The base amount to calculate tax on</param>
        /// <param name="tax">The tax to apply</param>
        /// <param name="operation">The operation type</param>
        /// <returns>The calculated tax amount</returns>
        [Description("Calculates tax amount for a specific base amount")]
        Task<decimal> CalculateTaxAmountAsync(decimal baseAmount, ITax tax, DocumentOperation operation);

        /// <summary>
        /// Gets all active taxes in the system
        /// </summary>
        /// <returns>Collection of active taxes</returns>
        [Description("Gets all active taxes in the system")]
        Task<IEnumerable<ITax>> GetActiveTaxesAsync();

        /// <summary>
        /// Gets taxes by type
        /// </summary>
        /// <param name="taxType">The type of tax to retrieve</param>
        /// <returns>Collection of taxes of the specified type</returns>
        [Description("Gets taxes by type")]
        Task<IEnumerable<ITax>> GetTaxesByTypeAsync(TaxType taxType);

        /// <summary>
        /// Validates tax configuration for a document
        /// </summary>
        /// <param name="document">The document to validate tax configuration for</param>
        /// <returns>Validation result</returns>
        [Description("Validates tax configuration for a document")]
        Task<ValidationResult> ValidateTaxConfigurationAsync(IDocument document);

        /// <summary>
        /// Creates tax totals for a document based on calculated taxes
        /// </summary>
        /// <param name="document">The document to create tax totals for</param>
        /// <param name="operation">The operation type</param>
        /// <returns>Collection of document totals for taxes</returns>
        [Description("Creates tax totals for a document")]
        Task<IEnumerable<IDocumentTotal>> CreateTaxTotalsAsync(IDocument document, DocumentOperation operation);

        /// <summary>
        /// Gets tax summary for a document
        /// </summary>
        /// <param name="document">The document to get tax summary for</param>
        /// <returns>Tax summary information</returns>
        [Description("Gets tax summary for a document")]
        Task<TaxSummary> GetTaxSummaryAsync(IDocument document);

        /// <summary>
        /// Recalculates all taxes for a document (when document is modified)
        /// </summary>
        /// <param name="document">The document to recalculate taxes for</param>
        /// <param name="operation">The operation type</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Recalculates all taxes for a document")]
        Task RecalculateDocumentTaxesAsync(IDocument document, DocumentOperation operation);
    }

    /// <summary>
    /// Tax summary information for a document
    /// </summary>
    [Description("Tax summary information for a document")]
    public class TaxSummary
    {
        /// <summary>
        /// Document ID this summary is for
        /// </summary>
        [Description("Document ID this summary is for")]
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Subtotal amount before taxes
        /// </summary>
        [Description("Subtotal amount before taxes")]
        public decimal SubtotalAmount { get; set; }

        /// <summary>
        /// Total tax amount
        /// </summary>
        [Description("Total tax amount")]
        public decimal TotalTaxAmount { get; set; }

        /// <summary>
        /// Total amount including taxes
        /// </summary>
        [Description("Total amount including taxes")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Collection of tax breakdown by tax type
        /// </summary>
        [Description("Collection of tax breakdown by tax type")]
        public ICollection<TaxBreakdown> TaxBreakdowns { get; set; } = new List<TaxBreakdown>();

        /// <summary>
        /// Date when the summary was calculated
        /// </summary>
        [Description("Date when the summary was calculated")]
        public DateTime CalculatedAt { get; set; }
    }

    /// <summary>
    /// Breakdown of taxes by type
    /// </summary>
    [Description("Breakdown of taxes by type")]
    public class TaxBreakdown
    {
        /// <summary>
        /// Tax code
        /// </summary>
        [Description("Tax code")]
        public string TaxCode { get; set; } = string.Empty;

        /// <summary>
        /// Tax name
        /// </summary>
        [Description("Tax name")]
        public string TaxName { get; set; } = string.Empty;

        /// <summary>
        /// Tax type
        /// </summary>
        [Description("Tax type")]
        public TaxType TaxType { get; set; }

        /// <summary>
        /// Tax rate applied
        /// </summary>
        [Description("Tax rate applied")]
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Base amount for tax calculation
        /// </summary>
        [Description("Base amount for tax calculation")]
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// Calculated tax amount
        /// </summary>
        [Description("Calculated tax amount")]
        public decimal TaxAmount { get; set; }
    }
}