using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Implementation of tax calculations and management service
    /// </summary>
    [Description("Implementation of tax calculations and management service")]
    public class TaxService : ITaxService
    {
        private readonly IRepository _repository;
        private readonly ILogger<TaxService> _logger;

        /// <summary>
        /// Initializes a new instance of the TaxService
        /// </summary>
        /// <param name="repository">Repository for data access</param>
        /// <param name="logger">Logger for the service</param>
        public TaxService(IRepository repository, ILogger<TaxService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calculates applicable taxes for a document based on the operation type
        /// </summary>
        /// <param name="document">The document to calculate taxes for</param>
        /// <param name="operation">The operation being performed (Sale, Purchase, Return)</param>
        /// <returns>Collection of applicable taxes</returns>
        [Description("Calculates applicable taxes for a document")]
        public async Task<IEnumerable<ITax>> GetApplicableTaxesAsync(IDocument document, DocumentOperation operation)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _logger.LogDebug("Getting applicable taxes for document {DocumentNumber} operation {Operation}", 
                document.DocumentNumber, operation);

            // Get all active taxes
            var activeTaxes = await GetActiveTaxesAsync();
            var applicableTaxes = new List<ITax>();

            // Apply business logic to determine which taxes apply
            // This is a simplified version - in a real system, this would be much more complex
            foreach (var tax in activeTaxes)
            {
                bool isApplicable = false;

                switch (operation)
                {
                    case DocumentOperation.Sale:
                        // For sales, typically apply sales tax, VAT
                        isApplicable = tax.TaxType == TaxType.VAT || 
                                     tax.TaxType == TaxType.Sales ||
                                     tax.TaxType == TaxType.Withholding;
                        break;

                    case DocumentOperation.Purchase:
                        // For purchases, typically apply VAT (as input tax), withholding
                        isApplicable = tax.TaxType == TaxType.VAT ||
                                     tax.TaxType == TaxType.Withholding;
                        break;

                    case DocumentOperation.Return:
                        // For returns, apply the same taxes as the original operation (with reversal)
                        isApplicable = tax.TaxType == TaxType.VAT || 
                                     tax.TaxType == TaxType.Sales;
                        break;
                }

                if (isApplicable)
                {
                    applicableTaxes.Add(tax);
                }
            }

            _logger.LogDebug("Found {TaxCount} applicable taxes for document {DocumentNumber}", 
                applicableTaxes.Count, document.DocumentNumber);

            return applicableTaxes;
        }

        /// <summary>
        /// Applies tax rules to document lines and calculates totals
        /// </summary>
        /// <param name="document">The document to apply taxes to</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Applies tax rules to document lines")]
        public async Task ApplyTaxRulesToDocumentAsync(IDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _logger.LogInformation("Applying tax rules to document {DocumentNumber}", document.DocumentNumber);

            // In a real implementation, this would:
            // 1. Evaluate tax rules based on business entity, items, amounts, etc.
            // 2. Apply tax exemptions and special cases
            // 3. Calculate line-level taxes
            // 4. Handle tax rounding rules
            
            // For now, we'll implement a simplified version that sets up basic tax structure
            
            // Clear existing tax totals
            var existingTaxTotals = document.DocumentTotals
                .Where(dt => dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var taxTotal in existingTaxTotals)
            {
                document.DocumentTotals.Remove(taxTotal);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Calculates tax amount for a specific base amount and tax
        /// </summary>
        /// <param name="baseAmount">The base amount to calculate tax on</param>
        /// <param name="tax">The tax to apply</param>
        /// <param name="operation">The operation type</param>
        /// <returns>The calculated tax amount</returns>
        [Description("Calculates tax amount for a specific base amount")]
        public async Task<decimal> CalculateTaxAmountAsync(decimal baseAmount, ITax tax, DocumentOperation operation)
        {
            if (tax == null)
                throw new ArgumentNullException(nameof(tax));

            if (baseAmount <= 0)
                return 0;

            // Basic tax calculation: baseAmount * (rate / 100)
            var taxAmount = Math.Round(baseAmount * (tax.Rate / 100m), 2, MidpointRounding.AwayFromZero);

            _logger.LogDebug("Calculated tax amount {TaxAmount} for base {BaseAmount} using tax {TaxCode} at rate {Rate}%", 
                taxAmount, baseAmount, tax.Code, tax.Rate);

            return await Task.FromResult(taxAmount);
        }

        /// <summary>
        /// Gets all active taxes in the system
        /// </summary>
        /// <returns>Collection of active taxes</returns>
        [Description("Gets all active taxes in the system")]
        public async Task<IEnumerable<ITax>> GetActiveTaxesAsync()
        {
            var activeTaxes = _repository.GetObjects<TaxDto>()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Code)
                .ToList();

            _logger.LogDebug("Retrieved {TaxCount} active taxes", activeTaxes.Count);

            return await Task.FromResult(activeTaxes);
        }

        /// <summary>
        /// Gets taxes by type
        /// </summary>
        /// <param name="taxType">The type of tax to retrieve</param>
        /// <returns>Collection of taxes of the specified type</returns>
        [Description("Gets taxes by type")]
        public async Task<IEnumerable<ITax>> GetTaxesByTypeAsync(TaxType taxType)
        {
            var taxes = _repository.GetObjects<TaxDto>()
                .Where(t => t.IsActive && t.TaxType == taxType)
                .OrderBy(t => t.Code)
                .ToList();

            _logger.LogDebug("Retrieved {TaxCount} taxes of type {TaxType}", taxes.Count, taxType);

            return await Task.FromResult(taxes);
        }

        /// <summary>
        /// Validates tax configuration for a document
        /// </summary>
        /// <param name="document">The document to validate tax configuration for</param>
        /// <returns>Validation result</returns>
        [Description("Validates tax configuration for a document")]
        public async Task<ValidationResult> ValidateTaxConfigurationAsync(IDocument document)
        {
            var result = new ValidationResult { IsValid = true };

            if (document == null)
            {
                result.IsValid = false;
                result.Errors.Add("Document cannot be null");
                return result;
            }

            // Validate tax totals
            var taxTotals = document.DocumentTotals
                .Where(dt => dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var taxTotal in taxTotals)
            {
                // Check for negative tax amounts
                if (taxTotal.Total < 0)
                {
                    result.Errors.Add($"Tax total {taxTotal.Concept} cannot be negative");
                    result.IsValid = false;
                }

                // Validate account codes are specified
                if (string.IsNullOrWhiteSpace(taxTotal.DebitAccountCode) && 
                    string.IsNullOrWhiteSpace(taxTotal.CreditAccountCode))
                {
                    result.Warnings.Add($"Tax total {taxTotal.Concept} has no account codes specified");
                }
            }

            // Check for unreasonably high tax rates (over 50%)
            var subtotal = document.DocumentTotals
                .Where(dt => !dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .Sum(dt => dt.Total);

            var totalTax = taxTotals.Sum(tt => tt.Total);

            if (subtotal > 0 && totalTax > 0)
            {
                var effectiveTaxRate = (totalTax / subtotal) * 100m;
                if (effectiveTaxRate > 50m)
                {
                    result.Warnings.Add($"Effective tax rate is {effectiveTaxRate:F2}% which seems unusually high");
                }
            }

            _logger.LogDebug("Tax configuration validation completed for document {DocumentNumber}: {IsValid}", 
                document.DocumentNumber, result.IsValid);

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Creates tax totals for a document based on calculated taxes
        /// </summary>
        /// <param name="document">The document to create tax totals for</param>
        /// <param name="operation">The operation type</param>
        /// <returns>Collection of document totals for taxes</returns>
        [Description("Creates tax totals for a document")]
        public async Task<IEnumerable<IDocumentTotal>> CreateTaxTotalsAsync(IDocument document, DocumentOperation operation)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _logger.LogInformation("Creating tax totals for document {DocumentNumber} operation {Operation}", 
                document.DocumentNumber, operation);

            var taxTotals = new List<IDocumentTotal>();

            // Calculate subtotal (excluding existing tax totals)
            var subtotal = document.DocumentTotals
                .Where(dt => !dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .Sum(dt => dt.Total);

            if (subtotal <= 0)
            {
                _logger.LogWarning("No subtotal found for document {DocumentNumber}, cannot calculate taxes", 
                    document.DocumentNumber);
                return taxTotals;
            }

            // Get applicable taxes for this operation
            var applicableTaxes = await GetApplicableTaxesAsync(document, operation);

            foreach (var tax in applicableTaxes)
            {
                var taxAmount = await CalculateTaxAmountAsync(subtotal, tax, operation);

                if (taxAmount > 0)
                {
                    var taxTotal = _repository.CreateObject<DocumentTotalDto>();
                    taxTotal.Concept = $"Tax: {tax.Name}";
                    taxTotal.Total = taxAmount;
                    taxTotal.IncludeInTransaction = true;

                    // Set account codes based on operation and tax type
                    SetTaxAccountCodes(taxTotal, tax, operation);

                    taxTotals.Add(taxTotal);
                }
            }

            _logger.LogInformation("Created {TaxTotalCount} tax totals totaling ${TotalTaxAmount:F2} for document {DocumentNumber}", 
                taxTotals.Count, taxTotals.Sum(tt => tt.Total), document.DocumentNumber);

            return taxTotals;
        }

        /// <summary>
        /// Gets tax summary for a document
        /// </summary>
        /// <param name="document">The document to get tax summary for</param>
        /// <returns>Tax summary information</returns>
        [Description("Gets tax summary for a document")]
        public async Task<TaxSummary> GetTaxSummaryAsync(IDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var summary = new TaxSummary
            {
                DocumentId = document.Id,
                CalculatedAt = DateTime.UtcNow
            };

            // Calculate subtotal (excluding taxes)
            summary.SubtotalAmount = document.DocumentTotals
                .Where(dt => !dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .Sum(dt => dt.Total);

            // Calculate total tax amount
            var taxTotals = document.DocumentTotals
                .Where(dt => dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .ToList();

            summary.TotalTaxAmount = taxTotals.Sum(tt => tt.Total);
            summary.TotalAmount = summary.SubtotalAmount + summary.TotalTaxAmount;

            // Create tax breakdowns
            foreach (var taxTotal in taxTotals)
            {
                // Extract tax code from concept (format: "Tax: TaxName")
                var taxName = taxTotal.Concept.Substring(5); // Remove "Tax: " prefix
                var tax = _repository.GetObjects<TaxDto>()
                    .FirstOrDefault(t => t.Name == taxName);

                if (tax != null)
                {
                    var breakdown = new TaxBreakdown
                    {
                        TaxCode = tax.Code,
                        TaxName = tax.Name,
                        TaxType = tax.TaxType,
                        TaxRate = tax.Rate,
                        BaseAmount = summary.SubtotalAmount,
                        TaxAmount = taxTotal.Total
                    };

                    summary.TaxBreakdowns.Add(breakdown);
                }
            }

            _logger.LogDebug("Generated tax summary for document {DocumentNumber}: Subtotal={SubtotalAmount:C}, Tax={TotalTaxAmount:C}, Total={TotalAmount:C}", 
                document.DocumentNumber, summary.SubtotalAmount, summary.TotalTaxAmount, summary.TotalAmount);

            return await Task.FromResult(summary);
        }

        /// <summary>
        /// Recalculates all taxes for a document (when document is modified)
        /// </summary>
        /// <param name="document">The document to recalculate taxes for</param>
        /// <param name="operation">The operation type</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Recalculates all taxes for a document")]
        public async Task RecalculateDocumentTaxesAsync(IDocument document, DocumentOperation operation)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _logger.LogInformation("Recalculating taxes for document {DocumentNumber}", document.DocumentNumber);

            // Clear existing tax totals
            var existingTaxTotals = document.DocumentTotals
                .Where(dt => dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var taxTotal in existingTaxTotals)
            {
                document.DocumentTotals.Remove(taxTotal);
            }

            // Apply tax rules
            await ApplyTaxRulesToDocumentAsync(document);

            // Create new tax totals
            var newTaxTotals = await CreateTaxTotalsAsync(document, operation);

            // Add new tax totals to document
            foreach (var taxTotal in newTaxTotals)
            {
                document.DocumentTotals.Add(taxTotal);
            }

            _logger.LogInformation("Recalculated taxes for document {DocumentNumber}: {TaxCount} tax totals", 
                document.DocumentNumber, newTaxTotals.Count());
        }

        #region Private Helper Methods

        /// <summary>
        /// Sets appropriate account codes for tax totals based on tax type and operation
        /// </summary>
        /// <param name="taxTotal">Tax total to set account codes for</param>
        /// <param name="tax">Tax definition</param>
        /// <param name="operation">Document operation</param>
        private void SetTaxAccountCodes(IDocumentTotal taxTotal, ITax tax, DocumentOperation operation)
        {
            // This is a simplified mapping - in a real system, this would come from tax accounting profiles
            switch (tax.TaxType)
            {
                case TaxType.VAT:
                    if (operation == DocumentOperation.Sale)
                    {
                        // Sales VAT - company collects from customer
                        taxTotal.DebitAccountCode = "ACCOUNTS_RECEIVABLE";
                        taxTotal.CreditAccountCode = "VAT_PAYABLE";
                    }
                    else if (operation == DocumentOperation.Purchase)
                    {
                        // Purchase VAT - company pays to supplier (input tax)
                        taxTotal.DebitAccountCode = "VAT_RECEIVABLE";
                        taxTotal.CreditAccountCode = "ACCOUNTS_PAYABLE";
                    }
                    break;

                case TaxType.Sales:
                    // Sales tax - similar to VAT for sales
                    taxTotal.DebitAccountCode = "ACCOUNTS_RECEIVABLE";
                    taxTotal.CreditAccountCode = "SALES_TAX_PAYABLE";
                    break;

                case TaxType.Withholding:
                    if (operation == DocumentOperation.Sale)
                    {
                        // Withholding on sales (customer withholds from payment)
                        taxTotal.DebitAccountCode = "WITHHOLDING_RECEIVABLE";
                        taxTotal.CreditAccountCode = "ACCOUNTS_RECEIVABLE";
                    }
                    else if (operation == DocumentOperation.Purchase)
                    {
                        // Withholding on purchases (we withhold from supplier)
                        taxTotal.DebitAccountCode = "ACCOUNTS_PAYABLE";
                        taxTotal.CreditAccountCode = "WITHHOLDING_PAYABLE";
                    }
                    break;

                case TaxType.Income:
                    // Income tax withholding
                    taxTotal.DebitAccountCode = "INCOME_TAX_RECEIVABLE";
                    taxTotal.CreditAccountCode = "INCOME_TAX_PAYABLE";
                    break;

                default:
                    // Default mapping
                    taxTotal.DebitAccountCode = "TAX_RECEIVABLE";
                    taxTotal.CreditAccountCode = "TAX_PAYABLE";
                    break;
            }
        }

        #endregion
    }
}