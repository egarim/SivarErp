using System;

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Interface for performance context provider
    /// </summary>
    public interface IPerformanceContextProvider
    {
        string? UserId { get; }
        string? UserName { get; }
        string? SessionId { get; }
        string? InstanceId { get; }
        string? Context { get; }
    }

    /// <summary>
    /// Interface for stream objects in activity stream
    /// </summary>
    public interface IStreamObject
    {
        string ObjectType { get; set; }
        string ObjectKey { get; set; }
        string DisplayName { get; set; }
        string? DisplayImage { get; set; }
    }
}