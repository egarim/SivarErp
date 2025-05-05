namespace Sivar.Erp.ChartOfAccounts
{
    /// <summary>
    /// Interface for chart of accounts entries
    /// </summary>
    public interface IAccount : IEntity, IAuditable, IArchivable
    {
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
        string OfficialCode { get; set; }
    }
}