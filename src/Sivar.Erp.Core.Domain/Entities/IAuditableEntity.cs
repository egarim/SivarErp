namespace Sivar.Erp.Core.Domain.Entities;

/// <summary>
/// Interface for entities that support comprehensive audit logging
/// Integration with ErpLoggingService
/// </summary>
public interface IAuditableEntity : IEntity
{
    /// <summary>
    /// Additional audit information in JSON format
    /// </summary>
    string? AuditData { get; set; }
    
    /// <summary>
    /// The source of the change (API, UI, Import, etc.)
    /// </summary>
    string? ChangeSource { get; set; }
    
    /// <summary>
    /// IP address of the change origin
    /// </summary>
    string? ChangeIpAddress { get; set; }
    
    /// <summary>
    /// User agent information
    /// </summary>
    string? ChangeUserAgent { get; set; }
}
