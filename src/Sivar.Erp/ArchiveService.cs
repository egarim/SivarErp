namespace Sivar.Erp
{
    /// <summary>
    /// Implementation of archiving service
    /// </summary>
    public class ArchiveService : IArchiveService
    {
        /// <summary>
        /// Archives an entity
        /// </summary>
        /// <param name="entity">Entity to archive</param>
        /// <returns>True if successful</returns>
        public bool Archive(IArchivable entity)
        {
            entity.IsArchived = true;
            return true;
        }

        /// <summary>
        /// Restores a previously archived entity
        /// </summary>
        /// <param name="entity">Entity to restore</param>
        /// <returns>True if successful</returns>
        public bool Restore(IArchivable entity)
        {
            entity.IsArchived = false;
            return true;
        }
    }
}
