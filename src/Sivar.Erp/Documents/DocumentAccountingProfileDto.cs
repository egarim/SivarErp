using System;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// DTO implementation of IDocumentAccountingProfile
    /// </summary>
    public class DocumentAccountingProfileDto : IDocumentAccountingProfile
    {
        public Guid Oid { get; set; } = Guid.NewGuid();
        public string DocumentOperation { get; set; }
        public string SalesAccountCode { get; set; }
        public string AccountsReceivableCode { get; set; }
        public string CostOfGoodsSoldAccountCode { get; set; }
        public string InventoryAccountCode { get; set; }
        public decimal CostRatio { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
    }
}