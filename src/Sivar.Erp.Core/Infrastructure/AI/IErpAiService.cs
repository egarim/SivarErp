using System.ComponentModel;

namespace Sivar.Erp.Core.Infrastructure.AI
{
    /// <summary>
    /// Service for exposing ERP functionality to AI agents
    /// Provides a structured way for AI systems to interact with ERP operations
    /// </summary>
    [Description("Service for exposing ERP functionality to AI agents")]
    public interface IErpAiService
    {
        /// <summary>
        /// Gets all available operations that AI agents can execute
        /// </summary>
        /// <returns>Collection of available AI operations</returns>
        [Description("Gets available operations for AI agents")]
        Task<IEnumerable<AiOperation>> GetAvailableOperationsAsync();

        /// <summary>
        /// Gets operations filtered by category
        /// </summary>
        /// <param name="category">Operation category to filter by</param>
        /// <returns>Collection of operations in the specified category</returns>
        [Description("Gets operations filtered by category")]
        Task<IEnumerable<AiOperation>> GetOperationsByCategoryAsync(
            [Description("Operation category")] string category);

        /// <summary>
        /// Executes an operation requested by an AI agent
        /// </summary>
        /// <param name="request">Operation request with parameters</param>
        /// <returns>Result of the operation execution</returns>
        [Description("Executes an operation requested by an AI agent")]
        Task<AiOperationResult> ExecuteOperationAsync(
            [Description("Operation to execute")] AiOperationRequest request);

        /// <summary>
        /// Validates an operation request before execution
        /// </summary>
        /// <param name="request">Operation request to validate</param>
        /// <returns>Validation result</returns>
        [Description("Validates an operation request before execution")]
        Task<AiOperationResult> ValidateOperationAsync(
            [Description("Operation request to validate")] AiOperationRequest request);

        /// <summary>
        /// Gets operation schema for a specific operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <returns>Detailed schema for the operation</returns>
        [Description("Gets operation schema for a specific operation")]
        Task<AiOperation?> GetOperationSchemaAsync(
            [Description("Operation name")] string operationName);

        /// <summary>
        /// Gets AI capabilities and limitations
        /// </summary>
        /// <returns>Information about AI integration capabilities</returns>
        [Description("Gets AI capabilities and limitations")]
        Task<AiCapabilities> GetCapabilitiesAsync();
    }

    /// <summary>
    /// Represents AI integration capabilities
    /// </summary>
    [Description("Represents AI integration capabilities")]
    public class AiCapabilities
    {
        /// <summary>
        /// Supported operation categories
        /// </summary>
        [Description("Supported operation categories")]
        public List<string> SupportedCategories { get; set; } = new();

        /// <summary>
        /// Maximum operations per session
        /// </summary>
        [Description("Maximum operations per session")]
        public int MaxOperationsPerSession { get; set; }

        /// <summary>
        /// Rate limit information
        /// </summary>
        [Description("Rate limit information")]
        public RateLimitInfo RateLimit { get; set; } = new();

        /// <summary>
        /// Security features available
        /// </summary>
        [Description("Security features available")]
        public List<string> SecurityFeatures { get; set; } = new();
    }

    /// <summary>
    /// Rate limit information for AI operations
    /// </summary>
    [Description("Rate limit information for AI operations")]
    public class RateLimitInfo
    {
        /// <summary>
        /// Maximum requests per minute
        /// </summary>
        [Description("Maximum requests per minute")]
        public int RequestsPerMinute { get; set; }

        /// <summary>
        /// Maximum requests per hour
        /// </summary>
        [Description("Maximum requests per hour")]
        public int RequestsPerHour { get; set; }

        /// <summary>
        /// Burst limit for short-term requests
        /// </summary>
        [Description("Burst limit for short-term requests")]
        public int BurstLimit { get; set; }
    }
}
