using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents.Models
{
    /// <summary>
    /// Implementation of the ITotal interface
    /// </summary>
    public class TotalDto : Entity, ITotal
    {
        /// <summary>
        /// Gets or sets the concept name
        /// </summary>
        public string Concept { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the total amount
        /// </summary>
        public decimal Total { get; set; }
        
        /// <summary>
        /// Gets or sets whether this total is included in the document total
        /// </summary>
        public bool IncludeInDocumentTotal { get; set; } = true;

        /// <summary>
        /// Gets or sets the debit account code
        /// </summary>
        public string? DebitAccountCode { get; set; }
        
        /// <summary>
        /// Gets or sets the credit account code
        /// </summary>
        public string? CreditAccountCode { get; set; }
        
        /// <summary>
        /// Gets or sets whether to include this total in transaction generation
        /// </summary>
        public bool IncludeInTransaction { get; set; } = true;
    }
}