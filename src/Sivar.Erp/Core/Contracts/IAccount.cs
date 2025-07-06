using System;

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Interface for accounts in the chart of accounts
    /// </summary>
    public interface IAccount
    {
        string OfficialCode { get; set; }
        string AccountName { get; set; }
        string? ParentAccountCode { get; set; }
        bool IsActive { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }
}