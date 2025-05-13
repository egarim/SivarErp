using Sivar.Erp.Documents;
using System;

namespace Sivar.Erp.FinancialStatements.Equity
{
    /// <summary>
    /// Interface for equity line assignment to document types
    /// </summary>
    public interface IEquityLineAssignment : IEntity
    {
        /// <summary>
        /// Reference to the equity line
        /// </summary>
        Guid EquityLineId { get; set; }

        /// <summary>
        /// Base document type that this line is assigned to
        /// </summary>
        DocumentType DocumentType { get; set; }

        /// <summary>
        /// Extended document type ID (if from extension)
        /// </summary>
        Guid? ExtendedDocumentTypeId { get; set; }
    }
    /// <summary>
    /// Implementation of equity line entity
    /// </summary>


    /// <summary>
    /// Interface for equity line service operations
    /// </summary>


    /// <summary>
    /// Validation result for equity lines
    /// </summary>


    /// <summary>
    /// Builder class for creating equity statement structure
    /// </summary>
}