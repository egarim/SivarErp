using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Sivar.Erp.Documents;
using System.Collections.Generic;
using Sivar.Erp.Services.ImportExport;

namespace Tests.Documents
{
    /// <summary>
    /// Tests for the document type import/export service
    /// </summary>
    [TestFixture]
    public class DocumentTypeImportExportServiceTests
    {
        private IDocumentTypeImportExportService _importExportService;

        [SetUp]
        public void Setup()
        {
            _importExportService = new DocumentTypeImportExportService();
        }

        #region Import Tests

        [Test]
        public async Task ImportFromCsvAsync_EmptyContent_ReturnsError()
        {
            // Arrange
            string csvContent = string.Empty;

            // Act
            var (importedDocumentTypes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedDocumentTypes, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("empty"));
        }

        [Test]
        public async Task ImportFromCsvAsync_HeaderOnly_ReturnsError()
        {
            // Arrange
            string csvContent = "Code,Name,DocumentOperation,IsEnabled";

            // Act
            var (importedDocumentTypes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedDocumentTypes, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("no data"));
        }

        [Test]
        public async Task ImportFromCsvAsync_MissingRequiredHeader_ReturnsError()
        {
            // Arrange
            string csvContent = "Code,Description,Type\nINV,Invoice,Sales";

            // Act
            var (importedDocumentTypes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedDocumentTypes, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("Name"));
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidContent_ImportsDocumentTypes()
        {
            // Arrange
            string csvContent = "Code,Name,DocumentOperation,IsEnabled\nINV,Sales Invoice,SalesInvoice,true\nPO,Purchase Order,PurchaseOrder,true";

            // Act
            var (importedDocumentTypes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedDocumentTypes.Count(), Is.EqualTo(2));

            var invoice = importedDocumentTypes.First();
            Assert.That(invoice.Code, Is.EqualTo("INV"));
            Assert.That(invoice.Name, Is.EqualTo("Sales Invoice"));
            Assert.That(invoice.DocumentOperation, Is.EqualTo(DocumentOperation.SalesInvoice));
            Assert.That(invoice.IsEnabled, Is.True);

            var purchaseOrder = importedDocumentTypes.Last();
            Assert.That(purchaseOrder.Code, Is.EqualTo("PO"));
            Assert.That(purchaseOrder.Name, Is.EqualTo("Purchase Order"));
            Assert.That(purchaseOrder.DocumentOperation, Is.EqualTo(DocumentOperation.PurchaseOrder));
            Assert.That(purchaseOrder.IsEnabled, Is.True);
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidCsvWithQuotes_ImportsDocumentTypes()
        {
            // Arrange
            string csvContent = "\"Code\",\"Name\",\"DocumentOperation\",\"IsEnabled\"\n\"INV\",\"Sales Invoice, Regular\",\"SalesInvoice\",\"true\"\n\"QUO\",\"Sales Quotation\",\"Quotation\",\"false\"";

            // Act
            var (importedDocumentTypes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedDocumentTypes.Count(), Is.EqualTo(2));

            var invoice = importedDocumentTypes.First();
            Assert.That(invoice.Name, Is.EqualTo("Sales Invoice, Regular"));
            Assert.That(invoice.IsEnabled, Is.True);

            var quotation = importedDocumentTypes.Last();
            Assert.That(quotation.Name, Is.EqualTo("Sales Quotation"));
            Assert.That(quotation.IsEnabled, Is.False);
        }

        [Test]
        public async Task ImportFromCsvAsync_InvalidDocumentOperation_UsesDefault()
        {
            // Arrange
            string csvContent = "Code,Name,DocumentOperation,IsEnabled\nINV,Sales Invoice,InvalidOperation,true";

            // Act
            var (importedDocumentTypes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedDocumentTypes.Count(), Is.EqualTo(1));
            Assert.That(importedDocumentTypes.First().DocumentOperation, Is.EqualTo(DocumentOperation.PurchaseOrder));
        }

        [Test]
        public async Task ImportFromCsvAsync_InvalidBooleanValue_UsesDefault()
        {
            // Arrange
            string csvContent = "Code,Name,DocumentOperation,IsEnabled\nINV,Sales Invoice,SalesInvoice,maybe";

            // Act
            var (importedDocumentTypes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedDocumentTypes.Count(), Is.EqualTo(1));
            Assert.That(importedDocumentTypes.First().IsEnabled, Is.True); // Default value
        }

        [Test]
        public async Task ImportFromCsvAsync_EmptyCodeOrName_ReturnsError()
        {
            // Arrange
            string csvContent = "Code,Name,DocumentOperation,IsEnabled\n,Sales Invoice,SalesInvoice,true\nINV,,SalesInvoice,true";

            // Act
            var (importedDocumentTypes, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "Test User");

            // Assert
            Assert.That(importedDocumentTypes, Is.Empty);
            Assert.That(errors.Count(), Is.EqualTo(2));
            Assert.That(errors.All(e => e.Contains("validation failed")), Is.True);
        }

        #endregion

        #region Export Tests

        [Test]
        public async Task ExportToCsvAsync_NullDocumentTypes_ReturnsHeaderOnly()
        {
            // Act
            string csv = await _importExportService.ExportToCsvAsync(null!);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("Code"));
            Assert.That(csv, Does.Contain("Name"));
            Assert.That(csv, Does.Contain("DocumentOperation"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(1));
        }

        [Test]
        public async Task ExportToCsvAsync_EmptyDocumentTypes_ReturnsHeaderOnly()
        {
            // Act
            string csv = await _importExportService.ExportToCsvAsync(Array.Empty<IDocumentType>());

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("Code"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(1));
        }

        [Test]
        public async Task ExportToCsvAsync_WithDocumentTypes_ReturnsCsvContent()
        {
            // Arrange
            var documentTypes = new List<IDocumentType>
            {
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "INV",
                    Name = "Sales Invoice",
                    DocumentOperation = DocumentOperation.SalesInvoice,
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "PO",
                    Name = "Purchase Order",
                    DocumentOperation = DocumentOperation.PurchaseOrder,
                    IsEnabled = false
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(documentTypes);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("Code"));
            Assert.That(csv, Does.Contain("Name"));
            Assert.That(csv, Does.Contain("Sales Invoice"));
            Assert.That(csv, Does.Contain("Purchase Order"));
            Assert.That(csv, Does.Contain("SalesInvoice"));
            Assert.That(csv, Does.Contain("PurchaseOrder"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(3)); // Header + 2 data rows
        }

        [Test]
        public async Task ExportToCsvAsync_WithCommasInData_QuotesValues()
        {
            // Arrange
            var documentTypes = new List<IDocumentType>
            {
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "INV",
                    Name = "Sales Invoice, Regular",
                    DocumentOperation = DocumentOperation.SalesInvoice,
                    IsEnabled = true
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(documentTypes);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("\"Sales Invoice, Regular\""));
        }

        [Test]
        public async Task ExportToCsvAsync_WithVariousOperations_IncludesAllTypes()
        {
            // Arrange
            var documentTypes = new List<IDocumentType>
            {
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "PO",
                    Name = "Purchase Order",
                    DocumentOperation = DocumentOperation.PurchaseOrder,
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "QUO",
                    Name = "Quotation",
                    DocumentOperation = DocumentOperation.Quotation,
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "DN",
                    Name = "Delivery Note",
                    DocumentOperation = DocumentOperation.DeliveryNote,
                    IsEnabled = false
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(documentTypes);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv, Does.Contain("PurchaseOrder"));
            Assert.That(csv, Does.Contain("Quotation"));
            Assert.That(csv, Does.Contain("DeliveryNote"));
            Assert.That(csv, Does.Contain("True"));
            Assert.That(csv, Does.Contain("False"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(4)); // Header + 3 data rows
        }

        #endregion

        #region Round Trip Tests

        [Test]
        public async Task RoundTrip_ExportThenImport_PreservesDocumentTypes()
        {
            // Arrange
            var originalDocumentTypes = new List<IDocumentType>
            {
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "INV",
                    Name = "Sales Invoice",
                    DocumentOperation = DocumentOperation.SalesInvoice,
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "PO",
                    Name = "Purchase Order",
                    DocumentOperation = DocumentOperation.PurchaseOrder,
                    IsEnabled = false
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(originalDocumentTypes);
            var (importedDocumentTypes, errors) = await _importExportService.ImportFromCsvAsync(csv, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedDocumentTypes.Count(), Is.EqualTo(originalDocumentTypes.Count));

            // Check first document type
            var firstOriginal = originalDocumentTypes[0];
            var firstImported = importedDocumentTypes.First();
            Assert.That(firstImported.Code, Is.EqualTo(firstOriginal.Code));
            Assert.That(firstImported.Name, Is.EqualTo(firstOriginal.Name));
            Assert.That(firstImported.DocumentOperation, Is.EqualTo(firstOriginal.DocumentOperation));
            Assert.That(firstImported.IsEnabled, Is.EqualTo(firstOriginal.IsEnabled));

            // Check second document type
            var secondOriginal = originalDocumentTypes[1];
            var secondImported = importedDocumentTypes.Skip(1).First();
            Assert.That(secondImported.Code, Is.EqualTo(secondOriginal.Code));
            Assert.That(secondImported.Name, Is.EqualTo(secondOriginal.Name));
            Assert.That(secondImported.DocumentOperation, Is.EqualTo(secondOriginal.DocumentOperation));
            Assert.That(secondImported.IsEnabled, Is.EqualTo(secondOriginal.IsEnabled));
        }

        [Test]
        public async Task RoundTrip_ExportThenImport_PreservesAllDocumentOperations()
        {
            // Arrange
            var originalDocumentTypes = new List<IDocumentType>
            {
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "PR",
                    Name = "Purchase Requisition",
                    DocumentOperation = DocumentOperation.PurchaseRequisition,
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "RFQ",
                    Name = "Request for Quotation",
                    DocumentOperation = DocumentOperation.RequestForQuotation,
                    IsEnabled = true
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "GRN",
                    Name = "Goods Receipt Note",
                    DocumentOperation = DocumentOperation.GoodsReceiptNote,
                    IsEnabled = false
                },
                new DocumentTypeDto
                {
                    Oid = Guid.NewGuid(),
                    Code = "DN",
                    Name = "Debit Note",
                    DocumentOperation = DocumentOperation.DebitNote,
                    IsEnabled = true
                }
            };

            // Act
            string csv = await _importExportService.ExportToCsvAsync(originalDocumentTypes);
            var (importedDocumentTypes, errors) = await _importExportService.ImportFromCsvAsync(csv, "Test User");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedDocumentTypes.Count(), Is.EqualTo(originalDocumentTypes.Count));

            // Check that all document operations are preserved
            var importedList = importedDocumentTypes.ToList();
            Assert.That(importedList.Any(d => d.DocumentOperation == DocumentOperation.PurchaseRequisition), Is.True);
            Assert.That(importedList.Any(d => d.DocumentOperation == DocumentOperation.RequestForQuotation), Is.True);
            Assert.That(importedList.Any(d => d.DocumentOperation == DocumentOperation.GoodsReceiptNote), Is.True);
            Assert.That(importedList.Any(d => d.DocumentOperation == DocumentOperation.DebitNote), Is.True);
        }

        #endregion
    }
}
