using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Entities.BusinessEntities;

namespace Sivar.Erp.Core.Shared.DTOs.BusinessEntities;

/// <summary>
/// Data transfer object for Business Entity
/// </summary>
public class BusinessEntityDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Company ID (tenant isolation)
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Branch ID (optional)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Business entity code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Business entity name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of business entity
    /// </summary>
    public BusinessEntityType EntityType { get; set; }

    /// <summary>
    /// Tax identification number
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// Physical address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State or province
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// ZIP or postal code
    /// </summary>
    public string? ZipCode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Primary phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Mobile phone number
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Primary email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Website URL
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Credit limit for customers
    /// </summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Payment terms in days
    /// </summary>
    public int PaymentTermsDays { get; set; }

    /// <summary>
    /// Whether this business entity is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// User who created this record
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// When this record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User who last updated this record
    /// </summary>
    public Guid? UpdatedByUserId { get; set; }

    /// <summary>
    /// When this record was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Company name (for display purposes)
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Branch name (for display purposes)
    /// </summary>
    public string? BranchName { get; set; }

    /// <summary>
    /// Created by user name (for display purposes)
    /// </summary>
    public string? CreatedByUserName { get; set; }

    /// <summary>
    /// Updated by user name (for display purposes)
    /// </summary>
    public string? UpdatedByUserName { get; set; }

    /// <summary>
    /// Display name (Code - Name)
    /// </summary>
    public string DisplayName => $"{Code} - {Name}";

    /// <summary>
    /// Full address as a single string
    /// </summary>
    public string FullAddress => GetFullAddress();

    /// <summary>
    /// Entity type display name
    /// </summary>
    public string EntityTypeDisplayName => EntityType.ToString();

    private string GetFullAddress()
    {
        var parts = new[] { Address, City, State, ZipCode, Country }
            .Where(x => !string.IsNullOrWhiteSpace(x));
        return string.Join(", ", parts);
    }
}

/// <summary>
/// DTO for creating a new business entity
/// </summary>
public class CreateBusinessEntityDto
{
    /// <summary>
    /// Branch ID (optional)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Business entity code
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Business entity name
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of business entity
    /// </summary>
    [Required]
    public BusinessEntityType EntityType { get; set; }

    /// <summary>
    /// Tax identification number
    /// </summary>
    [StringLength(50)]
    public string? TaxId { get; set; }

    /// <summary>
    /// Physical address
    /// </summary>
    [StringLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [StringLength(100)]
    public string? City { get; set; }

    /// <summary>
    /// State or province
    /// </summary>
    [StringLength(100)]
    public string? State { get; set; }

    /// <summary>
    /// ZIP or postal code
    /// </summary>
    [StringLength(20)]
    public string? ZipCode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    [StringLength(100)]
    public string? Country { get; set; }

    /// <summary>
    /// Primary phone number
    /// </summary>
    [StringLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Mobile phone number
    /// </summary>
    [StringLength(20)]
    [Phone]
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Primary email address
    /// </summary>
    [StringLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Website URL
    /// </summary>
    [StringLength(500)]
    [Url]
    public string? Website { get; set; }

    /// <summary>
    /// Credit limit for customers
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Credit limit must be a positive value")]
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Payment terms in days
    /// </summary>
    [Range(1, 365, ErrorMessage = "Payment terms must be between 1 and 365 days")]
    public int PaymentTermsDays { get; set; } = 30;

    /// <summary>
    /// Whether this business entity is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing business entity
/// </summary>
public class UpdateBusinessEntityDto : CreateBusinessEntityDto
{
    // Inherits all properties from CreateBusinessEntityDto
    // This allows for partial updates and maintains consistency
}

/// <summary>
/// DTO for business entity statistics
/// </summary>
public class BusinessEntityStatsDto
{
    /// <summary>
    /// Total number of business entities
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Number of active business entities
    /// </summary>
    public int ActiveCount { get; set; }

    /// <summary>
    /// Number of inactive business entities
    /// </summary>
    public int InactiveCount { get; set; }

    /// <summary>
    /// Count by entity type
    /// </summary>
    public Dictionary<BusinessEntityType, int> CountByType { get; set; } = new();

    /// <summary>
    /// Number of customers with credit limits
    /// </summary>
    public int CustomersWithCreditLimit { get; set; }

    /// <summary>
    /// Total credit limit amount
    /// </summary>
    public decimal TotalCreditLimit { get; set; }

    /// <summary>
    /// Number of entities created this month
    /// </summary>
    public int CreatedThisMonth { get; set; }

    /// <summary>
    /// Number of entities created this year
    /// </summary>
    public int CreatedThisYear { get; set; }
}

/// <summary>
/// DTO for import results
/// </summary>
public class ImportResultDto
{
    /// <summary>
    /// Number of records processed
    /// </summary>
    public int ProcessedCount { get; set; }

    /// <summary>
    /// Number of records successfully imported
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of records that failed to import
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// List of error messages
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// List of warning messages
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Whether the import was successful
    /// </summary>
    public bool IsSuccess => ErrorCount == 0;
}

/// <summary>
/// DTO for validation results
/// </summary>
public class ValidationResultDto
{
    /// <summary>
    /// Whether validation passed
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// List of validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}
