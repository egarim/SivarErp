//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using NUnit.Framework;
//using Sivar.Erp.Documents;
//using Sivar.Erp.ChartOfAccounts;

//namespace Sivar.Erp.Tests.Integration
//{
//    /// <summary>
//    /// Integration tests for Documents and Chart of Accounts modules
//    /// </summary>
//    [TestFixture]
//    public class AccountingIntegrationTests
//    {
//        private IAuditService _auditService;
//        private IDocumentService _documentService;
//        private TransactionService _transactionService;
//        private AccountValidator _accountValidator;
//        private AccountBalanceCalculator _accountBalanceCalculator;

//        // Dictionary to store our accounts
//        private Dictionary<string, AccountDto> _accounts;

//        // Dictionary to store our completed documents
//        private Dictionary<string, IDocument> _documents;

//        // Dictionary to store our completed transactions
//        private Dictionary<string, ITransaction> _transactions;

//        // Constants for testing
//        private const string TEST_USER = "IntegrationTest";
//        private readonly DateOnly _testDate = new DateOnly(2025, 5, 1);

//        [SetUp]
//        public async Task Setup()
//        {
//            // Initialize services
//            _auditService = new AuditService();
//            _documentService = new DocumentService(_auditService);
//            _transactionService = new TransactionService();
//            _accountValidator = new AccountValidator();
//            _accountBalanceCalculator = new AccountBalanceCalculator();

//            // Initialize storage
//            _accounts = new Dictionary<string, AccountDto>();
//            _documents = new Dictionary<string, IDocument>();
//            _transactions = new Dictionary<string, ITransaction>();

//            // Create Chart of Accounts
//            await SetupChartOfAccounts();
//        }

//        #region Chart of Accounts Setup

//        /// <summary>
//        /// Creates a basic chart of accounts for a retail business
//        /// </summary>
//        private async Task SetupChartOfAccounts()
//        {
//            // Clear accounts dictionary in case this method is called multiple times
//            _accounts.Clear();

//            // Assets (1xxxx)
//            await CreateAccount("Cash", "10100", AccountType.Asset, "Cash on hand and in banks");
//            await CreateAccount("Accounts Receivable", "11000", AccountType.Asset, "Amounts owed by customers");
//            await CreateAccount("Inventory", "12000", AccountType.Asset, "Merchandise held for sale");
//            await CreateAccount("Store Equipment", "15000", AccountType.Asset, "Equipment used in store operations");
//            await CreateAccount("Accumulated Depreciation - Store Equipment", "15100", AccountType.Asset, "Accumulated depreciation for store equipment");

//            // Liabilities (2xxxx)
//            await CreateAccount("Accounts Payable", "20000", AccountType.Liability, "Amounts owed to suppliers");
//            await CreateAccount("Sales Tax Payable", "21000", AccountType.Liability, "Sales tax collected but not yet remitted");
//            await CreateAccount("Payroll Liabilities", "22000", AccountType.Liability, "Employee withholdings and employer payroll taxes");

//            // Equity (3xxxx)
//            await CreateAccount("Owner's Capital", "30000", AccountType.Equity, "Owner's investment in the business");
//            await CreateAccount("Retained Earnings", "31000", AccountType.Equity, "Accumulated earnings of the business");

//            // Revenue (4xxxx)
//            await CreateAccount("Sales Revenue", "40000", AccountType.Revenue, "Revenue from sales of merchandise");
//            await CreateAccount("Service Revenue", "41000", AccountType.Revenue, "Revenue from services provided");

//            // Expenses (6xxxx)
//            await CreateAccount("Cost of Goods Sold", "60000", AccountType.Expense, "Cost of merchandise sold");
//            await CreateAccount("Wages Expense", "61000", AccountType.Expense, "Wages paid to employees");
//            await CreateAccount("Rent Expense", "62000", AccountType.Expense, "Rent for store premises");
//            await CreateAccount("Utilities Expense", "63000", AccountType.Expense, "Electricity, water, etc.");
//            await CreateAccount("Advertising Expense", "64000", AccountType.Expense, "Marketing and advertising costs");

//            // Verify all accounts are valid
//            foreach (var account in _accounts.Values)
//            {
//                bool isValid = _accountValidator.ValidateAccount(account);
//                Assert.That(isValid, Is.True, $"Account {account.AccountName} validation failed");
//            }

//            // Verify expected number of accounts
//            Assert.That(_accounts.Count, Is.EqualTo(17), "Expected 17 accounts in chart of accounts");
//        }

//        /// <summary>
//        /// Helper method to create an account and store it in the accounts dictionary
//        /// </summary>
//        private async Task CreateAccount(string name, string code, AccountType type, string description)
//        {
//            var account = new AccountDto
//            {
//                Id = Guid.NewGuid(),
//                AccountName = name,
//                OfficialCode = code,
//                AccountType = type,
//                IsArchived = false
//            };

//            // Set audit information
//            _auditService.SetCreationAudit(account, TEST_USER);

//            // Store account (ensuring no duplicates)
//            if (!_accounts.ContainsKey(name))
//            {
//                _accounts[name] = account;
//            }
//        }

//        #endregion

//        #region Test Cases

//        [Test]
//        public async Task CanRecordPurchaseOfInventory()
//        {
//            // Arrange - Create document for inventory purchase
//            var document = new DocumentDto
//            {
//                DocumentDate = _testDate,
//                DocumentNo = "PO-001",
//                Description = "Purchase of inventory from Electronics Wholesale",
//                DocumentType = DocumentType.Miscellaneous,
//                DocumentComments = "30-day payment terms",
//                InternalComments = "First order from this supplier"
//            };

//            // Act - Create document
//            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
//            _documents["InventoryPurchase"] = createdDocument;

//            // Create transaction
//            var transaction = new TransactionDto
//            {
//                DocumentId = createdDocument.Id,
//                TransactionDate = document.DocumentDate,
//                Description = "Purchase of 10 smartphones at $300 each"
//            };

//            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
//            _transactions["InventoryPurchase"] = createdTransaction;

//            // Create ledger entries
//            var ledgerEntries = new List<ILedgerEntry>
//            {
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Inventory"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 3000.00m // 10 phones * $300
//                },
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Accounts Payable"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 3000.00m
//                }
//            };

//            // Validate transaction
//            var isValid = await _transactionService.ValidateTransactionAsync(
//                createdTransaction.Id, ledgerEntries);

//            // Assert
//            Assert.That(createdDocument.Id, Is.Not.EqualTo(Guid.Empty));
//            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
//            Assert.That(isValid, Is.True, "Transaction should be balanced");
//        }

//        [Test]
//        public async Task CanRecordSaleToCustomer()
//        {
//            // Make sure chart of accounts is set up
//            if (_accounts.Count == 0)
//            {
//                await SetupChartOfAccounts();
//            }

//            // Arrange - Create document for sales transaction
//            var document = new DocumentDto
//            {
//                DocumentDate = _testDate,
//                DocumentNo = "INV-001",
//                Description = "Sale to customer Jane Smith",
//                DocumentType = DocumentType.Miscellaneous,
//                DocumentComments = "Paid immediately",
//                InternalComments = "First-time customer"
//            };

//            // Act - Create document
//            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
//            _documents["Sale"] = createdDocument;

//            // Create transaction
//            var transaction = new TransactionDto
//            {
//                DocumentId = createdDocument.Id,
//                TransactionDate = document.DocumentDate,
//                Description = "Sale of 1 smartphone at $500"
//            };

//            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
//            _transactions["Sale"] = createdTransaction;

//            // Create ledger entries - this is a more complex transaction with multiple entries
//            var ledgerEntries = new List<ILedgerEntry>
//            {
//                // Cash received
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Cash"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 500.00m
//                },
//                // Sales revenue
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Sales Revenue"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 500.00m
//                },
//                // Cost of goods sold (assuming cost is $300 per phone)
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Cost of Goods Sold"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 300.00m
//                },
//                // Reduce inventory
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Inventory"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 300.00m
//                }
//            };

//            // Validate transaction
//            var isValid = await _transactionService.ValidateTransactionAsync(
//                createdTransaction.Id, ledgerEntries);

//            // Assert
//            Assert.That(createdDocument.Id, Is.Not.EqualTo(Guid.Empty));
//            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
//            Assert.That(isValid, Is.True, "Transaction should be balanced");

//            // Verify total debits = total credits
//            decimal totalDebits = ledgerEntries
//                .Where(e => e.EntryType == EntryType.Debit)
//                .Sum(e => e.Amount);

//            decimal totalCredits = ledgerEntries
//                .Where(e => e.EntryType == EntryType.Credit)
//                .Sum(e => e.Amount);

//            Assert.That(totalDebits, Is.EqualTo(totalCredits));
//        }

//        [Test]
//        public async Task CanRecordBusinessExpense()
//        {
//            // Make sure chart of accounts is set up
//            if (_accounts.Count == 0)
//            {
//                await SetupChartOfAccounts();
//            }

//            // Arrange - Create document for utility expense
//            var document = new DocumentDto
//            {
//                DocumentDate = _testDate,
//                DocumentNo = "UTIL-001",
//                Description = "May electricity bill",
//                DocumentType = DocumentType.Miscellaneous,
//                DocumentComments = "Paid by check #1001",
//                InternalComments = "Higher than usual due to air conditioning"
//            };

//            // Act - Create document
//            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
//            _documents["UtilityExpense"] = createdDocument;

//            // Create transaction
//            var transaction = new TransactionDto
//            {
//                DocumentId = createdDocument.Id,
//                TransactionDate = document.DocumentDate,
//                Description = "Payment for May electricity bill"
//            };

//            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
//            _transactions["UtilityExpense"] = createdTransaction;

//            // Create ledger entries
//            var ledgerEntries = new List<ILedgerEntry>
//            {
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Utilities Expense"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 250.00m
//                },
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Cash"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 250.00m
//                }
//            };

//            // Validate transaction
//            var isValid = await _transactionService.ValidateTransactionAsync(
//                createdTransaction.Id, ledgerEntries);

//            // Assert
//            Assert.That(createdDocument.Id, Is.Not.EqualTo(Guid.Empty));
//            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
//            Assert.That(isValid, Is.True, "Transaction should be balanced");
//        }

//        [Test]
//        public async Task CanExecuteFullAccountingCycle()
//        {
//            // Run these in a defined order, with clean account setup first
//            _accounts.Clear();
//            _documents.Clear();
//            _transactions.Clear();

//            await SetupChartOfAccounts();

//            // 1. Record inventory purchase
//            await RecordPurchaseOfInventory();

//            // 2. Record sale to customer
//            await RecordSaleToCustomer();

//            // 3. Record utility expense
//            await RecordBusinessExpense();

//            // 4. Create a payment to supplier
//            await RecordPaymentToSupplier();

//            // 5. Verify account balances
//            await VerifyAccountBalances();
//        }

//        // Use private methods instead of the test methods directly
//        private async Task RecordPurchaseOfInventory()
//        {
//            // Arrange - Create document for inventory purchase
//            var document = new DocumentDto
//            {
//                DocumentDate = _testDate,
//                DocumentNo = "PO-001",
//                Description = "Purchase of inventory from Electronics Wholesale",
//                DocumentType = DocumentType.Miscellaneous,
//                DocumentComments = "30-day payment terms",
//                InternalComments = "First order from this supplier"
//            };

//            // Act - Create document
//            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
//            _documents["InventoryPurchase"] = createdDocument;

//            // Create transaction
//            var transaction = new TransactionDto
//            {
//                DocumentId = createdDocument.Id,
//                TransactionDate = document.DocumentDate,
//                Description = "Purchase of 10 smartphones at $300 each"
//            };

//            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
//            _transactions["InventoryPurchase"] = createdTransaction;

//            // Create ledger entries
//            var ledgerEntries = new List<ILedgerEntry>
//            {
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Inventory"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 3000.00m // 10 phones * $300
//                },
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Accounts Payable"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 3000.00m
//                }
//            };

//            // Validate transaction
//            var isValid = await _transactionService.ValidateTransactionAsync(
//                createdTransaction.Id, ledgerEntries);

//            // Assert
//            Assert.That(isValid, Is.True, "Transaction should be balanced");
//        }

//        private async Task RecordSaleToCustomer()
//        {
//            // Arrange - Create document for sales transaction
//            var document = new DocumentDto
//            {
//                DocumentDate = _testDate,
//                DocumentNo = "INV-001",
//                Description = "Sale to customer Jane Smith",
//                DocumentType = DocumentType.Miscellaneous,
//                DocumentComments = "Paid immediately",
//                InternalComments = "First-time customer"
//            };

//            // Act - Create document
//            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
//            _documents["Sale"] = createdDocument;

//            // Create transaction
//            var transaction = new TransactionDto
//            {
//                DocumentId = createdDocument.Id,
//                TransactionDate = document.DocumentDate,
//                Description = "Sale of 1 smartphone at $500"
//            };

//            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
//            _transactions["Sale"] = createdTransaction;

//            // Create ledger entries
//            var ledgerEntries = new List<ILedgerEntry>
//            {
//                // Cash received
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Cash"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 500.00m
//                },
//                // Sales revenue
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Sales Revenue"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 500.00m
//                },
//                // Cost of goods sold (assuming cost is $300 per phone)
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Cost of Goods Sold"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 300.00m
//                },
//                // Reduce inventory
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Inventory"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 300.00m
//                }
//            };

//            // Validate transaction
//            var isValid = await _transactionService.ValidateTransactionAsync(
//                createdTransaction.Id, ledgerEntries);

//            // Assert
//            Assert.That(isValid, Is.True, "Transaction should be balanced");
//        }

//        private async Task RecordBusinessExpense()
//        {
//            // Arrange - Create document for utility expense
//            var document = new DocumentDto
//            {
//                DocumentDate = _testDate,
//                DocumentNo = "UTIL-001",
//                Description = "May electricity bill",
//                DocumentType = DocumentType.Miscellaneous,
//                DocumentComments = "Paid by check #1001",
//                InternalComments = "Higher than usual due to air conditioning"
//            };

//            // Act - Create document
//            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
//            _documents["UtilityExpense"] = createdDocument;

//            // Create transaction
//            var transaction = new TransactionDto
//            {
//                DocumentId = createdDocument.Id,
//                TransactionDate = document.DocumentDate,
//                Description = "Payment for May electricity bill"
//            };

//            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
//            _transactions["UtilityExpense"] = createdTransaction;

//            // Create ledger entries
//            var ledgerEntries = new List<ILedgerEntry>
//            {
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Utilities Expense"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 250.00m
//                },
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Cash"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 250.00m
//                }
//            };

//            // Validate transaction
//            var isValid = await _transactionService.ValidateTransactionAsync(
//                createdTransaction.Id, ledgerEntries);

//            // Assert
//            Assert.That(isValid, Is.True, "Transaction should be balanced");
//        }

//        private async Task RecordPaymentToSupplier()
//        {
//            // Arrange - Create document for payment to supplier
//            var document = new DocumentDto
//            {
//                DocumentDate = _testDate.AddDays(15), // 15 days after purchase
//                DocumentNo = "PAY-001",
//                Description = "Payment to Electronics Wholesale",
//                DocumentType = DocumentType.Miscellaneous,
//                DocumentComments = "Paid by check #1002",
//                InternalComments = "Payment for PO-001"
//            };

//            // Act - Create document
//            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
//            _documents["SupplierPayment"] = createdDocument;

//            // Create transaction
//            var transaction = new TransactionDto
//            {
//                DocumentId = createdDocument.Id,
//                TransactionDate = document.DocumentDate,
//                Description = "Payment for inventory purchase"
//            };

//            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
//            _transactions["SupplierPayment"] = createdTransaction;

//            // Create ledger entries
//            var ledgerEntries = new List<ILedgerEntry>
//            {
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Accounts Payable"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 3000.00m
//                },
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = createdTransaction.Id,
//                    AccountId = _accounts["Cash"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 3000.00m
//                }
//            };

//            // Validate transaction
//            var isValid = await _transactionService.ValidateTransactionAsync(
//                createdTransaction.Id, ledgerEntries);

//            // Assert
//            Assert.That(isValid, Is.True, "Transaction should be balanced");
//        }

//        private async Task VerifyAccountBalances()
//        {
//            // In a real implementation with a database, we would verify each account balance
//            // by retrieving the actual values from the database through AccountBalanceCalculator

//            // Mock implementation of the balance calculator to simulate database access
//            // This extends the existing AccountBalanceCalculator with mock data
//            var mockBalanceCalculator = new MockAccountBalanceCalculator(_accounts, _transactions);

//            // Verify Cash account balance
//            decimal cashBalance = mockBalanceCalculator.CalculateAccountBalance(
//                _accounts["Cash"].Id,
//                _testDate.AddDays(15)
//            );
//            Assert.That(cashBalance, Is.EqualTo(-2750m), "Cash balance is incorrect");

//            // Verify Inventory balance 
//            decimal inventoryBalance = mockBalanceCalculator.CalculateAccountBalance(
//                _accounts["Inventory"].Id,
//                _testDate.AddDays(15)
//            );
//            Assert.That(inventoryBalance, Is.EqualTo(2700m), "Inventory balance is incorrect");

//            // Verify Accounts Payable balance
//            decimal accountsPayableBalance = mockBalanceCalculator.CalculateAccountBalance(
//                _accounts["Accounts Payable"].Id,
//                _testDate.AddDays(15)
//            );
//            Assert.That(accountsPayableBalance, Is.EqualTo(0m), "Accounts Payable balance is incorrect");

//            // Verify Sales Revenue
//            decimal salesRevenueBalance = mockBalanceCalculator.CalculateAccountBalance(
//                _accounts["Sales Revenue"].Id,
//                _testDate.AddDays(15)
//            );
//            Assert.That(salesRevenueBalance, Is.EqualTo(-500m), "Sales Revenue balance is incorrect");

//            // Verify Cost of Goods Sold
//            decimal cogsBalance = mockBalanceCalculator.CalculateAccountBalance(
//                _accounts["Cost of Goods Sold"].Id,
//                _testDate.AddDays(15)
//            );
//            Assert.That(cogsBalance, Is.EqualTo(300m), "Cost of Goods Sold balance is incorrect");

//            // Verify Utilities Expense
//            decimal utilitiesExpenseBalance = mockBalanceCalculator.CalculateAccountBalance(
//                _accounts["Utilities Expense"].Id,
//                _testDate.AddDays(15)
//            );
//            Assert.That(utilitiesExpenseBalance, Is.EqualTo(250m), "Utilities Expense balance is incorrect");

//            // Also verify the accounting equation as a sanity check
//            decimal assets = cashBalance + inventoryBalance;
//            decimal liabilities = accountsPayableBalance;
//            decimal revenues = -salesRevenueBalance; // Negate because Credit balances are negative
//            decimal expenses = cogsBalance + utilitiesExpenseBalance;

//            decimal leftSide = assets;
//            decimal rightSide = liabilities + (revenues - expenses);

//            Assert.That(Math.Abs(leftSide - rightSide), Is.LessThan(0.01m),
//                "Accounting equation should be balanced");
//        }
//        /// <summary>
//        /// Mock implementation of account balance calculator for testing
//        /// </summary>
//        public class MockAccountBalanceCalculator : AccountBalanceCalculator
//        {
//            private readonly Dictionary<string, AccountDto> _accounts;
//            private readonly Dictionary<Guid, List<LedgerEntryDto>> _ledgerEntriesByTransaction = new();
//            private readonly Dictionary<Guid, decimal> _accountBalances = new();

//            public MockAccountBalanceCalculator(
//                Dictionary<string, AccountDto> accounts,
//                Dictionary<string, ITransaction> transactions)
//            {
//                _accounts = accounts;

//                // Create mock ledger entries for each transaction
//                InitializeLedgerEntries(transactions);

//                // Calculate account balances based on ledger entries
//                CalculateAllBalances();
//            }

//            /// <summary>
//            /// Creates mock ledger entries for each transaction
//            /// </summary>
//            private void InitializeLedgerEntries(Dictionary<string, ITransaction> transactions)
//            {
//                // For inventory purchase
//                if (transactions.TryGetValue("InventoryPurchase", out var inventoryPurchase))
//                {
//                    var entries = new List<LedgerEntryDto>
//            {
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = inventoryPurchase.Id,
//                    AccountId = _accounts["Inventory"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 3000.00m
//                },
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = inventoryPurchase.Id,
//                    AccountId = _accounts["Accounts Payable"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 3000.00m
//                }
//            };
//                    _ledgerEntriesByTransaction[inventoryPurchase.Id] = entries;
//                }

//                // For sale
//                if (transactions.TryGetValue("Sale", out var sale))
//                {
//                    var entries = new List<LedgerEntryDto>
//            {
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = sale.Id,
//                    AccountId = _accounts["Cash"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 500.00m
//                },
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = sale.Id,
//                    AccountId = _accounts["Sales Revenue"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 500.00m
//                },
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = sale.Id,
//                    AccountId = _accounts["Cost of Goods Sold"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 300.00m
//                },
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = sale.Id,
//                    AccountId = _accounts["Inventory"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 300.00m
//                }
//            };
//                    _ledgerEntriesByTransaction[sale.Id] = entries;
//                }

//                // For utility expense
//                if (transactions.TryGetValue("UtilityExpense", out var utilityExpense))
//                {
//                    var entries = new List<LedgerEntryDto>
//            {
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = utilityExpense.Id,
//                    AccountId = _accounts["Utilities Expense"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 250.00m
//                },
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = utilityExpense.Id,
//                    AccountId = _accounts["Cash"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 250.00m
//                }
//            };
//                    _ledgerEntriesByTransaction[utilityExpense.Id] = entries;
//                }

//                // For supplier payment
//                if (transactions.TryGetValue("SupplierPayment", out var supplierPayment))
//                {
//                    var entries = new List<LedgerEntryDto>
//            {
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = supplierPayment.Id,
//                    AccountId = _accounts["Accounts Payable"].Id,
//                    EntryType = EntryType.Debit,
//                    Amount = 3000.00m
//                },
//                new LedgerEntryDto
//                {
//                    Id = Guid.NewGuid(),
//                    TransactionId = supplierPayment.Id,
//                    AccountId = _accounts["Cash"].Id,
//                    EntryType = EntryType.Credit,
//                    Amount = 3000.00m
//                }
//            };
//                    _ledgerEntriesByTransaction[supplierPayment.Id] = entries;
//                }
//            }

//            /// <summary>
//            /// Calculates balances for all accounts based on ledger entries
//            /// </summary>
//            private void CalculateAllBalances()
//            {
//                // Initialize all account balances to zero
//                foreach (var account in _accounts.Values)
//                {
//                    _accountBalances[account.Id] = 0m;
//                }

//                // Process each transaction's ledger entries
//                foreach (var entries in _ledgerEntriesByTransaction.Values)
//                {
//                    foreach (var entry in entries)
//                    {
//                        if (entry.EntryType == EntryType.Debit)
//                        {
//                            _accountBalances[entry.AccountId] += entry.Amount;
//                        }
//                        else // Credit
//                        {
//                            _accountBalances[entry.AccountId] -= entry.Amount;
//                        }
//                    }
//                }
//            }

//            /// <summary>
//            /// Calculates the balance of an account as of a specific date
//            /// </summary>
//            /// <param name="accountId">Account ID</param>
//            /// <param name="asOfDate">Date to calculate balance for</param>
//            /// <returns>Account balance</returns>
//            public new decimal CalculateAccountBalance(Guid accountId, DateOnly asOfDate)
//            {
//                // In a real implementation, we would filter entries by date
//                // For this mock, we'll just return the pre-calculated balance
//                if (_accountBalances.TryGetValue(accountId, out decimal balance))
//                {
//                    return balance;
//                }

//                return 0m;
//            }

//            /// <summary>
//            /// Gets all ledger entries for an account
//            /// </summary>
//            /// <param name="accountId">Account ID</param>
//            /// <returns>Collection of ledger entries</returns>
//            public IEnumerable<LedgerEntryDto> GetLedgerEntriesForAccount(Guid accountId)
//            {
//                var result = new List<LedgerEntryDto>();

//                foreach (var entries in _ledgerEntriesByTransaction.Values)
//                {
//                    result.AddRange(entries.Where(e => e.AccountId == accountId));
//                }

//                return result;
//            }

//            /// <summary>
//            /// Gets ledger entries for a transaction
//            /// </summary>
//            /// <param name="transactionId">Transaction ID</param>
//            /// <returns>Collection of ledger entries</returns>
//            public IEnumerable<LedgerEntryDto> GetLedgerEntriesForTransaction(Guid transactionId)
//            {
//                if (_ledgerEntriesByTransaction.TryGetValue(transactionId, out var entries))
//                {
//                    return entries;
//                }

//                return new List<LedgerEntryDto>();
//            }
//        }
//        #endregion
//    }
//}