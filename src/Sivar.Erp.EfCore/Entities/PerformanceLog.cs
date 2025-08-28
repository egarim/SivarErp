using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Performance Logs
/// </summary>
[Table("PerformanceLogs")]
public class PerformanceLog : BaseEntity
{
    /// <summary>
    /// Name of the operation being logged
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the operation in milliseconds
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Start time of the operation
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time of the operation
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Additional details about the operation
    /// </summary>
    [MaxLength(1000)]
    public string? Details { get; set; }

    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// User who initiated the operation
    /// </summary>
    [MaxLength(100)]
    public string? UserId { get; set; }

    /// <summary>
    /// IP address from which the operation was initiated
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }
}
