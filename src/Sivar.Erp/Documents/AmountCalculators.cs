using System;
using System.Linq;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Common amount calculators for transaction entries
    /// </summary>
    public static class AmountCalculators
    {
        /// <summary>
        /// Gets the total of all document lines (sum of line amounts)
        /// </summary>
        public static decimal LineTotal(DocumentDto document)
        {
            return document.Lines.Sum(line => line.Amount);
        }
        
        /// <summary>
        /// Gets the subtotal amount (excluding taxes)
        /// </summary>
        public static decimal Subtotal(DocumentDto document)
        {
            return document.DocumentTotals
                .Where(t => !t.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .Sum(t => t.Total);
        }
        
        /// <summary>
        /// Gets the tax total amount
        /// </summary>
        public static decimal TaxTotal(DocumentDto document)
        {
            return document.DocumentTotals
                .Where(t => t.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .Sum(t => t.Total);
        }
        
        /// <summary>
        /// Gets the grand total amount (including taxes)
        /// </summary>
        public static decimal GrandTotal(DocumentDto document)
        {
            return document.DocumentTotals.Sum(t => t.Total);
        }
        
        /// <summary>
        /// Gets the total amount for a specific concept from document totals
        /// </summary>
        public static Func<DocumentDto, decimal> ForConcept(string concept)
        {
            return document => document.DocumentTotals
                .Where(t => t.Concept == concept)
                .Sum(t => t.Total);
        }
        
        /// <summary>
        /// Gets the total amount for all concepts that start with a specific prefix
        /// </summary>
        public static Func<DocumentDto, decimal> ForConceptStartingWith(string conceptPrefix)
        {
            return document => document.DocumentTotals
                .Where(t => t.Concept.StartsWith(conceptPrefix, StringComparison.OrdinalIgnoreCase))
                .Sum(t => t.Total);
        }
        
        /// <summary>
        /// Returns a fixed amount calculator
        /// </summary>
        public static Func<DocumentDto, decimal> FixedAmount(decimal amount)
        {
            return _ => amount;
        }
        
        /// <summary>
        /// Calculates a percentage of another calculator's result
        /// </summary>
        public static Func<DocumentDto, decimal> Percentage(Func<DocumentDto, decimal> baseCalculator, decimal percentage)
        {
            return document => baseCalculator(document) * (percentage / 100m);
        }
        
        /// <summary>
        /// Calculates the cost of goods sold by taking a percentage of the subtotal
        /// This is a placeholder for a real COGS calculation that would use actual inventory costs
        /// </summary>
        public static Func<DocumentDto, decimal> EstimatedCostOfGoodsSold(decimal costPercentage = 60m)
        {
            return document => Math.Round(Subtotal(document) * (costPercentage / 100m), 2);
        }
    }
}