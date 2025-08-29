# PaymentMethod Import/Export Service

This document describes the PaymentMethod import/export functionality that follows the same pattern as the existing Account import/export service.

## Files Created

### 1. Interface
- `Sivar.Erp\Modules\ImportExport\IPaymentMethodImportExportService.cs` - Interface definition

### 2. Validator
- `Sivar.Erp\Modules\Payments\Validation\PaymentMethodValidator.cs` - Business rule validation

### 3. POCO Implementation
- `Sivar.Erp\Infrastructure\ImportExport\PaymentMethodImportExportService.cs` - Infrastructure layer implementation

### 4. XAF Implementation
- `Sivar.Erp.Xaf.Module\Services\ImportExport\XafPaymentMethodImportExportService.cs` - XAF ObjectSpace implementation

### 5. Updated Files
- `Sivar.Erp.Xaf.Module\BusinessObjects\ImportFile.cs` - Added PaymentMethods to FileType enum
- `Sivar.Erp.Xaf.Module\Controllers\ImportController.cs` - Added PaymentMethod import handling

## CSV Format

The PaymentMethod CSV format includes the following columns:

```csv
Code,Name,Type,AccountCode,RequiresBankAccount,RequiresReference,IsActive
```

### Required Fields
- **Code**: Unique identifier (max 50 chars, alphanumeric + underscores)
- **Name**: Display name (max 200 chars)
- **Type**: PaymentMethodType enum value (Cash, Check, BankTransfer, CreditCard, DebitCard, DigitalWallet, Other)

### Optional Fields
- **AccountCode**: GL Account code for this payment method (max 50 chars)
- **RequiresBankAccount**: Boolean indicating if bank account is required
- **RequiresReference**: Boolean indicating if reference is required
- **IsActive**: Boolean indicating if payment method is active (defaults to true)

## PaymentMethodType Enum Values

- `Cash`
- `Check`
- `BankTransfer`
- `CreditCard`
- `DebitCard`
- `DigitalWallet`
- `Other`

## Example CSV

See `sample-paymentmethods.csv` for a complete example with common payment methods.

## Usage

### Import Single File

1. In XAF, go to ImportFile view
2. Select FileType: PaymentMethods
3. Upload a CSV file with the correct format
4. Click Import

### Import from ZIP

1. Create a ZIP file containing `paymentmethods.csv`
2. In XAF, go to ImportFile view
3. Select FileType: All
4. Upload the ZIP file
5. Click Import

## Validation Rules

The PaymentMethodValidator enforces these rules:

1. **Code** and **Name** are required
2. **Code** must be unique and contain only alphanumeric characters and underscores
3. **Code** maximum length: 50 characters
4. **Name** maximum length: 200 characters
5. **AccountCode** maximum length: 50 characters (if provided)
6. Cash payment methods should not require bank account

## Architecture

The implementation follows the same pattern as the Account import/export service:

1. **Interface Layer**: `IPaymentMethodImportExportService` defines the contract
2. **Validation Layer**: `PaymentMethodValidator` handles business rules
3. **Infrastructure Layer**: `PaymentMethodImportExportService` provides POCO implementation
4. **XAF Layer**: `XafPaymentMethodImportExportService` integrates with XAF ObjectSpace
5. **UI Layer**: `ImportController` handles user interactions

## Error Handling

The service provides detailed error reporting including:

- Missing required headers
- Invalid data types
- Validation failures
- Duplicate codes (both in database and import file)
- Column count mismatches

Errors are collected and returned as a list of strings, allowing the user to see all issues at once rather than stopping at the first error.
