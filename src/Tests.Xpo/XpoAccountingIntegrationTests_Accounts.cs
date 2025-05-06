using DevExpress.Xpo;
using NUnit.Framework;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.Documents;
using System;
using System.Threading.Tasks;

namespace Sivar.Erp.Tests.Integration
{
    public partial class XpoAccountingIntegrationTests
    {
        #region Chart of Accounts Setup

        /// <summary>
        /// Creates a basic chart of accounts for a retail business
        /// </summary>
        private async Task SetupChartOfAccounts()
        {
            // Clear accounts dictionary in case this method is called multiple times
            _accounts.Clear();

            // Assets (1xxxx)
            await CreateAccount("Cash", "10100", AccountType.Asset, "Cash on hand and in banks");
            await CreateAccount("Accounts Receivable", "11000", AccountType.Asset, "Amounts owed by customers");
            await CreateAccount("Inventory", "12000", AccountType.Asset, "Merchandise held for sale");
            await CreateAccount("Store Equipment", "15000", AccountType.Asset, "Equipment used in store operations");
            await CreateAccount("Accumulated Depreciation - Store Equipment", "15100", AccountType.Asset, "Accumulated depreciation for store equipment");

            // Liabilities (2xxxx)
            await CreateAccount("Accounts Payable", "20000", AccountType.Liability, "Amounts owed to suppliers");
            await CreateAccount("Sales Tax Payable", "21000", AccountType.Liability, "Sales tax collected but not yet remitted");
            await CreateAccount("Payroll Liabilities", "22000", AccountType.Liability, "Employee withholdings and employer payroll taxes");

            // Equity (3xxxx)
            await CreateAccount("Owner's Capital", "30000", AccountType.Equity, "Owner's investment in the business");
            await CreateAccount("Retained Earnings", "31000", AccountType.Equity, "Accumulated earnings of the business");

            // Revenue (4xxxx)
            await CreateAccount("Sales Revenue", "40000", AccountType.Revenue, "Revenue from sales of merchandise");
            await CreateAccount("Service Revenue", "41000", AccountType.Revenue, "Revenue from services provided");

            // Expenses (6xxxx)
            await CreateAccount("Cost of Goods Sold", "60000", AccountType.Expense, "Cost of merchandise sold");
            await CreateAccount("Wages Expense", "61000", AccountType.Expense, "Wages paid to employees");
            await CreateAccount("Rent Expense", "62000", AccountType.Expense, "Rent for store premises");
            await CreateAccount("Utilities Expense", "63000", AccountType.Expense, "Electricity, water, etc.");
            await CreateAccount("Advertising Expense", "64000", AccountType.Expense, "Marketing and advertising costs");

            // Verify all accounts are valid
            foreach (var account in _accounts.Values)
            {
                bool isValid = account.Validate();
                Assert.That(isValid, Is.True, $"Account {account.AccountName} validation failed");
            }

            // Verify expected number of accounts
            Assert.That(_accounts.Count, Is.EqualTo(17), "Expected 17 accounts in chart of accounts");
        }

        /// <summary>
        /// Helper method to create an account and store it in the accounts dictionary
        /// </summary>
        private async Task CreateAccount(string name, string code, AccountType type, string description)
        {
            var account = new Sivar.Erp.Xpo.ChartOfAccounts.XpoAccount(_unitOfWork)
            {
                Id = Guid.NewGuid(),
                AccountName = name,
                OfficialCode = code,
                AccountType = type,
                IsArchived = false
            };

            // Set audit information
            _auditService.SetCreationAudit(account, TEST_USER);

            // Store account (ensuring no duplicates)
            if (!_accounts.ContainsKey(name))
            {
                _accounts[name] = account;
            }

            // Save changes to the database
            await _unitOfWork.CommitChangesAsync();
        }

        /// <summary>
        /// Calculates the balance of an account as of a specific date
        /// </summary>
        private async Task<decimal> GetAccountBalance(Guid accountId, DateOnly asOfDate)
        {
            // Flush any pending changes to ensure the database is up to date
            await _unitOfWork.CommitChangesAsync();

            // First, get all transactions that have dates on or before the specified date
            var validTransactions = _unitOfWork.Query<Sivar.Erp.Xpo.Documents.XpoTransaction>()
                .Where(t => t.TransactionDate <= asOfDate)
                .Select(t => t.Id)
                .ToList();

            // Now get all ledger entries for the account that reference those transactions
            var ledgerEntries = _unitOfWork.Query<Sivar.Erp.Xpo.Documents.XpoLedgerEntry>()
                .Where(e => e.AccountId == accountId && validTransactions.Contains(e.TransactionId))
                .ToList();

            // Calculate the balance
            decimal debitSum = ledgerEntries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
            decimal creditSum = ledgerEntries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);

            // Return the net balance (positive for debit, negative for credit)
            return debitSum - creditSum;
        }

        #endregion
    }
}