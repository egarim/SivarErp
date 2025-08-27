using NUnit.Framework;
using Sivar.Erp.Core.Application.DTOs.Accounting;
using Sivar.Erp.Core.Application.DTOs.Inventory;
using Sivar.Erp.Core.Shared.DTOs.Identity;
using Sivar.Erp.Core.Shared.Responses;
using Sivar.Erp.Core.Domain.Entities.Accounting;

namespace Sivar.Erp.Core.Tests.Integration;

/// <summary>
/// Simple integration tests to validate API endpoints are working
/// </summary>
[TestFixture]
public class BasicApiTests : ApiTestBase
{
    /// <summary>
    /// Test GET /api/accounts endpoint
    /// </summary>
    [Test]
    public async Task GetAccounts_ShouldReturnSuccessResponse()
    {
        // Act
        var response = await GetAsync<ApiResponse<IEnumerable<AccountSummaryDto>>>("/api/accounts");

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        
        Console.WriteLine($"✅ Accounts API working - Retrieved {response.Data?.Count()} accounts");
    }

    /// <summary>
    /// Test POST /api/accounts endpoint
    /// </summary>
    [Test]
    public async Task CreateAccount_ShouldReturnSuccessResponse()
    {
        // Arrange
        var createAccountDto = new CreateAccountDto
        {
            Code = "TEST001",
            Name = "Test Account",
            Type = Domain.Enums.AccountType.Asset,
            Category = AccountCategory.CurrentAssets,
            Description = "Test account for integration testing"
        };

        // Act
        var response = await PostAsync<CreateAccountDto, ApiResponse<AccountDto>>("/api/accounts", createAccountDto);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data.Code, Is.EqualTo("TEST001"));
        
        Console.WriteLine($"✅ Account creation working - Created account: {response.Data.Code}");
    }

    /// <summary>
    /// Test GET /api/companies/my-companies endpoint
    /// </summary>
    [Test]
    public async Task GetCompanies_ShouldReturnSuccessResponse()
    {
        // Act
        var response = await GetAsync<ApiResponse<IEnumerable<CompanyDto>>>("api/companies/my-companies");

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        
        Console.WriteLine($"✅ Companies API working - Retrieved {response.Data?.Count()} companies");
    }

    /// <summary>
    /// Test POST /api/companies endpoint
    /// </summary>
    [Test]
    public async Task CreateCompany_ShouldReturnSuccessResponse()
    {
        // Arrange
        var createCompanyDto = new CreateCompanyDto
        {
            Name = "Test Company",
            Description = "Test company for integration testing",
            Industry = "Technology"
        };

        // Act
        var response = await PostAsync<CreateCompanyDto, ApiResponse<CompanyDto>>("/api/companies", createCompanyDto);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data.Name, Is.EqualTo("Test Company"));
        
        Console.WriteLine($"✅ Company creation working - Created company: {response.Data.Name}");
    }

    /// <summary>
    /// Complete workflow test: Create company, then create account
    /// </summary>
    [Test]
    public async Task CompleteWorkflow_ShouldWork()
    {
        // Step 1: Create a company
        var company = new CreateCompanyDto
        {
            Name = "Workflow Test Company",
            Description = "Testing the complete workflow",
            Industry = "Testing"
        };

        var companyResponse = await PostAsync<CreateCompanyDto, ApiResponse<CompanyDto>>("/api/companies", company);
        Assert.That(companyResponse?.IsSuccess, Is.True);

        // Step 2: Create accounts for the company
        var accounts = new[]
        {
            new CreateAccountDto { Code = "1001", Name = "Cash", Type = Domain.Enums.AccountType.Asset, Category = AccountCategory.CurrentAssets },
            new CreateAccountDto { Code = "2001", Name = "Accounts Payable", Type = Domain.Enums.AccountType.Liability, Category = AccountCategory.CurrentLiabilities }
        };

        foreach (var account in accounts)
        {
            var accountResponse = await PostAsync<CreateAccountDto, ApiResponse<AccountDto>>("/api/accounts", account);
            Assert.That(accountResponse?.IsSuccess, Is.True);
            Console.WriteLine($"Created account: {accountResponse?.Data?.Code} - {accountResponse?.Data?.Name}");
        }

        // Step 3: Verify we can get companies
        var companiesResponse = await GetAsync<ApiResponse<IEnumerable<CompanyDto>>>("api/companies/my-companies");
        Assert.That(companiesResponse?.IsSuccess, Is.True);
        Assert.That(companiesResponse?.Data?.Any(c => c.Name == "Workflow Test Company"), Is.True);

        // Step 4: Verify we can get accounts
        var accountsResponse = await GetAsync<ApiResponse<IEnumerable<AccountSummaryDto>>>("/api/accounts");
        Assert.That(accountsResponse?.IsSuccess, Is.True);
        Assert.That(accountsResponse?.Data?.Any(a => a.Code == "1001"), Is.True);

        Console.WriteLine("✅ Complete workflow test passed!");
    }

    /// <summary>
    /// Test journal entries functionality
    /// </summary>
    [Test]
    public async Task JournalEntries_ShouldWork()
    {
        // First, create a company and some accounts
        var company = new CreateCompanyDto
        {
            Name = "Journal Test Company",
            Description = "Testing journal entries",
            Industry = "Testing"
        };

        var companyResponse = await PostAsync<CreateCompanyDto, ApiResponse<CompanyDto>>("/api/companies", company);
        Assert.That(companyResponse?.IsSuccess, Is.True);

        // Create accounts for the journal entry
        var accounts = new[]
        {
            new CreateAccountDto { Code = "1100", Name = "Cash", Type = Domain.Enums.AccountType.Asset, Category = AccountCategory.CurrentAssets },
            new CreateAccountDto { Code = "4100", Name = "Sales Revenue", Type = Domain.Enums.AccountType.Revenue, Category = AccountCategory.OperatingRevenue }
        };

        foreach (var account in accounts)
        {
            var accountResponse = await PostAsync<CreateAccountDto, ApiResponse<AccountDto>>("/api/accounts", account);
            Assert.That(accountResponse?.IsSuccess, Is.True);
        }

        // Test journal entries health endpoint
        var healthResponse = await GetAsync<ApiResponse<string>>("/api/simplejournalentries/health");
        Assert.That(healthResponse?.IsSuccess, Is.True);
        Console.WriteLine("✅ Journal Entries API health check passed");

        // Get accounts for journal entries
        Client.DefaultRequestHeaders.Add("X-Company-Id", companyResponse?.Data?.Id.ToString());
        var accountsResponse = await GetAsync<ApiResponse<IEnumerable<AccountSummaryDto>>>("/api/simplejournalentries/accounts");
        Assert.That(accountsResponse?.IsSuccess, Is.True);
        Console.WriteLine($"✅ Retrieved {accountsResponse?.Data?.Count()} accounts for journal entries");

        // Create a simple journal entry
        var journalEntry = new 
        {
            Date = DateTime.Today,
            Description = "Test journal entry - Cash sale",
            Lines = new[]
            {
                new { AccountCode = "1100", Description = "Cash received", DebitAmount = 1000.00m, CreditAmount = 0.00m },
                new { AccountCode = "4100", Description = "Sales revenue", DebitAmount = 0.00m, CreditAmount = 1000.00m }
            }
        };

        var journalResponse = await PostAsync<object, ApiResponse<object>>("/api/simplejournalentries/simple", journalEntry);
        Assert.That(journalResponse?.IsSuccess, Is.True);
        Console.WriteLine("✅ Journal entry created successfully");

        Console.WriteLine("✅ Journal Entries functionality test passed!");
    }

    /// <summary>
    /// Test inventory (products) functionality
    /// </summary>
    [Test]
    public async Task Inventory_ProductsApi_ShouldWork()
    {
        // Test products health endpoint
        var healthResponse = await GetAsync<ApiResponse<string>>("/api/products/health");
        Assert.That(healthResponse?.IsSuccess, Is.True);
        Console.WriteLine("✅ Products API health check passed");

        // Get initial products (should be empty)
        var initialProductsResponse = await GetAsync<ApiResponse<IEnumerable<ProductSummaryDto>>>("/api/products");
        Assert.That(initialProductsResponse?.IsSuccess, Is.True);
        Console.WriteLine($"✅ Retrieved {initialProductsResponse?.Data?.Count()} initial products");

        // Create a test product
        var createProductDto = new 
        {
            Code = "PROD001",
            Name = "Test Product",
            Description = "Test product for integration testing",
            Type = 3, // ProductType.Finished
            Category = "Electronics",
            UnitOfMeasure = "EA",
            StandardCost = 100.00m,
            MinimumStock = 10.0m,
            ReorderPoint = 20.0m,
            IsStockable = true,
            IsPurchasable = true,
            IsSaleable = true
        };

        var createResponse = await PostAsync<object, ApiResponse<ProductDto>>("/api/products", createProductDto);
        Assert.That(createResponse?.IsSuccess, Is.True);
        Assert.That(createResponse?.Data?.Code, Is.EqualTo("PROD001"));
        Console.WriteLine($"✅ Product created successfully: {createResponse?.Data?.Code} - {createResponse?.Data?.Name}");

        // Get the created product by ID
        var productId = createResponse?.Data?.Id;
        var getProductResponse = await GetAsync<ApiResponse<ProductDto>>($"/api/products/{productId}");
        Assert.That(getProductResponse?.IsSuccess, Is.True);
        Assert.That(getProductResponse?.Data?.Code, Is.EqualTo("PROD001"));
        Console.WriteLine($"✅ Product retrieved by ID: {getProductResponse?.Data?.Code}");

        // Get all products (should now have our created product)
        var allProductsResponse = await GetAsync<ApiResponse<IEnumerable<ProductSummaryDto>>>("/api/products");
        Assert.That(allProductsResponse?.IsSuccess, Is.True);
        Assert.That(allProductsResponse?.Data?.Count(), Is.GreaterThanOrEqualTo(1));
        Assert.That(allProductsResponse?.Data?.Any(p => p.Code == "PROD001"), Is.True);
        Console.WriteLine($"✅ Products list contains our test product - Total products: {allProductsResponse?.Data?.Count()}");

        Console.WriteLine("✅ Inventory Products API functionality test passed!");
    }
}
