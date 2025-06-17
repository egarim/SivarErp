using System;

namespace Sivar.Erp.Services.Taxes.TaxRule
{
    /// <summary>
    /// Validates tax rules before import or update
    /// </summary>
    public class TaxRuleValidator
    {
        /// <summary>
        /// Validates a tax rule
        /// </summary>
        /// <param name="taxRule">Tax rule to validate</param>
        /// <returns>True if the tax rule is valid, false otherwise</returns>
        public bool ValidateTaxRule(ITaxRule taxRule)
        {
            if (taxRule == null)
            {
                return false;
            }

            // Tax ID must be provided
            if (taxRule.TaxId == string.Empty)
            {
                return false;
            }

            // Priority must be a positive integer
            if (taxRule.Priority < 0)
            {
                return false;
            }

            // At least one of the filter criteria should be defined (DocumentOperation, BusinessEntityGroupId, ItemGroupId)
            // Otherwise the rule would apply to all documents, entities and items which would be too broad
            bool hasFilter = taxRule.DocumentOperation.HasValue ||
                             !string.IsNullOrEmpty(taxRule.BusinessEntityGroupId) || 
                             !string.IsNullOrEmpty(taxRule.ItemGroupId);

            return hasFilter;
        }
    }
}