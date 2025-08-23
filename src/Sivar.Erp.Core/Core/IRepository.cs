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
        /// Marks an object as modified for change tracking
        /// </summary>
        /// <param name="obj">The object to mark as modified</param>
        [Description("Marks an object as modified for change tracking")]
        void MarkAsModified(object obj);
        
        /// <summary>
        /// Gets statistics about the repository contents
        /// </summary>
        /// <returns>Dictionary with entity type names and counts</returns>
        [Description("Gets statistics about the repository contents")]
        Dictionary<string, int> GetStatistics();
        
        /// <summary>
        /// Clears all data from the repository
        /// </summary>
        [Description("Clears all data from the repository")]
        void Clear();
        
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

        // Additional methods for enhanced Repository pattern support

        /// <summary>
        /// Gets an object by its primary key asynchronously
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve</typeparam>
        /// <param name="key">The primary key value</param>
        /// <returns>The object if found, null otherwise</returns>
        [Description("Gets an object by its primary key asynchronously")]
        Task<T?> GetByKeyAsync<T>(object key) where T : class;

        /// <summary>
        /// Gets objects matching the specified predicate
        /// </summary>
        /// <typeparam name="T">The type of objects to retrieve</typeparam>
        /// <param name="predicate">The filtering predicate</param>
        /// <returns>A queryable collection of matching objects</returns>
        [Description("Gets objects matching the specified predicate")]
        IQueryable<T> GetObjects<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// Updates an existing object
        /// </summary>
        /// <typeparam name="T">The type of object to update</typeparam>
        /// <param name="obj">The object to update</param>
        [Description("Updates an existing object")]
        void UpdateObject<T>(T obj) where T : class;

        /// <summary>
        /// Deletes an object
        /// </summary>
        /// <typeparam name="T">The type of object to delete</typeparam>
        /// <param name="obj">The object to delete</param>
        [Description("Deletes an object")]
        void DeleteObject<T>(T obj) where T : class;

        /// <summary>
        /// Gets multiple objects by their keys in a single operation
        /// </summary>
        /// <typeparam name="T">The type of objects to retrieve</typeparam>
        /// <param name="keys">The collection of primary keys</param>
        /// <returns>A collection of objects matching the keys</returns>
        [Description("Gets multiple objects by their keys in a single operation")]
        Task<IEnumerable<T>> GetBatchAsync<T>(IEnumerable<object> keys) where T : class;

        /// <summary>
        /// Inserts multiple objects in a single operation
        /// </summary>
        /// <typeparam name="T">The type of objects to insert</typeparam>
        /// <param name="objects">The objects to insert</param>
        [Description("Inserts multiple objects in a single operation")]
        Task BulkInsertAsync<T>(IEnumerable<T> objects) where T : class;

        /// <summary>
        /// Updates multiple objects in a single operation
        /// </summary>
        /// <typeparam name="T">The type of objects to update</typeparam>
        /// <param name="objects">The objects to update</param>
        [Description("Updates multiple objects in a single operation")]
        Task BulkUpdateAsync<T>(IEnumerable<T> objects) where T : class;

        /// <summary>
        /// Saves all pending changes to the repository
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        [Description("Saves all pending changes to the repository")]
        Task SaveChangesAsync();

        /// <summary>
        /// Begins a new transaction for data consistency
        /// </summary>
        /// <returns>A repository transaction</returns>
        [Description("Begins a new transaction for data consistency")]
        IRepositoryTransaction BeginTransaction();

        /// <summary>
        /// Gets the total count of objects of the specified type
        /// </summary>
        /// <typeparam name="T">The type of objects to count</typeparam>
        /// <returns>The total count of objects</returns>
        [Description("Gets the total count of objects of the specified type")]
        Task<int> GetCountAsync<T>() where T : class;

        /// <summary>
        /// Checks if any objects of the specified type exist matching the predicate
        /// </summary>
        /// <typeparam name="T">The type of objects to check</typeparam>
        /// <param name="predicate">The filtering predicate</param>
        /// <returns>True if any objects exist, false otherwise</returns>
        [Description("Checks if any objects of the specified type exist matching the predicate")]
        Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// Clears the cache for the specified type
        /// </summary>
        /// <typeparam name="T">The type to clear cache for</typeparam>
        [Description("Clears the cache for the specified type")]
        void ClearCache<T>() where T : class;

        /// <summary>
        /// Clears all caches
        /// </summary>
        [Description("Clears all caches")]
        void ClearAllCaches();
    }
}