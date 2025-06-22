# Payment Methods Module

This module provides payment processing functionality for sales and purchase transactions in the Sivar ERP system.

## Overview

The Payment Methods module allows you to:

- Define different payment methods (Cash, Check, Bank Transfer, Credit Card, etc.)
- Process payments for sales invoices (money received from customers)
- Process payments for purchase invoices (money paid to suppliers)
- Generate proper accounting journal entries for all payment transactions
- Track payment status and references

## Components

### Models

- `PaymentMethodDto`: Defines payment methods available in the system
- `PaymentDto`: Represents individual payment transactions
- `PaymentMethodType`: Enum for different types of payment methods
- `PaymentStatus`: Enum for payment status tracking

### Services

- `IPaymentService`: Core payment processing service
- `IPaymentMethodService`: Payment method management service
- `PaymentService`: Implementation of payment processing
- `PaymentMethodService`: Implementation of payment method management

## Usage in Tests

The payment functionality has been integrated into the `CompleteAccountingWorkflowTest` and demonstrates:

1. **Payment Method Setup**:

   - Cash (Account Code: 1100)
   - Check (Account Code: 1110) - Requires bank account and reference
   - Bank Transfer (Account Code: 1120) - Requires bank account and reference
   - Credit Card (Account Code: 1130) - Requires reference
   - Debit Card (Account Code: 1140) - Requires reference
   - Digital Wallet (Account Code: 1150) - Requires reference

2. **Sales Payment Processing**:

   - Customer pays invoice via cash
   - Creates journal entries: Debit Cash, Credit Accounts Receivable

3. **Purchase Payment Processing**:
   - Company pays supplier via check
   - Creates journal entries: Debit Accounts Payable, Credit Bank Account

## Accounting Impact

### Sales Payment (Customer Pays)

```
Debit:  Cash/Bank Account        $XXX.XX
Credit: Accounts Receivable      $XXX.XX
```

### Purchase Payment (We Pay Supplier)

```
Debit:  Accounts Payable         $XXX.XX
Credit: Cash/Bank Account        $XXX.XX
```

## Key Features

1. **Multiple Payment Methods**: Support for various payment types with different requirements
2. **Automatic GL Posting**: Payments generate proper double-entry journal entries
3. **Account Mapping**: Each payment method can be linked to specific GL accounts
4. **Payment Tracking**: Full audit trail of all payment transactions
5. **Document Integration**: Payments are linked to their source documents
6. **Status Management**: Track payment status from pending to completed

## Test Execution

Run the `CompleteAccountingWorkflowTest.ExecuteCompleteWorkflowTest()` method to see the complete flow:

1. Data import and setup
2. Tax accounting profile setup
3. Document accounting profile setup
4. Purchase transaction creation and posting
5. Sales transaction creation and posting
6. Journal entry analysis
7. **Payment processing (NEW)**
8. Performance metrics

The test demonstrates end-to-end payment processing integrated with the existing accounting workflow.
