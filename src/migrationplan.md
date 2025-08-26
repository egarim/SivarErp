# Sivar ERP Migration Plan: From ObjectDb to EF Core

## Overview
This document outlines the incremental migration strategy from the current `Sivar.Erp` project (using in-memory ObjectDb) to the new `Sivar.Erp.Core` project (using Entity Framework Core with InMemory and PostgreSQL support). The migration will be modular, non-breaking, and support multi-tenant and multi-branch scenarios.

## Migration Goals
1. **Zero Downtime**: Current functionality remains operational during migration
2. **Incremental**: Module-by-module migration avoiding breaking changes
3. **Multi-Tenant Support**: DbContext design for tenant isolation
4. **Multi-Branch Support**: Database versioning and branch-specific schemas
5. **EF Core Integration**: Replace ObjectDb with proper ORM persistence
6. **Test Recreation**: Rebuild `CompleteAccountingWorkflowTest.cs` using new infrastructure

## Architecture Comparison

### Current Architecture (Sivar.Erp)
- **Data Layer**: In-memory `IObjectDb` with collection-based storage
- **Modules**: Direct dependency on `IObjectDb`
- **Services**: Stateful services with ObjectDb injection
- **Testing**: Direct ObjectDb manipulation
- **Architecture**: Monolithic desktop application

### Target Architecture (Sivar.Erp.Core) - Client-Server

#### Server Side (Cloud API)
- **Data Layer**: EF Core DbContext with PostgreSQL
- **API Layer**: REST API + SignalR for real-time updates
- **Business Layer**: Domain services with repository pattern
- **Authentication**: Keycloak with multiple providers (Email, Gmail, Facebook, etc.)
- **Authorization**: Role-based access control per company/branch
- **Multi-Tenancy**: Hierarchical model (User → Companies → Branches → Invited Users)
- **User Management**: Company ownership, user invitations, role assignments

#### Client Side (Multiple Platforms)
- **Blazor WebAssembly**: SPA with HTTP clients, offline capabilities
- **Blazor Server**: Server-side rendering with SignalR connection
- **MAUI Hybrid**: Mobile app hosting Blazor components
- **Shared Components**: Reusable Blazor UI components across all clients
- **Data Access**: HTTP clients + SignalR hubs (no direct database access)
- **Authentication**: Keycloak integration with social providers
- **Company/Branch Selection**: Multi-company dashboard and context switching

#### Shared Libraries
- **Contracts**: DTOs, API contracts, SignalR hub interfaces
- **Components**: Shared Blazor UI components
- **Client Services**: HTTP service abstractions
- **Authentication**: Keycloak client libraries and token management
- **Authorization**: Role and permission models

## Phase 1: Foundation Infrastructure (Weeks 1-2)

### 1.1 Project Structure Setup

**Projects to Create:**

**Note:** `Sivar.Erp.Core` project already exists with advanced logging, performance monitoring, and telemetry infrastructure that will be leveraged and extended.

```
Sivar.Erp.Core/                         # ✅ EXISTS - Contains logging, performance, telemetry
├── Infrastructure/
│   ├── Logging/                        # ✅ EXISTS - ErpLoggingService, SerilogConfiguration
│   ├── Performance/                    # ✅ EXISTS - InMemoryPerformanceMonitor, MemoryOptimizer
│   └── Telemetry/                      # ✅ EXISTS - OpenTelemetryConfiguration, SivarErpTelemetry
├── Sivar.Erp.Core.Domain/              # Domain entities, value objects
├── Sivar.Erp.Core.Application/         # Application services, interfaces
├── Sivar.Erp.Core.Infrastructure/      # EF Core, repositories, external services
├── Sivar.Erp.Core.Api/                 # REST API + SignalR hubs
├── Sivar.Erp.Core.Shared/              # DTOs, contracts, enums
├── Sivar.Erp.Core.Client.Services/     # HTTP clients, abstractions
├── Sivar.Erp.Core.Components/          # Shared Blazor components (localized)
├── Sivar.Erp.Core.Auth/                # Keycloak integration and auth services
└── Sivar.Erp.Core.Localization/        # Localization resources and services

Client Applications/
├── Sivar.Erp.Web.Server/               # Blazor Server app
├── Sivar.Erp.Web.Wasm/                 # Blazor WebAssembly app
└── Sivar.Erp.Mobile/                   # MAUI Hybrid app

Infrastructure/
├── Keycloak/                           # Keycloak configuration and deployment
│   ├── realm-config.json              # Realm configuration
│   ├── docker-compose.yml             # Keycloak deployment
│   └── providers/                      # Social provider configurations

Testing/
├── Sivar.Erp.Core.Tests/               # Unit & integration tests (NUnit)
├── Sivar.Erp.Api.Tests/                # API integration tests (NUnit + TestServer)
├── Sivar.Erp.Components.Tests/         # Blazor component tests (bUnit + NUnit)
├── Sivar.Erp.E2E.Tests/                # End-to-end tests (Playwright + NUnit)
├── Sivar.Erp.BusinessScenarios.Tests/  # Business scenario integration tests
└── Sivar.Erp.Localization.Tests/       # Localization tests
```

### 1.2 Server-Side Infrastructure (API Project) - Enhanced with Existing Monitoring

**Files to Create in `Sivar.Erp.Core.Api`:**

```
Controllers/
├── AccountingController.cs
├── InventoryController.cs
├── PaymentsController.cs
├── TaxController.cs
├── CompaniesController.cs              # Company management
├── BranchesController.cs               # Branch management
├── UserInvitationsController.cs        # User invitation system
├── UserCompaniesController.cs          # User-company associations
├── MonitoringController.cs             # Performance metrics API (using existing IPerformanceMonitor)
└── HealthController.cs                 # Health checks with telemetry

Hubs/
├── AccountingHub.cs                     # Real-time accounting updates
├── InventoryHub.cs                      # Stock level changes
├── NotificationHub.cs                   # General notifications
├── CompanyHub.cs                        # Company-specific updates
├── UserActivityHub.cs                   # User activity notifications
└── PerformanceHub.cs                    # Real-time performance metrics

Infrastructure/
├── Authentication/
│   ├── KeycloakConfiguration.cs
│   ├── KeycloakAuthHandler.cs
│   ├── CompanyAuthorizationHandler.cs
│   └── BranchAuthorizationHandler.cs
├── Middleware/
│   ├── CompanyContextMiddleware.cs
│   ├── BranchContextMiddleware.cs
│   ├── ExceptionMiddleware.cs
│   ├── RequestLoggingMiddleware.cs      # Integration with existing ErpLoggingService
│   ├── PerformanceTrackingMiddleware.cs # Integration with existing IPerformanceMonitor
│   └── TelemetryMiddleware.cs          # Integration with existing SivarErpTelemetry
├── SignalR/
│   ├── UserConnectionManager.cs
│   ├── CompanyGroupManager.cs
│   └── BranchGroupManager.cs
└── Extensions/
    ├── ServiceCollectionExtensions.cs  # Enhanced with existing infrastructure
    └── ApplicationBuilderExtensions.cs  # Middleware pipeline setup
```

**Key Integrations with Existing Infrastructure:**
- **ErpLoggingService**: Integrate structured ERP logging throughout controllers and middleware
- **IPerformanceMonitor**: Track API performance metrics and real-time monitoring
- **SivarErpTelemetry**: OpenTelemetry integration for distributed tracing
- **MemoryOptimizer**: Optimize API memory usage for high-performance scenarios

### 1.3 Domain & Infrastructure (Server-Side) - Enhanced with Monitoring

**Files to Create in `Sivar.Erp.Core.Domain`:**
```
Entities/
├── BaseEntity.cs                       # Enhanced with telemetry tracking
├── ITenantEntity.cs
├── IAuditableEntity.cs                 # Integration with ErpLoggingService
├── IEntity.cs
└── Identity/
    ├── User.cs                         # Keycloak user reference
    ├── Company.cs                      # Tenant company
    ├── Branch.cs                       # Company branch
    ├── UserCompany.cs                  # User-company association
    ├── UserInvitation.cs               # User invitation system
    ├── Role.cs                         # Company/branch roles
    └── Permission.cs                   # Granular permissions

ValueObjects/
├── Money.cs
├── CompanyId.cs
├── BranchId.cs
├── UserId.cs                           # Keycloak user ID
└── InvitationToken.cs

Interfaces/
├── IRepository.cs                      # Enhanced with performance tracking
├── IUnitOfWork.cs                      # Integration with ErpLoggingService
├── ICompanyContext.cs
├── IBranchContext.cs
└── IUserContext.cs
```

**Files to Create in `Sivar.Erp.Core.Infrastructure`:**
```
Data/
├── ErpDbContext.cs                     # Enhanced with telemetry and performance monitoring
├── CompanyDbContext.cs                 # Company-specific DbContext with logging
├── Configurations/
│   ├── Identity/
│   │   ├── UserConfiguration.cs
│   │   ├── CompanyConfiguration.cs
│   │   ├── BranchConfiguration.cs
│   │   ├── UserCompanyConfiguration.cs
│   │   └── UserInvitationConfiguration.cs
│   └── ...
└── Migrations/

Repositories/
├── GenericRepository.cs                # Enhanced with IPerformanceMonitor integration
├── UnitOfWork.cs                       # Integration with ErpLoggingService and telemetry
└── Identity/
    ├── CompanyRepository.cs
    ├── BranchRepository.cs
    ├── UserCompanyRepository.cs
    └── UserInvitationRepository.cs

MultiTenancy/
├── ICompanyResolver.cs
├── CompanyResolver.cs                  # Enhanced with performance tracking
├── IBranchResolver.cs
├── BranchResolver.cs
├── CompanyService.cs                   # Integration with existing logging infrastructure
└── BranchService.cs

ExternalServices/
├── Keycloak/
│   ├── IKeycloakService.cs
│   ├── KeycloakService.cs              # Enhanced with telemetry and error tracking
│   ├── KeycloakUserService.cs
│   └── KeycloakAdminService.cs
└── Notifications/
    ├── IEmailService.cs
    ├── EmailService.cs                 # Integration with ErpLoggingService
    └── InvitationEmailService.cs

Performance/                            # ✅ EXISTING - Extend existing implementation
├── IPerformanceMonitor.cs              # ✅ EXISTS - Already implemented
├── InMemoryPerformanceMonitor.cs       # ✅ EXISTS - Already implemented
├── MemoryOptimizer.cs                  # ✅ EXISTS - Already implemented
├── PerformanceModels.cs                # ✅ EXISTS - Already implemented
├── RepositoryPerformanceDecorator.cs   # NEW - Decorator for repository performance tracking
└── EfCorePerformanceInterceptor.cs     # NEW - EF Core interceptor for query performance

Logging/                                # ✅ EXISTING - Extend existing implementation
├── ErpLoggingService.cs                # ✅ EXISTS - Already implemented with comprehensive ERP logging
├── SerilogConfiguration.cs             # ✅ EXISTS - Already configured with multiple sinks
├── AuditLoggingDecorator.cs           # NEW - Decorator for audit logging
└── EntityChangeTracker.cs             # NEW - Track entity changes for audit

Telemetry/                              # ✅ EXISTING - Extend existing implementation
├── OpenTelemetryConfiguration.cs       # ✅ EXISTS - Already configured with Jaeger/OTLP
├── SivarErpTelemetry.cs               # ✅ EXISTS - Already implemented with metrics
├── ApiTelemetryExtensions.cs          # NEW - API-specific telemetry extensions
└── DatabaseTelemetryExtensions.cs     # NEW - Database operation telemetry
```

**Key Enhancements:**
- **Repository Performance Tracking**: Decorate repositories with existing IPerformanceMonitor
- **EF Core Query Monitoring**: Interceptor using existing telemetry infrastructure
- **Audit Logging**: Leverage existing ErpLoggingService for comprehensive audit trails
- **Memory Optimization**: Use existing MemoryOptimizer for DbContext and repository performance

### 1.4 Client-Side Infrastructure

**Files to Create in `Sivar.Erp.Core.Client.Services`:**
```
Abstractions/
├── IAccountingApiService.cs
├── IInventoryApiService.cs
├── IPaymentApiService.cs
├── ITaxApiService.cs
├── ICompanyApiService.cs               # Company management
├── IBranchApiService.cs                # Branch management
└── IUserInvitationApiService.cs        # User invitation API

Http/
├── ApiClientBase.cs
├── AccountingApiService.cs
├── InventoryApiService.cs
├── PaymentApiService.cs
├── TaxApiService.cs
├── CompanyApiService.cs
├── BranchApiService.cs
└── UserInvitationApiService.cs

SignalR/
├── ISignalRService.cs
├── SignalRService.cs
├── HubConnectionManager.cs
├── CompanyHubService.cs
└── UserActivityHubService.cs

Authentication/
├── IKeycloakAuthService.cs
├── KeycloakAuthService.cs
├── ITokenService.cs
├── TokenService.cs
├── ICompanyContextService.cs
├── CompanyContextService.cs
└── UserContextService.cs
```

**Files to Create in `Sivar.Erp.Core.Shared`:**
```
DTOs/
├── Accounting/
├── Inventory/
├── Payments/
├── Taxes/
└── Identity/
    ├── UserDto.cs
    ├── CompanyDto.cs
    ├── BranchDto.cs
    ├── UserCompanyDto.cs
    ├── UserInvitationDto.cs
    ├── RoleDto.cs
    └── PermissionDto.cs

Contracts/
├── API/
│   ├── IAccountingApi.cs
│   ├── IInventoryApi.cs
│   ├── ICompanyApi.cs
│   ├── IBranchApi.cs
│   └── IUserInvitationApi.cs
└── SignalR/
    ├── IAccountingHub.cs
    ├── IInventoryHub.cs
    ├── ICompanyHub.cs
    └── IUserActivityHub.cs

Models/
├── ApiResponse.cs
├── PaginatedResult.cs
├── CompanyInfo.cs
├── BranchInfo.cs
├── UserInfo.cs
├── AuthenticationResult.cs
└── InvitationRequest.cs

Enums/
├── CompanyRole.cs                      # Owner, Admin, User, etc.
├── BranchRole.cs                       # Manager, Employee, etc.
├── InvitationStatus.cs                 # Pending, Accepted, Expired
└── PermissionType.cs                   # Read, Write, Delete, etc.
```

### 1.5 Localization Infrastructure

**Files to Create in `Sivar.Erp.Core.Localization`:**
```
Resources/
├── SharedResources.en.resx             # English shared resources
├── SharedResources.es.resx             # Spanish shared resources
├── Accounting/
│   ├── AccountingResources.en.resx     # English accounting terms
│   └── AccountingResources.es.resx     # Spanish accounting terms
├── Inventory/
│   ├── InventoryResources.en.resx      # English inventory terms
│   └── InventoryResources.es.resx      # Spanish inventory terms
├── Payments/
│   ├── PaymentResources.en.resx        # English payment terms
│   └── PaymentResources.es.resx        # Spanish payment terms
└── ValidationMessages/
    ├── ValidationResources.en.resx     # English validation messages
    └── ValidationResources.es.resx     # Spanish validation messages

Services/
├── ILocalizationService.cs
├── LocalizationService.cs
├── IResourceProvider.cs
├── ResourceProvider.cs
└── CultureService.cs

Extensions/
├── LocalizationExtensions.cs
└── ComponentLocalizationExtensions.cs
```

**Key Features:**
- Resource-based localization for English and Spanish
- Module-specific resource files
- Component localization helpers
- Culture switching support
- Validation message localization

### 1.6 Testing Infrastructure Setup

**Files to Create in Testing Projects:**

#### Integration Testing Base (`Sivar.Erp.Core.Tests`)
```
Infrastructure/
├── TestDbContextFactory.cs
├── IntegrationTestBase.cs
├── CompanyTestContext.cs               # Multi-company test setup
├── KeycloakTestService.cs              # Mock Keycloak for testing
└── LocalizationTestBase.cs             # Localization test helpers

Fixtures/
├── DatabaseFixture.cs
├── CompanyFixture.cs                   # Company setup for tests
├── UserFixture.cs                      # User setup for tests
└── LocalizationFixture.cs              # Culture setup for tests

Utilities/
├── TestDataBuilder.cs
├── AssertionExtensions.cs
├── CultureTestHelper.cs
└── BusinessScenarioRunner.cs           # Business scenario test runner
```

#### Component Testing (`Sivar.Erp.Components.Tests`)
```
Infrastructure/
├── ComponentTestBase.cs                # bUnit test base
├── LocalizedComponentTestBase.cs       # Localized component testing
├── MockApiServices.cs                  # Mock API services for components
└── TestServiceProvider.cs

BusinessScenarios/
├── AccountingWorkflowComponentTests.cs # Full accounting workflow UI tests
├── InventoryWorkflowComponentTests.cs  # Inventory management UI tests
├── PaymentWorkflowComponentTests.cs    # Payment processing UI tests
└── UserInvitationComponentTests.cs     # User invitation UI tests

Localization/
├── ComponentLocalizationTests.cs       # Test all components in both languages
├── ValidationMessageTests.cs           # Test localized validation messages
└── CultureSwitchingTests.cs           # Test culture switching functionality
```

#### E2E Testing (`Sivar.Erp.E2E.Tests`)
```
Infrastructure/
├── PlaywrightTestBase.cs              # Playwright setup
├── E2ETestConfiguration.cs            # Test environment configuration
├── BrowserContextFactory.cs           # Browser context management
└── LocalizationE2EHelper.cs          # E2E localization testing

BusinessScenarios/
├── CompleteAccountingWorkflowE2E.cs   # End-to-end accounting workflow
├── CompanyManagementE2E.cs            # Company creation and management
├── UserInvitationWorkflowE2E.cs       # Complete user invitation flow
├── InventoryManagementE2E.cs          # Inventory operations workflow
└── MultiLanguageWorkflowE2E.cs        # Test workflows in both languages

PageObjects/
├── LoginPage.cs
├── CompanySelectorPage.cs
├── AccountingDashboardPage.cs
├── InventoryPage.cs
├── PaymentPage.cs
└── UserManagementPage.cs
```

#### Business Scenario Testing (`Sivar.Erp.BusinessScenarios.Tests`)
```
Scenarios/
├── PurchaseToPaymentScenario.cs       # Complete purchase-to-payment workflow
├── SaleToCollectionScenario.cs        # Complete sales-to-collection workflow
├── InventoryReceiptToSaleScenario.cs  # Inventory receipt through sale
├── CompanySetupScenario.cs            # New company setup workflow
├── UserOnboardingScenario.cs          # User invitation and onboarding
└── MultiCompanyOperationsScenario.cs  # Cross-company operations

Localization/
├── SpanishBusinessScenarios.cs        # All scenarios in Spanish
├── EnglishBusinessScenarios.cs        # All scenarios in English
└── LanguageSwitchingScenarios.cs      # Language switching during workflows
```

### 1.7 Keycloak Authentication Infrastructure

**Files to Create in `Sivar.Erp.Core.Auth`:**
```
Keycloak/
├── IKeycloakClientService.cs
├── KeycloakClientService.cs
├── KeycloakConfiguration.cs
├── KeycloakTokenValidator.cs
└── KeycloakUserMapper.cs

Providers/
├── IAuthProviderService.cs
├── AuthProviderService.cs
├── EmailAuthProvider.cs
├── GoogleAuthProvider.cs
├── FacebookAuthProvider.cs
└── MicrosoftAuthProvider.cs

Services/
├── IUserManagementService.cs
├── UserManagementService.cs
├── ICompanyManagementService.cs
├── CompanyManagementService.cs
├── IInvitationService.cs
├── InvitationService.cs
└── IRoleManagementService.cs
```

**Keycloak Configuration Files:**
```
Infrastructure/Keycloak/
├── realm-config.json                   # Sivar ERP realm configuration
├── docker-compose.yml                  # Keycloak deployment
├── client-config.json                  # API client configuration
├── providers/
│   ├── google-provider.json
│   ├── facebook-provider.json
│   ├── microsoft-provider.json
│   └── email-provider.json
└── themes/
    └── sivar-erp/                      # Custom Keycloak theme
```

**Key Features:**
- Keycloak realm specifically for Sivar ERP
- Multiple identity providers (Email, Google, Facebook, Microsoft)
- Custom user registration flow with company creation
- Role-based access control with company/branch context
- User invitation system with email verification

### 1.8 Multi-Tenancy Infrastructure (Enhanced for User-Company-Branch Model)

**Files to Create in `Sivar.Erp.Core.Infrastructure/MultiTenancy`:**
- `CompanyContext.cs` - Current company information
- `BranchContext.cs` - Current branch information  
- `UserContext.cs` - Current user information
- `CompanyDbContextFactory.cs` - Creates company-specific DbContext
- `MultiTenantConfiguration.cs` - Company and branch settings

**Hierarchical Tenant Model:**
```
Keycloak User (Global Identity)
└── UserCompany (User can own/be invited to multiple companies)
    ├── Company A (Tenant)
    │   ├── Branch 1
    │   ├── Branch 2
    │   └── Invited Users (with specific roles)
    └── Company B (Tenant)
        ├── Branch 1
        └── Invited Users (with specific roles)
```

**Key Features:**
- User can create multiple companies (become owner)
- User can be invited to multiple companies (with different roles)
- Each company has multiple branches
- Data isolation per company (tenant)
- Role-based permissions per company/branch
- Cross-company user invitations

### 1.9 API & SignalR Configuration (Server-Side) - Enhanced with Existing Infrastructure

**Files to Create in `Sivar.Erp.Core.Api`:**
- `Program.cs` - API startup with Keycloak integration and existing telemetry
- `appsettings.json` - Keycloak, multi-company, and monitoring configurations
- `Dockerfile` - Container configuration for cloud deployment

**Enhanced Keycloak and Monitoring Integration:**
```json
{
  "Keycloak": {
    "Authority": "https://keycloak.sivarerp.com/realms/sivar-erp",
    "ClientId": "sivar-erp-api",
    "ClientSecret": "...",
    "RequireHttps": true
  },
  "Localization": {
    "DefaultCulture": "en-US",
    "SupportedCultures": ["en-US", "es-ES"],
    "ResourcePath": "Resources"
  },
  "Database": {
    "DefaultConnection": "Host=localhost;Database=sivar_erp_main;Username=...",
    "CompanyConnectionTemplate": "Host=localhost;Database=sivar_erp_company_{0};Username=..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Sivar.Erp.Core": "Debug"
    },
    "FilePath": "logs/sivar-erp-.log",
    "Seq": {
      "ServerUrl": "http://localhost:5341",
      "ApiKey": "your-seq-api-key"
    }
  },
  "Telemetry": {
    "ServiceName": "Sivar.Erp.Core.Api",
    "ServiceVersion": "1.0.0",
    "JaegerEndpoint": "http://localhost:14268",
    "OtlpEndpoint": "http://localhost:4317"
  },
  "Performance": {
    "EnableDetailedTracking": true,
    "RetentionPeriod": "7.00:00:00",
    "MemoryOptimization": true
  }
}
```

**Program.cs Setup with Existing Infrastructure:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add existing telemetry and logging infrastructure
builder.Services.AddSivarErpObservability(options =>
{
    options.ServiceName = "Sivar.Erp.Core.Api";
    options.ServiceVersion = "1.0.0";
    options.JaegerEndpoint = builder.Configuration["Telemetry:JaegerEndpoint"];
    options.OtlpEndpoint = builder.Configuration["Telemetry:OtlpEndpoint"];
});

// Add existing logging service
builder.Services.AddErpLogging();

// Add existing performance monitoring
builder.Services.AddScoped<IPerformanceMonitor, InMemoryPerformanceMonitor>();

// Configure for performance optimization
if (builder.Configuration.GetValue<bool>("Performance:MemoryOptimization"))
{
    builder.Services.ConfigureForHighPerformance();
}

// Add other services...
builder.Services.AddControllers();
builder.Services.AddSignalR();

var app = builder.Build();

// Use existing telemetry middleware
app.UseMiddleware<TelemetryMiddleware>();
app.UseMiddleware<PerformanceTrackingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.Run();
```

**Database Support with Performance Monitoring:**
- PostgreSQL for production (cloud) with query performance tracking
- InMemory provider for testing with memory optimization
- Separate databases per company (tenant isolation) with connection monitoring
- Connection pooling and optimization per company using existing MemoryOptimizer

### 1.10 Client Applications Setup (Enhanced for Multi-Company + Localization)

**Blazor WebAssembly (`Sivar.Erp.Web.Wasm`):**
```
Program.cs                               # WASM startup with Keycloak + Localization
wwwroot/appsettings.json                # Keycloak client + localization configuration
Resources/                              # Client-side localization resources
├── Pages.en.json                       # English page resources
└── Pages.es.json                       # Spanish page resources
Services/ClientServiceExtensions.cs     # DI setup with auth + localization
Components/
├── CompanySelector.razor               # Company/branch selection
├── LanguageSelector.razor              # Language selection component
├── UserProfile.razor                   # User profile management
└── InviteUser.razor                    # User invitation component
```

**Blazor Server (`Sivar.Erp.Web.Server`):**
```
Program.cs                               # Server startup with Keycloak + Localization
appsettings.json                        # Keycloak + localization configuration
Services/ServerServiceExtensions.cs     # DI setup with auth + localization
Resources/                              # Server-side localization resources
├── Pages/                              # Page-specific resources
│   ├── Account.en.resx
│   ├── Account.es.resx
│   ├── Dashboard.en.resx
│   └── Dashboard.es.resx
Areas/Identity/                         # Custom identity pages
├── Pages/Account/
│   ├── Login.cshtml
│   ├── Register.cshtml
│   └── CompanySelection.cshtml
```

**MAUI Hybrid (`Sivar.Erp.Mobile`):**
```
MauiProgram.cs                          # MAUI startup with Keycloak + Localization
Platforms/                              # Platform-specific auth + localization configs
├── Android/
│   ├── AndroidManifest.xml            # Android auth configuration
│   └── Resources/values-es/            # Spanish Android resources
├── iOS/
│   ├── Info.plist                     # iOS auth configuration
│   └── es.lproj/                       # Spanish iOS resources
└── Windows/Package.appxmanifest       # Windows auth configuration
Resources/                              # Mobile localization resources
├── AppResources.en.resx
├── AppResources.es.resx
├── Raw/                                # Raw localization files
│   ├── en.json
│   └── es.json
Services/MobileServiceExtensions.cs     # Mobile-specific DI with auth + localization
```

## Phase 2: Monitoring Infrastructure Integration (Weeks 3-4)

### 2.1 Leverage Existing Monitoring Infrastructure

**Existing Infrastructure Analysis:**
The `Sivar.Erp.Core` project already contains comprehensive monitoring infrastructure that needs to be integrated throughout the migration:

#### ✅ **Existing Logging Infrastructure**
- **ErpLoggingService**: Comprehensive ERP-specific logging with structured data
- **SerilogConfiguration**: Multi-sink logging (Console, File, Debug, Seq, Error-specific)
- **Logging Extensions**: Business process, audit, and performance metric logging

#### ✅ **Existing Performance Monitoring**
- **IPerformanceMonitor**: Complete performance tracking interface
- **InMemoryPerformanceMonitor**: Real-time performance metrics with operation statistics
- **MemoryOptimizer**: Memory usage optimization and garbage collection management
- **PerformanceModels**: Time ranges, metrics, and system health models

#### ✅ **Existing Telemetry Infrastructure**
- **SivarErpTelemetry**: OpenTelemetry integration with counters, histograms, and gauges
- **OpenTelemetryConfiguration**: Jaeger and OTLP exporters with service configuration
- **Activity Source**: Distributed tracing for operations

### 2.2 Integration Strategy for New Components

**Files to Create for Enhanced Integration:**

#### Performance Monitoring Enhancements
```
Sivar.Erp.Core.Infrastructure/Performance/
├── RepositoryPerformanceDecorator.cs   # NEW - Repository operation tracking
├── EfCorePerformanceInterceptor.cs     # NEW - EF Core query performance
├── ApiPerformanceTracker.cs            # NEW - API endpoint performance
├── SignalRPerformanceTracker.cs        # NEW - Real-time hub performance
└── MultiTenantPerformanceContext.cs    # NEW - Company/branch-specific metrics

Sivar.Erp.Core.Api/Middleware/
├── PerformanceTrackingMiddleware.cs     # NEW - HTTP request performance
├── CompanyPerformanceMiddleware.cs      # NEW - Company-specific metrics
└── TelemetryEnrichmentMiddleware.cs     # NEW - Add company/user context to telemetry
```

#### Logging Enhancements
```
Sivar.Erp.Core.Infrastructure/Logging/
├── AuditLoggingDecorator.cs            # NEW - Comprehensive audit trail
├── EntityChangeTracker.cs              # NEW - Entity change tracking
├── MultiTenantLoggingContext.cs        # NEW - Company/branch logging context
└── SecurityAuditLogger.cs              # NEW - Security event logging

Sivar.Erp.Core.Api/Logging/
├── ApiAuditLogger.cs                   # NEW - API-specific audit logging
├── AuthenticationLogger.cs             # NEW - Keycloak authentication logging
└── CompanyActivityLogger.cs            # NEW - Company-specific activity logging
```

#### Telemetry Enhancements
```
Sivar.Erp.Core.Infrastructure/Telemetry/
├── DatabaseTelemetryExtensions.cs      # NEW - Database operation telemetry
├── MultiTenantTelemetryEnricher.cs     # NEW - Company/branch telemetry context
├── BusinessMetricsCollector.cs         # NEW - Business-specific metrics
└── PerformanceAlertService.cs          # NEW - Performance threshold alerts

Sivar.Erp.Core.Api/Telemetry/
├── ApiTelemetryExtensions.cs           # NEW - API-specific telemetry
├── SignalRTelemetryHub.cs              # NEW - Real-time telemetry hub
└── UserActivityTelemetry.cs            # NEW - User behavior telemetry
```

### 2.3 Repository and Data Layer Integration

**Enhanced Repository with Existing Monitoring:**

```csharp
// RepositoryPerformanceDecorator.cs
public class RepositoryPerformanceDecorator<T> : IRepository<T> where T : class
{
    private readonly IRepository<T> _repository;
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly IErpLoggingService _loggingService;

    public async Task<T> GetByIdAsync(Guid id)
    {
        using var tracker = _performanceMonitor.StartTracking($"Repository.{typeof(T).Name}.GetById");
        using var activity = SivarErpTelemetry.StartActivity($"Repository.GetById", new Dictionary<string, object?>
        {
            ["entity.type"] = typeof(T).Name,
            ["entity.id"] = id.ToString()
        });

        try
        {
            var result = await _repository.GetByIdAsync(id);
            
            _loggingService.LogBusinessEntityOperation("Get", id.ToString(), typeof(T).Name, 
                GetCurrentUserId(), new { Found = result != null });
            
            SivarErpTelemetry.RecordRepositoryOperation("Get", typeof(T).Name, 
                tracker.Elapsed.TotalSeconds, result != null ? 1 : 0);
            
            return result;
        }
        catch (Exception ex)
        {
            _loggingService.LogValidationError(typeof(T).Name, id.ToString(), 
                new List<string> { ex.Message });
            
            SivarErpTelemetry.RecordError("Repository.Error", $"Repository.{typeof(T).Name}");
            throw;
        }
    }
}
```

**EF Core Performance Interceptor:**

```csharp
// EfCorePerformanceInterceptor.cs
public class EfCorePerformanceInterceptor : DbCommandInterceptor
{
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly IErpLoggingService _loggingService;

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result, 
        CancellationToken cancellationToken = default)
    {
        var duration = eventData.Duration;
        var commandText = command.CommandText;
        
        // Record performance metrics using existing infrastructure
        _performanceMonitor.RecordOperation($"Database.Query", duration, true);
        
        // Log using existing ERP logging service
        _loggingService.LogPerformanceMetrics("Database.Query", duration, 
            result.RecordsAffected);
        
        // Record telemetry using existing infrastructure
        SivarErpTelemetry.RecordRepositoryOperation("Query", "Database", 
            duration.TotalSeconds, result.RecordsAffected);
        
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }
}
```

### 2.4 API Layer Integration

**Performance Tracking Middleware:**

```csharp
// PerformanceTrackingMiddleware.cs
public class PerformanceTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly IErpLoggingService _loggingService;

    public async Task InvokeAsync(HttpContext context)
    {
        var operationName = $"API.{context.Request.Method}.{context.Request.Path}";
        
        using var tracker = _performanceMonitor.StartTracking(operationName);
        using var activity = SivarErpTelemetry.StartActivity(operationName, new Dictionary<string, object?>
        {
            ["http.method"] = context.Request.Method,
            ["http.url"] = context.Request.Path,
            ["user.id"] = context.User?.FindFirst("sub")?.Value
        });

        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
            
            stopwatch.Stop();
            var success = context.Response.StatusCode < 400;
            
            // Use existing infrastructure for tracking
            _loggingService.LogPerformanceMetrics(operationName, stopwatch.Elapsed);
            SivarErpTelemetry.RecordOperation(operationName, stopwatch.Elapsed.TotalSeconds, success);
            
            activity?.SetTag("http.status_code", context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _loggingService.LogValidationError("API", operationName, new List<string> { ex.Message });
            SivarErpTelemetry.RecordError("API.Error", operationName);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

### 2.5 SignalR Integration with Monitoring

**Performance Hub for Real-time Metrics:**

```csharp
// PerformanceHub.cs - Real-time performance monitoring
public class PerformanceHub : Hub
{
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly IErpLoggingService _loggingService;

    public async Task SubscribeToPerformanceMetrics(string companyId)
    {
        using var activity = SivarErpTelemetry.StartActivity("SignalR.PerformanceSubscription");
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Performance_{companyId}");
        
        // Get real-time metrics using existing infrastructure
        var metrics = await _performanceMonitor.GetRealTimeMetricsAsync();
        await Clients.Caller.SendAsync("PerformanceMetrics", metrics);
        
        _loggingService.LogSecurityEvent("PerformanceSubscription", 
            Context.User?.FindFirst("sub")?.Value ?? "Anonymous", 
            $"Company_{companyId}", true);
    }

    public async Task<object> GetSystemHealth()
    {
        using var tracker = _performanceMonitor.StartTracking("SignalR.GetSystemHealth");
        
        var health = await _performanceMonitor.GetSystemHealthAsync();
        return health;
    }
}
```

### 2.6 Enhanced Configuration

**Program.cs Integration:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure existing telemetry infrastructure
builder.Services.AddSivarErpObservability(options =>
{
    options.ServiceName = "Sivar.Erp.Core.Api";
    options.JaegerEndpoint = builder.Configuration["Telemetry:JaegerEndpoint"];
    options.OtlpEndpoint = builder.Configuration["Telemetry:OtlpEndpoint"];
});

// Add existing logging infrastructure
builder.Services.AddErpLogging();

// Add existing performance monitoring
builder.Services.AddScoped<IPerformanceMonitor, InMemoryPerformanceMonitor>();

// Add performance decorators for repositories
builder.Services.Decorate<IRepository<Customer>, RepositoryPerformanceDecorator<Customer>>();
builder.Services.Decorate<IRepository<Invoice>, RepositoryPerformanceDecorator<Invoice>>();

// Add EF Core performance interceptor
builder.Services.AddDbContext<ErpDbContext>(options =>
{
    options.UseNpgsql(connectionString)
           .AddInterceptors(new EfCorePerformanceInterceptor());
});

// Configure memory optimization
if (builder.Configuration.GetValue<bool>("Performance:MemoryOptimization"))
{
    builder.Services.ConfigureForHighPerformance();
}

var app = builder.Build();

// Add monitoring middleware pipeline
app.UseMiddleware<TelemetryEnrichmentMiddleware>();
app.UseMiddleware<PerformanceTrackingMiddleware>();
app.UseMiddleware<CompanyPerformanceMiddleware>();

app.MapHub<PerformanceHub>("/hubs/performance");
```

This integration approach leverages all the existing sophisticated monitoring infrastructure while extending it for the new client-server architecture and multi-tenant requirements.

## Phase 3: User Management and Company Structure (Weeks 5-6)

### 2.1 User and Company Domain Entities

**Files to Create in `Sivar.Erp.Core.Domain/Entities/Identity/`:**

```csharp
// User.cs - Reference to Keycloak user
public class User : BaseEntity
{
    public string KeycloakUserId { get; set; }        // Keycloak user ID
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PreferredLanguage { get; set; }
    public string TimeZone { get; set; }
    public DateTime LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<UserCompany> UserCompanies { get; set; }
    public ICollection<UserInvitation> SentInvitations { get; set; }
    public ICollection<UserInvitation> ReceivedInvitations { get; set; }
}

// Company.cs - Tenant company
public class Company : BaseEntity, IAuditableEntity
{
    public string Name { get; set; }
    public string TaxId { get; set; }
    public string Address { get; set; }
    public string Country { get; set; }
    public string Currency { get; set; }
    public string TimeZone { get; set; }
    public string LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public string OwnerId { get; set; }               // Keycloak user ID of owner
    
    // Navigation properties
    public User Owner { get; set; }
    public ICollection<Branch> Branches { get; set; }
    public ICollection<UserCompany> UserCompanies { get; set; }
    public ICollection<UserInvitation> Invitations { get; set; }
}

// Branch.cs - Company branch
public class Branch : BaseEntity, IAuditableEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public bool IsHeadquarters { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public Company Company { get; set; }
    public ICollection<UserCompany> UserCompanies { get; set; }
}

// UserCompany.cs - User-company association with roles
public class UserCompany : BaseEntity
{
    public string UserId { get; set; }               // Keycloak user ID
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }              // Null = access to all branches
    public CompanyRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public User User { get; set; }
    public Company Company { get; set; }
    public Branch Branch { get; set; }
    public ICollection<UserPermission> Permissions { get; set; }
}

// UserInvitation.cs - User invitation system
public class UserInvitation : BaseEntity
{
    public string InviterUserId { get; set; }        // Who sent the invitation
    public string InviteeEmail { get; set; }         // Email to invite
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public CompanyRole Role { get; set; }
    public string InvitationToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string Message { get; set; }
    
    // Navigation properties
    public User Inviter { get; set; }
    public Company Company { get; set; }
    public Branch Branch { get; set; }
}
```

### 2.2 Company Management Services

**Files to Create in `Sivar.Erp.Core.Application/Services/Identity/`:**

```csharp
// ICompanyService.cs
public interface ICompanyService
{
    Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto request, string ownerId);
    Task<CompanyDto> UpdateCompanyAsync(Guid companyId, UpdateCompanyDto request, string userId);
    Task<bool> DeleteCompanyAsync(Guid companyId, string userId);
    Task<IEnumerable<CompanyDto>> GetUserCompaniesAsync(string userId);
    Task<CompanyDto> GetCompanyAsync(Guid companyId, string userId);
    Task<bool> UserHasAccessToCompanyAsync(string userId, Guid companyId);
}

// IUserInvitationService.cs
public interface IUserInvitationService
{
    Task<UserInvitationDto> SendInvitationAsync(SendInvitationDto request, string inviterId);
    Task<UserInvitationDto> AcceptInvitationAsync(string token, string userId);
    Task<bool> RejectInvitationAsync(string token);
    Task<IEnumerable<UserInvitationDto>> GetPendingInvitationsAsync(string email);
    Task<IEnumerable<UserInvitationDto>> GetCompanyInvitationsAsync(Guid companyId, string userId);
}

// IBranchService.cs
public interface IBranchService
{
    Task<BranchDto> CreateBranchAsync(Guid companyId, CreateBranchDto request, string userId);
    Task<BranchDto> UpdateBranchAsync(Guid branchId, UpdateBranchDto request, string userId);
    Task<bool> DeleteBranchAsync(Guid branchId, string userId);
    Task<IEnumerable<BranchDto>> GetCompanyBranchesAsync(Guid companyId, string userId);
}
```

### 2.3 Keycloak Integration

**Files to Create in `Sivar.Erp.Core.Auth/Keycloak/`:**

```csharp
// IKeycloakUserService.cs
public interface IKeycloakUserService
{
    Task<KeycloakUserDto> GetUserAsync(string userId);
    Task<KeycloakUserDto> GetUserByEmailAsync(string email);
    Task<bool> UserExistsAsync(string email);
    Task<string> CreateUserAsync(CreateKeycloakUserDto request);
    Task UpdateUserAsync(string userId, UpdateKeycloakUserDto request);
    Task<bool> SendEmailVerificationAsync(string userId);
    Task<bool> SendPasswordResetAsync(string email);
}

// KeycloakConfiguration.cs
public class KeycloakConfiguration
{
    public string Authority { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string AdminClientId { get; set; }
    public string AdminClientSecret { get; set; }
    public string Realm { get; set; }
    public bool RequireHttps { get; set; }
    public Dictionary<string, ProviderConfiguration> IdentityProviders { get; set; }
}
```

### 2.4 API Controllers for User Management

**Files to Create in `Sivar.Erp.Core.Api/Controllers/`:**

```csharp
// CompaniesController.cs
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CompanyDto>> CreateCompany(CreateCompanyDto request);
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetMyCompanies();
    
    [HttpGet("{companyId}")]
    public async Task<ActionResult<CompanyDto>> GetCompany(Guid companyId);
    
    [HttpPut("{companyId}")]
    public async Task<ActionResult<CompanyDto>> UpdateCompany(Guid companyId, UpdateCompanyDto request);
    
    [HttpDelete("{companyId}")]
    public async Task<ActionResult> DeleteCompany(Guid companyId);
}

// UserInvitationsController.cs
[Route("api/[controller]")]
[Authorize]
public class UserInvitationsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserInvitationDto>> SendInvitation(SendInvitationDto request);
    
    [HttpPost("accept/{token}")]
    public async Task<ActionResult<UserInvitationDto>> AcceptInvitation(string token);
    
    [HttpPost("reject/{token}")]
    public async Task<ActionResult> RejectInvitation(string token);
    
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<UserInvitationDto>>> GetPendingInvitations();
    
    [HttpGet("company/{companyId}")]
    public async Task<ActionResult<IEnumerable<UserInvitationDto>>> GetCompanyInvitations(Guid companyId);
}
```

## Phase 4: Core Domain Migration (Weeks 7-8)

### 4.1 Base Entity Types (Enhanced with Monitoring)

**Files to Migrate/Create:**

#### From `Sivar.Erp` → `Sivar.Erp.Core`
- `BaseEntity.cs` → `Core/Domain/Entities/BaseEntity.cs` (Enhanced with telemetry tracking)
- `IEntity.cs` → `Core/Domain/Entities/IEntity.cs`
- `BusinessKeyAttribute.cs` → `Core/Domain/Attributes/BusinessKeyAttribute.cs`
- `IDateTimeZoneTrackable.cs` → `Core/Domain/Entities/IAuditableEntity.cs` (Enhanced with ErpLoggingService)

#### Enhanced Entity Features with Existing Infrastructure:
- Company isolation properties (CompanyId, BranchId)
- Enhanced auditing with user tracking using ErpLoggingService
- Change tracking with Keycloak user context and SivarErpTelemetry
- Soft delete support with performance monitoring
- Performance tracking for entity operations using IPerformanceMonitor

### 4.2 Document Entities (Enhanced with Comprehensive Monitoring)

**Files to Create in `Core/Domain/Entities/Documents/`:**
```csharp
// Document.cs (from DocumentDto.cs) - Enhanced with monitoring
public class Document : BaseEntity, ITenantEntity, IAuditableEntity
{
    // ... existing properties
    
    // Enhanced tracking methods using existing infrastructure
    public void LogOperation(string operation, IErpLoggingService loggingService, string userId)
    {
        loggingService.LogDocumentOperation(operation, DocumentNumber, DocumentType, userId, 
            new { CompanyId, BranchId, Amount = TotalAmount });
    }
    
    public void TrackPerformance(string operation, TimeSpan duration, IPerformanceMonitor performanceMonitor)
    {
        performanceMonitor.RecordOperation($"Document.{operation}", duration, true);
        SivarErpTelemetry.RecordOperation($"Document.{operation}", duration.TotalSeconds, true, "Document");
    }
}

// DocumentLine.cs (from LineDto.cs) - Enhanced with monitoring
public class DocumentLine : BaseEntity, ITenantEntity
{
    // ... existing properties
    
    public void LogLineOperation(string operation, IErpLoggingService loggingService, string userId)
    {
        loggingService.LogBusinessEntityOperation(operation, Id.ToString(), "DocumentLine", userId,
            new { DocumentId, ItemCode, Quantity, UnitPrice });
    }
}
```
- `DocumentType.cs` (from `DocumentTypeDto.cs`)
- `DocumentAccountingProfile.cs` (from `DocumentAccountingProfileDto.cs`)

**Entity Configurations in `Core/Infrastructure/Data/Configurations/Documents/`:**
- `DocumentConfiguration.cs`
- `DocumentLineConfiguration.cs`
- `DocumentTypeConfiguration.cs`

## Phase 4: Business Entities Module (Weeks 7-8)

### 4.1 Business Entity Migration

**Files to Create:**

#### Domain Layer
```
Core/Domain/Entities/BusinessEntities/
├── BusinessEntity.cs              (from BusinessEntityDto.cs)
├── IBusinessEntity.cs            (interface evolution)
└── BusinessEntityType.cs         (enum/value object)
```

#### Application Layer (Server-Side)
```
Sivar.Erp.Core.Application/
├── Interfaces/BusinessEntities/
│   ├── IBusinessEntityService.cs
│   └── IBusinessEntityQueryService.cs
└── Services/BusinessEntities/
    ├── BusinessEntityService.cs       # Business logic
    └── BusinessEntityQueryService.cs  # Query operations
```

#### API Layer (Server-Side)
```
Sivar.Erp.Core.Api/Controllers/
└── BusinessEntitiesController.cs      # REST endpoints
```

#### Client Services (Client-Side)
```
Sivar.Erp.Core.Client.Services/
├── IBusinessEntityApiService.cs       # Client interface
└── BusinessEntityApiService.cs        # HTTP client implementation
```

#### Shared Contracts
```
Sivar.Erp.Core.Shared/
├── DTOs/BusinessEntities/
│   ├── BusinessEntityDto.cs           # API contract
│   ├── CreateBusinessEntityDto.cs     # Create request
│   └── UpdateBusinessEntityDto.cs     # Update request
└── Contracts/API/
    └── IBusinessEntitiesApi.cs        # API contract definition
```

#### Infrastructure Layer (Server-Side)
```
Sivar.Erp.Core.Infrastructure/
├── Repositories/BusinessEntities/
│   └── BusinessEntityRepository.cs
└── Data/Configurations/
    └── BusinessEntityConfiguration.cs
```

### 4.2 Client-Server Implementation

**Key Architecture Changes:**
- **Server**: Repository pattern with EF Core for data persistence
- **Clients**: HTTP services for API communication
- **Real-time**: SignalR for live updates (stock changes, transaction posts)
- **Offline**: Local storage for critical data in WASM/MAUI clients
- **Sync**: Conflict resolution for offline-to-online synchronization

**Client-Side Data Flow:**
```
Blazor Component → Client Service → HTTP Client → API Controller → Application Service → Repository → Database
```

**Real-time Updates:**
```
Server Event → SignalR Hub → Client Hub Connection → UI Update
```

## Phase 5: Accounting Module Migration (Weeks 9-12)

### 5.1 Chart of Accounts (Server-Side)

**Files to Create:**

#### Domain Entities (`Sivar.Erp.Core.Domain/Entities/Accounting/`)
```
├── Account.cs                     (from IAccount interface)
├── AccountType.cs                 (enum)
├── FiscalPeriod.cs               (from FiscalPeriodDto.cs)
├── Transaction.cs                 (from TransactionDto.cs)
├── LedgerEntry.cs                (from LedgerEntryDto.cs)
└── TransactionBatch.cs           (from ITransactionBatch)
```

#### Repository Layer (`Sivar.Erp.Core.Infrastructure/Repositories/Accounting/`)
```
├── AccountRepository.cs
├── FiscalPeriodRepository.cs
├── TransactionRepository.cs
└── LedgerEntryRepository.cs
```

#### Application Services (`Sivar.Erp.Core.Application/Services/Accounting/`)
```
├── IAccountingService.cs
├── AccountingService.cs
├── IFiscalPeriodService.cs
├── FiscalPeriodService.cs
├── ITransactionService.cs
├── TransactionService.cs
└── BalanceCalculatorService.cs
```

#### API Controllers (`Sivar.Erp.Core.Api/Controllers/`)
```
├── AccountsController.cs          # Chart of accounts API
├── FiscalPeriodsController.cs     # Fiscal periods API
├── TransactionsController.cs      # Transaction management API
└── ReportsController.cs           # Financial reports API
```

#### SignalR Hubs (`Sivar.Erp.Core.Api/Hubs/`)
```
├── AccountingHub.cs               # Real-time transaction posts
└── ReportsHub.cs                  # Live report updates
```

#### Client Services (`Sivar.Erp.Core.Client.Services/`)
```
├── IAccountingApiService.cs
├── AccountingApiService.cs
├── IAccountingSignalRService.cs
└── AccountingSignalRService.cs
```

#### Shared DTOs (`Sivar.Erp.Core.Shared/DTOs/Accounting/`)
```
├── AccountDto.cs
├── FiscalPeriodDto.cs
├── TransactionDto.cs
├── LedgerEntryDto.cs
├── BalanceSheetDto.cs
└── IncomeStatementDto.cs
```

### 5.2 Journal Entry System

**Server-Side Files:**
```
Domain Entities (Sivar.Erp.Core.Domain/Entities/Accounting/JournalEntries/):
├── JournalEntry.cs
├── JournalEntryLine.cs
└── JournalBatch.cs

Application Services (Sivar.Erp.Core.Application/Services/Accounting/):
├── IJournalEntryService.cs
├── JournalEntryService.cs
├── IJournalEntryReportService.cs
└── JournalEntryReportService.cs

API Controllers (Sivar.Erp.Core.Api/Controllers/):
├── JournalEntriesController.cs
└── JournalReportsController.cs
```

**Client-Side Files:**
```
Client Services (Sivar.Erp.Core.Client.Services/):
├── IJournalEntryApiService.cs
└── JournalEntryApiService.cs

Shared DTOs (Sivar.Erp.Core.Shared/DTOs/Accounting/):
├── JournalEntryDto.cs
├── JournalEntryLineDto.cs
└── JournalReportDto.cs
```

### 5.3 Blazor Components (Shared UI + Localized)

**Files to Create in `Sivar.Erp.Core.Components/Accounting/`:**
```
├── ChartOfAccountsGrid.razor           # Account management (localized)
├── TransactionForm.razor               # Transaction entry (localized)
├── JournalEntryForm.razor              # Journal entry creation (localized)
├── BalanceSheetReport.razor            # Balance sheet display (localized)
├── IncomeStatementReport.razor         # P&L display (localized)
├── AccountingDashboard.razor           # Real-time dashboard (localized)
└── LocalizedAccountingBase.cs          # Base class for localized accounting components
```

**Component Features:**
- Real-time updates via SignalR
- Offline data entry with sync
- Multi-company data filtering
- Responsive design for mobile/desktop
- **Full localization support for English and Spanish**
- **Localized validation messages**
- **Culture-aware number and date formatting**
- **Localized business terminology**

**Localization Implementation Example:**
```csharp
@inherits LocalizedComponentBase
@inject ILocalizationService Localization

<div class="accounting-dashboard">
    <h2>@Localization["Dashboard.Title"]</h2>
    <div class="financial-summary">
        <div class="metric">
            <label>@Localization["Accounting.TotalRevenue"]</label>
            <span>@Revenue.ToString("C", CultureInfo.CurrentCulture)</span>
        </div>
        <div class="metric">
            <label>@Localization["Accounting.TotalExpenses"]</label>
            <span>@Expenses.ToString("C", CultureInfo.CurrentCulture)</span>
        </div>
    </div>
</div>

@code {
    [Parameter] public decimal Revenue { get; set; }
    [Parameter] public decimal Expenses { get; set; }
}
```

**Shared Localization Components:**
```
Sivar.Erp.Core.Components/Shared/
├── LanguageSelector.razor              # Language switching component
├── LocalizedValidationSummary.razor    # Localized validation display
├── LocalizedDatePicker.razor           # Culture-aware date picker
├── LocalizedNumberInput.razor          # Culture-aware number input
├── LocalizedCurrencyInput.razor        # Currency input with localization
└── LocalizedDataGrid.razor             # Data grid with localized headers
```

## Phase 6: Inventory Module Migration (Weeks 13-15)

### 6.1 Inventory Entities

**Files to Create:**

#### Domain Layer
```
Core/Domain/Entities/Inventory/
├── InventoryItem.cs              (from InventoryItemDto.cs)
├── StockLevel.cs                 (from IStockLevel)
├── InventoryTransaction.cs       (from InventoryTransactionDto.cs)
├── InventoryReservation.cs       (from IInventoryReservation)
├── InventoryLayer.cs             (from InventoryLayerDto.cs)
├── Warehouse.cs                  (new entity)
└── InventoryMovement.cs          (new entity)
```

#### Value Objects
```
Core/Domain/ValueObjects/Inventory/
├── StockQuantity.cs
├── UnitCost.cs
└── InventoryLocation.cs
```

### 6.2 Inventory Services

**Files to Create:**
```
Core/Application/Services/Inventory/
├── IInventoryService.cs
├── InventoryService.cs
├── IKardexService.cs
├── KardexService.cs
├── IInventoryReservationService.cs
└── InventoryReservationService.cs
```

### 6.3 Advanced Inventory Features

**Files to Create:**
```
Core/Application/Services/Inventory/Advanced/
├── IInventoryAnalyticsService.cs
├── InventoryAnalyticsService.cs
├── IInventoryTransferService.cs
├── InventoryTransferService.cs
├── ICycleCountingService.cs
└── CycleCountingService.cs
```

## Phase 7: Tax Module Migration (Weeks 16-17)

### 7.1 Tax Entities

**Files to Create:**
```
Core/Domain/Entities/Taxes/
├── Tax.cs                        (from ITax)
├── TaxGroup.cs                   (from ITaxGroup)
├── TaxRule.cs                    (from ITaxRule)
├── TaxAccountingProfile.cs       (from TaxAccountingProfile)
└── GroupMembership.cs            (from GroupMembershipDto)
```

### 7.2 Tax Services

**Files to Create:**
```
Core/Application/Services/Taxes/
├── ITaxService.cs
├── TaxService.cs
├── ITaxCalculatorService.cs
├── TaxCalculatorService.cs
├── ITaxAccountingProfileService.cs
└── TaxAccountingProfileService.cs
```

## Phase 8: Payment Module Migration (Weeks 18-19)

### 8.1 Payment Entities

**Files to Create:**
```
Core/Domain/Entities/Payments/
├── Payment.cs                    (from PaymentDto)
├── PaymentMethod.cs              (from PaymentMethodDto)
├── PaymentLine.cs                (new)
└── PaymentStatus.cs              (enum)
```

### 8.2 Payment Services

**Files to Create:**
```
Core/Application/Services/Payments/
├── IPaymentService.cs
├── PaymentService.cs
├── IPaymentMethodService.cs
└── PaymentMethodService.cs
```

## Phase 9: Security Module Migration (Weeks 20-21)

### 9.1 Security Entities

**Files to Create:**
```
Core/Domain/Entities/Security/
├── User.cs                       (from User)
├── Role.cs                       (from Role)
├── Permission.cs                 (new)
├── SecurityEvent.cs              (from SecurityEvent)
└── UserSession.cs                (new)
```

### 9.2 Security Services

**Files to Create:**
```
Core/Application/Services/Security/
├── ISecurityService.cs
├── SecurityService.cs
├── IUserService.cs
├── UserService.cs
├── IRoleService.cs
└── RoleService.cs
```

## Phase 10: Data Import/Export Migration (Weeks 22-23)

### 10.1 Import/Export Services

**Files to Create:**
```
Core/Application/Services/ImportExport/
├── IDataImportService.cs
├── DataImportService.cs
├── ICsvImportService.cs
├── CsvImportService.cs
├── IDataExportService.cs
└── DataExportService.cs
```

### 10.2 Migration Utilities

**Files to Create:**
```
Core/Infrastructure/Migration/
├── IDataMigrationService.cs
├── DataMigrationService.cs
├── ObjectDbToEfCoreMapper.cs
└── MigrationValidationService.cs
```

## Phase 11: Module Integration (Weeks 24-25)

### 11.1 Module Factory

**Files to Create:**
```
Core/Modules/
├── IModuleFactory.cs
├── ModuleFactory.cs
├── IErpModuleRegistry.cs
└── ErpModuleRegistry.cs
```

### 11.2 Dependency Injection Configuration

**Files to Create:**
```
Core/Configuration/
├── ServiceCollectionExtensions.cs
├── DbContextExtensions.cs
├── RepositoryExtensions.cs
└── ModuleExtensions.cs
```

## Phase 12: Testing Infrastructure (Weeks 26-28)

### 12.1 Testing Framework Setup

**Testing Technologies:**
- **NUnit**: Primary testing framework for all test types
- **bUnit**: Blazor component testing
- **Microsoft Playwright**: End-to-end browser testing
- **TestServer**: API integration testing
- **FluentAssertions**: Enhanced assertion library

**Package References for Test Projects:**
```xml
<PackageReference Include="NUnit" Version="3.14.0" />
<PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.7.2" />
<PackageReference Include="bUnit" Version="1.24.10" />
<PackageReference Include="Microsoft.Playwright.NUnit" Version="1.40.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="Microsoft.Extensions.Localization.Testing" Version="8.0.0" />
```

### 12.2 Business Scenario Integration Tests

**Files to Create in `Sivar.Erp.BusinessScenarios.Tests/`:**

```csharp
[TestFixture]
public class CompleteAccountingWorkflowIntegrationTests : BusinessScenarioTestBase
{
    [Test]
    [TestCase("en-US", "English")]
    [TestCase("es-ES", "Spanish")]
    public async Task ExecuteCompleteAccountingWorkflow_InLanguage_ShouldSucceed(string culture, string languageName)
    {
        // Arrange: Set culture for test
        SetTestCulture(culture);
        
        // Create company and user in test context
        var company = await CreateTestCompanyAsync();
        var user = await CreateTestUserAsync(company.Id);
        
        // Act & Assert: Execute complete workflow
        await ExecutePurchaseInvoiceWorkflow(company, user);
        await ExecuteSalesInvoiceWorkflow(company, user);
        await ExecutePaymentWorkflow(company, user);
        await ValidateAccountingBalances(company);
        
        // Verify localized messages and formatting
        await VerifyLocalizedOutput(culture);
    }
    
    [Test]
    public async Task MultiCompanyWorkflow_WithUserInvitations_ShouldMaintainDataIsolation()
    {
        // Arrange: Create two companies
        var companyA = await CreateTestCompanyAsync("Company A");
        var companyB = await CreateTestCompanyAsync("Company B");
        
        // Create owner for Company A
        var ownerA = await CreateTestUserAsync(companyA.Id, CompanyRole.Owner);
        
        // Create owner for Company B
        var ownerB = await CreateTestUserAsync(companyB.Id, CompanyRole.Owner);
        
        // Invite ownerA to Company B as employee
        await InviteUserToCompany(companyB.Id, ownerA.Email, CompanyRole.Employee);
        await AcceptInvitation(ownerA.KeycloakUserId);
        
        // Act: Perform operations in both companies
        var transactionA = await CreateTransactionInCompany(companyA.Id, ownerA.KeycloakUserId);
        var transactionB = await CreateTransactionInCompany(companyB.Id, ownerA.KeycloakUserId);
        
        // Assert: Verify data isolation
        var companyATransactions = await GetCompanyTransactions(companyA.Id, ownerA.KeycloakUserId);
        var companyBTransactions = await GetCompanyTransactions(companyB.Id, ownerA.KeycloakUserId);
        
        companyATransactions.Should().Contain(t => t.Id == transactionA.Id);
        companyATransactions.Should().NotContain(t => t.Id == transactionB.Id);
        
        companyBTransactions.Should().Contain(t => t.Id == transactionB.Id);
        companyBTransactions.Should().NotContain(t => t.Id == transactionA.Id);
    }
}

[TestFixture]
public class InventoryManagementScenarioTests : BusinessScenarioTestBase
{
    [Test]
    [TestCase("en-US")]
    [TestCase("es-ES")]
    public async Task CompleteInventoryWorkflow_WithLocalization_ShouldSucceed(string culture)
    {
        // Test complete inventory workflow in specified language
        SetTestCulture(culture);
        
        var company = await CreateTestCompanyAsync();
        var warehouse = await CreateTestWarehouseAsync(company.Id);
        var item = await CreateTestInventoryItemAsync(company.Id);
        
        // Execute inventory workflow
        await ReceiveInventoryAsync(item.Id, warehouse.Id, 100, 10.50m);
        await ReserveInventoryAsync(item.Id, warehouse.Id, 25);
        await IssueInventoryAsync(item.Id, warehouse.Id, 30);
        
        // Validate results with localized assertions
        await ValidateInventoryLevels(item.Id, warehouse.Id, expectedQuantity: 70, expectedReserved: 25);
        await ValidateLocalizedInventoryReports(culture, item.Id);
    }
}
```

### 12.3 Blazor Component Testing with bUnit

**Files to Create in `Sivar.Erp.Components.Tests/`:**

```csharp
[TestFixture]
public class AccountingDashboardComponentTests : LocalizedComponentTestBase
{
    [Test]
    [TestCase("en-US", "Total Revenue")]
    [TestCase("es-ES", "Ingresos Totales")]
    public void AccountingDashboard_WithLocalization_DisplaysCorrectLabels(string culture, string expectedRevenueLabel)
    {
        // Arrange
        SetupTestCulture(culture);
        var mockAccountingService = MockAccountingApiService();
        
        Services.AddSingleton(mockAccountingService);
        Services.AddLocalization();
        Services.AddSingleton(MockLocalizationService(culture));
        
        // Act
        var component = RenderComponent<AccountingDashboard>(parameters => parameters
            .Add(p => p.CompanyId, TestCompanyId)
            .Add(p => p.Revenue, 150000m)
            .Add(p => p.Expenses, 75000m));
        
        // Assert
        component.Find("label").TextContent.Should().Contain(expectedRevenueLabel);
        component.Find(".revenue-amount").TextContent.Should().Match(GetCultureCurrencyPattern(culture, 150000m));
    }
    
    [Test]
    public async Task AccountingDashboard_WithSignalRUpdates_UpdatesInRealTime()
    {
        // Arrange
        var mockSignalRService = MockSignalRService();
        Services.AddSingleton(mockSignalRService);
        
        var component = RenderComponent<AccountingDashboard>();
        
        // Act
        await mockSignalRService.TriggerTransactionPosted(new TransactionDto 
        { 
            Amount = 5000m, 
            CompanyId = TestCompanyId 
        });
        
        // Assert
        component.WaitForAssertion(() => 
            component.Find(".recent-transactions").Should().NotBeNull());
    }
}

[TestFixture]
public class LocalizedFormComponentTests : LocalizedComponentTestBase
{
    [Test]
    [TestCase("en-US")]
    [TestCase("es-ES")]
    public async Task TransactionForm_WithValidationErrors_ShowsLocalizedMessages(string culture)
    {
        // Arrange
        SetupTestCulture(culture);
        var component = RenderComponent<TransactionForm>();
        
        // Act
        var submitButton = component.Find("button[type=submit]");
        await submitButton.ClickAsync();
        
        // Assert
        var validationSummary = component.Find(".validation-summary");
        validationSummary.Should().NotBeNull();
        
        var expectedMessage = GetLocalizedValidationMessage(culture, "Amount.Required");
        validationSummary.TextContent.Should().Contain(expectedMessage);
    }
}
```

### 12.4 End-to-End Testing with Playwright

**Files to Create in `Sivar.Erp.E2E.Tests/`:**

```csharp
[TestFixture]
public class CompleteBusinessWorkflowE2ETests : PlaywrightTestBase
{
    [Test]
    [TestCase("en-US", "Dashboard")]
    [TestCase("es-ES", "Tablero")]
    public async Task CompleteWorkflow_MultiLanguage_ShouldSucceed(string culture, string expectedDashboardTitle)
    {
        // Arrange
        await SetBrowserLanguage(culture);
        
        // Act: Login and navigate
        var loginPage = new LoginPage(Page);
        await loginPage.NavigateToAsync();
        await loginPage.LoginAsync(TestUser.Email, TestUser.Password);
        
        // Select language
        var languageSelector = new LanguageSelector(Page);
        await languageSelector.SelectLanguageAsync(culture);
        
        // Verify dashboard loads with correct language
        var dashboardPage = new DashboardPage(Page);
        await dashboardPage.WaitForLoadAsync();
        
        var dashboardTitle = await dashboardPage.GetTitleAsync();
        dashboardTitle.Should().Contain(expectedDashboardTitle);
        
        // Execute complete business workflow
        await ExecuteAccountingWorkflowE2E(dashboardPage, culture);
        await ExecuteInventoryWorkflowE2E(dashboardPage, culture);
        await ExecutePaymentWorkflowE2E(dashboardPage, culture);
    }
    
    [Test]
    public async Task MultiCompanyWorkflow_E2E_ShouldMaintainContext()
    {
        // Test complete multi-company workflow
        var loginPage = new LoginPage(Page);
        await loginPage.NavigateToAsync();
        await loginPage.LoginAsync(TestUser.Email, TestUser.Password);
        
        // Switch between companies and verify data isolation
        var companySelector = new CompanySelectorPage(Page);
        await companySelector.SelectCompanyAsync("Test Company A");
        
        var accountingPage = new AccountingPage(Page);
        await accountingPage.CreateTransactionAsync("Test Transaction A");
        
        await companySelector.SelectCompanyAsync("Test Company B");
        await accountingPage.NavigateToAsync();
        
        // Verify Transaction A is not visible in Company B
        var transactions = await accountingPage.GetTransactionsAsync();
        transactions.Should().NotContain(t => t.Contains("Test Transaction A"));
    }
}

[TestFixture]
public class LocalizationE2ETests : PlaywrightTestBase
{
    [Test]
    public async Task LanguageSwitching_DuringWorkflow_ShouldPersistData()
    {
        // Test that switching languages during a workflow maintains state
        var loginPage = new LoginPage(Page);
        await loginPage.NavigateToAsync();
        await loginPage.LoginAsync(TestUser.Email, TestUser.Password);
        
        // Start creating transaction in English
        var accountingPage = new AccountingPage(Page);
        await accountingPage.NavigateToAsync();
        await accountingPage.StartNewTransactionAsync();
        await accountingPage.FillTransactionDetailsAsync("Test Transaction", "100.00");
        
        // Switch to Spanish
        var languageSelector = new LanguageSelector(Page);
        await languageSelector.SelectLanguageAsync("es-ES");
        
        // Verify form data is preserved and UI is in Spanish
        var transactionAmount = await accountingPage.GetTransactionAmountAsync();
        transactionAmount.Should().Be("100.00");
        
        var saveButton = await accountingPage.GetSaveButtonTextAsync();
        saveButton.Should().Be("Guardar"); // Spanish for "Save"
        
        // Complete transaction in Spanish
        await accountingPage.SaveTransactionAsync();
        
        // Verify success message in Spanish
        var successMessage = await accountingPage.GetSuccessMessageAsync();
        successMessage.Should().Contain("Transacción guardada exitosamente");
    }
}
```

### 12.5 Migration of CompleteAccountingWorkflowTest

**Enhanced Test Migration (`Sivar.Erp.BusinessScenarios.Tests/CompleteAccountingWorkflowIntegrationTest.cs`):**

```csharp
[TestFixture]
public class CompleteAccountingWorkflowIntegrationTest : BusinessScenarioTestBase
{
    private ICompanyApiService _companyService;
    private IAccountingApiService _accountingService;
    private IInventoryApiService _inventoryService;
    private IPaymentApiService _paymentService;
    
    [SetUp]
    public async Task Setup()
    {
        // Setup services with API clients instead of ObjectDb
        _companyService = GetRequiredService<ICompanyApiService>();
        _accountingService = GetRequiredService<IAccountingApiService>();
        _inventoryService = GetRequiredService<IInventoryApiService>();
        _paymentService = GetRequiredService<IPaymentApiService>();
        
        // Create test company and authenticate user
        await SetupTestCompanyAndUser();
    }
    
    [Test]
    [TestCase("en-US", "Purchase Invoice")]
    [TestCase("es-ES", "Factura de Compra")]
    public async Task ExecuteCompleteWorkflowTest_WithLocalization_ShouldSucceed(string culture, string expectedDocumentType)
    {
        // Set culture for test
        SetTestCulture(culture);
        
        var results = new List<string>();
        
        try
        {
            // Execute workflow using API calls instead of direct ObjectDb access
            await ExecutePurchaseWorkflow(results, culture);
            await ExecuteSalesWorkflow(results, culture);
            await ExecutePaymentWorkflow(results, culture);
            await ValidateAccountingResults(results, culture);
            
            // Verify localized data
            await VerifyLocalizedDocumentTypes(expectedDocumentType);
        }
        catch (Exception ex)
        {
            results.Add($"Error in {culture}: {ex.Message}");
            throw;
        }
        
        var log = string.Join(Environment.NewLine, results);
        TestContext.WriteLine(log);
        
        results.Should().NotContain(r => r.Contains("Error"));
        results.Should().Contain(r => r.Contains("Workflow completed successfully"));
    }
    
    private async Task ExecutePurchaseWorkflow(List<string> results, string culture)
    {
        // Create purchase document via API
        var purchaseDocument = await CreatePurchaseDocumentViaApi(culture);
        results.Add($"Created purchase document: {purchaseDocument.DocumentNumber}");
        
        // Post transaction via API
        var transaction = await _accountingService.CreateTransactionFromDocumentAsync(purchaseDocument.Id);
        results.Add($"Posted transaction: {transaction.TransactionNumber}");
        
        // Verify inventory update via API
        var inventoryLevels = await _inventoryService.GetStockLevelsAsync(TestCompanyId);
        results.Add($"Updated inventory levels: {inventoryLevels.Count} items");
    }
    
    // Additional workflow methods using API calls...
}
```

## Phase 13: Performance and Optimization (Weeks 28-29)

### 13.1 Performance Monitoring

**Files to Create:**
```
Core/Infrastructure/Performance/
├── IPerformanceService.cs
├── PerformanceService.cs
├── QueryOptimizationService.cs
└── CachingService.cs
```

### 13.2 Query Optimization

**Optimizations:**
- Entity Framework query optimization
- Proper indexing strategies
- Lazy loading configuration
- Bulk operations support

## Multi-Tenant Implementation Details (Enhanced with Keycloak)

### Hierarchical Tenant Architecture

#### User-Company-Branch Model
```
Keycloak User (Global Identity)
├── AuthProviders: Email, Google, Facebook, Microsoft
├── User Profile: Name, Email, Preferences
└── UserCompany Associations:
    ├── Company A (Owner)
    │   ├── Role: Owner
    │   ├── Branches: All branches access
    │   ├── Permissions: Full access
    │   └── Invited Users:
    │       ├── User B (Admin) → Branch 1, 2
    │       └── User C (Employee) → Branch 1 only
    └── Company B (Invited User)
        ├── Role: Manager
        ├── Branches: Branch 1 only
        └── Permissions: Read/Write (no delete)
```

### Database Strategy (Company-Based Isolation)

#### Option 1: Separate Databases per Company (Recommended)
```csharp
public class CompanyDbContextFactory : IDbContextFactory<ErpDbContext>
{
    private readonly IConfiguration _configuration;
    private readonly ICompanyResolver _companyResolver;
    private readonly IKeycloakService _keycloakService;
    
    public ErpDbContext CreateDbContext()
    {
        var userContext = _keycloakService.GetCurrentUser();
        var companyContext = _companyResolver.GetCurrentCompany(userContext.UserId);
        
        // Validate user has access to company
        if (!await _companyResolver.UserHasAccessAsync(userContext.UserId, companyContext.CompanyId))
            throw new UnauthorizedAccessException("User does not have access to this company");
        
        var connectionString = GetCompanyConnectionString(companyContext.CompanyId);
        
        var options = new DbContextOptionsBuilder<ErpDbContext>()
            .UseNpgsql(connectionString)
            .Options;
            
        return new ErpDbContext(options, companyContext, userContext);
    }
    
    private string GetCompanyConnectionString(Guid companyId)
    {
        var template = _configuration.GetConnectionString("CompanyTemplate");
        return string.Format(template, companyId);
    }
}
```

#### Option 2: Shared Database with Company Filtering
```csharp
public class ErpDbContext : DbContext
{
    public CompanyContext CompanyContext { get; }
    public UserContext UserContext { get; }
    
    public ErpDbContext(DbContextOptions<ErpDbContext> options, 
                       CompanyContext companyContext, 
                       UserContext userContext) 
        : base(options)
    {
        CompanyContext = companyContext;
        UserContext = userContext;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply global company filters
        modelBuilder.ApplyCompanyFilters(CompanyContext.CompanyId);
        
        // Apply branch filters if user has limited branch access
        if (CompanyContext.BranchId.HasValue)
        {
            modelBuilder.ApplyBranchFilters(CompanyContext.BranchId.Value);
        }
        
        // Configure indexes for company queries
        modelBuilder.ConfigureCompanyIndexes();
    }
}
```

### Keycloak Integration Details

#### Authentication Flow
```csharp
public class KeycloakAuthService : IKeycloakAuthService
{
    public async Task<AuthResult> LoginAsync(string email, string password, string provider = "email")
    {
        // Authenticate with Keycloak
        var keycloakResult = await _keycloakClient.AuthenticateAsync(email, password, provider);
        
        if (!keycloakResult.IsSuccess)
            return AuthResult.Failed(keycloakResult.Error);
        
        // Get or create user in our system
        var user = await _userService.GetOrCreateUserAsync(keycloakResult.User);
        
        // Get user's companies
        var companies = await _companyService.GetUserCompaniesAsync(user.KeycloakUserId);
        
        return AuthResult.Success(keycloakResult.Token, user, companies);
    }
    
    public async Task<CompanyContext> SwitchCompanyAsync(string userId, Guid companyId, Guid? branchId = null)
    {
        // Validate access
        var hasAccess = await _companyService.UserHasAccessAsync(userId, companyId, branchId);
        if (!hasAccess)
            throw new UnauthorizedAccessException();
        
        // Create company context
        var company = await _companyService.GetCompanyAsync(companyId);
        var branch = branchId.HasValue ? await _branchService.GetBranchAsync(branchId.Value) : null;
        var userCompany = await _userCompanyService.GetUserCompanyAsync(userId, companyId);
        
        return new CompanyContext
        {
            CompanyId = companyId,
            CompanyName = company.Name,
            BranchId = branchId,
            BranchName = branch?.Name,
            UserRole = userCompany.Role,
            Permissions = userCompany.Permissions
        };
    }
}
```

#### User Invitation System
```csharp
public class InvitationService : IInvitationService
{
    public async Task<UserInvitationDto> SendInvitationAsync(SendInvitationDto request, string inviterId)
    {
        // Validate inviter has permission to invite users
        var inviterAccess = await _companyService.UserHasPermissionAsync(
            inviterId, request.CompanyId, "INVITE_USERS");
        
        if (!inviterAccess)
            throw new UnauthorizedAccessException("User cannot invite users to this company");
        
        // Check if user already has access
        var existingAccess = await _userCompanyService.UserHasAccessAsync(
            request.InviteeEmail, request.CompanyId);
        
        if (existingAccess)
            throw new InvalidOperationException("User already has access to this company");
        
        // Create invitation
        var invitation = new UserInvitation
        {
            InviterUserId = inviterId,
            InviteeEmail = request.InviteeEmail,
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            Role = request.Role,
            InvitationToken = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Status = InvitationStatus.Pending,
            Message = request.Message
        };
        
        await _repository.AddAsync(invitation);
        await _unitOfWork.SaveChangesAsync();
        
        // Send invitation email
        await _emailService.SendInvitationEmailAsync(invitation);
        
        return _mapper.Map<UserInvitationDto>(invitation);
    }
    
    public async Task<UserInvitationDto> AcceptInvitationAsync(string token, string userId)
    {
        var invitation = await _repository.GetByTokenAsync(token);
        
        if (invitation == null || invitation.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired invitation");
        
        // Verify email matches Keycloak user
        var keycloakUser = await _keycloakService.GetUserAsync(userId);
        if (keycloakUser.Email != invitation.InviteeEmail)
            throw new UnauthorizedAccessException("Email mismatch");
        
        // Create user-company association
        var userCompany = new UserCompany
        {
            UserId = userId,
            CompanyId = invitation.CompanyId,
            BranchId = invitation.BranchId,
            Role = invitation.Role,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        await _userCompanyRepository.AddAsync(userCompany);
        
        // Update invitation status
        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;
        
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<UserInvitationDto>(invitation);
    }
}
```

## Migration Execution Strategy

### Week-by-Week Execution

**Weeks 1-2: Foundation Infrastructure**
- Set up EF Core infrastructure
- Create Keycloak realm and configuration
- Implement multi-company framework
- Set up base entity types

**Weeks 3-4: User Management & Company Structure**
- Implement Keycloak integration
- Create user, company, and branch entities
- Build user invitation system
- Implement company management APIs

**Weeks 5-6: Core Domain Migration**
- Migrate base entity types with company context
- Set up document system with multi-tenancy
- Create repository patterns with company filtering

**Weeks 7-8: Business Entities Module**
- Migrate business entities with company context
- Implement client-server architecture
- Create shared Blazor components

**Weeks 9-12: Accounting Module**
- Migrate accounting entities and services
- Implement company-aware financial data
- Create accounting APIs and SignalR hubs
- Build accounting UI components

**Weeks 13-15: Inventory Module**
- Migrate inventory with company/branch context
- Implement multi-location inventory
- Create inventory APIs and real-time updates

**Weeks 16-21: Additional Modules**
- Tax module migration (Weeks 16-17)
- Payment module migration (Weeks 18-19)
- Security module migration (Weeks 20-21)

**Weeks 22-23: Data Import/Export**
- Company-aware data import/export
- Migration utilities from ObjectDb

**Weeks 24-25: Module Integration**
- Complete module integration
- Keycloak and multi-company testing

**Weeks 26-27: Testing Infrastructure**
- Test migration with company context
- Multi-tenant test scenarios
- API and client integration tests

**Weeks 28-29: Performance & Optimization**
- Performance tuning
- Query optimization for multi-tenancy
- Documentation and deployment preparation

### Risk Mitigation

1. **Parallel Development**: New system developed alongside existing system
2. **Incremental Testing**: Each module tested independently
3. **Data Validation**: Comprehensive data migration validation
4. **Rollback Strategy**: Ability to rollback to ObjectDb if needed
5. **Performance Benchmarks**: Performance monitoring throughout migration

### Success Criteria

1. **Functional Parity**: All existing functionality replicated
2. **Performance**: Equal or better performance than ObjectDb
3. **Test Coverage**: 90%+ test coverage on new modules
4. **Multi-Tenancy**: Working multi-tenant and multi-branch support
5. **Backward Compatibility**: Existing tests pass during transition

### Deployment Strategy (Cloud-Native)

1. **Phase 1**: Deploy API infrastructure to cloud (Azure/AWS)
   - PostgreSQL database cluster
   - API with authentication and multi-tenancy
   - Container orchestration (Kubernetes/Docker)
   
2. **Phase 2**: Deploy client applications
   - Blazor Server app (immediate compatibility)
   - Blazor WebAssembly (progressive web app)
   - MAUI apps to app stores
   
3. **Phase 3**: Data migration and tenant onboarding
   - Migrate existing tenant data via API
   - Gradual rollout to user groups
   - A/B testing between old and new systems
   
4. **Phase 4**: Complete cutover
   - Real-time synchronization testing
   - Performance optimization
   - Legacy system deprecation

### Cloud Architecture Benefits

1. **Scalability**: Horizontal scaling of API instances
2. **Reliability**: Database clustering and failover
3. **Multi-Platform**: Single API serves all client types
4. **Real-Time**: SignalR for live collaboration
5. **Offline Support**: Local data caching in WASM/MAUI
6. **Updates**: Independent client and server deployments

This migration plan ensures a smooth, incremental transition from the current ObjectDb-based system to a robust EF Core-based architecture with multi-tenancy support while maintaining system stability throughout the process.
