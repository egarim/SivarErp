using NUnit.Framework;
using Sivar.Erp.Modules;
using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.ImportExport;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Modules
{
    [TestFixture]
    public class DataImportHelperTests
    {
        private readonly string _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "Tests", "ElSalvador", "Data", "New");
        private IAccountImportExportService _accountImportService;
        private ITaxImportExportService _taxImportService;
        private ITaxGroupImportExportService _taxGroupImportService;
        private IBusinessEntityImportExportService _businessEntityImportService;
        private IItemImportExportService _itemImportService;
        private IDocumentTypeImportExportService _documentTypeImportService;
        private ITaxRuleImportExportService _taxRuleImportService;
        private IGroupMembershipImportExportService _groupMembershipImportService;
        private DataImportHelper _dataImportHelper;
        private ObjectDb _objectDb;

        [SetUp]
        public void Setup()
        {
            // Create all required import/export services
            var elSalvadorAccountValidator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
            _accountImportService = new AccountImportExportService(elSalvadorAccountValidator);
            _taxImportService = new TaxImportExportService();
            _taxGroupImportService = new TaxGroupImportExportService();
            _businessEntityImportService = new BusinessEntityImportExportService();
            _itemImportService = new ItemImportExportService();
            _documentTypeImportService = new DocumentTypeImportExportService();
            _groupMembershipImportService = new GroupMembershipImportExportService();

            // Create the DataImportHelper instance
            _dataImportHelper = new DataImportHelper(
                _accountImportService,
                _taxImportService,
                _taxGroupImportService,
                _documentTypeImportService,
                _businessEntityImportService,
                _itemImportService,
                _groupMembershipImportService,
                _taxRuleImportService,
                "DataImportHelperTest");

            // Create an empty ObjectDb
            _objectDb = new ObjectDb();
        }

        [Test]
        public async Task ImportAllDataAsync_WithValidDataDirectory_ImportsAllData()
        {
            // Arrange
            Assert.That(Directory.Exists(_testDataPath), Is.True, $"Test data directory not found: {_testDataPath}");

            // Act
            var results = await _dataImportHelper.ImportAllDataAsync(_objectDb, _testDataPath);

            // Assert
            // 1. Verify results dictionary contains entries for all expected files
            Assert.That(results.ContainsKey("ComercialChartOfAccounts.txt"), Is.True, "Chart of Accounts import missing from results");
            Assert.That(results.ContainsKey("ElSalvadorTaxGroups.txt"), Is.True, "Tax Groups import missing from results");
            Assert.That(results.ContainsKey("ElSalvadorTaxes.txt"), Is.True, "Taxes import missing from results");
            Assert.That(results.ContainsKey("BusinesEntities.txt"), Is.True, "Business Entities import missing from results");
            Assert.That(results.ContainsKey("Items.txt"), Is.True, "Items import missing from results");
            Assert.That(results.ContainsKey("GroupMemberships.csv"), Is.True, "Group Memberships import missing from results");
            Assert.That(results.ContainsKey("DocumentTypes.csv"), Is.True, "Document Types import missing from results");
            
            // 2. Check for success messages (not errors) in results
            foreach (var fileResults in results)
            {
                Assert.That(fileResults.Value.Any(m => m.StartsWith("Successfully")), Is.True, 
                    $"Import of {fileResults.Key} did not complete successfully: {string.Join(", ", fileResults.Value)}");
            }

            // 3. Verify ObjectDb now contains data
            Assert.That(_objectDb.Accounts, Is.Not.Empty, "No accounts were imported");
            Assert.That(_objectDb.TaxGroups, Is.Not.Empty, "No tax groups were imported");
            Assert.That(_objectDb.Taxes, Is.Not.Empty, "No taxes were imported");
            Assert.That(_objectDb.BusinessEntities, Is.Not.Empty, "No business entities were imported");
            Assert.That(_objectDb.Items, Is.Not.Empty, "No items were imported");
            Assert.That(_objectDb.GroupMemberships, Is.Not.Empty, "No group memberships were imported");
            Assert.That(_objectDb.DocumentTypes, Is.Not.Empty, "No document types were imported");

            // 4. Verify specific counts based on expected data in files
            Assert.That(_objectDb.Accounts.Count, Is.GreaterThan(10), "Too few accounts imported");
            Assert.That(_objectDb.TaxGroups.Count, Is.GreaterThan(5), "Too few tax groups imported");
            Assert.That(_objectDb.Taxes.Count, Is.GreaterThan(2), "Too few taxes imported");
            Assert.That(_objectDb.BusinessEntities.Count, Is.GreaterThan(2), "Too few business entities imported");
            Assert.That(_objectDb.Items.Count, Is.GreaterThan(2), "Too few items imported");
            Assert.That(_objectDb.DocumentTypes.Count, Is.EqualTo(8), "Incorrect number of document types imported");
        }

        [Test]
        public async Task ImportAllDataAsync_WithInvalidDirectory_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            string invalidDirectory = Path.Combine(_testDataPath, "DoesNotExist");
            
            // Act & Assert
            var ex = Assert.ThrowsAsync<DirectoryNotFoundException>(async () => 
                await _dataImportHelper.ImportAllDataAsync(_objectDb, invalidDirectory));
            
            Assert.That(ex.Message, Does.Contain("not found"));
        }
        
        [Test]
        public async Task ImportAllDataAsync_WithNullObjectDb_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _dataImportHelper.ImportAllDataAsync(null, _testDataPath));
            
            Assert.That(ex.ParamName, Is.EqualTo("objectDb"));
        }

        [Test]
        public async Task ImportAllDataAsync_WithNullOrEmptyDirectory_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
                await _dataImportHelper.ImportAllDataAsync(_objectDb, string.Empty));
            
            Assert.That(ex.ParamName, Is.EqualTo("dataDirectory"));
        }
    }
}