namespace Sivar.Erp
{
    /// <summary>
    /// Implementation of the audit service
    /// </summary>
    public class AuditService : IAuditService
    {
        /// <summary>
        /// Sets audit information for a newly created entity
        /// </summary>
        /// <param name="entity">Entity to initialize</param>
        /// <param name="userName">User performing the operation</param>
        public void SetCreationAudit(IAuditable entity, string userName)
        {
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
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userName;
        }
    }
}
