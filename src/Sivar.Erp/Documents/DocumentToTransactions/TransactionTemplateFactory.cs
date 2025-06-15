using System;
using System.Collections.Generic;

namespace Sivar.Erp.Documents.DocumentToTransactions
{
    /// <summary>
    /// Factory for creating common transaction templates
    /// </summary>
    public static class TransactionTemplateFactory
    {
        /// <summary>
        /// Creates a sales invoice template
        /// </summary>
        /// <param name="documentTypeCode">The document type code for sales invoices</param>
        /// <returns>A transaction template for sales invoices</returns>
        public static TransactionTemplate CreateSalesInvoiceTemplate(string documentTypeCode = "INV")
        {
            return new TransactionTemplate(documentTypeCode, document => {
                    string customerName = document.BusinessEntity?.Name ?? "Unknown Customer";
                    return $"Sales Invoice - {customerName} - {document.Date}";
                })
                .WithEntries(
                    // Debit Accounts Receivable for the grand total
                    AccountingTransactionEntry.Debit("AccountsReceivable", AmountCalculators.GrandTotal)
                        .WithAccountName("Accounts Receivable"),
                    
                    // Credit Sales Revenue for the subtotal
                    AccountingTransactionEntry.Credit("SalesRevenue", AmountCalculators.Subtotal)
                        .WithAccountName("Sales Revenue"),
                    
                    // Credit Sales Tax Payable for the tax amount
                    AccountingTransactionEntry.Credit("SalesTaxPayable", AmountCalculators.TaxTotal)
                        .WithAccountName("Sales Tax Payable"),
                    
                    // Debit Cost of Goods Sold (if inventory tracking is enabled)
                    AccountingTransactionEntry.Debit("CostOfGoodsSold", AmountCalculators.EstimatedCostOfGoodsSold())
                        .WithAccountName("Cost of Goods Sold"),
                    
                    // Credit Inventory (if inventory tracking is enabled)
                    AccountingTransactionEntry.Credit("Inventory", AmountCalculators.EstimatedCostOfGoodsSold())
                        .WithAccountName("Inventory")
                );
        }
        
        /// <summary>
        /// Creates a purchase invoice template
        /// </summary>
        /// <param name="documentTypeCode">The document type code for purchase invoices</param>
        /// <returns>A transaction template for purchase invoices</returns>
        public static TransactionTemplate CreatePurchaseInvoiceTemplate(string documentTypeCode = "PO")
        {
            return new TransactionTemplate(documentTypeCode, document => {
                    string vendorName = document.BusinessEntity?.Name ?? "Unknown Vendor";
                    return $"Purchase Invoice - {vendorName} - {document.Date}";
                })
                .WithEntries(
                    // Debit Inventory for the subtotal
                    AccountingTransactionEntry.Debit("Inventory", AmountCalculators.Subtotal)
                        .WithAccountName("Inventory"),
                    
                    // Debit Input Tax for the tax amount
                    AccountingTransactionEntry.Debit("InputTax", AmountCalculators.TaxTotal)
                        .WithAccountName("Input Tax Receivable"),
                    
                    // Credit Accounts Payable for the grand total
                    AccountingTransactionEntry.Credit("AccountsPayable", AmountCalculators.GrandTotal)
                        .WithAccountName("Accounts Payable")
                );
        }
        
        /// <summary>
        /// Creates a payment template
        /// </summary>
        /// <param name="documentTypeCode">The document type code for payments</param>
        /// <param name="paymentMethod">The payment method (cash, bank, etc.)</param>
        /// <returns>A transaction template for payments</returns>
        public static TransactionTemplate CreatePaymentTemplate(string documentTypeCode = "PAY", string paymentMethod = "Cash")
        {
            return new TransactionTemplate(documentTypeCode, document => {
                    string entityName = document.BusinessEntity?.Name ?? "Unknown Entity";
                    return $"Payment - {entityName} - {document.Date}";
                })
                .WithEntries(
                    // Debit Accounts Payable
                    AccountingTransactionEntry.Debit("AccountsPayable", AmountCalculators.GrandTotal)
                        .WithAccountName("Accounts Payable"),
                    
                    // Credit the payment method account
                    AccountingTransactionEntry.Credit(paymentMethod, AmountCalculators.GrandTotal)
                        .WithAccountName(paymentMethod)
                );
        }
        
        /// <summary>
        /// Creates a receipt template
        /// </summary>
        /// <param name="documentTypeCode">The document type code for receipts</param>
        /// <param name="receiptMethod">The receipt method (cash, bank, etc.)</param>
        /// <returns>A transaction template for receipts</returns>
        public static TransactionTemplate CreateReceiptTemplate(string documentTypeCode = "REC", string receiptMethod = "Cash")
        {
            return new TransactionTemplate(documentTypeCode, document => {
                    string entityName = document.BusinessEntity?.Name ?? "Unknown Entity";
                    return $"Receipt - {entityName} - {document.Date}";
                })
                .WithEntries(
                    // Debit the receipt method account
                    AccountingTransactionEntry.Debit(receiptMethod, AmountCalculators.GrandTotal)
                        .WithAccountName(receiptMethod),
                    
                    // Credit Accounts Receivable
                    AccountingTransactionEntry.Credit("AccountsReceivable", AmountCalculators.GrandTotal)
                        .WithAccountName("Accounts Receivable")
                );
        }
        
        /// <summary>
        /// Creates a template for miscellaneous expenses
        /// </summary>
        /// <param name="documentTypeCode">The document type code</param>
        /// <param name="expenseAccount">The expense account key</param>
        /// <param name="expenseAccountName">The expense account name</param>
        /// <param name="paymentMethod">The payment method account key</param>
        /// <returns>A transaction template for expenses</returns>
        public static TransactionTemplate CreateExpenseTemplate(
            string documentTypeCode, 
            string expenseAccount, 
            string expenseAccountName, 
            string paymentMethod = "Cash")
        {
            return new TransactionTemplate(documentTypeCode, document => {
                    string entityName = document.BusinessEntity?.Name ?? "Unknown Entity";
                    return $"Expense - {entityName} - {document.Date}";
                })
                .WithEntries(
                    // Debit the expense account
                    AccountingTransactionEntry.Debit(expenseAccount, AmountCalculators.GrandTotal)
                        .WithAccountName(expenseAccountName),
                    
                    // Credit the payment method account
                    AccountingTransactionEntry.Credit(paymentMethod, AmountCalculators.GrandTotal)
                        .WithAccountName(paymentMethod)
                );
        }
        
        /// <summary>
        /// Creates a custom template with the specified entries
        /// </summary>
        /// <param name="documentTypeCode">The document type code</param>
        /// <param name="description">The transaction description generator</param>
        /// <param name="entries">The transaction entries</param>
        /// <returns>A custom transaction template</returns>
        public static TransactionTemplate CreateCustomTemplate(
            string documentTypeCode,
            Func<DocumentDto, string> description,
            params AccountingTransactionEntry[] entries)
        {
            var template = new TransactionTemplate(documentTypeCode, description);
            template.WithEntries(entries);
            return template;
        }
    }
}