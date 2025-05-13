using System;

namespace Sivar.Erp.FinancialStatements.Equity
{
    /// <summary>
    /// Interface for equity line service operations
    /// </summary>
    public interface IEquityLineService
    {
        /// <summary>
        /// Creates a new equity line
        /// </summary>
        /// <param name="line">Line to create</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Created line with ID</returns>
        Task<IEquityLine> CreateLineAsync(IEquityLine line, string userName);

        /// <summary>
        /// Updates an existing equity line
        /// </summary>
        /// <param name="line">Line with updated values</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Updated line</returns>
        Task<IEquityLine> UpdateLineAsync(IEquityLine line, string userName);

        /// <summary>
        /// Deletes an equity line
        /// </summary>
        /// <param name="id">Line ID</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteLineAsync(Guid id);

        /// <summary>
        /// Retrieves all equity lines
        /// </summary>
        /// <returns>All equity lines ordered by visible index</returns>
        Task<IEnumerable<IEquityLine>> GetAllLinesAsync();

        /// <summary>
        /// Retrieves equity lines by type
        /// </summary>
        /// <param name="lineType">Type of lines to retrieve</param>
        /// <returns>Lines of specified type</returns>
        Task<IEnumerable<IEquityLine>> GetLinesByTypeAsync(EquityLineType lineType);

        /// <summary>
        /// Validates an equity line before create/update
        /// </summary>
        /// <param name="line">Line to validate</param>
        /// <returns>Validation result</returns>
        Task<EquityLineValidationResult> ValidateLineAsync(IEquityLine line);

        /// <summary>
        /// Reorders equity lines
        /// </summary>
        /// <param name="lineIds">Line IDs in new order</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>True if successful</returns>
        Task<bool> ReorderLinesAsync(IEnumerable<Guid> lineIds, string userName);
    }
    /// <summary>
    /// Validation result for equity lines
    /// </summary>


    /// <summary>
    /// Builder class for creating equity statement structure
    /// </summary>
}