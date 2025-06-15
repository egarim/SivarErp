using System;
using System.Linq;

namespace Sivar.Erp.Documents
{
    public interface ITotal
    {
        Guid Oid { get; set; }
        string Concept { get; set; }
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
