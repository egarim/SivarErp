using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Sivar.Erp.ImportExport;
using Sivar.Erp.Services.ImportExport;
using Sivar.Erp.Taxes.TaxGroup;

namespace Sivar.Erp.Tests.Taxes
{
    /// <summary>
    /// Tests for the tax group import/export service
    /// </summary>
    [TestFixture]
    public class TaxGroupImportExportServiceTests
    {
        private ITaxGroupImportExportService _importExportService;

        [SetUp]
        public void Setup()
        {
            _importExportService = new TaxGroupImportExportService();
        }

        #region Import Tests

        [Test]
        public async Task ImportFromCsvAsync_EmptyContent_ReturnsError()
        {
            // Arrange
            string csvContent = string.Empty;

            // Act
            var (importedTaxGroups, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(importedTaxGroups, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("empty"));
        }

        [Test]
        public async Task ImportFromCsvAsync_HeaderOnly_ReturnsError()
        {
            // Arrange
            string csvContent = "Code,Name,Description,IsEnabled,GroupType";

            // Act
            var (importedTaxGroups, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(importedTaxGroups, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("no data"));
        }

        [Test]
        public async Task ImportFromCsvAsync_MissingRequiredHeader_ReturnsError()
        {
            // Arrange
            string csvContent = "Name,Description\nRegistered Taxpayers,Companies with tax ID";

            // Act
            var (importedTaxGroups, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(importedTaxGroups, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("Code"));
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidContent_ImportsTaxGroups()
        {
            // Arrange
            string csvContent = "Code,Name,Description,IsEnabled,GroupType\nREGISTERED,Registered Taxpayers,Companies with tax ID,true,BusinessEntity";

            // Act
            var (importedTaxGroups, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedTaxGroups.Count(), Is.EqualTo(1));
            
            var taxGroup = importedTaxGroups.First();
            Assert.That(taxGroup.Code, Is.EqualTo("REGISTERED"));
            Assert.That(taxGroup.Name, Is.EqualTo("Registered Taxpayers"));
            Assert.That(taxGroup.Description, Is.EqualTo("Companies with tax ID"));
            Assert.That(taxGroup.IsEnabled, Is.True);
        }

        [Test]
        public async Task ImportFromCsvAsync_MultipleGroups_ImportsAllGroups()
        {
            // Arrange
            string csvContent = 
                "Code,Name,Description,IsEnabled,GroupType\n" +
                "REGISTERED,Registered Taxpayers,Companies with tax ID,true,BusinessEntity\n" +
                "EXEMPT,Exempt Entities,Entities exempt from taxes,true,BusinessEntity\n" +
                "TAXABLE_ITEMS,Taxable Products,Products subject to tax,true,Item";

            // Act
            var (importedTaxGroups, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedTaxGroups.Count(), Is.EqualTo(3));
            
            var groups = importedTaxGroups.ToList();
            Assert.That(groups[0].Code, Is.EqualTo("REGISTERED"));
            Assert.That(groups[1].Code, Is.EqualTo("EXEMPT"));
            Assert.That(groups[2].Code, Is.EqualTo("TAXABLE_ITEMS"));
        }

        #endregion

        #region Export Tests

        [Test]
        public async Task ExportToCsvAsync_EmptyCollection_ReturnsHeaderOnly()
        {
            // Arrange
            var taxGroups = Array.Empty<ITaxGroup>();

            // Act
            var csvContent = await _importExportService.ExportToCsvAsync(taxGroups);

            // Assert
            Assert.That(csvContent, Is.Not.Empty);
            var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines.Length, Is.EqualTo(1)); // Header only
            Assert.That(lines[0], Does.Contain("Code"));
            Assert.That(lines[0], Does.Contain("Name"));
        }

        [Test]
        public async Task ExportToCsvAsync_WithTaxGroups_ReturnsValidCsv()
        {
            // Arrange
            var taxGroups = new[]
            {
                new TaxGroupDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "REGISTERED",
                    Name = "Registered Taxpayers",
                    Description = "Companies with tax ID",
                    IsEnabled = true
                },
                new TaxGroupDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "EXEMPT",
                    Name = "Exempt Entities",
                    Description = "Entities exempt from taxes",
                    IsEnabled = true
                }
            };

            // Act
            var csvContent = await _importExportService.ExportToCsvAsync(taxGroups);

            // Assert
            Assert.That(csvContent, Is.Not.Empty);
            var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines.Length, Is.EqualTo(3)); // Header + 2 rows
            Assert.That(lines[1], Does.Contain("REGISTERED"));
            Assert.That(lines[1], Does.Contain("Registered Taxpayers"));
            Assert.That(lines[2], Does.Contain("EXEMPT"));
            Assert.That(lines[2], Does.Contain("Exempt Entities"));
        }

        #endregion

        #region Roundtrip Tests

        [Test]
        public async Task RoundTrip_ExportAndImport_PreservesData()
        {
            // Arrange
            var originalTaxGroups = new List<TaxGroupDto>
            {
                new TaxGroupDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "REGISTERED",
                    Name = "Registered Taxpayers",
                    Description = "Companies with tax ID",
                    IsEnabled = true
                },
                new TaxGroupDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "EXEMPT",
                    Name = "Exempt Entities",
                    Description = "Entities exempt from taxes",
                    IsEnabled = true
                },
                new TaxGroupDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "TAXABLE_ITEMS",
                    Name = "Taxable Products",
                    Description = "Products subject to tax",
                    IsEnabled = true
                }
            };

            // Act - Export
            var csvContent = await _importExportService.ExportToCsvAsync(originalTaxGroups);

            // Act - Import
            var (importedTaxGroups, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedTaxGroups.Count(), Is.EqualTo(3));

            var taxGroupDict = importedTaxGroups.ToDictionary(g => g.Code);
            
            // Check that all groups were imported correctly
            Assert.That(taxGroupDict.ContainsKey("REGISTERED"), Is.True);
            Assert.That(taxGroupDict["REGISTERED"].Name, Is.EqualTo("Registered Taxpayers"));
            Assert.That(taxGroupDict["REGISTERED"].Description, Is.EqualTo("Companies with tax ID"));
            
            Assert.That(taxGroupDict.ContainsKey("EXEMPT"), Is.True);
            Assert.That(taxGroupDict["EXEMPT"].Name, Is.EqualTo("Exempt Entities"));
            
            Assert.That(taxGroupDict.ContainsKey("TAXABLE_ITEMS"), Is.True);
            Assert.That(taxGroupDict["TAXABLE_ITEMS"].Name, Is.EqualTo("Taxable Products"));
        }

        #endregion
    }
}