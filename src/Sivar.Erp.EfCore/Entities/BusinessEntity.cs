using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Core.Interfaces;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Business Entities (customers, suppliers, etc.)
/// </summary>
[Table("BusinessEntities")]
public class BusinessEntity : BaseEntity, IBusinessEntity
{
    /// <summary>
    /// Unique code for the business entity
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Name of the business entity
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Address of the business entity
    /// </summary>
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// City where the business entity is located
    /// </summary>
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State/Province where the business entity is located
    /// </summary>
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// ZIP/Postal code of the business entity
    /// </summary>
    [MaxLength(20)]
    public string ZipCode { get; set; } = string.Empty;

    /// <summary>
    /// Country where the business entity is located
    /// </summary>
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the business entity
    /// </summary>
    [MaxLength(50)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the business entity
    /// </summary>
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
}
