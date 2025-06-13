using System;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Represents a rule for generating a transaction entry
    /// </summary>
    public class AccountingTransactionEntry
    {
        /// <summary>
        /// Account key to use from the account mappings
        /// </summary>
        public string AccountKey { get; set; }
        
        /// <summary>
        /// Type of entry (debit or credit)
        /// </summary>
        public EntryType EntryType { get; set; }
        
        /// <summary>
        /// A function to calculate the amount based on the document
        /// </summary>
        public Func<DocumentDto, decimal> AmountCalculator { get; set; }
        
        /// <summary>
        /// Optional description for the entry
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional account name override
        /// </summary>
        public string AccountNameOverride { get; set; }
        
        /// <summary>
        /// Creates a new transaction entry with the specified account key and entry type
        /// </summary>
        /// <param name="accountKey">The key used to look up the account ID from mappings</param>
        /// <param name="entryType">The entry type (debit or credit)</param>
        /// <param name="amountCalculator">Function that calculates the amount based on document data</param>
        public AccountingTransactionEntry(
            string accountKey, 
            EntryType entryType, 
            Func<DocumentDto, decimal> amountCalculator)
        {
            AccountKey = accountKey ?? throw new ArgumentNullException(nameof(accountKey));
            EntryType = entryType;
            AmountCalculator = amountCalculator ?? throw new ArgumentNullException(nameof(amountCalculator));
        }

        /// <summary>
        /// Creates a debit entry
        /// </summary>
        /// <param name="accountKey">The account key</param>
        /// <param name="amountCalculator">The amount calculator</param>
        /// <returns>An accounting transaction entry</returns>
        public static AccountingTransactionEntry Debit(
            string accountKey, 
            Func<DocumentDto, decimal> amountCalculator)
        {
            return new AccountingTransactionEntry(accountKey, EntryType.Debit, amountCalculator);
        }

        /// <summary>
        /// Creates a credit entry
        /// </summary>
        /// <param name="accountKey">The account key</param>
        /// <param name="amountCalculator">The amount calculator</param>
        /// <returns>An accounting transaction entry</returns>
        public static AccountingTransactionEntry Credit(
            string accountKey, 
            Func<DocumentDto, decimal> amountCalculator)
        {
            return new AccountingTransactionEntry(accountKey, EntryType.Credit, amountCalculator);
        }

        /// <summary>
        /// Sets the description for this entry
        /// </summary>
        /// <param name="description">The description</param>
        /// <returns>This entry for fluent chaining</returns>
        public AccountingTransactionEntry WithDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        /// Sets the account name override for this entry
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <returns>This entry for fluent chaining</returns>
        public AccountingTransactionEntry WithAccountName(string accountName)
        {
            AccountNameOverride = accountName;
            return this;
        }
    }
}