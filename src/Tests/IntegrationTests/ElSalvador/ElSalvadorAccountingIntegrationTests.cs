using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Sivar.Erp;
using Sivar.Erp.Documents;
using Sivar.Erp.Documents.Tax;
using Sivar.Erp.Taxes.TaxGroup;
using Sivar.Erp.Taxes.TaxRule;

namespace Tests.IntegrationTests.ElSalvador
{
    /// <summary>
    /// Tests for El Salvador accounting integration with tax calculation
    /// </summary>
    [TestFixture]
    public class ElSalvadorAccountingIntegrationTests
    {
        private ITransactionService _transactionService;
        private ITaxAccountingProfileService _taxAccountingService;
        private Dictionary<string, Guid> _accountMappings;

        [SetUp]
        public void Setup()
        {
            // Setup test dependencies
            _transactionService = new TransactionService();
            _taxAccountingService = new TaxAccountingProfileService();
            
            // Create account mappings
            _accountMappings = new Dictionary<string, Guid>
            {
                ["AccountsReceivable"] = Guid.NewGuid(),
                ["SalesRevenue"] = Guid.NewGuid(),
                ["SalesTaxPayable"] = Guid.NewGuid(),
                ["InputTax"] = Guid.NewGuid(),
                ["AccountsPayable"] = Guid.NewGuid(),
                ["CostOfGoodsSold"] = Guid.NewGuid(),
                ["Inventory"] = Guid.NewGuid()
            };
            
            // Set up tax accounting profiles
            SetupTaxAccountingProfiles();
        }
        
        /// <summary>
        /// Sets up the tax accounting profiles for different document categories
        /// </summary>
        private void SetupTaxAccountingProfiles()
        {
            // For sales invoices (like Credito Fiscal), IVA is a credit to tax payable
            _taxAccountingService.RegisterTaxAccountingProfile(
                DocumentCategory.SalesInvoice, 
                "IVA", 
                new TaxAccountingInfo
                {
                    CreditAccountCode = "SalesTaxPayable", 
                    IncludeInTransaction = true,
                    AccountDescription = "IVA por Pagar"
                });
                
            // For sales credit notes, IVA is a debit to tax payable
            _taxAccountingService.RegisterTaxAccountingProfile(
                DocumentCategory.CreditNote, 
                "IVA", 
                new TaxAccountingInfo
                {
                    DebitAccountCode = "SalesTaxPayable", 
                    IncludeInTransaction = true,
                    AccountDescription = "IVA por Pagar (Reverso)"
                });
                
            // For purchase invoices, IVA is a debit to input tax
            _taxAccountingService.RegisterTaxAccountingProfile(
                DocumentCategory.PurchaseInvoice, 
                "IVA", 
                new TaxAccountingInfo
                {
                    DebitAccountCode = "InputTax", 
                    IncludeInTransaction = true,
                    AccountDescription = "IVA Crédito Fiscal"
                });
        }
        
        [Test]
        public async Task CreditoFiscal_WithTaxAccounting_GeneratesCorrectEntries()
        {
            // Arrange
            // 1. Create document type with category
            var creditoFiscalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                Category = DocumentCategory.SalesInvoice,
                IsEnabled = true
            };
            
            // 2. Create business entity
            var business = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Name = "Empresa ABC S.A. de C.V.",
                Code = "ABC-001"
            };
            
            // 3. Create tax group for business
            var taxableGroupId = Guid.NewGuid();
            
            // 4. Create IVA tax
            var ivaTax = new TaxDto
            {
                Oid = Guid.NewGuid(),
                Name = "IVA",
                Code = "IVA",
                TaxType = TaxType.Percentage,
                ApplicationLevel = TaxApplicationLevel.Line,
                Percentage = 13m,
                IsEnabled = true
            };
            
            // 5. Create tax rules
            var taxRules = new List<TaxRuleDto>
            {
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Oid,
                    DocumentTypeCode = creditoFiscalDocType.Code,
                    BusinessEntityGroupId = taxableGroupId,
                    IsEnabled = true,
                    Priority = 1
                }
            };
            
            // 6. Create group memberships
            var groupMemberships = new List<GroupMembershipDto>
            {
                new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = taxableGroupId,
                    EntityId = business.Oid,
                    GroupType = GroupType.BusinessEntity
                }
            };
            
            // 7. Create tax rule evaluator
            var taxRuleEvaluator = new TaxRuleEvaluator(
                taxRules,
                new List<TaxDto> { ivaTax },
                groupMemberships
            );
            
            // 8. Create document
            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                DocumentNumber = "CF-001",
                Date = DateOnly.FromDateTime(DateTime.Today),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = business,
                DocumentType = creditoFiscalDocType
            };
            
            // 9. Create product
            var product = new ItemDto
            {
                Oid = Guid.NewGuid(),
                Code = "PROD-001",
                Description = "Product XYZ",
                BasePrice = 100m
            };
            
            // 10. Add line to document
            var line = new LineDto
            {
                Item = product,
                Quantity = 2,
                UnitPrice = 100m
                // Amount = 200 will be calculated
            };
            
            document.Lines.Add(line);
            
            // Act
            // 1. Calculate taxes with accounting information
            var taxCalculator = document.CreateTaxCalculatorWithAccounting(
                creditoFiscalDocType.Code,
                taxRuleEvaluator,
                _taxAccountingService
            );
            
            // 2. Calculate line taxes
            taxCalculator.CalculateLineTaxes(line);
            
            // 3. Calculate document taxes
            taxCalculator.CalculateDocumentTaxes();
            
            // 4. Create simple transaction generator and generate transaction
            var generator = new SimpleTransactionGenerator(_transactionService, _accountMappings);
            var (transaction, ledgerEntries) = await document.GenerateSimpleTransactionAsync(generator);
            
            // Assert
            // 1. Verify document has IVA tax with accounting information
            var ivaTaxTotal = document.DocumentTotals.FirstOrDefault(t => t.Concept.Contains("IVA"));
            Assert.That(ivaTaxTotal, Is.Not.Null);
            Assert.That(ivaTaxTotal.Total, Is.EqualTo(26m)); // 13% of 200
            Assert.That((ivaTaxTotal as TotalDto).CreditAccountCode, Is.EqualTo("SalesTaxPayable"));
            Assert.That((ivaTaxTotal as TotalDto).IncludeInTransaction, Is.True);
            
            // 2. Verify transaction has correct entries
            Assert.That(transaction, Is.Not.Null);
            Assert.That(transaction.DocumentId, Is.EqualTo(document.Oid));
            
            // 3. Verify ledger entries include IVA tax entry
            var taxEntry = ledgerEntries.FirstOrDefault(e => 
                e.EntryType == EntryType.Credit && 
                e.AccountId == _accountMappings["SalesTaxPayable"]);
                
            Assert.That(taxEntry, Is.Not.Null);
            Assert.That(taxEntry.Amount, Is.EqualTo(26m));
        }
        
        [Test]
        public async Task CompareTraditionalAndAccountingIntegratedApproaches()
        {
            // Arrange
            // 1. Set up document and tax calculation the same way as before
            var docType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                Category = DocumentCategory.SalesInvoice,
                IsEnabled = true
            };
            
            var business = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Name = "Empresa ABC S.A. de C.V.",
                Code = "ABC-001"
            };
            
            var taxableGroupId = Guid.NewGuid();
            
            var ivaTax = new TaxDto
            {
                Oid = Guid.NewGuid(),
                Name = "IVA",
                Code = "IVA",
                TaxType = TaxType.Percentage,
                ApplicationLevel = TaxApplicationLevel.Line,
                Percentage = 13m,
                IsEnabled = true
            };
            
            var taxRules = new List<TaxRuleDto>
            {
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Oid,
                    DocumentTypeCode = docType.Code,
                    BusinessEntityGroupId = taxableGroupId,
                    IsEnabled = true,
                    Priority = 1
                }
            };
            
            var groupMemberships = new List<GroupMembershipDto>
            {
                new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = taxableGroupId,
                    EntityId = business.Oid,
                    GroupType = GroupType.BusinessEntity
                }
            };
            
            var taxRuleEvaluator = new TaxRuleEvaluator(taxRules, new List<TaxDto> { ivaTax }, groupMemberships);
            
            // Create identical documents for both approaches
            var traditionalDoc = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                DocumentNumber = "CF-001-T",
                Date = DateOnly.FromDateTime(DateTime.Today),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = business,
                DocumentType = docType
            };
            
            var integratedDoc = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                DocumentNumber = "CF-001-I",
                Date = DateOnly.FromDateTime(DateTime.Today),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = business,
                DocumentType = docType
            };
            
            var product = new ItemDto
            {
                Oid = Guid.NewGuid(),
                Code = "PROD-001",
                Description = "Product XYZ",
                BasePrice = 100m
            };
            
            // Add identical lines to both documents
            var traditionalLine = new LineDto { Item = product, Quantity = 2, UnitPrice = 100m };
            var integratedLine = new LineDto { Item = product, Quantity = 2, UnitPrice = 100m };
            
            traditionalDoc.Lines.Add(traditionalLine);
            integratedDoc.Lines.Add(integratedLine);
            
            // Act
            // 1. Traditional approach: Calculate taxes, then use template
            var traditionalCalc = new DocumentTaxCalculator(traditionalDoc, docType.Code, taxRuleEvaluator);
            traditionalCalc.CalculateLineTaxes(traditionalLine);
            traditionalCalc.CalculateDocumentTaxes();
            
            // Create traditional transaction template
            var template = new TransactionTemplate(
                docType.Code,
                document => $"Credito Fiscal - {document.BusinessEntity?.Name ?? "Unknown"} - {document.Date}"
            ).WithEntries(
                // Debit Accounts Receivable for the grand total
                AccountingTransactionEntry.Debit("AccountsReceivable", AmountCalculators.GrandTotal)
                    .WithAccountName("Cuentas por Cobrar"),
                // Credit Sales Revenue for the subtotal
                AccountingTransactionEntry.Credit("SalesRevenue", AmountCalculators.Subtotal)
                    .WithAccountName("Ingresos por Ventas"),
                // Credit IVA Payable for the tax amount
                AccountingTransactionEntry.Credit("SalesTaxPayable", AmountCalculators.TaxTotal)
                    .WithAccountName("IVA por Pagar")
            );
            
            var traditionalGenerator = new DocumentTransactionGenerator(_transactionService, _accountMappings);
            traditionalGenerator.RegisterTemplate(template);
            var (traditionalTransaction, traditionalEntries) = await traditionalDoc.GenerateTransactionAsync(traditionalGenerator);
            
            // 2. Integrated approach: Calculate taxes with accounting info
            var integratedCalc = integratedDoc.CreateTaxCalculatorWithAccounting(
                docType.Code, taxRuleEvaluator, _taxAccountingService);
            integratedCalc.CalculateLineTaxes(integratedLine);
            integratedCalc.CalculateDocumentTaxes();
            
            // Create integrated transaction generator
            var integratedGenerator = new SimpleTransactionGenerator(_transactionService, _accountMappings);
            
            // Add additional account mappings for the integrated approach
            integratedGenerator.AddAccountMappings(new Dictionary<string, Guid>
            {
                ["SalesRevenue"] = _accountMappings["SalesRevenue"],
                ["AccountsReceivable"] = _accountMappings["AccountsReceivable"]
            });
            
            // Manually set accounting codes for non-tax totals (in a real app, these would be set via a similar service)
            var subtotalTotal = integratedDoc.DocumentTotals.FirstOrDefault(t => t.Concept == "Subtotal");
            if (subtotalTotal is TotalDto subtotalDto)
            {
                subtotalDto.CreditAccountCode = "SalesRevenue";
                subtotalDto.IncludeInTransaction = true;
            }
            
            var grandTotal = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = "Grand Total",
                Total = traditionalLine.Amount + traditionalDoc.DocumentTotals.Sum(t => t.Total),
                DebitAccountCode = "AccountsReceivable",
                IncludeInTransaction = true
            };
            
            integratedDoc.DocumentTotals.Add(grandTotal);
            
            var (integratedTransaction, integratedEntries) = await integratedDoc.GenerateSimpleTransactionAsync(integratedGenerator);
            
            // Assert
            // 1. Both approaches should create similar tax entries
            var traditionalTaxEntry = traditionalEntries.FirstOrDefault(e => 
                e.EntryType == EntryType.Credit && e.AccountId == _accountMappings["SalesTaxPayable"]);
                
            var integratedTaxEntry = integratedEntries.FirstOrDefault(e => 
                e.EntryType == EntryType.Credit && e.AccountId == _accountMappings["SalesTaxPayable"]);
            
            Assert.That(traditionalTaxEntry, Is.Not.Null);
            Assert.That(integratedTaxEntry, Is.Not.Null);
            Assert.That(traditionalTaxEntry.Amount, Is.EqualTo(integratedTaxEntry.Amount));
            
            // 2. Print both transactions for comparison
            Console.WriteLine("Traditional Transaction Entries:");
            foreach (var entry in traditionalEntries)
            {
                Console.WriteLine($"  {entry.EntryType} {entry.AccountName}: {entry.Amount}");
            }
            
            Console.WriteLine("Integrated Transaction Entries:");
            foreach (var entry in integratedEntries)
            {
                Console.WriteLine($"  {entry.EntryType} {entry.AccountName}: {entry.Amount}");
            }
        }
    }
}