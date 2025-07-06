using System;

namespace Sivar.Erp.Infrastructure.Diagnostics
{
    /// <summary>
    /// Provides context information for performance tracking
    /// </summary>
    public interface IPerformanceContextProvider
    {
        /// <summary>
        /// Gets the current user ID for performance tracking
        /// </summary>
        string? UserId { get; }
        
        /// <summary>
        /// Gets the current user name for performance tracking
        /// </summary>
        string? UserName { get; }
        
        /// <summary>
        /// Gets the current session ID for performance tracking
        /// </summary>
        string? SessionId { get; }
        
        /// <summary>
        /// Gets the current context or operation being tracked
        /// </summary>
        string? Context { get; }
        
        /// <summary>
        /// Gets a unique instance identifier for this provider
        /// </summary>
        string InstanceId { get; }

        // Legacy methods for backward compatibility
        string? GetUserId() => UserId;
        string? GetUserName() => UserName;
        string? GetSessionId() => SessionId;
        string? GetContext() => Context;
        string? GetInstanceId() => InstanceId;
    }
}