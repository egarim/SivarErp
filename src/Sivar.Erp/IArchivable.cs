namespace Sivar.Erp
{
    /// <summary>
    /// Interface for entities that support archiving (soft delete)
    /// </summary>
    public interface IArchivable : IEntity
    {
        /// <summary>
        /// Indicates whether the entity is archived
        /// </summary>
        bool IsArchived { get; set; }
    }
}
