using System.ComponentModel;
using System.Linq.Expressions;

namespace Sivar.Erp.Core.Core
{
    /// <summary>
    /// Generic repository interface for data access operations
    /// </summary>
    [Description("Generic repository interface for data access operations")]
    public interface IRepository : IDisposable
    {
        /// <summary>
        /// Creates a new object of the specified type
        /// </summary>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <returns>A new instance of the specified type</returns>
        [Description("Creates a new object of the specified type")]
        T CreateObject<T>() where T : class, new();
        
        /// <summary>
        /// Retrieves an object by its primary key
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve</typeparam>
        /// <param name="key">The primary key value</param>
        /// <returns>The object with the specified key, or null if not found</returns>
        [Description("Retrieves an object by its primary key")]
        T? GetObjectByKey<T>(object key) where T : class;
        
        /// <summary>
        /// Gets all objects of the specified type
        /// </summary>
        /// <typeparam name="T">The type of objects to retrieve</typeparam>
        /// <returns>A queryable collection of objects</returns>
        [Description("Gets all objects of the specified type")]
        IQueryable<T> GetObjects<T>() where T : class;
        
        /// <summary>
        /// Finds first object matching criteria
        /// </summary>
        /// <typeparam name="T">The type of object to find</typeparam>
        /// <param name="criteria">The search criteria</param>
        /// <returns>The first object matching the criteria, or null if none found</returns>
        [Description("Finds first object matching criteria")]
        T? FindObject<T>(Expression<Func<T, bool>> criteria) where T : class;
        
        /// <summary>
        /// Commits all pending changes
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        [Description("Commits all pending changes")]
        Task CommitChanges();
        
        /// <summary>
        /// Rollback all pending changes
        /// </summary>
        [Description("Rollback all pending changes")]
        void Rollback();
        
        /// <summary>
        /// Check if repository has unsaved changes
        /// </summary>
        [Description("Check if repository has unsaved changes")]
        bool IsModified { get; }
    }
}