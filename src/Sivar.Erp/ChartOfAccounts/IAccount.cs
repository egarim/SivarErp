namespace Sivar.Erp.ChartOfAccounts
{
    /// <summary>
    /// Interface for chart of accounts entries
    /// </summary>
    public interface IAccount : IEntity, IArchivable
    {
        /// <summary>
        /// UTC timestamp when the entity was created
        /// </summary>
        DateTime InsertedAt { get; set; }

        /// <summary>
        /// User who created the entity
        /// </summary>
        string InsertedBy { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was last updated
        /// </summary>
        DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who last updated the entity
        /// </summary>
        string UpdatedBy { get; set; }
        
        /// <summary>
        /// Optional reference to balance sheet or income statement line
        /// </summary>
        Guid? BalanceAndIncomeLineId { get; set; }

        /// <summary>
        /// Name of the account
        /// </summary>
        string AccountName { get; set; }

        /// <summary>
        /// Type of account (asset, liability, etc.)
        /// </summary>
        AccountType AccountType { get; set; }

        /// <summary>
        /// Official code/identifier for the account (e.g., for SAF-T reporting)
        /// </summary>
        [BusinessKey()]
        string OfficialCode { get; set; }

        /// <summary>
        /// Official code/identifier for the parent account 
        /// </summary>
        string? ParentOfficialCode { get; set; }
    }
}