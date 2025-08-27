using Sivar.Erp.Core.Shared.DTOs.Identity;
using Sivar.Erp.Core.Shared.Responses;

namespace Sivar.Erp.Core.Tests.Integration;

/// <summary>
/// Integration tests for Companies API endpoints
/// </summary>
[TestFixture]
public class CompaniesApiTests : ApiTestBase
{
    [Test]
    public async Task GetCompanies_ShouldReturnSuccessResponse()
    {
        // Act
        var response = await GetAsync<ApiResponse<IEnumerable<CompanyDto>>>("/api/companies");

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        
        Console.WriteLine($"Retrieved {response.Data.Count()} companies");
    }

    [Test]
    public async Task CreateCompany_ShouldCreateAndReturnCompany()
    {
        // Arrange
        var createCompanyDto = new CreateCompanyDto
        {
            Name = "Test Company Integration",
            TaxId = "TEST-TAX-123",
            Email = "test@integration.com",
            Phone = "+503-1234-5678",
            Address = "123 Test Street, Test City"
        };

        // Act
        var response = await PostAsync<CreateCompanyDto, ApiResponse<CompanyDto>>("/api/companies", createCompanyDto);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data.Name, Is.EqualTo(createCompanyDto.Name));
        Assert.That(response.Data.TaxId, Is.EqualTo(createCompanyDto.TaxId));
        Assert.That(response.Data.Email, Is.EqualTo(createCompanyDto.Email));
        
        Console.WriteLine($"Created company with ID: {response.Data.Id}");
    }

    [Test]
    public async Task CreateCompany_ThenGetById_ShouldReturnSameCompany()
    {
        // Arrange
        var createCompanyDto = new CreateCompanyDto
        {
            Name = "Test Company for Get By ID",
            TaxId = "TEST-TAX-456",
            Email = "getbyid@test.com",
            Phone = "+503-8765-4321",
            Address = "456 GetById Avenue, Test City"
        };

        // Act - Create
        var createResponse = await PostAsync<CreateCompanyDto, ApiResponse<CompanyDto>>("/api/companies", createCompanyDto);
        
        Assert.That(createResponse?.IsSuccess, Is.True);
        Assert.That(createResponse?.Data?.Id, Is.Not.Null);

        // Act - Get by ID
        var getResponse = await GetAsync<ApiResponse<CompanyDto>>($"/api/companies/{createResponse.Data.Id}");

        // Assert
        Assert.That(getResponse, Is.Not.Null);
        Assert.That(getResponse.IsSuccess, Is.True);
        Assert.That(getResponse.Data, Is.Not.Null);
        Assert.That(getResponse.Data.Id, Is.EqualTo(createResponse.Data.Id));
        Assert.That(getResponse.Data.Name, Is.EqualTo(createCompanyDto.Name));
        Assert.That(getResponse.Data.TaxId, Is.EqualTo(createCompanyDto.TaxId));
        
        Console.WriteLine($"Successfully retrieved company: {getResponse.Data.Name}");
    }

    [Test]
    public async Task UpdateCompany_ShouldModifyAndReturnUpdatedCompany()
    {
        // Arrange - Create a company first
        var createCompanyDto = new CreateCompanyDto
        {
            Name = "Original Company Name",
            TaxId = "ORIG-TAX-789",
            Email = "original@test.com",
            Phone = "+503-1111-2222",
            Address = "789 Original Street"
        };

        var createResponse = await PostAsync<CreateCompanyDto, ApiResponse<CompanyDto>>("/api/companies", createCompanyDto);
        Assert.That(createResponse?.IsSuccess, Is.True);

        // Arrange - Update data
        var updateCompanyDto = new UpdateCompanyDto
        {
            Name = "Updated Company Name",
            Email = "updated@test.com",
            Phone = "+503-3333-4444",
            Address = "789 Updated Avenue"
        };

        // Act
        var updateResponse = await PutAsync<UpdateCompanyDto, ApiResponse<CompanyDto>>(
            $"/api/companies/{createResponse.Data.Id}", 
            updateCompanyDto);

        // Assert
        Assert.That(updateResponse, Is.Not.Null);
        Assert.That(updateResponse.IsSuccess, Is.True);
        Assert.That(updateResponse.Data, Is.Not.Null);
        Assert.That(updateResponse.Data.Id, Is.EqualTo(createResponse.Data.Id));
        Assert.That(updateResponse.Data.Name, Is.EqualTo(updateCompanyDto.Name));
        Assert.That(updateResponse.Data.Email, Is.EqualTo(updateCompanyDto.Email));
        Assert.That(updateResponse.Data.Phone, Is.EqualTo(updateCompanyDto.Phone));
        Assert.That(updateResponse.Data.TaxId, Is.EqualTo(createCompanyDto.TaxId)); // Should remain unchanged
        
        Console.WriteLine($"Successfully updated company: {updateResponse.Data.Name}");
    }

    [Test]
    public async Task DeleteCompany_ShouldRemoveCompanyFromSystem()
    {
        // Arrange - Create a company to delete
        var createCompanyDto = new CreateCompanyDto
        {
            Name = "Company To Delete",
            TaxId = "DELETE-TAX-999",
            Email = "delete@test.com",
            Phone = "+503-9999-0000",
            Address = "999 Delete Street"
        };

        var createResponse = await PostAsync<CreateCompanyDto, ApiResponse<CompanyDto>>("/api/companies", createCompanyDto);
        Assert.That(createResponse?.IsSuccess, Is.True);

        // Act - Delete the company
        await DeleteAsync($"/api/companies/{createResponse.Data.Id}");

        // Assert - Try to get the deleted company (should fail)
        var getResponse = await Client.GetAsync($"/api/companies/{createResponse.Data.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound)
                    .Or.EqualTo(System.Net.HttpStatusCode.BadRequest));
        
        Console.WriteLine($"Successfully deleted company with ID: {createResponse.Data.Id}");
    }

    [Test]
    public async Task CreateMultipleCompanies_ShouldAllAppearInCompaniesList()
    {
        // Arrange
        var companies = new[]
        {
            new CreateCompanyDto
            {
                Name = "Tech Solutions Inc",
                TaxId = "TECH-001",
                Email = "info@techsolutions.com",
                Phone = "+503-1000-0001",
                Address = "100 Tech Plaza"
            },
            new CreateCompanyDto
            {
                Name = "Manufacturing Corp",
                TaxId = "MANUF-002", 
                Email = "contact@manufacturing.com",
                Phone = "+503-2000-0002",
                Address = "200 Industrial Blvd"
            },
            new CreateCompanyDto
            {
                Name = "Service Providers Ltd",
                TaxId = "SERV-003",
                Email = "hello@serviceproviders.com",
                Phone = "+503-3000-0003",
                Address = "300 Service Avenue"
            }
        };

        // Act - Create companies
        var createdCompanies = new List<CompanyDto>();
        foreach (var company in companies)
        {
            var response = await PostAsync<CreateCompanyDto, ApiResponse<CompanyDto>>("/api/companies", company);
            Assert.That(response?.IsSuccess, Is.True);
            createdCompanies.Add(response.Data);
        }

        // Act - Get all companies
        var allCompaniesResponse = await GetAsync<ApiResponse<IEnumerable<CompanyDto>>>("/api/companies");

        // Assert
        Assert.That(allCompaniesResponse?.IsSuccess, Is.True);
        Assert.That(allCompaniesResponse?.Data, Is.Not.Null);
        
        var companiesList = allCompaniesResponse.Data.ToList();
        
        // Verify all created companies are in the list
        foreach (var createdCompany in createdCompanies)
        {
            var foundCompany = companiesList.FirstOrDefault(c => c.Id == createdCompany.Id);
            Assert.That(foundCompany, Is.Not.Null, $"Company {createdCompany.Name} not found in companies list");
            Assert.That(foundCompany.TaxId, Is.EqualTo(createdCompany.TaxId));
        }
        
        Console.WriteLine($"Successfully verified {createdCompanies.Count} companies in the companies list");
    }
}
