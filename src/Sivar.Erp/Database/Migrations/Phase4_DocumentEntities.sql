-- Phase 4: Core Domain Migration - Document Entities
-- Migration script for creating document management tables

-- Create schemas if they don't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'docs')
    EXEC('CREATE SCHEMA docs')
GO

-- Create Document Types table
CREATE TABLE docs.DocumentTypes (
    Oid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Code NVARCHAR(20) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Category NVARCHAR(50) NOT NULL,
    NumberPrefix NVARCHAR(10) NULL,
    NumberSuffix NVARCHAR(10) NULL,
    NextNumber BIGINT NOT NULL DEFAULT 1,
    NumberFormat NVARCHAR(50) NOT NULL DEFAULT '{Prefix}{Number:D6}{Suffix}',
    IsActive BIT NOT NULL DEFAULT 1,
    AutoGenerateNumbers BIT NOT NULL DEFAULT 1,
    RequiresApproval BIT NOT NULL DEFAULT 0,
    RequiresLines BIT NOT NULL DEFAULT 1,
    RequiresBusinessEntity BIT NOT NULL DEFAULT 0,
    AllowZeroAmount BIT NOT NULL DEFAULT 0,
    DefaultCurrencyCode NVARCHAR(3) NOT NULL DEFAULT 'USD',
    
    -- Tenant fields
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    BranchId UNIQUEIDENTIFIER NOT NULL,
    
    -- Audit fields
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedAt DATETIME2 NULL,
    
    -- Soft delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedBy NVARCHAR(100) NULL,
    DeletedAt DATETIME2 NULL,
    
    -- Version tracking
    Version INT NOT NULL DEFAULT 1,
    RowVersion ROWVERSION,
    
    CONSTRAINT PK_DocumentTypes PRIMARY KEY (Oid),
    CONSTRAINT UK_DocumentTypes_Code UNIQUE (Code),
    INDEX IX_DocumentTypes_Name (Name),
    INDEX IX_DocumentTypes_Category_Active (Category, IsActive),
    INDEX IX_DocumentTypes_Tenant (CompanyId, BranchId),
    INDEX IX_DocumentTypes_CreatedAt (CreatedAt),
    INDEX IX_DocumentTypes_IsDeleted (IsDeleted)
);
GO

-- Create Business Entities table
CREATE TABLE docs.BusinessEntities (
    Oid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Code NVARCHAR(20) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    EntityType NVARCHAR(50) NOT NULL DEFAULT 'Customer',
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Contact information
    Email NVARCHAR(100) NULL,
    Phone NVARCHAR(20) NULL,
    Address NVARCHAR(500) NULL,
    
    -- Tax information
    TaxId NVARCHAR(50) NULL,
    TaxCategory NVARCHAR(50) NULL,
    
    -- Financial
    DefaultCurrencyCode NVARCHAR(3) NULL DEFAULT 'USD',
    PaymentTerms NVARCHAR(50) NULL,
    CreditLimit DECIMAL(18,2) NULL,
    
    -- Tenant fields
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    BranchId UNIQUEIDENTIFIER NOT NULL,
    
    -- Audit fields
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedAt DATETIME2 NULL,
    
    -- Soft delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedBy NVARCHAR(100) NULL,
    DeletedAt DATETIME2 NULL,
    
    -- Version tracking
    Version INT NOT NULL DEFAULT 1,
    RowVersion ROWVERSION,
    
    CONSTRAINT PK_BusinessEntities PRIMARY KEY (Oid),
    CONSTRAINT UK_BusinessEntities_Code UNIQUE (Code),
    INDEX IX_BusinessEntities_Name (Name),
    INDEX IX_BusinessEntities_EntityType (EntityType),
    INDEX IX_BusinessEntities_IsActive (IsActive),
    INDEX IX_BusinessEntities_Tenant (CompanyId, BranchId),
    INDEX IX_BusinessEntities_CreatedAt (CreatedAt),
    INDEX IX_BusinessEntities_IsDeleted (IsDeleted)
);
GO

-- Create Items table
CREATE TABLE docs.Items (
    Oid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    ItemType NVARCHAR(50) NOT NULL DEFAULT 'Product',
    Category NVARCHAR(100) NOT NULL DEFAULT 'General',
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Inventory
    UnitOfMeasure NVARCHAR(10) NULL,
    StandardCost DECIMAL(18,4) NULL,
    ListPrice DECIMAL(18,4) NULL,
    IsInventoryItem BIT NOT NULL DEFAULT 1,
    
    -- Tax
    TaxCategory NVARCHAR(50) NULL,
    DefaultTaxPercent DECIMAL(5,2) NULL,
    
    -- Tenant fields
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    BranchId UNIQUEIDENTIFIER NOT NULL,
    
    -- Audit fields
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedAt DATETIME2 NULL,
    
    -- Soft delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedBy NVARCHAR(100) NULL,
    DeletedAt DATETIME2 NULL,
    
    -- Version tracking
    Version INT NOT NULL DEFAULT 1,
    RowVersion ROWVERSION,
    
    CONSTRAINT PK_Items PRIMARY KEY (Oid),
    CONSTRAINT UK_Items_Code UNIQUE (Code),
    INDEX IX_Items_Name (Name),
    INDEX IX_Items_Category (Category),
    INDEX IX_Items_ItemType (ItemType),
    INDEX IX_Items_IsActive (IsActive),
    INDEX IX_Items_IsInventoryItem (IsInventoryItem),
    INDEX IX_Items_Tenant (CompanyId, BranchId),
    INDEX IX_Items_CreatedAt (CreatedAt),
    INDEX IX_Items_IsDeleted (IsDeleted)
);
GO

-- Create Documents table
CREATE TABLE docs.Documents (
    Oid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    DocumentNumber NVARCHAR(50) NOT NULL,
    DocumentDate DATE NOT NULL,
    DocumentTime TIME NOT NULL,
    DocumentTypeId UNIQUEIDENTIFIER NOT NULL,
    BusinessEntityId UNIQUEIDENTIFIER NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Draft',
    CurrencyCode NVARCHAR(3) NOT NULL DEFAULT 'USD',
    ExchangeRate DECIMAL(18,8) NOT NULL DEFAULT 1.0,
    Remarks NVARCHAR(1000) NULL,
    DueDate DATE NULL,
    
    -- Financial totals
    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalDiscount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalTax DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    -- Posting information
    IsPosted BIT NOT NULL DEFAULT 0,
    PostedAt DATETIME2 NULL,
    PostedBy NVARCHAR(100) NULL,
    
    -- Computed columns
    LineCount AS (SELECT COUNT(*) FROM docs.DocumentLines WHERE DocumentId = Oid),
    
    -- Tenant fields
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    BranchId UNIQUEIDENTIFIER NOT NULL,
    
    -- Audit fields
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedAt DATETIME2 NULL,
    
    -- Soft delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedBy NVARCHAR(100) NULL,
    DeletedAt DATETIME2 NULL,
    
    -- Version tracking
    Version INT NOT NULL DEFAULT 1,
    RowVersion ROWVERSION,
    
    CONSTRAINT PK_Documents PRIMARY KEY (Oid),
    CONSTRAINT UK_Documents_DocumentNumber UNIQUE (DocumentNumber),
    CONSTRAINT FK_Documents_DocumentType FOREIGN KEY (DocumentTypeId) REFERENCES docs.DocumentTypes(Oid),
    CONSTRAINT FK_Documents_BusinessEntity FOREIGN KEY (BusinessEntityId) REFERENCES docs.BusinessEntities(Oid),
    INDEX IX_Documents_Company_Date (CompanyId, DocumentDate),
    INDEX IX_Documents_Type_Status (DocumentTypeId, Status),
    INDEX IX_Documents_BusinessEntity (BusinessEntityId),
    INDEX IX_Documents_Posted (IsPosted),
    INDEX IX_Documents_Tenant (CompanyId, BranchId),
    INDEX IX_Documents_CreatedAt (CreatedAt),
    INDEX IX_Documents_IsDeleted (IsDeleted)
);
GO

-- Create Document Lines table
CREATE TABLE docs.DocumentLines (
    Oid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    LineNumber INT NOT NULL,
    ItemId UNIQUEIDENTIFIER NULL,
    ItemCode NVARCHAR(50) NULL,
    Description NVARCHAR(500) NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    UnitOfMeasure NVARCHAR(10) NULL,
    UnitPrice DECIMAL(18,4) NOT NULL,
    
    -- Discounts
    DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    -- Tax
    TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    -- Totals
    LineTotal DECIMAL(18,2) NOT NULL,
    
    -- Currency
    CurrencyCode NVARCHAR(3) NOT NULL DEFAULT 'USD',
    ExchangeRate DECIMAL(18,8) NOT NULL DEFAULT 1.0,
    
    Notes NVARCHAR(500) NULL,
    
    -- Tenant fields
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    BranchId UNIQUEIDENTIFIER NOT NULL,
    
    -- Audit fields
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedAt DATETIME2 NULL,
    
    -- Soft delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedBy NVARCHAR(100) NULL,
    DeletedAt DATETIME2 NULL,
    
    -- Version tracking
    Version INT NOT NULL DEFAULT 1,
    RowVersion ROWVERSION,
    
    CONSTRAINT PK_DocumentLines PRIMARY KEY (Oid),
    CONSTRAINT UK_DocumentLines_Document_LineNumber UNIQUE (DocumentId, LineNumber),
    CONSTRAINT FK_DocumentLines_Document FOREIGN KEY (DocumentId) REFERENCES docs.Documents(Oid) ON DELETE CASCADE,
    CONSTRAINT FK_DocumentLines_Item FOREIGN KEY (ItemId) REFERENCES docs.Items(Oid),
    INDEX IX_DocumentLines_Item (ItemId),
    INDEX IX_DocumentLines_ItemCode (ItemCode),
    INDEX IX_DocumentLines_Tenant (CompanyId, BranchId),
    INDEX IX_DocumentLines_CreatedAt (CreatedAt),
    INDEX IX_DocumentLines_IsDeleted (IsDeleted)
);
GO

-- Create Document Totals table
CREATE TABLE docs.DocumentTotals (
    Oid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    TotalType NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    Rate DECIMAL(5,2) NULL,
    BaseAmount DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    CurrencyCode NVARCHAR(3) NOT NULL DEFAULT 'USD',
    ExchangeRate DECIMAL(18,8) NOT NULL DEFAULT 1.0,
    
    -- Tenant fields
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    BranchId UNIQUEIDENTIFIER NOT NULL,
    
    -- Audit fields
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedAt DATETIME2 NULL,
    
    -- Soft delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedBy NVARCHAR(100) NULL,
    DeletedAt DATETIME2 NULL,
    
    -- Version tracking
    Version INT NOT NULL DEFAULT 1,
    RowVersion ROWVERSION,
    
    CONSTRAINT PK_DocumentTotals PRIMARY KEY (Oid),
    CONSTRAINT FK_DocumentTotals_Document FOREIGN KEY (DocumentId) REFERENCES docs.Documents(Oid) ON DELETE CASCADE,
    INDEX IX_DocumentTotals_Document_Type (DocumentId, TotalType),
    INDEX IX_DocumentTotals_Tenant (CompanyId, BranchId),
    INDEX IX_DocumentTotals_CreatedAt (CreatedAt),
    INDEX IX_DocumentTotals_IsDeleted (IsDeleted)
);
GO

-- Insert sample document types
INSERT INTO docs.DocumentTypes (Code, Name, Description, Category, NumberPrefix, CompanyId, BranchId, CreatedBy)
VALUES 
    ('SO', 'Sales Order', 'Customer sales orders', 'Sales', 'SO-', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System'),
    ('SI', 'Sales Invoice', 'Customer invoices', 'Sales', 'INV-', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System'),
    ('PO', 'Purchase Order', 'Supplier purchase orders', 'Purchasing', 'PO-', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System'),
    ('PI', 'Purchase Invoice', 'Supplier invoices', 'Purchasing', 'PINV-', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System'),
    ('QT', 'Quote', 'Customer quotations', 'Sales', 'QT-', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System');
GO

-- Insert sample business entities
INSERT INTO docs.BusinessEntities (Code, Name, EntityType, Email, Phone, CompanyId, BranchId, CreatedBy)
VALUES 
    ('CUST001', 'ABC Corporation', 'Customer', 'contact@abc.com', '+1-555-0001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System'),
    ('CUST002', 'XYZ Industries', 'Customer', 'info@xyz.com', '+1-555-0002', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System'),
    ('SUPP001', 'Global Suppliers Inc', 'Supplier', 'sales@global.com', '+1-555-0003', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System'),
    ('SUPP002', 'Tech Components Ltd', 'Supplier', 'orders@techcomp.com', '+1-555-0004', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System');
GO

-- Insert sample items
INSERT INTO docs.Items (Code, Name, Description, Category, UnitOfMeasure, ListPrice, CompanyId, BranchId, CreatedBy)
VALUES 
    ('ITEM001', 'Laptop Computer', 'High-performance business laptop', 'Electronics', 'Each', 1299.99, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System'),
    ('ITEM002', 'Office Chair', 'Ergonomic office chair', 'Furniture', 'Each', 299.99, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System'),
    ('ITEM003', 'Paper A4', 'Office paper A4 size', 'Office Supplies', 'Ream', 12.99, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System'),
    ('SERV001', 'IT Consulting', 'IT consulting services', 'Services', 'Hour', 125.00, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System');
GO

PRINT 'Phase 4: Document entities migration completed successfully'
GO
