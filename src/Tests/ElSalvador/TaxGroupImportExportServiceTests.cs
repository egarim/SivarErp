using NUnit.Framework;
using Sivar.Erp.Services.ImportExport;
using Sivar.Erp.Services.Taxes.TaxGroup;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.ElSalvador
{
    [TestFixture]
    public class TaxGroupImportExportServiceTests
    {
        private readonly string _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "Tests", "ElSalvador", "Data", "New");
        private ITaxGroupImportExportService _taxGroupImportService;

        [SetUp]
        public void Setup()
        {
            _taxGroupImportService = new TaxGroupImportExportService();
        }

        [Test]
        public async Task ImportElSalvadorTaxGroups_Success()
        {
            // Arrange
            string taxGroupsPath = Path.Combine(_testDataPath, "ElSalvadorTaxGroups.txt");
            string csvContent = await File.ReadAllTextAsync(taxGroupsPath);

            // Act
            var (importedTaxGroups, errors) = await _taxGroupImportService.ImportFromCsvAsync(csvContent, "TaxGroupImportTest");

            // Assert
            Assert.That(errors, Is.Empty, "Tax group import should not have errors");
            Assert.That(importedTaxGroups, Is.Not.Empty, "Should have imported tax groups");
            Assert.That(importedTaxGroups.Count(), Is.EqualTo(10), "Should have imported 10 tax groups");

            // Verify specific tax groups were imported
            var taxGroups = importedTaxGroups.ToList();

            // Business Entity Groups
            var registeredTaxpayersGroup = taxGroups.FirstOrDefault(g => g.Code == "REGISTERED_TAXPAYERS");
            Assert.That(registeredTaxpayersGroup, Is.Not.Null, "Should have imported REGISTERED_TAXPAYERS group");
            Assert.That(registeredTaxpayersGroup.Name, Is.EqualTo("Contribuyentes Registrados"));
            Assert.That(registeredTaxpayersGroup.Description, Is.EqualTo("Entidades registradas con NRC (Número de Registro de Contribuyente)"));
            Assert.That(registeredTaxpayersGroup.IsEnabled, Is.True);

            var finalConsumersGroup = taxGroups.FirstOrDefault(g => g.Code == "FINAL_CONSUMERS");
            Assert.That(finalConsumersGroup, Is.Not.Null, "Should have imported FINAL_CONSUMERS group");
            Assert.That(finalConsumersGroup.Name, Is.EqualTo("Consumidores Finales"));

            var exemptEntitiesGroup = taxGroups.FirstOrDefault(g => g.Code == "EXEMPT_ENTITIES");
            Assert.That(exemptEntitiesGroup, Is.Not.Null, "Should have imported EXEMPT_ENTITIES group");
            Assert.That(exemptEntitiesGroup.Name, Is.EqualTo("Entidades Exentas"));

            var governmentGroup = taxGroups.FirstOrDefault(g => g.Code == "GOVERNMENT");
            Assert.That(governmentGroup, Is.Not.Null, "Should have imported GOVERNMENT group");
            Assert.That(governmentGroup.Name, Is.EqualTo("Entidades de Gobierno"));

            var smallTaxpayersGroup = taxGroups.FirstOrDefault(g => g.Code == "SMALL_TAXPAYERS");
            Assert.That(smallTaxpayersGroup, Is.Not.Null, "Should have imported SMALL_TAXPAYERS group");
            Assert.That(smallTaxpayersGroup.Name, Is.EqualTo("Pequeños Contribuyentes"));

            // Item Groups
            var taxableItemsGroup = taxGroups.FirstOrDefault(g => g.Code == "TAXABLE_ITEMS");
            Assert.That(taxableItemsGroup, Is.Not.Null, "Should have imported TAXABLE_ITEMS group");
            Assert.That(taxableItemsGroup.Name, Is.EqualTo("Artículos Gravados"));

            var exemptItemsGroup = taxGroups.FirstOrDefault(g => g.Code == "EXEMPT_ITEMS");
            Assert.That(exemptItemsGroup, Is.Not.Null, "Should have imported EXEMPT_ITEMS group");
            Assert.That(exemptItemsGroup.Name, Is.EqualTo("Artículos Exentos"));

            var fuelItemsGroup = taxGroups.FirstOrDefault(g => g.Code == "FUEL_ITEMS");
            Assert.That(fuelItemsGroup, Is.Not.Null, "Should have imported FUEL_ITEMS group");
            Assert.That(fuelItemsGroup.Name, Is.EqualTo("Combustibles"));

            var telecomItemsGroup = taxGroups.FirstOrDefault(g => g.Code == "TELECOM_ITEMS");
            Assert.That(telecomItemsGroup, Is.Not.Null, "Should have imported TELECOM_ITEMS group");
            Assert.That(telecomItemsGroup.Name, Is.EqualTo("Telecomunicaciones"));

            var tourismItemsGroup = taxGroups.FirstOrDefault(g => g.Code == "TOURISM_ITEMS");
            Assert.That(tourismItemsGroup, Is.Not.Null, "Should have imported TOURISM_ITEMS group");
            Assert.That(tourismItemsGroup.Name, Is.EqualTo("Turismo"));
        }

        [Test]
        public async Task ExportAndImportElSalvadorTaxGroups_RoundtripPreservesData()
        {
            // Arrange: First import the tax groups
            string taxGroupsPath = Path.Combine(_testDataPath, "ElSalvadorTaxGroups.txt");
            string originalCsvContent = await File.ReadAllTextAsync(taxGroupsPath);
            
            var (originalImportedGroups, importErrors) = await _taxGroupImportService.ImportFromCsvAsync(originalCsvContent, "TaxGroupRoundtripTest");
            Assert.That(importErrors, Is.Empty, "Initial import should not have errors");

            // Act: Export the imported groups
            string exportedCsv = await _taxGroupImportService.ExportToCsvAsync(originalImportedGroups);
            
            // Act: Re-import the exported CSV
            var (reimportedGroups, reimportErrors) = await _taxGroupImportService.ImportFromCsvAsync(exportedCsv, "TaxGroupRoundtripTest");
            
            // Assert
            Assert.That(reimportErrors, Is.Empty, "Re-import should not have errors");
            Assert.That(reimportedGroups.Count(), Is.EqualTo(originalImportedGroups.Count()), "Should have same number of groups after roundtrip");
            
            // Verify all tax groups were preserved correctly
            var originalDict = originalImportedGroups.ToDictionary(g => g.Code);
            var reimportedDict = reimportedGroups.ToDictionary(g => g.Code);
            
            foreach (var code in originalDict.Keys)
            {
                Assert.That(reimportedDict.ContainsKey(code), Is.True, $"Reimported data should contain {code}");
                Assert.That(reimportedDict[code].Name, Is.EqualTo(originalDict[code].Name), $"Name should match for {code}");
                Assert.That(reimportedDict[code].Description, Is.EqualTo(originalDict[code].Description), $"Description should match for {code}");
                Assert.That(reimportedDict[code].IsEnabled, Is.EqualTo(originalDict[code].IsEnabled), $"IsEnabled should match for {code}");
            }
        }
    }
}