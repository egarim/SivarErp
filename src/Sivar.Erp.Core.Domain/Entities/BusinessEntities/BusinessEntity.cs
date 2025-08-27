using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities.Identity;

namespace Sivar.Erp.Core.Domain.Entities.BusinessEntities;

/// <summary>
/// Represents a business entity (customer, supplier, vendor, etc.) with multi-tenant support
/// </summary>
public class BusinessEntity : BaseEntity, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// The company this business entity belongs to
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Optional branch this business entity is associated with
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Business entity code/identifier
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Business entity name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of business entity (Customer, Supplier, Vendor, etc.)
    /// </summary>
    [Required]
    public BusinessEntityType EntityType { get; set; }

    /// <summary>
    /// Tax identification number
    /// </summary>
    [MaxLength(50)]
    public string? TaxId { get; set; }

    /// <summary>
    /// Physical address
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
    /// ZIP or postal code
    /// </summary>
    [MaxLength(20)]
    public string? ZipCode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    [MaxLength(100)]
    public string? Country { get; set; }

    /// <summary>
    /// Primary phone number
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Mobile phone number
    /// </summary>
    [MaxLength(20)]
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Primary email address
    /// </summary>
    [MaxLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Website URL
    /// </summary>
    [MaxLength(500)]
    [Url]
    public string? Website { get; set; }

    /// <summary>
    /// Credit limit for customers
    /// </summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Payment terms in days
    /// </summary>
    public int PaymentTermsDays { get; set; } = 30;

    /// <summary>
    /// Whether this business entity is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// User who created this record
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// When this record was created
    /// </summary>
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who last updated this record
    /// </summary>
    public Guid? UpdatedByUserId { get; set; }

    /// <summary>
    /// When this record was last updated
    /// </summary>
    public new DateTime? UpdatedAt { get; set; }

    // IAuditableEntity implementation
    /// <summary>
    /// Additional audit information in JSON format
    /// </summary>
    public string? AuditData { get; set; }
    
    /// <summary>
    /// The source of the change (API, UI, Import, etc.)
    /// </summary>
    public string? ChangeSource { get; set; }
    
    /// <summary>
    /// IP address of the change origin
    /// </summary>
    public string? ChangeIpAddress { get; set; }
    
    /// <summary>
    /// User agent information
    /// </summary>
    public string? ChangeUserAgent { get; set; }

    // Navigation properties
    /// <summary>
    /// The company this business entity belongs to
    /// </summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>
    /// The branch this business entity is associated with (if any)
    /// </summary>
    public virtual Branch? Branch { get; set; }

    /// <summary>
    /// User who created this record
    /// </summary>
    public virtual User CreatedByUser { get; set; } = null!;

    /// <summary>
    /// User who last updated this record
    /// </summary>
    public virtual User? UpdatedByUser { get; set; }

    /// <summary>
    /// Get the full address as a single string
    /// </summary>
    public string GetFullAddress()
    {
        var parts = new[] { Address, City, State, ZipCode, Country }
            .Where(x => !string.IsNullOrWhiteSpace(x));
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Get the display name (Code - Name)
    /// </summary>
    public string GetDisplayName()
    {
        return $"{Code} - {Name}";
    }

    /// <summary>
    /// Check if this business entity has complete contact information
    /// </summary>
    public bool HasCompleteContactInfo()
    {
        return !string.IsNullOrWhiteSpace(Email) || 
               !string.IsNullOrWhiteSpace(PhoneNumber) ||
               !string.IsNullOrWhiteSpace(MobileNumber);
    }

    /// <summary>
    /// Check if this business entity has a valid address
    /// </summary>
    public bool HasValidAddress()
    {
        return !string.IsNullOrWhiteSpace(Address) &&
               !string.IsNullOrWhiteSpace(City) &&
               !string.IsNullOrWhiteSpace(Country);
    }

    /// <summary>
    /// Get a summary of the business entity for display purposes
    /// </summary>
    public string GetSummary()
    {
        var summary = $"{GetDisplayName()}\n";
        
        if (HasValidAddress())
        {
            summary += $"Address: {GetFullAddress()}\n";
        }
        
        if (!string.IsNullOrWhiteSpace(Email))
        {
            summary += $"Email: {Email}\n";
        }
        
        if (!string.IsNullOrWhiteSpace(PhoneNumber))
        {
            summary += $"Phone: {PhoneNumber}\n";
        }
        
        return summary.TrimEnd('\n');
    }
}
