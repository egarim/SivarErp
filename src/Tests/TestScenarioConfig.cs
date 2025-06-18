using Sivar.Erp.Documents;
using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Services.Taxes;
using Sivar.Erp.Services.Taxes.TaxAccountingProfiles;
using Sivar.Erp.Services.Taxes.TaxRule;

public class TestScenarioConfig
{
    public string TestScenarioId { get; set; }
    public string Description { get; set; }
    public string DocumentType { get; set; }
    public string DocumentNumber { get; set; }
    public string BusinessEntityCode { get; set; }
    public string Date { get; set; }
}
