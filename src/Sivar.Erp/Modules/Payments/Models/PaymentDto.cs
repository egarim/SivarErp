using System;
using System.Collections.Generic;

namespace Sivar.Erp.Modules.Payments.Models
{
    public class PaymentDto
    {
        public string PaymentId { get; set; } = Guid.NewGuid().ToString();
        public string DocumentNumber { get; set; } = string.Empty;
        public PaymentMethodDto PaymentMethod { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateOnly PaymentDate { get; set; }
        public string? Reference { get; set; }
        public string? BankAccount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? Notes { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; } = new();
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled
    }
}
