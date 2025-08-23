using System;
using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.BusinessEntities
{
    /// <summary>
    /// Business entity interface
    /// </summary>
    [Description("Business entity interface")]
    public interface IBusinessEntity : IEntity
    {
        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
        [Description("Entity name")]
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the entity code
        /// </summary>
        [Description("Entity code")]
        string Code { get; set; }

        /// <summary>
        /// Gets or sets the entity description
        /// </summary>
        [Description("Entity description")]
        string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether the entity is active
        /// </summary>
        [Description("Whether the entity is active")]
        bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets additional metadata
        /// </summary>
        [Description("Additional metadata")]
        Dictionary<string, object> Metadata { get; set; }
    }
}
