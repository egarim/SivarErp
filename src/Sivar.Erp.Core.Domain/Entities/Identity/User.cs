using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Core.Domain.Entities.Identity;

/// <summary>
/// Represents a user reference from Keycloak
/// This is not the full user data but a reference to the Keycloak user
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Keycloak user ID (sub claim)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string KeycloakUserId { get; set; } = string.Empty;
    
    /// <summary>
    /// User's email address (cached from Keycloak)
    /// </summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User's display name (cached from Keycloak)
    /// </summary>
    [MaxLength(255)]
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// User's first name (cached from Keycloak)
    /// </summary>
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    /// <summary>
    /// User's last name (cached from Keycloak)
    /// </summary>
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    /// <summary>
    /// User's preferred language/culture
    /// </summary>
    [MaxLength(10)]
    public string PreferredLanguage { get; set; } = "en-US";
    
    /// <summary>
    /// Last time the user was active in the system
    /// </summary>
    public DateTime? LastActiveAt { get; set; }
    
    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Companies this user is associated with
    /// </summary>
    public virtual ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    
    /// <summary>
    /// Roles assigned to this user
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
    /// <summary>
    /// Custom permissions granted to this user
    /// </summary>
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    
    /// <summary>
    /// Invitations sent by this user
    /// </summary>
    public virtual ICollection<UserInvitation> SentInvitations { get; set; } = new List<UserInvitation>();
    
    /// <summary>
    /// Invitations received by this user
    /// </summary>
    public virtual ICollection<UserInvitation> ReceivedInvitations { get; set; } = new List<UserInvitation>();
}
