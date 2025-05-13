using System;

namespace Sivar.Erp.FinancialStatements.BalanceAndIncome
{
    /// <summary>
    /// Interface for balance and income line service operations
    /// </summary>
    public interface IBalanceAndIncomeLineService
    {
        /// <summary>
        /// Creates a new line
        /// </summary>
        /// <param name="line">Line to create</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Created line with ID</returns>
        Task<IBalanceAndIncomeLine> CreateLineAsync(IBalanceAndIncomeLine line, string userName);

        /// <summary>
        /// Updates an existing line
        /// </summary>
        /// <param name="line">Line with updated values</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Updated line</returns>
        Task<IBalanceAndIncomeLine> UpdateLineAsync(IBalanceAndIncomeLine line, string userName);

        /// <summary>
        /// Deletes a line
        /// </summary>
        /// <param name="id">Line ID</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteLineAsync(Guid id);

        /// <summary>
        /// Moves a line to a new position in the hierarchy
        /// </summary>
        /// <param name="lineId">ID of line to move</param>
        /// <param name="parentId">ID of new parent (null for root level)</param>
        /// <param name="position">Position within parent's children</param>
        /// <returns>Updated line</returns>
        Task<IBalanceAndIncomeLine> MoveLineAsync(Guid lineId, Guid? parentId, int position);

        /// <summary>
        /// Retrieves lines by type
        /// </summary>
        /// <param name="lineType">Type of lines to retrieve</param>
        /// <returns>Collection of lines</returns>
        Task<IEnumerable<IBalanceAndIncomeLine>> GetLinesByTypeAsync(BalanceIncomeLineType lineType);

        /// <summary>
        /// Retrieves child lines of a parent
        /// </summary>
        /// <param name="parentId">Parent line ID</param>
        /// <returns>Collection of child lines</returns>
        Task<IEnumerable<IBalanceAndIncomeLine>> GetChildLinesAsync(Guid parentId);

        /// <summary>
        /// Retrieves all lines in tree order
        /// </summary>
        /// <returns>All lines ordered by left index</returns>
        Task<IEnumerable<IBalanceAndIncomeLine>> GetAllLinesTreeOrderAsync();

        /// <summary>
        /// Validates a line before create/update
        /// </summary>
        /// <param name="line">Line to validate</param>
        /// <returns>Validation result</returns>
        Task<BalanceLineValidationResult> ValidateLineAsync(IBalanceAndIncomeLine line);

        /// <summary>
        /// Gets the count of accounts assigned to a line
        /// </summary>
        /// <param name="lineId">Line ID</param>
        /// <returns>Number of accounts assigned</returns>
        Task<int> GetAssignedAccountsCountAsync(Guid lineId);
    }
    /// <summary>
    /// Implementation of balance sheet and income statement line
    /// </summary>


    /// <summary>
    /// Abstract base implementation of balance and income line service
    /// </summary>
}