using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sivar.Erp.Documents;
using Sivar.Erp.Documents.Tax;
using Sivar.Erp.Taxes;
using Sivar.Erp.Taxes.TaxGroup;
using Sivar.Erp.Taxes.TaxRule;

namespace Tests.IntegrationTests.ElSalvador
{
    /// <summary>
    /// Tests for El Salvador tax rule evaluation
    /// </summary>
    [TestFixture]
    public class TaxRuleEvaluatorTests
    {
        [Test]
        public void TaxRuleEvaluator_EvaluatesDocumentTaxes_ForElSalvador()
        {
            // Arrange
            // 1. Create document types for El Salvador
            var creditoFiscalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true,
                DocumentOperation = DocumentOperation.SalesInvoice
            };

            var consumidorFinalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CNF",
                Name = "Consumidor Final",
                IsEnabled = true,
                DocumentOperation = DocumentOperation.SalesInvoice
            };

            // 2. Create business entities
            var registeredCompany = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Code = "COMPANY001",
                Name = "Empresa Registrada S.A. de C.V."
            };

            var individualConsumer = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Code = "CONSUMER001",
                Name = "Consumidor Final"
            };

            // 3. Create products/items
            var standardProduct = new ItemDto
            {
                Oid = Guid.NewGuid(),
                Code = "PROD001",
                Description = "Producto Est�ndar",
                BasePrice = 100m
            };

            var exemptProduct = new ItemDto
            {
                Oid = Guid.NewGuid(),
                Code = "PROD002",
                Description = "Producto Exento",
                BasePrice = 50m
            };

            // 4. Create tax groups
            var registeredCompanyGroupId = Guid.NewGuid();
            var exemptItemGroupId = Guid.NewGuid();

            // 5. Create taxes
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
                // Rule 1: Apply IVA to Credito Fiscal documents for registered companies (except for exempt items)
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Oid,
                    DocumentOperation = DocumentOperation.SalesInvoice,
                    BusinessEntityGroupId = registeredCompanyGroupId,
                    IsEnabled = true,
                    Priority = 1
                },
                
                // Rule 2: Don't apply IVA to exempt items (override rule)
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Oid,
                    DocumentOperation = DocumentOperation.SalesInvoice,
                    BusinessEntityGroupId = registeredCompanyGroupId,
                    ItemGroupId = exemptItemGroupId,
                    IsEnabled = false, // This rule PREVENTS the tax from being applied
                    Priority = 0 // Higher priority (lower number) makes this rule override Rule 1
                }
            };

            // 7. Setup group memberships
            var groupMemberships = new List<GroupMembershipDto>
            {
                // Add registered company to taxable group
                new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = registeredCompanyGroupId,
                    EntityId = registeredCompany.Oid,
                    GroupType = GroupType.BusinessEntity
                },
                
                // Add exempt product to exempt item group
                new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = exemptItemGroupId,
                    EntityId = exemptProduct.Oid,
                    GroupType = GroupType.Item
                }
            };

            // 8. Create the TaxRuleEvaluator
            var taxRuleEvaluator = new TaxRuleEvaluator(
                taxRules,
                new List<TaxDto> { ivaTax },
                groupMemberships);

            // 9. Create document for testing
            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                DocumentNumber = "CF-2023-001",
                Date = DateOnly.FromDateTime(DateTime.Today),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = registeredCompany,
                DocumentType = creditoFiscalDocType
            };

            // 10. Add lines to document
            var standardLine = new LineDto
            {
                Item = standardProduct,
                Quantity = 2,
                UnitPrice = 100m
                // Amount = 200m will be automatically calculated
            };

            var exemptLine = new LineDto
            {
                Item = exemptProduct,
                Quantity = 3,
                UnitPrice = 50m
                // Amount = 150m will be automatically calculated
            };

            // Initialize the Amount property of each line, since we're using a test environment
            // and the actual business logic might not calculate it automatically
            standardLine.Amount = standardLine.Quantity * standardLine.UnitPrice;
            exemptLine.Amount = exemptLine.Quantity * exemptLine.UnitPrice;

            document.Lines.Add(standardLine);
            document.Lines.Add(exemptLine);

            // Act
            // 1. Get applicable document taxes
            var documentTaxes = taxRuleEvaluator.GetApplicableDocumentTaxes(document, document.DocumentType.Code);

            // 2. Get applicable line taxes for each line
            var standardLineTaxes = taxRuleEvaluator.GetApplicableLineTaxes(document, document.DocumentType.Code, standardLine);

            var exemptLineTaxes = taxRuleEvaluator.GetApplicableLineTaxes(document, document.DocumentType.Code, exemptLine);

            // 3. Calculate all taxes using DocumentTaxCalculator
            var taxCalculator = new DocumentTaxCalculator(
                document,
                document.DocumentType.Code,
                taxRuleEvaluator);

            // Calculate line taxes
            taxCalculator.CalculateLineTaxes(standardLine);
            taxCalculator.CalculateLineTaxes(exemptLine);

            // Calculate document level taxes
            taxCalculator.CalculateDocumentTaxes();

            // Output results to the console
            PrintTaxEvaluationResults(
                document,
                documentTaxes,
                standardLineTaxes,
                exemptLineTaxes);

            // Assert
            // 1. Document should have no document-level taxes (all are line level)
            Assert.That(documentTaxes, Is.Empty, "No document-level taxes should be found");

            // 2. Standard product line should have IVA tax
            Assert.That(standardLineTaxes, Has.Count.EqualTo(1), "Standard product should have one tax (IVA)");
            Assert.That(standardLineTaxes[0].Code, Is.EqualTo("IVA"), "Tax should be IVA");

            // 3. Exempt product line should have no taxes due to the override rule
            Assert.That(exemptLineTaxes, Is.Empty, "Exempt product should have no taxes");

            // 4. Check that the tax was actually applied to the standard line
            var ivaLineTotal = standardLine.LineTotals
                .FirstOrDefault(t => t.Concept.Contains("IVA"));
            Assert.That(ivaLineTotal, Is.Not.Null, "IVA tax should be applied to standard line");
            Assert.That(ivaLineTotal.Total, Is.EqualTo(26m), "IVA (13%) of 200 should be 26");

            // 5. Check document totals include the IVA tax
            var ivaDocTotal = document.DocumentTotals
                .FirstOrDefault(t => t.Concept.Contains("IVA"));
            Assert.That(ivaDocTotal, Is.Not.Null, "IVA tax should be included in document totals");
            Assert.That(ivaDocTotal.Total, Is.EqualTo(26m), "Document total for IVA should be 26");

            // 6. Calculate and check the grand total
            var subtotal = standardLine.Amount + exemptLine.Amount; // 200 + 150 = 350
            var grandTotal = subtotal + document.DocumentTotals.Sum(t => t.Total); // 350 + 26 = 376
            Assert.That(grandTotal, Is.EqualTo(376m), "Grand total should be 376");
        }

        [Test]
        public void TaxRuleEvaluator_ComparesCreditoFiscalVsConsumidorFinal()
        {
            // Arrange
            // 1. Create document types
            var creditoFiscalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true,
                DocumentOperation = DocumentOperation.SalesInvoice
            };

            var consumidorFinalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CNF",
                Name = "Consumidor Final",
                IsEnabled = true,
                DocumentOperation = DocumentOperation.SalesInvoice
            };

            // 2. Create business entities
            var company = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Code = "COMPANY001",
                Name = "Empresa S.A. de C.V."
            };

            var individual = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Code = "CONS001",
                Name = "Juan Perez"
            };

            // 3. Create product
            var product = new ItemDto
            {
                Oid = Guid.NewGuid(),
                Code = "PROD001",
                Description = "Producto Est�ndar",
                BasePrice = 100m
            };

            // 4. Create tax group for companies
            var taxableCompanyGroupId = Guid.NewGuid();

            // 5. Create taxes
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
                // Rule for applying IVA to Credito Fiscal documents for registered companies
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Oid,
                    DocumentOperation = DocumentOperation.SalesInvoice,
                    DocumentTypeCode = creditoFiscalDocType.Code, // Keep for backward compatibility
                    BusinessEntityGroupId = taxableCompanyGroupId,
                    IsEnabled = true,
                    Priority = 1
                }
                // No rule for Consumidor Final means no tax applies
            };

            // 7. Setup group memberships
            var groupMemberships = new List<GroupMembershipDto>
            {
                // Add company to taxable group
                new GroupMembershipDto
                {
                    Oid = Guid.NewGuid(),
                    GroupId = taxableCompanyGroupId,
                    EntityId = company.Oid,
                    GroupType = GroupType.BusinessEntity
                }
            };

            // 8. Create the TaxRuleEvaluator
            var taxRuleEvaluator = new TaxRuleEvaluator(
                taxRules,
                new List<TaxDto> { ivaTax },
                groupMemberships);

            // 9. Create documents
            var cfDocument = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                DocumentNumber = "CF-2023-001",
                Date = DateOnly.FromDateTime(DateTime.Today),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = company,
                DocumentType = creditoFiscalDocType
            };

            var cnfDocument = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                DocumentNumber = "CNF-2023-001",
                Date = DateOnly.FromDateTime(DateTime.Today),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                BusinessEntity = individual,
                DocumentType = consumidorFinalDocType
            };

            // 10. Add same line to both documents
            var cfLine = new LineDto
            {
                Item = product,
                Quantity = 2,
                UnitPrice = 100m
            };

            var cnfLine = new LineDto
            {
                Item = product,
                Quantity = 2,
                UnitPrice = 100m
            };

            // Initialize line amounts
            cfLine.Amount = cfLine.Quantity * cfLine.UnitPrice;
            cnfLine.Amount = cnfLine.Quantity * cnfLine.UnitPrice;

            cfDocument.Lines.Add(cfLine);
            cnfDocument.Lines.Add(cnfLine);

            // Act
            // Calculate taxes for both documents
            var cfTaxCalculator = new DocumentTaxCalculator(
                cfDocument,
                creditoFiscalDocType.Code,
                taxRuleEvaluator);

            var cnfTaxCalculator = new DocumentTaxCalculator(
                cnfDocument,
                consumidorFinalDocType.Code,
                taxRuleEvaluator);

            // Calculate line taxes
            cfTaxCalculator.CalculateLineTaxes(cfLine);
            cnfTaxCalculator.CalculateLineTaxes(cnfLine);

            // Calculate document taxes
            cfTaxCalculator.CalculateDocumentTaxes();
            cnfTaxCalculator.CalculateDocumentTaxes();

            // Print results
            Console.WriteLine("===== CREDITO FISCAL DOCUMENT =====");
            PrintDocumentWithTaxes(cfDocument);

            Console.WriteLine("\n===== CONSUMIDOR FINAL DOCUMENT =====");
            PrintDocumentWithTaxes(cnfDocument);

            // Assert
            // 1. Credito Fiscal should have IVA tax
            var cfIvaTax = cfLine.LineTotals.FirstOrDefault(t => t.Concept.Contains("IVA"));
            Assert.That(cfIvaTax, Is.Not.Null, "Credito Fiscal should have IVA tax applied");
            Assert.That(cfIvaTax.Total, Is.EqualTo(26m), "IVA tax should be 13% of 200 = 26");

            // 2. Consumidor Final should not have any taxes
            Assert.That(cnfLine.LineTotals, Is.Empty, "Consumidor Final should not have any line taxes");

            // 3. Grand totals should be different
            var cfSubtotal = cfLine.Amount;
            var cfGrandTotal = cfSubtotal + cfDocument.DocumentTotals.Sum(t => t.Total);

            var cnfSubtotal = cnfLine.Amount;
            var cnfGrandTotal = cnfSubtotal + cnfDocument.DocumentTotals.Sum(t => t.Total);

            Assert.That(cfGrandTotal, Is.EqualTo(226m), "CF grand total should be 226 (200 + 26)");
            Assert.That(cnfGrandTotal, Is.EqualTo(200m), "CNF grand total should be 200 (no taxes)");
        }

        /// <summary>
        /// Prints detailed tax evaluation results to the console
        /// </summary>
        private void PrintTaxEvaluationResults(
            DocumentDto document,
            IList<TaxDto> documentTaxes,
            IList<TaxDto> standardLineTaxes,
            IList<TaxDto> exemptLineTaxes)
        {
            var sb = new StringBuilder();

            sb.AppendLine("====================================================");
            sb.AppendLine("          TAX RULE EVALUATOR TEST RESULTS           ");
            sb.AppendLine("====================================================");

            // Basic document info
            sb.AppendLine($"Document Number: {document.DocumentNumber}");
            sb.AppendLine($"Document Type: {document.DocumentType?.Name} ({document.DocumentType?.Code})");
            sb.AppendLine($"Document Operation: {document.DocumentType?.DocumentOperation}");
            sb.AppendLine($"Business Entity: {document.BusinessEntity?.Name} ({document.BusinessEntity?.Code})");
            sb.AppendLine($"Date: {document.Date}");

            // Document taxes found by evaluator
            sb.AppendLine("\n-- DOCUMENT-LEVEL TAXES --");
            if (documentTaxes.Any())
            {
                foreach (var tax in documentTaxes)
                {
                    sb.AppendLine($"Tax: {tax.Name} ({tax.Code}) - {FormatTaxRate(tax)}");
                }
            }
            else
            {
                sb.AppendLine("No document-level taxes found");
            }

            // Line taxes found by evaluator
            sb.AppendLine("\n-- LINE-LEVEL TAXES --");

            // Standard product
            var standardLine = document.Lines[0] as LineDto;
            sb.AppendLine($"\nStandard Product: {standardLine?.Item?.Code} - {standardLine?.Item?.Description}");
            sb.AppendLine($"Line Amount: {standardLine?.Amount:C2}");

            if (standardLineTaxes.Any())
            {
                sb.AppendLine("Applicable Taxes:");
                foreach (var tax in standardLineTaxes)
                {
                    sb.AppendLine($"  - {tax.Name} ({tax.Code}) - {FormatTaxRate(tax)}");
                }
            }
            else
            {
                sb.AppendLine("No taxes apply to this line");
            }

            // Line totals after calculation
            if (standardLine?.LineTotals.Any() == true)
            {
                sb.AppendLine("Applied Line Totals:");
                foreach (var total in standardLine.LineTotals)
                {
                    sb.AppendLine($"  - {total.Concept}: {total.Total:C2}");
                }
            }

            // Exempt product
            var exemptLine = document.Lines[1] as LineDto;
            sb.AppendLine($"\nExempt Product: {exemptLine?.Item?.Code} - {exemptLine?.Item?.Description}");
            sb.AppendLine($"Line Amount: {exemptLine?.Amount:C2}");

            if (exemptLineTaxes.Any())
            {
                sb.AppendLine("Applicable Taxes:");
                foreach (var tax in exemptLineTaxes)
                {
                    sb.AppendLine($"  - {tax.Name} ({tax.Code}) - {FormatTaxRate(tax)}");
                }
            }
            else
            {
                sb.AppendLine("No taxes apply to this line (exempt)");
            }

            // Line totals after calculation
            if (exemptLine?.LineTotals.Any() == true)
            {
                sb.AppendLine("Applied Line Totals:");
                foreach (var total in exemptLine.LineTotals)
                {
                    sb.AppendLine($"  - {total.Concept}: {total.Total:C2}");
                }
            }

            // Document totals
            sb.AppendLine("\n-- DOCUMENT TOTALS AFTER CALCULATION --");
            decimal subtotal = document.Lines.Sum(l => l.Amount);
            sb.AppendLine($"Subtotal: {subtotal:C2}");

            if (document.DocumentTotals.Any())
            {
                foreach (var total in document.DocumentTotals)
                {
                    sb.AppendLine($"{total.Concept}: {total.Total:C2}");
                }

                decimal grandTotal = subtotal + document.DocumentTotals.Sum(t => t.Total);
                sb.AppendLine($"GRAND TOTAL: {grandTotal:C2}");
            }
            else
            {
                sb.AppendLine($"GRAND TOTAL: {subtotal:C2} (No additional totals)");
            }

            sb.AppendLine("====================================================");

            // Output to console
            Console.WriteLine(sb.ToString());

            // Also output to test context
            TestContext.WriteLine(sb.ToString());
        }

        /// <summary>
        /// Formats a tax rate for display
        /// </summary>
        private string FormatTaxRate(TaxDto tax)
        {
            return tax.TaxType switch
            {
                TaxType.Percentage => $"{tax.Percentage}%",
                TaxType.FixedAmount => $"${tax.Amount:0.00} fixed",
                TaxType.AmountPerUnit => $"${tax.Amount:0.00} per unit",
                _ => "Unknown tax type"
            };
        }

        /// <summary>
        /// Print document with tax information to the console
        /// </summary>
        private void PrintDocumentWithTaxes(DocumentDto document)
        {
            var sb = new StringBuilder();

            // Basic document info
            sb.AppendLine($"Document Number: {document.DocumentNumber}");
            sb.AppendLine($"Document Type: {document.DocumentType?.Name} ({document.DocumentType?.Code})");
            sb.AppendLine($"Document Operation: {document.DocumentType?.DocumentOperation}");
            sb.AppendLine($"Business Entity: {document.BusinessEntity?.Name}");

            // Lines
            sb.AppendLine("\nLINES:");
            foreach (var line in document.Lines)
            {
                var lineDto = line as LineDto;

                sb.AppendLine($"- {lineDto?.Item?.Description}");
                sb.AppendLine($"  Quantity: {lineDto?.Quantity} � ${lineDto?.UnitPrice:0.00} = ${lineDto?.Amount:0.00}");

                if (lineDto?.LineTotals.Any() == true)
                {
                    foreach (var total in lineDto.LineTotals)
                    {
                        sb.AppendLine($"  {total.Concept}: ${total.Total:0.00}");
                    }
                }
                else
                {
                    sb.AppendLine("  No taxes or additional totals");
                }
            }

            // Document totals
            sb.AppendLine("\nDOCUMENT TOTALS:");
            decimal subtotal = document.Lines.Sum(l => l.Amount);
            sb.AppendLine($"Subtotal: ${subtotal:0.00}");

            decimal taxTotal = 0;

            if (document.DocumentTotals.Any())
            {
                foreach (var total in document.DocumentTotals)
                {
                    sb.AppendLine($"{total.Concept}: ${total.Total:0.00}");

                    if (total.Concept.Contains("IVA") || total.Concept.Contains("Tax"))
                    {
                        taxTotal += total.Total;
                    }
                }
            }

            decimal grandTotal = subtotal + document.DocumentTotals.Sum(t => t.Total);
            sb.AppendLine($"GRAND TOTAL: ${grandTotal:0.00}");

            // Output
            Console.WriteLine(sb.ToString());
        }
    }
}