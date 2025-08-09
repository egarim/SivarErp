using System.ComponentModel;

namespace Sivar.Erp.Core.Core
{
    /// <summary>
    /// Base implementation of the IEntity interface
    /// </summary>
    [Description("Base implementation of the IEntity interface")]
    public abstract class Entity : IEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}