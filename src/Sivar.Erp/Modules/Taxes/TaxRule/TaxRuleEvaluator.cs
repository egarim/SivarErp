using System;
using System.Collections.Generic;
using System.Linq;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.Taxes;
using Sivar.Erp.Services.Taxes.TaxGroup;

namespace Sivar.Erp.Services.Taxes.TaxRule
{
    /// <summary>
    /// Evaluates tax rules against documents to determine applicable taxes
    /// </summary>
    public class TaxRuleEvaluator
    {
        private readonly IList<TaxRuleDto> _taxRules;
        private readonly IList<TaxDto> _availableTaxes;
        private readonly IList<GroupMembershipDto> _groupMemberships;

        public TaxRuleEvaluator(
            IList<TaxRuleDto> taxRules,
            IList<TaxDto> availableTaxes,
            IList<GroupMembershipDto> groupMemberships)
        {
            _taxRules = taxRules;
            _availableTaxes = availableTaxes;
            _groupMemberships = groupMemberships;
        }

        /// <summary>
        /// Gets applicable document-level taxes based on document type and business entity
        /// </summary>
        public IList<TaxDto> GetApplicableDocumentTaxes(DocumentDto document, string documentTypeCode)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            // Get the business entity ID
            var businessEntityId = document.BusinessEntity?.Code;

            if (!string.IsNullOrEmpty(businessEntityId))
                return new List<TaxDto>();

            // Get all business entity groups that this entity belongs to
            var entityGroupIds = GetGroupsForEntity(businessEntityId, GroupType.BusinessEntity);

            // Find applicable document-level taxes
            return GetApplicableTaxes(document.DocumentType.DocumentOperation, entityGroupIds, new List<string>())
                .Where(tax => tax.ApplicationLevel == TaxApplicationLevel.Document)
                .ToList();
        }

        /// <summary>
        /// Gets applicable line-level taxes based on document type, business entity, and item
        /// </summary>
        public IList<TaxDto> GetApplicableLineTaxes(DocumentDto document, string documentTypeCode, LineDto line)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            // Get the business entity and item IDs
            var businessEntityId = document.BusinessEntity.Code;
            var itemId = line.Item?.Code;

            if (!string.IsNullOrEmpty(document.BusinessEntity.Code) || !string.IsNullOrEmpty(itemId))
                return new List<TaxDto>();

            // Get the groups these entities belong to
            var entityGroupIds = GetGroupsForEntity(businessEntityId, GroupType.BusinessEntity);
            var itemGroupIds = GetGroupsForEntity(itemId, GroupType.Item);

            // Find applicable line-level taxes
            return GetApplicableTaxes(document.DocumentType.DocumentOperation, entityGroupIds, itemGroupIds)
                .Where(tax => tax.ApplicationLevel == TaxApplicationLevel.Line)
                .ToList();
        }

        /// <summary>
        /// Gets all groups that an entity belongs to
        /// </summary>
        private IList<string> GetGroupsForEntity(string entityId, GroupType groupType)
        {
            return _groupMemberships
                .Where(m => m.EntityId == entityId && m.GroupType == groupType)
                .Select(m => m.GroupId)
                .ToList();
        }

        /// <summary>
        /// Gets taxes that apply based on document type, entity groups, and item groups
        /// </summary>
        private IEnumerable<TaxDto> GetApplicableTaxes(
            DocumentOperation documentOperation,
            IList<string> entityGroupIds,
            IList<string> itemGroupIds)
        {
            // Dictionary to track final decision for each tax (whether it should be applied)
            var taxDecisions = new Dictionary<string, bool>();

            // Get rules that match our criteria, ordered by priority (lower number = higher priority)
            var matchingRules = _taxRules
                .Where(rule => rule.DocumentOperation == documentOperation)
                .Where(rule =>
                    string.IsNullOrEmpty(rule.BusinessEntityGroupId) ||
                    entityGroupIds.Contains(rule.BusinessEntityGroupId))
                .Where(rule =>
                    string.IsNullOrEmpty(rule.ItemGroupId) ||
                    itemGroupIds.Contains(rule.ItemGroupId))
                .OrderBy(rule => rule.Priority)
                .ToList();

            // Process rules in priority order to make final decisions
            foreach (var rule in matchingRules)
            {
                // The most specific rule (with matching ItemGroupId) for each tax wins
                if (!taxDecisions.ContainsKey(rule.TaxId) || !string.IsNullOrEmpty(rule.ItemGroupId))
                {
                    // If the rule is disabled, it means we should NOT apply the tax
                    taxDecisions[rule.TaxId] = rule.IsEnabled;
                }
            }

            // Return only enabled taxes that have a positive decision
            return _availableTaxes
                .Where(tax => tax.IsEnabled &&
                           taxDecisions.ContainsKey(tax.Code) &&
                           taxDecisions[tax.Code])
                .ToList();
        }
    }
}