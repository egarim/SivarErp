using System;

namespace Sivar.Erp.Modules.Payments.Models
{
    public interface IPaymentDto
    {
        Dictionary<string, string> AdditionalData { get; set; }
        decimal Amount { get; set; }
        string? BankAccount { get; set; }
        string DocumentNumber { get; set; }
        string? Notes { get; set; }
        DateOnly PaymentDate { get; set; }
        string PaymentId { get; set; }
        PaymentMethodDto PaymentMethod { get; set; }
        string? Reference { get; set; }
        PaymentStatus Status { get; set; }
    }
}
