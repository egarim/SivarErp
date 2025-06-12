using System;
using System.Collections.Generic;
using System.Linq;
using Sivar.Erp.Documents.Tax;
using Sivar.Erp.Taxes.TaxRule;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Handles tax calculations at the document level
    /// </summary>
    public class DocumentTaxCalculator
    {
        private readonly DocumentDto _document;
        private readonly string _documentTypeCode;
        private readonly TaxRuleEvaluator _taxRuleEvaluator;

        /// <summary>
        /// Initializes a new instance of the DocumentTaxCalculator
        /// </summary>
        /// <param name="document">The document to calculate taxes for</param>
        /// <param name="documentTypeCode">The document type code to use for tax rule evaluation</param>
        /// <param name="taxRuleEvaluator">The evaluator for determining applicable taxes</param>
        public DocumentTaxCalculator(
            DocumentDto document, 
            string documentTypeCode, 
            TaxRuleEvaluator taxRuleEvaluator)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _documentTypeCode = documentTypeCode ?? throw new ArgumentNullException(nameof(documentTypeCode));
            _taxRuleEvaluator = taxRuleEvaluator ?? throw new ArgumentNullException(nameof(taxRuleEvaluator));
        }

        /// <summary>
        /// Calculates document-level taxes and updates the document totals
        /// </summary>
        public void CalculateDocumentTaxes()
        {
            RemoveExistingDocumentTaxes();

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
                
                _document.DocumentTotals.Add(taxTotal);
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
                
                line.LineTotals.Add(taxTotal);
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
                // Assume tax totals have concept starting with "Tax:"
                if (total.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                {
                    _document.DocumentTotals.RemoveAt(i);
                }
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