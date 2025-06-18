using Sivar.Erp.Documents;
using System;
using System.Linq;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Represents a tax accounting profile for import/export
    /// </summary>
    public class TaxAccountingProfile
    {
        public string TaxCode { get; set; }
        public DocumentOperation DocumentOperation { get; set; }
        public string DebitAccountCode { get; set; }
        public string CreditAccountCode { get; set; }
        public string AccountDescription { get; set; }
        public bool IncludeInTransaction { get; set; } = true;
    }
}