using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Domain.Entities.Identity;

/// <summary>
/// Represents an invitation for a user to join a company
/// </summary>
public class UserInvitation : BaseEntity, ITenantEntity
{
    /// <summary>
    /// The company this invitation is for
    /// </summary>
    public Guid CompanyId { get; set; }
    
    /// <summary>
    /// Email address of the invited user
    /// </summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User who sent the invitation
    /// </summary>
    public Guid InvitedByUserId { get; set; }
    
    /// <summary>
    /// User who was invited (if they already exist in the system)
    /// </summary>
    public Guid? InvitedUserId { get; set; }
    
    /// <summary>
    /// Role the invited user will have in the company
    /// </summary>
    public CompanyRole Role { get; set; } = CompanyRole.User;
    
    /// <summary>
    /// Current status of the invitation
    /// </summary>
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    
    /// <summary>
    /// Unique invitation token
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// When the invitation expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// When the invitation was sent
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the invitation was responded to
    /// </summary>
    public DateTime? RespondedAt { get; set; }
    
    /// <summary>
    /// When the invitation was accepted
    /// </summary>
    public DateTime? AcceptedAt { get; set; }
    
    /// <summary>
    /// When the invitation was declined
    /// </summary>
    public DateTime? DeclinedAt { get; set; }
    
    /// <summary>
    /// Personal message included with the invitation
    /// </summary>
    [MaxLength(1000)]
    public string? Message { get; set; }
    
    /// <summary>
    /// Specific branches the invited user will have access to
    /// </summary>
    [MaxLength(1000)]
    public string? BranchAccessJson { get; set; }
    
    /// <summary>
    /// Custom permissions for the invited user
    /// </summary>
    [MaxLength(2000)]
    public string? CustomPermissionsJson { get; set; }
    
    /// <summary>
    /// Number of reminder emails sent
    /// </summary>
    public int ReminderCount { get; set; } = 0;
    
    /// <summary>
    /// Last time a reminder was sent
    /// </summary>
    public DateTime? LastReminderAt { get; set; }
    
    /// <summary>
    /// Navigation property to company
    /// </summary>
    public virtual Company Company { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the user who sent the invitation
    /// </summary>
    public virtual User InvitedByUser { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the invited user (if they exist)
    /// </summary>
    public virtual User? InvitedUser { get; set; }
    
    /// <summary>
    /// Checks if the invitation is still valid (not expired and status is pending)
    /// </summary>
    public bool IsValid => Status == InvitationStatus.Pending && ExpiresAt > DateTime.UtcNow;
    
    /// <summary>
    /// Marks the invitation as expired if past expiration date
    /// </summary>
    public void UpdateExpiredStatus()
    {
        if (Status == InvitationStatus.Pending && ExpiresAt <= DateTime.UtcNow)
        {
            Status = InvitationStatus.Expired;
            RespondedAt = DateTime.UtcNow;
        }
    }
}
