using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities;

namespace Sivar.Erp.Core.Domain.Entities.Identity;

/// <summary>
/// Represents a custom permission assigned to a user within a specific company
/// </summary>
public class UserPermission : BaseEntity, ITenantEntity
{
    /// <summary>
    /// The company this permission assignment belongs to
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// The user this permission is assigned to
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The permission name (from ErpPermissions constants)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PermissionName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this permission is granted (true) or explicitly denied (false)
    /// </summary>
    public bool IsGranted { get; set; } = true;

    /// <summary>
    /// Optional branch this permission is specific to (null means company-wide)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// When this permission assignment becomes effective
    /// </summary>
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this permission assignment expires (null means no expiration)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Whether this permission is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional notes about this permission assignment
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Who granted this permission
    /// </summary>
    public Guid? GrantedByUserId { get; set; }

    /// <summary>
    /// When this permission was granted
    /// </summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// The company this permission belongs to
    /// </summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>
    /// The user this permission is assigned to
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// The branch this permission is specific to (if any)
    /// </summary>
    public virtual Branch? Branch { get; set; }

    /// <summary>
    /// The user who granted this permission
    /// </summary>
    public virtual User? GrantedByUser { get; set; }

    /// <summary>
    /// Check if this permission is currently valid based on effective dates and active status
    /// </summary>
    public bool IsValid()
    {
        var now = DateTime.UtcNow;
        return IsActive && 
               EffectiveFrom <= now && 
               (EffectiveTo == null || EffectiveTo > now);
    }

    /// <summary>
    /// Check if this permission is valid at a specific date
    /// </summary>
    public bool IsValidAt(DateTime date)
    {
        return IsActive && 
               EffectiveFrom <= date && 
               (EffectiveTo == null || EffectiveTo > date);
    }

    /// <summary>
    /// Check if this permission applies to a specific branch
    /// </summary>
    public bool AppliesTo(Guid? branchId)
    {
        // Company-wide permission (BranchId is null) applies to all branches
        // Branch-specific permission applies only to that branch
        return BranchId == null || BranchId == branchId;
    }

    /// <summary>
    /// Get the scope description of this permission
    /// </summary>
    public string GetScopeDescription()
    {
        if (BranchId == null)
            return "Company-wide";
        
        return $"Branch-specific (ID: {BranchId})";
    }
}
