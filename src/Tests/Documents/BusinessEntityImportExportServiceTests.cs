using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;
using Sivar.Erp.Services.ImportExport;
using Sivar.Erp.BusinesEntities;

namespace Tests.Documents
{
    /// <summary>
    /// Tests for the business entity import/export service
    /// </summary>
    [TestFixture]
    public class BusinessEntityImportExportServiceTests
    {
        private IBusinessEntityImportExportService _importExportService;

        [SetUp]
        public void Setup()
        {
            _importExportService = new BusinessEntityImportExportService();
        }

        #region Import Tests

        [Test]
        public async Task ImportFromCsvAsync_EmptyContent_ReturnsError()
        {
            // Arrange
            string csvContent = string.Empty;

            // Act
            var (importedBusinessEntities, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedBusinessEntities, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("empty"));
        }

        [Test]
        public async Task ImportFromCsvAsync_HeaderOnly_ReturnsError()
        {
            // Arrange
            string csvContent = "Code,Name,Address,City,State,ZipCode,Country,PhoneNumber,Email";

            // Act
            var (importedBusinessEntities, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedBusinessEntities, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("no data"));
        }

        [Test]
        public async Task ImportFromCsvAsync_MissingRequiredHeader_ReturnsError()
        {
            // Arrange
            string csvContent = "ID,Code,Address,City\nBE001,Acme Inc,123 Main St,New York";

            // Act
            var (importedBusinessEntities, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedBusinessEntities, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("Name"));
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidContent_ImportsBusinessEntities()
        {
            // Arrange
            string csvContent = "Code,Name,Address,City,State,ZipCode,Country,PhoneNumber,Email\n" +
                               "BE001,Acme Inc,123 Main St,New York,NY,10001,USA,+1 212 555 1234,info@acme.com\n" +
                               "BE002,TechCorp,456 Park Ave,San Francisco,CA,94101,USA,+1 415 555 6789,contact@techcorp.com";

            // Act
            var (importedBusinessEntities, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedBusinessEntities.Count(), Is.EqualTo(2));
            Assert.That(importedBusinessEntities.First().Code, Is.EqualTo("BE001"));
            Assert.That(importedBusinessEntities.First().Name, Is.EqualTo("Acme Inc"));
            Assert.That(importedBusinessEntities.First().Address, Is.EqualTo("123 Main St"));
            Assert.That(importedBusinessEntities.First().Email, Is.EqualTo("info@acme.com"));

            var secondEntity = importedBusinessEntities.Last();
            Assert.That(secondEntity.Code, Is.EqualTo("BE002"));
            Assert.That(secondEntity.Name, Is.EqualTo("TechCorp"));
            Assert.That(secondEntity.City, Is.EqualTo("San Francisco"));
            Assert.That(secondEntity.State, Is.EqualTo("CA"));
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidCsvWithQuotes_ImportsBusinessEntities()
        {
            // Arrange
            string csvContent = "\"Code\",\"Name\",\"Address\",\"City\",\"State\",\"ZipCode\",\"Country\",\"PhoneNumber\",\"Email\"\n" +
                               "\"BE001\",\"Acme, Inc.\",\"123 Main St, Suite 100\",\"New York\",\"NY\",\"10001\",\"USA\",\"+1 212 555 1234\",\"info@acme.com\"";

            // Act
            var (importedBusinessEntities, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedBusinessEntities.Count(), Is.EqualTo(1));
            Assert.That(importedBusinessEntities.First().Name, Is.EqualTo("Acme, Inc."));
            Assert.That(importedBusinessEntities.First().Address, Is.EqualTo("123 Main St, Suite 100"));
        }

        [Test]
        public async Task ImportFromCsvAsync_InvalidEmail_ReturnsValidationError()
        {
            // Arrange
            string csvContent = "Code,Name,Email\nBE001,Acme Inc,invalid-email";

            // Act
            var (importedBusinessEntities, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedBusinessEntities, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("validation failed"));
        }

        [Test]
        public async Task ImportFromCsvAsync_MissingOptionalFields_ImportsSuccessfully()
        {
            // Arrange
            string csvContent = "Code,Name\nBE001,Acme Inc";

            // Act
            var (importedBusinessEntities, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedBusinessEntities.Count(), Is.EqualTo(1));
            Assert.That(importedBusinessEntities.First().Code, Is.EqualTo("BE001"));
            Assert.That(importedBusinessEntities.First().Name, Is.EqualTo("Acme Inc"));
            Assert.That(importedBusinessEntities.First().Address, Is.Null);
            Assert.That(importedBusinessEntities.First().Email, Is.Null);
        }

        #endregion

        #region Export Tests

        [Test]
        public async Task ExportToCsvAsync_NullBusinessEntities_ReturnsHeaderOnly()
        {
            // Act
            string csv = await _importExportService.ExportToCsvAsync(null!);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("Code"));
            Assert.That(csv, Does.Contain("Name"));
            Assert.That(csv, Does.Contain("Email"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(1));
        }

        [Test]
        public async Task ExportToCsvAsync_EmptyBusinessEntities_ReturnsHeaderOnly()
        {
            // Act
            string csv = await _importExportService.ExportToCsvAsync(Array.Empty<IBusinessEntity>());

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("Code"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(1));
        }

        [Test]
        public async Task ExportToCsvAsync_WithBusinessEntities_ReturnsCsvContent()
        {
            // Arrange
            var businessEntities = new List<IBusinessEntity>
            {
                new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "BE001",
                    Name = "Acme Inc",
                    Address = "123 Main St",
                    City = "New York",
                    State = "NY", 
                    ZipCode = "10001",
                    Country = "USA",
                    PhoneNumber = "+1 212 555 1234",
                    Email = "info@acme.com"
                },
                new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "BE002",
                    Name = "TechCorp",
                    Address = "456 Park Ave",
                    City = "San Francisco",
                    State = "CA",
                    ZipCode = "94101",
                    Country = "USA",
                    PhoneNumber = "+1 415 555 6789",
                    Email = "contact@techcorp.com"
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(businessEntities);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("Code"));
            Assert.That(csv, Does.Contain("Name"));
            Assert.That(csv, Does.Contain("BE001"));
            Assert.That(csv, Does.Contain("BE002"));
            Assert.That(csv, Does.Contain("Acme Inc"));
            Assert.That(csv, Does.Contain("TechCorp"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(3)); // Header + 2 data rows
        }

        [Test]
        public async Task ExportToCsvAsync_WithCommasInData_QuotesValues()
        {
            // Arrange
            var businessEntities = new List<IBusinessEntity>
            {
                new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "BE001",
                    Name = "Acme, Inc.",
                    Address = "123 Main St, Suite 100",
                    City = "New York",
                    Email = "info@acme.com"
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(businessEntities);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("\"Acme, Inc.\""));
            Assert.That(csv, Does.Contain("\"123 Main St, Suite 100\""));
        }

        [Test]
        public async Task ExportToCsvAsync_WithNullFields_HandlesNullValues()
        {
            // Arrange
            var businessEntities = new List<IBusinessEntity>
            {
                new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "BE001",
                    Name = "Acme Inc",
                    // Other fields are null
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(businessEntities);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("BE001"));
            Assert.That(csv, Does.Contain("Acme Inc"));
            // Make sure the row has 8 commas (9 fields total)
            var dataRow = csv.Trim().Split('\n')[1];
            Assert.That(dataRow.Count(c => c == ','), Is.EqualTo(8));
        }

        #endregion

        #region Round Trip Tests

        [Test]
        public async Task RoundTrip_ExportThenImport_PreservesBusinessEntities()
        {
            // Arrange
            var originalBusinessEntities = new List<IBusinessEntity>
            {
                new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "BE001",
                    Name = "Acme Inc",
                    Address = "123 Main St",
                    City = "New York",
                    State = "NY",
                    ZipCode = "10001",
                    Country = "USA",
                    PhoneNumber = "+1 212 555 1234",
                    Email = "info@acme.com"
                },
                new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "BE002",
                    Name = "TechCorp",
                    Address = "456 Park Ave",
                    City = "San Francisco",
                    State = "CA",
                    ZipCode = "94101",
                    Country = "USA",
                    PhoneNumber = "+1 415 555 6789",
                    Email = "contact@techcorp.com"
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(originalBusinessEntities);
            var (importedBusinessEntities, errors) = await _importExportService.ImportFromCsvAsync(csv, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedBusinessEntities.Count(), Is.EqualTo(originalBusinessEntities.Count));

            // Check first business entity
            var firstOriginal = originalBusinessEntities[0];
            var firstImported = importedBusinessEntities.First();
            Assert.That(firstImported.Code, Is.EqualTo(firstOriginal.Code));
            Assert.That(firstImported.Name, Is.EqualTo(firstOriginal.Name));
            Assert.That(firstImported.Address, Is.EqualTo(firstOriginal.Address));
            Assert.That(firstImported.City, Is.EqualTo(firstOriginal.City));
            Assert.That(firstImported.State, Is.EqualTo(firstOriginal.State));
            Assert.That(firstImported.ZipCode, Is.EqualTo(firstOriginal.ZipCode));
            Assert.That(firstImported.Country, Is.EqualTo(firstOriginal.Country));
            Assert.That(firstImported.PhoneNumber, Is.EqualTo(firstOriginal.PhoneNumber));
            Assert.That(firstImported.Email, Is.EqualTo(firstOriginal.Email));

            // Check second business entity
            var secondOriginal = originalBusinessEntities[1];
            var secondImported = importedBusinessEntities.Skip(1).First();
            Assert.That(secondImported.Code, Is.EqualTo(secondOriginal.Code));
            Assert.That(secondImported.Name, Is.EqualTo(secondOriginal.Name));
            Assert.That(secondImported.Email, Is.EqualTo(secondOriginal.Email));
        }

        [Test]
        public async Task RoundTrip_ExportThenImport_PreservesComplexData()
        {
            // Arrange
            var originalBusinessEntities = new List<IBusinessEntity>
            {
                new BusinessEntityDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "BE001",
                    Name = "Acme, Inc. & Partners",
                    Address = "123 Main St, Suite 100, Building A",
                    City = "New York",
                    Email = "info+sales@acme.com"
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(originalBusinessEntities);
            var (importedBusinessEntities, errors) = await _importExportService.ImportFromCsvAsync(csv, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedBusinessEntities.Count(), Is.EqualTo(originalBusinessEntities.Count));

            var imported = importedBusinessEntities.First();
            Assert.That(imported.Name, Is.EqualTo("Acme, Inc. & Partners"));
            Assert.That(imported.Address, Is.EqualTo("123 Main St, Suite 100, Building A"));
            Assert.That(imported.Email, Is.EqualTo("info+sales@acme.com"));
        }

        #endregion
    }
}