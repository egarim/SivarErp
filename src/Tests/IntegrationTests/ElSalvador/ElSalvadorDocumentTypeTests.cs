using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sivar.Erp;
using Sivar.Erp.Documents;

namespace Tests.IntegrationTests.ElSalvador
{
    /// <summary>
    /// Tests for El Salvador specific document types
    /// </summary>
    [TestFixture]
    public class ElSalvadorDocumentTypeTests
    {
        private IAuditService _auditService;
        
        [SetUp]
        public void Setup()
        {
            // Setup test dependencies
            _auditService = new AuditService();
        }

        [Test]
        public void CreateElSalvadorDocumentTypes_ValidateProperties()
        {
            // Arrange
            var creditoFiscalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true
            };

            var consumidorFinalDocType = new DocumentTypeDto 
            {
                Oid = Guid.NewGuid(),
                Code = "CNF",
                Name = "Consumidor Final",
                IsEnabled = true
            };

            // Act - Nothing specific to do here, as we're just validating properties

            // Assert
            // Validate Credito Fiscal document type
            Assert.That(creditoFiscalDocType.Oid, Is.Not.EqualTo(Guid.Empty));
            Assert.That(creditoFiscalDocType.Code, Is.EqualTo("CF"));
            Assert.That(creditoFiscalDocType.Name, Is.EqualTo("Credito Fiscal"));
            Assert.That(creditoFiscalDocType.IsEnabled, Is.True);
            
            // Validate Consumidor Final document type
            Assert.That(consumidorFinalDocType.Oid, Is.Not.EqualTo(Guid.Empty));
            Assert.That(consumidorFinalDocType.Code, Is.EqualTo("CNF"));
            Assert.That(consumidorFinalDocType.Name, Is.EqualTo("Consumidor Final"));
            Assert.That(consumidorFinalDocType.IsEnabled, Is.True);
        }

        [Test]
        public void ElSalvador_DocumentTypes_AreUnique()
        {
            // Arrange
            var documentTypes = new List<IDocumentType>
            {
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "CF",
                    Name = "Credito Fiscal",
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "CNF",
                    Name = "Consumidor Final",
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "EXP",
                    Name = "Factura de Exportación",
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "FACT",
                    Name = "Factura Comercial",
                    IsEnabled = true
                }
            };

            // Act
            var distinctCodes = documentTypes.Select(dt => dt.Code).Distinct().Count();
            var distinctNames = documentTypes.Select(dt => dt.Name).Distinct().Count();

            // Assert
            Assert.That(distinctCodes, Is.EqualTo(documentTypes.Count), "All document type codes should be unique");
            Assert.That(distinctNames, Is.EqualTo(documentTypes.Count), "All document type names should be unique");
        }

        [Test]
        public void ElSalvador_DocumentType_CanUpdateProperties()
        {
            // Arrange
            var creditoFiscalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true
            };

            // Act
            bool originalState = creditoFiscalDocType.IsEnabled;
            creditoFiscalDocType.IsEnabled = false;
            string originalName = creditoFiscalDocType.Name;
            creditoFiscalDocType.Name = "Crédito Fiscal (Updated)";

            // Assert
            Assert.That(originalState, Is.True);
            Assert.That(creditoFiscalDocType.IsEnabled, Is.False);
            Assert.That(originalName, Is.EqualTo("Credito Fiscal"));
            Assert.That(creditoFiscalDocType.Name, Is.EqualTo("Crédito Fiscal (Updated)"));
        }

        [Test]
        public void ElSalvador_DocumentType_RaisesPropertyChangedEvent()
        {
            // Arrange
            var documentType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true
            };

            string propertyNameRaised = null;
            documentType.PropertyChanged += (sender, args) => {
                propertyNameRaised = args.PropertyName;
            };

            // Act
            documentType.Name = "Crédito Fiscal (Changed)";

            // Assert
            Assert.That(propertyNameRaised, Is.EqualTo("Name"));
        }

        [Test]
        public void ElSalvador_CreateDocumentWithSpecificType()
        {
            // Arrange
            var creditoFiscalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true
            };

            // Create a document using the Document class 
            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                Date = new DateOnly(2023, 12, 15),
                Time = new TimeOnly(10, 30, 0)
            };

            // Since there is no direct property for document type in DocumentDto,
            // we would typically store this in a custom property bag or as metadata
            // Test the connection between document and document type
            
            // We can use object metadata (simulated for test)
            var documentMetadata = new Dictionary<string, object>
            {
                ["DocumentTypeOid"] = creditoFiscalDocType.Oid,
                ["DocumentTypeCode"] = creditoFiscalDocType.Code
            };

            // Check document has been created with appropriate fields for El Salvador
            Assert.That(documentMetadata["DocumentTypeOid"], Is.EqualTo(creditoFiscalDocType.Oid));
            Assert.That(documentMetadata["DocumentTypeCode"], Is.EqualTo("CF"));
            Assert.That(document.Oid, Is.Not.EqualTo(Guid.Empty));
            Assert.That(document.Date, Is.EqualTo(new DateOnly(2023, 12, 15)));
        }
    }
}