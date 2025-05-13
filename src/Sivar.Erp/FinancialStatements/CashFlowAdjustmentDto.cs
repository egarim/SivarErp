using Sivar.Erp.Documents;
using System;

namespace Sivar.Erp.FinancialStatements
{
    /// <summary>
    /// Implementation of cash flow adjustment
    /// </summary>
    public class CashFlowAdjustmentDto : ICashFlowAdjustment
    {
        /// <summary>
        /// Unique identifier for the adjustment
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to the transaction being adjusted
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Reference to the account being adjusted
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Type of adjustment (debit or credit)
        /// </summary>
        public EntryType EntryType { get; set; }

        /// <summary>
        /// Amount of the adjustment
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Validates the adjustment
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // All required fields must be set
            if (TransactionId == Guid.Empty || AccountId == Guid.Empty)
            {
                return false;
            }

            // Amount must be positive
            if (Amount <= 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates the effective amount based on entry type
        /// </summary>
        /// <returns>Positive for debit, negative for credit</returns>
        public decimal GetEffectiveAmount()
        {
            return EntryType == EntryType.Debit ? Amount : -Amount;
        }
    }
    /// <summary>
    /// Validation result for cash flow lines
    /// </summary>
}