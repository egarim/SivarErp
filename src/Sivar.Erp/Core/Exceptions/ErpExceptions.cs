using System;

namespace Sivar.Erp.Core.Exceptions
{
    /// <summary>
    /// Base exception class for ERP-specific exceptions
    /// </summary>
    public class ErpException : Exception
    {
        /// <summary>
        /// Error code for categorizing the exception
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Module where the exception occurred
        /// </summary>
        public string Module { get; }

        public ErpException(string message) : base(message)
        {
            ErrorCode = "ERP_GENERAL";
            Module = "General";
        }

        public ErpException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "ERP_GENERAL";
            Module = "General";
        }

        public ErpException(string message, string errorCode, string module) : base(message)
        {
            ErrorCode = errorCode;
            Module = module;
        }

        public ErpException(string message, string errorCode, string module, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
            Module = module;
        }
    }

    /// <summary>
    /// Exception thrown when a business rule validation fails
    /// </summary>
    public class BusinessRuleException : ErpException
    {
        public BusinessRuleException(string message, string module = "Business") : base(message, "BUSINESS_RULE_VIOLATION", module)
        {
        }

        public BusinessRuleException(string message, string module, Exception innerException) : base(message, "BUSINESS_RULE_VIOLATION", module, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when data validation fails
    /// </summary>
    public class ValidationException : ErpException
    {
        public ValidationException(string message, string module = "Validation") : base(message, "VALIDATION_ERROR", module)
        {
        }

        public ValidationException(string message, string module, Exception innerException) : base(message, "VALIDATION_ERROR", module, innerException)
        {
        }
    }
}