using System;

namespace Sivar.Erp.Core.Core
{
    /// <summary>
    /// Represents a repository transaction for managing data consistency
    /// </summary>
    public interface IRepositoryTransaction : IDisposable
    {
        /// <summary>
        /// Commits all changes made in this transaction
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Rolls back all changes made in this transaction
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// Gets a value indicating whether this transaction has been completed (committed or rolled back)
        /// </summary>
        bool IsCompleted { get; }
    }
}
