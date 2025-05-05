namespace Sivar.Erp
{
    /// <summary>
    /// Interface for entities with audit information
    /// </summary>
    public interface IAuditable : IEntity
    {
        /// <summary>
        /// UTC timestamp when the entity was created
        /// </summary>
        DateTime InsertedAt { get; set; }

        /// <summary>
        /// User who created the entity
        /// </summary>
        string InsertedBy { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was last updated
        /// </summary>
        DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who last updated the entity
        /// </summary>
        string UpdatedBy { get; set; }
    }
}
