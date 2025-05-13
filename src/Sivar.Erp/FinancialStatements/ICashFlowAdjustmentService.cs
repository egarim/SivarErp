using System;

namespace Sivar.Erp.FinancialStatements
{
    /// <summary>
    /// Interface for cash flow adjustment service operations
    /// </summary>
    public interface ICashFlowAdjustmentService
    {
        /// <summary>
        /// Creates a new cash flow adjustment
        /// </summary>
        /// <param name="adjustment">Adjustment to create</param>
        /// <returns>Created adjustment with ID</returns>
        Task<ICashFlowAdjustment> CreateAdjustmentAsync(ICashFlowAdjustment adjustment);

        /// <summary>
        /// Updates an existing adjustment
        /// </summary>
        /// <param name="adjustment">Adjustment with updated values</param>
        /// <returns>Updated adjustment</returns>
        Task<ICashFlowAdjustment> UpdateAdjustmentAsync(ICashFlowAdjustment adjustment);

        /// <summary>
        /// Deletes an adjustment
        /// </summary>
        /// <param name="id">Adjustment ID</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteAdjustmentAsync(Guid id);

        /// <summary>
        /// Retrieves an adjustment by ID
        /// </summary>
        /// <param name="id">Adjustment ID</param>
        /// <returns>Adjustment if found</returns>
        Task<ICashFlowAdjustment?> GetAdjustmentByIdAsync(Guid id);

        /// <summary>
        /// Retrieves adjustments by transaction
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <returns>Adjustments for the transaction</returns>
        Task<IEnumerable<ICashFlowAdjustment>> GetAdjustmentsByTransactionAsync(Guid transactionId);

        /// <summary>
        /// Retrieves adjustments by account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Adjustments for the account</returns>
        Task<IEnumerable<ICashFlowAdjustment>> GetAdjustmentsByAccountAsync(Guid accountId);

        /// <summary>
        /// Retrieves adjustments by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Adjustments within the date range</returns>
        Task<IEnumerable<ICashFlowAdjustment>> GetAdjustmentsByDateRangeAsync(DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Validates an adjustment before create/update
        /// </summary>
        /// <param name="adjustment">Adjustment to validate</param>
        /// <returns>Validation result</returns>
        Task<CashFlowAdjustmentValidationResult> ValidateAdjustmentAsync(ICashFlowAdjustment adjustment);

        /// <summary>
        /// Gets total adjustments for an account within a date range
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Total adjustment amount</returns>
        Task<decimal> GetTotalAdjustmentsAsync(Guid accountId, DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Checks if a transaction exists for an adjustment
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <returns>True if transaction exists</returns>
        Task<bool> TransactionExistsAsync(Guid transactionId);

        /// <summary>
        /// Checks if an account exists for an adjustment
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>True if account exists</returns>
        Task<bool> AccountExistsAsync(Guid accountId);
    }
}