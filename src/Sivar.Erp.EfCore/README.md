# Sivar.Erp.EfCore

This project provides Entity Framework Core support for the Sivar ERP system. It includes entities, DbContext, and repository implementations that map to the interfaces defined in the main ERP project.

## Features

- **Entity Framework Entities**: All major ERP entities mapped to database tables
- **DbContext**: Complete database context with relationships and indexing
- **Repository Pattern**: Implementation of IObjectDb interface using Entity Framework
- **Service Extensions**: Easy dependency injection setup
- **Support for Multiple Databases**: SQL Server, In-Memory, and custom configurations

## Installation

Add the project reference to your application:

```xml
<ProjectReference Include="..\Sivar.Erp.EfCore\Sivar.Erp.EfCore.csproj" />
```

## Configuration

### SQL Server

```csharp
services.AddSivarErpEntityFramework("Server=localhost;Database=SivarErp;Trusted_Connection=true;");
```

### In-Memory Database (for testing)

```csharp
services.AddSivarErpEntityFrameworkInMemory("TestDatabase");
```

### Custom Configuration

```csharp
services.AddSivarErpEntityFramework(options =>
{
    options.UseSqlServer(connectionString);
    options.EnableSensitiveDataLogging(); // Only for development
});
```

## Usage

### Using the Repository

```csharp
public class MyService
{
    private readonly EfObjectDbRepository _repository;

    public MyService(EfObjectDbRepository repository)
    {
        _repository = repository;
    }

    public async Task<IList<IAccount>> GetAccountsAsync()
    {
        return _repository.Accounts;
    }

    public async Task AddAccountAsync(Account account)
    {
        _repository.DbContext.Accounts.Add(account);
        await _repository.SaveChangesAsync();
    }
}
```

### Using DbContext Directly

```csharp
public class AccountService
{
    private readonly SivarErpDbContext _context;

    public AccountService(SivarErpDbContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> GetActiveAccountsAsync()
    {
        return await _context.Accounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.OfficialCode)
            .ToListAsync();
    }
}
```

## Entities

The following entities are included:

### Accounting
- `Account` - Chart of accounts entries
- `FiscalPeriod` - Fiscal period definitions
- `Transaction` - Financial transactions
- `LedgerEntry` - Individual ledger entries
- `TransactionBatch` - Transaction batches

### Business & Documents
- `BusinessEntity` - Customers, suppliers, etc.
- `DocumentType` - Document type definitions
- `DocumentAccountingProfile` - Accounting rules for documents

### Inventory
- `Item` - Basic items
- `InventoryItem` - Items with inventory tracking
- `StockLevel` - Current stock levels
- `InventoryTransaction` - Inventory movements
- `InventoryReservation` - Inventory reservations
- `InventoryLayer` - FIFO/LIFO costing layers

### Taxes
- `Tax` - Tax definitions
- `TaxGroup` - Tax groups
- `TaxRule` - Tax application rules
- `GroupMembership` - Entity/item group memberships

### Payments
- `PaymentMethod` - Payment method definitions
- `Payment` - Payment records

### Security
- `User` - System users
- `Role` - User roles
- `SecurityEvent` - Security audit events

### System
- `ActivityRecord` - Activity logging
- `Sequence` - Number sequences
- `PerformanceLog` - Performance monitoring

## Database Migrations

To create and apply migrations:

```bash
# Add migration
dotnet ef migrations add InitialCreate --project Sivar.Erp.EfCore

# Update database
dotnet ef database update --project Sivar.Erp.EfCore
```

## Configuration in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
builder.Services.AddSivarErpEntityFramework(
    builder.Configuration.GetConnectionString("DefaultConnection"));

// Register repository
builder.Services.AddScoped<EfObjectDbRepository>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SivarErpDbContext>();
    context.Database.EnsureCreated();
}
```

## Advanced Usage

### Custom Queries

```csharp
// Complex queries using LINQ
var salesTransactions = await _context.Transactions
    .Include(t => t.LedgerEntries)
    .Where(t => t.TransactionDate >= DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)))
    .Where(t => t.LedgerEntries.Any(le => le.OfficialCode.StartsWith("4"))) // Revenue accounts
    .ToListAsync();
```

### Bulk Operations

```csharp
// Bulk insert
var accounts = GenerateChartOfAccounts();
_context.Accounts.AddRange(accounts);
await _context.SaveChangesAsync();
```

### Transaction Management

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Multiple operations
    _context.Accounts.Add(newAccount);
    _context.Transactions.Add(newTransaction);
    
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

## Performance Considerations

- Use `Include()` for eager loading related data
- Use `AsNoTracking()` for read-only queries
- Consider pagination for large result sets
- Use compiled queries for frequently executed queries
- Optimize indexes based on query patterns

## Testing

For unit testing, use the in-memory database:

```csharp
var options = new DbContextOptionsBuilder<SivarErpDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;

using var context = new SivarErpDbContext(options);
var repository = new EfObjectDbRepository(context);

// Your test code here
```
