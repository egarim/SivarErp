using DevExpress.Xpo;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.Documents;
using Sivar.Erp.Xpo.Core;
using Sivar.Erp.Xpo.Documents;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Xpo.ChartOfAccounts
{
    /// <summary>
    /// XPO implementation of the account balance calculator
    /// </summary>
    public class XpoAccountBalanceCalculator
    {
        /// <summary>
        /// Calculates the balance of an account as of a specific date
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="asOfDate">Date to calculate balance for</param>
        /// <returns>Account balance (positive for debit balance, negative for credit balance)</returns>
        public async Task<decimal> CalculateAccountBalanceAsync(Guid accountId, DateOnly asOfDate)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            // Retrieve account to check its type
            var account = await uow.GetObjectByKeyAsync<XpoAccount>(accountId);

            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found");
            }

            // Query all ledger entries for this account up to the specified date
            var ledgerEntries = await Task.Run(() =>
                uow.Query<XpoLedgerEntry>()
                    .Where(e => e.AccountId == accountId &&
                           e.Transaction.TransactionDate <= asOfDate)
                    .ToList());

            // Calculate balance
            decimal debitSum = ledgerEntries
                .Where(e => e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            decimal creditSum = ledgerEntries
                .Where(e => e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            // Calculate the net balance
            // Positive means debit balance, negative means credit balance
            decimal balance = debitSum - creditSum;

            return balance;
        }

        /// <summary>
        /// Calculates the turnover (sum of all transactions) for an account within a date range
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>Tuple containing debit turnover and credit turnover</returns>
        public async Task<(decimal DebitTurnover, decimal CreditTurnover)> CalculateAccountTurnoverAsync(
            Guid accountId, DateOnly startDate, DateOnly endDate)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            // Query all ledger entries for this account within the specified date range
            var ledgerEntries = await Task.Run(() =>
                uow.Query<XpoLedgerEntry>()
                    .Where(e => e.AccountId == accountId &&
                           e.Transaction.TransactionDate >= startDate &&
                           e.Transaction.TransactionDate <= endDate)
                    .ToList());

            // Calculate turnover
            decimal debitTurnover = ledgerEntries
                .Where(e => e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            decimal creditTurnover = ledgerEntries
                .Where(e => e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            return (debitTurnover, creditTurnover);
        }

        /// <summary>
        /// Calculates the account opening balance as of a specific date
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="asOfDate">Date to calculate opening balance for</param>
        /// <returns>Opening balance</returns>
        public async Task<decimal> CalculateOpeningBalanceAsync(Guid accountId, DateOnly asOfDate)
        {
            // Opening balance is the balance as of the day before
            return await CalculateAccountBalanceAsync(accountId, asOfDate.AddDays(-1));
        }

        /// <summary>
        /// Determines if an account has any transactions
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>True if account has transactions, false otherwise</returns>
        public async Task<bool> HasTransactionsAsync(Guid accountId)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            // Check if there are any ledger entries for this account
            return await Task.Run(() =>
                uow.Query<XpoLedgerEntry>()
                    .Any(e => e.AccountId == accountId));
        }

        /// <summary>
        /// Calculates a trial balance for all accounts as of a specific date
        /// </summary>
        /// <param name="asOfDate">Date to calculate trial balance for</param>
        /// <returns>Collection of account balances</returns>
        public async Task<IQueryable<AccountBalance>> CalculateTrialBalanceAsync(DateOnly asOfDate)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            var accounts = uow.Query<XpoAccount>().Where(a => !a.IsArchived);

            // This would normally be done with a specialized query directly to the database
            // For this example, we'll use LINQ to calculate the balances
            var trialBalance = from account in accounts
                               let entries = uow.Query<XpoLedgerEntry>()
                                   .Where(e => e.AccountId == account.Id &&
                                          e.Transaction.TransactionDate <= asOfDate)
                               let debitSum = entries.Where(e => e.EntryType == EntryType.Debit)
                                   .Sum(e => e.Amount)
                               let creditSum = entries.Where(e => e.EntryType == EntryType.Credit)
                                   .Sum(e => e.Amount)
                               select new AccountBalance
                               {
                                   AccountId = account.Id,
                                   AccountName = account.AccountName,
                                   AccountType = account.AccountType,
                                   OfficialCode = account.OfficialCode,
                                   DebitBalance = account.HasDebitBalance() ?
                                       Math.Max(debitSum - creditSum, 0) : 0,
                                   CreditBalance = !account.HasDebitBalance() ?
                                       Math.Max(creditSum - debitSum, 0) : 0
                               };

            return trialBalance;
        }
    }

    /// <summary>
    /// Contains account balance information for reporting
    /// </summary>
    public class AccountBalance
    {
        /// <summary>
        /// Account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Account name
        /// </summary>
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Account type
        /// </summary>
        public AccountType AccountType { get; set; }

        /// <summary>
        /// Official code
        /// </summary>
        public string OfficialCode { get; set; } = string.Empty;

        /// <summary>
        /// Debit balance (if account normally has debit balance)
        /// </summary>
        public decimal DebitBalance { get; set; }

        /// <summary>
        /// Credit balance (if account normally has credit balance)
        /// </summary>
        public decimal CreditBalance { get; set; }

        /// <summary>
        /// Gets the net balance of the account
        /// </summary>
        public decimal NetBalance
        {
            get
            {
                if (DebitBalance > 0)
                    return DebitBalance;
                if (CreditBalance > 0)
                    return -CreditBalance;
                return 0;
            }
        }
    }
}