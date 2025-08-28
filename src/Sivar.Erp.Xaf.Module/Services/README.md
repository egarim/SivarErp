# XAF Module Services

This directory contains XAF-specific service implementations that integrate with DevExpress XAF framework using `IObjectSpace` for data operations.

## Directory Structure

```
Services/
├── ImportExport/           # Import/Export service implementations
│   ├── XafAccountImportExportService.cs
│   └── README.md
└── README.md              # This file
```

## Overview

The services in this directory are XAF adaptations of the existing POCO services found in the main ERP modules. They provide the same functionality but are specifically designed to work with XAF's data access patterns and Entity Framework entities.

## Key Differences from POCO Services

### Data Access Pattern
- **POCO Services**: Work with in-memory collections and manual persistence
- **XAF Services**: Use `IObjectSpace` for database operations with automatic transaction management

### Entity Types
- **POCO Services**: Work with DTO classes (e.g., `AccountDto`)
- **XAF Services**: Work with Entity Framework entities (e.g., `Sivar.Erp.EfCore.Entities.Account`)

### Persistence Strategy
- **POCO Services**: Manual collection management
- **XAF Services**: Automatic persistence through `ObjectSpace.CommitChanges()`

### Validation Approach
- **POCO Services**: Direct validation on DTOs
- **XAF Services**: Convert entities to DTOs for validation compatibility

## Integration Guidelines

### Service Registration
Services should be registered in the XAF application's dependency injection container with appropriate scope:

```csharp
// Scoped services (recommended for data operations)
services.AddScoped<IServiceInterface, XafServiceImplementation>();
```

### ObjectSpace Management
Services receive `IObjectSpace` through constructor injection and rely on XAF's lifecycle management:

```csharp
public class XafService : IService
{
    private readonly IObjectSpace _objectSpace;
    
    public XafService(IObjectSpace objectSpace)
    {
        _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
    }
}
```

### Error Handling
XAF services should:
1. Use XAF's transaction boundaries through ObjectSpace
2. Provide detailed error messages for business rule violations
3. Log operations when logger is available
4. Handle database-specific exceptions appropriately

## Available Services

### ImportExport Services
- **XafAccountImportExportService**: Chart of accounts CSV import/export with XAF persistence

## Best Practices

### 1. Dependency Injection
- Accept required dependencies through constructor injection
- Use optional parameters for non-critical dependencies (e.g., logging)
- Follow XAF's recommended service lifetime scopes

### 2. Error Handling
- Always validate inputs before processing
- Use structured error messages that can be displayed to users
- Log exceptions with sufficient context for debugging

### 3. Performance
- Leverage XAF's built-in performance optimizations
- Use efficient queries through ObjectSpace
- Consider batch operations for large data sets

### 4. Validation
- Reuse existing validation logic where possible
- Maintain compatibility with POCO service validation patterns
- Provide clear validation error messages

### 5. Logging
- Support optional logging through `ILogger<T>`
- Log significant operations (start, completion, errors)
- Use structured logging with relevant context

## Future Expansion

This directory will grow to include XAF implementations of other import/export services such as:
- Tax import/export services
- Business entity import/export services
- Document type import/export services
- Inventory item import/export services

Each service will follow the same patterns established by the `XafAccountImportExportService` for consistency and maintainability.
