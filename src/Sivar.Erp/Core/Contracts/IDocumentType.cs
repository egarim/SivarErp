using System;
using System.Collections.Generic;

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Interface for document types
    /// </summary>
    public interface IDocumentType
    {
        string Code { get; set; }
        string Name { get; set; }
        string? Description { get; set; }
        bool IsActive { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }

    /// <summary>
    /// Interface for document lines
    /// </summary>
    public interface IDocumentLine
    {
        int LineNumber { get; set; }
        IItem? Item { get; set; }
        decimal Quantity { get; set; }
        decimal UnitPrice { get; set; }
        decimal Amount { get; set; }
    }

    /// <summary>
    /// Interface for document totals
    /// </summary>
    public interface ITotal
    {
        string Concept { get; set; }
        decimal Total { get; set; }
    }
}