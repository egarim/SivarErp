using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Implementation of tax service for calculating and managing taxes
    /// </summary>
    [Description("Implementation of tax service")]
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
            _logger.LogInformation("Getting applicable taxes for document {DocumentNumber} operation {Operation}", 
                document.DocumentNumber, operation);

            // Get all active taxes
            var activeTaxes = await GetActiveTaxesAsync();

            // Filter taxes based on operation type and business entity
            var applicableTaxes = activeTaxes.Where(tax =>
            {
                // Simple logic - in real implementation this would be more complex
                return operation switch
                {
                    DocumentOperation.Sale => tax.TaxType == TaxType.VAT || tax.TaxType == TaxType.Sales,
                    DocumentOperation.Purchase => tax.TaxType == TaxType.VAT,
                    DocumentOperation.Return => tax.TaxType == TaxType.VAT,
                    _ => false
                };
            });

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
            _logger.LogInformation("Applying tax rules to document {DocumentNumber}", document.DocumentNumber);

            // Placeholder implementation - would contain complex tax rule logic
            await Task.CompletedTask;

            _logger.LogInformation("Applied tax rules to document {DocumentNumber}", document.DocumentNumber);
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
            if (!tax.IsActive)
                return 0;

            // Simple percentage calculation
            var taxAmount = baseAmount * (tax.Rate / 100);

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
                .Where(t => t.TaxType == taxType && t.IsActive)
                .OrderBy(t => t.Code)
                .ToList();

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

            // Check if document has required tax information
            if (document.BusinessEntity?.EntityType == BusinessEntityType.Customer)
            {
                // Validate customer tax requirements
                // This would include checking tax exempt status, etc.
            }

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
            var taxTotals = new List<IDocumentTotal>();
            var applicableTaxes = await GetApplicableTaxesAsync(document, operation);

            foreach (var tax in applicableTaxes)
            {
                var baseAmount = document.TotalAmount; // Simplified - would calculate proper base
                var taxAmount = await CalculateTaxAmountAsync(baseAmount, tax, operation);

                if (taxAmount > 0)
                {
                    var taxTotal = _repository.CreateObject<DocumentTotalDto>();
                    taxTotal.Concept = $"{tax.Name} ({tax.Rate}%)";
                    taxTotal.Total = taxAmount;
                    taxTotal.IncludeInTransaction = true;
                    // Set appropriate account codes based on tax type and operation
                    
                    taxTotals.Add(taxTotal);
                }
            }

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
            var summary = new TaxSummary
            {
                DocumentId = document.Id,
                SubtotalAmount = document.TotalAmount,
                CalculatedAt = DateTime.UtcNow
            };

            // Calculate tax breakdowns
            // This would involve complex logic to break down taxes by type
            summary.TotalTaxAmount = 0; // Placeholder
            summary.TotalAmount = summary.SubtotalAmount + summary.TotalTaxAmount;

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
            _logger.LogInformation("Recalculating taxes for document {DocumentNumber}", document.DocumentNumber);

            // Clear existing tax totals
            var existingTaxTotals = document.DocumentTotals
                .Where(dt => dt.Concept.Contains("Tax") || dt.Concept.Contains("%"))
                .ToList();

            foreach (var taxTotal in existingTaxTotals)
            {
                document.DocumentTotals.Remove(taxTotal);
            }

            // Recalculate and add new tax totals
            var newTaxTotals = await CreateTaxTotalsAsync(document, operation);
            foreach (var taxTotal in newTaxTotals)
            {
                document.DocumentTotals.Add(taxTotal);
            }

            // Mark document as modified
            if (document is DocumentDto documentDto)
            {
                _repository.MarkAsModified(documentDto);
            }

            _logger.LogInformation("Recalculated taxes for document {DocumentNumber}", document.DocumentNumber);
        }
    }
}