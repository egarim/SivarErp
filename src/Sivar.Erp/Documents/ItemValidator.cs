using System;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Validator for item business rules
    /// </summary>
    public class ItemValidator
    {
        /// <summary>
        /// Initializes a new instance of ItemValidator
        /// </summary>
        public ItemValidator()
        {
        }

        /// <summary>
        /// Validates the item code format
        /// </summary>
        /// <param name="code">Item code to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateItemCode(string code)
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
        /// Validates the item price is within acceptable range
        /// </summary>
        /// <param name="price">Item price to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateItemPrice(decimal price)
        {
            // Price should not be negative
            if (price < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the complete item
        /// </summary>
        /// <param name="item">Item to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateItem(IItem item)
        {
            if (item == null)
            {
                return false;
            }

            // Validate code
            if (!ValidateItemCode(item.Code))
            {
                return false;
            }

            // Validate description is not empty
            if (string.IsNullOrWhiteSpace(item.Description))
            {
                return false;
            }

            // Validate price
            if (!ValidateItemPrice(item.BasePrice))
            {
                return false;
            }

            // Validate type is specified
            if (string.IsNullOrWhiteSpace(item.Type))
            {
                return false;
            }

            return true;
        }
    }
}