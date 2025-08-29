# Import Files Documentation

This document provides comprehensive information about all CSV import files supported by the SivarErp system. The import system uses a priority-based approach (1.0 to 10.0) to ensure proper dependency resolution during bulk imports.

## Import Priority Order

The system imports files in the following order to maintain data integrity:

1. **Accounts** (Priority: 1.0) - Foundation entities referenced by other modules
2. **Tax Groups** (Priority: 2.0) - Grouping entities for taxes
3. **Taxes** (Priority: 3.0) - Tax definitions
4. **Tax Rules** (Priority: 4.0) - Tax application rules
5. **Business Entities** (Priority: 5.0) - Customers, vendors, etc.
6. **Document Types** (Priority: 6.0) - Document type definitions
7. **Items** (Priority: 7.0) - Products and services
8. **Group Memberships** (Priority: 8.0) - Entity-to-group associations
9. **Payment Methods** (Priority: 9.0) - Payment method definitions
10. **Transactions** (Priority: 10.0) - Actual business transactions (depends on all above)

---

## 1. Accounts (accounts.csv)

**Purpose**: Chart of accounts - the foundation of the accounting system. Defines all accounts used for financial transactions.

**Priority**: 1.0 (First to import)

**Required Headers**:
- `AccountName` (Required)
- `OfficialCode` (Required)
- `AccountType` (Required)

**Optional Headers**:
- `ParentOfficialCode`
- `BalanceAndIncomeLineId`

**Properties**:
- **AccountName**: The descriptive name of the account (e.g., "Cash in Bank", "Accounts Receivable")
- **OfficialCode**: Unique identifier/code for the account (e.g., "1100", "1200")
- **AccountType**: Type of account (Asset, Liability, Equity, Revenue, Expense)
- **ParentOfficialCode**: Code of parent account for hierarchical structure
- **BalanceAndIncomeLineId**: GUID linking to balance sheet or income statement line items

**Example**:
```csv
AccountName,OfficialCode,AccountType,ParentOfficialCode,BalanceAndIncomeLineId
Cash in Bank,1100,Asset,,550e8400-e29b-41d4-a716-446655440000
Accounts Receivable,1200,Asset,,550e8400-e29b-41d4-a716-446655440001
Sales Revenue,4100,Revenue,,550e8400-e29b-41d4-a716-446655440002
```

**Validation**: Uses AccountValidator with El Salvador account type prefixes. Account codes must follow specific format rules.

---

## 2. Tax Groups (taxgroups.csv)

**Purpose**: Groups for organizing taxes and business entities. Used to apply tax rules to groups of entities or items.

**Priority**: 2.0

**Required Headers**:
- `Code` (Required)
- `Name` (Required)
- `GroupType` (Required)

**Optional Headers**:
- `Description`
- `IsEnabled`

**Properties**:
- **Code**: Unique identifier for the tax group
- **Name**: Descriptive name of the tax group
- **GroupType**: Type of group (BusinessEntity, Item) - for documentation purposes
- **Description**: Additional description of the group's purpose
- **IsEnabled**: Boolean indicating if the group is active (default: true)

**Example**:
```csv
Code,Name,Description,IsEnabled,GroupType
RETAIL,Retail Customers,Standard retail customers,true,BusinessEntity
WHOLESALE,Wholesale Customers,Bulk wholesale customers,true,BusinessEntity
SERVICES,Service Items,Professional services,true,Item
```

---

## 3. Taxes (taxes.csv)

**Purpose**: Defines tax rates and rules applied to transactions. Core tax configuration for the system.

**Priority**: 3.0

**Required Headers**:
- `Code` (Required)
- `Name` (Required)
- `TaxType` (Required)
- `ApplicationLevel` (Required)

**Optional Headers**:
- `Percentage`
- `Amount`
- `IsEnabled`
- `IsIncludedInPrice`
- `DebitAccountCode`
- `CreditAccountCode`
- `AccountDescription`

**Properties**:
- **Code**: Unique tax identifier (e.g., "IVA", "VAT")
- **Name**: Descriptive name of the tax
- **TaxType**: Type of tax calculation method
- **ApplicationLevel**: Where the tax is applied (Document, Line)
- **Percentage**: Tax rate as percentage (e.g., 13.0 for 13%)
- **Amount**: Fixed tax amount (alternative to percentage)
- **IsEnabled**: Whether tax is currently active
- **IsIncludedInPrice**: Whether tax is included in item prices
- **DebitAccountCode**: Account for tax debit entries
- **CreditAccountCode**: Account for tax credit entries
- **AccountDescription**: Description for accounting entries

**Example**:
```csv
Code,Name,TaxType,ApplicationLevel,Percentage,Amount,IsEnabled,IsIncludedInPrice,DebitAccountCode,CreditAccountCode,AccountDescription
IVA,Impuesto al Valor Agregado,Percentage,Document,13.0,,true,false,1300,2100,IVA por pagar
ISR,Impuesto Sobre la Renta,Percentage,Line,1.0,,true,false,1301,2101,ISR retenido
```

---

## 4. Tax Rules (taxrules.csv)

**Purpose**: Defines which taxes apply to specific combinations of business entities and items.

**Priority**: 4.0

**Required Headers**:
- `TaxCode` (Required)
- `DocumentOperation` (Required)

**Optional Headers**:
- `BusinessEntityGroupCode`
- `ItemGroupCode`
- `IsEnabled`
- `Priority`

**Properties**:
- **TaxCode**: Reference to tax definition (from taxes.csv)
- **DocumentOperation**: Type of document (Sale, Purchase, etc.)
- **BusinessEntityGroupCode**: Applies to specific business entity group
- **ItemGroupCode**: Applies to specific item group
- **IsEnabled**: Whether the rule is active
- **Priority**: Rule application priority (higher numbers = higher priority)

**Example**:
```csv
TaxCode,DocumentOperation,BusinessEntityGroupCode,ItemGroupCode,IsEnabled,Priority
IVA,SalesInvoice,RETAIL,,true,10
ISR,SalesInvoice,WHOLESALE,SERVICES,true,5
```

---

## 5. Business Entities (businessentities.csv)

**Purpose**: Customers, vendors, and other business partners. Essential for creating transactions.

**Priority**: 5.0

**Required Headers**:
- `Code` (Required)
- `Name` (Required)

**Optional Headers**:
- `Address`
- `City`
- `State`
- `ZipCode`
- `Country`
- `PhoneNumber`
- `Email`

**Properties**:
- **Code**: Unique identifier for the business entity
- **Name**: Full business name
- **Address**: Street address
- **City**: City location
- **State**: State or province
- **ZipCode**: Postal code
- **Country**: Country location
- **PhoneNumber**: Contact phone number
- **Email**: Contact email address

**Example**:
```csv
Code,Name,Address,City,State,ZipCode,Country,PhoneNumber,Email
CUST001,ABC Corporation,123 Main St,San Salvador,San Salvador,01101,El Salvador,+503-2234-5678,contact@abc.com
VEND001,XYZ Suppliers,456 Commerce Ave,Santa Ana,Santa Ana,02101,El Salvador,+503-2445-1234,sales@xyz.com
```

---

## 6. Document Types (documenttypes.csv)

**Purpose**: Defines types of business documents (invoices, purchase orders, etc.) used in transactions.

**Priority**: 6.0

**Required Headers**:
- `Code` (Required)
- `Name` (Required)
- `DocumentOperation` (Required)

**Optional Headers**:
- `IsEnabled`

**Properties**:
- **Code**: Unique document type identifier
- **Name**: Descriptive name of the document type
- **DocumentOperation**: Operation type (SalesInvoice, PurchaseInvoice, etc.)
- **IsEnabled**: Whether document type is active (default: true)

**Example**:
```csv
Code,Name,DocumentOperation,IsEnabled
SI,Sales Invoice,SalesInvoice,true
PI,Purchase Invoice,PurchaseInvoice,true
PO,Purchase Order,PurchaseOrder,true
```

---

## 7. Items (items.csv)

**Purpose**: Products and services that can be sold or purchased. Inventory and service catalog.

**Priority**: 7.0

**Required Headers**:
- `Code` (Required)
- `Type` (Required)
- `Description` (Required)
- `BasePrice` (Required)

**Properties**:
- **Code**: Unique item identifier/SKU
- **Type**: Item type (Product, Service, etc.)
- **Description**: Full description of the item
- **BasePrice**: Base selling price (decimal value)

**Example**:
```csv
Code,Type,Description,BasePrice
PROD001,Product,Premium Coffee Beans 1kg,15.50
SERV001,Service,IT Consulting Per Hour,75.00
PROD002,Product,Office Chair - Ergonomic,125.00
```

---

## 8. Group Memberships (groupmemberships.csv)

**Purpose**: Associates business entities or items with their respective groups for tax rule application.

**Priority**: 8.0

**Required Headers**:
- `GroupId` (Required)
- `EntityId` (Required)
- `GroupType` (Required)

**Optional Headers**:
- `Oid`

**Properties**:
- **Oid**: Unique identifier (GUID) for the membership record
- **GroupId**: Reference to group (from tax groups)
- **EntityId**: Reference to entity or item being grouped
- **GroupType**: Type of grouping (BusinessEntity, Item)

**Example**:
```csv
Oid,GroupId,EntityId,GroupType
550e8400-e29b-41d4-a716-446655440000,RETAIL,CUST001,BusinessEntity
550e8400-e29b-41d4-a716-446655440001,WHOLESALE,CUST002,BusinessEntity
550e8400-e29b-41d4-a716-446655440002,SERVICES,SERV001,Item
```

---

## 9. Payment Methods (paymentmethods.csv)

**Purpose**: Defines available payment methods for transactions (cash, credit card, bank transfer, etc.).

**Priority**: 9.0

**Required Headers**:
- `Code` (Required)
- `Name` (Required)
- `Type` (Required)

**Optional Headers**:
- `AccountCode`
- `RequiresBankAccount`
- `RequiresReference`
- `IsActive`

**Properties**:
- **Code**: Unique payment method identifier
- **Name**: Descriptive name of payment method
- **Type**: Payment method type (Cash, CreditCard, BankTransfer, etc.)
- **AccountCode**: Associated accounting account
- **RequiresBankAccount**: Boolean - whether bank account info is required
- **RequiresReference**: Boolean - whether reference number is required
- **IsActive**: Boolean - whether payment method is currently available

**Example**:
```csv
Code,Name,Type,AccountCode,RequiresBankAccount,RequiresReference,IsActive
CASH,Cash Payment,Cash,1100,false,false,true
CC,Credit Card,CreditCard,1150,false,true,true
BANK,Bank Transfer,BankTransfer,1100,true,true,true
CHECK,Check Payment,Check,1100,true,true,true
```

---

## 10. Transactions (transactions.csv)

**Purpose**: Actual business transactions with associated ledger entries. Contains the financial data.

**Priority**: 10.0 (Last to import - depends on all other entities)

**Transaction Headers**:
- `TransactionId` (or `TransactionNumber`)
- `Date` (or `TransactionDate`)
- `Description`
- `DocumentId` (or `DocumentNumber`)

**Ledger Entry Headers**:
- `LedgerEntryNumber` (or `EntryId`)
- `TransactionId` (or `TransactionNumber`)
- `AccountId`
- `OfficialCode`
- `AccountName`
- `EntryType`
- `Amount`

**Properties**:

**Transaction Properties**:
- **TransactionId/TransactionNumber**: Unique transaction identifier
- **Date/TransactionDate**: Transaction date (YYYY-MM-DD format)
- **Description**: Transaction description
- **DocumentId/DocumentNumber**: Associated document reference

**Ledger Entry Properties**:
- **LedgerEntryNumber/EntryId**: Unique entry identifier
- **TransactionId/TransactionNumber**: Links to transaction
- **AccountId**: Internal account identifier
- **OfficialCode**: Account code (must exist in accounts.csv)
- **AccountName**: Account name for reference
- **EntryType**: Debit or Credit
- **Amount**: Transaction amount (decimal)

**Special Format**: Transactions can be imported in a combined format with sections:

```csv
# TRANSACTIONS
TransactionId,Date,Description,DocumentId
TXN001,2024-01-15,Sale to Customer ABC,INV001
TXN002,2024-01-16,Purchase from Vendor XYZ,PO001

# LEDGER ENTRIES
EntryId,TransactionId,AccountId,OfficialCode,AccountName,EntryType,Amount
LE001,TXN001,1,1200,Accounts Receivable,Debit,113.00
LE002,TXN001,2,4100,Sales Revenue,Credit,100.00
LE003,TXN001,3,2100,IVA por Pagar,Credit,13.00
```

---

## Import Process

### Single File Import
Each file type can be imported individually through the Import Controller by selecting the appropriate FileType.

### Bulk Import (ZIP)
Multiple files can be imported simultaneously by creating a ZIP archive containing CSV files with the correct naming convention:
- `accounts.csv`
- `taxgroups.csv`
- `taxes.csv`
- `taxrules.csv`
- `businessentities.csv`
- `documenttypes.csv`
- `items.csv`
- `groupmemberships.csv`
- `paymentmethods.csv`
- `transactions.csv`

The system will automatically process files in priority order to maintain referential integrity.

### Validation Rules
- All imports include header validation to ensure required columns are present
- Data validation is performed using specific validators for each entity type
- Error messages are collected and reported for failed imports
- Successful imports report the number of records imported

### File Format Requirements
- CSV format with comma separators
- UTF-8 encoding (with or without BOM) or Windows-1252
- Quoted fields supported for values containing commas
- Empty lines are skipped
- First line must contain headers (case-insensitive matching)

### Error Handling
- Column count mismatches are reported with line numbers
- Validation failures include specific error details
- Import continues processing after individual record failures
- Summary of successful imports and errors is provided
