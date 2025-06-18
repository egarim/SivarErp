using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using System.Collections.ObjectModel;

/// <summary>
/// Builds test data using existing DTOs and configuration
/// </summary>
public class TestDataBuilder
{
    private readonly TestConfigurationService _config;

    public TestDataBuilder(TestConfigurationService config)
    {
        _config = config;
    }

    /// <summary>
    /// Create document with specified type (returns your existing DocumentDto)
    /// </summary>
    public DocumentDto CreateTestDocument(string documentTypeCode)
    {
        var documentType = _config.GetDocumentType(documentTypeCode);
        if (documentType == null)
            throw new ArgumentException($"Document type '{documentTypeCode}' not found or disabled");

        var businessEntity = GetBusinessEntityForDocumentType(documentType);

        return new DocumentDto
        {
            Oid = Guid.NewGuid(),
            DocumentNumber = GenerateDocumentNumber(documentType),
            Date = DateOnly.FromDateTime(DateTime.Today),
            Time = TimeOnly.FromDateTime(DateTime.Now),
            DocumentType = documentType,
            BusinessEntity = businessEntity,
            Lines = new ObservableCollection<IDocumentLine>(),
            DocumentTotals = new ObservableCollection<ITotal>()
        };
    }

    /// <summary>
    /// Create test document with lines (uses your existing LineDto)
    /// </summary>
    public DocumentDto CreateTestDocumentWithLines(string documentTypeCode, int lineCount = 2)
    {
        var document = CreateTestDocument(documentTypeCode);

        for (int i = 0; i < lineCount; i++)
        {
            var line = new LineDto
            {
              
                LineNumber = i + 1,
                Description = $"Test Item {i + 1}",
                Quantity = 1,
                UnitPrice = Random.Shared.Next(100, 1000),
                LineTotals = new ObservableCollection<ITotal>()
            };

            document.Lines.Add(line);
        }

        return document;
    }

    /// <summary>
    /// Get appropriate business entity based on document type
    /// </summary>
    private IBusinessEntity GetBusinessEntityForDocumentType(IDocumentType documentType)
    {
        // Use your existing group logic
        return documentType.DocumentOperation switch
        {
            DocumentOperation.SalesInvoice when documentType.Code == "FCF"
                => _config.GetRandomBusinessEntityFromGroup("FINAL_CONSUMERS"),
            DocumentOperation.SalesInvoice when documentType.Code == "CCF"
                => _config.GetRandomBusinessEntityFromGroup("REGISTERED_TAXPAYERS"),
            DocumentOperation.SalesInvoice when documentType.Code == "FEX"
                => _config.GetRandomBusinessEntityFromGroup("REGISTERED_TAXPAYERS"),
            _ => _config.GetRandomBusinessEntityFromGroup("REGISTERED_TAXPAYERS")
        };
    }

    private string GenerateDocumentNumber(IDocumentType documentType)
    {
        return $"{documentType.Code}-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
    }
}