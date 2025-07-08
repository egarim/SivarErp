using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp;

namespace Sivar.Erp.EfCore.Entities.Core
{
    [Table("Accounts")]
    public class Account : IAccount
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();

        public DateTime InsertedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string InsertedBy { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;

        public Guid? BalanceAndIncomeLineId { get; set; }

        [Required]
        [MaxLength(200)]
        public string AccountName { get; set; } = string.Empty;

        public AccountType AccountType { get; set; }

        [Required]
        [MaxLength(20)]
        public string OfficialCode { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ParentOfficialCode { get; set; }
    }
}
