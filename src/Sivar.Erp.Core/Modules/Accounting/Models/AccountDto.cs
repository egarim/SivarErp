using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Accounting.Models
{
    /// <summary>
    /// Implementation of the IAccount interface
    /// </summary>
    public class AccountDto : Entity, IAccount
    {
        /// <summary>
        /// Gets or sets the official account code
        /// </summary>
        public string OfficialCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the account name
        /// </summary>
        public string AccountName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the account description
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Gets or sets the account type
        /// </summary>
        public AccountType AccountType { get; set; }
        
        /// <summary>
        /// Gets or sets the parent account code
        /// </summary>
        public string? ParentAccountCode { get; set; }
        
        /// <summary>
        /// Gets or sets whether the account is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}