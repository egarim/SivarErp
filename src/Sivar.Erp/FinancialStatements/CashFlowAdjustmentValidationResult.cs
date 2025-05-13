using System;

namespace Sivar.Erp.FinancialStatements
{
    /// <summary>
    /// Validation result for cash flow adjustments
    /// </summary>
    public class CashFlowAdjustmentValidationResult
    {
        /// <summary>
        /// Whether the validation passed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Collection of validation error messages
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Collection of validation warning messages
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        /// <returns>Valid result</returns>
        public static CashFlowAdjustmentValidationResult Success()
        {
            return new CashFlowAdjustmentValidationResult { IsValid = true };
        }

        /// <summary>
        /// Creates a failed validation result with errors
        /// </summary>
        /// <param name="errors">Error messages</param>
        /// <returns>Invalid result</returns>
        public static CashFlowAdjustmentValidationResult Failure(params string[] errors)
        {
            return new CashFlowAdjustmentValidationResult
            {
                IsValid = false,
                Errors = new List<string>(errors)
            };
        }

        /// <summary>
        /// Adds an error to the validation result
        /// </summary>
        /// <param name="error">Error message</param>
        public void AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
        }

        /// <summary>
        /// Adds a warning to the validation result
        /// </summary>
        /// <param name="warning">Warning message</param>
        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }
    }
}