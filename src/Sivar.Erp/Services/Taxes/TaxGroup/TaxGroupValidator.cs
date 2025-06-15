using System;

namespace Sivar.Erp.Services.Taxes.TaxGroup
{
    /// <summary>
    /// Validator for tax groups
    /// </summary>
    public class TaxGroupValidator
    {
        /// <summary>
        /// Validates a tax group
        /// </summary>
        /// <param name="taxGroup">The tax group to validate</param>
        /// <returns>True if the tax group is valid, false otherwise</returns>
        public bool ValidateTaxGroup(ITaxGroup taxGroup)
        {
            if (taxGroup == null)
                return false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(taxGroup.Code))
                return false;
            
            if (string.IsNullOrWhiteSpace(taxGroup.Name))
                return false;

            return true;
        }

        /// <summary>
        /// Validates a tax group code
        /// </summary>
        /// <param name="code">The code to validate</param>
        /// <returns>True if the code is valid, false otherwise</returns>
        public bool ValidateCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;
            
            // Code should not be too long and should not contain invalid characters
            if (code.Length > 50 || code.Contains(",") || code.Contains("\""))
                return false;
            
            return true;
        }

        /// <summary>
        /// Validates a tax group name
        /// </summary>
        /// <param name="name">The name to validate</param>
        /// <returns>True if the name is valid, false otherwise</returns>
        public bool ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            
            // Name should not be too long
            if (name.Length > 100)
                return false;
            
            return true;
        }

        /// <summary>
        /// Validates a tax group description
        /// </summary>
        /// <param name="description">The description to validate</param>
        /// <returns>True if the description is valid, false otherwise</returns>
        public bool ValidateDescription(string description)
        {
            // Description can be null or empty
            if (string.IsNullOrWhiteSpace(description))
                return true;
            
            // Description should not be too long
            if (description.Length > 500)
                return false;
            
            return true;
        }
    }
}