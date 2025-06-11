using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Sivar.Erp.Documents;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.FinancialStatements;
using Sivar.Erp.FinancialStatements.Generation;
using static Sivar.Erp.Tests.Integration.AccountingIntegrationTests;

namespace Sivar.Erp.Tests.Integration
{
    /// <summary>
    /// Integration tests simulating a complete accounting cycle for El Salvador S.A de C.V
    /// </summary>
    [TestFixture]
    public class ElSalvadorCompanySimulationTests
    {
        private IAuditService _auditService;
        private IDocumentService _documentService;
        private TransactionService _transactionService;
        private AccountValidator _accountValidator;
        private AccountBalanceCalculator _accountBalanceCalculator;

        // Dictionary to store our accounts
        private Dictionary<string, AccountDto> _accounts;

        // Dictionary to store our completed documents
        private Dictionary<string, IDocument> _documents;

        // Dictionary to store our completed transactions
        private Dictionary<string, ITransaction> _transactions;

        // Constants for testing
        private const string TEST_USER = "IntegrationTestUser";
        private readonly DateOnly _testStartDate = new DateOnly(2023, 1, 1);  // Beginning of fiscal year
        private readonly DateOnly _testEndDate = new DateOnly(2023, 12, 31);  // End of fiscal year

        // Company details
        private readonly string _companyName = "El Salvador S.A de C.V";
        private readonly string _companyAddress = "Calle La Reforma #123, San Salvador, El Salvador";
        private readonly string _companyCurrency = "USD";

        [SetUp]
        public async Task Setup()
        {
            // Initialize services
            _auditService = new AuditService();
            _documentService = new DocumentService(_auditService);
            _transactionService = new TransactionService();
            _accountValidator = new AccountValidator();
            _accountBalanceCalculator = new AccountBalanceCalculator();

            // Initialize storage
            _accounts = new Dictionary<string, AccountDto>();
            _documents = new Dictionary<string, IDocument>();
            _transactions = new Dictionary<string, ITransaction>();

            // Create Chart of Accounts
            await SetupChartOfAccounts();
        }

        #region Chart of Accounts Setup

        /// <summary>
        /// Creates a comprehensive chart of accounts for El Salvador S.A de C.V
        /// </summary>
        private async Task SetupChartOfAccounts()
        {
            // Clear accounts dictionary in case this method is called multiple times
            _accounts.Clear();

            // Assets (1xxxx)
            // Current Assets
            await CreateAccount("Cash in Bank", "11000", AccountType.Asset, "Operating cash in bank accounts");
            await CreateAccount("Petty Cash", "11100", AccountType.Asset, "Cash on hand for small expenses");
            await CreateAccount("Accounts Receivable - Customers", "12000", AccountType.Asset, "Amounts owed by customers");
            await CreateAccount("Allowance for Doubtful Accounts", "12100", AccountType.Asset, "Estimated uncollectible receivables");
            await CreateAccount("Inventory - Finished Goods", "13000", AccountType.Asset, "Products ready for sale");
            await CreateAccount("Inventory - Raw Materials", "13100", AccountType.Asset, "Materials for production");
            await CreateAccount("Inventory - Work in Process", "13200", AccountType.Asset, "Partially completed products");
            await CreateAccount("Prepaid Expenses", "14000", AccountType.Asset, "Expenses paid in advance");

            // Fixed Assets
            await CreateAccount("Land", "15000", AccountType.Asset, "Land owned by company");
            await CreateAccount("Buildings", "15100", AccountType.Asset, "Buildings owned by company");
            await CreateAccount("Accumulated Depreciation - Buildings", "15150", AccountType.Asset, "Accumulated depreciation for buildings");
            await CreateAccount("Machinery and Equipment", "15200", AccountType.Asset, "Production machinery and equipment");
            await CreateAccount("Accumulated Depreciation - Machinery", "15250", AccountType.Asset, "Accumulated depreciation for machinery");
            await CreateAccount("Vehicles", "15300", AccountType.Asset, "Company-owned vehicles");
            await CreateAccount("Accumulated Depreciation - Vehicles", "15350", AccountType.Asset, "Accumulated depreciation for vehicles");
            await CreateAccount("Office Equipment", "15400", AccountType.Asset, "Office furniture and equipment");
            await CreateAccount("Accumulated Depreciation - Office Equipment", "15450", AccountType.Asset, "Accumulated depreciation for office equipment");

            // Other Assets
            await CreateAccount("Investments", "16000", AccountType.Asset, "Long-term investments");
            await CreateAccount("Intangible Assets", "17000", AccountType.Asset, "Patents, trademarks, and other intangibles");
            await CreateAccount("Accumulated Amortization", "17100", AccountType.Asset, "Accumulated amortization of intangible assets");

            // Liabilities (2xxxx)
            // Current Liabilities
            await CreateAccount("Accounts Payable", "21000", AccountType.Liability, "Amounts owed to suppliers");
            await CreateAccount("Notes Payable - Short Term", "21100", AccountType.Liability, "Short-term loans due within one year");
            await CreateAccount("Accrued Expenses", "21200", AccountType.Liability, "Expenses incurred but not yet paid");
            await CreateAccount("Salaries and Wages Payable", "21300", AccountType.Liability, "Salaries and wages earned but not yet paid");
            await CreateAccount("Income Tax Payable", "21400", AccountType.Liability, "Income taxes due");
            await CreateAccount("VAT Payable", "21500", AccountType.Liability, "Value-added tax collected but not yet remitted");

            // Long-term Liabilities
            await CreateAccount("Long-term Loans", "22000", AccountType.Liability, "Loans due after one year");
            await CreateAccount("Mortgage Payable", "22100", AccountType.Liability, "Mortgage on company property");
            await CreateAccount("Deferred Tax Liability", "22200", AccountType.Liability, "Future tax obligations");

            // Equity (3xxxx)
            await CreateAccount("Common Stock", "31000", AccountType.Equity, "Shareholders' equity");
            await CreateAccount("Additional Paid-in Capital", "31100", AccountType.Equity, "Amount paid by investors above par value");
            await CreateAccount("Retained Earnings", "32000", AccountType.Equity, "Accumulated earnings of the company");
            await CreateAccount("Current Year Earnings", "33000", AccountType.Equity, "Current year profit or loss");

            // Revenue (4xxxx)
            await CreateAccount("Sales - Product Line A", "41000", AccountType.Revenue, "Revenue from product line A");
            await CreateAccount("Sales - Product Line B", "41100", AccountType.Revenue, "Revenue from product line B");
            await CreateAccount("Sales - Services", "42000", AccountType.Revenue, "Revenue from services provided");
            await CreateAccount("Sales Returns and Allowances", "43000", AccountType.Revenue, "Customer returns and allowances");
            await CreateAccount("Sales Discounts", "44000", AccountType.Revenue, "Discounts given to customers");
            await CreateAccount("Other Income", "45000", AccountType.Revenue, "Miscellaneous income");

            // Expenses (6xxxx) - Changed from 5xxxx to match the AccountValidator's expectation
            // Cost of Goods Sold
            await CreateAccount("Cost of Goods Sold - Product Line A", "61000", AccountType.Expense, "Cost of product line A");
            await CreateAccount("Cost of Goods Sold - Product Line B", "61100", AccountType.Expense, "Cost of product line B");
            await CreateAccount("Direct Labor", "62000", AccountType.Expense, "Direct production labor costs");
            await CreateAccount("Manufacturing Overhead", "63000", AccountType.Expense, "Indirect production costs");

            // Operating Expenses
            await CreateAccount("Salaries and Wages", "64000", AccountType.Expense, "Administrative and sales salaries");
            await CreateAccount("Employee Benefits", "64100", AccountType.Expense, "Health insurance, retirement plans, etc.");
            await CreateAccount("Rent Expense", "64200", AccountType.Expense, "Rent for facilities");
            await CreateAccount("Utilities Expense", "64300", AccountType.Expense, "Electricity, water, gas, etc.");
            await CreateAccount("Office Supplies", "64400", AccountType.Expense, "Office consumables");
            await CreateAccount("Advertising and Marketing", "64500", AccountType.Expense, "Promotional expenses");
            await CreateAccount("Professional Fees", "64600", AccountType.Expense, "Legal, accounting, and consulting fees");
            await CreateAccount("Depreciation Expense", "64700", AccountType.Expense, "Depreciation of fixed assets");
            await CreateAccount("Amortization Expense", "64800", AccountType.Expense, "Amortization of intangible assets");

            // Other Expenses
            await CreateAccount("Interest Expense", "65000", AccountType.Expense, "Interest on loans and credit");
            await CreateAccount("Foreign Exchange Loss", "65100", AccountType.Expense, "Losses from currency exchange");
            await CreateAccount("Income Tax Expense", "66000", AccountType.Expense, "Current period income taxes");

            // Debug statement to identify duplicate accounts
            var duplicateAccounts = _accounts.Values
                .GroupBy(a => a.AccountName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateAccounts.Any())
            {
                Console.WriteLine("Duplicate accounts found:");
                foreach (var name in duplicateAccounts)
                {
                    Console.WriteLine($"- {name}");
                }
            }

            // Debug statement to show the expected vs actual count
            Console.WriteLine($"Expected account count: 52, Actual account count: {_accounts.Count}");

            // List all accounts for debugging
            Console.WriteLine("All accounts:");
            foreach (var account in _accounts.Values.OrderBy(a => a.OfficialCode))
            {
                Console.WriteLine($"- {account.AccountName} ({account.OfficialCode})");
            }

            // Verify all accounts are valid
            foreach (var account in _accounts.Values)
            {
                bool isValid = _accountValidator.ValidateAccount(account);
                Assert.That(isValid, Is.True, $"Account {account.AccountName} validation failed");
            }

            // Update the expected count to match the actual count in the chart of accounts
            Assert.That(_accounts.Count, Is.EqualTo(55), "Expected 55 accounts in chart of accounts");
        }

        /// <summary>
        /// Helper method to create an account and store it in the accounts dictionary
        /// </summary>
        private async Task CreateAccount(string name, string code, AccountType type, string description)
        {
            var account = new AccountDto
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
        }

        #endregion

        [Test]
        public async Task ChartOfAccounts_ShouldHaveExpectedStructure()
        {
            // Ensure chart of accounts is set up
            if (_accounts.Count == 0)
            {
                await SetupChartOfAccounts();
            }

            // Test account counts by type
            var assetAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Asset);
            var liabilityAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Liability);
            var equityAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Equity);
            var revenueAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Revenue);
            var expenseAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Expense);

            // Assert - update expected expense account count to 16
            Assert.That(assetAccounts, Is.EqualTo(20), "Should have 20 asset accounts");
            Assert.That(liabilityAccounts, Is.EqualTo(9), "Should have 9 liability accounts");
            Assert.That(equityAccounts, Is.EqualTo(4), "Should have 4 equity accounts");
            Assert.That(revenueAccounts, Is.EqualTo(6), "Should have 6 revenue accounts");
            Assert.That(expenseAccounts, Is.EqualTo(16), "Should have 16 expense accounts"); // Changed from 13 to 16

            // Verify structure follows standard numbering convention
            foreach (var account in _accounts.Values)
            {
                var code = account.OfficialCode;
                var firstDigit = int.Parse(code.Substring(0, 1));

                switch (account.AccountType)
                {
                    case AccountType.Asset:
                        Assert.That(firstDigit, Is.EqualTo(1), $"Asset account {account.AccountName} should start with 1");
                        break;
                    case AccountType.Liability:
                        Assert.That(firstDigit, Is.EqualTo(2), $"Liability account {account.AccountName} should start with 2");
                        break;
                    case AccountType.Equity:
                        Assert.That(firstDigit, Is.EqualTo(3), $"Equity account {account.AccountName} should start with 3");
                        break;
                    case AccountType.Revenue:
                        Assert.That(firstDigit, Is.EqualTo(4), $"Revenue account {account.AccountName} should start with 4");
                        break;
                    case AccountType.Expense:
                        Assert.That(firstDigit, Is.EqualTo(6), $"Expense account {account.AccountName} should start with 6"); // Changed from 5 to 6
                        break;
                }
            }
        }



        #region Transaction Simulation Tests

        [Test]
        public async Task CanRecordOpeningBalances()
        {
            // Arrange - Create document for opening balances
            var document = new DocumentDto
            {
                DocumentDate = _testStartDate,
                DocumentNo = "OB-2023-001",
                Description = "Opening Balances for 2023 Fiscal Year",
                DocumentType = DocumentType.BalanceTransfer,
                DocumentComments = "Initial balances as of January 1, 2023",
                InternalComments = "Transferred from previous accounting system"
            };

            // Act - Create document
            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
            _documents["OpeningBalances"] = createdDocument;

            // Create transaction
            var transaction = new TransactionDto
            {
                DocumentId = createdDocument.Id,
                TransactionDate = document.DocumentDate,
                Description = "Opening balances for El Salvador S.A de C.V"
            };

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            _transactions["OpeningBalances"] = createdTransaction;

            // Create ledger entries for opening balances
            var ledgerEntries = new List<ILedgerEntry>
    {
        // Assets (Debits)
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Cash in Bank"].Id,
            EntryType = EntryType.Debit,
            Amount = 250000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Petty Cash"].Id,
            EntryType = EntryType.Debit,
            Amount = 5000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Accounts Receivable - Customers"].Id,
            EntryType = EntryType.Debit,
            Amount = 75000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Inventory - Finished Goods"].Id,
            EntryType = EntryType.Debit,
            Amount = 180000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Inventory - Raw Materials"].Id,
            EntryType = EntryType.Debit,
            Amount = 120000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Land"].Id,
            EntryType = EntryType.Debit,
            Amount = 500000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Buildings"].Id,
            EntryType = EntryType.Debit,
            Amount = 750000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Machinery and Equipment"].Id,
            EntryType = EntryType.Debit,
            Amount = 350000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Office Equipment"].Id,
            EntryType = EntryType.Debit,
            Amount = 85000.00m
        },
        
        // Contra Assets (Credits)
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Accumulated Depreciation - Buildings"].Id,
            EntryType = EntryType.Credit,
            Amount = 150000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Accumulated Depreciation - Machinery"].Id,
            EntryType = EntryType.Credit,
            Amount = 115000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Accumulated Depreciation - Office Equipment"].Id,
            EntryType = EntryType.Credit,
            Amount = 35000.00m
        },
        
        // Liabilities (Credits)
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Accounts Payable"].Id,
            EntryType = EntryType.Credit,
            Amount = 95000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Long-term Loans"].Id,
            EntryType = EntryType.Credit,
            Amount = 650000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["VAT Payable"].Id,
            EntryType = EntryType.Credit,
            Amount = 15000.00m
        },
        
        // Equity (Credits)
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Common Stock"].Id,
            EntryType = EntryType.Credit,
            Amount = 800000.00m
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Retained Earnings"].Id,
            EntryType = EntryType.Credit,
            Amount = 455000.00m
        }
    };

            // Validate transaction
            var isValid = await _transactionService.ValidateTransactionAsync(
                createdTransaction.Id, ledgerEntries);

            // Assert
            Assert.That(createdDocument.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(isValid, Is.True, "Transaction should be balanced");

            // Verify total debits = total credits
            decimal totalDebits = ledgerEntries
                .Where(e => e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            decimal totalCredits = ledgerEntries
                .Where(e => e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            Assert.That(totalDebits, Is.EqualTo(totalCredits));
        }

        [Test]
        public async Task CanRecordInventoryPurchase()
        {
            // Arrange - Create document for raw materials purchase
            var document = new DocumentDto
            {
                DocumentDate = new DateOnly(2023, 2, 15),
                DocumentNo = "PO-2023-085",
                Description = "Purchase of Raw Materials from Distribuidora Industrial, S.A.",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "Terms: Net 30",
                InternalComments = "For production of Product Line A"
            };

            // Act - Create document
            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
            _documents["RawMaterialsPurchase"] = createdDocument;

            // Create transaction
            var transaction = new TransactionDto
            {
                DocumentId = createdDocument.Id,
                TransactionDate = document.DocumentDate,
                Description = "Purchase of raw materials for Product Line A production"
            };

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            _transactions["RawMaterialsPurchase"] = createdTransaction;

            // Invoice total: $45,000 for raw materials + $5,850 VAT (13% in El Salvador)
            decimal purchaseAmount = 45000.00m;
            decimal vatAmount = 5850.00m; // 13% of purchase amount

            // Create ledger entries
            var ledgerEntries = new List<ILedgerEntry>
    {
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Inventory - Raw Materials"].Id,
            EntryType = EntryType.Debit,
            Amount = purchaseAmount
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Accounts Payable"].Id,
            EntryType = EntryType.Credit,
            Amount = purchaseAmount + vatAmount
        },
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["VAT Payable"].Id,
            EntryType = EntryType.Debit,
            Amount = vatAmount
        }
    };

            // Validate transaction
            var isValid = await _transactionService.ValidateTransactionAsync(
                createdTransaction.Id, ledgerEntries);

            // Assert
            Assert.That(createdDocument.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(isValid, Is.True, "Transaction should be balanced");
        }

        [Test]
        public async Task CanRecordSalesTransaction()
        {
            // Arrange - Create document for sales transaction
            var document = new DocumentDto
            {
                DocumentDate = new DateOnly(2023, 3, 10),
                DocumentNo = "INV-2023-142",
                Description = "Sale to Industrias Consolidadas, S.A.",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "Terms: Net 15",
                InternalComments = "Major client in San Salvador area"
            };

            // Act - Create document
            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
            _documents["ProductSale"] = createdDocument;

            // Create transaction
            var transaction = new TransactionDto
            {
                DocumentId = createdDocument.Id,
                TransactionDate = document.DocumentDate,
                Description = "Sale of Product Line A items"
            };

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            _transactions["ProductSale"] = createdTransaction;

            // Sales invoice: $65,000 + $8,450 VAT (13% in El Salvador)
            decimal saleAmount = 65000.00m;
            decimal vatAmount = 8450.00m; // 13% of sale amount
            decimal costOfGoodsAmount = 39000.00m; // Cost of items sold

            // Create ledger entries
            var ledgerEntries = new List<ILedgerEntry>
    {
        // Record the sale to accounts receivable
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Accounts Receivable - Customers"].Id,
            EntryType = EntryType.Debit,
            Amount = saleAmount + vatAmount
        },
        // Record the revenue
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Sales - Product Line A"].Id,
            EntryType = EntryType.Credit,
            Amount = saleAmount
        },
        // Record the VAT collected
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["VAT Payable"].Id,
            EntryType = EntryType.Credit,
            Amount = vatAmount
        },
        // Record Cost of Goods Sold
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Cost of Goods Sold - Product Line A"].Id,
            EntryType = EntryType.Debit,
            Amount = costOfGoodsAmount
        },
        // Reduce inventory
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Inventory - Finished Goods"].Id,
            EntryType = EntryType.Credit,
            Amount = costOfGoodsAmount
        }
    };

            // Validate transaction
            var isValid = await _transactionService.ValidateTransactionAsync(
                createdTransaction.Id, ledgerEntries);

            // Assert
            Assert.That(createdDocument.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(isValid, Is.True, "Transaction should be balanced");
        }

        [Test]
        public async Task CanRecordExpensePayments()
        {
            // Arrange - Create document for expense payments
            var document = new DocumentDto
            {
                DocumentDate = new DateOnly(2023, 3, 31),
                DocumentNo = "EXP-2023-031",
                Description = "March 2023 Operating Expenses",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "Monthly expenses paid by bank transfer",
                InternalComments = "Regular monthly operating costs"
            };

            // Act - Create document
            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
            _documents["OperatingExpenses"] = createdDocument;

            // Create transaction
            var transaction = new TransactionDto
            {
                DocumentId = createdDocument.Id,
                TransactionDate = document.DocumentDate,
                Description = "Payment of March 2023 operating expenses"
            };

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            _transactions["OperatingExpenses"] = createdTransaction;

            // Create ledger entries for various expenses
            var ledgerEntries = new List<ILedgerEntry>
    {
        // Salaries expense
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Salaries and Wages"].Id,
            EntryType = EntryType.Debit,
            Amount = 28500.00m
        },
        // Employee benefits
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Employee Benefits"].Id,
            EntryType = EntryType.Debit,
            Amount = 6400.00m
        },
        // Rent expense
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Rent Expense"].Id,
            EntryType = EntryType.Debit,
            Amount = 12000.00m
        },
        // Utilities expense
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Utilities Expense"].Id,
            EntryType = EntryType.Debit,
            Amount = 5200.00m
        },
        // Office supplies
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Office Supplies"].Id,
            EntryType = EntryType.Debit,
            Amount = 1800.00m
        },
        // Payment from cash in bank
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Cash in Bank"].Id,
            EntryType = EntryType.Credit,
            Amount = 53900.00m
        }
    };

            // Validate transaction
            var isValid = await _transactionService.ValidateTransactionAsync(
                createdTransaction.Id, ledgerEntries);

            // Assert
            Assert.That(createdDocument.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(isValid, Is.True, "Transaction should be balanced");
        }

        [Test]
        public async Task CanRecordAssetAcquisitionAndDepreciation()
        {
            // PART 1: Asset Acquisition
            // Arrange - Create document for asset acquisition
            var acquisitionDocument = new DocumentDto
            {
                DocumentDate = new DateOnly(2023, 4, 15),
                DocumentNo = "FA-2023-016",
                Description = "Purchase of Packaging Equipment",
                DocumentType = DocumentType.FixedAssetsOperational,
                DocumentComments = "Paid by bank transfer to Maquinaria Industrial, S.A.",
                InternalComments = "5-year useful life, straight-line depreciation"
            };

            // Act - Create document
            var createdAcquisitionDocument = await _documentService.CreateDocumentAsync(acquisitionDocument, TEST_USER);
            _documents["EquipmentAcquisition"] = createdAcquisitionDocument;

            // Create transaction
            var acquisitionTransaction = new TransactionDto
            {
                DocumentId = createdAcquisitionDocument.Id,
                TransactionDate = acquisitionDocument.DocumentDate,
                Description = "Purchase of new packaging equipment for production line"
            };

            var createdAcquisitionTransaction = await _transactionService.CreateTransactionAsync(acquisitionTransaction);
            _transactions["EquipmentAcquisition"] = createdAcquisitionTransaction;

            // Asset purchase: $85,000 + $11,050 VAT (13% in El Salvador)
            decimal assetCost = 85000.00m;
            decimal vatAmount = 11050.00m;

            // Create ledger entries
            var acquisitionEntries = new List<ILedgerEntry>
    {
        // Record the asset purchase
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdAcquisitionTransaction.Id,
            AccountId = _accounts["Machinery and Equipment"].Id,
            EntryType = EntryType.Debit,
            Amount = assetCost
        },
        // Record VAT paid
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdAcquisitionTransaction.Id,
            AccountId = _accounts["VAT Payable"].Id,
            EntryType = EntryType.Debit,
            Amount = vatAmount
        },
        // Payment from bank account
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdAcquisitionTransaction.Id,
            AccountId = _accounts["Cash in Bank"].Id,
            EntryType = EntryType.Credit,
            Amount = assetCost + vatAmount
        }
    };

            // Validate transaction
            var isAcquisitionValid = await _transactionService.ValidateTransactionAsync(
                createdAcquisitionTransaction.Id, acquisitionEntries);

            // Assert acquisition
            Assert.That(createdAcquisitionDocument.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdAcquisitionTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(isAcquisitionValid, Is.True, "Asset acquisition transaction should be balanced");

            // PART 2: Depreciation Entry
            // Arrange - Create document for monthly depreciation
            var depreciationDocument = new DocumentDto
            {
                DocumentDate = new DateOnly(2023, 4, 30),
                DocumentNo = "DEP-2023-004",
                Description = "Monthly Depreciation Entry",
                DocumentType = DocumentType.FixedAssetsDepreciation,
                DocumentComments = "April 2023 depreciation",
                InternalComments = "Includes depreciation on newly acquired packaging equipment"
            };

            // Act - Create document
            var createdDepreciationDocument = await _documentService.CreateDocumentAsync(depreciationDocument, TEST_USER);
            _documents["MonthlyDepreciation"] = createdDepreciationDocument;

            // Create transaction
            var depreciationTransaction = new TransactionDto
            {
                DocumentId = createdDepreciationDocument.Id,
                TransactionDate = depreciationDocument.DocumentDate,
                Description = "April 2023 depreciation expense"
            };

            var createdDepreciationTransaction = await _transactionService.CreateTransactionAsync(depreciationTransaction);
            _transactions["MonthlyDepreciation"] = createdDepreciationTransaction;

            // Monthly depreciation calculation:
            // New equipment: $85,000 / 5 years / 12 months = $1,416.67 per month
            // Existing assets: buildings $3,125, machinery $4,583.33, office equipment $1,416.67
            decimal newEquipmentDepreciation = 1416.67m;
            decimal buildingsDepreciation = 3125.00m;
            decimal machineryDepreciation = 4583.33m;
            decimal officeEquipmentDepreciation = 1416.67m;
            decimal totalDepreciation = newEquipmentDepreciation + buildingsDepreciation +
                                       machineryDepreciation + officeEquipmentDepreciation;

            // Create ledger entries
            var depreciationEntries = new List<ILedgerEntry>
    {
        // Record the depreciation expense
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdDepreciationTransaction.Id,
            AccountId = _accounts["Depreciation Expense"].Id,
            EntryType = EntryType.Debit,
            Amount = totalDepreciation
        },
        // Increase accumulated depreciation - buildings
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdDepreciationTransaction.Id,
            AccountId = _accounts["Accumulated Depreciation - Buildings"].Id,
            EntryType = EntryType.Credit,
            Amount = buildingsDepreciation
        },
        // Increase accumulated depreciation - machinery
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdDepreciationTransaction.Id,
            AccountId = _accounts["Accumulated Depreciation - Machinery"].Id,
            EntryType = EntryType.Credit,
            Amount = machineryDepreciation + newEquipmentDepreciation
        },
        // Increase accumulated depreciation - office equipment
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdDepreciationTransaction.Id,
            AccountId = _accounts["Accumulated Depreciation - Office Equipment"].Id,
            EntryType = EntryType.Credit,
            Amount = officeEquipmentDepreciation
        }
    };

            // Validate transaction
            var isDepreciationValid = await _transactionService.ValidateTransactionAsync(
                createdDepreciationTransaction.Id, depreciationEntries);

            // Assert depreciation
            Assert.That(createdDepreciationDocument.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdDepreciationTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(isDepreciationValid, Is.True, "Depreciation transaction should be balanced");
        }

        [Test]
        public async Task CanRecordLoanPaymentWithInterest()
        {
            // Arrange - Create document for loan payment
            var document = new DocumentDto
            {
                DocumentDate = new DateOnly(2023, 3, 31),
                DocumentNo = "LP-2023-003",
                Description = "Monthly Payment - Long-term Loan",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "Payment to Banco Agricola, S.A.",
                InternalComments = "Loan #12345-67890, 8% annual interest rate"
            };

            // Act - Create document
            var createdDocument = await _documentService.CreateDocumentAsync(document, TEST_USER);
            _documents["LoanPayment"] = createdDocument;

            // Create transaction
            var transaction = new TransactionDto
            {
                DocumentId = createdDocument.Id,
                TransactionDate = document.DocumentDate,
                Description = "March 2023 loan payment - principal and interest"
            };

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            _transactions["LoanPayment"] = createdTransaction;

            // Monthly loan payment calculation:
            // Principal: $10,000
            // Interest: $650,000 * 8% / 12 months = $4,333.33
            decimal principalPayment = 10000.00m;
            decimal interestPayment = 4333.33m;
            decimal totalPayment = principalPayment + interestPayment;

            // Create ledger entries
            var ledgerEntries = new List<ILedgerEntry>
    {
        // Principal payment reduces the loan liability
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Long-term Loans"].Id,
            EntryType = EntryType.Debit,
            Amount = principalPayment
        },
        // Interest expense
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Interest Expense"].Id,
            EntryType = EntryType.Debit,
            Amount = interestPayment
        },
        // Payment from bank account
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Cash in Bank"].Id,
            EntryType = EntryType.Credit,
            Amount = totalPayment
        }
    };

            // Validate transaction
            var isValid = await _transactionService.ValidateTransactionAsync(
                createdTransaction.Id, ledgerEntries);

            // Assert
            Assert.That(createdDocument.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(isValid, Is.True, "Transaction should be balanced");
        }

        [Test]
        public async Task CanExecuteFullQuarterAccountingCycle()
        {
            // Clear previous test data
            _accounts.Clear();
            _documents.Clear();
            _transactions.Clear();

            // Set up chart of accounts
            await SetupChartOfAccounts();

            // 1. Record opening balances for the fiscal year
            await CanRecordOpeningBalances();

            // 2. Record raw materials purchase
            await CanRecordInventoryPurchase();

            // 3. Record sales transaction with revenue recognition
            await CanRecordSalesTransaction();

            // 4. Record payment of operating expenses
            await CanRecordExpensePayments();

            // 5. Record asset acquisition and monthly depreciation
            await CanRecordAssetAcquisitionAndDepreciation();

            // 6. Record loan payment with principal and interest
            await CanRecordLoanPaymentWithInterest();

            // 7. Verify account balances at the end of Q1 2023
            // Mock implementation of the balance calculator to simulate database access
            var mockBalanceCalculator = new MockAccountBalanceCalculator(_accounts, _transactions);

            // Verify key account balances
            decimal cashBalance = mockBalanceCalculator.CalculateAccountBalance(
                _accounts["Cash in Bank"].Id,
                new DateOnly(2023, 3, 31)
            );

            decimal accountsReceivableBalance = mockBalanceCalculator.CalculateAccountBalance(
                _accounts["Accounts Receivable - Customers"].Id,
                new DateOnly(2023, 3, 31)
            );

            decimal inventoryFinishedGoodsBalance = mockBalanceCalculator.CalculateAccountBalance(
                _accounts["Inventory - Finished Goods"].Id,
                new DateOnly(2023, 3, 31)
            );

            decimal salesRevenueBalance = mockBalanceCalculator.CalculateAccountBalance(
                _accounts["Sales - Product Line A"].Id,
                new DateOnly(2023, 3, 31)
            );

            decimal retainedEarningsBalance = mockBalanceCalculator.CalculateAccountBalance(
                _accounts["Retained Earnings"].Id,
                new DateOnly(2023, 3, 31)
            );

            // Assert expected balances based on transactions
            Assert.That(cashBalance, Is.GreaterThan(0), "Cash balance should be positive");
            Assert.That(accountsReceivableBalance, Is.GreaterThan(0), "Accounts receivable should have a positive balance");
            Assert.That(inventoryFinishedGoodsBalance, Is.GreaterThan(0), "Inventory should have a positive balance");
            Assert.That(salesRevenueBalance, Is.LessThan(0), "Sales revenue should have a credit (negative) balance");
            Assert.That(retainedEarningsBalance, Is.LessThan(0), "Retained earnings should have a credit (negative) balance");
        }


        #endregion

        #region Document Generation Tests

        [Test]
        public async Task CanGenerateSalesInvoice()
        {
            // 1. First create a chart of accounts if not already set up
            if (_accounts.Count == 0)
            {
                await SetupChartOfAccounts();
            }

            // 2. Define customer information
            var customer = new CustomerInfo
            {
                Name = "Industrias Consolidadas, S.A.",
                TaxId = "0614-010182-001-0",  // El Salvador NIT format
                Address = "Boulevard del Ejército, Km. 3, Soyapango, San Salvador",
                Email = "compras@industriasconsolidadas.com",
                PhoneNumber = "2277-8800"
            };

            // 3. Create the sales invoice document
            var invoiceDate = new DateOnly(2023, 3, 15);
            var invoiceDocument = new DocumentDto
            {
                DocumentDate = invoiceDate,
                DocumentNo = "FAC-2023-165",
                Description = $"Venta a {customer.Name}",
                DocumentType = DocumentType.Miscellaneous,  // Using a general type, could be customized
                DocumentComments = "Condiciones de pago: 30 días",
                InternalComments = "Cliente frecuente de la zona metropolitana"
            };

            // 4. Add the document to the system
            var createdInvoice = await _documentService.CreateDocumentAsync(invoiceDocument, TEST_USER);
            _documents["SalesInvoice"] = createdInvoice;

            // 5. Generate the invoice items
            var invoiceItems = new List<InvoiceItemDto>
    {
        new InvoiceItemDto
        {
            ItemCode = "PROD-A-001",
            Description = "Procesador de alimentos industrial",
            Quantity = 2,
            UnitPrice = 7500.00m,
            DiscountPercentage = 5,
            TaxRate = 13 // 13% VAT in El Salvador
        },
        new InvoiceItemDto
        {
            ItemCode = "PROD-A-005",
            Description = "Mezcladora industrial 50L",
            Quantity = 1,
            UnitPrice = 12000.00m,
            DiscountPercentage = 0,
            TaxRate = 13 // 13% VAT in El Salvador
        },
        new InvoiceItemDto
        {
            ItemCode = "SRV-INST",
            Description = "Servicio de instalación y configuración",
            Quantity = 1,
            UnitPrice = 1500.00m,
            DiscountPercentage = 0,
            TaxRate = 13 // 13% VAT in El Salvador
        }
    };

            // 6. Calculate invoice totals
            decimal subtotal = 0;
            decimal discounts = 0;
            decimal taxableAmount = 0;
            decimal taxAmount = 0;
            decimal totalAmount = 0;

            foreach (var item in invoiceItems)
            {
                decimal lineAmount = item.Quantity * item.UnitPrice;
                decimal lineDiscount = lineAmount * (item.DiscountPercentage / 100);
                decimal lineTaxableAmount = lineAmount - lineDiscount;
                decimal lineTaxAmount = lineTaxableAmount * (item.TaxRate / 100);

                subtotal += lineAmount;
                discounts += lineDiscount;
                taxableAmount += lineTaxableAmount;
                taxAmount += lineTaxAmount;
            }

            totalAmount = taxableAmount + taxAmount;

            // 7. Create a transaction for this invoice
            var transaction = new TransactionDto
            {
                DocumentId = createdInvoice.Id,
                TransactionDate = invoiceDate,
                Description = $"Venta según factura {invoiceDocument.DocumentNo}"
            };

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            _transactions["SalesInvoice"] = createdTransaction;

            // 8. Create the accounting entries
            var ledgerEntries = new List<ILedgerEntry>
    {
        // Record accounts receivable
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Accounts Receivable - Customers"].Id,
            EntryType = EntryType.Debit,
            Amount = totalAmount
        },
        // Record sales revenue
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Sales - Product Line A"].Id,
            EntryType = EntryType.Credit,
            Amount = taxableAmount
        },
        // Record VAT payable
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["VAT Payable"].Id,
            EntryType = EntryType.Credit,
            Amount = taxAmount
        },
        // Record Cost of Goods Sold (assuming 60% of sale price)
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Cost of Goods Sold - Product Line A"].Id,
            EntryType = EntryType.Debit,
            Amount = taxableAmount * 0.6m
        },
        // Reduce inventory
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Inventory - Finished Goods"].Id,
            EntryType = EntryType.Credit,
            Amount = taxableAmount * 0.6m
        }
    };

            // 9. Validate the transaction
            var isValid = await _transactionService.ValidateTransactionAsync(
                createdTransaction.Id, ledgerEntries);

            // 10. Create the invoice document metadata
            var invoiceMetadata = new SalesInvoiceMetadata
            {
                DocumentId = createdInvoice.Id,
                InvoiceNumber = invoiceDocument.DocumentNo,
                InvoiceDate = invoiceDate,
                CustomerInfo = customer,
                Items = invoiceItems,
                Subtotal = subtotal,
                Discounts = discounts,
                TaxableAmount = taxableAmount,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                PaymentTerms = "30 días",
                Currency = _companyCurrency,
                CompanyName = _companyName,
                CompanyAddress = _companyAddress,
                CompanyTaxId = "0614-151290-101-1" // Example NIT for our company
            };

            // 11. In a real system, we would generate the actual PDF/printable invoice here
            // For test purposes, we'll just verify that all data is ready for document generation

            // Assert
            Assert.That(createdInvoice.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(isValid, Is.True, "Invoice transaction should be balanced");
            Assert.That(invoiceItems.Count, Is.EqualTo(3), "Invoice should have 3 line items");
            Assert.That(Math.Round(totalAmount, 2), Is.EqualTo(27740.50m), "Total invoice amount should match expected value");
        }

        [Test]
        public async Task CanGeneratePurchaseOrder()
        {
            // 1. First create a chart of accounts if not already set up
            if (_accounts.Count == 0)
            {
                await SetupChartOfAccounts();
            }

            // 2. Define supplier information
            var supplier = new SupplierInfo
            {
                Name = "Maquinaria Industrial, S.A.",
                TaxId = "0614-120591-001-2",  // El Salvador NIT format
                Address = "Calle Antigua a Soyapango, Bodega 15, Ilopango, San Salvador",
                Email = "ventas@maquinariaindustrial.com",
                PhoneNumber = "2294-7700",
                SupplierCode = "SUP-0123"
            };

            // 3. Create the purchase order document
            var poDate = new DateOnly(2023, 4, 5);
            var poDocument = new DocumentDto
            {
                DocumentDate = poDate,
                DocumentNo = "OC-2023-042",
                Description = $"Orden de Compra a {supplier.Name}",
                DocumentType = DocumentType.Miscellaneous,  // Using a general type, could be customized
                DocumentComments = "Entregar en bodega central",
                InternalComments = "Para reposición de inventario"
            };

            // 4. Add the document to the system
            var createdPO = await _documentService.CreateDocumentAsync(poDocument, TEST_USER);
            _documents["PurchaseOrder"] = createdPO;

            // 5. Generate the purchase order items
            var poItems = new List<PurchaseOrderItemDto>
    {
        new PurchaseOrderItemDto
        {
            ItemCode = "MP-203",
            Description = "Motor eléctrico trifásico 5HP",
            Quantity = 10,
            UnitPrice = 350.00m,
            ExpectedDeliveryDate = poDate.AddDays(15)
        },
        new PurchaseOrderItemDto
        {
            ItemCode = "MP-115",
            Description = "Panel de control automático",
            Quantity = 5,
            UnitPrice = 620.00m,
            ExpectedDeliveryDate = poDate.AddDays(15)
        },
        new PurchaseOrderItemDto
        {
            ItemCode = "MP-309",
            Description = "Gabinete metálico industrial",
            Quantity = 5,
            UnitPrice = 450.00m,
            ExpectedDeliveryDate = poDate.AddDays(30)
        }
    };

            // 6. Calculate purchase order totals
            decimal subtotal = 0;
            decimal taxAmount = 0;
            decimal totalAmount = 0;

            foreach (var item in poItems)
            {
                decimal lineAmount = item.Quantity * item.UnitPrice;
                subtotal += lineAmount;
            }

            taxAmount = subtotal * 0.13m; // 13% VAT in El Salvador
            totalAmount = subtotal + taxAmount;

            // 7. In a real system this would be a commitment but wouldn't post accounting entries yet
            // However, for the test we'll create a transaction showing receipt of goods

            // 8. Create the purchase order metadata
            var poMetadata = new PurchaseOrderMetadata
            {
                DocumentId = createdPO.Id,
                PurchaseOrderNumber = poDocument.DocumentNo,
                PODate = poDate,
                SupplierInfo = supplier,
                Items = poItems,
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                DeliveryInstructions = "Entregar en bodega central, preguntar por Jefe de Bodega",
                PaymentTerms = "Crédito 45 días",
                Currency = _companyCurrency,
                CompanyName = _companyName,
                CompanyAddress = _companyAddress,
                CompanyTaxId = "0614-151290-101-1" // Example NIT for our company
            };

            // 9. Assert
            Assert.That(createdPO.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(poItems.Count, Is.EqualTo(3), "Purchase order should have 3 line items");
            Assert.That(Math.Round(totalAmount, 2), Is.EqualTo(8208.50m), "Total PO amount should match expected value");
        }

        [Test]
        public async Task CanGeneratePaymentReceipt()
        {
            // 1. First create a chart of accounts if not already set up
            if (_accounts.Count == 0)
            {
                await SetupChartOfAccounts();
            }

            // 2. Define customer information
            var customer = new CustomerInfo
            {
                Name = "Distribuidora Nacional, S.A.",
                TaxId = "0614-220588-002-4",  // El Salvador NIT format
                Address = "Avenida Los Próceres, Edificio Torre Distribución, San Salvador",
                Email = "pagos@disnacional.com",
                PhoneNumber = "2298-4400"
            };

            // 3. Create the payment receipt document
            var receiptDate = new DateOnly(2023, 5, 12);
            var receiptDocument = new DocumentDto
            {
                DocumentDate = receiptDate,
                DocumentNo = "REC-2023-098",
                Description = $"Recibo de pago - {customer.Name}",
                DocumentType = DocumentType.Miscellaneous,  // Using a general type, could be customized
                DocumentComments = "Pago de facturas pendientes",
                InternalComments = "Cobro gestionado por Departamento de Créditos y Cobros"
            };

            // 4. Add the document to the system
            var createdReceipt = await _documentService.CreateDocumentAsync(receiptDocument, TEST_USER);
            _documents["PaymentReceipt"] = createdReceipt;

            // 5. Define invoices being paid
            var invoicesPaid = new List<InvoiceReference>
    {
        new InvoiceReference
        {
            InvoiceNumber = "FAC-2023-118",
            InvoiceDate = new DateOnly(2023, 3, 15),
            OriginalAmount = 12500.00m,
            AmountPaid = 12500.00m,
            Balance = 0.00m
        },
        new InvoiceReference
        {
            InvoiceNumber = "FAC-2023-142",
            InvoiceDate = new DateOnly(2023, 4, 2),
            OriginalAmount = 8750.00m,
            AmountPaid = 8750.00m,
            Balance = 0.00m
        }
    };

            // 6. Calculate receipt total
            decimal totalPayment = invoicesPaid.Sum(i => i.AmountPaid);

            // 7. Create a transaction for this receipt
            var transaction = new TransactionDto
            {
                DocumentId = createdReceipt.Id,
                TransactionDate = receiptDate,
                Description = $"Cobro según recibo {receiptDocument.DocumentNo}"
            };

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            _transactions["PaymentReceipt"] = createdTransaction;

            // 8. Create the accounting entries
            var ledgerEntries = new List<ILedgerEntry>
    {
        // Record cash received
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Cash in Bank"].Id,
            EntryType = EntryType.Debit,
            Amount = totalPayment
        },
        // Reduce accounts receivable
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdTransaction.Id,
            AccountId = _accounts["Accounts Receivable - Customers"].Id,
            EntryType = EntryType.Credit,
            Amount = totalPayment
        }
    };

            // 9. Validate the transaction
            var isValid = await _transactionService.ValidateTransactionAsync(
                createdTransaction.Id, ledgerEntries);

            // 10. Create the receipt metadata
            var receiptMetadata = new PaymentReceiptMetadata
            {
                DocumentId = createdReceipt.Id,
                ReceiptNumber = receiptDocument.DocumentNo,
                ReceiptDate = receiptDate,
                CustomerInfo = customer,
                InvoicesPaid = invoicesPaid,
                PaymentMethod = "Transferencia Bancaria",
                ReferenceNumber = "TRF-123456",
                TotalAmount = totalPayment,
                Currency = _companyCurrency,
                CompanyName = _companyName,
                CompanyAddress = _companyAddress
            };

            // 11. Assert
            Assert.That(createdReceipt.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(isValid, Is.True, "Receipt transaction should be balanced");
            Assert.That(invoicesPaid.Count, Is.EqualTo(2), "Receipt should have 2 invoices paid");
            Assert.That(totalPayment, Is.EqualTo(21250.00m), "Total receipt amount should match expected value");
        }

        [Test]
        public async Task CanGenerateJournalEntryDocument()
        {
            // 1. First create a chart of accounts if not already set up
            if (_accounts.Count == 0)
            {
                await SetupChartOfAccounts();
            }

            // 2. Create the journal entry document
            var journalDate = new DateOnly(2023, 6, 30);
            var journalDocument = new DocumentDto
            {
                DocumentDate = journalDate,
                DocumentNo = "DIE-2023-065",
                Description = "Asiento de Diario - Ajustes de fin de mes",
                DocumentType = DocumentType.AccountingNote,  // Using accounting note type
                DocumentComments = "Ajustes contables junio 2023",
                InternalComments = "Preparado por el Contador General"
            };

            // 3. Add the document to the system
            var createdJournal = await _documentService.CreateDocumentAsync(journalDocument, TEST_USER);
            _documents["JournalEntry"] = createdJournal;

            // 4. Create a transaction for this journal entry
            var transaction = new TransactionDto
            {
                DocumentId = createdJournal.Id,
                TransactionDate = journalDate,
                Description = "Ajustes varios de fin de mes"
            };

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            _transactions["JournalEntry"] = createdTransaction;

            // 5. Create entries for a few typical month-end adjustments
            var journalEntries = new List<JournalEntryLineDto>
    {
        // Accrual for utilities not yet billed
        new JournalEntryLineDto
        {
            AccountId = _accounts["Utilities Expense"].Id,
            AccountCode = "64300",
            AccountName = "Utilities Expense",
            Debit = 3200.00m,
            Credit = 0.00m,
            Description = "Provisión gastos servicios pendientes de facturar"
        },
        new JournalEntryLineDto
        {
            AccountId = _accounts["Accrued Expenses"].Id,
            AccountCode = "21200",
            AccountName = "Accrued Expenses",
            Debit = 0.00m,
            Credit = 3200.00m,
            Description = "Provisión gastos servicios pendientes de facturar"
        },
        
        // Record prepaid expenses consumed
        new JournalEntryLineDto
        {
            AccountId = _accounts["Office Supplies"].Id,
            AccountCode = "64400",
            AccountName = "Office Supplies",
            Debit = 1750.00m,
            Credit = 0.00m,
            Description = "Reconocimiento de gastos prepagados - Junio"
        },
        new JournalEntryLineDto
        {
            AccountId = _accounts["Prepaid Expenses"].Id,
            AccountCode = "14000",
            AccountName = "Prepaid Expenses",
            Debit = 0.00m,
            Credit = 1750.00m,
            Description = "Reconocimiento de gastos prepagados - Junio"
        },
        
        // Provision for doubtful accounts
        new JournalEntryLineDto
        {
            AccountId = _accounts["Sales - Product Line A"].Id,  // This would typically be a bad debt expense account
            AccountCode = "41000",
            AccountName = "Sales - Product Line A",
            Debit = 2500.00m,
            Credit = 0.00m,
            Description = "Provisión cuentas dudosas - Junio"
        },
        new JournalEntryLineDto
        {
            AccountId = _accounts["Allowance for Doubtful Accounts"].Id,
            AccountCode = "12100",
            AccountName = "Allowance for Doubtful Accounts",
            Debit = 0.00m,
            Credit = 2500.00m,
            Description = "Provisión cuentas dudosas - Junio"
        }
    };

            // 6. Calculate journal totals
            decimal totalDebits = journalEntries.Sum(je => je.Debit);
            decimal totalCredits = journalEntries.Sum(je => je.Credit);

            // 7. Create the accounting entries in the system
            var ledgerEntries = new List<ILedgerEntry>();

            foreach (var entry in journalEntries)
            {
                if (entry.Debit > 0)
                {
                    ledgerEntries.Add(new LedgerEntryDto
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = createdTransaction.Id,
                        AccountId = entry.AccountId,
                        EntryType = EntryType.Debit,
                        Amount = entry.Debit
                    });
                }

                if (entry.Credit > 0)
                {
                    ledgerEntries.Add(new LedgerEntryDto
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = createdTransaction.Id,
                        AccountId = entry.AccountId,
                        EntryType = EntryType.Credit,
                        Amount = entry.Credit
                    });
                }
            }

            // 8. Validate the transaction
            var isValid = await _transactionService.ValidateTransactionAsync(
                createdTransaction.Id, ledgerEntries);

            // 9. Create the journal entry metadata
            var journalMetadata = new JournalEntryMetadata
            {
                DocumentId = createdJournal.Id,
                JournalNumber = journalDocument.DocumentNo,
                JournalDate = journalDate,
                Entries = journalEntries,
                TotalDebits = totalDebits,
                TotalCredits = totalCredits,
                PreparedBy = "Carlos Mendoza",
                ApprovedBy = "María Velasquez",
                CompanyName = _companyName,
                FiscalPeriod = "Junio 2023"
            };

            // 10. Assert
            Assert.That(createdJournal.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(isValid, Is.True, "Journal entry transaction should be balanced");
            Assert.That(totalDebits, Is.EqualTo(totalCredits), "Total debits should equal total credits");
            Assert.That(journalEntries.Count, Is.EqualTo(6), "Journal should have 6 entry lines");
        }

        [Test]
        public async Task CanGenerateCompleteDocumentationForTransactions()
        {
            // This test simulates a complete business process with all documentation:
            // 1. Purchase Order
            // 2. Vendor Invoice
            // 3. Goods Receipt
            // 4. Payment to Vendor

            // Create a chart of accounts if not already set up
            if (_accounts.Count == 0)
            {
                await SetupChartOfAccounts();
            }

            // Step 1: Generate Purchase Order
            var supplier = new SupplierInfo
            {
                Name = "Suministros Técnicos, S.A.",
                TaxId = "0614-180395-003-8",
                Address = "Calle El Progreso, Col. Escalón, San Salvador",
                Email = "ordenes@suministecnicos.com.sv",
                PhoneNumber = "2263-5500",
                SupplierCode = "SUP-0375"
            };

            var poDate = new DateOnly(2023, 7, 5);
            var poDocument = new DocumentDto
            {
                DocumentDate = poDate,
                DocumentNo = "OC-2023-087",
                Description = $"Orden de Compra a {supplier.Name}",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "Entregar según especificaciones técnicas adjuntas",
                InternalComments = "Aprobado por Gerencia de Producción"
            };

            var createdPO = await _documentService.CreateDocumentAsync(poDocument, TEST_USER);
            _documents["ProcurementPO"] = createdPO;

            // Purchase Order Items
            var poItems = new List<PurchaseOrderItemDto>
    {
        new PurchaseOrderItemDto
        {
            ItemCode = "RM-10234",
            Description = "Acero inoxidable grado 304 - lámina 4x8 calibre 18",
            Quantity = 20,
            UnitPrice = 185.00m,
            ExpectedDeliveryDate = poDate.AddDays(10)
        },
        new PurchaseOrderItemDto
        {
            ItemCode = "RM-10542",
            Description = "Perfil de aluminio estructural 40x40mm",
            Quantity = 50,
            UnitPrice = 78.50m,
            ExpectedDeliveryDate = poDate.AddDays(10)
        }
    };

            // Step 2: Record vendor invoice
            var invoiceDate = new DateOnly(2023, 7, 15);
            var vendorInvoiceDocument = new DocumentDto
            {
                DocumentDate = invoiceDate,
                DocumentNo = "CCF-5487",  // "Comprobante de Crédito Fiscal" - El Salvador tax document
                Description = $"Factura de {supplier.Name} - Ref. OC-2023-087",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "Registro de factura de proveedor",
                InternalComments = "Factura recibida en almacén central"
            };

            var createdVendorInvoice = await _documentService.CreateDocumentAsync(vendorInvoiceDocument, TEST_USER);
            _documents["ProcurementInvoice"] = createdVendorInvoice;

            // Calculate invoice totals
            decimal subtotal = poItems.Sum(item => item.Quantity * item.UnitPrice);
            decimal taxAmount = subtotal * 0.13m; // 13% VAT
            decimal totalAmount = subtotal + taxAmount;

            // Create transaction for vendor invoice
            var invoiceTransaction = new TransactionDto
            {
                DocumentId = createdVendorInvoice.Id,
                TransactionDate = invoiceDate,
                Description = "Registro de factura de proveedor para materias primas"
            };

            var createdInvoiceTransaction = await _transactionService.CreateTransactionAsync(invoiceTransaction);
            _transactions["ProcurementInvoice"] = createdInvoiceTransaction;

            // Create ledger entries for vendor invoice
            var invoiceLedgerEntries = new List<ILedgerEntry>
    {
        // Debit inventory
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdInvoiceTransaction.Id,
            AccountId = _accounts["Inventory - Raw Materials"].Id,
            EntryType = EntryType.Debit,
            Amount = subtotal
        },
        // Debit VAT
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdInvoiceTransaction.Id,
            AccountId = _accounts["VAT Payable"].Id,
            EntryType = EntryType.Debit,
            Amount = taxAmount
        },
        // Credit accounts payable
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdInvoiceTransaction.Id,
            AccountId = _accounts["Accounts Payable"].Id,
            EntryType = EntryType.Credit,
            Amount = totalAmount
        }
    };

            // Validate invoice transaction
            var isInvoiceValid = await _transactionService.ValidateTransactionAsync(
                createdInvoiceTransaction.Id, invoiceLedgerEntries);

            // Step 3: Record goods receipt
            var grDate = new DateOnly(2023, 7, 15);
            var grDocument = new DocumentDto
            {
                DocumentDate = grDate,
                DocumentNo = "GR-2023-087",
                Description = "Recepción de materias primas - CCF-5487",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "Material recibido completo según orden de compra",
                InternalComments = "Recibido e inspeccionado por J. Morales"
            };

            var createdGR = await _documentService.CreateDocumentAsync(grDocument, TEST_USER);
            _documents["ProcurementGR"] = createdGR;

            // Goods receipt would typically involve inventory movements but not financial transactions
            // In some ERP systems, this would trigger the vendor invoice transaction
            // In our case we've already created the invoice transaction

            // Step 4: Record payment to vendor
            var paymentDate = new DateOnly(2023, 8, 14); // 30 days after invoice
            var paymentDocument = new DocumentDto
            {
                DocumentDate = paymentDate,
                DocumentNo = "CH-11245",  // Represents check number
                Description = $"Pago a {supplier.Name} - CCF-5487",
                DocumentType = DocumentType.Miscellaneous,
                DocumentComments = "Pago de factura CCF-5487 por compra de materias primas",
                InternalComments = "Autorizado por Gerencia Financiera"
            };

            var createdPayment = await _documentService.CreateDocumentAsync(paymentDocument, TEST_USER);
            _documents["ProcurementPayment"] = createdPayment;

            // Create transaction for payment
            var paymentTransaction = new TransactionDto
            {
                DocumentId = createdPayment.Id,
                TransactionDate = paymentDate,
                Description = $"Pago a proveedor {supplier.Name} - CCF-5487"
            };

            var createdPaymentTransaction = await _transactionService.CreateTransactionAsync(paymentTransaction);
            _transactions["ProcurementPayment"] = createdPaymentTransaction;

            // Create ledger entries for payment
            var paymentLedgerEntries = new List<ILedgerEntry>
    {
        // Debit accounts payable (reducing liability)
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdPaymentTransaction.Id,
            AccountId = _accounts["Accounts Payable"].Id,
            EntryType = EntryType.Debit,
            Amount = totalAmount
        },
        // Credit cash (reducing asset)
        new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            TransactionId = createdPaymentTransaction.Id,
            AccountId = _accounts["Cash in Bank"].Id,
            EntryType = EntryType.Credit,
            Amount = totalAmount
        }
    };

            // Validate payment transaction
            var isPaymentValid = await _transactionService.ValidateTransactionAsync(
                createdPaymentTransaction.Id, paymentLedgerEntries);

            // Create procurement cycle metadata
            var procurementMetadata = new ProcurementCycleMetadata
            {
                SupplierInfo = supplier,
                PurchaseOrder = new DocumentReference
                {
                    DocumentId = createdPO.Id,
                    DocumentNumber = poDocument.DocumentNo,
                    DocumentDate = poDate
                },
                VendorInvoice = new DocumentReference
                {
                    DocumentId = createdVendorInvoice.Id,
                    DocumentNumber = vendorInvoiceDocument.DocumentNo,
                    DocumentDate = invoiceDate
                },
                GoodsReceipt = new DocumentReference
                {
                    DocumentId = createdGR.Id,
                    DocumentNumber = grDocument.DocumentNo,
                    DocumentDate = grDate
                },
                Payment = new DocumentReference
                {
                    DocumentId = createdPayment.Id,
                    DocumentNumber = paymentDocument.DocumentNo,
                    DocumentDate = paymentDate
                },
                TotalAmount = totalAmount,
                Currency = _companyCurrency,
                ItemsQuantity = poItems.Sum(item => item.Quantity)
            };

            // Assert
            Assert.That(createdPO.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdVendorInvoice.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdGR.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdPayment.Id, Is.Not.EqualTo(Guid.Empty));

            Assert.That(isInvoiceValid, Is.True, "Vendor invoice transaction should be balanced");
            Assert.That(isPaymentValid, Is.True, "Vendor payment transaction should be balanced");

            // Verify complete document flow
            Assert.That(_documents.Count(d => d.Key.StartsWith("Procurement")), Is.EqualTo(4),
                "Should have 4 documents in the procurement cycle");
            Assert.That(_transactions.Count(t => t.Key.StartsWith("Procurement")), Is.EqualTo(2),
                "Should have 2 financial transactions in the procurement cycle");
        }








        // Supporting classes for document metadata
        public class CustomerInfo
        {
            public string Name { get; set; }
            public string TaxId { get; set; }
            public string Address { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }

        public class SupplierInfo
        {
            public string Name { get; set; }
            public string TaxId { get; set; }
            public string Address { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string SupplierCode { get; set; }
        }

        public class InvoiceItemDto
        {
            public string ItemCode { get; set; }
            public string Description { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal DiscountPercentage { get; set; }
            public decimal TaxRate { get; set; }
        }

        public class PurchaseOrderItemDto
        {
            public string ItemCode { get; set; }
            public string Description { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public DateOnly ExpectedDeliveryDate { get; set; }
        }

        public class InvoiceReference
        {
            public string InvoiceNumber { get; set; }
            public DateOnly InvoiceDate { get; set; }
            public decimal OriginalAmount { get; set; }
            public decimal AmountPaid { get; set; }
            public decimal Balance { get; set; }
        }

        public class DocumentReference
        {
            public Guid DocumentId { get; set; }
            public string DocumentNumber { get; set; }
            public DateOnly DocumentDate { get; set; }
        }

        public class JournalEntryLineDto
        {
            public Guid AccountId { get; set; }
            public string AccountCode { get; set; }
            public string AccountName { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public string Description { get; set; }
        }

        // Metadata classes for document generation
        public class SalesInvoiceMetadata
        {
            public Guid DocumentId { get; set; }
            public string InvoiceNumber { get; set; }
            public DateOnly InvoiceDate { get; set; }
            public CustomerInfo CustomerInfo { get; set; }
            public List<InvoiceItemDto> Items { get; set; }
            public decimal Subtotal { get; set; }
            public decimal Discounts { get; set; }
            public decimal TaxableAmount { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal TotalAmount { get; set; }
            public string PaymentTerms { get; set; }
            public string Currency { get; set; }
            public string CompanyName { get; set; }
            public string CompanyAddress { get; set; }
            public string CompanyTaxId { get; set; }
        }

        public class PurchaseOrderMetadata
        {
            public Guid DocumentId { get; set; }
            public string PurchaseOrderNumber { get; set; }
            public DateOnly PODate { get; set; }
            public SupplierInfo SupplierInfo { get; set; }
            public List<PurchaseOrderItemDto> Items { get; set; }
            public decimal Subtotal { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal TotalAmount { get; set; }
            public string DeliveryInstructions { get; set; }
            public string PaymentTerms { get; set; }
            public string Currency { get; set; }
            public string CompanyName { get; set; }
            public string CompanyAddress { get; set; }
            public string CompanyTaxId { get; set; }
        }

        public class PaymentReceiptMetadata
        {
            public Guid DocumentId { get; set; }
            public string ReceiptNumber { get; set; }
            public DateOnly ReceiptDate { get; set; }
            public CustomerInfo CustomerInfo { get; set; }
            public List<InvoiceReference> InvoicesPaid { get; set; }
            public string PaymentMethod { get; set; }
            public string ReferenceNumber { get; set; }
            public decimal TotalAmount { get; set; }
            public string Currency { get; set; }
            public string CompanyName { get; set; }
            public string CompanyAddress { get; set; }
        }

        public class JournalEntryMetadata
        {
            public Guid DocumentId { get; set; }
            public string JournalNumber { get; set; }
            public DateOnly JournalDate { get; set; }
            public List<JournalEntryLineDto> Entries { get; set; }
            public decimal TotalDebits { get; set; }
            public decimal TotalCredits { get; set; }
            public string PreparedBy { get; set; }
            public string ApprovedBy { get; set; }
            public string CompanyName { get; set; }
            public string FiscalPeriod { get; set; }
        }

        public class ProcurementCycleMetadata
        {
            public SupplierInfo SupplierInfo { get; set; }
            public DocumentReference PurchaseOrder { get; set; }
            public DocumentReference VendorInvoice { get; set; }
            public DocumentReference GoodsReceipt { get; set; }
            public DocumentReference Payment { get; set; }
            public decimal TotalAmount { get; set; }
            public string Currency { get; set; }
            public decimal ItemsQuantity { get; set; }
        }

        #endregion


    }
}