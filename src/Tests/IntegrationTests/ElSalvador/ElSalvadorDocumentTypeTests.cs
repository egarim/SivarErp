using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sivar.Erp;
using Sivar.Erp.Documents;
using Sivar.Erp.Documents.Tax;
using Sivar.Erp.Taxes.TaxRule;
using Sivar.Erp.Taxes.TaxGroup;
using System.Diagnostics;
using Sivar.Erp.Taxes;

namespace Tests.IntegrationTests.ElSalvador
{
    /// <summary>
    /// Tests for El Salvador specific document types
    /// </summary>
    [TestFixture]
    public class ElSalvadorDocumentTypeTests
    {
    
        private ITransactionService _transactionService;

        [SetUp]
        public void Setup()
        {
            // Setup test dependencies
          
            _transactionService = new TransactionService();
        }

        [Test]
        public async Task CreateElSalvadorDocumentTypes_ValidateProperties()
        {
            // Arrange
            // 1. Create El Salvador document types
            var creditoFiscalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true
            };

            var consumidorFinalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CNF",
                Name = "Consumidor Final",
                IsEnabled = true
            };

            // 2. Create account mappings for transaction generation
            var accountMappings = new Dictionary<string, Guid>
            {
                ["AccountsReceivable"] = Guid.NewGuid(),
                ["SalesRevenue"] = Guid.NewGuid(),
                ["SalesTaxPayable"] = Guid.NewGuid(),
                ["CostOfGoodsSold"] = Guid.NewGuid(),
                ["Inventory"] = Guid.NewGuid(),
                ["Cash"] = Guid.NewGuid()
            };

            // 3. Create transaction generator
            var generator = new DocumentTransactionGenerator(_transactionService, accountMappings);

            // 4. Create transaction templates for El Salvador document types
            // For "Credito Fiscal", which requires IVA tax handling
            var cfTemplate = new TransactionTemplate(
                creditoFiscalDocType.Code,
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
                    .WithAccountName("IVA por Pagar"),

                // Optional: Cost of Goods Sold entries
                AccountingTransactionEntry.Debit("CostOfGoodsSold", AmountCalculators.EstimatedCostOfGoodsSold(60))
                    .WithAccountName("Costo de Ventas"),

                AccountingTransactionEntry.Credit("Inventory", AmountCalculators.EstimatedCostOfGoodsSold(60))
                    .WithAccountName("Inventario")
            );

            // For "Consumidor Final", similar but might have different tax handling
            var cnfTemplate = new TransactionTemplate(
                consumidorFinalDocType.Code,
                document => $"Consumidor Final - {document.BusinessEntity?.Name ?? "Unknown"} - {document.Date}"
            ).WithEntries(
                // For cash sales to final consumers
                AccountingTransactionEntry.Debit("Cash", AmountCalculators.GrandTotal)
                    .WithAccountName("Efectivo"),

                // Credit Sales Revenue
                AccountingTransactionEntry.Credit("SalesRevenue", AmountCalculators.Subtotal)
                    .WithAccountName("Ingresos por Ventas"),

                // Credit IVA Payable for any applicable tax
                AccountingTransactionEntry.Credit("SalesTaxPayable", AmountCalculators.TaxTotal)
                    .WithAccountName("IVA por Pagar"),

                // Optional Cost of Goods Sold entries
                AccountingTransactionEntry.Debit("CostOfGoodsSold", AmountCalculators.EstimatedCostOfGoodsSold(60))
                    .WithAccountName("Costo de Ventas"),

                AccountingTransactionEntry.Credit("Inventory", AmountCalculators.EstimatedCostOfGoodsSold(60))
                    .WithAccountName("Inventario")
            );

            // 5. Register templates with the generator
            generator.RegisterTemplate(cfTemplate);
            generator.RegisterTemplate(cnfTemplate);

            // 6. Create sample documents
            var business = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Name = "Empresa ABC S.A. de C.V.",
                Code = "ABC-001"
            };

            // Credito Fiscal document
            var cfDocument = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = business,
                DocumentType = creditoFiscalDocType
            };

            // Add document totals
            cfDocument.DocumentTotals.Add(new TotalDto { Concept = "Subtotal", Total = 1000.00m });
            cfDocument.DocumentTotals.Add(new TotalDto { Concept = "Tax: IVA (13%)", Total = 130.00m });

            // Consumidor Final document
            var cnfDocument = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = new BusinessEntityDto { Name = "Cliente Final", Code = "CF-001" },
                DocumentType = consumidorFinalDocType
            };

            // Add document totals (Consumidor Final might have simplified tax handling)
            cnfDocument.DocumentTotals.Add(new TotalDto { Concept = "Subtotal", Total = 500.00m });
            cnfDocument.DocumentTotals.Add(new TotalDto { Concept = "Tax: IVA (13%)", Total = 65.00m });

            // Act - Generate transactions for both documents
            var (cfTransaction, cfLedgerEntries) = await cfDocument.GenerateTransactionAsync(generator);
            var (cnfTransaction, cnfLedgerEntries) = await cnfDocument.GenerateTransactionAsync(generator);

            // Assert
            // Validate Credito Fiscal document type and transaction
            Assert.That(creditoFiscalDocType.Oid, Is.Not.EqualTo(Guid.Empty));
            Assert.That(creditoFiscalDocType.Code, Is.EqualTo("CF"));
            Assert.That(creditoFiscalDocType.Name, Is.EqualTo("Credito Fiscal"));
            Assert.That(creditoFiscalDocType.IsEnabled, Is.True);

            // Validate Consumidor Final document type and transaction 
            Assert.That(consumidorFinalDocType.Oid, Is.Not.EqualTo(Guid.Empty));
            Assert.That(consumidorFinalDocType.Code, Is.EqualTo("CNF"));
            Assert.That(consumidorFinalDocType.Name, Is.EqualTo("Consumidor Final"));
            Assert.That(consumidorFinalDocType.IsEnabled, Is.True);

            // Validate CF transaction details
            Assert.That(cfTransaction, Is.Not.Null);
            Assert.That(cfTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(cfTransaction.DocumentId, Is.EqualTo(cfDocument.Oid));
            Assert.That(cfTransaction.Description, Does.Contain("Credito Fiscal"));
            Assert.That(cfTransaction.Description, Does.Contain(business.Name));

            // Check that correct ledger entries were created for CF document
            Assert.That(cfLedgerEntries.Count, Is.EqualTo(5), "Should have 5 ledger entries for Credito Fiscal");

            // Verify specific entries for CF document
            var cfReceivableEntry = cfLedgerEntries.FirstOrDefault(e => e.AccountName == "Cuentas por Cobrar" && e.EntryType == EntryType.Debit);
            var cfRevenueEntry = cfLedgerEntries.FirstOrDefault(e => e.AccountName == "Ingresos por Ventas" && e.EntryType == EntryType.Credit);
            var cfTaxEntry = cfLedgerEntries.FirstOrDefault(e => e.AccountName == "IVA por Pagar" && e.EntryType == EntryType.Credit);

            Assert.That(cfReceivableEntry, Is.Not.Null, "Should have an accounts receivable entry");
            Assert.That(cfRevenueEntry, Is.Not.Null, "Should have a sales revenue entry");
            Assert.That(cfTaxEntry, Is.Not.Null, "Should have an IVA payable entry");

            Assert.That(cfReceivableEntry.Amount, Is.EqualTo(1130.00m), "Accounts receivable should equal grand total (1000 + 130)");
            Assert.That(cfRevenueEntry.Amount, Is.EqualTo(1000.00m), "Revenue should equal subtotal");
            Assert.That(cfTaxEntry.Amount, Is.EqualTo(130.00m), "IVA payable should equal tax total");

            // Validate CNF transaction details
            Assert.That(cnfTransaction, Is.Not.Null);
            Assert.That(cnfTransaction.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(cnfTransaction.DocumentId, Is.EqualTo(cnfDocument.Oid));
            Assert.That(cnfTransaction.Description, Does.Contain("Consumidor Final"));

            // Check that correct ledger entries were created for CNF document
            Assert.That(cnfLedgerEntries.Count, Is.EqualTo(5), "Should have 5 ledger entries for Consumidor Final");

            // Verify specific entries for CNF document
            var cnfCashEntry = cnfLedgerEntries.FirstOrDefault(e => e.AccountName == "Efectivo" && e.EntryType == EntryType.Debit);
            var cnfRevenueEntry = cnfLedgerEntries.FirstOrDefault(e => e.AccountName == "Ingresos por Ventas" && e.EntryType == EntryType.Credit);
            var cnfTaxEntry = cnfLedgerEntries.FirstOrDefault(e => e.AccountName == "IVA por Pagar" && e.EntryType == EntryType.Credit);

            Assert.That(cnfCashEntry, Is.Not.Null, "Should have a cash entry");
            Assert.That(cnfRevenueEntry, Is.Not.Null, "Should have a sales revenue entry");
            Assert.That(cnfTaxEntry, Is.Not.Null, "Should have an IVA payable entry");

            Assert.That(cnfCashEntry.Amount, Is.EqualTo(565.00m), "Cash should equal grand total (500 + 65)");
            Assert.That(cnfRevenueEntry.Amount, Is.EqualTo(500.00m), "Revenue should equal subtotal");
            Assert.That(cnfTaxEntry.Amount, Is.EqualTo(65.00m), "IVA payable should equal tax total");

            // Print details for debugging
            PrintTransactionToConsole("CREDITO FISCAL", cfDocument, cfTransaction, cfLedgerEntries);
            PrintTransactionToConsole("CONSUMIDOR FINAL", cnfDocument, cnfTransaction, cnfLedgerEntries);
        }

        [Test]
        public void ElSalvador_DocumentTypes_AreUnique()
        {
            // Arrange
            var documentTypes = new List<IDocumentType>
            {
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "CF",
                    Name = "Credito Fiscal",
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "CNF",
                    Name = "Consumidor Final",
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "EXP",
                    Name = "Factura de Exportación",
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "FACT",
                    Name = "Factura Comercial",
                    IsEnabled = true
                }
            };

            // Act
            var distinctCodes = documentTypes.Select(dt => dt.Code).Distinct().Count();
            var distinctNames = documentTypes.Select(dt => dt.Name).Distinct().Count();

            // Assert
            Assert.That(distinctCodes, Is.EqualTo(documentTypes.Count), "All document type codes should be unique");
            Assert.That(distinctNames, Is.EqualTo(documentTypes.Count), "All document type names should be unique");
        }

   

        [Test]
        public void ElSalvador_DocumentType_RaisesPropertyChangedEvent()
        {
            // Arrange
            var documentType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true
            };

            string propertyNameRaised = null;
            documentType.PropertyChanged += (sender, args) => {
                propertyNameRaised = args.PropertyName;
            };

            // Act
            documentType.Name = "Crédito Fiscal (Changed)";

            // Assert
            Assert.That(propertyNameRaised, Is.EqualTo("Name"));
        }

    

        [Test]
        public void ElSalvador_CalculateIVA_ForCreditoFiscal_And_ConsumidorFinal()
        {
            // Arrange
            // 1. Create document types for El Salvador
            var creditoFiscalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true
            };

            var consumidorFinalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CNF",
                Name = "Consumidor Final",
                IsEnabled = true
            };

            // 2. Create business entities - one taxable (for Credito Fiscal) and one non-taxable (for Consumidor Final)
            var taxableBusiness = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Name = "Empresa ABC S.A. de C.V.",
                Code = "ABC-001"
            };

            var nonTaxableBusiness = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Name = "Cliente Final",
                Code = "CF-001"
            };

            // 3. Create groups for businesses
            var taxableGroupId = Guid.NewGuid();
            var nonTaxableGroupId = Guid.NewGuid();

            // 4. Create a product (item) for the invoice lines
            var product = new ItemDto
            {
                Oid = Guid.NewGuid(),
                Code = "PROD-001",
                Description = "Product XYZ",
                BasePrice = 100m
            };

            // 5. Create IVA tax (13%)
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

            // 6. Create tax rules
            var taxRules = new List<TaxRuleDto>
            {
                // Rule for applying IVA to Credito Fiscal documents
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Oid,
                    DocumentTypeCode = creditoFiscalDocType.Code, // "CF"
                    BusinessEntityGroupId = taxableGroupId,
                    IsEnabled = true,
                    Priority = 1
                }
                // No rule for Consumidor Final means no tax applies
            };

            // 7. Setup group memberships
            var groupMemberships = new List<GroupMembershipDto>
            {
                // Add taxable business to taxable group
                new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = taxableGroupId,
                    EntityId = taxableBusiness.Oid,
                    GroupType = GroupType.BusinessEntity
                }
                // Non-taxable business is not added to any tax group
            };

            // 8. Create the TaxRuleEvaluator
            var taxRuleEvaluator = new TaxRuleEvaluator(
                taxRules, 
                new List<TaxDto> { ivaTax }, 
                groupMemberships);

            // 9. Create documents
            var creditoFiscalDoc = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                Date = new DateOnly(2025, 6, 12),
                Time = new TimeOnly(10, 0, 0),
                BusinessEntity = taxableBusiness,
                DocumentType = creditoFiscalDocType
            };

            var consumidorFinalDoc = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                Date = new DateOnly(2025, 6, 12),
                Time = new TimeOnly(10, 0, 0),
                BusinessEntity = nonTaxableBusiness,
                DocumentType = consumidorFinalDocType
            };

            // 10. Create lines with quantity and unit price for both documents
            var cfLine = new LineDto
            {
                Item = product,
                Quantity = 2,
                UnitPrice = 100m
                // Amount = 200 will be calculated
            };
            
            var cnfLine = new LineDto
            {
                Item = product,
                Quantity = 2,
                UnitPrice = 100m
                // Amount = 200 will be calculated
            };
            
            creditoFiscalDoc.Lines.Add(cfLine);
            consumidorFinalDoc.Lines.Add(cnfLine);

            // Act
            // Calculate taxes using the DocumentTaxCalculator for both documents
            var cfDocTaxCalculator = new DocumentTaxCalculator(
                creditoFiscalDoc, 
                creditoFiscalDocType.Code, 
                taxRuleEvaluator);
            
            var cnfDocTaxCalculator = new DocumentTaxCalculator(
                consumidorFinalDoc, 
                consumidorFinalDocType.Code, 
                taxRuleEvaluator);

            // Calculate line-level taxes (should apply IVA to CF document line only)
            cfDocTaxCalculator.CalculateLineTaxes(cfLine as LineDto);
            cnfDocTaxCalculator.CalculateLineTaxes(cnfLine as LineDto);
            
            // Now calculate document totals, which should include the line tax totals
            cfDocTaxCalculator.CalculateDocumentTaxes();
            cnfDocTaxCalculator.CalculateDocumentTaxes();

            // Assert
            // Check that Credito Fiscal line has correct IVA tax
            var ivaLineTotal = cfLine.LineTotals
                .FirstOrDefault(t => t.Concept.Contains("IVA"));
            
            Assert.That(ivaLineTotal, Is.Not.Null, "IVA tax total should be present for Credito Fiscal line");
            Assert.That(cfLine.Amount, Is.EqualTo(200m), "Line amount should be 200");
            Assert.That(ivaLineTotal.Total, Is.EqualTo(26m), "IVA (13%) of 200 should be 26");

            // Check that the document totals include the line tax totals
            var ivaDocumentTotal = creditoFiscalDoc.DocumentTotals
                .FirstOrDefault(t => t.Concept.Contains("IVA"));
                
            Assert.That(ivaDocumentTotal, Is.Not.Null, "IVA tax total should be present in document totals");
            Assert.That(ivaDocumentTotal.Total, Is.EqualTo(26m), "Document tax total should match the line tax total");
            
            // Calculate the grand total
            var subtotal = cfLine.Amount;
            var expectedGrandTotal = subtotal + creditoFiscalDoc.DocumentTotals.Sum(t => t.Total);
            
            Assert.That(expectedGrandTotal, Is.EqualTo(226m), "Grand total should be 226 (200 + 26)");

            // Check that Consumidor Final line has no tax applied
            var cnfTaxTotal = cnfLine.LineTotals
                .FirstOrDefault(t => t.Concept.Contains("IVA"));
            
            Assert.That(cnfTaxTotal, Is.Null, "Consumidor Final should not have IVA tax applied");
            Assert.That(cnfLine.Amount, Is.EqualTo(200m), "Line amount should be 200");
            Assert.That(cnfLine.LineTotals.Count, Is.EqualTo(0), "Consumidor Final should have no line totals");
            
            // Verify Consumidor Final document has no tax totals
            var cnfDocTaxTotal = consumidorFinalDoc.DocumentTotals
                .FirstOrDefault(t => t.Concept.Contains("IVA"));
                
            Assert.That(cnfDocTaxTotal, Is.Null, "Consumidor Final document should not have IVA tax total");
            
            // Print the document for debugging
            PrintDocumentToConsole(creditoFiscalDoc);
        }
        
        [Test]
        public void ElSalvador_PrintCreditoFiscalDocument_ToConsole()
        {
            // This test creates a full Credito Fiscal document and prints its details to the debug console
            
            // Create document type
            var creditoFiscalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true
            };
            
            // Create business entity
            var business = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Name = "Empresa ABC S.A. de C.V.",
                Code = "ABC-001"
            };
            
            // Create product
            var product1 = new ItemDto
            {
                Oid = Guid.NewGuid(),
                Code = "PROD-001",
                Description = "Product XYZ",
                BasePrice = 100m
            };
            
            var product2 = new ItemDto
            {
                Oid = Guid.NewGuid(),
                Code = "PROD-002",
                Description = "Service ABC",
                BasePrice = 75.50m
            };
            
            // Create IVA tax
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
            
            // Create tax group for business
            var taxableGroupId = Guid.NewGuid();
            
            // Create tax rules
            var taxRules = new List<TaxRuleDto>
            {
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Oid,
                    DocumentTypeCode = "CF",
                    BusinessEntityGroupId = taxableGroupId,
                    IsEnabled = true,
                    Priority = 1
                }
            };
            
            // Group memberships
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
            
            // Tax rule evaluator
            var taxRuleEvaluator = new TaxRuleEvaluator(
                taxRules, 
                new List<TaxDto> { ivaTax }, 
                groupMemberships);
            
            // Create document
            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Now),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = business,
                DocumentType = creditoFiscalDocType
            };
            
            // Add lines
            var line1 = new LineDto
            {
                Item = product1,
                Quantity = 3,
                UnitPrice = 100m
                // Amount = 300 will be calculated
            };
            
            var line2 = new LineDto
            {
                Item = product2,
                Quantity = 2,
                UnitPrice = 75.50m
                // Amount = 151 will be calculated
            };
            
            document.Lines.Add(line1);
            document.Lines.Add(line2);
            
            // Add some custom line totals to demonstrate the distinct totals calculation
            var discountTotal1 = new TotalDto 
            {
                Oid = Guid.NewGuid(),
                Concept = "Discount",
                Total = -15.00m
            };
            
            var discountTotal2 = new TotalDto 
            {
                Oid = Guid.NewGuid(),
                Concept = "Discount",
                Total = -7.50m
            };
            
            var handlingFee1 = new TotalDto 
            {
                Oid = Guid.NewGuid(),
                Concept = "Handling Fee",
                Total = 5.00m
            };
            
            var handlingFee2 = new TotalDto 
            {
                Oid = Guid.NewGuid(),
                Concept = "Handling Fee",
                Total = 3.50m
            };
            
            // Add custom totals to lines
            line1.LineTotals.Add(discountTotal1);
            line1.LineTotals.Add(handlingFee1);
            line2.LineTotals.Add(discountTotal2);
            line2.LineTotals.Add(handlingFee2);
            
            // Create tax calculator and process document
            var taxCalculator = new DocumentTaxCalculator(
                document, 
                creditoFiscalDocType.Code, 
                taxRuleEvaluator);
            
            // Calculate line taxes
            foreach (var line in document.Lines.OfType<LineDto>())
            {
                taxCalculator.CalculateLineTaxes(line);
            }
            
            // Calculate document level taxes, which now also includes the distinct totals from lines
            taxCalculator.CalculateDocumentTaxes();
            
            // Calculate expected document totals for verification
            decimal subtotal = document.Lines.Sum(l => l.Amount); // 300 + 151 = 451
            decimal expectedTotalDiscounts = discountTotal1.Total + discountTotal2.Total; // -15 + -7.5 = -22.5
            decimal expectedTotalHandlingFees = handlingFee1.Total + handlingFee2.Total; // 5 + 3.5 = 8.5
            decimal expectedIvaTaxLine1 = line1.Amount * 0.13m; // 300 * 0.13 = 39
            decimal expectedIvaTaxLine2 = line2.Amount * 0.13m; // 151 * 0.13 = 19.63
            decimal expectedTotalIva = expectedIvaTaxLine1 + expectedIvaTaxLine2; // 39 + 19.63 = 58.63
            
            // Print to console for visual inspection
            PrintDocumentToConsole(document);
            
            // Assert specific document totals
            var documentDiscountTotal = document.DocumentTotals.FirstOrDefault(t => t.Concept == "Discount");
            var documentHandlingFeeTotal = document.DocumentTotals.FirstOrDefault(t => t.Concept == "Handling Fee");
            var documentIvaTaxTotal = document.DocumentTotals.FirstOrDefault(t => t.Concept.Contains("IVA"));
            
            // Verify that the distinct totals have been summed and added to document totals
            Assert.That(documentDiscountTotal, Is.Not.Null, "Document should have a discount total");
            Assert.That(documentHandlingFeeTotal, Is.Not.Null, "Document should have a handling fee total");
            Assert.That(documentIvaTaxTotal, Is.Not.Null, "Document should have an IVA tax total");
            
            // Verify the correct values
            Assert.That(documentDiscountTotal.Total, Is.EqualTo(expectedTotalDiscounts), "Document discount should be sum of line discounts");
            Assert.That(documentHandlingFeeTotal.Total, Is.EqualTo(expectedTotalHandlingFees), "Document handling fee should be sum of line handling fees");
            Assert.That(Math.Round(documentIvaTaxTotal.Total, 2), Is.EqualTo(Math.Round(expectedTotalIva, 2)), "Document IVA tax should be sum of line IVA taxes");
            
            // Verify IVA tax totals on each line
            var ivaTaxLine1 = line1.LineTotals.FirstOrDefault(t => t.Concept.Contains("IVA"));
            var ivaTaxLine2 = line2.LineTotals.FirstOrDefault(t => t.Concept.Contains("IVA"));
            
            Assert.That(ivaTaxLine1, Is.Not.Null, "Line 1 should have IVA tax");
            Assert.That(ivaTaxLine2, Is.Not.Null, "Line 2 should have IVA tax");
            Assert.That(Math.Round(ivaTaxLine1.Total, 2), Is.EqualTo(Math.Round(expectedIvaTaxLine1, 2)), "Line 1 IVA tax should be 13% of line amount");
            Assert.That(Math.Round(ivaTaxLine2.Total, 2), Is.EqualTo(Math.Round(expectedIvaTaxLine2, 2)), "Line 2 IVA tax should be 13% of line amount");
            
            // Calculate and verify the grand total
            decimal expectedGrandTotal = subtotal + document.DocumentTotals.Sum(t => t.Total);
            decimal actualGrandTotal = subtotal + document.DocumentTotals.Sum(t => t.Total);
            
            Assert.That(Math.Round(actualGrandTotal, 2), Is.EqualTo(Math.Round(expectedGrandTotal, 2)), "Grand total should include subtotal plus all document totals");
            
            // Verify specific values for simple test case
            var expectedSimpleGrandTotal = subtotal + expectedTotalDiscounts + expectedTotalHandlingFees + expectedTotalIva;
            Assert.That(Math.Round(actualGrandTotal, 2), Is.EqualTo(Math.Round(expectedSimpleGrandTotal, 2)), 
                "Grand total should be: subtotal + discounts + handling fees + IVA taxes");
        }

        /// <summary>
        /// Formats and prints a document to the debug console
        /// </summary>
        private void PrintDocumentToConsole(DocumentDto document)
        {
            var sb = new StringBuilder();
            
            // Print header information with clear separation
            sb.AppendLine("====================================================");
            sb.AppendLine("                DOCUMENT DETAILS                    ");
            sb.AppendLine("====================================================");
            
            // Basic document info
            sb.AppendLine($"Document ID: {document.Oid}");
            sb.AppendLine($"Date: {document.Date}");
            sb.AppendLine($"Time: {document.Time}");
            
            // Document Type
            if (document.DocumentType != null)
            {
                sb.AppendLine("\n-- DOCUMENT TYPE --");
                sb.AppendLine($"Code: {document.DocumentType.Code}");
                sb.AppendLine($"Name: {document.DocumentType.Name}");
                sb.AppendLine($"Enabled: {document.DocumentType.IsEnabled}");
            }
            
            // Business Entity
            if (document.BusinessEntity != null)
            {
                sb.AppendLine("\n-- BUSINESS ENTITY --");
                sb.AppendLine($"Code: {document.BusinessEntity.Code}");
                sb.AppendLine($"Name: {document.BusinessEntity.Name}");
            }
            
            // Document Lines
            sb.AppendLine("\n-- DOCUMENT LINES --");
            int lineNumber = 1;
            decimal subtotal = 0;
            
            foreach (var line in document.Lines)
            {
                sb.AppendLine($"\nLINE {lineNumber}:");
                
                if (line.Item != null)
                {
                    sb.AppendLine($"  Item: {line.Item.Code} - {line.Item.Description}");
                    sb.AppendLine($"  Base Price: {line.Item.BasePrice:C2}");
                }
                
                // Line details - cast to LineDto if needed to access specific properties
                if (line is LineDto lineDto)
                {
                    sb.AppendLine($"  Quantity: {lineDto.Quantity}");
                    sb.AppendLine($"  Unit Price: {lineDto.UnitPrice:C2}");
                }
                
                sb.AppendLine($"  Amount: {line.Amount:C2}");
                
                subtotal += line.Amount;
                
                // Line Totals
                if (line.LineTotals.Any())
                {
                    sb.AppendLine("  -- Line Totals --");
                    foreach (var total in line.LineTotals)
                    {
                        sb.AppendLine($"    {total.Concept}: {total.Total:C2}");
                    }
                }
                
                lineNumber++;
            }
            
            // Document Totals
            sb.AppendLine("\n-- DOCUMENT TOTALS --");
            sb.AppendLine($"Subtotal: {subtotal:C2}");
            
            if (document.DocumentTotals.Any())
            {
                foreach (var total in document.DocumentTotals)
                {
                    sb.AppendLine($"{total.Concept}: {total.Total:C2}");
                }
                
                // Calculate and display grand total
                decimal grandTotal = subtotal + document.DocumentTotals.Sum(t => t.Total);
                sb.AppendLine($"GRAND TOTAL: {grandTotal:C2}");
            }
            else
            {
                // If no document totals, just display the subtotal as the grand total
                sb.AppendLine($"GRAND TOTAL: {subtotal:C2}");
            }
            
            sb.AppendLine("\n====================================================");
            
            // Print to debug console
            Debug.WriteLine(sb.ToString());
            
            // Also print to test context output for easier viewing in test results
            TestContext.WriteLine(sb.ToString());
        }
        
        /// <summary>
        /// Print transaction and ledger entries to console for debugging
        /// </summary>
        private void PrintTransactionToConsole(string title, DocumentDto document, TransactionDto transaction, List<LedgerEntryDto> ledgerEntries)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("====================================================");
            sb.AppendLine($"            {title} TRANSACTION DETAILS            ");
            sb.AppendLine("====================================================");
            
            // Transaction details
            sb.AppendLine($"Transaction ID: {transaction.Id}");
            sb.AppendLine($"Document ID: {transaction.DocumentId}");
            sb.AppendLine($"Date: {transaction.TransactionDate}");
            sb.AppendLine($"Description: {transaction.Description}");
            
            // Document details
            sb.AppendLine("\n-- DOCUMENT INFO --");
            sb.AppendLine($"Document Type: {document.DocumentType?.Name ?? "Unknown"} ({document.DocumentType?.Code ?? "Unknown"})");
            sb.AppendLine($"Business Entity: {document.BusinessEntity?.Name ?? "Unknown"}");
            
            // Document totals
            sb.AppendLine("\n-- DOCUMENT TOTALS --");
            foreach (var total in document.DocumentTotals)
            {
                sb.AppendLine($"{total.Concept}: {total.Total:C2}");
            }
            
            // Ledger entries
            sb.AppendLine("\n-- LEDGER ENTRIES --");
            
            decimal totalDebits = 0;
            decimal totalCredits = 0;
            
            foreach (var entry in ledgerEntries)
            {
                string entryType = entry.EntryType == EntryType.Debit ? "DEBIT" : "CREDIT";
                sb.AppendLine($"{entryType} {entry.AccountName}: {entry.Amount:C2}");
                
                if (entry.EntryType == EntryType.Debit)
                    totalDebits += entry.Amount;
                else
                    totalCredits += entry.Amount;
            }
            
            sb.AppendLine("\n-- TRANSACTION SUMMARY --");
            sb.AppendLine($"Total Debits: {totalDebits:C2}");
            sb.AppendLine($"Total Credits: {totalCredits:C2}");
            sb.AppendLine($"Difference: {totalDebits - totalCredits:C2}");
            sb.AppendLine($"Balanced: {Math.Abs(totalDebits - totalCredits) < 0.01m}");
            
            sb.AppendLine("\n====================================================");
            
            // Print to debug console
            Debug.WriteLine(sb.ToString());
            
            // Also print to test context output for easier viewing in test results
            TestContext.WriteLine(sb.ToString());
        }
    }
}