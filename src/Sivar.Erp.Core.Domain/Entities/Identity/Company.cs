using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Core.Domain.Entities.Identity;

/// <summary>
/// Represents a company (tenant) in the system
/// </summary>
public class Company : BaseEntity
{
    /// <summary>
    /// Company name
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Company description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Company industry
    /// </summary>
    [MaxLength(100)]
    public string? Industry { get; set; }
    
    /// <summary>
    /// Company legal name
    /// </summary>
    [MaxLength(255)]
    public string? LegalName { get; set; }
    
    /// <summary>
    /// Tax identification number
    /// </summary>
    [MaxLength(50)]
    public string? TaxId { get; set; }
    
    /// <summary>
    /// Company registration number
    /// </summary>
    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }
    
    /// <summary>
    /// Company email address
    /// </summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }
    
    /// <summary>
    /// Company phone number
    /// </summary>
    [MaxLength(50)]
    public string? Phone { get; set; }
    
    /// <summary>
    /// Company website
    /// </summary>
    [MaxLength(255)]
    public string? Website { get; set; }
    
    /// <summary>
    /// Company address
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
    /// Company logo URL
    /// </summary>
    [MaxLength(500)]
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Default currency for the company
    /// </summary>
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Default language for the company
    /// </summary>
    [MaxLength(10)]
    public string DefaultLanguage { get; set; } = "en-US";
    
    /// <summary>
    /// Company timezone
    /// </summary>
    [MaxLength(100)]
    public string TimeZone { get; set; } = "UTC";
    
    /// <summary>
    /// Whether the company is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Database name for this company (for multi-database tenancy)
    /// </summary>
    [MaxLength(100)]
    public string? DatabaseName { get; set; }
    
    /// <summary>
    /// Users associated with this company
    /// </summary>
    public virtual ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    
    /// <summary>
    /// Branches of this company
    /// </summary>
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    
    /// <summary>
    /// Invitations for this company
    /// </summary>
    public virtual ICollection<UserInvitation> Invitations { get; set; } = new List<UserInvitation>();
}
