using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Domain.Interfaces;
using Sivar.Erp.Core.Infrastructure.Repositories;
using Sivar.Erp.Core.Infrastructure.Repositories.Identity;
using Sivar.Erp.Core.Infrastructure.Repositories.Accounting;
using Sivar.Erp.Core.Application.Services.Identity;
using Sivar.Erp.Core.Application.Services.Accounting;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.Accounting;

// Monitoring imports disabled temporarily
/*
using Sivar.Erp.Core.Infrastructure.Performance;
using Sivar.Erp.Core.Infrastructure.Logging;
using Sivar.Erp.Core.Infrastructure.Telemetry;
*/

var builder = WebApplication.CreateBuilder(args);

// Temporarily disable monitoring infrastructure to focus on Chart of Accounts
// TODO: Re-enable once Chart of Accounts API is functional
/*
// Add monitoring infrastructure
builder.Services.AddSivarErpObservability(options =>
{
    options.ServiceName = "Sivar.Erp.Core.Api";
    options.ServiceVersion = "1.0.0";
    options.JaegerEndpoint = builder.Configuration["Telemetry:JaegerEndpoint"];
    options.OtlpEndpoint = builder.Configuration["Telemetry:OtlpEndpoint"];
});

// Add logging infrastructure
builder.Services.AddErpLogging();

// Add performance monitoring
builder.Services.AddScoped<IPerformanceMonitor, InMemoryPerformanceMonitor>();
*/

// Add services to the container
builder.Services.AddControllers();

// Add EF Core with InMemory database for development
builder.Services.AddDbContext<ErpDbContext>(options =>
{
    options.UseInMemoryDatabase("SivarErpCoreDb");
    
    // Performance interceptor disabled temporarily
    // TODO: Re-enable once monitoring infrastructure is fixed
    /*
    var performanceMonitor = serviceProvider.GetService<IPerformanceMonitor>();
    var loggingService = serviceProvider.GetService<IErpLoggingService>();
    
    if (performanceMonitor != null && loggingService != null)
    {
        options.AddInterceptors(new EfCorePerformanceInterceptor(performanceMonitor, loggingService));
    }
    */
});

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(ITenantRepository<>), typeof(TenantRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add repository performance decorators
// Note: We'll manually wrap repositories for now since we don't have a decorator library
// This provides performance monitoring for all repository operations

// Register specific repositories
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserInvitationRepository, UserInvitationRepository>();

// Register accounting repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();

// Register Business Entity repositories (Phase 4)
builder.Services.AddScoped<Sivar.Erp.Core.Domain.Interfaces.Repositories.BusinessEntities.IBusinessEntityRepository, Sivar.Erp.Core.Infrastructure.Repositories.BusinessEntities.BusinessEntityRepository>();

// Register application services
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IUserInvitationService, UserInvitationService>();

// Register accounting services
builder.Services.AddScoped<IAccountService, AccountService>();
// Journal Entry Service - Minimal implementation for testing
builder.Services.AddScoped<IJournalEntryService, MinimalJournalEntryService>();

// Register Business Entity services (Phase 4)
builder.Services.AddScoped<Sivar.Erp.Core.Application.Services.BusinessEntities.IBusinessEntityService, Sivar.Erp.Core.Application.Services.BusinessEntities.BusinessEntityService>();

// Register inventory repositories
builder.Services.AddScoped<Sivar.Erp.Core.Domain.Interfaces.Repositories.Inventory.IProductRepository, Sivar.Erp.Core.Infrastructure.Repositories.Inventory.ProductRepository>();
builder.Services.AddScoped<Sivar.Erp.Core.Domain.Interfaces.Repositories.Inventory.IWarehouseRepository, Sivar.Erp.Core.Infrastructure.Repositories.Inventory.WarehouseRepository>();

// Register inventory services
builder.Services.AddScoped<Sivar.Erp.Core.Application.Services.Inventory.IProductService, Sivar.Erp.Core.Application.Services.Inventory.ProductService>();

// Register sales repositories
builder.Services.AddScoped<Sivar.Erp.Core.Domain.Interfaces.Repositories.Sales.ICustomerRepository, Sivar.Erp.Core.Infrastructure.Repositories.Sales.CustomerRepository>();
builder.Services.AddScoped<Sivar.Erp.Core.Domain.Interfaces.Repositories.Sales.ISalesOrderRepository, Sivar.Erp.Core.Infrastructure.Repositories.Sales.SalesOrderRepository>();
builder.Services.AddScoped<Sivar.Erp.Core.Domain.Interfaces.Repositories.Sales.ISalesOrderLineRepository, Sivar.Erp.Core.Infrastructure.Repositories.Sales.SalesOrderLineRepository>();
builder.Services.AddScoped<Sivar.Erp.Core.Domain.Interfaces.Repositories.Sales.IInvoiceRepository, Sivar.Erp.Core.Infrastructure.Repositories.Sales.InvoiceRepository>();
builder.Services.AddScoped<Sivar.Erp.Core.Domain.Interfaces.Repositories.Sales.IInvoiceLineRepository, Sivar.Erp.Core.Infrastructure.Repositories.Sales.InvoiceLineRepository>();

// Register sales services
builder.Services.AddScoped<Sivar.Erp.Core.Application.Services.Sales.CustomerService>();

// Register Phase 3 multi-tenancy and RBAC services
builder.Services.AddHttpContextAccessor();
// TODO: Fix service compilation errors before enabling
// builder.Services.AddScoped<Sivar.Erp.Core.Infrastructure.Services.Identity.ITenantContextService, Sivar.Erp.Core.Infrastructure.Services.Identity.TenantContextService>();
// builder.Services.AddScoped<Sivar.Erp.Core.Infrastructure.Services.Authorization.IRoleBasedAccessControlService, Sivar.Erp.Core.Infrastructure.Services.Authorization.RoleBasedAccessControlService>();

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Sivar ERP Core API", 
        Version = "v1",
        Description = "Modern ERP Core API with multi-tenant support"
    });
    
    // Include XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sivar ERP Core API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Add tenant context middleware for multi-tenancy
// TODO: Fix middleware compilation errors before enabling
// app.UseMiddleware<Sivar.Erp.Core.Infrastructure.Middleware.TenantContextMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Ensure database is created with seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
    context.Database.EnsureCreated();
    
    // Add seed data for development
    await SeedDevelopmentData(context);
}

app.Run();

async Task SeedDevelopmentData(ErpDbContext context)
{
    if (!context.Users.Any())
    {
        var testUser = new Sivar.Erp.Core.Domain.Entities.Identity.User
        {
            Id = Guid.NewGuid(),
            KeycloakUserId = "test-user-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(testUser);
        await context.SaveChangesAsync();
    }
}

// Make Program class accessible for testing
public partial class Program { }
