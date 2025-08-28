using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Infrastructure.ActivityStream;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework representation of an activity record in the activity stream
/// </summary>
[Table("ActivityRecords")]
[Index(nameof(Date), nameof(Time), Name = "IX_ActivityRecords_DateTime")]
[Index(nameof(ActorType), nameof(ActorId), Name = "IX_ActivityRecords_Actor")]
[Index(nameof(TargetType), nameof(TargetId), Name = "IX_ActivityRecords_Target")]
[Index(nameof(Verb), Name = "IX_ActivityRecords_Verb")]
[Index(nameof(IsPublic), Name = "IX_ActivityRecords_IsPublic")]
public class ActivityRecord : BaseEntity, IActivityRecord
{
    /// <summary>
    /// Type of the actor entity
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string ActorType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the actor entity
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string ActorId { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the actor
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string ActorDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The action that was performed
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Verb { get; set; } = string.Empty;

    /// <summary>
    /// Type of the target entity
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string TargetType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the target entity
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string TargetId { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the target
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string TargetDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Type of the optional object entity
    /// </summary>
    [MaxLength(100)]
    public virtual string? ObjectType { get; set; }

    /// <summary>
    /// ID of the optional object entity
    /// </summary>
    [MaxLength(100)]
    public virtual string? ObjectId { get; set; }

    /// <summary>
    /// Display name of the optional object
    /// </summary>
    [MaxLength(200)]
    public virtual string? ObjectDisplayName { get; set; }

    /// <summary>
    /// Human-readable description of the activity
    /// </summary>
    [Required]
    [MaxLength(500)]
    public virtual string Description { get; set; } = string.Empty;

    /// <summary>
    /// Detailed information about the activity (JSON format)
    /// </summary>
    public virtual string? Details { get; set; }

    /// <summary>
    /// The date when the activity occurred
    /// </summary>
    [Required]
    public virtual DateOnly Date { get; set; }

    /// <summary>
    /// The time when the activity occurred
    /// </summary>
    [Required]
    public virtual TimeOnly Time { get; set; }

    /// <summary>
    /// The timezone identifier where the activity occurred
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string TimeZoneId { get; set; } = "UTC";

    /// <summary>
    /// Whether this activity is public or private
    /// </summary>
    public virtual bool IsPublic { get; set; } = true;

    /// <summary>
    /// URL or resource for additional context
    /// </summary>
    [MaxLength(500)]
    public virtual string? ContextUrl { get; set; }

    /// <summary>
    /// Tags for categorizing and filtering activities (JSON array)
    /// </summary>
    public virtual string? TagsJson { get; set; }

    // Interface implementations
    [NotMapped]
    public IStreamObject Actor 
    { 
        get => new StreamObject { ObjectType = ActorType, ObjectKey = ActorId, DisplayName = ActorDisplayName };
        set 
        { 
            ActorType = value?.ObjectType ?? string.Empty;
            ActorId = value?.ObjectKey ?? string.Empty;
            ActorDisplayName = value?.DisplayName ?? string.Empty;
        }
    }

    [NotMapped]
    public IStreamObject Target 
    { 
        get => new StreamObject { ObjectType = TargetType, ObjectKey = TargetId, DisplayName = TargetDisplayName };
        set 
        { 
            TargetType = value?.ObjectType ?? string.Empty;
            TargetId = value?.ObjectKey ?? string.Empty;
            TargetDisplayName = value?.DisplayName ?? string.Empty;
        }
    }

    [NotMapped]
    public IStreamObject? Object 
    { 
        get => string.IsNullOrEmpty(ObjectType) ? null : 
               new StreamObject { ObjectType = ObjectType, ObjectKey = ObjectId!, DisplayName = ObjectDisplayName! };
        set 
        { 
            ObjectType = value?.ObjectType;
            ObjectId = value?.ObjectKey;
            ObjectDisplayName = value?.DisplayName;
        }
    }

    [NotMapped]
    public ICollection<string> Tags
    {
        get => string.IsNullOrEmpty(TagsJson) ? 
               new List<string>() : 
               System.Text.Json.JsonSerializer.Deserialize<List<string>>(TagsJson) ?? new List<string>();
        set => TagsJson = System.Text.Json.JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public Guid Id
    {
        get => Oid;
        set => Oid = value;
    }
}

/// <summary>
/// Simple implementation of IStreamObject for ActivityRecord
/// </summary>
public class StreamObject : IStreamObject
{
    public string ObjectType { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? DisplayImage { get; set; }
}
