using System.ComponentModel;
using Sivar.Erp.Core.Modules.Documents;
using Sivar.Erp.Core.Modules.Taxes.Models;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Service for tax calculations and management
    /// </summary>
    [Description("Service for tax calculations and management")]
    public interface ITaxService
    {
        /// <summary>
        /// Calculates applicable taxes for a document
        /// </summary>
        /// <param name="document">The document to calculate taxes for</param>
        /// <param name="operation">The operation being performed</param>
        /// <returns>A collection of applicable taxes</returns>
        [Description("Calculates applicable taxes for a document")]
        Task<IEnumerable<ITax>> GetApplicableTaxesAsync(IDocument document, DocumentOperation operation);
        
        /// <summary>
        /// Applies tax rules to document lines
        /// </summary>
        /// <param name="document">The document to apply tax rules to</param>
        [Description("Applies tax rules to document lines")]
        Task ApplyTaxRulesToDocumentAsync(IDocument document);
    }
    
    /// <summary>
    /// Document operations for tax purposes
    /// </summary>
    public enum DocumentOperation
    {
        /// <summary>
        /// Sales operation
        /// </summary>
        Sales,
        
        /// <summary>
        /// Purchase operation
        /// </summary>
        Purchase,
        
        /// <summary>
        /// Return operation
        /// </summary>
        Return,
        
        /// <summary>
        /// Internal transfer
        /// </summary>
        Transfer
    }
}