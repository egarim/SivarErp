using System;
using System.Collections.Generic;

namespace Sivar.Erp.Modules.Payments.Models
{
    public class PaymentMethodDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public PaymentMethodType Type { get; set; }
        public string? AccountCode { get; set; } // GL Account for this payment method
        public bool RequiresBankAccount { get; set; }
        public bool RequiresReference { get; set; }
        public bool IsActive { get; set; } = true;
        public Dictionary<string, string> AdditionalProperties { get; set; } = new();
    }

    public enum PaymentMethodType
    {
        Cash,
        Check,
        BankTransfer,
        CreditCard,
        DebitCard,
        DigitalWallet,
        Other
    }
}
