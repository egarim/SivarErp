using System;

namespace Sivar.Erp.Modules.Documents.Core.Entities
{
    /// <summary>
    /// Interface for document total entities
    /// </summary>
    public interface ITotal
    {
        /// <summary>
        /// Unique identifier for the total
        /// </summary>
        Guid Oid { get; set; }

        /// <summary>
        /// Description of what this total represents
        /// </summary>
        string Concept { get; set; }

        /// <summary>
        /// The calculated total amount
        /// </summary>
        decimal Total { get; set; }

        /// <summary>
        /// Account code to post to when this total represents a debit amount
        /// </summary>
        string DebitAccountCode { get; set; }
        
        /// <summary>
        /// Account code to post to when this total represents a credit amount
        /// </summary>
        string CreditAccountCode { get; set; }
        
        /// <summary>
        /// Indicates if this total should be included in transaction generation
        /// </summary>
        bool IncludeInTransaction { get; set; }
    }
}
