using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Modules.Payments.Models;
using Sivar.Erp.Modules.Payments.Services;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Services;

namespace Sivar.Erp.Modules.Payments.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IObjectDb _objectDb;
        private readonly ILogger<PaymentService> _logger;
        private readonly Dictionary<string, string> _accountMappings;

        public PaymentService(IObjectDb objectDb, ILogger<PaymentService> logger, Dictionary<string, string> accountMappings)
        {
            _objectDb = objectDb;
            _logger = logger;
            _accountMappings = accountMappings;
        }

        public async Task<IList<PaymentMethodDto>> GetPaymentMethodsAsync()
        {
            return await Task.FromResult(_objectDb.PaymentMethods?.Where(pm => pm.IsActive).ToList() ?? new List<PaymentMethodDto>());
        }

        public async Task<PaymentMethodDto?> GetPaymentMethodAsync(string code)
        {
            return await Task.FromResult(_objectDb.PaymentMethods?.FirstOrDefault(pm => pm.Code == code && pm.IsActive));
        }

        public async Task<PaymentDto> CreatePaymentAsync(PaymentDto payment, string userId)
        {
            payment.PaymentId = Guid.NewGuid().ToString();
            payment.Status = PaymentStatus.Pending;

            _objectDb.Payments ??= new List<PaymentDto>();
            _objectDb.Payments.Add(payment);

            _logger.LogInformation($"Created payment {payment.PaymentId} for document {payment.DocumentNumber}");
            return await Task.FromResult(payment);
        }

        public async Task<PaymentDto> ProcessPaymentAsync(string paymentId, string userId)
        {
            var payment = _objectDb.Payments?.FirstOrDefault(p => p.PaymentId == paymentId);
            if (payment == null)
                throw new InvalidOperationException($"Payment {paymentId} not found");

            payment.Status = PaymentStatus.Completed;

            _logger.LogInformation($"Processed payment {paymentId}");
            return await Task.FromResult(payment);
        }

        public async Task<IList<PaymentDto>> GetDocumentPaymentsAsync(string documentNumber)
        {
            return await Task.FromResult(_objectDb.Payments?.Where(p => p.DocumentNumber == documentNumber).ToList() ?? new List<PaymentDto>());
        }

        public async Task<(ITransaction Transaction, IList<LedgerEntryDto> LedgerEntries)> GeneratePaymentTransactionAsync(PaymentDto payment, DocumentDto document)
        {
            var transaction = new TransactionDto
            {
                TransactionNumber = $"PAY-{DateTime.Now:yyyyMMdd}-{payment.PaymentId[..8]}",
                TransactionDate = payment.PaymentDate,
                DocumentNumber = payment.DocumentNumber,
                IsPosted = false
            };

            var ledgerEntries = new List<LedgerEntryDto>();

            // Determine if this is a payment received (sales) or payment made (purchase)
            bool isPaymentReceived = document.DocumentType.Code.Contains("CCF") ||
                                   document.DocumentType.Code.Contains("FAC") ||
                                   document.DocumentType.Name.Contains("Sales", StringComparison.OrdinalIgnoreCase);

            if (isPaymentReceived)
            {
                // Payment received (Sales): Debit Cash/Bank, Credit Accounts Receivable
                ledgerEntries.Add(new LedgerEntryDto
                {
                    LedgerEntryNumber = $"{transaction.TransactionNumber}-001",
                    TransactionNumber = transaction.TransactionNumber,
                    OfficialCode = payment.PaymentMethod.AccountCode ?? _accountMappings["CASH"],
                    EntryType = EntryType.Debit,
                    Amount = payment.Amount
                });

                ledgerEntries.Add(new LedgerEntryDto
                {
                    LedgerEntryNumber = $"{transaction.TransactionNumber}-002",
                    TransactionNumber = transaction.TransactionNumber,
                    OfficialCode = _accountMappings["ACCOUNTS_RECEIVABLE"],
                    EntryType = EntryType.Credit,
                    Amount = payment.Amount
                });
            }
            else
            {
                // Payment made (Purchase): Debit Accounts Payable, Credit Cash/Bank
                ledgerEntries.Add(new LedgerEntryDto
                {
                    LedgerEntryNumber = $"{transaction.TransactionNumber}-001",
                    TransactionNumber = transaction.TransactionNumber,
                    OfficialCode = _accountMappings["ACCOUNTS_PAYABLE"],
                    EntryType = EntryType.Debit,
                    Amount = payment.Amount
                });

                ledgerEntries.Add(new LedgerEntryDto
                {
                    LedgerEntryNumber = $"{transaction.TransactionNumber}-002",
                    TransactionNumber = transaction.TransactionNumber,
                    OfficialCode = payment.PaymentMethod.AccountCode ?? _accountMappings["CASH"],
                    EntryType = EntryType.Credit,
                    Amount = payment.Amount
                });
            }

            return await Task.FromResult((transaction as ITransaction, ledgerEntries as IList<LedgerEntryDto>));
        }
    }
}
