//using System;
//using NUnit.Framework;
//using Sivar.Erp.Documents;

//namespace Sivar.Erp.Tests.Documents
//{
//    /// <summary>
//    /// Unit tests for the Document module
//    /// </summary>
//    [TestFixture]
//    public class DocumentTests
//    {
//        private IAuditService _auditService;
//        private IDocumentService _documentService;

//        [SetUp]
//        public void Setup()
//        {
//            // Setup test dependencies
//            _auditService = new AuditService();
//            _documentService = new DocumentService(_auditService);
//        }

//        #region DocumentDto Tests

//        [Test]
//        public void DocumentDto_NewDocument_HasEmptyGuid()
//        {
//            // Arrange & Act
//            var document = new DocumentDto();

//            // Assert
//            Assert.That(document.Id, Is.EqualTo(Guid.Empty));
//        }

//        [Test]
//        public void DocumentDto_NewDocument_HasEmptyStrings()
//        {
//            // Arrange & Act
//            var document = new DocumentDto();

//            // Assert
//            Assert.That(document.DocumentNo, Is.EqualTo(string.Empty));
//            Assert.That(document.Description, Is.EqualTo(string.Empty));
//            Assert.That(document.DocumentComments, Is.EqualTo(string.Empty));
//            Assert.That(document.InternalComments, Is.EqualTo(string.Empty));
//            Assert.That(document.ExternalId, Is.EqualTo(string.Empty));
//        }

//        [Test]
//        public void DocumentDto_NewDocument_HasDefaultDocumentType()
//        {
//            // Arrange & Act
//            var document = new DocumentDto();

//            // Assert
//            Assert.That(document.DocumentType, Is.EqualTo(default(DocumentType)));
//        }

//        [Test]
//        public void DocumentDto_SettingProperties_WorksAsExpected()
//        {
//            // Arrange
//            var document = new DocumentDto();
//            var id = Guid.NewGuid();
//            var documentDate = new DateOnly(2025, 1, 1);
//            var documentType = DocumentType.BalanceTransfer;

//            // Act
//            document.Id = id;
//            document.DocumentDate = documentDate;
//            document.DocumentNo = "DOC-001";
//            document.Description = "Test Document";
//            document.DocumentComments = "For testing";
//            document.InternalComments = "Internal note";
//            document.DocumentType = documentType;
//            document.ExtendedDocumentTypeId = null;
//            document.ExternalId = "EXT-001";

//            // Assert
//            Assert.That(document.Id, Is.EqualTo(id));
//            Assert.That(document.DocumentDate, Is.EqualTo(documentDate));
//            Assert.That(document.DocumentNo, Is.EqualTo("DOC-001"));
//            Assert.That(document.Description, Is.EqualTo("Test Document"));
//            Assert.That(document.DocumentComments, Is.EqualTo("For testing"));
//            Assert.That(document.InternalComments, Is.EqualTo("Internal note"));
//            Assert.That(document.DocumentType, Is.EqualTo(documentType));
//            Assert.That(document.ExtendedDocumentTypeId, Is.Null);
//            Assert.That(document.ExternalId, Is.EqualTo("EXT-001"));
//        }

//        #endregion

//        #region DocumentService Tests

//        [Test]
//        public async Task DocumentService_CreateDocument_GeneratesIdForEmptyGuid()
//        {
//            // Arrange
//            var document = new DocumentDto
//            {
//                DocumentDate = new DateOnly(2025, 1, 1),
//                DocumentNo = "DOC-001",
//                Description = "Test Document"
//            };

//            // Act
//            var result = await _documentService.CreateDocumentAsync(document, "Test User");

//            // Assert
//            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
//        }

//        [Test]
//        public async Task DocumentService_CreateDocument_PreservesProvidedId()
//        {
//            // Arrange
//            var id = Guid.NewGuid();
//            var document = new DocumentDto
//            {
//                Id = id,
//                DocumentDate = new DateOnly(2025, 1, 1),
//                DocumentNo = "DOC-001",
//                Description = "Test Document"
//            };

//            // Act
//            var result = await _documentService.CreateDocumentAsync(document, "Test User");

//            // Assert
//            Assert.That(result.Id, Is.EqualTo(id));
//        }

//        [Test]
//        public async Task DocumentService_CreateDocument_SetsAuditFields()
//        {
//            // Arrange
//            var document = new DocumentDto
//            {
//                DocumentDate = new DateOnly(2025, 1, 1),
//                DocumentNo = "DOC-001",
//                Description = "Test Document"
//            };

//            // Act
//            var result = await _documentService.CreateDocumentAsync(document, "Test User");

//            // Assert
//            Assert.That(result.InsertedBy, Is.EqualTo("Test User"));
//            Assert.That(result.UpdatedBy, Is.EqualTo("Test User"));
//            Assert.That(result.InsertedAt, Is.GreaterThan(DateTime.MinValue));
//            Assert.That(result.UpdatedAt, Is.GreaterThan(DateTime.MinValue));
//        }

//        [Test]
//        public async Task DocumentService_UpdateDocument_ThrowsExceptionForEmptyGuid()
//        {
//            // Arrange
//            var document = new DocumentDto
//            {
//                DocumentDate = new DateOnly(2025, 1, 1),
//                DocumentNo = "DOC-001",
//                Description = "Test Document"
//            };

//            // Act & Assert
//            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
//                await _documentService.UpdateDocumentAsync(document, "Test User"));

//            Assert.That(ex.Message, Does.Contain("Document ID must be provided"));
//        }

//        [Test]
//        public async Task DocumentService_UpdateDocument_UpdatesAuditFields()
//        {
//            // Arrange
//            var document = new DocumentDto
//            {
//                Id = Guid.NewGuid(),
//                DocumentDate = new DateOnly(2025, 1, 1),
//                DocumentNo = "DOC-001",
//                Description = "Test Document",
//                InsertedAt = DateTime.UtcNow.AddMinutes(-10),
//                InsertedBy = "Original User",
//                UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
//                UpdatedBy = "Original User"
//            };

//            // Act
//            var result = await _documentService.UpdateDocumentAsync(document, "Update User");

//            // Assert
//            Assert.That(result.InsertedBy, Is.EqualTo("Original User"));
//            Assert.That(result.UpdatedBy, Is.EqualTo("Update User"));
//            Assert.That(result.InsertedAt, Is.LessThan(result.UpdatedAt));
//        }

//        #endregion

//        #region TransactionDto Tests

//        [Test]
//        public void TransactionDto_NewTransaction_HasEmptyGuid()
//        {
//            // Arrange & Act
//            var transaction = new TransactionDto();

//            // Assert
//            Assert.That(transaction.Id, Is.EqualTo(Guid.Empty));
//        }

//        [Test]
//        public void TransactionDto_NewTransaction_HasEmptyDocumentId()
//        {
//            // Arrange & Act
//            var transaction = new TransactionDto();

//            // Assert
//            Assert.That(transaction.DocumentId, Is.EqualTo(Guid.Empty));
//        }

//        [Test]
//        public void TransactionDto_SettingProperties_WorksAsExpected()
//        {
//            // Arrange
//            var transaction = new TransactionDto();
//            var id = Guid.NewGuid();
//            var documentId = Guid.NewGuid();
//            var transactionDate = new DateOnly(2025, 1, 1);

//            // Act
//            transaction.Id = id;
//            transaction.DocumentId = documentId;
//            transaction.TransactionDate = transactionDate;
//            transaction.Description = "Test Transaction";

//            // Assert
//            Assert.That(transaction.Id, Is.EqualTo(id));
//            Assert.That(transaction.DocumentId, Is.EqualTo(documentId));
//            Assert.That(transaction.TransactionDate, Is.EqualTo(transactionDate));
//            Assert.That(transaction.Description, Is.EqualTo("Test Transaction"));
//        }

//        #endregion

//        #region TransactionService Tests

//        [Test]
//        public async Task TransactionService_CreateTransaction_GeneratesIdForEmptyGuid()
//        {
//            // Arrange
//            var transactionService = new TransactionService();
//            var transaction = new TransactionDto
//            {
//                DocumentId = Guid.NewGuid(),
//                TransactionDate = new DateOnly(2025, 1, 1),
//                Description = "Test Transaction"
//            };

//            // Act
//            var result = await transactionService.CreateTransactionAsync(transaction);

//            // Assert
//            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
//        }

//        [Test]
//        public async Task TransactionService_ValidateTransaction_FailsForNoEntries()
//        {
//            // Arrange
//            var transactionService = new TransactionService();
//            var transactionId = Guid.NewGuid();
//            var entries = new List<ILedgerEntry>();

//            // Act
//            var result = await transactionService.ValidateTransactionAsync(transactionId, entries);

//            // Assert
//            Assert.That(result, Is.False);
//        }

//        [Test]
//        public async Task TransactionService_ValidateTransaction_FailsForImbalancedEntries()
//        {
//            // Arrange
//            var transactionService = new TransactionService();
//            var transactionId = Guid.NewGuid();
//            var entries = new List<ILedgerEntry>
//            {
//                new LedgerEntryDto
//                {
//                    TransactionId = transactionId,
//                    AccountId = Guid.NewGuid(),
//                    EntryType = EntryType.Debit,
//                    Amount = 100.00m
//                },
//                new LedgerEntryDto
//                {
//                    TransactionId = transactionId,
//                    AccountId = Guid.NewGuid(),
//                    EntryType = EntryType.Credit,
//                    Amount = 50.00m
//                }
//            };

//            // Act
//            var result = await transactionService.ValidateTransactionAsync(transactionId, entries);

//            // Assert
//            Assert.That(result, Is.False);
//        }

//        [Test]
//        public async Task TransactionService_ValidateTransaction_SucceedsForBalancedEntries()
//        {
//            // Arrange
//            var transactionService = new TransactionService();
//            var transactionId = Guid.NewGuid();
//            var entries = new List<ILedgerEntry>
//            {
//                new LedgerEntryDto
//                {
//                    TransactionId = transactionId,
//                    AccountId = Guid.NewGuid(),
//                    EntryType = EntryType.Debit,
//                    Amount = 100.00m
//                },
//                new LedgerEntryDto
//                {
//                    TransactionId = transactionId,
//                    AccountId = Guid.NewGuid(),
//                    EntryType = EntryType.Credit,
//                    Amount = 100.00m
//                }
//            };

//            // Act
//            var result = await transactionService.ValidateTransactionAsync(transactionId, entries);

//            // Assert
//            Assert.That(result, Is.True);
//        }

//        #endregion

//        #region LedgerEntryDto Tests

//        [Test]
//        public void LedgerEntryDto_NewLedgerEntry_HasEmptyGuid()
//        {
//            // Arrange & Act
//            var ledgerEntry = new LedgerEntryDto();

//            // Assert
//            Assert.That(ledgerEntry.Id, Is.EqualTo(Guid.Empty));
//        }

//        [Test]
//        public void LedgerEntryDto_NewLedgerEntry_HasEmptyTransactionId()
//        {
//            // Arrange & Act
//            var ledgerEntry = new LedgerEntryDto();

//            // Assert
//            Assert.That(ledgerEntry.TransactionId, Is.EqualTo(Guid.Empty));
//        }

//        [Test]
//        public void LedgerEntryDto_SettingProperties_WorksAsExpected()
//        {
//            // Arrange
//            var ledgerEntry = new LedgerEntryDto();
//            var id = Guid.NewGuid();
//            var transactionId = Guid.NewGuid();
//            var accountId = Guid.NewGuid();
//            var personId = Guid.NewGuid();
//            var costCentreId = Guid.NewGuid();

//            // Act
//            ledgerEntry.Id = id;
//            ledgerEntry.TransactionId = transactionId;
//            ledgerEntry.AccountId = accountId;
//            ledgerEntry.EntryType = EntryType.Debit;
//            ledgerEntry.Amount = 123.45m;
//            ledgerEntry.PersonId = personId;
//            ledgerEntry.CostCentreId = costCentreId;

//            // Assert
//            Assert.That(ledgerEntry.Id, Is.EqualTo(id));
//            Assert.That(ledgerEntry.TransactionId, Is.EqualTo(transactionId));
//            Assert.That(ledgerEntry.AccountId, Is.EqualTo(accountId));
//            Assert.That(ledgerEntry.EntryType, Is.EqualTo(EntryType.Debit));
//            Assert.That(ledgerEntry.Amount, Is.EqualTo(123.45m));
//            Assert.That(ledgerEntry.PersonId, Is.EqualTo(personId));
//            Assert.That(ledgerEntry.CostCentreId, Is.EqualTo(costCentreId));
//        }

//        #endregion

//        #region Integration Tests

//        [Test]
//        public async Task Integration_CreateDocumentWithTransaction()
//        {
//            // Arrange
//            var documentService = new DocumentService(_auditService);
//            var transactionService = new TransactionService();

//            var document = new DocumentDto
//            {
//                DocumentDate = new DateOnly(2025, 1, 1),
//                DocumentNo = "DOC-001",
//                Description = "Test Document",
//                DocumentType = DocumentType.Miscellaneous
//            };

//            // Act - Create document
//            var createdDocument = await documentService.CreateDocumentAsync(document, "Test User");

//            // Create transaction
//            var transaction = new TransactionDto
//            {
//                DocumentId = createdDocument.Id,
//                TransactionDate = document.DocumentDate,
//                Description = "Test Transaction"
//            };

//            var createdTransaction = await transactionService.CreateTransactionAsync(transaction);

//            // Create ledger entries
//            var ledgerEntries = new List<ILedgerEntry>
//            {
//                new LedgerEntryDto
//                {
//                    TransactionId = createdTransaction.Id,
//                    AccountId = Guid.NewGuid(), // In real code, this would be a valid account ID
//                    EntryType = EntryType.Debit,
//                    Amount = 1000.00m
//                },
//                new LedgerEntryDto
//                {
//                    TransactionId = createdTransaction.Id,
//                    AccountId = Guid.NewGuid(), // In real code, this would be a valid account ID
//                    EntryType = EntryType.Credit,
//                    Amount = 1000.00m
//                }
//            };

//            var isValid = await transactionService.ValidateTransactionAsync(
//                createdTransaction.Id, ledgerEntries);

//            // Assert
//            Assert.That(createdDocument.Id, Is.Not.EqualTo(Guid.Empty));
//            Assert.That(createdTransaction.Id, Is.Not.EqualTo(Guid.Empty));
//            Assert.That(createdTransaction.DocumentId, Is.EqualTo(createdDocument.Id));
//            Assert.That(isValid, Is.True);
//        }

//        #endregion
//    }
//}