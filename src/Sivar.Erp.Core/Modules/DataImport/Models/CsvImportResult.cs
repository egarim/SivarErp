namespace Sivar.Erp.Core.Modules.DataImport.Models
{
    /// <summary>
    /// Represents the result of a CSV import operation
    /// </summary>
    /// <typeparam name="T">The type of entity being imported</typeparam>
    public class CsvImportResult<T>
    {
        /// <summary>
        /// Indicates whether the import was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The collection of successfully imported entities
        /// </summary>
        public List<T> ImportedEntities { get; set; } = new();

        /// <summary>
        /// Any errors that occurred during the import
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// The total number of records processed
        /// </summary>
        public int TotalProcessed { get; set; }

        /// <summary>
        /// The time it took to process the import
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
}