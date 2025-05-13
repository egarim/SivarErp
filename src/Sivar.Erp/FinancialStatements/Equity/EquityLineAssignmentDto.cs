using Sivar.Erp.Documents;
using System;

namespace Sivar.Erp.FinancialStatements.Equity
{
    /// <summary>
    /// Implementation of equity line assignment to document types
    /// </summary>
    public class EquityLineAssignmentDto : IEquityLineAssignment
    {
        /// <summary>
        /// Unique identifier for the assignment
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to the equity line
        /// </summary>
        public Guid EquityLineId { get; set; }

        /// <summary>
        /// Base document type that this line is assigned to
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Extended document type ID (if from extension)
        /// </summary>
        public Guid? ExtendedDocumentTypeId { get; set; }

        /// <summary>
        /// Validates the equity line assignment
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // Equity line ID must be set
            if (EquityLineId == Guid.Empty)
            {
                return false;
            }

            // Document type must be specified
            if (DocumentType == 0 && ExtendedDocumentTypeId == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if this assignment uses an extended document type
        /// </summary>
        /// <returns>True if extended document type is used</returns>
        public bool UsesExtendedDocumentType()
        {
            return ExtendedDocumentTypeId.HasValue;
        }
    }
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