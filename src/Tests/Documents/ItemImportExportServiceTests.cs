using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;
using Sivar.Erp.Documents;

using Sivar.Erp.Services.ImportExport;

namespace Tests.Documents
{
    /// <summary>
    /// Tests for the item import/export service
    /// </summary>
    [TestFixture]
    public class ItemImportExportServiceTests
    {
        private IItemImportExportService _importExportService;

        [SetUp]
        public void Setup()
        {
            _importExportService = new ItemImportExportService();
        }

        #region Import Tests

        [Test]
        public async Task ImportFromCsvAsync_EmptyContent_ReturnsError()
        {
            // Arrange
            string csvContent = string.Empty;

            // Act
            var (importedItems, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedItems, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("empty"));
        }

        [Test]
        public async Task ImportFromCsvAsync_HeaderOnly_ReturnsError()
        {
            // Arrange
            string csvContent = "Code,Type,Description,BasePrice";

            // Act
            var (importedItems, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedItems, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("no data"));
        }

        [Test]
        public async Task ImportFromCsvAsync_MissingRequiredHeader_ReturnsError()
        {
            // Arrange
            string csvContent = "ItemCode,ItemType,ItemDesc\nIT001,Product,Computer";

            // Act
            var (importedItems, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedItems, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.Any(e => e.Contains("Code") || e.Contains("Type") || 
                                       e.Contains("Description") || e.Contains("BasePrice")), Is.True);
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidContent_ImportsItems()
        {
            // Arrange
            string csvContent = "Code,Type,Description,BasePrice\nIT001,Product,Computer,1000\nIT002,Service,Maintenance,250.50";

            // Act
            var (importedItems, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedItems.Count(), Is.EqualTo(2));
            Assert.That(importedItems.First().Code, Is.EqualTo("IT001"));
            Assert.That(importedItems.First().Type, Is.EqualTo("Product"));
            Assert.That(importedItems.First().Description, Is.EqualTo("Computer"));
            Assert.That(importedItems.First().BasePrice, Is.EqualTo(1000));

            var secondItem = importedItems.Last();
            Assert.That(secondItem.Code, Is.EqualTo("IT002"));
            Assert.That(secondItem.Type, Is.EqualTo("Service"));
            Assert.That(secondItem.BasePrice, Is.EqualTo(250.50m));
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidCsvWithQuotes_ImportsItems()
        {
            // Arrange
            string csvContent = "\"Code\",\"Type\",\"Description\",\"BasePrice\"\n\"IT001\",\"Product\",\"Computer, Desktop\",\"1000\"\n\"IT002\",\"Service\",\"Maintenance, Monthly\",\"250.50\"";

            // Act
            var (importedItems, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedItems.Count(), Is.EqualTo(2));
            Assert.That(importedItems.First().Description, Is.EqualTo("Computer, Desktop"));
            Assert.That(importedItems.Last().Description, Is.EqualTo("Maintenance, Monthly"));
        }

        [Test]
        public async Task ImportFromCsvAsync_InvalidPrice_UsesDefault()
        {
            // Arrange
            string csvContent = "Code,Type,Description,BasePrice\nIT001,Product,Computer,NotAPrice";

            // Act
            var (importedItems, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedItems.Count(), Is.EqualTo(1));
            Assert.That(importedItems.First().BasePrice, Is.EqualTo(0));
        }

        #endregion

        #region Export Tests

        [Test]
        public async Task ExportToCsvAsync_NullItems_ReturnsHeaderOnly()
        {
            // Act
            string csv = await _importExportService.ExportToCsvAsync(null!);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("Code"));
            Assert.That(csv, Does.Contain("Type"));
            Assert.That(csv, Does.Contain("Description"));
            Assert.That(csv, Does.Contain("BasePrice"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(1));
        }

        [Test]
        public async Task ExportToCsvAsync_EmptyItems_ReturnsHeaderOnly()
        {
            // Act
            string csv = await _importExportService.ExportToCsvAsync(Array.Empty<IItem>());

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("Code"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(1));
        }

        [Test]
        public async Task ExportToCsvAsync_WithItems_ReturnsCsvContent()
        {
            // Arrange
            var items = new List<IItem>
            {
                new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "IT001",
                    Type = "Product",
                    Description = "Computer",
                    BasePrice = 1000.00m
                },
                new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "IT002",
                    Type = "Service",
                    Description = "Maintenance",
                    BasePrice = 250.50m
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(items);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("Code"));
            Assert.That(csv, Does.Contain("Type"));
            Assert.That(csv, Does.Contain("IT001"));
            Assert.That(csv, Does.Contain("IT002"));
            Assert.That(csv, Does.Contain("Product"));
            Assert.That(csv, Does.Contain("Service"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(3)); // Header + 2 data rows
        }

        [Test]
        public async Task ExportToCsvAsync_WithCommasInData_QuotesValues()
        {
            // Arrange
            var items = new List<IItem>
            {
                new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "IT001",
                    Type = "Product",
                    Description = "Computer, Desktop",
                    BasePrice = 1000.00m
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(items);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("\"Computer, Desktop\""));
        }

        #endregion

        #region Round Trip Tests

        [Test]
        public async Task RoundTrip_ExportThenImport_PreservesItems()
        {
            // Arrange
            var originalItems = new List<IItem>
            {
                new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "IT001",
                    Type = "Product",
                    Description = "Computer",
                    BasePrice = 1000.00m
                },
                new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "IT002",
                    Type = "Service",
                    Description = "Maintenance",
                    BasePrice = 250.50m
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(originalItems);
            var (importedItems, errors) = await _importExportService.ImportFromCsvAsync(csv, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedItems.Count(), Is.EqualTo(originalItems.Count));

            // Check first item
            var firstOriginal = originalItems[0];
            var firstImported = importedItems.First();
            Assert.That(firstImported.Code, Is.EqualTo(firstOriginal.Code));
            Assert.That(firstImported.Type, Is.EqualTo(firstOriginal.Type));
            Assert.That(firstImported.Description, Is.EqualTo(firstOriginal.Description));
            Assert.That(firstImported.BasePrice, Is.EqualTo(firstOriginal.BasePrice));

            // Check second item
            var secondOriginal = originalItems[1];
            var secondImported = importedItems.Skip(1).First();
            Assert.That(secondImported.Code, Is.EqualTo(secondOriginal.Code));
            Assert.That(secondImported.Type, Is.EqualTo(secondOriginal.Type));
            Assert.That(secondImported.Description, Is.EqualTo(secondOriginal.Description));
            Assert.That(secondImported.BasePrice, Is.EqualTo(secondOriginal.BasePrice));
        }

        [Test]
        public async Task RoundTrip_ExportThenImport_PreservesComplexDescription()
        {
            // Arrange
            var originalItems = new List<IItem>
            {
                new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "IT001",
                    Type = "Product",
                    Description = "Computer, Desktop with monitor, keyboard, and mouse",
                    BasePrice = 1500.00m
                },
                new ItemDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "IT002",
                    Type = "Service",
                    Description = "Support, Extended 24/7",
                    BasePrice = 350.00m
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(originalItems);
            var (importedItems, errors) = await _importExportService.ImportFromCsvAsync(csv, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedItems.Count(), Is.EqualTo(originalItems.Count));

            var importedList = importedItems.ToList();
            Assert.That(importedList[0].Description, Is.EqualTo(originalItems[0].Description));
            Assert.That(importedList[1].Description, Is.EqualTo(originalItems[1].Description));
        }

        #endregion
    }
}