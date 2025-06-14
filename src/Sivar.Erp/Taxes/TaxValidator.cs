using Sivar.Erp.Documents.Tax;
using System;

namespace Sivar.Erp.Taxes
{
    /// <summary>
    /// Validator for tax data
    /// </summary>
    public class TaxValidator
    {
        /// <summary>
        /// Validates a tax entity
        /// </summary>
        /// <param name="tax">The tax to validate</param>
        /// <returns>True if tax is valid, false otherwise</returns>
        public bool ValidateTax(TaxDto tax)
        {
            if (tax == null)
                return false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(tax.Code))
                return false;

            if (string.IsNullOrWhiteSpace(tax.Name))
                return false;

            // Validate percentage for percentage tax type
            if (tax.TaxType == TaxType.Percentage)
            {
                if (tax.Percentage < 0 || tax.Percentage > 100)
                    return false;
            }

            // Validate amount for fixed amount tax types
            if (tax.TaxType == TaxType.FixedAmount || tax.TaxType == TaxType.AmountPerUnit)
            {
                if (tax.Amount < 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a tax code
        /// </summary>
        /// <param name="code">The tax code to validate</param>
        /// <returns>True if code is valid, false otherwise</returns>
        public bool ValidateCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            // Tax codes should be alphanumeric and not too long
            if (code.Length > 20)
                return false;

            return true;
        }

        /// <summary>
        /// Validates a tax name
        /// </summary>
        /// <param name="name">The tax name to validate</param>
        /// <returns>True if name is valid, false otherwise</returns>
        public bool ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (name.Length > 100)
                return false;

            return true;
        }

        /// <summary>
        /// Validates a tax percentage
        /// </summary>
        /// <param name="percentage">The tax percentage to validate</param>
        /// <returns>True if percentage is valid, false otherwise</returns>
        public bool ValidatePercentage(decimal percentage)
        {
            // Tax percentage should be between 0 and 100
            return percentage >= 0 && percentage <= 100;
        }

        /// <summary>
        /// Validates a tax amount
        /// </summary>
        /// <param name="amount">The tax amount to validate</param>
        /// <returns>True if amount is valid, false otherwise</returns>
        public bool ValidateAmount(decimal amount)
        {
            // Tax amount should be non-negative
            return amount >= 0;
        }

        /// <summary>
        /// Validates account codes
        /// </summary>
        /// <param name="accountCode">The account code to validate</param>
        /// <returns>True if account code is valid or empty, false otherwise</returns>
        public bool ValidateAccountCode(string accountCode)
        {
            // Empty account code is valid (not all taxes require accounting codes)
            if (string.IsNullOrWhiteSpace(accountCode))
                return true;

            // Validate format of account code if needed
            // This is a simple length check; you might want to implement more complex validation
            if (accountCode.Length > 20)
                return false;

            return true;
        }
    }
}