using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Examples of how to use the transaction generator system
    /// </summary>
    public static class TransactionGeneratorExamples
    {
        /// <summary>
        /// Example of setting up and using the transaction generator
        /// </summary>
        public static async Task BasicSetupExample(ITransactionService transactionService)
        {
            // 1. Create account mappings
            var accountMappings = new Dictionary<string, Guid>
            {
                ["Cash"] = new Guid("11111111-1111-1111-1111-111111111111"),
                ["Bank"] = new Guid("22222222-2222-2222-2222-222222222222"),
                ["AccountsReceivable"] = new Guid("33333333-3333-3333-3333-333333333333"),
                ["AccountsPayable"] = new Guid("44444444-4444-4444-4444-444444444444"),
                ["SalesRevenue"] = new Guid("55555555-5555-5555-5555-555555555555"),
                ["SalesTaxPayable"] = new Guid("66666666-6666-6666-6666-666666666666"),
                ["InputTax"] = new Guid("77777777-7777-7777-7777-777777777777"),
                ["CostOfGoodsSold"] = new Guid("88888888-8888-8888-8888-888888888888"),
                ["Inventory"] = new Guid("99999999-9999-9999-9999-999999999999")
            };

            // 2. Create the transaction generator
            var generator = new DocumentTransactionGenerator(transactionService, accountMappings);

            // 3. Register templates using the factory
            generator.RegisterTemplate(TransactionTemplateFactory.CreateSalesInvoiceTemplate("INV"));
            generator.RegisterTemplate(TransactionTemplateFactory.CreatePurchaseInvoiceTemplate("PO"));
            generator.RegisterTemplate(TransactionTemplateFactory.CreatePaymentTemplate("PAY", "Bank"));
            generator.RegisterTemplate(TransactionTemplateFactory.CreateReceiptTemplate("REC", "Cash"));
            
            // 4. Create a document (this would normally come from your application)
            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Today),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                DocumentType = new DocumentTypeDto { Code = "INV", Name = "Sales Invoice" },
                BusinessEntity = new BusinessEntityDto { Name = "Customer XYZ" }
            };
            
            // Add document totals for the transaction to use
            document.DocumentTotals.Add(new TotalDto { Concept = "Subtotal", Total = 1000.00m });
            document.DocumentTotals.Add(new TotalDto { Concept = "Tax: IVA (13%)", Total = 130.00m });
            
            // 5. Generate and save the transaction
            var (transaction, ledgerEntries) = await generator.GenerateTransactionAsync(document);
            
            // 6. Do something with the transaction and ledger entries
            Console.WriteLine($"Created transaction: {transaction.Id} - {transaction.Description}");
            
            foreach (var entry in ledgerEntries)
            {
                Console.WriteLine($"  {entry.EntryType} {entry.AccountName}: {entry.Amount:C}");
            }
        }
        
        /// <summary>
        /// Example of creating custom templates
        /// </summary>
        public static void CustomTemplateExample(DocumentTransactionGenerator generator)
        {
            // Create a custom template for a specific document type
            var customTemplate = new TransactionTemplate("CUSTOM", 
                document => $"Custom Transaction - {document.Date}")
                .WithEntries(
                    // Debit Office Supplies Expense
                    AccountingTransactionEntry.Debit("OfficeSupplies", AmountCalculators.Subtotal)
                        .WithAccountName("Office Supplies Expense"),
                    
                    // Credit Cash
                    AccountingTransactionEntry.Credit("Cash", AmountCalculators.GrandTotal)
                );
            
            // Register the template
            generator.RegisterTemplate(customTemplate);
            
            // Add any necessary account mappings
            generator.AddAccountMapping("OfficeSupplies", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        }
        
        /// <summary>
        /// Example of advanced configurations
        /// </summary>
        public static void AdvancedConfigurationExample()
        {
            var transactionService = new TransactionService();
            var generator = new DocumentTransactionGenerator(transactionService);
            
            // Create a template for payroll transactions
            var payrollTemplate = new TransactionTemplate("PAYROLL",
                    document => $"Payroll - {document.Date}")
                .WithEntries(
                    // Debit Salary Expense
                    AccountingTransactionEntry.Debit("SalaryExpense", 
                        AmountCalculators.ForConcept("Gross Salary"))
                        .WithAccountName("Salary Expense"),
                        
                    // Debit Employer Tax Expense
                    AccountingTransactionEntry.Debit("EmployerTaxExpense", 
                        AmountCalculators.ForConcept("Employer Taxes"))
                        .WithAccountName("Employer Tax Expense"),
                        
                    // Credit Employee Tax Payable
                    AccountingTransactionEntry.Credit("EmployeeTaxPayable", 
                        AmountCalculators.ForConcept("Employee Taxes"))
                        .WithAccountName("Employee Tax Payable"),
                        
                    // Credit Net Salary Payable
                    AccountingTransactionEntry.Credit("SalaryPayable",
                        document => AmountCalculators.ForConcept("Gross Salary")(document) - 
                                    AmountCalculators.ForConcept("Employee Taxes")(document))
                        .WithAccountName("Salary Payable")
                );
                
            // Create a template with custom amount calculation
            var discountTemplate = new TransactionTemplate("DISC",
                    document => $"Sales Discount - {document.Date}")
                .WithEntries(
                    // Debit Discount Expense
                    AccountingTransactionEntry.Debit("DiscountExpense", 
                        document => {
                            // Custom complex calculation based on multiple conditions
                            var subtotal = AmountCalculators.Subtotal(document);
                            
                            // Different discount rates based on subtotal amount
                            if (subtotal > 5000)
                                return subtotal * 0.10m; // 10% discount
                            else if (subtotal > 1000)
                                return subtotal * 0.05m; // 5% discount
                            else
                                return 0; // No discount
                        })
                        .WithAccountName("Sales Discount Expense"),
                        
                    // Credit Accounts Receivable for the discount amount
                    AccountingTransactionEntry.Credit("AccountsReceivable", 
                        document => {
                            // Same calculation as above to keep debits and credits balanced
                            var subtotal = AmountCalculators.Subtotal(document);
                            
                            if (subtotal > 5000)
                                return subtotal * 0.10m;
                            else if (subtotal > 1000)
                                return subtotal * 0.05m;
                            else
                                return 0;
                        })
                        .WithAccountName("Accounts Receivable")
                );
                
            // Register templates
            generator.RegisterTemplate(payrollTemplate);
            generator.RegisterTemplate(discountTemplate);
        }
    }
}