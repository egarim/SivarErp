using System.Linq.Expressions;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Tests.Infrastructure
{
    /// <summary>
    /// Extension methods for IRepository to provide test-friendly functionality
    /// </summary>
    public static class RepositoryTestExtensions
    {
        /// <summary>
        /// Gets all objects of the specified type asynchronously
        /// </summary>
        public static async Task<IEnumerable<T>> GetObjectsAsync<T>(this IRepository repository) where T : class
        {
            await Task.Yield();
            return repository.GetObjects<T>().ToList();
        }

        /// <summary>
        /// Finds the first object matching the predicate asynchronously
        /// </summary>
        public static async Task<T?> FindObjectAsync<T>(this IRepository repository, Expression<Func<T, bool>> predicate) where T : class
        {
            await Task.Yield();
            return repository.GetObjects<T>().FirstOrDefault(predicate);
        }

        /// <summary>
        /// Deletes all objects of the specified type
        /// </summary>
        public static async Task DeleteAllAsync<T>(this IRepository repository) where T : class
        {
            await Task.Yield();
            var objects = repository.GetObjects<T>().ToList();
            foreach (var obj in objects)
            {
                repository.DeleteObject(obj);
            }
        }

        /// <summary>
        /// Gets the count of objects of the specified type
        /// </summary>
        public static async Task<int> GetCountAsync<T>(this IRepository repository) where T : class
        {
            await Task.Yield();
            return repository.GetObjects<T>().Count();
        }
    }
}
