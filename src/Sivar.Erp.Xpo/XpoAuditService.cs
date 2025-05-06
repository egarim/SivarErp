using DevExpress.Xpo;
using Sivar.Erp.Xpo.Core;
using System;

namespace Sivar.Erp.Xpo.Services
{
    /// <summary>
    /// Implementation of audit service using XPO
    /// </summary>
    public class XpoAuditService : IAuditService
    {
        /// <summary>
        /// Sets audit information for a newly created entity
        /// </summary>
        /// <param name="entity">Entity to initialize</param>
        /// <param name="userName">User performing the operation</param>
        public void SetCreationAudit(IAuditable entity, string userName)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.InsertedAt = now;
            entity.UpdatedAt = now;
            entity.InsertedBy = userName;
            entity.UpdatedBy = userName;
        }

        /// <summary>
        /// Updates audit information for an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <param name="userName">User performing the operation</param>
        public void SetUpdateAudit(IAuditable entity, string userName)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userName;
        }
    }
}