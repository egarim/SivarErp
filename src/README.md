# Sivar ERP Core - Modern Multi-Tenant ERP System

## Overview

Sivar ERP Core is a modern, cloud-native ERP system built with .NET 9, Entity Framework Core, and Keycloak authentication. This implementation represents a complete migration from the legacy ObjectDb-based system to a scalable, multi-tenant architecture.

## Architecture

### Clean Architecture Layers

- **Domain Layer** (`Sivar.Erp.Core.Domain`): Core business entities and interfaces
- **Infrastructure Layer** (`Sivar.Erp.Core.Infrastructure`): Data access, repositories, and external services
- **Application Layer** (`Sivar.Erp.Core.Application`): Business logic and application services
- **API Layer** (`Sivar.Erp.Core.Api`): REST API endpoints and controllers
- **Shared Layer** (`Sivar.Erp.Core.Shared`): DTOs and contracts

### Key Features

#### 🏢 Multi-Tenancy
- **Company-based isolation**: Each company operates as a separate tenant
- **Branch support**: Multiple branches per company with user access control
- **Data isolation**: Automatic filtering of data by company context

#### 🔐 Authentication & Authorization
- **Keycloak integration**: Enterprise-grade identity and access management
- **Multiple identity providers**: Email, Google, Facebook, Microsoft
- **Role-based access control**: Owner, Admin, Manager, User, ReadOnly roles
- **JWT bearer tokens**: Stateless authentication for APIs

#### 📊 Database Support
- **Development**: InMemory database for rapid development
- **Production**: PostgreSQL with full ACID compliance
- **Migrations**: Automatic database schema management

#### 🔍 Observability
- **Structured logging**: Serilog with multiple sinks
- **Performance monitoring**: Built-in tracking for all operations
- **Health checks**: Comprehensive system health monitoring

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- Docker (for Keycloak and PostgreSQL)
- Git

### Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/egarim/SivarErp.git
   cd SivarErp/src
   ```

2. **Start Keycloak (optional for development)**
   ```bash
   cd Infrastructure/Keycloak
   docker-compose up -d
   ```

3. **Run the API**
   ```bash
   cd Sivar.Erp.Core.Api
   dotnet run
   ```

4. **Access the API**
   - Swagger UI: `http://localhost:5000` or `https://localhost:7001`
   - Health Check: `http://localhost:5000/health`

### Sample API Calls

#### Get Companies
```bash
curl -X GET "http://localhost:5000/api/companies" -H "accept: application/json"
```

#### Create Company
```bash
curl -X POST "http://localhost:5000/api/companies" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Company",
    "country": "US",
    "currency": "USD",
    "timeZone": "UTC"
  }'
```

## Project Structure

```
src/
├── Sivar.Erp.Core.Domain/           # Domain entities and interfaces
│   ├── Entities/
│   │   ├── Identity/                # User, Company, Branch entities
│   │   ├── BaseEntity.cs           # Base entity with auditing
│   │   └── Interfaces/             # Repository and context interfaces
│   └── ValueObjects/               # Money, CompanyId, etc.
│
├── Sivar.Erp.Core.Infrastructure/   # Data access and external services
│   ├── Data/
│   │   ├── ErpDbContext.cs         # Main DbContext
│   │   └── Configurations/         # Entity configurations
│   ├── Repositories/               # Repository implementations
│   └── MultiTenancy/               # Multi-tenant infrastructure
│
├── Sivar.Erp.Core.Application/      # Application services
│   ├── Services/                   # Business logic services
│   ├── DTOs/                       # Data transfer objects
│   └── Interfaces/                 # Service interfaces
│
├── Sivar.Erp.Core.Api/              # REST API
│   ├── Controllers/                # API controllers
│   ├── Program.cs                  # Application startup
│   └── appsettings.json           # Configuration
│
├── Sivar.Erp.Core.Shared/          # Shared contracts
│   └── DTOs/                       # Request/Response DTOs
│
└── Infrastructure/
    └── Keycloak/                   # Keycloak configuration
        ├── realm-config.json       # Realm setup
        └── docker-compose.yml      # Container setup
```

## Configuration

### Database Configuration

#### Development (InMemory)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "InMemory"
  }
}
```

#### Production (PostgreSQL)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=sivar_erp_main;Username=sivar_erp_user;Password=your_password"
  }
}
```

### Keycloak Configuration

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/sivar-erp",
    "RequireHttps": false,
    "ClientId": "sivar-erp-api",
    "ClientSecret": "your-client-secret"
  }
}
```

## Testing

### Running Tests
```bash
dotnet test
```

### Test Data
The system automatically seeds test data when running in development mode:
- Test Company: "Test Company"
- Test Branch: "Main Branch"
- Test User: "test@sivarerp.com"

## API Documentation

### Companies API

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/companies` | GET | List all companies |
| `/api/companies/{id}` | GET | Get company by ID |
| `/api/companies` | POST | Create new company |
| `/api/companies/{id}` | PUT | Update company |
| `/api/companies/{id}` | DELETE | Delete company |

### Authentication

All API endpoints (except health checks) require authentication. Include the JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## Logging

The system uses Serilog for structured logging with multiple sinks:

- **Console**: Development logging
- **File**: Persistent log files in `logs/` directory
- **Structured data**: JSON format for easy parsing

### Log Levels
- **Information**: General application flow
- **Warning**: Unexpected situations
- **Error**: Error conditions
- **Debug**: Detailed troubleshooting information

## Security

### Authentication Flow
1. User authenticates with Keycloak (email, Google, Facebook, Microsoft)
2. Keycloak issues JWT token
3. API validates JWT token on each request
4. User context is established for multi-tenant operations

### Authorization
- **Company-based**: Users can only access their associated companies
- **Role-based**: Different permissions based on user role
- **Resource-based**: Granular permissions for specific operations

## Multi-Tenancy

### Data Isolation
- Each company operates as a separate tenant
- Automatic query filtering by company context
- No cross-tenant data access

### User Management
- Users can belong to multiple companies
- Different roles per company
- Invitation-based user onboarding

## Deployment

### Docker Support
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "Sivar.Erp.Core.Api.dll"]
```

### Environment Variables
- `ConnectionStrings__DefaultConnection`: Database connection
- `Keycloak__Authority`: Keycloak server URL
- `Keycloak__ClientSecret`: Client secret for API

## Migration from Legacy System

This system is designed to replace the legacy ObjectDb-based Sivar.Erp system:

### Migration Benefits
- **Scalability**: Cloud-native architecture
- **Performance**: EF Core optimizations
- **Security**: Enterprise-grade authentication
- **Multi-tenancy**: Built-in tenant isolation
- **Observability**: Comprehensive monitoring

### Migration Strategy
1. ✅ **Phase 1**: Foundation infrastructure (COMPLETED)
2. 🚧 **Phase 2**: User management and authentication
3. ⏳ **Phase 3**: Core business entities migration
4. ⏳ **Phase 4**: Application services implementation
5. ⏳ **Phase 5**: Client applications (Blazor, MAUI)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## License

This project is proprietary software owned by Sivar ERP.

## Support

For support and questions, please contact the development team.

---

**Note**: The legacy `Sivar.Erp` and `Tests` projects should not be modified as they are maintained for backward compatibility only.
