using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Taxes.TaxGroup;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Group Memberships
/// </summary>
[Table("GroupMemberships")]
public class GroupMembership : BaseEntity
{
    /// <summary>
    /// The group code that defines the tax behavior
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string GroupCode { get; set; } = string.Empty;

    /// <summary>
    /// The entity ID that is a member of the group (either business entity or item depending on GroupType)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// The type of group this membership relates to
    /// </summary>
    public virtual GroupType GroupType { get; set; }
}
