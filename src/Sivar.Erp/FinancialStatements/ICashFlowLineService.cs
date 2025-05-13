using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.FinancialStatements
{
    #region Service Interfaces

    /// <summary>
    /// Interface for cash flow line service operations
    /// </summary>
    public interface ICashFlowLineService
    {
        /// <summary>
        /// Creates a new cash flow line
        /// </summary>
        /// <param name="line">Line to create</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Created line with ID</returns>
        Task<ICashFlowLine> CreateLineAsync(ICashFlowLine line, string userName);

        /// <summary>
        /// Updates an existing cash flow line
        /// </summary>
        /// <param name="line">Line with updated values</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Updated line</returns>
        Task<ICashFlowLine> UpdateLineAsync(ICashFlowLine line, string userName);

        /// <summary>
        /// Deletes a cash flow line
        /// </summary>
        /// <param name="id">Line ID</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteLineAsync(Guid id);

        /// <summary>
        /// Moves a line in the hierarchy
        /// </summary>
        /// <param name="lineId">Line ID</param>
        /// <param name="parentId">New parent ID</param>
        /// <param name="position">Position within parent</param>
        /// <returns>Updated line</returns>
        Task<ICashFlowLine> MoveLineAsync(Guid lineId, Guid? parentId, int position);

        /// <summary>
        /// Retrieves all cash flow lines
        /// </summary>
        /// <returns>All lines ordered by left index</returns>
        Task<IEnumerable<ICashFlowLine>> GetAllLinesAsync();

        /// <summary>
        /// Retrieves lines by type
        /// </summary>
        /// <param name="lineType">Type of lines to retrieve</param>
        /// <returns>Lines of specified type</returns>
        Task<IEnumerable<ICashFlowLine>> GetLinesByTypeAsync(CashFlowLineType lineType);

        /// <summary>
        /// Retrieves child lines of a parent
        /// </summary>
        /// <param name="parentId">Parent line ID</param>
        /// <returns>Child lines</returns>
        Task<IEnumerable<ICashFlowLine>> GetChildLinesAsync(Guid parentId);

        /// <summary>
        /// Validates a line before create/update
        /// </summary>
        /// <param name="line">Line to validate</param>
        /// <returns>Validation result</returns>
        Task<CashFlowLineValidationResult> ValidateLineAsync(ICashFlowLine line);

        /// <summary>
        /// Gets account assignments for a line
        /// </summary>
        /// <param name="lineId">Line ID</param>
        /// <returns>Account assignments</returns>
        Task<IEnumerable<ICashFlowLineAssignment>> GetAccountAssignmentsAsync(Guid lineId);

        /// <summary>
        /// Assigns an account to a cash flow line
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="lineId">Line ID</param>
        /// <returns>Created assignment</returns>
        Task<ICashFlowLineAssignment> AssignAccountAsync(Guid accountId, Guid lineId);

        /// <summary>
        /// Removes an account assignment
        /// </summary>
        /// <param name="assignmentId">Assignment ID</param>
        /// <returns>True if successful</returns>
        Task<bool> RemoveAccountAssignmentAsync(Guid assignmentId);

        /// <summary>
        /// Gets accounts assigned to a line
        /// </summary>
        /// <param name="lineId">Line ID</param>
        /// <returns>Collection of account IDs</returns>
        Task<IEnumerable<Guid>> GetAssignedAccountsAsync(Guid lineId);

        /// <summary>
        /// Validates account assignment before creation
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="lineId">Line ID</param>
        /// <returns>Validation result</returns>
        Task<bool> ValidateAccountAssignmentAsync(Guid accountId, Guid lineId);
    }
    #endregion
}