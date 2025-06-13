using System;
using System.Collections.Generic;

namespace Sivar.Erp.Documents.Tax
{
    /// <summary>
    /// Service for managing tax accounting profiles
    /// </summary>
    public interface ITaxAccountingProfileService
    {
        /// <summary>
        /// Get accounting mapping for a tax in a specific document category
        /// </summary>
        /// <param name="category">Document category</param>
        /// <param name="taxCode">Tax code</param>
        /// <returns>Tax accounting information or null if not found</returns>
        TaxAccountingInfo GetTaxAccountingInfo(DocumentCategory category, string taxCode);
        
        /// <summary>
        /// Register a tax accounting profile
        /// </summary>
        /// <param name="category">Document category</param>
        /// <param name="taxCode">Tax code</param>
        /// <param name="accountingInfo">Accounting information</param>
        void RegisterTaxAccountingProfile(DocumentCategory category, string taxCode, TaxAccountingInfo accountingInfo);
        
        /// <summary>
        /// Get all tax accounting mappings for a document category
        /// </summary>
        /// <param name="category">Document category</param>
        /// <returns>Dictionary mapping tax codes to accounting information</returns>
        Dictionary<string, TaxAccountingInfo> GetTaxAccountingMapForCategory(DocumentCategory category);
    }
}