using System.ComponentModel;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.DataImport.Models;

namespace Sivar.Erp.Core.Modules.DataImport.Importers
{
    /// <summary>
    /// Base interface for entity-specific importers
    /// </summary>
    /// <typeparam name="T">The type of entity to import</typeparam>
    [Description("Base interface for entity-specific importers")]
    public interface IEntityImporter<T> where T : class
    {
        /// <summary>
        /// Imports entities with business logic validation
        /// </summary>
        /// <param name="repository">The repository to store the imported entities</param>
        /// <param name="csvContent">The CSV content as a string</param>
        /// <param name="userName">The user performing the import</param>
        /// <returns>Result containing imported entities and any errors</returns>
        [Description("Imports entities with business logic validation")]
        Task<EntityImportResult<T>> ImportAsync(
            IRepository repository,
            string csvContent,
            string userName);
    }

    /// <summary>
    /// Result of an entity import operation
    /// </summary>
    /// <typeparam name="T">The type of entity imported</typeparam>
    public class EntityImportResult<T> where T : class
    {
        /// <summary>
        /// Gets or sets whether the import was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the collection of imported entities
        /// </summary>
        public List<T> ImportedEntities { get; set; } = new();

        /// <summary>
        /// Gets or sets any errors that occurred during the import
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Gets or sets the total number of records processed
        /// </summary>
        public int TotalProcessed { get; set; }
    }
}