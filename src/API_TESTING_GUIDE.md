# Sivar ERP Core API Testing Guide

## API Testing with Swagger UI

The API is running at: **http://localhost:5108**
Swagger documentation: **http://localhost:5108/swagger**

## Accounting Endpoints Available

### 1. Account Types (GET /api/accounts/types)
- **Purpose**: Get all available account types
- **Expected Response**: ["Asset", "Liability", "Equity", "Revenue", "Expense"]

### 2. Account Categories (GET /api/accounts/categories) 
- **Purpose**: Get all available account categories
- **Expected Response**: Enum values for account categories

### 3. Create Account (POST /api/accounts)
- **Purpose**: Create a new account
- **Sample Request Body**:
```json
{
  "code": "1000",
  "name": "Cash",
  "description": "Cash on hand and in bank",
  "type": "Asset", 
  "category": "CurrentAssets",
  "isActive": true,
  "currency": "USD"
}
```

### 4. Get Account by ID (GET /api/accounts/{id})
- **Purpose**: Retrieve a specific account
- **Parameter**: Account GUID

### 5. Get All Accounts (GET /api/accounts)
- **Purpose**: Get all accounts with optional filtering
- **Query Parameters**: 
  - pageNumber (default: 1)
  - pageSize (default: 10) 
  - searchTerm (optional)

### 6. Get Accounts by Type (GET /api/accounts/by-type/{accountType})
- **Purpose**: Filter accounts by type (Asset, Liability, etc.)
- **Parameter**: AccountType enum value

### 7. Get Accounts by Category (GET /api/accounts/by-category/{category})
- **Purpose**: Filter accounts by category
- **Parameter**: AccountCategory enum value

### 8. Chart of Accounts (GET /api/accounts/chart-of-accounts)
- **Purpose**: Get complete chart of accounts structure
- **Response**: Hierarchical account structure

## Testing Steps

1. **Open Swagger UI**: Navigate to http://localhost:5108/swagger
2. **Test Account Types**: Click on GET /api/accounts/types → "Try it out" → "Execute"
3. **Test Account Categories**: Click on GET /api/accounts/categories → "Try it out" → "Execute"  
4. **Create Sample Account**: Use POST /api/accounts with the sample JSON above
5. **Verify Creation**: Use GET /api/accounts to see your created account
6. **Test Filtering**: Try GET /api/accounts/by-type/Asset to filter by account type

## Notes

- All endpoints support proper error handling
- Multi-tenant isolation is implemented (using hardcoded tenant for demo)
- Responses follow consistent ApiResponse pattern
- API includes comprehensive validation and business rules
