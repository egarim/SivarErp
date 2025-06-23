using System;

namespace Sivar.Erp.ErpSystem.Diagnostics
{
    /// <summary>
    /// Platform-agnostic interface for providing performance logging context information.
    /// Implementations can vary by platform (ASP.NET Core, WPF, Console, etc.)
    /// </summary>
    public interface IPerformanceContextProvider
    {
        /// <summary>
        /// Gets the current user identifier
        /// </summary>
        string? GetUserId();

        /// <summary>
        /// Gets the current user display name
        /// </summary>
        string? GetUserName();

        /// <summary>
        /// Gets the application instance identifier
        /// </summary>
        string GetInstanceId();

        /// <summary>
        /// Gets the current session identifier (if applicable)
        /// </summary>
        string? GetSessionId();

        /// <summary>
        /// Gets additional context information
        /// </summary>
        string? GetContext();
    }
}
