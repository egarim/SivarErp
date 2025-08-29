# Transaction Import/Export Service

This document describes the Transaction import/export functionality that has been updated to follow the same pattern as other import/export services in the system.

## Files Updated/Created

### 1. Updated POCO Implementation
- `Sivar.Erp\Modules\ImportExport\TransactionsImportExportService.cs` - Updated to implement `ITransactionImportExportService` with async methods

### 2. New XAF Implementation
- `Sivar.Erp.Xaf.Module\Services\ImportExport\XafTransactionsImportExportService.cs` - **New XAF service following the same pattern as XafAccountImportExportService**

### 3. Existing XAF Implementation  
- `Sivar.Erp.Xaf.Module\Services\ImportExport\XafTransactionImportExportService.cs` - Existing service (different implementation approach)

### 3. Updated Files
- `Sivar.Erp.Xaf.Module\BusinessObjects\ImportFile.cs` - Added Transactions to FileType enum
- `Sivar.Erp.Xaf.Module\Controllers\ImportController.cs` - Added Transaction import handling using the new XafTransactionsImportExportService

## CSV Format

The Transaction CSV format is a **composite format** that includes both transactions and their associated ledger entries in a single file, separated by section headers.

### Format Structure:
```csv
# TRANSACTIONS
TransactionId,Date,Description,DocumentId

# LEDGER ENTRIES  
EntryId,TransactionId,AccountId,OfficialCode,AccountName,EntryType,Amount
```

### Transaction Section Fields:
- **TransactionId**: Unique transaction identifier
- **Date**: Transaction date (YYYY-MM-DD format)
- **Description**: Transaction description (quoted if contains commas)
- **DocumentId**: Reference document number

### Ledger Entry Section Fields:
- **EntryId**: Unique ledger entry identifier
- **TransactionId**: References the transaction this entry belongs to
- **AccountId**: (Legacy field, can be same as OfficialCode)
- **OfficialCode**: Account official code
- **AccountName**: Account name (quoted if contains commas)
- **EntryType**: Either "Debit" or "Credit"
- **Amount**: Entry amount (decimal format)

## Key Features

### 1. Dual Format Support
The service supports both:
- **Separate CSV files**: One for transactions, one for ledger entries
- **Combined CSV file**: Both sections in a single file with section headers

### 2. Validation
- Validates that debits equal credits for each transaction
- Checks for missing transactions when processing ledger entries
- Provides detailed error reporting with line numbers

### 3. Error Handling
- Comprehensive error collection and reporting
- Continues processing after encountering errors
- Returns both successfully imported data and error details

## Usage

### Import Single File

1. In XAF, go to ImportFile view
2. Select FileType: Transactions
3. Upload a CSV file with the combined format
4. Click Import

### Import from ZIP

1. Create a ZIP file containing `transactions.csv`
2. In XAF, go to ImportFile view
3. Select FileType: All
4. Upload the ZIP file
5. Click Import

## Example CSV

See `sample-transactions.csv` for a complete example showing:
- Sales invoice payment transaction
- Purchase invoice transaction  
- Bank transfer transaction

Each transaction demonstrates proper debit/credit balance.

## Architecture Consistency

The transaction import/export service now follows the same pattern as other services:

1. **Interface Layer**: `ITransactionImportExportService` defines async contract
2. **POCO Layer**: `TransactionsImportExportService` provides infrastructure implementation  
3. **XAF Layer**: `XafTransactionsImportExportService` integrates with XAF ObjectSpace (new, follows AccountImportExportService pattern)
4. **Legacy XAF Layer**: `XafTransactionImportExportService` (existing, different approach)
5. **UI Layer**: `ImportController` handles user interactions using the new XafTransactionsImportExportService

## Technical Implementation Details

### Transaction Creation
- Creates XAF Transaction entities using ObjectSpace
- Properly sets audit fields (InsertedBy, InsertedAt, etc.)
- Links ledger entries to transactions via foreign keys

### Ledger Entry Creation  
- Creates XAF LedgerEntry entities
- Validates account codes against existing accounts
- Maintains referential integrity with parent transactions

### Data Validation
- Ensures accounting equation balance (Debits = Credits)
- Validates date formats and numeric amounts
- Checks for duplicate transaction numbers

### Error Recovery
- Collects all errors instead of stopping at first failure
- Provides meaningful error messages with context
- Allows partial imports when some data is valid

## Integration Notes

The transaction import/export service integrates seamlessly with:
- XAF ObjectSpace for data persistence
- Existing account validation systems
- Import controller for unified UI experience
- ZIP import functionality for bulk operations

This completes the transaction import/export functionality and brings it in line with the established patterns used by other entity import/export services in the system.
