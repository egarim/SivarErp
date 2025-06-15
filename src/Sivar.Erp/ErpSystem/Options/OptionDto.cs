using System;

namespace Sivar.Erp.ErpSystem.Options
{
    /// <summary>
    /// Data transfer object for options
    /// </summary>
    public class OptionDto : IOption
    {
        /// <summary>
        /// Unique identifier for this option
        /// </summary>
        public Guid Oid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Unique code for the option
        /// </summary>
        public required string Code { get; set; }

        /// <summary>
        /// Display name for the option
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Description of what this option controls
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Module this option belongs to
        /// </summary>
        public required string ModuleName { get; set; }

        /// <summary>
        /// Whether this option is active in the system
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When this option record was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this option was last modified
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
        
        /// <summary>
        /// Validates this option
        /// </summary>
        /// <returns>True if the option is valid, false otherwise</returns>
        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Code) &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(ModuleName);
        }
    }
}