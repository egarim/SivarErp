using System;

namespace Sivar.Erp.ErpSystem.Diagnostics
{
    /// <summary>
    /// Default implementation of IPerformanceContextProvider for testing and general use.
    /// Provides basic context information without platform-specific dependencies.
    /// </summary>
    public class DefaultPerformanceContextProvider : IPerformanceContextProvider
    {
        private static readonly string _instanceId = Environment.MachineName + "_" + Guid.NewGuid().ToString("N")[..8];

        private readonly string? _userId;
        private readonly string? _userName;
        private readonly string? _sessionId;
        private readonly string? _context;

        public DefaultPerformanceContextProvider(
            string? userId = null,
            string? userName = null,
            string? sessionId = null,
            string? context = null)
        {
            _userId = userId;
            _userName = userName;
            _sessionId = sessionId;
            _context = context;
        }

        public string? GetUserId() => _userId;

        public string? GetUserName() => _userName;

        public string GetInstanceId() => _instanceId;

        public string? GetSessionId() => _sessionId;

        public string? GetContext() => _context;
    }
}
