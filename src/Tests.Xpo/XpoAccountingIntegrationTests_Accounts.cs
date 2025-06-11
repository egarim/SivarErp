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
        #region Chart of Accounts Setup        /// <summary>
        /// Creates a basic chart of accounts for a retail business
        /// </summary>
        private async Task SetupChartOfAccounts()
        {
            // Clear accounts dictionary in case this method is called multiple times
            _accounts.Clear();

            // Assets (1xxxx) - Create hierarchical structure
            await CreateAccount("Assets", "10000", AccountType.Asset, "All company assets");
            await CreateAccount("Current Assets", "11000", AccountType.Asset, "Assets expected to be converted to cash within one year", "10000");
            await CreateAccount("Cash", "10100", AccountType.Asset, "Cash on hand and in banks", "11000");
            await CreateAccount("Petty Cash", "10110", AccountType.Asset, "Small amounts of cash for minor expenses", "10100");
            await CreateAccount("Accounts Receivable", "11100", AccountType.Asset, "Amounts owed by customers", "11000");
            await CreateAccount("Inventory", "12000", AccountType.Asset, "Merchandise held for sale", "11000");

            await CreateAccount("Fixed Assets", "15000", AccountType.Asset, "Long-term tangible assets", "10000");
            await CreateAccount("Store Equipment", "15100", AccountType.Asset, "Equipment used in store operations", "15000");
            await CreateAccount("Accumulated Depreciation - Store Equipment", "15200", AccountType.Asset, "Accumulated depreciation for store equipment", "15000");

            // Liabilities (2xxxx) - Create hierarchical structure
            await CreateAccount("Liabilities", "20000", AccountType.Liability, "All company liabilities");
            await CreateAccount("Current Liabilities", "21000", AccountType.Liability, "Debts due within one year", "20000");
            await CreateAccount("Accounts Payable", "21100", AccountType.Liability, "Amounts owed to suppliers", "21000");
            await CreateAccount("Sales Tax Payable", "21200", AccountType.Liability, "Sales tax collected but not yet remitted", "21000");
            await CreateAccount("Payroll Liabilities", "22000", AccountType.Liability, "Employee withholdings and employer payroll taxes", "20000");

            // Equity (3xxxx)
            await CreateAccount("Equity", "30000", AccountType.Equity, "Owner's equity in the business");
            await CreateAccount("Owner's Capital", "30100", AccountType.Equity, "Owner's investment in the business", "30000");
            await CreateAccount("Retained Earnings", "31000", AccountType.Equity, "Accumulated earnings of the business", "30000");

            // Revenue (4xxxx)
            await CreateAccount("Revenue", "40000", AccountType.Revenue, "All company revenue");
            await CreateAccount("Sales Revenue", "40100", AccountType.Revenue, "Revenue from sales of merchandise", "40000");
            await CreateAccount("Service Revenue", "41000", AccountType.Revenue, "Revenue from services provided", "40000");

            // Expenses (6xxxx) - Create hierarchical structure
            await CreateAccount("Expenses", "60000", AccountType.Expense, "All company expenses");
            await CreateAccount("Cost of Goods Sold", "60100", AccountType.Expense, "Cost of merchandise sold", "60000");
            await CreateAccount("Operating Expenses", "61000", AccountType.Expense, "Day-to-day operating expenses", "60000");
            await CreateAccount("Wages Expense", "61100", AccountType.Expense, "Wages paid to employees", "61000");
            await CreateAccount("Rent Expense", "62000", AccountType.Expense, "Rent for store premises", "61000");
            await CreateAccount("Utilities Expense", "63000", AccountType.Expense, "Electricity, water, etc.", "61000");
            await CreateAccount("Advertising Expense", "64000", AccountType.Expense, "Marketing and advertising costs", "61000");

            // Verify all accounts are valid
            foreach (var account in _accounts.Values)
            {
                bool isValid = account.Validate();
                Assert.That(isValid, Is.True, $"Account {account.AccountName} validation failed");
            }

            // Verify expected number of accounts (now with hierarchical structure)
            Assert.That(_accounts.Count, Is.EqualTo(25), "Expected 25 accounts in hierarchical chart of accounts");
        }/// <summary>
         /// Helper method to create an account and store it in the accounts dictionary
         /// </summary>
        private async Task CreateAccount(string name, string code, AccountType type, string description, string? parentCode = null)
        {
            var account = new Sivar.Erp.Xpo.ChartOfAccounts.XpoAccount(_unitOfWork)
            {
                Id = Guid.NewGuid(),
                AccountName = name ?? throw new ArgumentNullException(nameof(name)),
                OfficialCode = code ?? throw new ArgumentNullException(nameof(code)),
                AccountType = type,
                ParentOfficialCode = parentCode,
                ParentAccountCode = parentCode, // Keep both for compatibility
                IsArchived = false
            };

            // Set audit information
            _auditService.SetCreationAudit(account, TEST_USER);            // Store account (ensuring no duplicates)
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