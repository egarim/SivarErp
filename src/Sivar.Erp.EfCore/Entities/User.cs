using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.ErpSystem.Modules.Security.Core;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Users
/// </summary>
[Table("Users")]
public class User : BaseEntity, IUser
{
    /// <summary>
    /// Unique user identifier
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Username for login
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Full name (computed property)
    /// </summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Whether the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// User creation date
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login date
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Password hash
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User roles (stored as JSON)
    /// </summary>
    public string RolesJson { get; set; } = "[]";

    /// <summary>
    /// Direct permissions (stored as JSON)
    /// </summary>
    public string DirectPermissionsJson { get; set; } = "[]";

    /// <summary>
    /// Additional properties (stored as JSON)
    /// </summary>
    public string? PropertiesJson { get; set; }

    // Interface implementations (not mapped to database)
    [NotMapped]
    public List<string> Roles 
    { 
        get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(RolesJson) ?? new List<string>(); 
        set => RolesJson = System.Text.Json.JsonSerializer.Serialize(value); 
    }

    [NotMapped]
    public List<string> DirectPermissions 
    { 
        get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(DirectPermissionsJson) ?? new List<string>(); 
        set => DirectPermissionsJson = System.Text.Json.JsonSerializer.Serialize(value); 
    }

    [NotMapped]
    public Dictionary<string, object> Properties 
    { 
        get => string.IsNullOrEmpty(PropertiesJson) ? new Dictionary<string, object>() : 
               System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(PropertiesJson) ?? new Dictionary<string, object>(); 
        set => PropertiesJson = System.Text.Json.JsonSerializer.Serialize(value); 
    }

    // Read-only interface implementations
    IReadOnlyList<string> IUser.Roles => Roles.AsReadOnly();
    IReadOnlyList<string> IUser.DirectPermissions => DirectPermissions.AsReadOnly();

}
