using System;

namespace Sivar.Erp.FinancialStatements.Equity
{
    /// <summary>
    /// Validation result for equity lines
    /// </summary>
    public class EquityLineValidationResult
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
        public static EquityLineValidationResult Success()
        {
            return new EquityLineValidationResult { IsValid = true };
        }

        /// <summary>
        /// Creates a failed validation result with errors
        /// </summary>
        /// <param name="errors">Error messages</param>
        /// <returns>Invalid result</returns>
        public static EquityLineValidationResult Failure(params string[] errors)
        {
            return new EquityLineValidationResult
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
    /// Builder class for creating equity statement structure
    /// </summary>
}