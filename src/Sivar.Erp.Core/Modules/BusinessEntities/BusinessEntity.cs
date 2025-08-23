using System;
using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.BusinessEntities
{
    /// <summary>
    /// Base class for business entities
    /// </summary>
    [Description("Base class for business entities")]
    public class BusinessEntity : IBusinessEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Description("Entity identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        [Description("Last update date")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
        [Description("Entity name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity code
        /// </summary>
        [Description("Entity code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity description
        /// </summary>
        [Description("Entity description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether the entity is active
        /// </summary>
        [Description("Whether the entity is active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets additional metadata
        /// </summary>
        [Description("Additional metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
