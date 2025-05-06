using DevExpress.Xpo;
using NUnit.Framework.Interfaces;
using Sivar.Erp.Documents;
using Sivar.Erp.Xpo.ChartOfAccounts;
using Sivar.Erp.Xpo.Documents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.Tests.Integration
{
    public partial class XpoAccountingIntegrationTests
    {
        #region Transaction Methods

        /// <summary>
        /// Creates a purchase transaction for inventory
        /// </summary>
        private async Task CreatePurchaseTransaction()
        {
            // Create the document
            var document = new XpoDocument(_unitOfWork)
            {
                Id = Guid.NewGuid(),
                DocumentDate = _testDate,
                DocumentNo = "PO-001",
                Description = "Purchase of inventory from Electronics Wholesale",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "30-day payment terms",
                InternalComments = "First order from this supplier"
            };

            // Save the document
            await _unitOfWork.CommitChangesAsync();
            _documents["InventoryPurchase"] = document;

            // Create transaction
            var transaction = new XpoTransaction(_unitOfWork)
            {
                Document = document,
                TransactionDate = document.DocumentDate,
                Description = "Purchase of 10 smartphones at $300 each"
            };

            // Save the transaction
            await _unitOfWork.CommitChangesAsync();
            _transactions["InventoryPurchase"] = transaction;

            // Create ledger entries
            await CreateLedgerEntry(
                transaction,
                _accounts["Inventory"],
                EntryType.Debit,
                3000.00m);

            await CreateLedgerEntry(
                transaction,
                _accounts["Accounts Payable"],
                EntryType.Credit,
                3000.00m);
        }

        /// <summary>
        /// Creates a sales transaction
        /// </summary>
        private async Task CreateSaleTransaction()
        {
            // Create the document
            var document = new XpoDocument(_unitOfWork)
            {
                Id = Guid.NewGuid(),
                DocumentDate = _testDate,
                DocumentNo = "INV-001",
                Description = "Sale to customer Jane Smith",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "Paid immediately",
                InternalComments = "First-time customer"
            };

            // Save the document
            await _unitOfWork.CommitChangesAsync();
            _documents["Sale"] = document;

            // Create transaction
            var transaction = new XpoTransaction(_unitOfWork)
            {
                Document = document,
                TransactionDate = document.DocumentDate,
                Description = "Sale of 1 smartphone at $500"
            };

            // Save the transaction
            await _unitOfWork.CommitChangesAsync();
            _transactions["Sale"] = transaction;

            // Create ledger entries
            await CreateLedgerEntry(
                transaction,
                _accounts["Cash"],
                EntryType.Debit,
                500.00m);

            await CreateLedgerEntry(
                transaction,
                _accounts["Sales Revenue"],
                EntryType.Credit,
                500.00m);

            await CreateLedgerEntry(
                transaction,
                _accounts["Cost of Goods Sold"],
                EntryType.Debit,
                300.00m);

            await CreateLedgerEntry(
                transaction,
                _accounts["Inventory"],
                EntryType.Credit,
                300.00m);
        }

        /// <summary>
        /// Creates an expense transaction
        /// </summary>
        private async Task CreateExpenseTransaction()
        {
            // Create the document
            var document = new XpoDocument(_unitOfWork)
            {
                Id = Guid.NewGuid(),
                DocumentDate = _testDate,
                DocumentNo = "UTIL-001",
                Description = "May electricity bill",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "Paid by check #1001",
                InternalComments = "Higher than usual due to air conditioning"
            };

            // Save the document
            await _unitOfWork.CommitChangesAsync();
            _documents["UtilityExpense"] = document;

            // Create transaction
            var transaction = new XpoTransaction(_unitOfWork)
            {
                Document = document,
                TransactionDate = document.DocumentDate,
                Description = "Payment for May electricity bill"
            };

            // Save the transaction
            await _unitOfWork.CommitChangesAsync();
            _transactions["UtilityExpense"] = transaction;

            // Create ledger entries
            await CreateLedgerEntry(
                transaction,
                _accounts["Utilities Expense"],
                EntryType.Debit,
                250.00m);

            await CreateLedgerEntry(
                transaction,
                _accounts["Cash"],
                EntryType.Credit,
                250.00m);
        }

        /// <summary>
        /// Creates a payment to supplier transaction
        /// </summary>
        private async Task CreatePaymentTransaction()
        {
            // Create the document
            var document = new XpoDocument(_unitOfWork)
            {
                Id = Guid.NewGuid(),
                DocumentDate = _testDate.AddDays(15), // 15 days after purchase
                DocumentNo = "PAY-001",
                Description = "Payment to Electronics Wholesale",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "Paid by check #1002",
                InternalComments = "Payment for PO-001"
            };

            // Save the document
            await _unitOfWork.CommitChangesAsync();
            _documents["SupplierPayment"] = document;

            // Create transaction
            var transaction = new XpoTransaction(_unitOfWork)
            {
                Document = document,
                TransactionDate = document.DocumentDate,
                Description = "Payment for inventory purchase"
            };

            // Save the transaction
            await _unitOfWork.CommitChangesAsync();
            _transactions["SupplierPayment"] = transaction;

            // Create ledger entries
            await CreateLedgerEntry(
                transaction,
                _accounts["Accounts Payable"],
                EntryType.Debit,
                3000.00m);

            await CreateLedgerEntry(
                transaction,
                _accounts["Cash"],
                EntryType.Credit,
                3000.00m);
        }

        /// <summary>
        /// Helper method to create a ledger entry
        /// </summary>
        private async Task CreateLedgerEntry(
            XpoTransaction transaction,
            XpoAccount account,
            EntryType entryType,
            decimal amount)
        {
            var entry = new XpoLedgerEntry(_unitOfWork)
            {
                Id = Guid.NewGuid(),
                Transaction = transaction,
                Account = account,
                EntryType = entryType,
                Amount = amount
            };

            await _unitOfWork.CommitChangesAsync();
            return;
        }

        /// <summary>
        /// Verifies account balances after all transactions
        /// </summary>
        private async Task VerifyAccountBalances()
        {
            // Create a new query to ensure we're getting the latest data from the database
            // Force the UnitOfWork to refresh its cache
            _unitOfWork.Reload(_accounts["Cash"]);
            _unitOfWork.Reload(_accounts["Inventory"]);
            _unitOfWork.Reload(_accounts["Accounts Payable"]);
            _unitOfWork.Reload(_accounts["Sales Revenue"]);
            _unitOfWork.Reload(_accounts["Cost of Goods Sold"]);
            _unitOfWork.Reload(_accounts["Utilities Expense"]);

            // Verify Cash account balance (-2750 = 500 - 250 - 3000)
            decimal cashBalance = await GetAccountBalance(_accounts["Cash"].Id, _testDate.AddDays(15));
            NUnit.Framework.Assert.That(cashBalance, NUnit.Framework.Is.EqualTo(-2750m), "Cash balance is incorrect");

            // Verify Inventory balance (2700 = 3000 - 300)
            decimal inventoryBalance = await GetAccountBalance(_accounts["Inventory"].Id, _testDate.AddDays(15));
            NUnit.Framework.Assert.That(inventoryBalance, NUnit.Framework.Is.EqualTo(2700m), "Inventory balance is incorrect");

            // Verify Accounts Payable balance (0 = -3000 + 3000)
            decimal accountsPayableBalance = await GetAccountBalance(_accounts["Accounts Payable"].Id, _testDate.AddDays(15));
            NUnit.Framework.Assert.That(accountsPayableBalance, NUnit.Framework.Is.EqualTo(0m), "Accounts Payable balance is incorrect");

            // Verify Sales Revenue balance (-500)
            decimal salesRevenueBalance = await GetAccountBalance(_accounts["Sales Revenue"].Id, _testDate.AddDays(15));
            NUnit.Framework.Assert.That(salesRevenueBalance, NUnit.Framework.Is.EqualTo(-500m), "Sales Revenue balance is incorrect");

            // Verify Cost of Goods Sold (300)
            decimal cogsBalance = await GetAccountBalance(_accounts["Cost of Goods Sold"].Id, _testDate.AddDays(15));
            NUnit.Framework.Assert.That(cogsBalance, NUnit.Framework.Is.EqualTo(300m), "Cost of Goods Sold balance is incorrect");

            // Verify Utilities Expense (250)
            decimal utilitiesExpenseBalance = await GetAccountBalance(_accounts["Utilities Expense"].Id, _testDate.AddDays(15));
            NUnit.Framework.Assert.That(utilitiesExpenseBalance, NUnit.Framework.Is.EqualTo(250m), "Utilities Expense balance is incorrect");

            // Also verify the accounting equation as a sanity check
            decimal assets = cashBalance + inventoryBalance;
            decimal liabilities = accountsPayableBalance;
            decimal revenues = -salesRevenueBalance; // Negate because Credit balances are negative
            decimal expenses = cogsBalance + utilitiesExpenseBalance;

            decimal leftSide = assets;
            decimal rightSide = liabilities + (revenues - expenses);

            NUnit.Framework.Assert.That(Math.Abs(leftSide - rightSide), NUnit.Framework.Is.LessThan(0.01m),
                "Accounting equation should be balanced");
        }

        #endregion
    }
}