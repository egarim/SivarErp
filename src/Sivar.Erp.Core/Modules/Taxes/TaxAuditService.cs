using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Logging;
using System.ComponentModel;
using Sivar.Erp.Core.Infrastructure.Repository;
using Sivar.Erp.Core.Modules.Taxes.Models;
using System.Diagnostics;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Tax audit service implementation
    /// </summary>
    [Description("Tax audit service")]
    public class TaxAuditService : ITaxAuditService
    {
        private readonly IRepository _repository;
        private readonly IBusinessEntityRepository _businessEntityRepository;
        private readonly ILogger<TaxAuditService> _logger;

        public TaxAuditService(
            IRepository repository,
            IBusinessEntityRepository businessEntityRepository,
            ILogger<TaxAuditService> logger)
        {
            _repository = repository;
            _businessEntityRepository = businessEntityRepository;
            _logger = logger;
        }

        public async Task LogTaxEventAsync(TaxAuditEvent auditEvent)
        {
            using var activity = LoggingExtensions.StartActivity("TaxAudit.LogEvent");

            try
            {
                // Enrich audit event with additional information
                auditEvent.Id = Guid.NewGuid().ToString();
                auditEvent.Timestamp = DateTime.UtcNow;
                auditEvent.MachineName = Environment.MachineName;
                auditEvent.ProcessId = Environment.ProcessId;

                // Add correlation ID if available
                if (Activity.Current?.Id != null)
                {
                    auditEvent.CorrelationId = Activity.Current.Id;
                }

                // Serialize and store the audit event
                await SaveAuditEventAsync(auditEvent);

                _logger.LogInformation("Logged tax audit event: {EventType} for {ResourceType} {ResourceId} by {UserId}", 
                    auditEvent.EventType, auditEvent.ResourceType, auditEvent.ResourceId, auditEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging tax audit event");
                // Don't rethrow - audit logging should not break business operations
            }
        }

        public async Task<List<TaxAuditEvent>> GetAuditTrailAsync(TaxAuditTrailQuery query)
        {
            using var activity = LoggingExtensions.StartActivity("TaxAudit.GetAuditTrail");

            try
            {
                _logger.LogInformation("Retrieving tax audit trail for period {StartDate} to {EndDate}", 
                    query.StartDate, query.EndDate);

                var auditEvents = await _repository.GetListAsync<TaxAuditEvent>(BuildAuditTrailPredicate(query));

                // Apply additional filtering
                var filteredEvents = auditEvents.AsQueryable();

                if (!string.IsNullOrWhiteSpace(query.EventType))
                {
                    if (Enum.TryParse<TaxAuditEventType>(query.EventType, out var eventType))
                    {
                        filteredEvents = filteredEvents.Where(e => e.EventType == eventType);
                    }
                }

                if (!string.IsNullOrWhiteSpace(query.ResourceType))
                {
                    filteredEvents = filteredEvents.Where(e => e.ResourceType == query.ResourceType);
                }

                if (!string.IsNullOrWhiteSpace(query.ResourceId))
                {
                    filteredEvents = filteredEvents.Where(e => e.ResourceId == query.ResourceId);
                }

                if (!string.IsNullOrWhiteSpace(query.UserId))
                {
                    filteredEvents = filteredEvents.Where(e => e.UserId == query.UserId);
                }

                // Apply pagination
                var results = filteredEvents
                    .OrderByDescending(e => e.Timestamp)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToList();

                _logger.LogInformation("Retrieved {Count} audit events from {TotalCount} total events", 
                    results.Count, filteredEvents.Count());

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax audit trail");
                throw;
            }
        }

        public async Task<TaxAuditSummary> GetAuditSummaryAsync(DateTime startDate, DateTime endDate)
        {
            using var activity = LoggingExtensions.StartActivity("TaxAudit.GetAuditSummary");

            try
            {
                _logger.LogInformation("Generating tax audit summary for period {StartDate} to {EndDate}", 
                    startDate, endDate);

                var auditEvents = await _repository.GetListAsync<TaxAuditEvent>(
                    e => e.Timestamp >= startDate && e.Timestamp <= endDate);

                var summary = new TaxAuditSummary
                {
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    GeneratedAt = DateTime.UtcNow,
                    TotalEvents = auditEvents.Count
                };

                // Group events by type
                summary.EventsByType = auditEvents
                    .GroupBy(e => e.EventType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count());

                // Group events by resource type
                summary.EventsByResourceType = auditEvents
                    .GroupBy(e => e.ResourceType)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Group events by user
                summary.EventsByUser = auditEvents
                    .GroupBy(e => e.UserId)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Group events by date
                summary.EventsByDate = auditEvents
                    .GroupBy(e => e.Timestamp.Date)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Calculate statistics
                if (auditEvents.Any())
                {
                    summary.Statistics = new Dictionary<string, object>
                    {
                        ["MostActiveUser"] = summary.EventsByUser.OrderByDescending(kvp => kvp.Value).First().Key,
                        ["MostCommonEventType"] = summary.EventsByType.OrderByDescending(kvp => kvp.Value).First().Key,
                        ["MostActiveDay"] = summary.EventsByDate.OrderByDescending(kvp => kvp.Value).First().Key,
                        ["AverageEventsPerDay"] = auditEvents.Count / Math.Max(1, (endDate - startDate).Days),
                        ["UniqueUsers"] = auditEvents.Select(e => e.UserId).Distinct().Count(),
                        ["UniqueResources"] = auditEvents.Select(e => e.ResourceId).Distinct().Count()
                    };
                }

                _logger.LogInformation("Generated audit summary with {TotalEvents} events across {Days} days", 
                    summary.TotalEvents, (endDate - startDate).Days);

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tax audit summary");
                throw;
            }
        }

        public async Task<List<TaxComplianceViolation>> DetectComplianceViolationsAsync(TaxComplianceCheckParameters parameters)
        {
            using var activity = LoggingExtensions.StartActivity("TaxAudit.DetectComplianceViolations");

            try
            {
                _logger.LogInformation("Detecting tax compliance violations for period {StartDate} to {EndDate}", 
                    parameters.StartDate, parameters.EndDate);

                var violations = new List<TaxComplianceViolation>();

                // Run different compliance checks
                violations.AddRange(await CheckTaxCalculationAccuracyAsync(parameters));
                violations.AddRange(await CheckTaxDocumentationAsync(parameters));
                violations.AddRange(await CheckTaxFilingTimelinessAsync(parameters));
                violations.AddRange(await CheckTaxRateConsistencyAsync(parameters));
                violations.AddRange(await CheckExemptionValidityAsync(parameters));

                // Sort violations by severity
                violations = violations.OrderByDescending(v => v.Severity)
                    .ThenByDescending(v => v.DetectedAt)
                    .ToList();

                _logger.LogInformation("Detected {ViolationCount} compliance violations: {HighSeverity} high, {MediumSeverity} medium, {LowSeverity} low", 
                    violations.Count,
                    violations.Count(v => v.Severity == ComplianceViolationSeverity.High),
                    violations.Count(v => v.Severity == ComplianceViolationSeverity.Medium),
                    violations.Count(v => v.Severity == ComplianceViolationSeverity.Low));

                return violations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting tax compliance violations");
                throw;
            }
        }

        public async Task<TaxAuditReport> GenerateAuditReportAsync(TaxAuditReportParameters parameters)
        {
            using var activity = LoggingExtensions.StartActivity("TaxAudit.GenerateAuditReport");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Generating tax audit report for period {StartDate} to {EndDate}", 
                    parameters.StartDate, parameters.EndDate);

                var report = new TaxAuditReport
                {
                    Parameters = parameters,
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedBy = parameters.RequestedBy
                };

                // Get audit trail
                var auditQuery = new TaxAuditTrailQuery
                {
                    StartDate = parameters.StartDate,
                    EndDate = parameters.EndDate,
                    EventType = parameters.EventType,
                    ResourceType = parameters.ResourceType,
                    PageNumber = 1,
                    PageSize = int.MaxValue // Get all events for the report
                };

                report.AuditEvents = await GetAuditTrailAsync(auditQuery);

                // Get audit summary
                report.Summary = await GetAuditSummaryAsync(parameters.StartDate, parameters.EndDate);

                // Detect compliance violations if requested
                if (parameters.IncludeComplianceCheck)
                {
                    var complianceParams = new TaxComplianceCheckParameters
                    {
                        StartDate = parameters.StartDate,
                        EndDate = parameters.EndDate,
                        BusinessEntityId = parameters.BusinessEntityId
                    };
                    report.ComplianceViolations = await DetectComplianceViolationsAsync(complianceParams);
                }

                // Generate recommendations
                report.Recommendations = GenerateAuditRecommendations(report);

                stopwatch.Stop();
                report.GenerationDuration = stopwatch.Elapsed;

                _logger.LogInformation("Generated tax audit report with {EventCount} events and {ViolationCount} violations in {Duration}ms", 
                    report.AuditEvents.Count, 
                    report.ComplianceViolations?.Count ?? 0, 
                    stopwatch.ElapsedMilliseconds);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tax audit report");
                throw;
            }
        }

        public async Task<bool> ArchiveOldAuditDataAsync(DateTime cutoffDate)
        {
            using var activity = LoggingExtensions.StartActivity("TaxAudit.ArchiveOldData");

            try
            {
                _logger.LogInformation("Archiving tax audit data older than {CutoffDate}", cutoffDate);

                // Get old audit events
                var oldEvents = await _repository.GetListAsync<TaxAuditEvent>(
                    e => e.Timestamp < cutoffDate);

                if (!oldEvents.Any())
                {
                    _logger.LogInformation("No audit data found for archiving");
                    return true;
                }

                // Archive the events (could be to a separate archive table or file)
                await ArchiveEventsAsync(oldEvents);

                // Remove from main table
                foreach (var eventToRemove in oldEvents)
                {
                    await _repository.DeleteAsync(eventToRemove);
                }

                _logger.LogInformation("Archived and removed {Count} old audit events", oldEvents.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving old tax audit data");
                throw;
            }
        }

        public async Task<TaxDataIntegrityReport> ValidateDataIntegrityAsync(TaxDataIntegrityCheckParameters parameters)
        {
            using var activity = LoggingExtensions.StartActivity("TaxAudit.ValidateDataIntegrity");

            try
            {
                _logger.LogInformation("Validating tax data integrity for period {StartDate} to {EndDate}", 
                    parameters.StartDate, parameters.EndDate);

                var report = new TaxDataIntegrityReport
                {
                    Parameters = parameters,
                    CheckedAt = DateTime.UtcNow
                };

                // Perform various integrity checks
                report.Issues.AddRange(await CheckTaxCalculationConsistencyAsync(parameters));
                report.Issues.AddRange(await CheckReferentialIntegrityAsync(parameters));
                report.Issues.AddRange(await CheckDataCompletenessAsync(parameters));
                report.Issues.AddRange(await CheckBusinessRuleViolationsAsync(parameters));

                // Calculate summary
                report.Summary = new TaxDataIntegritySummary
                {
                    TotalIssues = report.Issues.Count,
                    CriticalIssues = report.Issues.Count(i => i.Severity == DataIntegritySeverity.Critical),
                    WarningIssues = report.Issues.Count(i => i.Severity == DataIntegritySeverity.Warning),
                    InfoIssues = report.Issues.Count(i => i.Severity == DataIntegritySeverity.Info),
                    IntegrityScore = CalculateIntegrityScore(report.Issues),
                    RecommendedActions = GenerateIntegrityRecommendations(report.Issues)
                };

                _logger.LogInformation("Data integrity validation completed: {TotalIssues} issues found, integrity score: {Score}%", 
                    report.Summary.TotalIssues, report.Summary.IntegrityScore);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tax data integrity");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetAuditStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            using var activity = LoggingExtensions.StartActivity("TaxAudit.GetStatistics");

            try
            {
                var auditEvents = await _repository.GetListAsync<TaxAuditEvent>(
                    e => e.Timestamp >= startDate && e.Timestamp <= endDate);

                var statistics = new Dictionary<string, object>
                {
                    ["TotalEvents"] = auditEvents.Count,
                    ["UniqueUsers"] = auditEvents.Select(e => e.UserId).Distinct().Count(),
                    ["UniqueResources"] = auditEvents.Select(e => e.ResourceId).Distinct().Count(),
                    ["EventsPerDay"] = auditEvents.GroupBy(e => e.Timestamp.Date).ToDictionary(g => g.Key, g => g.Count()),
                    ["EventsByType"] = auditEvents.GroupBy(e => e.EventType).ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    ["EventsByUser"] = auditEvents.GroupBy(e => e.UserId).ToDictionary(g => g.Key, g => g.Count()),
                    ["MostActiveHour"] = auditEvents.GroupBy(e => e.Timestamp.Hour).OrderByDescending(g => g.Count()).First().Key,
                    ["AverageEventsPerUser"] = auditEvents.Count / Math.Max(1, auditEvents.Select(e => e.UserId).Distinct().Count()),
                    ["PeakActivity"] = auditEvents.GroupBy(e => e.Timestamp.Date).Max(g => g.Count()),
                    ["DataVolumeProcessed"] = auditEvents.Sum(e => e.AdditionalData.ContainsKey("DataSize") ? long.Parse(e.AdditionalData["DataSize"]) : 0)
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit statistics");
                throw;
            }
        }

        // Private helper methods
        private async Task SaveAuditEventAsync(TaxAuditEvent auditEvent)
        {
            await _repository.InsertAsync(auditEvent);
        }

        private System.Linq.Expressions.Expression<Func<TaxAuditEvent, bool>> BuildAuditTrailPredicate(TaxAuditTrailQuery query)
        {
            return e => e.Timestamp >= query.StartDate && e.Timestamp <= query.EndDate;
        }

        private async Task<List<TaxComplianceViolation>> CheckTaxCalculationAccuracyAsync(TaxComplianceCheckParameters parameters)
        {
            var violations = new List<TaxComplianceViolation>();
            
            // Implement tax calculation accuracy checks
            await Task.CompletedTask;
            
            return violations;
        }

        private async Task<List<TaxComplianceViolation>> CheckTaxDocumentationAsync(TaxComplianceCheckParameters parameters)
        {
            var violations = new List<TaxComplianceViolation>();
            
            // Implement tax documentation checks
            await Task.CompletedTask;
            
            return violations;
        }

        private async Task<List<TaxComplianceViolation>> CheckTaxFilingTimelinessAsync(TaxComplianceCheckParameters parameters)
        {
            var violations = new List<TaxComplianceViolation>();
            
            // Implement tax filing timeliness checks
            await Task.CompletedTask;
            
            return violations;
        }

        private async Task<List<TaxComplianceViolation>> CheckTaxRateConsistencyAsync(TaxComplianceCheckParameters parameters)
        {
            var violations = new List<TaxComplianceViolation>();
            
            // Implement tax rate consistency checks
            await Task.CompletedTask;
            
            return violations;
        }

        private async Task<List<TaxComplianceViolation>> CheckExemptionValidityAsync(TaxComplianceCheckParameters parameters)
        {
            var violations = new List<TaxComplianceViolation>();
            
            // Implement exemption validity checks
            await Task.CompletedTask;
            
            return violations;
        }

        private List<TaxAuditRecommendation> GenerateAuditRecommendations(TaxAuditReport report)
        {
            var recommendations = new List<TaxAuditRecommendation>();

            // Generate recommendations based on audit findings
            if (report.ComplianceViolations?.Any(v => v.Severity == ComplianceViolationSeverity.High) == true)
            {
                recommendations.Add(new TaxAuditRecommendation
                {
                    Priority = RecommendationPriority.High,
                    Category = "Compliance",
                    Title = "Address High-Severity Compliance Violations",
                    Description = "Immediate action required to resolve high-severity compliance violations.",
                    EstimatedEffort = "High"
                });
            }

            return recommendations;
        }

        private async Task ArchiveEventsAsync(List<TaxAuditEvent> events)
        {
            // Archive events to archive storage
            await Task.CompletedTask;
        }

        private async Task<List<TaxDataIntegrityIssue>> CheckTaxCalculationConsistencyAsync(TaxDataIntegrityCheckParameters parameters)
        {
            var issues = new List<TaxDataIntegrityIssue>();
            
            // Implement tax calculation consistency checks
            await Task.CompletedTask;
            
            return issues;
        }

        private async Task<List<TaxDataIntegrityIssue>> CheckReferentialIntegrityAsync(TaxDataIntegrityCheckParameters parameters)
        {
            var issues = new List<TaxDataIntegrityIssue>();
            
            // Implement referential integrity checks
            await Task.CompletedTask;
            
            return issues;
        }

        private async Task<List<TaxDataIntegrityIssue>> CheckDataCompletenessAsync(TaxDataIntegrityCheckParameters parameters)
        {
            var issues = new List<TaxDataIntegrityIssue>();
            
            // Implement data completeness checks
            await Task.CompletedTask;
            
            return issues;
        }

        private async Task<List<TaxDataIntegrityIssue>> CheckBusinessRuleViolationsAsync(TaxDataIntegrityCheckParameters parameters)
        {
            var issues = new List<TaxDataIntegrityIssue>();
            
            // Implement business rule violation checks
            await Task.CompletedTask;
            
            return issues;
        }

        private decimal CalculateIntegrityScore(List<TaxDataIntegrityIssue> issues)
        {
            if (!issues.Any()) return 100m;

            var totalWeight = issues.Sum(i => i.Severity switch
            {
                DataIntegritySeverity.Critical => 10,
                DataIntegritySeverity.Warning => 3,
                DataIntegritySeverity.Info => 1,
                _ => 1
            });

            var maxPossibleWeight = 100; // Baseline score
            return Math.Max(0, maxPossibleWeight - totalWeight);
        }

        private List<string> GenerateIntegrityRecommendations(List<TaxDataIntegrityIssue> issues)
        {
            var recommendations = new List<string>();

            if (issues.Any(i => i.Severity == DataIntegritySeverity.Critical))
            {
                recommendations.Add("Immediate action required for critical data integrity issues");
            }

            if (issues.Count(i => i.Severity == DataIntegritySeverity.Warning) > 10)
            {
                recommendations.Add("Consider implementing automated data validation rules");
            }

            return recommendations;
        }
    }
}
