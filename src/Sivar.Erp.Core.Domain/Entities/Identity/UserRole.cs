using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities;

namespace Sivar.Erp.Core.Domain.Entities.Identity;

/// <summary>
/// Represents a user's role within a specific company
/// </summary>
public class UserRole : BaseEntity, ITenantEntity
{
    /// <summary>
    /// The company this role assignment belongs to
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// The user this role is assigned to
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The role name (from ErpRoles constants)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Optional branch this role is specific to (null means company-wide)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Whether this role assignment is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When this role was assigned
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who assigned this role
    /// </summary>
    [MaxLength(255)]
    public string? AssignedBy { get; set; }

    /// <summary>
    /// When this role expires (null for no expiration)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Notes about this role assignment
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Company Company { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Branch? Branch { get; set; }

    /// <summary>
    /// Checks if this role assignment is currently valid
    /// </summary>
    public bool IsValid()
    {
        return IsActive && (ExpiresAt == null || ExpiresAt > DateTime.UtcNow);
    }

    /// <summary>
    /// Gets the scope of this role (Company-wide or Branch-specific)
    /// </summary>
    public string GetScope()
    {
        return BranchId.HasValue ? "Branch" : "Company";
    }
}
