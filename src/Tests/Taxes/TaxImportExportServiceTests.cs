using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

using Sivar.Erp.Services.ImportExport;
using Sivar.Erp.Services.Taxes;

namespace Tests.Taxes
{
    /// <summary>
    /// Tests for the tax import/export service
    /// </summary>
    [TestFixture]
    public class TaxImportExportServiceTests
    {
        private ITaxImportExportService _importExportService;

        [SetUp]
        public void Setup()
        {
            _importExportService = new TaxImportExportService();
        }

        #region Import Tests

        [Test]
        public async Task ImportFromCsvAsync_EmptyContent_ReturnsError()
        {
            // Arrange
            string csvContent = string.Empty;

            // Act
            var (importedTaxes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(importedTaxes, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("empty"));
        }

        [Test]
        public async Task ImportFromCsvAsync_HeaderOnly_ReturnsError()
        {
            // Arrange
            string csvContent = "Code,Name,TaxType,ApplicationLevel,Percentage,Amount,IsEnabled,IsIncludedInPrice";

            // Act
            var (importedTaxes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(importedTaxes, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("no data"));
        }

        [Test]
        public async Task ImportFromCsvAsync_MissingRequiredHeader_ReturnsError()
        {
            // Arrange
            string csvContent = "Code,Name,TaxType\nIVA,Value Added Tax,Percentage";

            // Act
            var (importedTaxes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(importedTaxes, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("ApplicationLevel"));
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidContent_ImportsTaxes()
        {
            // Arrange
            string csvContent = "Code,Name,TaxType,ApplicationLevel,Percentage,Amount\nIVA,Value Added Tax,Percentage,Line,13.00,\nISR,Income Tax,Percentage,Document,10.00,";

            // Act
            var (importedTaxes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedTaxes.Count(), Is.EqualTo(2));
            
            var firstTax = importedTaxes.First();
            Assert.That(firstTax.Code, Is.EqualTo("IVA"));
            Assert.That(firstTax.Name, Is.EqualTo("Value Added Tax"));
            Assert.That(firstTax.TaxType, Is.EqualTo(TaxType.Percentage));
            Assert.That(firstTax.ApplicationLevel, Is.EqualTo(TaxApplicationLevel.Line));
            Assert.That(firstTax.Percentage, Is.EqualTo(13.00m));
            
            var secondTax = importedTaxes.ElementAt(1);
            Assert.That(secondTax.Code, Is.EqualTo("ISR"));
            Assert.That(secondTax.ApplicationLevel, Is.EqualTo(TaxApplicationLevel.Document));
        }

        [Test]
        public async Task ImportFromCsvAsync_WithFixedAmountTax_ImportsTaxes()
        {
            // Arrange
            string csvContent = "Code,Name,TaxType,ApplicationLevel,Percentage,Amount\nFEE,Service Fee,FixedAmount,Line,,5.00";

            // Act
            var (importedTaxes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedTaxes.Count(), Is.EqualTo(1));
            
            var tax = importedTaxes.First();
            Assert.That(tax.Code, Is.EqualTo("FEE"));
            Assert.That(tax.TaxType, Is.EqualTo(TaxType.FixedAmount));
            Assert.That(tax.Amount, Is.EqualTo(5.00m));
        }

        #endregion

        #region Export Tests

        [Test]
        public async Task ExportToCsvAsync_EmptyCollection_ReturnsHeaderOnly()
        {
            // Arrange
            var taxes = Array.Empty<TaxDto>();

            // Act
            var csvContent = await _importExportService.ExportToCsvAsync(taxes);

            // Assert
            Assert.That(csvContent, Is.Not.Empty);
            var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines.Length, Is.EqualTo(1)); // Header only
            Assert.That(lines[0], Does.Contain("Code"));
            Assert.That(lines[0], Does.Contain("Name"));
        }

        [Test]
        public async Task ExportToCsvAsync_WithTaxes_ReturnsValidCsv()
        {
            // Arrange
            var taxes = new[]
            {
                new TaxDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "IVA",
                    Name = "Value Added Tax",
                    TaxType = TaxType.Percentage,
                    ApplicationLevel = TaxApplicationLevel.Line,
                    Percentage = 13.00m,
                    IsEnabled = true,
                    IsIncludedInPrice = false
                },
                new TaxDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "ISR",
                    Name = "Income Tax",
                    TaxType = TaxType.Percentage,
                    ApplicationLevel = TaxApplicationLevel.Document,
                    Percentage = 10.00m,
                    IsEnabled = true,
                    IsIncludedInPrice = false
                }
            };

            // Act
            var csvContent = await _importExportService.ExportToCsvAsync(taxes);

            // Assert
            Assert.That(csvContent, Is.Not.Empty);
            var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines.Length, Is.EqualTo(3)); // Header + 2 rows
            Assert.That(lines[1], Does.Contain("IVA"));
            Assert.That(lines[1], Does.Contain("Value Added Tax"));
            Assert.That(lines[2], Does.Contain("ISR"));
            Assert.That(lines[2], Does.Contain("Income Tax"));
        }

        [Test]
        public async Task ExportToCsvAsync_WithFixedAmountTax_IncludesAmount()
        {
            // Arrange
            var taxes = new[]
            {
                new TaxDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "FEE",
                    Name = "Service Fee",
                    TaxType = TaxType.FixedAmount,
                    ApplicationLevel = TaxApplicationLevel.Line,
                    Amount = 5.00m,
                    IsEnabled = true,
                    IsIncludedInPrice = false
                }
            };

            // Act
            var csvContent = await _importExportService.ExportToCsvAsync(taxes);

            // Assert
            Assert.That(csvContent, Is.Not.Empty);
            var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines.Length, Is.EqualTo(2)); // Header + 1 row
            Assert.That(lines[1], Does.Contain("FEE"));
            Assert.That(lines[1], Does.Contain("FixedAmount"));
            Assert.That(lines[1], Does.Contain("5"));
        }

        #endregion

        #region Roundtrip Tests

        [Test]
        public async Task RoundTrip_ExportAndImport_PreservesData()
        {
            // Arrange
            var originalTaxes = new[]
            {
                new TaxDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "IVA",
                    Name = "Value Added Tax",
                    TaxType = TaxType.Percentage,
                    ApplicationLevel = TaxApplicationLevel.Line,
                    Percentage = 13.00m,
                    IsEnabled = true,
                    IsIncludedInPrice = false
                },
                new TaxDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "ISR",
                    Name = "Income Tax",
                    TaxType = TaxType.Percentage,
                    ApplicationLevel = TaxApplicationLevel.Document,
                    Percentage = 10.00m,
                    IsEnabled = true,
                    IsIncludedInPrice = false
                },
                new TaxDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "FEE",
                    Name = "Service Fee",
                    TaxType = TaxType.FixedAmount,
                    ApplicationLevel = TaxApplicationLevel.Line,
                    Amount = 5.00m,
                    IsEnabled = true,
                    IsIncludedInPrice = false
                }
            };

            // Act - Export
            var csvContent = await _importExportService.ExportToCsvAsync(originalTaxes);

            // Act - Import
            var (importedTaxes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedTaxes.Count(), Is.EqualTo(3));

            var taxLookup = importedTaxes.ToDictionary(t => t.Code);
            
            // Check IVA
            Assert.That(taxLookup.ContainsKey("IVA"), Is.True);
            Assert.That(taxLookup["IVA"].Name, Is.EqualTo("Value Added Tax"));
            Assert.That(taxLookup["IVA"].TaxType, Is.EqualTo(TaxType.Percentage));
            Assert.That(taxLookup["IVA"].Percentage, Is.EqualTo(13.00m));

            // Check ISR
            Assert.That(taxLookup.ContainsKey("ISR"), Is.True);
            Assert.That(taxLookup["ISR"].ApplicationLevel, Is.EqualTo(TaxApplicationLevel.Document));
            Assert.That(taxLookup["ISR"].Percentage, Is.EqualTo(10.00m));

            // Check FEE
            Assert.That(taxLookup.ContainsKey("FEE"), Is.True);
            Assert.That(taxLookup["FEE"].TaxType, Is.EqualTo(TaxType.FixedAmount));
            Assert.That(taxLookup["FEE"].Amount, Is.EqualTo(5.00m));
        }

        #endregion
    }
}