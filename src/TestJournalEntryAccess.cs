using Sivar.Erp.Core.Application.Services.Accounting;

namespace TestAccess;

// Test if we can access IJournalEntryService from outside the Application project
public class TestClass
{
    private IJournalEntryService? _service;
    
    public void TestMethod()
    {
        // This will compile if the interface is accessible
    }
}
