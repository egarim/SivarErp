namespace Sivar.Erp.Core.Domain.Entities;

/// <summary>
/// Interface for entities that belong to a specific tenant (company)
/// </summary>
public interface ITenantEntity : IEntity
{
    /// <summary>
    /// The company/tenant this entity belongs to
    /// </summary>
    Guid CompanyId { get; set; }
}
