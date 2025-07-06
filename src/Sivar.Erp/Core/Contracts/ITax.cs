using System;

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Interface for taxes
    /// </summary>
    public interface ITax
    {
        string Code { get; set; }
        string Name { get; set; }
        decimal Rate { get; set; }
        bool IsActive { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }

    /// <summary>
    /// Interface for tax groups
    /// </summary>
    public interface ITaxGroup
    {
        string Code { get; set; }
        string Name { get; set; }
        string? Description { get; set; }
        bool IsActive { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }

    /// <summary>
    /// Interface for tax rules
    /// </summary>
    public interface ITaxRule
    {
        string Code { get; set; }
        string Description { get; set; }
        bool IsActive { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }

    /// <summary>
    /// Interface for group membership
    /// </summary>
    public interface IGroupMembership
    {
        string MemberCode { get; set; }
        string GroupCode { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }
}