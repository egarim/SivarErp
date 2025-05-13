using Sivar.Erp.Documents;
using System;

namespace Sivar.Erp.FinancialStatements.Equity
{
    /// <summary>
    /// Interface for equity line assignment service operations
    /// </summary>
    public interface IEquityLineAssignmentService
    {
        /// <summary>
        /// Creates a new equity line assignment
        /// </summary>
        /// <param name="assignment">Assignment to create</param>
        /// <returns>Created assignment with ID</returns>
        Task<IEquityLineAssignment> CreateAssignmentAsync(IEquityLineAssignment assignment);

        /// <summary>
        /// Updates an existing assignment
        /// </summary>
        /// <param name="assignment">Assignment with updated values</param>
        /// <returns>Updated assignment</returns>
        Task<IEquityLineAssignment> UpdateAssignmentAsync(IEquityLineAssignment assignment);

        /// <summary>
        /// Deletes an assignment
        /// </summary>
        /// <param name="id">Assignment ID</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteAssignmentAsync(Guid id);

        /// <summary>
        /// Retrieves assignments by equity line
        /// </summary>
        /// <param name="equityLineId">Equity line ID</param>
        /// <returns>Assignments for the line</returns>
        Task<IEnumerable<IEquityLineAssignment>> GetAssignmentsByLineAsync(Guid equityLineId);

        /// <summary>
        /// Retrieves assignments by document type
        /// </summary>
        /// <param name="documentType">Document type</param>
        /// <returns>Assignments for the document type</returns>
        Task<IEnumerable<IEquityLineAssignment>> GetAssignmentsByDocumentTypeAsync(DocumentType documentType);

        /// <summary>
        /// Validates an assignment before create/update
        /// </summary>
        /// <param name="assignment">Assignment to validate</param>
        /// <returns>Validation result</returns>
        Task<EquityAssignmentValidationResult> ValidateAssignmentAsync(IEquityLineAssignment assignment);

        /// <summary>
        /// Checks if a document type is assigned to any line
        /// </summary>
        /// <param name="documentType">Document type</param>
        /// <returns>True if assigned</returns>
        Task<bool> IsDocumentTypeAssignedAsync(DocumentType documentType);

        /// <summary>
        /// Gets the equity line for a document type
        /// </summary>
        /// <param name="documentType">Document type</param>
        /// <param name="extendedDocumentTypeId">Extended document type ID</param>
        /// <returns>Equity line if found</returns>
        Task<IEquityLine?> GetLineForDocumentTypeAsync(DocumentType documentType, Guid? extendedDocumentTypeId = null);
    }
    /// <summary>
    /// Validation result for equity lines
    /// </summary>


    /// <summary>
    /// Builder class for creating equity statement structure
    /// </summary>
}