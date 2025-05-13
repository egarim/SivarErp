using System;

namespace Sivar.Erp.FinancialStatements.BalanceAndIncome
{
    /// <summary>
    /// Validation result for balance and income lines
    /// </summary>
    public class BalanceLineValidationResult
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
        public static BalanceLineValidationResult Success()
        {
            return new BalanceLineValidationResult { IsValid = true };
        }

        /// <summary>
        /// Creates a failed validation result with errors
        /// </summary>
        /// <param name="errors">Error messages</param>
        /// <returns>Invalid result</returns>
        public static BalanceLineValidationResult Failure(params string[] errors)
        {
            return new BalanceLineValidationResult
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
    /// <summary>
    /// Abstract base implementation of balance and income line service
    /// </summary>
}