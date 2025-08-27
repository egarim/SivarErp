using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Sivar.Erp.Core.Infrastructure.Performance;
using Sivar.Erp.Core.Infrastructure.Logging;
using System.ComponentModel;

namespace Sivar.Erp.Core.Infrastructure.Performance;

/// <summary>
/// EF Core interceptor for monitoring database query performance
/// </summary>
[Description("EF Core interceptor for monitoring database query performance")]
public class EfCorePerformanceInterceptor : DbCommandInterceptor
{
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly IErpLoggingService _loggingService;
    private const int SlowQueryThresholdMs = 1000;

    public EfCorePerformanceInterceptor(
        IPerformanceMonitor performanceMonitor,
        IErpLoggingService loggingService)
    {
        _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }

    /// <summary>
    /// Intercepts command execution to track performance
    /// </summary>
    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        await LogQueryPerformance(command, eventData);
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Intercepts scalar command execution to track performance
    /// </summary>
    public override async ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        await LogQueryPerformance(command, eventData);
        return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Intercepts non-query command execution to track performance
    /// </summary>
    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await LogQueryPerformance(command, eventData);
        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Synchronous reader execution tracking
    /// </summary>
    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        LogQueryPerformance(command, eventData).GetAwaiter().GetResult();
        return base.ReaderExecuted(command, eventData, result);
    }

    /// <summary>
    /// Synchronous scalar execution tracking
    /// </summary>
    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        LogQueryPerformance(command, eventData).GetAwaiter().GetResult();
        return base.ScalarExecuted(command, eventData, result);
    }

    /// <summary>
    /// Synchronous non-query execution tracking
    /// </summary>
    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        LogQueryPerformance(command, eventData).GetAwaiter().GetResult();
        return base.NonQueryExecuted(command, eventData, result);
    }

    /// <summary>
    /// Logs query performance metrics
    /// </summary>
    private async Task LogQueryPerformance(DbCommand command, CommandExecutedEventData eventData)
    {
        var duration = eventData.Duration;
        var commandType = GetCommandType(command.CommandText);
        var operationName = $"EfCore.{commandType}";

        // Record performance metrics
        _performanceMonitor.RecordOperation(operationName, duration, !eventData.IsAsync || eventData.IsAsync);

        // Log performance metrics
        var durationMs = duration.TotalMilliseconds;
        _loggingService.LogPerformanceMetrics(operationName, duration);

        // Log slow queries
        if (durationMs > SlowQueryThresholdMs)
        {
            _loggingService.LogBusinessEntityOperation(
                "SlowQuery", 
                TruncateQuery(command.CommandText),
                "DatabaseQuery", 
                "System",
                new { Duration = durationMs, CommandType = commandType, ParameterCount = command.Parameters.Count });
        }

        // Log detailed query info for debugging
        _loggingService.LogBusinessEntityOperation(
            "QueryExecution",
            TruncateQuery(command.CommandText),
            "DatabaseQuery",
            "System",
            new { Duration = durationMs, CommandType = commandType });
    }

    /// <summary>
    /// Determines the type of database command
    /// </summary>
    private static string GetCommandType(string commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText))
            return "Unknown";

        var trimmed = commandText.Trim().ToUpper();
        
        if (trimmed.StartsWith("SELECT"))
            return "Select";
        if (trimmed.StartsWith("INSERT"))
            return "Insert";
        if (trimmed.StartsWith("UPDATE"))
            return "Update";
        if (trimmed.StartsWith("DELETE"))
            return "Delete";
        if (trimmed.StartsWith("CREATE"))
            return "Create";
        if (trimmed.StartsWith("ALTER"))
            return "Alter";
        if (trimmed.StartsWith("DROP"))
            return "Drop";

        return "Other";
    }

    /// <summary>
    /// Truncates long query text for logging
    /// </summary>
    private static string TruncateQuery(string query, int maxLength = 500)
    {
        if (string.IsNullOrEmpty(query) || query.Length <= maxLength)
            return query;

        return query.Substring(0, maxLength) + "...";
    }
}
