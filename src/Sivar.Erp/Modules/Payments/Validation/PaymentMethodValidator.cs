using Sivar.Erp.Modules.Payments.Models;

namespace Sivar.Erp.Modules.Payments.Validation
{
    /// <summary>
    /// Validator for payment method business rules
    /// </summary>
    public class PaymentMethodValidator
    {
        /// <summary>
        /// Validates a payment method DTO
        /// </summary>
        /// <param name="paymentMethod">Payment method to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidatePaymentMethod(PaymentMethodDto paymentMethod)
        {
            if (paymentMethod == null)
                return false;

            // Required field validation
            if (string.IsNullOrWhiteSpace(paymentMethod.Code))
                return false;

            if (string.IsNullOrWhiteSpace(paymentMethod.Name))
                return false;

            // Code format validation (alphanumeric, underscores, max 50 chars)
            if (paymentMethod.Code.Length > 50)
                return false;

            if (!IsValidCode(paymentMethod.Code))
                return false;

            // Name length validation
            if (paymentMethod.Name.Length > 200)
                return false;

            // AccountCode validation (if provided)
            if (!string.IsNullOrWhiteSpace(paymentMethod.AccountCode) && paymentMethod.AccountCode.Length > 50)
                return false;

            // Business rule validation
            if (paymentMethod.RequiresBankAccount && paymentMethod.Type == PaymentMethodType.Cash)
                return false; // Cash shouldn't require bank account

            return true;
        }

        /// <summary>
        /// Validates a payment method interface
        /// </summary>
        /// <param name="paymentMethod">Payment method to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidatePaymentMethod(IPaymentMethod paymentMethod)
        {
            if (paymentMethod == null)
                return false;

            // Required field validation
            if (string.IsNullOrWhiteSpace(paymentMethod.Code))
                return false;

            if (string.IsNullOrWhiteSpace(paymentMethod.Name))
                return false;

            // Code format validation (alphanumeric, underscores, max 50 chars)
            if (paymentMethod.Code.Length > 50)
                return false;

            if (!IsValidCode(paymentMethod.Code))
                return false;

            // Name length validation
            if (paymentMethod.Name.Length > 200)
                return false;

            // AccountCode validation (if provided)
            if (!string.IsNullOrWhiteSpace(paymentMethod.AccountCode) && paymentMethod.AccountCode.Length > 50)
                return false;

            // Business rule validation
            if (paymentMethod.RequiresBankAccount && paymentMethod.Type == PaymentMethodType.Cash)
                return false; // Cash shouldn't require bank account

            return true;
        }

        /// <summary>
        /// Validates that a code contains only valid characters
        /// </summary>
        /// <param name="code">Code to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool IsValidCode(string code)
        {
            // Allow alphanumeric characters and underscores
            return code.All(c => char.IsLetterOrDigit(c) || c == '_');
        }
    }
}
