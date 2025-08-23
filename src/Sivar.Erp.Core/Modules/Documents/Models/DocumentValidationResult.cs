using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Documents.Models
{
    /// <summary>
    /// Represents the result of a document validation operation
    /// </summary>
    [Description("Represents the result of a document validation operation")]
    public class DocumentValidationResult : ValidationResult
    {
        /// <summary>
        /// Gets or sets the collection of document-specific validation errors
        /// </summary>
        [Description("Collection of document-specific validation errors")]
        public List<DocumentValidationError> DocumentErrors { get; set; } = new List<DocumentValidationError>();

        /// <summary>
        /// Gets whether there are line-level errors
        /// </summary>
        [Description("Whether there are line-level errors")]
        public bool HasLineErrors => DocumentErrors.Any(e => e.LineNumber.HasValue);

        /// <summary>
        /// Gets whether there are header-level errors
        /// </summary>
        [Description("Whether there are header-level errors")]
        public bool HasHeaderErrors => DocumentErrors.Any(e => !e.LineNumber.HasValue);

        /// <summary>
        /// Adds a document-specific error
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="propertyName">Property name (optional)</param>
        /// <param name="lineNumber">Line number (optional)</param>
        /// <param name="lineProperty">Line property (optional)</param>
        [Description("Adds a document-specific error")]
        public void AddDocumentError(string message, string? propertyName = null, int? lineNumber = null, string? lineProperty = null)
        {
            IsValid = false;
            DocumentErrors.Add(new DocumentValidationError(message, propertyName, lineNumber, lineProperty));
            Errors.Add($"{(lineNumber.HasValue ? $"Line {lineNumber}: " : "")}{message}");
        }

        /// <summary>
        /// Creates a successful document validation result
        /// </summary>
        /// <returns>Successful validation result</returns>
        [Description("Creates a successful document validation result")]
        public static new DocumentValidationResult Success() => new DocumentValidationResult { IsValid = true };

        /// <summary>
        /// Creates a failed document validation result
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        /// <param name="propertyName">Property name (optional)</param>
        /// <returns>Failed validation result</returns>
        [Description("Creates a failed document validation result")]
        public static new DocumentValidationResult Failure(string errorMessage, string? propertyName = null)
        {
            var result = new DocumentValidationResult { IsValid = false };
            result.AddDocumentError(errorMessage, propertyName);
            return result;
        }
    }

    /// <summary>
    /// Represents a document-specific validation error
    /// </summary>
    [Description("Represents a document-specific validation error")]
    public class DocumentValidationError
    {
        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        [Description("Error message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the property name
        /// </summary>
        [Description("Property name")]
        public string? PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the line number
        /// </summary>
        [Description("Line number")]
        public int? LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the line property
        /// </summary>
        [Description("Line property")]
        public string? LineProperty { get; set; }

        /// <summary>
        /// Gets or sets the error code
        /// </summary>
        [Description("Error code")]
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the DocumentValidationError class
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="propertyName">Property name (optional)</param>
        /// <param name="lineNumber">Line number (optional)</param>
        /// <param name="lineProperty">Line property (optional)</param>
        public DocumentValidationError(string message, string? propertyName = null, int? lineNumber = null, string? lineProperty = null)
        {
            Message = message;
            PropertyName = propertyName;
            LineNumber = lineNumber;
            LineProperty = lineProperty;
        }
    }
}
