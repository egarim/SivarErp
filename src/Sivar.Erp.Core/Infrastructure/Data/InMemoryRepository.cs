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
            return (IList<T>)_collections.GetOrAdd(typeof(T), _ => 
                Activator.CreateInstance(typeof(List<>).MakeGenericType(typeof(T))) as IList 
                ?? new List<T>());
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
    }
}