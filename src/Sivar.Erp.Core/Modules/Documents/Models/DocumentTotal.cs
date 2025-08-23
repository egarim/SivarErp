using System;
using System.ComponentModel;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Document total implementation for accounting calculations
    /// </summary>
    [Description("Document total implementation for accounting calculations")]
    public class DocumentTotal : IDocumentTotal
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Document ID this total belongs to
        /// </summary>
        [Description("Document ID this total belongs to")]
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Concept or description of the total
        /// </summary>
        [Description("Concept or description of the total")]
        public string Concept { get; set; } = string.Empty;

        /// <summary>
        /// Total amount
        /// </summary>
        [Description("Total amount")]
        public decimal Total { get; set; }

        /// <summary>
        /// Debit account code for this total
        /// </summary>
        [Description("Debit account code for this total")]
        public string? DebitAccountCode { get; set; }

        /// <summary>
        /// Credit account code for this total
        /// </summary>
        [Description("Credit account code for this total")]
        public string? CreditAccountCode { get; set; }

        /// <summary>
        /// Indicates if this total should be included in transaction generation
        /// </summary>
        [Description("Indicates if this total should be included in transaction generation")]
        public bool IncludeInTransaction { get; set; } = true;

        /// <summary>
        /// Tax rate applied (if applicable)
        /// </summary>
        [Description("Tax rate applied (if applicable)")]
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Currency code for the total
        /// </summary>
        [Description("Currency code for the total")]
        public string CurrencyCode { get; set; } = "USD";

        /// <summary>
        /// Order in which this total appears
        /// </summary>
        [Description("Order in which this total appears")]
        public int DisplayOrder { get; set; }
    }
}
