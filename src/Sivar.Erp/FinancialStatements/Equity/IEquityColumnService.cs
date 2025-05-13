using System;

namespace Sivar.Erp.FinancialStatements.Equity
{
    /// <summary>
    /// Interface for equity column service operations
    /// </summary>
    public interface IEquityColumnService
    {
        /// <summary>
        /// Creates a new equity column
        /// </summary>
        /// <param name="column">Column to create</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Created column with ID</returns>
        Task<IEquityColumn> CreateColumnAsync(IEquityColumn column, string userName);

        /// <summary>
        /// Updates an existing equity column
        /// </summary>
        /// <param name="column">Column with updated values</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Updated column</returns>
        Task<IEquityColumn> UpdateColumnAsync(IEquityColumn column, string userName);

        /// <summary>
        /// Deletes an equity column
        /// </summary>
        /// <param name="id">Column ID</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteColumnAsync(Guid id);

        /// <summary>
        /// Retrieves all equity columns
        /// </summary>
        /// <returns>All equity columns ordered by visible index</returns>
        Task<IEnumerable<IEquityColumn>> GetAllColumnsAsync();

        /// <summary>
        /// Validates an equity column before create/update
        /// </summary>
        /// <param name="column">Column to validate</param>
        /// <returns>Validation result</returns>
        Task<EquityColumnValidationResult> ValidateColumnAsync(IEquityColumn column);

        /// <summary>
        /// Reorders equity columns
        /// </summary>
        /// <param name="columnIds">Column IDs in new order</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>True if successful</returns>
        Task<bool> ReorderColumnsAsync(IEnumerable<Guid> columnIds, string userName);

        /// <summary>
        /// Assigns accounts to a column
        /// </summary>
        /// <param name="columnId">Column ID</param>
        /// <param name="accountIds">Account IDs to assign</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>True if successful</returns>
        Task<bool> AssignAccountsToColumnAsync(Guid columnId, IEnumerable<Guid> accountIds, string userName);
    }
    /// <summary>
    /// Validation result for equity lines
    /// </summary>


    /// <summary>
    /// Builder class for creating equity statement structure
    /// </summary>
}