using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.ImportExport;
using Sivar.Erp.Services.Taxes.TaxGroup;

/// <summary>
/// Test configuration service that loads data using existing import services
/// </summary>
public class TestConfigurationService
{
    private readonly DocumentTypeImportExportService _documentTypeImporter;
    private readonly BusinessEntityImportExportService _businessEntityImporter;
    private readonly GroupMembershipImportExportService _groupMembershipImporter;

    private List<IDocumentType> _documentTypes = new();
    private List<IBusinessEntity> _businessEntities = new();
    private List<GroupMembershipDto> _groupMemberships = new();

    public TestConfigurationService()
    {
        _documentTypeImporter = new DocumentTypeImportExportService();
        _businessEntityImporter = new BusinessEntityImportExportService();
        _groupMembershipImporter = new GroupMembershipImportExportService();
    }

    /// <summary>
    /// Load all test configuration data from CSV files
    /// </summary>
    public async Task LoadTestConfiguration(string dataDirectory)
    {
        await LoadDocumentTypes(Path.Combine(dataDirectory, "ElSalvador_Data_New_DocumentTypes.csv"));
        await LoadBusinessEntities(Path.Combine(dataDirectory, "BusinessEntities.csv"));
        await LoadGroupMemberships(Path.Combine(dataDirectory, "ElSalvador_Data_New_GroupMemberships.csv"));
    }

    /// <summary>
    /// Get document type by code (uses your existing DTO)
    /// </summary>
    public IDocumentType GetDocumentType(string code) =>
        _documentTypes.FirstOrDefault(dt => dt.Code == code && dt.IsEnabled);

    /// <summary>
    /// Get all enabled document types of specific operation
    /// </summary>
    public List<IDocumentType> GetDocumentTypesByOperation(DocumentOperation operation) =>
        _documentTypes.Where(dt => dt.DocumentOperation == operation && dt.IsEnabled).ToList();

    /// <summary>
    /// Get business entities by group (from GroupMemberships CSV using existing service)
    /// </summary>
    public List<IBusinessEntity> GetBusinessEntitiesByGroup(string groupId)
    {
        var entityCodes = _groupMemberships
            .Where(gm => gm.GroupId == groupId && gm.GroupType == GroupType.BusinessEntity)
            .Select(gm => gm.EntityId)
            .ToList();

        return _businessEntities.Where(be => entityCodes.Contains(be.Code)).ToList();
    }

    /// <summary>
    /// Get random business entity from specific group
    /// </summary>
    public IBusinessEntity GetRandomBusinessEntityFromGroup(string groupId)
    {
        var entities = GetBusinessEntitiesByGroup(groupId);
        return entities.Any() ? entities[Random.Shared.Next(entities.Count)] : null;
    }

    private async Task LoadDocumentTypes(string csvPath)
    {
        if (!File.Exists(csvPath)) return;

        var csvContent = await File.ReadAllTextAsync(csvPath);
        var (importedTypes, errors) = await _documentTypeImporter.ImportFromCsvAsync(csvContent, "test-user");

        if (!errors.Any())
        {
            _documentTypes = importedTypes.ToList();
        }
    }

    private async Task LoadBusinessEntities(string csvPath)
    {
        if (!File.Exists(csvPath)) return;

        var csvContent = await File.ReadAllTextAsync(csvPath);
        var (importedEntities, errors) = await _businessEntityImporter.ImportFromCsvAsync(csvContent, "test-user");

        if (!errors.Any())
        {
            _businessEntities = importedEntities.ToList();
        }
    }

    private async Task LoadGroupMemberships(string csvPath)
    {
        if (!File.Exists(csvPath)) return;

        var csvContent = await File.ReadAllTextAsync(csvPath);
        var (importedMemberships, errors) = await _groupMembershipImporter.ImportFromCsvAsync(csvContent, "test-user");

        if (!errors.Any())
        {
            _groupMemberships = importedMemberships.ToList();
        }
    }
}