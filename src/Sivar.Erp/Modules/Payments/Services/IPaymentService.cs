using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Modules.Payments.Models;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.Accounting.Transactions;

namespace Sivar.Erp.Modules.Payments.Services
{
    public interface IPaymentService
    {
        Task<IList<PaymentMethodDto>> GetPaymentMethodsAsync();
        Task<PaymentMethodDto?> GetPaymentMethodAsync(string code);
        Task<PaymentDto> CreatePaymentAsync(PaymentDto payment, string userId);
        Task<PaymentDto> ProcessPaymentAsync(string paymentId, string userId);
        Task<IList<PaymentDto>> GetDocumentPaymentsAsync(string documentNumber);
        Task<(ITransaction Transaction, IList<LedgerEntryDto> LedgerEntries)> GeneratePaymentTransactionAsync(PaymentDto payment, DocumentDto document);
    }

    public interface IPaymentMethodService
    {
        Task<PaymentMethodDto> CreatePaymentMethodAsync(PaymentMethodDto paymentMethod, string userId);
        Task<PaymentMethodDto> UpdatePaymentMethodAsync(PaymentMethodDto paymentMethod, string userId);
        Task<IList<PaymentMethodDto>> GetAllPaymentMethodsAsync();
        Task<PaymentMethodDto?> GetPaymentMethodByCodeAsync(string code);
    }
}
