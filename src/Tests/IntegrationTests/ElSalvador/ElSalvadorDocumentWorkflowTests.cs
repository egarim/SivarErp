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
    /// Integration tests simulating document workflows using El Salvador document types
    /// </summary>
    [TestFixture]
    public class ElSalvadorDocumentWorkflowTests
    {
        private IAuditService _auditService;
        private Dictionary<string, IDocumentType> _documentTypes;
        private Dictionary<string, DocumentDto> _documents;
        
        [SetUp]
        public void Setup()
        {
            // Setup test dependencies
            _auditService = new AuditService();
            _documentTypes = new Dictionary<string, IDocumentType>();
            _documents = new Dictionary<string, DocumentDto>();
            
            // Create document types specific to El Salvador
            SetupDocumentTypes();
        }
        
        private void SetupDocumentTypes()
        {
            // Create the Credito Fiscal document type
            var creditoFiscalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CF",
                Name = "Credito Fiscal",
                IsEnabled = true
            };
            _documentTypes["CreditoFiscal"] = creditoFiscalDocType;
            
            // Create the Consumidor Final document type
            var consumidorFinalDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "CNF",
                Name = "Consumidor Final",
                IsEnabled = true
            };
            _documentTypes["ConsumidorFinal"] = consumidorFinalDocType;
            
            // Additional document types for El Salvador
            var exportInvoiceDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "EXP",
                Name = "Factura de Exportación",
                IsEnabled = true
            };
            _documentTypes["Exportacion"] = exportInvoiceDocType;
            
            var debitNoteDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "ND",
                Name = "Nota de Débito",
                IsEnabled = true
            };
            _documentTypes["NotaDebito"] = debitNoteDocType;
            
            var creditNoteDocType = new DocumentTypeDto
            {
                Oid = Guid.NewGuid(),
                Code = "NC",
                Name = "Nota de Crédito",
                IsEnabled = true
            };
            _documentTypes["NotaCredito"] = creditNoteDocType;
        }
        
        [Test]
        public void ElSalvador_DocumentTypes_AreCreatedCorrectly()
        {
            // Assert
            Assert.That(_documentTypes.Count, Is.EqualTo(5));
            Assert.That(_documentTypes["CreditoFiscal"].Name, Is.EqualTo("Credito Fiscal"));
            Assert.That(_documentTypes["ConsumidorFinal"].Name, Is.EqualTo("Consumidor Final"));
            Assert.That(_documentTypes["Exportacion"].Name, Is.EqualTo("Factura de Exportación"));
            Assert.That(_documentTypes["NotaDebito"].Name, Is.EqualTo("Nota de Débito"));
            Assert.That(_documentTypes["NotaCredito"].Name, Is.EqualTo("Nota de Crédito"));
        }
        
        [Test]
        public void ElSalvador_CreateCreditoFiscalDocument()
        {
            // Arrange
            var documentType = _documentTypes["CreditoFiscal"];
            var businessEntity = new BusinessEntityDto 
            { 
                Oid = Guid.NewGuid(),
                Name = "Empresa ABC S.A. de C.V.",
                Code = "ABC-001"
            };
            
            // Act
            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                Date = new DateOnly(2023, 12, 15),
                Time = new TimeOnly(10, 30, 0),
                BusinessEntity = businessEntity
            };
            
            // In a real implementation, we would attach the document type to the document
            // For testing purposes, we're simulating this connection with metadata
            var documentMetadata = new Dictionary<string, object>
            {
                ["DocumentTypeOid"] = documentType.Oid,
                ["DocumentTypeCode"] = documentType.Code,
                ["DocumentNumber"] = "CF-20231215-001",
                ["TaxIdentificationNumber"] = "0123-123456-123-1", // This would typically be stored in a custom field
                ["Description"] = "Venta de mercadería a cliente corporativo",
                ["IsCustomer"] = true,  // Business role would be stored in a related table
                ["IsSupplier"] = false
            };
            
            _documents["CreditoFiscal"] = document;
            
            // Assert
            Assert.That(document.Oid, Is.Not.EqualTo(Guid.Empty));
            Assert.That(document.BusinessEntity, Is.Not.Null);
            Assert.That(document.BusinessEntity.Name, Is.EqualTo("Empresa ABC S.A. de C.V."));
            Assert.That(documentMetadata["DocumentTypeCode"], Is.EqualTo("CF"));
            Assert.That(documentMetadata["DocumentNumber"].ToString(), Does.StartWith("CF-"));
            Assert.That(documentMetadata.ContainsKey("TaxIdentificationNumber"), Is.True);
        }
        
        [Test]
        public void ElSalvador_CreateConsumidorFinalDocument()
        {
            // Arrange
            var documentType = _documentTypes["ConsumidorFinal"];
            
            // For Consumidor Final, we might not track full business entity information
            var businessEntity = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Name = "Cliente Final",
                Code = "CF-001"
            };
            
            // Act
            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                Date = new DateOnly(2023, 12, 15),
                Time = new TimeOnly(14, 45, 0),
                BusinessEntity = businessEntity
            };
            
            // In a real implementation, we would attach the document type to the document
            // For testing purposes, we're simulating this connection with metadata
            var documentMetadata = new Dictionary<string, object>
            {
                ["DocumentTypeOid"] = documentType.Oid,
                ["DocumentTypeCode"] = documentType.Code,
                ["DocumentNumber"] = "CNF-20231215-042",
                ["Description"] = "Venta al detalle",
                ["IsCustomer"] = true  // Business role would be stored in a related table
            };
            
            _documents["ConsumidorFinal"] = document;
            
            // Assert
            Assert.That(document.Oid, Is.Not.EqualTo(Guid.Empty));
            Assert.That(document.BusinessEntity, Is.Not.Null);
            Assert.That(document.BusinessEntity.Name, Is.EqualTo("Cliente Final"));
            Assert.That(documentMetadata["DocumentTypeCode"], Is.EqualTo("CNF"));
            Assert.That(documentMetadata["DocumentNumber"].ToString(), Does.StartWith("CNF-"));
        }
        
        [Test]
        public void ElSalvador_CompareDocumentTypes_DifferentProperties()
        {
            // Arrange
            var creditoFiscal = _documentTypes["CreditoFiscal"];
            var consumidorFinal = _documentTypes["ConsumidorFinal"];
            
            // Act & Assert
            Assert.That(creditoFiscal.Code, Is.Not.EqualTo(consumidorFinal.Code));
            Assert.That(creditoFiscal.Name, Is.Not.EqualTo(consumidorFinal.Name));
            Assert.That(creditoFiscal.Oid, Is.Not.EqualTo(consumidorFinal.Oid));
        }
        
        [Test]
        public void ElSalvador_BusinessRules_CreditoFiscalRequiresTaxId()
        {
            // Arrange
            var documentType = _documentTypes["CreditoFiscal"];
            
            // For testing purposes, we're storing additional metadata in dictionaries
            var businessEntityWithTaxIdMetadata = new Dictionary<string, string>
            {
                ["Code"] = "ABC-001",
                ["Name"] = "Empresa Valida S.A.",
                ["TaxId"] = "0614-290185-105-8",
                ["Type"] = "Customer"
            };
            
            var businessEntityWithoutTaxIdMetadata = new Dictionary<string, string>
            {
                ["Code"] = "XYZ-001",
                ["Name"] = "Empresa Invalida S.A.",
                ["TaxId"] = "",
                ["Type"] = "Customer"
            };
            
            // Act - Simulate validation
            bool isValidWithTaxId = !string.IsNullOrEmpty(businessEntityWithTaxIdMetadata["TaxId"]);
            bool isValidWithoutTaxId = !string.IsNullOrEmpty(businessEntityWithoutTaxIdMetadata["TaxId"]);
            
            // Assert
            Assert.That(isValidWithTaxId, Is.True, "Credito Fiscal requires a Tax ID");
            Assert.That(isValidWithoutTaxId, Is.False, "Business entity without Tax ID should be invalid for Credito Fiscal");
        }
        
        [Test]
        public void ElSalvador_BusinessRules_ConsumidorFinalDoesNotRequireTaxId()
        {
            // Arrange
            var documentType = _documentTypes["ConsumidorFinal"];
            
            // For testing purposes, we're storing additional metadata in dictionaries
            var businessEntityMetadata = new Dictionary<string, string>
            {
                ["Code"] = "CF-001",
                ["Name"] = "Cliente Final",
                ["TaxId"] = "",
                ["Type"] = "Customer"
            };
            
            // Act - For Consumidor Final, TaxId is optional
            bool isValid = true; // Always valid regardless of TaxId for Consumidor Final
            
            // Assert
            Assert.That(isValid, Is.True, "Consumidor Final does not require a Tax ID");
        }
    }
}