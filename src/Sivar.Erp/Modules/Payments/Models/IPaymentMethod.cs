using System;

namespace Sivar.Erp.Modules.Payments.Models
{
    public interface IPaymentMethod
    {
        string? AccountCode { get; set; }
        Dictionary<string, string> AdditionalProperties { get; set; }
        string Code { get; set; }
        bool IsActive { get; set; }
        string Name { get; set; }
        bool RequiresBankAccount { get; set; }
        bool RequiresReference { get; set; }
        PaymentMethodType Type { get; set; }
    }
}
