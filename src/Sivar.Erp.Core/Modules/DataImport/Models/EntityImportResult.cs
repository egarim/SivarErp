using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.DataImport.Models
{
    /// <summary>
    /// Result of importing entities from a CSV file
    /// </summary>
    /// <typeparam name="T">Type of entity being imported</typeparam>
    [Description("Result of importing entities from a CSV file")]
    public class EntityImportResult<T> where T : class
    {
        /// <summary>
        /// Indicates if the import was successful
        /// </summary>
        [Description("Indicates if the import was successful")]
        public bool Success { get; set; }

        /// <summary>
        /// List of successfully imported entities
        /// </summary>
        [Description("List of successfully imported entities")]
        public List<T> ImportedEntities { get; set; } = new();

        /// <summary>
        /// List of errors encountered during import
        /// </summary>
        [Description("List of errors encountered during import")]
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Total number of records processed
        /// </summary>
        [Description("Total number of records processed")]
        public int TotalProcessed { get; set; }

        /// <summary>
        /// Time taken to complete the import
        /// </summary>
        [Description("Time taken to complete the import")]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Entity type name for logging and tracking
        /// </summary>
        [Description("Entity type name for logging and tracking")]
        public string EntityType { get; set; } = typeof(T).Name;
    }

    /// <summary>
    /// Result of batch import operations
    /// </summary>
    [Description("Result of batch import operations")]
    public class BatchImportResult
    {
        /// <summary>
        /// Indicates if the batch import was successful
        /// </summary>
        [Description("Indicates if the batch import was successful")]
        public bool Success { get; set; }

        /// <summary>
        /// List of individual import results
        /// </summary>
        [Description("List of individual import results")]
        public List<ImportResult> ImportResults { get; set; } = new();

        /// <summary>
        /// Total number of records processed across all imports
        /// </summary>
        [Description("Total number of records processed across all imports")]
        public int TotalProcessed { get; set; }

        /// <summary>
        /// Total number of records successfully imported
        /// </summary>
        [Description("Total number of records successfully imported")]
        public int TotalImported { get; set; }

        /// <summary>
        /// List of errors encountered during batch import
        /// </summary>
        [Description("List of errors encountered during batch import")]
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Total duration of the batch import operation
        /// </summary>
        [Description("Total duration of the batch import operation")]
        public TimeSpan TotalDuration { get; set; }
    }

    /// <summary>
    /// Result of importing a complete data set
    /// </summary>
    [Description("Result of importing a complete data set")]
    public class DataSetImportResult
    {
        /// <summary>
        /// Name of the data set imported
        /// </summary>
        [Description("Name of the data set imported")]
        public string DataSetName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the data set import was successful
        /// </summary>
        [Description("Indicates if the data set import was successful")]
        public bool Success { get; set; }

        /// <summary>
        /// Summary of the import operation
        /// </summary>
        [Description("Summary of the import operation")]
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Dictionary of import results by entity type
        /// </summary>
        [Description("Dictionary of import results by entity type")]
        public Dictionary<string, ImportResult> Results { get; set; } = new();

        /// <summary>
        /// Total duration of the data set import
        /// </summary>
        [Description("Total duration of the data set import")]
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Generic import result for any entity type
    /// </summary>
    [Description("Generic import result for any entity type")]
    public class ImportResult
    {
        /// <summary>
        /// Indicates if the import was successful
        /// </summary>
        [Description("Indicates if the import was successful")]
        public bool Success { get; set; }

        /// <summary>
        /// Entity type that was imported
        /// </summary>
        [Description("Entity type that was imported")]
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Total number of records processed
        /// </summary>
        [Description("Total number of records processed")]
        public int TotalProcessed { get; set; }

        /// <summary>
        /// Total number of records successfully imported
        /// </summary>
        [Description("Total number of records successfully imported")]
        public int TotalImported { get; set; }

        /// <summary>
        /// List of errors encountered during import
        /// </summary>
        [Description("List of errors encountered during import")]
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Duration of the import operation
        /// </summary>
        [Description("Duration of the import operation")]
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Validation result for import data
    /// </summary>
    [Description("Validation result for import data")]
    public class ImportValidationResult
    {
        /// <summary>
        /// Indicates if the data is valid for import
        /// </summary>
        [Description("Indicates if the data is valid for import")]
        public bool IsValid { get; set; }

        /// <summary>
        /// List of validation errors
        /// </summary>
        [Description("List of validation errors")]
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// List of validation warnings
        /// </summary>
        [Description("List of validation warnings")]
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// Number of records that will be processed
        /// </summary>
        [Description("Number of records that will be processed")]
        public int RecordCount { get; set; }
    }

    /// <summary>
    /// Import request information
    /// </summary>
    [Description("Import request information")]
    public class ImportRequest
    {
        /// <summary>
        /// Unique identifier for the import request
        /// </summary>
        [Description("Unique identifier for the import request")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Data source (CSV content or file path)
        /// </summary>
        [Description("Data source (CSV content or file path)")]
        public string DataSource { get; set; } = string.Empty;

        /// <summary>
        /// Type of entity to import
        /// </summary>
        [Description("Type of entity to import")]
        public Type? EntityType { get; set; }

        /// <summary>
        /// Priority of the import request
        /// </summary>
        [Description("Priority of the import request")]
        public ImportPriority Priority { get; set; } = ImportPriority.Normal;

        /// <summary>
        /// Additional options for the import
        /// </summary>
        [Description("Additional options for the import")]
        public Dictionary<string, object> Options { get; set; } = new();
    }

    /// <summary>
    /// Import priority levels
    /// </summary>
    [Description("Import priority levels")]
    public enum ImportPriority
    {
        /// <summary>
        /// Low priority import
        /// </summary>
        [Description("Low priority import")]
        Low = 1,

        /// <summary>
        /// Normal priority import
        /// </summary>
        [Description("Normal priority import")]
        Normal = 2,

        /// <summary>
        /// High priority import
        /// </summary>
        [Description("High priority import")]
        High = 3,

        /// <summary>
        /// Critical priority import
        /// </summary>
        [Description("Critical priority import")]
        Critical = 4
    }

    /// <summary>
    /// Import history record for auditing
    /// </summary>
    [Description("Import history record for auditing")]
    public class ImportHistory
    {
        /// <summary>
        /// Unique identifier for the history record
        /// </summary>
        [Description("Unique identifier for the history record")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who performed the import
        /// </summary>
        [Description("User who performed the import")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Entity type that was imported
        /// </summary>
        [Description("Entity type that was imported")]
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Source of the import data
        /// </summary>
        [Description("Source of the import data")]
        public string ImportSource { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the import was successful
        /// </summary>
        [Description("Indicates if the import was successful")]
        public bool Success { get; set; }

        /// <summary>
        /// Number of records processed
        /// </summary>
        [Description("Number of records processed")]
        public int RecordsProcessed { get; set; }

        /// <summary>
        /// Number of records successfully imported
        /// </summary>
        [Description("Number of records successfully imported")]
        public int RecordsImported { get; set; }

        /// <summary>
        /// Error details if the import failed
        /// </summary>
        [Description("Error details if the import failed")]
        public string? ErrorDetails { get; set; }
    }
}