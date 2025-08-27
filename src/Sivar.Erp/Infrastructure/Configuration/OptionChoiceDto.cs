using System;

namespace Sivar.Erp.Infrastructure.Configuration
{
    /// <summary>
    /// Data transfer object for option choices
    /// </summary>
    public class OptionChoiceDto : IOptionChoice
    {
        /// <summary>
        /// Unique identifier for this option choice
        /// </summary>
        public Guid Oid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Reference to the parent option
        /// </summary>
        public Guid OptionId { get; set; }

        /// <summary>
        /// Name of this choice
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Description of what this choice represents
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Optional display order for UI presentation
        /// </summary>
        public string? DisplayOrder { get; set; }

        /// <summary>
        /// Whether this is the default choice for the option
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Whether this choice is active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Validates this option choice
        /// </summary>
        /// <returns>True if the option choice is valid, false otherwise</returns>
        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   OptionId != Guid.Empty;
        }
    }
}