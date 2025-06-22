using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.Services;
using Microsoft.Extensions.Logging;

namespace Sivar.Erp.ErpSystem.Modules.Security.Services
{
    public class ObjectDbAuditService : IAuditService
    {
        private readonly IObjectDb _objectDb;
        private readonly ISecurityContext _securityContext;
        private readonly ILogger<ObjectDbAuditService> _logger;

        public ObjectDbAuditService(
            IObjectDb objectDb,
            ISecurityContext securityContext,
            ILogger<ObjectDbAuditService> logger)
        {
            _objectDb = objectDb;
            _securityContext = securityContext;
            _logger = logger;
        }

        public async Task LogAsync(string action, string result, string details, string? userId = null)
        {
            var securityEvent = new SecurityEvent
            {
                Action = action,
                Result = result,
                Details = details,
                UserId = userId ?? _securityContext.UserId,
                UserName = _securityContext.UserName,
                Timestamp = DateTime.UtcNow
            };

            await LogSecurityEventAsync(securityEvent);
        }

        public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
        {
            try
            {
                _objectDb.SecurityEvents.Add(securityEvent);

                // Also log to structured logging
                _logger.LogInformation("Security Event: {Action} | {Result} | User: {UserName} | Details: {Details}",
                    securityEvent.Action, securityEvent.Result, securityEvent.UserName, securityEvent.Details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event: {Action} | {Result}",
                    securityEvent.Action, securityEvent.Result);
            }

            await Task.CompletedTask;
        }

        public async Task<IEnumerable<SecurityEvent>> GetSecurityEventsAsync(DateTime fromDate, DateTime toDate, string? userId = null)
        {
            var events = _objectDb.SecurityEvents
                .Where(e => e.Timestamp >= fromDate && e.Timestamp <= toDate);

            if (!string.IsNullOrEmpty(userId))
            {
                events = events.Where(e => e.UserId == userId);
            }

            return await Task.FromResult(events.OrderByDescending(e => e.Timestamp));
        }
    }
}
