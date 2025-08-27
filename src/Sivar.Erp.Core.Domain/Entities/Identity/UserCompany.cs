using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Domain.Entities.Identity;

/// <summary>
/// Represents the association between a user and a company with roles
/// </summary>
public class UserCompany : BaseEntity, ITenantEntity
{
    /// <summary>
    /// The user ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// The company ID
    /// </summary>
    public Guid CompanyId { get; set; }
    
    /// <summary>
    /// The user's role within this company
    /// </summary>
    public CompanyRole Role { get; set; } = CompanyRole.User;
    
    /// <summary>
    /// Whether the user is the owner of this company
    /// </summary>
    public bool IsOwner { get; set; }
    
    /// <summary>
    /// Whether the user account is active within this company
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Date when the user was invited to this company
    /// </summary>
    public DateTime? InvitedAt { get; set; }
    
    /// <summary>
    /// Date when the user joined the company
    /// </summary>
    public DateTime JoinedAt { get; set; }
    
    /// <summary>
    /// Date when the user accepted the invitation
    /// </summary>
    public DateTime? AcceptedAt { get; set; }
    
    /// <summary>
    /// Date when the user was deactivated (if applicable)
    /// </summary>
    public DateTime? DeactivatedAt { get; set; }
    
    /// <summary>
    /// User who invited this user to the company
    /// </summary>
    public Guid? InvitedByUserId { get; set; }
    
    /// <summary>
    /// Specific branches this user has access to (empty means all branches)
    /// </summary>
    [MaxLength(1000)]
    public string? BranchAccessJson { get; set; }
    
    /// <summary>
    /// Custom permissions JSON for this user in this company
    /// </summary>
    [MaxLength(2000)]
    public string? CustomPermissionsJson { get; set; }
    
    /// <summary>
    /// Navigation property to user
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to company
    /// </summary>
    public virtual Company Company { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the user who invited this user
    /// </summary>
    public virtual User? InvitedByUser { get; set; }
}
