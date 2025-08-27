using System;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.Configuration
{
    /// <summary>
    /// Interface for option choice entities
    /// </summary>
    public interface IOptionChoice : IEntity
    {
        /// <summary>
        /// Reference to the parent option
        /// </summary>
        Guid OptionId { get; set; }

        /// <summary>
        /// Name of this choice
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Description of what this choice represents
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Optional display order for UI presentation
        /// </summary>
        string? DisplayOrder { get; set; }

        /// <summary>
        /// Whether this is the default choice for the option
        /// </summary>
        bool IsDefault { get; set; }

        /// <summary>
        /// Whether this choice is active
        /// </summary>
        bool IsActive { get; set; }
    }
}