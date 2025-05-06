using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using DevExpress.Xpo;
using Sivar.Erp.Documents;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.Xpo.Core;
using Sivar.Erp.Xpo.Documents;
using Sivar.Erp.Xpo.ChartOfAccounts;
using Sivar.Erp.Xpo.Services;

namespace Sivar.Erp.Tests.Integration
{
    /// <summary>
    /// Integration tests for Documents and Chart of Accounts modules using XPO
    /// </summary>
    [TestFixture]
    public partial class XpoAccountingIntegrationTests
    {
        private IAuditService _auditService;
        private XpoDocumentService _documentService;
        private XpoTransactionService _transactionService;
        private XpoAccountService _accountService;
        private XpoAccountBalanceCalculator _accountBalanceCalculator;
        private UnitOfWork _unitOfWork;

        // Dictionary to store our accounts
        private Dictionary<string, XpoAccount> _accounts;

        // Dictionary to store our completed documents
        private Dictionary<string, XpoDocument> _documents;

        // Dictionary to store our completed transactions
        private Dictionary<string, XpoTransaction> _transactions;

        // Constants for testing
        private const string TEST_USER = "IntegrationTest";
        private readonly DateOnly _testDate = new DateOnly(2025, 5, 1);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Initialize the XPO data layer with an in-memory database for testing
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(
                "XpoProvider=InMemoryDataStore", DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
        }

        [SetUp]
        public async Task Setup()
        {
            // Create a new UnitOfWork for each test
            _unitOfWork = new UnitOfWork();

            // Initialize services
            _auditService = new XpoAuditService();
            _documentService = new XpoDocumentService(_auditService);
            _transactionService = new XpoTransactionService();
            _accountService = new XpoAccountService(_auditService, new XpoArchiveService());
            _accountBalanceCalculator = new XpoAccountBalanceCalculator();

            // Initialize storage
            _accounts = new Dictionary<string, XpoAccount>();
            _documents = new Dictionary<string, XpoDocument>();
            _transactions = new Dictionary<string, XpoTransaction>();

            // Create Chart of Accounts
            await SetupChartOfAccounts();
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose the UnitOfWork after each test
            _unitOfWork.Dispose();
        }

        [Test]
        public async Task CanRecordPurchaseOfInventory()
        {
            await CreatePurchaseTransaction();

            // Verify account balances
            decimal inventoryBalance = await GetAccountBalance(_accounts["Inventory"].Id, _testDate);
            decimal payablesBalance = await GetAccountBalance(_accounts["Accounts Payable"].Id, _testDate);

            Assert.That(inventoryBalance, Is.EqualTo(3000m), "Inventory balance is incorrect");
            Assert.That(payablesBalance, Is.EqualTo(-3000m), "Accounts Payable balance is incorrect");
        }

        [Test]
        public async Task CanRecordSaleToCustomer()
        {
            await CreateSaleTransaction();

            // Verify account balances
            decimal cashBalance = await GetAccountBalance(_accounts["Cash"].Id, _testDate);
            decimal inventoryBalance = await GetAccountBalance(_accounts["Inventory"].Id, _testDate);
            decimal revenueBalance = await GetAccountBalance(_accounts["Sales Revenue"].Id, _testDate);
            decimal cogsBalance = await GetAccountBalance(_accounts["Cost of Goods Sold"].Id, _testDate);

            Assert.That(cashBalance, Is.EqualTo(500m), "Cash balance is incorrect");
            Assert.That(inventoryBalance, Is.EqualTo(-300m), "Inventory balance is incorrect");
            Assert.That(revenueBalance, Is.EqualTo(-500m), "Sales Revenue balance is incorrect");
            Assert.That(cogsBalance, Is.EqualTo(300m), "Cost of Goods Sold balance is incorrect");
        }

        [Test]
        public async Task CanRecordBusinessExpense()
        {
            await CreateExpenseTransaction();

            // Verify account balances
            decimal cashBalance = await GetAccountBalance(_accounts["Cash"].Id, _testDate);
            decimal expenseBalance = await GetAccountBalance(_accounts["Utilities Expense"].Id, _testDate);

            Assert.That(cashBalance, Is.EqualTo(-250m), "Cash balance is incorrect");
            Assert.That(expenseBalance, Is.EqualTo(250m), "Utilities Expense balance is incorrect");
        }

        [Test]
        public async Task CanRecordPaymentToSupplier()
        {
            // First create a purchase to have something to pay for
            await CreatePurchaseTransaction();

            // Then create the payment
            await CreatePaymentTransaction();

            // Verify account balances
            decimal cashBalance = await GetAccountBalance(_accounts["Cash"].Id, _testDate.AddDays(15));
            decimal payablesBalance = await GetAccountBalance(_accounts["Accounts Payable"].Id, _testDate.AddDays(15));

            Assert.That(cashBalance, Is.EqualTo(-3000m), "Cash balance is incorrect");
            Assert.That(payablesBalance, Is.EqualTo(0m), "Accounts Payable balance is incorrect");
        }

        [Test]
        public async Task CanExecuteFullAccountingCycle()
        {
            // Run these in a defined order, with clean account setup first
            _accounts.Clear();
            _documents.Clear();
            _transactions.Clear();

            // Start fresh with a new UnitOfWork
            _unitOfWork.Dispose();
            _unitOfWork = new UnitOfWork();

            await SetupChartOfAccounts();

            // 1. Record inventory purchase
            await CreatePurchaseTransaction();

            // 2. Record sale to customer
            await CreateSaleTransaction();

            // 3. Record utility expense
            await CreateExpenseTransaction();

            // 4. Create a payment to supplier
            await CreatePaymentTransaction();

            // 5. Verify account balances
            await VerifyAccountBalances();
        }
    }
}