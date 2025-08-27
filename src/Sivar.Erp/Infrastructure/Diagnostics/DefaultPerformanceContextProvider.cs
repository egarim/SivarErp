using System;

namespace Sivar.Erp.Infrastructure.Diagnostics
{
    /// <summary>
    /// Default implementation of IPerformanceContextProvider
    /// </summary>
    public class DefaultPerformanceContextProvider : IPerformanceContextProvider
    {
        /// <summary>
        /// Gets the current user ID for performance tracking
        /// </summary>
        public string? UserId { get; }
        
        /// <summary>
        /// Gets the current user name for performance tracking
        /// </summary>
        public string? UserName { get; }
        
        /// <summary>
        /// Gets the current session ID for performance tracking
        /// </summary>
        public string? SessionId { get; }
        
        /// <summary>
        /// Gets the current context or operation being tracked
        /// </summary>
        public string? Context { get; }
        
        /// <summary>
        /// Gets a unique instance identifier for this provider
        /// </summary>
        public string InstanceId { get; }

        /// <summary>
        /// Initializes a new instance of DefaultPerformanceContextProvider
        /// </summary>
        /// <param name="userId">User ID for tracking</param>
        /// <param name="userName">User name for tracking</param>
        /// <param name="sessionId">Session ID for tracking</param>
        /// <param name="context">Context or operation being tracked</param>
        public DefaultPerformanceContextProvider(
            string? userId = null,
            string? userName = null,
            string? sessionId = null,
            string? context = null)
        {
            UserId = userId;
            UserName = userName;
            SessionId = sessionId;
            Context = context;
            InstanceId = Guid.NewGuid().ToString();
        }
    }
}