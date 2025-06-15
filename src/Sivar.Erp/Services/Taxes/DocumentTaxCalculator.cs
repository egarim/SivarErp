using System.Collections.Generic;
using System.Linq;
using Sivar.Erp.Services.Taxes.TaxRule;
using Sivar.Erp.Services.Taxes.TaxAccountingProfiles;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Services.Taxes
{
    /// <summary>
    /// Handles tax calculations at the document level
    /// </summary>
    public class DocumentTaxCalculator
    {
        private readonly DocumentDto _document;
        private readonly string _documentTypeCode;
        private readonly TaxRuleEvaluator _taxRuleEvaluator;
        private readonly ITaxAccountingProfileService _taxAccountingService;

        /// <summary>
        /// Initializes a new instance of the DocumentTaxCalculator
        /// </summary>
        /// <param name="document">The document to calculate taxes for</param>
        /// <param name="documentTypeCode">The document type code to use for tax rule evaluation</param>
        /// <param name="taxRuleEvaluator">The evaluator for determining applicable taxes</param>
        /// <param name="taxAccountingService">Optional service for tax accounting profiles</param>
        public DocumentTaxCalculator(
            DocumentDto document, 
            string documentTypeCode, 
            TaxRuleEvaluator taxRuleEvaluator,
            ITaxAccountingProfileService taxAccountingService = null)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _documentTypeCode = documentTypeCode ?? throw new ArgumentNullException(nameof(documentTypeCode));
            _taxRuleEvaluator = taxRuleEvaluator ?? throw new ArgumentNullException(nameof(taxRuleEvaluator));
            _taxAccountingService = taxAccountingService;
        }

        /// <summary>
        /// Calculates document-level taxes and updates the document totals
        /// </summary>
        public void CalculateDocumentTaxes()
        {
            RemoveExistingDocumentTaxes();
            
            // Calculate and add the sum of distinct totals from line items (except taxes)
            CalculateDistinctTotalsFromLines();
            
            // Sum line taxes and add to document totals
            SumLineItemTaxesToDocumentTotals();

            // Get applicable document-level taxes based on document type and entity group
            var applicableTaxes = _taxRuleEvaluator.GetApplicableDocumentTaxes(_document, _documentTypeCode);
            
            if (!applicableTaxes.Any())
                return;

            // Get the base amount from line totals (excluding existing taxes)
            decimal documentTotalBeforeTax = CalculateDocumentTotalBeforeTax();

            foreach (var tax in applicableTaxes)
            {
                decimal taxAmount = CalculateDocumentTaxAmount(tax, documentTotalBeforeTax);
                
                // Add tax to document totals
                var taxTotal = new TotalDto
                {
                    Oid = Guid.NewGuid(),
                    Concept = $"Tax: {tax.Name} ({tax.Code})",
                    Total = taxAmount
                };
                
                // Add accounting information if available
                ApplyTaxAccountingInfo(taxTotal, tax);
                
                _document.DocumentTotals.Add(taxTotal);
            }
        }

        /// <summary>
        /// Sums all line tax totals and adds them to document totals
        /// </summary>
        private void SumLineItemTaxesToDocumentTotals()
        {
            // Get all lines that are LineDto
            var lines = _document.Lines.OfType<LineDto>().ToList();
            
            if (!lines.Any())
                return;

            // Group line tax totals by concept and sum their values
            var taxTotals = lines
                .SelectMany(l => l.LineTotals)
                // Select only totals that start with "Tax:"
                .Where(t => t.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .GroupBy(t => t.Concept)
                .Select(g => new
                {
                    Concept = g.Key,
                    Total = g.Sum(t => t.Total),
                    // Copy accounting properties from the first total
                    FirstTotal = g.FirstOrDefault()
                })
                .ToList();

            // Add the summed tax totals to the document totals
            foreach (var tax in taxTotals)
            {
                var documentTaxTotal = new TotalDto
                {
                    Oid = Guid.NewGuid(),
                    Concept = tax.Concept,
                    Total = tax.Total
                };
                
                // Copy accounting properties if available
                if (tax.FirstTotal != null && tax.FirstTotal is TotalDto firstTotal)
                {
                    documentTaxTotal.DebitAccountCode = firstTotal.DebitAccountCode;
                    documentTaxTotal.CreditAccountCode = firstTotal.CreditAccountCode;
                    documentTaxTotal.IncludeInTransaction = firstTotal.IncludeInTransaction;
                }
                
                _document.DocumentTotals.Add(documentTaxTotal);
            }
        }

        /// <summary>
        /// Calculates the sum of distinct totals from all document lines and adds them to document totals
        /// </summary>
        private void CalculateDistinctTotalsFromLines()
        {
            // Get all lines that are LineDto
            var lines = _document.Lines.OfType<LineDto>().ToList();
            
            if (!lines.Any())
                return;

            // Group line totals by concept and sum their values (excluding tax totals)
            var distinctTotals = lines
                .SelectMany(l => l.LineTotals)
                // Filter out totals that start with "Tax:" as they are handled separately
                .Where(t => !t.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .GroupBy(t => t.Concept)
                .Select(g => new
                {
                    Concept = g.Key,
                    Total = g.Sum(t => t.Total),
                    // Copy accounting properties from the first total
                    FirstTotal = g.FirstOrDefault()
                })
                .ToList();

            // Add the distinct totals to the document totals
            foreach (var total in distinctTotals)
            {
                var documentTotal = new TotalDto
                {
                    Oid = Guid.NewGuid(),
                    Concept = total.Concept,
                    Total = total.Total
                };
                
                // Copy accounting properties if available
                if (total.FirstTotal != null && total.FirstTotal is TotalDto firstTotal)
                {
                    documentTotal.DebitAccountCode = firstTotal.DebitAccountCode;
                    documentTotal.CreditAccountCode = firstTotal.CreditAccountCode;
                    documentTotal.IncludeInTransaction = firstTotal.IncludeInTransaction;
                }
                
                _document.DocumentTotals.Add(documentTotal);
            }
        }

        /// <summary>
        /// Calculates line-level taxes for a specific line
        /// </summary>
        public void CalculateLineTaxes(LineDto line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));
                
            RemoveExistingLineTaxes(line);
            
            // Get applicable line-level taxes based on document type, entity group, and item group
            var applicableTaxes = _taxRuleEvaluator.GetApplicableLineTaxes(_document, _documentTypeCode, line);
            
            if (!applicableTaxes.Any())
                return;
                
            foreach (var tax in applicableTaxes)
            {
                decimal taxAmount = CalculateLineTaxAmount(tax, line);
                
                // Add tax to line totals
                var taxTotal = new TotalDto
                {
                    Oid = Guid.NewGuid(),
                    Concept = $"Tax: {tax.Name} ({tax.Code})",
                    Total = taxAmount
                };
                
                // Add accounting information if available
                ApplyTaxAccountingInfo(taxTotal, tax);
                
                line.LineTotals.Add(taxTotal);
            }
        }

        /// <summary>
        /// Apply tax accounting info to a total if available
        /// </summary>
        private void ApplyTaxAccountingInfo(TotalDto total, TaxDto tax)
        {
            if (_taxAccountingService != null && _document.DocumentType != null)
            {
                var accountingInfo = _taxAccountingService.GetTaxAccountingInfo(
                    _document.DocumentType.DocumentOperation, 
                    tax.Code);
                    
                if (accountingInfo != null)
                {
                    total.DebitAccountCode = accountingInfo.DebitAccountCode;
                    total.CreditAccountCode = accountingInfo.CreditAccountCode;
                    total.IncludeInTransaction = accountingInfo.IncludeInTransaction;
                }
            }
        }

        /// <summary>
        /// Removes existing tax totals from the document
        /// </summary>
        private void RemoveExistingDocumentTaxes()
        {
            for (int i = _document.DocumentTotals.Count - 1; i >= 0; i--)
            {
                var total = _document.DocumentTotals[i];
                // Remove all existing document totals, as we'll recalculate them all
                _document.DocumentTotals.RemoveAt(i);
            }
        }
        
        /// <summary>
        /// Removes existing tax totals from a line
        /// </summary>
        private void RemoveExistingLineTaxes(LineDto line)
        {
            for (int i = line.LineTotals.Count - 1; i >= 0; i--)
            {
                var total = line.LineTotals[i];
                // Assume tax totals have concept starting with "Tax:"
                if (total.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                {
                    line.LineTotals.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Calculates the document total before tax
        /// </summary>
        private decimal CalculateDocumentTotalBeforeTax()
        {
            decimal total = 0;
            
            // Sum all line amounts
            foreach (var line in _document.Lines)
            {
                total += line.Amount;
            }
            
            return total;
        }

        /// <summary>
        /// Calculates the tax amount based on tax type for document-level taxes
        /// </summary>
        private decimal CalculateDocumentTaxAmount(TaxDto tax, decimal documentTotal)
        {
            switch (tax.TaxType)
            {
                case TaxType.Percentage:
                    return documentTotal * (tax.Percentage / 100m);
                    
                case TaxType.FixedAmount:
                    return tax.Amount;
                    
                case TaxType.AmountPerUnit:
                    // Sum of quantities across all lines
                    decimal totalQuantity = _document.Lines
                        .OfType<LineDto>()
                        .Sum(l => l.Quantity);
                    return tax.Amount * totalQuantity;
                    
                default:
                    return 0;
            }
        }
        
        /// <summary>
        /// Calculates the tax amount based on tax type for line-level taxes
        /// </summary>
        private decimal CalculateLineTaxAmount(TaxDto tax, LineDto line)
        {
            switch (tax.TaxType)
            {
                case TaxType.Percentage:
                    return line.Amount * (tax.Percentage / 100m);
                    
                case TaxType.FixedAmount:
                    return tax.Amount;
                    
                case TaxType.AmountPerUnit:
                    return tax.Amount * line.Quantity;
                    
                default:
                    return 0;
            }
        }
    }
}