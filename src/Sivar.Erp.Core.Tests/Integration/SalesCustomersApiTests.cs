using NUnit.Framework;
using System.Net;
using System.Text;
using System.Text.Json;
using Sivar.Erp.Core.Application.DTOs.Sales;
using Sivar.Erp.Core.Domain.Enums;
using Sivar.Erp.Core.Shared.Responses;

namespace Sivar.Erp.Core.Tests.Integration;

/// <summary>
/// Integration tests for Sales (Customers) API functionality
/// </summary>
[TestFixture]
public class SalesCustomersApiTests : ApiTestBase
{
    [Test]
    public async Task Sales_CustomersApi_ShouldWork()
    {
        // 1. Health check
        Console.WriteLine("✅ Testing Customers API health check...");
        var healthResponse = await Client.GetAsync("/api/customers/health");
        Assert.That(healthResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Console.WriteLine("✅ Customers API health check passed");

        // 2. Get initial customers (should be 0)
        var initialCustomersResponse = await Client.GetAsync("/api/customers");
        Assert.That(initialCustomersResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var initialCustomersContent = await initialCustomersResponse.Content.ReadAsStringAsync();
        var initialCustomersApiResponse = JsonSerializer.Deserialize<ApiResponse<List<CustomerSummaryDto>>>(
            initialCustomersContent, JsonOptions);

        Assert.That(initialCustomersApiResponse, Is.Not.Null);
        Assert.That(initialCustomersApiResponse.IsSuccess, Is.True);
        Console.WriteLine($"✅ Retrieved {initialCustomersApiResponse.Data!.Count} initial customers");

        // 3. Create a new customer
        var createCustomerDto = new CreateCustomerDto
        {
            Code = "CUST001",
            Name = "Test Customer Inc.",
            ContactPerson = "John Smith",
            Email = "john.smith@testcustomer.com",
            Phone = "555-0123",
            Mobile = "555-0124",
            CustomerType = CustomerType.Corporate,
            TaxId = "TAX123456",
            BillingAddress = "123 Business Ave",
            BillingCity = "Business City",
            BillingState = "BC",
            BillingPostalCode = "12345",
            BillingCountry = "USA",
            ShippingAddress = "456 Shipping St",
            ShippingCity = "Shipping City",
            ShippingState = "SC",
            ShippingPostalCode = "67890",
            ShippingCountry = "USA",
            PaymentTerms = PaymentTerms.Net30,
            CreditLimit = 50000.00m,
            Notes = "VIP Customer - Priority Support"
        };

        var createCustomerJson = JsonSerializer.Serialize(createCustomerDto, JsonOptions);
        var createCustomerContent = new StringContent(createCustomerJson, Encoding.UTF8, "application/json");

        var createCustomerResponse = await Client.PostAsync("/api/customers", createCustomerContent);
        Assert.That(createCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var createCustomerResponseContent = await createCustomerResponse.Content.ReadAsStringAsync();
        var createCustomerApiResponse = JsonSerializer.Deserialize<ApiResponse<CustomerDto>>(
            createCustomerResponseContent, JsonOptions);

        Assert.That(createCustomerApiResponse, Is.Not.Null);
        Assert.That(createCustomerApiResponse.IsSuccess, Is.True);
        Assert.That(createCustomerApiResponse.Data, Is.Not.Null);
        
        var createdCustomer = createCustomerApiResponse.Data;
        Assert.That(createdCustomer.Code, Is.EqualTo(createCustomerDto.Code));
        Assert.That(createdCustomer.Name, Is.EqualTo(createCustomerDto.Name));
        Assert.That(createdCustomer.CustomerType, Is.EqualTo(createCustomerDto.CustomerType));
        Assert.That(createdCustomer.CreditLimit, Is.EqualTo(createCustomerDto.CreditLimit));
        Assert.That(createdCustomer.IsActive, Is.True);

        Console.WriteLine($"✅ Customer created successfully: {createdCustomer.Code} - {createdCustomer.Name}");

        // 4. Get customer by ID
        var getCustomerResponse = await Client.GetAsync($"/api/customers/{createdCustomer.Id}");
        Assert.That(getCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var getCustomerContent = await getCustomerResponse.Content.ReadAsStringAsync();
        var getCustomerApiResponse = JsonSerializer.Deserialize<ApiResponse<CustomerDto>>(
            getCustomerContent, JsonOptions);

        Assert.That(getCustomerApiResponse, Is.Not.Null);
        Assert.That(getCustomerApiResponse.IsSuccess, Is.True);
        Assert.That(getCustomerApiResponse.Data!.Id, Is.EqualTo(createdCustomer.Id));
        Console.WriteLine($"✅ Customer retrieved by ID: {getCustomerApiResponse.Data.Code}");

        // 5. Get customer by code
        var getCustomerByCodeResponse = await Client.GetAsync($"/api/customers/by-code/{createdCustomer.Code}");
        Assert.That(getCustomerByCodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var getCustomerByCodeContent = await getCustomerByCodeResponse.Content.ReadAsStringAsync();
        var getCustomerByCodeApiResponse = JsonSerializer.Deserialize<ApiResponse<CustomerDto>>(
            getCustomerByCodeContent, JsonOptions);

        Assert.That(getCustomerByCodeApiResponse, Is.Not.Null);
        Assert.That(getCustomerByCodeApiResponse.IsSuccess, Is.True);
        Assert.That(getCustomerByCodeApiResponse.Data!.Code, Is.EqualTo(createdCustomer.Code));
        Console.WriteLine($"✅ Customer retrieved by code: {getCustomerByCodeApiResponse.Data.Code}");

        // 6. Get customer balance
        var getBalanceResponse = await Client.GetAsync($"/api/customers/{createdCustomer.Id}/balance");
        Assert.That(getBalanceResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var getBalanceContent = await getBalanceResponse.Content.ReadAsStringAsync();
        var getBalanceApiResponse = JsonSerializer.Deserialize<ApiResponse<decimal>>(
            getBalanceContent, JsonOptions);

        Assert.That(getBalanceApiResponse, Is.Not.Null);
        Assert.That(getBalanceApiResponse.IsSuccess, Is.True);
        Assert.That(getBalanceApiResponse.Data, Is.EqualTo(0)); // New customer should have 0 balance
        Console.WriteLine($"✅ Customer balance retrieved: ${getBalanceApiResponse.Data}");

        // 7. Get customers by type
        var getByTypeResponse = await Client.GetAsync($"/api/customers/by-type/{CustomerType.Corporate}");
        Assert.That(getByTypeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var getByTypeContent = await getByTypeResponse.Content.ReadAsStringAsync();
        var getByTypeApiResponse = JsonSerializer.Deserialize<ApiResponse<List<CustomerSummaryDto>>>(
            getByTypeContent, JsonOptions);

        Assert.That(getByTypeApiResponse, Is.Not.Null);
        Assert.That(getByTypeApiResponse.IsSuccess, Is.True);
        Assert.That(getByTypeApiResponse.Data!.Count, Is.GreaterThanOrEqualTo(1));
        
        var ourCustomer = getByTypeApiResponse.Data.FirstOrDefault(c => c.Code == createdCustomer.Code);
        Assert.That(ourCustomer, Is.Not.Null);
        Console.WriteLine($"✅ Customer found in Corporate customers list: {ourCustomer.Code}");

        // 8. Update customer
        var updateCustomerDto = new CreateCustomerDto
        {
            Code = createCustomerDto.Code,
            Name = "Updated Test Customer Inc.",
            ContactPerson = "Jane Smith",
            Email = "jane.smith@testcustomer.com",
            Phone = "555-0125",
            Mobile = "555-0126",
            CustomerType = CustomerType.Corporate,
            TaxId = createCustomerDto.TaxId,
            BillingAddress = createCustomerDto.BillingAddress,
            BillingCity = createCustomerDto.BillingCity,
            BillingState = createCustomerDto.BillingState,
            BillingPostalCode = createCustomerDto.BillingPostalCode,
            BillingCountry = createCustomerDto.BillingCountry,
            ShippingAddress = createCustomerDto.ShippingAddress,
            ShippingCity = createCustomerDto.ShippingCity,
            ShippingState = createCustomerDto.ShippingState,
            ShippingPostalCode = createCustomerDto.ShippingPostalCode,
            ShippingCountry = createCustomerDto.ShippingCountry,
            PaymentTerms = PaymentTerms.Net45,
            CreditLimit = 75000.00m,
            Notes = "Premium VIP Customer - 24/7 Support"
        };

        var updateCustomerJson = JsonSerializer.Serialize(updateCustomerDto, JsonOptions);
        var updateCustomerContent = new StringContent(updateCustomerJson, Encoding.UTF8, "application/json");

        var updateCustomerResponse = await Client.PutAsync($"/api/customers/{createdCustomer.Id}", updateCustomerContent);
        Assert.That(updateCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var updateCustomerResponseContent = await updateCustomerResponse.Content.ReadAsStringAsync();
        var updateCustomerApiResponse = JsonSerializer.Deserialize<ApiResponse<CustomerDto>>(
            updateCustomerResponseContent, JsonOptions);

        Assert.That(updateCustomerApiResponse, Is.Not.Null);
        Assert.That(updateCustomerApiResponse.IsSuccess, Is.True);
        Assert.That(updateCustomerApiResponse.Data!.Name, Is.EqualTo(updateCustomerDto.Name));
        Assert.That(updateCustomerApiResponse.Data.ContactPerson, Is.EqualTo(updateCustomerDto.ContactPerson));
        Assert.That(updateCustomerApiResponse.Data.CreditLimit, Is.EqualTo(updateCustomerDto.CreditLimit));
        Assert.That(updateCustomerApiResponse.Data.PaymentTerms, Is.EqualTo(updateCustomerDto.PaymentTerms));

        Console.WriteLine($"✅ Customer updated successfully: {updateCustomerApiResponse.Data.Name}");

        // 9. Get all customers again to verify our customer is included
        var finalCustomersResponse = await Client.GetAsync("/api/customers");
        Assert.That(finalCustomersResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var finalCustomersContent = await finalCustomersResponse.Content.ReadAsStringAsync();
        var finalCustomersApiResponse = JsonSerializer.Deserialize<ApiResponse<List<CustomerSummaryDto>>>(
            finalCustomersContent, JsonOptions);

        Assert.That(finalCustomersApiResponse, Is.Not.Null);
        Assert.That(finalCustomersApiResponse.IsSuccess, Is.True);
        Assert.That(finalCustomersApiResponse.Data!.Count, Is.EqualTo(initialCustomersApiResponse.Data!.Count + 1));

        var finalCustomer = finalCustomersApiResponse.Data.FirstOrDefault(c => c.Code == createdCustomer.Code);
        Assert.That(finalCustomer, Is.Not.Null);
        Assert.That(finalCustomer.Name, Is.EqualTo(updateCustomerDto.Name));
        Console.WriteLine($"✅ Customers list contains our updated customer - Total customers: {finalCustomersApiResponse.Data.Count}");

        // 10. Test duplicate code validation
        var duplicateCustomerDto = new CreateCustomerDto
        {
            Code = createCustomerDto.Code, // Same code
            Name = "Duplicate Customer",
            CustomerType = CustomerType.Individual
        };

        var duplicateCustomerJson = JsonSerializer.Serialize(duplicateCustomerDto, JsonOptions);
        var duplicateCustomerContent = new StringContent(duplicateCustomerJson, Encoding.UTF8, "application/json");

        var duplicateCustomerResponse = await Client.PostAsync("/api/customers", duplicateCustomerContent);
        Assert.That(duplicateCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var duplicateCustomerResponseContent = await duplicateCustomerResponse.Content.ReadAsStringAsync();
        var duplicateCustomerApiResponse = JsonSerializer.Deserialize<ApiResponse<CustomerDto>>(
            duplicateCustomerResponseContent, JsonOptions);

        Assert.That(duplicateCustomerApiResponse, Is.Not.Null);
        Assert.That(duplicateCustomerApiResponse.IsSuccess, Is.False);
        Assert.That(duplicateCustomerApiResponse.Message, Does.Contain("already exists"));
        Console.WriteLine($"✅ Duplicate code validation working correctly");

        Console.WriteLine("✅ Sales Customers API functionality test passed!");
    }
}
