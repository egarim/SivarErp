using System;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.Configuration
{
    /// <summary>
    /// Interface for option entities
    /// </summary>
    public interface IOption : IEntity
    {
        /// <summary>
        /// Unique code for the option
        /// </summary>
        string Code { get; set; }

        /// <summary>
        /// Display name for the option
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Description of what this option controls
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Module this option belongs to
        /// </summary>
        string ModuleName { get; set; }

        /// <summary>
        /// Whether this option is active in the system
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// When this option record was created
        /// </summary>
        DateTime CreatedDate { get; set; }

        /// <summary>
        /// When this option was last modified
        /// </summary>
        DateTime? ModifiedDate { get; set; }
    }
}