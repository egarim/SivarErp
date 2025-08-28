using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.ErpSystem.Modules.Security.Core;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Security Events
/// </summary>
[Table("SecurityEvents")]
public class SecurityEvent : BaseEntity, ISecurityEvent
{
    /// <summary>
    /// Unique identifier for the security event
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Action that was performed
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Result of the action
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Details about the event
    /// </summary>
    [MaxLength(2000)]
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// User ID who performed the action
    /// </summary>
    [MaxLength(100)]
    public string? UserId { get; set; }

    /// <summary>
    /// Username who performed the action
    /// </summary>
    [MaxLength(100)]
    public string? UserName { get; set; }

    /// <summary>
    /// IP address from which the action was performed
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of the client
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional data (stored as JSON)
    /// </summary>
    public string? AdditionalDataJson { get; set; }

    // Interface implementation (not mapped to database)
    [NotMapped]
    public Dictionary<string, object> AdditionalData 
    { 
        get => string.IsNullOrEmpty(AdditionalDataJson) ? new Dictionary<string, object>() : 
               System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(AdditionalDataJson) ?? new Dictionary<string, object>(); 
        set => AdditionalDataJson = System.Text.Json.JsonSerializer.Serialize(value); 
    }
}
