using Sivar.Erp.Xpo.Core;

namespace Sivar.Erp.Xpo.Services
{
    /// <summary>
    /// Implementation of archiving service using XPO
    /// </summary>
    public class XpoArchiveService : IArchiveService
    {
        /// <summary>
        /// Archives an entity
        /// </summary>
        /// <param name="entity">Entity to archive</param>
        /// <returns>True if successful</returns>
        public bool Archive(IArchivable entity)
        {
            entity.IsArchived = true;

            // If it's an XPO object, save changes
            if (entity is XpoArchivableBase xpoEntity)
            {
                xpoEntity.Save();
            }

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

            // If it's an XPO object, save changes
            if (entity is XpoArchivableBase xpoEntity)
            {
                xpoEntity.Save();
            }

            return true;
        }
    }
}