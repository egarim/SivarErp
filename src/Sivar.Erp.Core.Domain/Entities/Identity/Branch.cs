using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities;

namespace Sivar.Erp.Core.Domain.Entities.Identity;

/// <summary>
/// Represents a branch within a company
/// </summary>
public class Branch : BaseEntity, ITenantEntity
{
    /// <summary>
    /// The company this branch belongs to
    /// </summary>
    public Guid CompanyId { get; set; }
    
    /// <summary>
    /// Branch name
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Branch code (unique within company)
    /// </summary>
    [MaxLength(20)]
    public string? Code { get; set; }
    
    /// <summary>
    /// Branch description
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Branch email address
    /// </summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }
    
    /// <summary>
    /// Branch phone number
    /// </summary>
    [MaxLength(50)]
    public string? Phone { get; set; }
    
    /// <summary>
    /// Branch address
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }
    
    /// <summary>
    /// City
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }
    
    /// <summary>
    /// State or province
    /// </summary>
    [MaxLength(100)]
    public string? State { get; set; }
    
    /// <summary>
    /// Country
    /// </summary>
    [MaxLength(100)]
    public string? Country { get; set; }
    
    /// <summary>
    /// Postal code
    /// </summary>
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    /// <summary>
    /// Whether this is the main/headquarters branch
    /// </summary>
    public bool IsHeadquarters { get; set; }
    
    /// <summary>
    /// Whether the branch is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Manager user ID
    /// </summary>
    public Guid? ManagerUserId { get; set; }
    
    /// <summary>
    /// Navigation property to company
    /// </summary>
    public virtual Company Company { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to manager
    /// </summary>
    public virtual User? Manager { get; set; }
}
