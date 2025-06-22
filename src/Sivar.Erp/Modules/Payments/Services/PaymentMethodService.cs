using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Modules.Payments.Models;
using Sivar.Erp.Services;

namespace Sivar.Erp.Modules.Payments.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IObjectDb _objectDb;
        private readonly ILogger<PaymentMethodService> _logger;

        public PaymentMethodService(IObjectDb objectDb, ILogger<PaymentMethodService> logger)
        {
            _objectDb = objectDb;
            _logger = logger;
        }

        public async Task<PaymentMethodDto> CreatePaymentMethodAsync(PaymentMethodDto paymentMethod, string userId)
        {
            _objectDb.PaymentMethods ??= new List<PaymentMethodDto>();

            // Check if payment method already exists
            var existing = _objectDb.PaymentMethods.FirstOrDefault(pm => pm.Code == paymentMethod.Code);
            if (existing != null)
            {
                throw new InvalidOperationException($"Payment method with code {paymentMethod.Code} already exists");
            }

            _objectDb.PaymentMethods.Add(paymentMethod);
            _logger.LogInformation($"Created payment method {paymentMethod.Code} - {paymentMethod.Name}");

            return await Task.FromResult(paymentMethod);
        }

        public async Task<PaymentMethodDto> UpdatePaymentMethodAsync(PaymentMethodDto paymentMethod, string userId)
        {
            var existing = _objectDb.PaymentMethods?.FirstOrDefault(pm => pm.Code == paymentMethod.Code);
            if (existing == null)
            {
                throw new InvalidOperationException($"Payment method with code {paymentMethod.Code} not found");
            }

            // Update properties
            existing.Name = paymentMethod.Name;
            existing.Type = paymentMethod.Type;
            existing.AccountCode = paymentMethod.AccountCode;
            existing.RequiresBankAccount = paymentMethod.RequiresBankAccount;
            existing.RequiresReference = paymentMethod.RequiresReference;
            existing.IsActive = paymentMethod.IsActive;
            existing.AdditionalProperties = paymentMethod.AdditionalProperties;

            _logger.LogInformation($"Updated payment method {paymentMethod.Code}");

            return await Task.FromResult(existing);
        }

        public async Task<IList<PaymentMethodDto>> GetAllPaymentMethodsAsync()
        {
            return await Task.FromResult(_objectDb.PaymentMethods?.ToList() ?? new List<PaymentMethodDto>());
        }

        public async Task<PaymentMethodDto?> GetPaymentMethodByCodeAsync(string code)
        {
            return await Task.FromResult(_objectDb.PaymentMethods?.FirstOrDefault(pm => pm.Code == code));
        }
    }
}
