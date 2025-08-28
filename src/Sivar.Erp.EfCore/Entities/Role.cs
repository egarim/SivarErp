using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.ErpSystem.Modules.Security.Core;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Roles
/// </summary>
[Table("Roles")]
public class Role : BaseEntity, IRole
{
    /// <summary>
    /// Name of the role
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the role
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the role
    /// </summary>
    [MaxLength(500)]
    public virtual string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a system role
    /// </summary>
    public virtual bool IsSystemRole { get; set; } = false;

    /// <summary>
    /// Role creation date
    /// </summary>
    public virtual DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Permissions (stored as JSON)
    /// </summary>
    public virtual string PermissionsJson { get; set; } = "[]";

    // Interface implementation (not mapped to database)
    [NotMapped]
    public List<string> Permissions 
    { 
        get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(PermissionsJson) ?? new List<string>(); 
        set => PermissionsJson = System.Text.Json.JsonSerializer.Serialize(value); 
    }

    // Read-only interface implementation
    IReadOnlyList<string> IRole.Permissions => Permissions.AsReadOnly();
}
