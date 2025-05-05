namespace Sivar.Erp
{
    /// <summary>
    /// Interface for audit service operations
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Sets audit information for a newly created entity
        /// </summary>
        /// <param name="entity">Entity to initialize</param>
        /// <param name="userName">User performing the operation</param>
        void SetCreationAudit(IAuditable entity, string userName);

        /// <summary>
        /// Updates audit information for an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <param name="userName">User performing the operation</param>
        void SetUpdateAudit(IAuditable entity, string userName);
    }
}
