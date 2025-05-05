namespace Sivar.Erp
{
    /// <summary>
    /// Base entity class implementing common interfaces
    /// This is used when you can control the base class
    /// </summary>
    public abstract class BaseEntity : IEntity, IAuditable
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was created
        /// </summary>
        public DateTime InsertedAt { get; set; }

        /// <summary>
        /// User who created the entity
        /// </summary>
        public required string InsertedBy { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who last updated the entity
        /// </summary>
        public required string UpdatedBy { get; set; }
    }
}
