namespace Sivar.Erp
{
    /// <summary>
    /// Interface for archiving service operations
    /// </summary>
    public interface IArchiveService
    {
        /// <summary>
        /// Archives an entity
        /// </summary>
        /// <param name="entity">Entity to archive</param>
        /// <returns>True if successful</returns>
        bool Archive(IArchivable entity);

        /// <summary>
        /// Restores a previously archived entity
        /// </summary>
        /// <param name="entity">Entity to restore</param>
        /// <returns>True if successful</returns>
        bool Restore(IArchivable entity);
    }
}
