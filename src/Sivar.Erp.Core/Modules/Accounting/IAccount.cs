using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Represents an account in the chart of accounts
    /// </summary>
    [Description("Represents an account in the chart of accounts")]
    public interface IAccount
    {
        /// <summary>
        /// Gets or sets the official account code
        /// </summary>
        string OfficialCode { get; set; }
        
        /// <summary>
        /// Gets or sets the account name
        /// </summary>
        string AccountName { get; set; }
        
        /// <summary>
        /// Gets or sets the account description
        /// </summary>
        string? Description { get; set; }
        
        /// <summary>
        /// Gets or sets the account type
        /// </summary>
        AccountType AccountType { get; set; }
        
        /// <summary>
        /// Gets or sets the parent account code
        /// </summary>
        string? ParentAccountCode { get; set; }
        
        /// <summary>
        /// Gets or sets whether the account is active
        /// </summary>
        bool IsActive { get; set; }
    }
    
    /// <summary>
    /// Types of accounts in the chart of accounts
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// Asset account
        /// </summary>
        Asset,
        
        /// <summary>
        /// Liability account
        /// </summary>
        Liability,
        
        /// <summary>
        /// Equity account
        /// </summary>
        Equity,
        
        /// <summary>
        /// Revenue account
        /// </summary>
        Revenue,
        
        /// <summary>
        /// Expense account
        /// </summary>
        Expense
    }
}