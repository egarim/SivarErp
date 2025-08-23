using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Infrastructure.Data
{
    /// <summary>
    /// In-memory implementation of the repository pattern for demos and testing
    /// </summary>
    [Description("In-memory implementation of the repository pattern for demos and testing")]
    public class InMemoryRepository : IRepository
    {
        private readonly ConcurrentDictionary<Type, IList> _collections = new();
        private readonly HashSet<object> _newObjects = new();
        private readonly HashSet<object> _modifiedObjects = new();
        private readonly object _lock = new();
        
        /// <summary>
        /// Creates a new object of the specified type
        /// </summary>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <returns>A new instance of the specified type</returns>
        public T CreateObject<T>() where T : class, new()
        {
            lock (_lock)
            {
                var obj = new T();
                _newObjects.Add(obj);
                
                // Initialize IEntity properties if applicable
                if (obj is IEntity entity)
                {
                    entity.Id = Guid.NewGuid();
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.UpdatedAt = DateTime.UtcNow;
                }
                
                GetCollection<T>().Add(obj);
                return obj;
            }
        }
        
        /// <summary>
        /// Retrieves an object by its primary key
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve</typeparam>
        /// <param name="key">The primary key value</param>
        /// <returns>The object with the specified key, or null if not found</returns>
        public T? GetObjectByKey<T>(object key) where T : class
        {
            lock (_lock)
            {
                // Try to find by Id if it's an IEntity
                if (typeof(IEntity).IsAssignableFrom(typeof(T)) && key is Guid guidKey)
                {
                    return GetObjects<T>().FirstOrDefault(e => ((IEntity)e).Id == guidKey);
                }
                
                // Fallback to using Id property
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty == null)
                    throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");
                    
                return GetObjects<T>().FirstOrDefault(e => 
                    object.Equals(idProperty.GetValue(e), key));
            }
        }
        
        /// <summary>
        /// Gets all objects of the specified type
        /// </summary>
        /// <typeparam name="T">The type of objects to retrieve</typeparam>
        /// <returns>A queryable collection of objects</returns>
        public IQueryable<T> GetObjects<T>() where T : class
        {
            lock (_lock)
            {
                return GetCollection<T>().AsQueryable();
            }
        }
        
        /// <summary>
        /// Finds first object matching criteria
        /// </summary>
        /// <typeparam name="T">The type of object to find</typeparam>
        /// <param name="criteria">The search criteria</param>
        /// <returns>The first object matching the criteria, or null if none found</returns>
        public T? FindObject<T>(Expression<Func<T, bool>> criteria) where T : class
        {
            lock (_lock)
            {
                return GetObjects<T>().FirstOrDefault(criteria);
            }
        }
        
        /// <summary>
        /// Commits all pending changes
        /// </summary>
        public async Task CommitChanges()
        {
            // Simulate async commit
            await Task.Delay(1);
            
            lock (_lock)
            {
                // Update timestamps for entities
                foreach (var obj in _newObjects.Concat(_modifiedObjects))
                {
                    if (obj is IEntity entity)
                    {
                        entity.UpdatedAt = DateTime.UtcNow;
                    }
                }
                
                _newObjects.Clear();
                _modifiedObjects.Clear();
            }
        }
        
        /// <summary>
        /// Rollback all pending changes
        /// </summary>
        public void Rollback()
        {
            lock (_lock)
            {
                // Remove new objects from collections
                foreach (var newObj in _newObjects)
                {
                    var type = newObj.GetType();
                    
                    // Find all collections that this object could be in (including interfaces/base classes)
                    var possibleCollectionTypes = GetAllAssignableTypes(type);
                    
                    foreach (var collectionType in possibleCollectionTypes)
                    {
                        if (_collections.TryGetValue(collectionType, out var collection))
                        {
                            var removeMethod = collection.GetType().GetMethod("Remove");
                            if (removeMethod != null)
                            {
                                removeMethod.Invoke(collection, new[] { newObj });
                            }
                        }
                    }
                }
                
                _newObjects.Clear();
                _modifiedObjects.Clear();
            }
        }
        
        /// <summary>
        /// Check if repository has unsaved changes
        /// </summary>
        public bool IsModified => _newObjects.Count > 0 || _modifiedObjects.Count > 0;
        
        /// <summary>
        /// Marks an object as modified
        /// </summary>
        /// <param name="obj">The object to mark as modified</param>
        public void MarkAsModified(object obj)
        {
            lock (_lock)
            {
                if (!_newObjects.Contains(obj))
                {
                    _modifiedObjects.Add(obj);
                }
            }
        }
        
        /// <summary>
        /// Gets statistics about the repository contents
        /// </summary>
        /// <returns>Dictionary containing collection names and their counts</returns>
        [Description("Gets statistics about the repository contents")]
        public Dictionary<string, int> GetStatistics()
        {
            lock (_lock)
            {
                var stats = new Dictionary<string, int>();
                
                foreach (var kvp in _collections)
                {
                    stats[kvp.Key.Name] = kvp.Value.Count;
                }
                
                stats["New Objects"] = _newObjects.Count;
                stats["Modified Objects"] = _modifiedObjects.Count;
                
                return stats;
            }
        }

        /// <summary>
        /// Gets all types that a given type can be assigned to
        /// </summary>
        private IEnumerable<Type> GetAllAssignableTypes(Type type)
        {
            // Add the type itself
            yield return type;
            
            // Add all interfaces
            foreach (var interfaceType in type.GetInterfaces())
            {
                yield return interfaceType;
            }
            
            // Add all base types
            var baseType = type.BaseType;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }
        
        /// <summary>
        /// Gets or creates a collection for the specified type
        /// </summary>
        private IList<T> GetCollection<T>() where T : class
        {
            return (IList<T>)_collections.GetOrAdd(typeof(T), _ => new List<T>());
        }

        /// <summary>
        /// Clears all data from the repository
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _collections.Clear();
                _newObjects.Clear();
                _modifiedObjects.Clear();
            }
        }
        
        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }

        // Additional methods for enhanced Repository pattern support

        /// <summary>
        /// Gets an object by its primary key asynchronously
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve</typeparam>
        /// <param name="key">The primary key value</param>
        /// <returns>The object if found, null otherwise</returns>
        public async Task<T?> GetByKeyAsync<T>(object key) where T : class
        {
            // Simulate async operation
            await Task.Yield();
            return GetObjectByKey<T>(key);
        }

        /// <summary>
        /// Gets objects matching the specified predicate
        /// </summary>
        /// <typeparam name="T">The type of objects to retrieve</typeparam>
        /// <param name="predicate">The filtering predicate</param>
        /// <returns>A queryable collection of matching objects</returns>
        public IQueryable<T> GetObjects<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            lock (_lock)
            {
                return GetCollection<T>().AsQueryable().Where(predicate);
            }
        }

        /// <summary>
        /// Updates an existing object
        /// </summary>
        /// <typeparam name="T">The type of object to update</typeparam>
        /// <param name="obj">The object to update</param>
        public void UpdateObject<T>(T obj) where T : class
        {
            MarkAsModified(obj);
        }

        /// <summary>
        /// Deletes an object
        /// </summary>
        /// <typeparam name="T">The type of object to delete</typeparam>
        /// <param name="obj">The object to delete</param>
        public void DeleteObject<T>(T obj) where T : class
        {
            lock (_lock)
            {
                var collection = GetCollection<T>();
                collection.Remove(obj);
                _newObjects.Remove(obj);
                _modifiedObjects.Remove(obj);
            }
        }

        /// <summary>
        /// Gets multiple objects by their keys in a single operation
        /// </summary>
        /// <typeparam name="T">The type of objects to retrieve</typeparam>
        /// <param name="keys">The collection of primary keys</param>
        /// <returns>A collection of objects matching the keys</returns>
        public async Task<IEnumerable<T>> GetBatchAsync<T>(IEnumerable<object> keys) where T : class
        {
            await Task.Yield();
            
            lock (_lock)
            {
                var results = new List<T>();
                foreach (var key in keys)
                {
                    var obj = GetObjectByKey<T>(key);
                    if (obj != null)
                        results.Add(obj);
                }
                return results;
            }
        }

        /// <summary>
        /// Inserts multiple objects in a single operation
        /// </summary>
        /// <typeparam name="T">The type of objects to insert</typeparam>
        /// <param name="objects">The objects to insert</param>
        public async Task BulkInsertAsync<T>(IEnumerable<T> objects) where T : class
        {
            await Task.Yield();
            
            lock (_lock)
            {
                var collection = GetCollection<T>();
                foreach (var obj in objects)
                {
                    // Initialize IEntity properties if applicable
                    if (obj is IEntity entity)
                    {
                        entity.Id = Guid.NewGuid();
                        entity.CreatedAt = DateTime.UtcNow;
                        entity.UpdatedAt = DateTime.UtcNow;
                    }
                    
                    collection.Add(obj);
                    _newObjects.Add(obj);
                }
            }
        }

        /// <summary>
        /// Updates multiple objects in a single operation
        /// </summary>
        /// <typeparam name="T">The type of objects to update</typeparam>
        /// <param name="objects">The objects to update</param>
        public async Task BulkUpdateAsync<T>(IEnumerable<T> objects) where T : class
        {
            await Task.Yield();
            
            lock (_lock)
            {
                foreach (var obj in objects)
                {
                    MarkAsModified(obj);
                }
            }
        }

        /// <summary>
        /// Saves all pending changes to the repository
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SaveChangesAsync()
        {
            await CommitChanges();
        }

        /// <summary>
        /// Begins a new transaction for data consistency
        /// </summary>
        /// <returns>A repository transaction</returns>
        public IRepositoryTransaction BeginTransaction()
        {
            return new InMemoryRepositoryTransaction(this);
        }

        /// <summary>
        /// Gets the total count of objects of the specified type
        /// </summary>
        /// <typeparam name="T">The type of objects to count</typeparam>
        /// <returns>The total count of objects</returns>
        public async Task<int> GetCountAsync<T>() where T : class
        {
            await Task.Yield();
            
            lock (_lock)
            {
                return GetCollection<T>().Count;
            }
        }

        /// <summary>
        /// Checks if any objects of the specified type exist matching the predicate
        /// </summary>
        /// <typeparam name="T">The type of objects to check</typeparam>
        /// <param name="predicate">The filtering predicate</param>
        /// <returns>True if any objects exist, false otherwise</returns>
        public async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            await Task.Yield();
            
            lock (_lock)
            {
                return GetCollection<T>().AsQueryable().Any(predicate);
            }
        }

        /// <summary>
        /// Clears the cache for the specified type
        /// </summary>
        /// <typeparam name="T">The type to clear cache for</typeparam>
        public void ClearCache<T>() where T : class
        {
            lock (_lock)
            {
                if (_collections.ContainsKey(typeof(T)))
                {
                    _collections[typeof(T)].Clear();
                }
            }
        }

        /// <summary>
        /// Clears all caches
        /// </summary>
        public void ClearAllCaches()
        {
            Clear();
        }
    }

    /// <summary>
    /// In-memory implementation of repository transaction
    /// </summary>
    internal class InMemoryRepositoryTransaction : IRepositoryTransaction
    {
        private readonly InMemoryRepository _repository;
        private bool _isCompleted;
        private bool _disposed;

        public InMemoryRepositoryTransaction(InMemoryRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Gets a value indicating whether the transaction has been completed
        /// </summary>
        public bool IsCompleted => _isCompleted;

        /// <summary>
        /// Commits the transaction asynchronously
        /// </summary>
        public async Task CommitAsync()
        {
            if (_isCompleted || _disposed)
                throw new InvalidOperationException("Transaction has already been completed or disposed");

            await _repository.CommitChanges();
            _isCompleted = true;
        }

        /// <summary>
        /// Rolls back the transaction asynchronously
        /// </summary>
        public async Task RollbackAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(InMemoryRepositoryTransaction));

            await Task.Run(() => _repository.Rollback());
            _isCompleted = true;
        }

        /// <summary>
        /// Disposes the transaction
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                if (!_isCompleted)
                {
                    // Auto-rollback if not committed
                    _repository.Rollback();
                }
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}