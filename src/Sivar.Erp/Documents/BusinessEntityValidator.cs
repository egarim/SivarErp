using Sivar.Erp.BusinessEntities;
using System;
using System.Text.RegularExpressions;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Validator for business entity business rules
    /// </summary>
    public class BusinessEntityValidator
    {
        /// <summary>
        /// Initializes a new instance of BusinessEntityValidator
        /// </summary>
        public BusinessEntityValidator()
        {
        }

        /// <summary>
        /// Validates the business entity code format
        /// </summary>
        /// <param name="code">Business entity code to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateCode(string code)
        {
            // Basic validation - code cannot be null or empty
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            // Code should be at least 2 characters
            if (code.Length < 2)
            {
                return false;
            }

            // Additional validation rules can be added here

            return true;
        }

        /// <summary>
        /// Validates the business entity email format
        /// </summary>
        /// <param name="email">Email to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return true; // Email is optional
            }

            try
            {
                // Simple regex for basic email validation
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates the business entity phone number format
        /// </summary>
        /// <param name="phoneNumber">Phone number to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return true; // Phone number is optional
            }

            // Basic validation - allow digits, spaces, parentheses, hyphens, and plus sign
            var regex = new Regex(@"^[0-9\s\(\)\-\+]+$");
            return regex.IsMatch(phoneNumber);
        }

        /// <summary>
        /// Validates the complete business entity
        /// </summary>
        /// <param name="businessEntity">Business entity to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateBusinessEntity(IBusinessEntity businessEntity)
        {
            if (businessEntity == null)
            {
                return false;
            }

            // Validate code
            if (!ValidateCode(businessEntity.Code))
            {
                return false;
            }

            // Validate name is not empty
            if (string.IsNullOrWhiteSpace(businessEntity.Name))
            {
                return false;
            }

            // Validate email format
            if (!ValidateEmail(businessEntity.Email))
            {
                return false;
            }

            // Validate phone number format
            if (!ValidatePhoneNumber(businessEntity.PhoneNumber))
            {
                return false;
            }

            return true;
        }
    }
}