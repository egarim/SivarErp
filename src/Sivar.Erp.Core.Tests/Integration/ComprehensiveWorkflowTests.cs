using Sivar.Erp.Core.Application.DTOs.Accounting;
using Sivar.Erp.Core.Shared.DTOs.Identity;
using Sivar.Erp.Core.Shared.Responses;
using System.Net;

namespace Sivar.Erp.Core.Tests.Integration;

/// <summary>
/// End-to-end workflow tests for the complete ERP API
/// </summary>
[TestFixture]
public class ComprehensiveWorkflowTests : ApiTestBase
{
    [Test]
    public async Task CompleteAccountingWorkflow_ShouldExecuteSuccessfully()
    {
        Console.WriteLine("=== Starting Complete Accounting Workflow Test ===");
        
        // Step 1: Create a company
        Console.WriteLine("\n1. Creating a test company...");
        var createCompanyDto = new CreateCompanyDto
        {
            Name = "Workflow Test Company",
            TaxId = "WORKFLOW-001",
            Email = "workflow@test.com", 
            Phone = "+503-7777-8888",
            Address = "777 Workflow Street, Test City"
        };

        var companyResponse = await PostAsync<CreateCompanyDto, ApiResponse<CompanyDto>>("/api/companies", createCompanyDto);
        Assert.That(companyResponse?.IsSuccess, Is.True);
        Console.WriteLine($"✅ Company created: {companyResponse.Data.Name} (ID: {companyResponse.Data.Id})");

        // Step 2: Create a chart of accounts
        Console.WriteLine("\n2. Setting up chart of accounts...");
        var accounts = new[]
        {
            new CreateAccountDto
            {
                AccountCode = "1000",
                AccountName = "Cash and Cash Equivalents",
                AccountType = Domain.Enums.AccountType.Asset,
                Description = "Checking and savings accounts"
            },
            new CreateAccountDto
            {
                AccountCode = "1100", 
                AccountName = "Accounts Receivable",
                AccountType = Domain.Enums.AccountType.Asset,
                Description = "Customer balances"
            },
            new CreateAccountDto
            {
                AccountCode = "2000",
                AccountName = "Accounts Payable",
                AccountType = Domain.Enums.AccountType.Liability,
                Description = "Vendor balances"
            },
            new CreateAccountDto
            {
                AccountCode = "3000",
                AccountName = "Owner's Equity",
                AccountType = Domain.Enums.AccountType.Equity,
                Description = "Business equity"
            },
            new CreateAccountDto
            {
                AccountCode = "4000",
                AccountName = "Sales Revenue",
                AccountType = Domain.Enums.AccountType.Revenue,
                Description = "Product and service sales"
            },
            new CreateAccountDto
            {
                AccountCode = "5000",
                AccountName = "Cost of Goods Sold",
                AccountType = Domain.Enums.AccountType.Expense,
                Description = "Direct costs"
            }
        };

        var createdAccounts = new List<AccountDto>();
        foreach (var account in accounts)
        {
            var accountResponse = await PostAsync<CreateAccountDto, ApiResponse<AccountDto>>("/api/accounts", account);
            Assert.That(accountResponse?.IsSuccess, Is.True);
            createdAccounts.Add(accountResponse.Data);
            Console.WriteLine($"✅ Account created: {account.AccountCode} - {account.AccountName}");
        }

        // Step 3: Verify all accounts are retrievable
        Console.WriteLine("\n3. Verifying chart of accounts...");
        var allAccountsResponse = await GetAsync<ApiResponse<IEnumerable<AccountSummaryDto>>>("/api/accounts");
        Assert.That(allAccountsResponse?.IsSuccess, Is.True);
        
        var accountsList = allAccountsResponse.Data.ToList();
        Assert.That(accountsList.Count, Is.GreaterThanOrEqualTo(accounts.Length));
        
        foreach (var createdAccount in createdAccounts)
        {
            var foundAccount = accountsList.FirstOrDefault(a => a.Id == createdAccount.Id);
            Assert.That(foundAccount, Is.Not.Null, $"Account {createdAccount.AccountCode} not found");
        }
        Console.WriteLine($"✅ Chart of accounts verified: {accountsList.Count} total accounts");

        // Step 4: Test individual account retrieval
        Console.WriteLine("\n4. Testing individual account retrieval...");
        foreach (var account in createdAccounts.Take(3)) // Test first 3 accounts
        {
            var accountDetailResponse = await GetAsync<ApiResponse<AccountDto>>($"/api/accounts/{account.Id}");
            Assert.That(accountDetailResponse?.IsSuccess, Is.True);
            Assert.That(accountDetailResponse?.Data?.Id, Is.EqualTo(account.Id));
            Console.WriteLine($"✅ Account details retrieved: {account.AccountCode}");
        }

        // Step 5: Test company retrieval
        Console.WriteLine("\n5. Verifying company data...");
        var companyDetailResponse = await GetAsync<ApiResponse<CompanyDto>>($"/api/companies/{companyResponse.Data.Id}");
        Assert.That(companyDetailResponse?.IsSuccess, Is.True);
        Assert.That(companyDetailResponse?.Data?.Name, Is.EqualTo(createCompanyDto.Name));
        Console.WriteLine($"✅ Company details verified: {companyDetailResponse.Data.Name}");

        // Step 6: Test error handling
        Console.WriteLine("\n6. Testing error handling...");
        var nonExistentAccountResponse = await Client.GetAsync($"/api/accounts/{Guid.NewGuid()}");
        Assert.That(nonExistentAccountResponse.StatusCode, 
                   Is.EqualTo(HttpStatusCode.NotFound).Or.EqualTo(HttpStatusCode.BadRequest));
        Console.WriteLine("✅ Error handling working correctly");

        Console.WriteLine("\n=== Complete Accounting Workflow Test PASSED ===");
    }

    [Test]
    public async Task CompleteSalesWorkflow_ShouldExecuteSuccessfully()
    {
        Console.WriteLine("=== Starting Complete Sales Workflow Test ===");
        
        // Step 1: Create a company first
        Console.WriteLine("\n1. Creating a test company...");
        var createCompanyDto = new CreateCompanyDto
        {
            Name = "Sales Test Company",
            Description = "Company for testing sales functionality",
            Industry = "Retail"
        };

        var companyResponse = await PostAsync<CreateCompanyDto, ApiResponse<CompanyDto>>("/api/companies", createCompanyDto);
        Assert.That(companyResponse?.IsSuccess, Is.True);
        Console.WriteLine($"✅ Company created: {companyResponse.Data.Name} (ID: {companyResponse.Data.Id})");

        // Step 2: Create customers
        Console.WriteLine("\n2. Creating customers...");
        var customers = new[]
        {
            new
            {
                Code = "CUST001",
                Name = "Enterprise Customer Corp",
                Type = Domain.Enums.CustomerType.Corporate,
                CreditLimit = 100000m
            },
            new
            {
                Code = "CUST002", 
                Name = "John Doe",
                Type = Domain.Enums.CustomerType.Individual,
                CreditLimit = 5000m
            }
        };

        var createdCustomers = new List<dynamic>();
        foreach (var customer in customers)
        {
            var createCustomerDto = new
            {
                Code = customer.Code,
                Name = customer.Name,
                ContactPerson = customer.Type == Domain.Enums.CustomerType.Corporate ? "Account Manager" : customer.Name,
                Email = $"{customer.Code.ToLower()}@test.com",
                Phone = "555-0100",
                CustomerType = customer.Type,
                PaymentTerms = Domain.Enums.PaymentTerms.Net30,
                CreditLimit = customer.CreditLimit,
                BillingAddress = "123 Customer St",
                BillingCity = "Customer City",
                BillingState = "CS",
                BillingPostalCode = "12345",
                BillingCountry = "USA"
            };

            var customerResponse = await PostAsync<object, dynamic>("/api/customers", createCustomerDto);
            Assert.That(customerResponse?.IsSuccess, Is.True);
            createdCustomers.Add(customerResponse.Data);
            Console.WriteLine($"✅ Customer created: {customer.Code} - {customer.Name}");
        }

        // Step 3: Retrieve and validate customers
        Console.WriteLine("\n3. Retrieving and validating customers...");
        var allCustomersResponse = await GetAsync<ApiResponse<dynamic>>("/api/customers");
        Assert.That(allCustomersResponse?.IsSuccess, Is.True);
        Console.WriteLine($"✅ Retrieved {allCustomersResponse.Data.Count} customers total");

        // Step 4: Test customer lookup by code
        foreach (var customer in createdCustomers)
        {
            var customerByCodeResponse = await GetAsync<dynamic>($"/api/customers/by-code/{customer.Code}");
            Assert.That(customerByCodeResponse?.IsSuccess, Is.True);
            Console.WriteLine($"✅ Customer lookup by code successful: {customer.Code}");
        }

        // Step 5: Test customer balance checks
        Console.WriteLine("\n4. Testing customer balance functionality...");
        foreach (var customer in createdCustomers)
        {
            var balanceResponse = await GetAsync<dynamic>($"/api/customers/{customer.Id}/balance");
            Assert.That(balanceResponse?.IsSuccess, Is.True);
            Assert.That(balanceResponse.Data, Is.EqualTo(0)); // New customers should have 0 balance
            Console.WriteLine($"✅ Customer {customer.Code} balance: ${balanceResponse.Data}");
        }

        // Step 6: Test customers by type
        Console.WriteLine("\n5. Testing customer filtering by type...");
        var corporateCustomersResponse = await GetAsync<dynamic>($"/api/customers/by-type/{Domain.Enums.CustomerType.Corporate}");
        Assert.That(corporateCustomersResponse?.IsSuccess, Is.True);
        Console.WriteLine($"✅ Corporate customers retrieved: {corporateCustomersResponse.Data.Count}");

        var individualCustomersResponse = await GetAsync<dynamic>($"/api/customers/by-type/{Domain.Enums.CustomerType.Individual}");
        Assert.That(individualCustomersResponse?.IsSuccess, Is.True);
        Console.WriteLine($"✅ Individual customers retrieved: {individualCustomersResponse.Data.Count}");

        // Step 7: Update a customer
        Console.WriteLine("\n6. Updating customer information...");
        var customerToUpdate = createdCustomers.First();
        var updateDto = new
        {
            Code = customerToUpdate.Code,
            Name = $"Updated {customerToUpdate.Name}",
            ContactPerson = "Updated Contact",
            Email = customerToUpdate.Email,
            Phone = "555-0199",
            Mobile = "555-0299",
            CustomerType = customerToUpdate.CustomerType,
            TaxId = "UPD123456",
            BillingAddress = "456 Updated St",
            BillingCity = "Updated City",
            BillingState = "UC",
            BillingPostalCode = "54321",
            BillingCountry = "USA",
            ShippingAddress = "789 Shipping Ave",
            ShippingCity = "Shipping Town",
            ShippingState = "ST",
            ShippingPostalCode = "98765",
            ShippingCountry = "USA",
            PaymentTerms = Domain.Enums.PaymentTerms.Net45,
            CreditLimit = customerToUpdate.CreditLimit * 1.5m,
            Notes = "Updated customer - Premium support"
        };

        var updateResponse = await PutAsync<object, dynamic>($"/api/customers/{customerToUpdate.Id}", updateDto);
        Assert.That(updateResponse?.IsSuccess, Is.True);
        Assert.That(updateResponse.Data.Name, Is.EqualTo(updateDto.Name));
        Console.WriteLine($"✅ Customer updated successfully: {updateResponse.Data.Name}");

        Console.WriteLine("\n=== Complete Sales Workflow Test PASSED ===");
    }

    [Test]
    public async Task MultiTenantIsolation_ShouldWorkCorrectly()
    {
        Console.WriteLine("=== Testing Multi-Tenant Data Isolation ===");

        // Create accounts for default tenant
        var defaultTenantAccount = new CreateAccountDto
        {
            AccountCode = "DEFAULT-001",
            AccountName = "Default Tenant Cash",
            AccountType = Domain.Enums.AccountType.Asset,
            Description = "Cash for default tenant"
        };

        var defaultResponse = await PostAsync<CreateAccountDto, ApiResponse<AccountDto>>("/api/accounts", defaultTenantAccount);
        Assert.That(defaultResponse?.IsSuccess, Is.True);
        Console.WriteLine($"✅ Account created for default tenant: {defaultResponse.Data.AccountCode}");

        // Switch to different tenant
        var differentTenantId = "87654321-4321-4321-4321-210987654321";
        Client.DefaultRequestHeaders.Remove("CompanyId");
        Client.DefaultRequestHeaders.Add("CompanyId", differentTenantId);

        // Create account for different tenant
        var differentTenantAccount = new CreateAccountDto
        {
            AccountCode = "DIFFERENT-001",
            AccountName = "Different Tenant Cash",
            AccountType = Domain.Enums.AccountType.Asset,
            Description = "Cash for different tenant"
        };

        var differentResponse = await PostAsync<CreateAccountDto, ApiResponse<AccountDto>>("/api/accounts", differentTenantAccount);
        Assert.That(differentResponse?.IsSuccess, Is.True);
        Console.WriteLine($"✅ Account created for different tenant: {differentResponse.Data.AccountCode}");

        // Verify different tenant only sees their own accounts
        var differentTenantAccountsList = await GetAsync<ApiResponse<IEnumerable<AccountSummaryDto>>>("/api/accounts");
        Assert.That(differentTenantAccountsList?.IsSuccess, Is.True);
        
        var differentAccounts = differentTenantAccountsList.Data.ToList();
        var hasDefaultAccount = differentAccounts.Any(a => a.Id == defaultResponse.Data.Id);
        Assert.That(hasDefaultAccount, Is.False, "Different tenant should not see default tenant's accounts");
        Console.WriteLine("✅ Multi-tenant isolation verified");

        // Switch back to default tenant and verify isolation
        Client.DefaultRequestHeaders.Remove("CompanyId");
        Client.DefaultRequestHeaders.Add("CompanyId", TestConstants.DefaultCompanyId);

        var defaultTenantAccountsList = await GetAsync<ApiResponse<IEnumerable<AccountSummaryDto>>>("/api/accounts");
        Assert.That(defaultTenantAccountsList?.IsSuccess, Is.True);
        
        var defaultAccounts = defaultTenantAccountsList.Data.ToList();
        var hasDifferentAccount = defaultAccounts.Any(a => a.Id == differentResponse.Data.Id);
        Assert.That(hasDifferentAccount, Is.False, "Default tenant should not see different tenant's accounts");
        Console.WriteLine("✅ Reverse multi-tenant isolation verified");

        Console.WriteLine("\n=== Multi-Tenant Data Isolation Test PASSED ===");
    }

    [Test]
    public async Task ApiHealthCheck_ShouldReturnAllEndpointsWorking()
    {
        Console.WriteLine("=== API Health Check Test ===");

        var endpoints = new[]
        {
            "/api/accounts",
            "/api/companies"
        };

        foreach (var endpoint in endpoints)
        {
            try
            {
                var response = await Client.GetAsync(endpoint);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), 
                           $"Endpoint {endpoint} returned {response.StatusCode}");
                Console.WriteLine($"✅ {endpoint} - OK");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Endpoint {endpoint} failed: {ex.Message}");
            }
        }

        Console.WriteLine("\n=== API Health Check PASSED ===");
    }
}
