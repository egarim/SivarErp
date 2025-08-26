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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add EF Core with InMemory database for development
builder.Services.AddDbContext<ErpDbContext>(options =>
    options.UseInMemoryDatabase("SivarErpCoreDb"));

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(ITenantRepository<>), typeof(TenantRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register specific repositories
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserInvitationRepository, UserInvitationRepository>();

// Register accounting repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();

// Register application services
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IUserInvitationService, UserInvitationService>();

// Register accounting services
builder.Services.AddScoped<IAccountService, AccountService>();
// Note: JournalEntryService registration temporarily commented out due to DI resolution issue
// The service exists and compiles correctly, but there's a runtime type resolution issue
builder.Services.AddScoped<IAccountService, AccountService>();
// Test - disable DI registration until we fix the issue
//builder.Services.AddScoped<Sivar.Erp.Core.Application.Services.Accounting.IJournalEntryService, Sivar.Erp.Core.Application.Services.Accounting.JournalEntryService>();

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
