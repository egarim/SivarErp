using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a document total with accounting information
    /// </summary>
    [Description("Document total with accounting information")]
    public interface IDocumentTotal : IEntity
    {
        /// <summary>
        /// Concept or description of the total
        /// </summary>
        [Description("Concept or description of the total")]
        string Concept { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        [Description("Total amount")]
        decimal Total { get; set; }

        /// <summary>
        /// Debit account code for this total
        /// </summary>
        [Description("Debit account code for this total")]
        string? DebitAccountCode { get; set; }

        /// <summary>
        /// Credit account code for this total
        /// </summary>
        [Description("Credit account code for this total")]
        string? CreditAccountCode { get; set; }

        /// <summary>
        /// Indicates if this total should be included in transaction generation
        /// </summary>
        [Description("Indicates if this total should be included in transaction generation")]
        bool IncludeInTransaction { get; set; }
    }
}
