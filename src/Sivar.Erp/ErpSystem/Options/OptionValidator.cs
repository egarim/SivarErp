using System;

namespace Sivar.Erp.ErpSystem.Options
{
    /// <summary>
    /// Validator for option business rules
    /// </summary>
    public class OptionValidator
    {
        /// <summary>
        /// Initializes a new instance of OptionValidator
        /// </summary>
        public OptionValidator()
        {
        }

        /// <summary>
        /// Validates the option code format
        /// </summary>
        /// <param name="code">Option code to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateOptionCode(string code)
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

            // Code should only contain alphanumeric characters and underscores
            foreach (char c in code)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates the option
        /// </summary>
        /// <param name="option">Option to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateOption(IOption option)
        {
            if (option == null)
            {
                return false;
            }

            // Validate code
            if (!ValidateOptionCode(option.Code))
            {
                return false;
            }

            // Validate name is not empty
            if (string.IsNullOrWhiteSpace(option.Name))
            {
                return false;
            }

            // Validate module name is not empty
            if (string.IsNullOrWhiteSpace(option.ModuleName))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the option choice
        /// </summary>
        /// <param name="choice">Option choice to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateOptionChoice(IOptionChoice choice)
        {
            if (choice == null)
            {
                return false;
            }

            // OptionId must not be empty
            if (choice.OptionId == Guid.Empty)
            {
                return false;
            }

            // Name must not be empty
            if (string.IsNullOrWhiteSpace(choice.Name))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the option detail
        /// </summary>
        /// <param name="detail">Option detail to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateOptionDetail(IOptionDetail detail)
        {
            if (detail == null)
            {
                return false;
            }

            // OptionId must not be empty
            if (detail.OptionId == Guid.Empty)
            {
                return false;
            }

            // OptionChoiceId must not be empty
            if (detail.OptionChoiceId == Guid.Empty)
            {
                return false;
            }

            // Value must not be empty
            if (string.IsNullOrWhiteSpace(detail.Value))
            {
                return false;
            }

            // If ValidTo is specified, it must be greater than ValidFrom
            if (detail.ValidTo.HasValue && detail.ValidTo.Value <= detail.ValidFrom)
            {
                return false;
            }

            return true;
        }
    }
}