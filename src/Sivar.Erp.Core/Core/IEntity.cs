using System.ComponentModel;

namespace Sivar.Erp.Core.Core
{
    /// <summary>
    /// Base interface for all business entities
    /// </summary>
    [Description("Base interface for all business entities")]
    public interface IEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        Guid Id { get; set; }
        
        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        DateTime UpdatedAt { get; set; }
    }
}