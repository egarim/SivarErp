using System.ComponentModel;

namespace Sivar.Erp.Core.Infrastructure.AI
{
    /// <summary>
    /// Represents an AI operation request
    /// </summary>
    [Description("Represents an AI operation request")]
    public class AiOperationRequest
    {
        /// <summary>
        /// Unique identifier for the operation
        /// </summary>
        [Description("Unique identifier for the operation")]
        public string OperationId { get; set; } = string.Empty;

        /// <summary>
        /// Name of the operation to execute
        /// </summary>
        [Description("Name of the operation to execute")]
        public string OperationName { get; set; } = string.Empty;

        /// <summary>
        /// Parameters for the operation
        /// </summary>
        [Description("Parameters for the operation")]
        public Dictionary<string, object> Parameters { get; set; } = new();

        /// <summary>
        /// User context for the operation
        /// </summary>
        [Description("User context for the operation")]
        public string UserContext { get; set; } = string.Empty;

        /// <summary>
        /// Session identifier for tracking
        /// </summary>
        [Description("Session identifier for tracking")]
        public string SessionId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the result of an AI operation
    /// </summary>
    [Description("Represents the result of an AI operation")]
    public class AiOperationResult
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        [Description("Indicates if the operation was successful")]
        public bool Success { get; set; }

        /// <summary>
        /// Result data from the operation
        /// </summary>
        [Description("Result data from the operation")]
        public object? Data { get; set; }

        /// <summary>
        /// Error message if operation failed
        /// </summary>
        [Description("Error message if operation failed")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Additional metadata about the operation
        /// </summary>
        [Description("Additional metadata about the operation")]
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Duration of the operation
        /// </summary>
        [Description("Duration of the operation")]
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Represents an available AI operation
    /// </summary>
    [Description("Represents an available AI operation")]
    public class AiOperation
    {
        /// <summary>
        /// Unique name of the operation
        /// </summary>
        [Description("Unique name of the operation")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable description
        /// </summary>
        [Description("Human-readable description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Category of the operation
        /// </summary>
        [Description("Category of the operation")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Required parameters for the operation
        /// </summary>
        [Description("Required parameters for the operation")]
        public List<AiOperationParameter> Parameters { get; set; } = new();

        /// <summary>
        /// Expected return type
        /// </summary>
        [Description("Expected return type")]
        public string ReturnType { get; set; } = string.Empty;

        /// <summary>
        /// Security requirements
        /// </summary>
        [Description("Security requirements")]
        public List<string> SecurityRequirements { get; set; } = new();
    }

    /// <summary>
    /// Represents a parameter for an AI operation
    /// </summary>
    [Description("Represents a parameter for an AI operation")]
    public class AiOperationParameter
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        [Description("Parameter name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Parameter type
        /// </summary>
        [Description("Parameter type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Parameter description
        /// </summary>
        [Description("Parameter description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if parameter is required
        /// </summary>
        [Description("Indicates if parameter is required")]
        public bool Required { get; set; }

        /// <summary>
        /// Default value for the parameter
        /// </summary>
        [Description("Default value for the parameter")]
        public object? DefaultValue { get; set; }
    }
}
