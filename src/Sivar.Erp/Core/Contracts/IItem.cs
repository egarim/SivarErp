using System;

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Interface for items
    /// </summary>
    public interface IItem
    {
        string Code { get; set; }
        string Description { get; set; }
        string? Category { get; set; }
        bool IsActive { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }
}