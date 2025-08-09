using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Domain
{
    /// <summary>
    /// Represents the result of a validation operation
    /// </summary>
    [Description("Represents the result of a validation operation")]
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets whether the validation was successful
        /// </summary>
        [Description("Whether the validation was successful")]
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the collection of validation errors
        /// </summary>
        [Description("Collection of validation errors")]
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the collection of validation warnings
        /// </summary>
        [Description("Collection of validation warnings")]
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        /// <returns>A validation result indicating success</returns>
        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Creates a failed validation result with the specified error
        /// </summary>
        /// <param name="error">The validation error</param>
        /// <returns>A validation result indicating failure</returns>
        public static ValidationResult Failure(string error)
        {
            return new ValidationResult 
            { 
                IsValid = false, 
                Errors = new List<string> { error } 
            };
        }

        /// <summary>
        /// Creates a failed validation result with multiple errors
        /// </summary>
        /// <param name="errors">The validation errors</param>
        /// <returns>A validation result indicating failure</returns>
        public static ValidationResult Failure(IEnumerable<string> errors)
        {
            return new ValidationResult 
            { 
                IsValid = false, 
                Errors = errors.ToList() 
            };
        }
    }
}