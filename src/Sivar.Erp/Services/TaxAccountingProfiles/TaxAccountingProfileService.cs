using Sivar.Erp.Documents;
using Sivar.Erp.Taxes;
using System;
using System.Collections.Generic;

namespace Sivar.Erp.Services.TaxAccountingProfiles
{
    /// <summary>
    /// Implementation of the tax accounting profile service
    /// </summary>
    public class TaxAccountingProfileService : ITaxAccountingProfileService
    {
        // Dictionary with document category, tax code, and accounting info
        private readonly Dictionary<DocumentOperation, Dictionary<string, TaxAccountingInfo>> _profiles =
            new Dictionary<DocumentOperation, Dictionary<string, TaxAccountingInfo>>();
        
        /// <summary>
        /// Get accounting mapping for a tax in a specific document category
        /// </summary>
        public TaxAccountingInfo GetTaxAccountingInfo(DocumentOperation category, string taxCode)
        {
            if (string.IsNullOrEmpty(taxCode))
                throw new ArgumentNullException(nameof(taxCode));
                
            if (_profiles.TryGetValue(category, out var categoryProfiles))
            {
                if (categoryProfiles.TryGetValue(taxCode, out var accountingInfo))
                {
                    return accountingInfo;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Register a tax accounting profile
        /// </summary>
        public void RegisterTaxAccountingProfile(DocumentOperation category, string taxCode, TaxAccountingInfo accountingInfo)
        {
            if (string.IsNullOrEmpty(taxCode))
                throw new ArgumentNullException(nameof(taxCode));
                
            if (accountingInfo == null)
                throw new ArgumentNullException(nameof(accountingInfo));
                
            if (!_profiles.TryGetValue(category, out var categoryProfiles))
            {
                categoryProfiles = new Dictionary<string, TaxAccountingInfo>(StringComparer.OrdinalIgnoreCase);
                _profiles[category] = categoryProfiles;
            }
            
            categoryProfiles[taxCode] = accountingInfo;
        }
        
        /// <summary>
        /// Get all tax accounting mappings for a document category
        /// </summary>
        public Dictionary<string, TaxAccountingInfo> GetTaxAccountingMapForCategory(DocumentOperation category)
        {
            if (_profiles.TryGetValue(category, out var categoryProfiles))
            {
                // Return a copy of the dictionary to prevent external modification
                return new Dictionary<string, TaxAccountingInfo>(categoryProfiles, StringComparer.OrdinalIgnoreCase);
            }
            
            return new Dictionary<string, TaxAccountingInfo>(StringComparer.OrdinalIgnoreCase);
        }
    }
}