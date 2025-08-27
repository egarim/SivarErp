using Sivar.Erp.Core.Application.DTOs.Accounting;
using Sivar.Erp.Core.Shared.Responses;

namespace Sivar.Erp.Core.Tests.Integration;

/// <summary>
/// Integration tests for Accounts API endpoints
/// </summary>
[TestFixture]
public class AccountsApiTests : ApiTestBase
{
    [Test]
    public async Task GetAccounts_ShouldReturnSuccessResponse()
    {
        // Act
        var response = await GetAsync<ApiResponse<IEnumerable<AccountSummaryDto>>>("/api/accounts");

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        
        Console.WriteLine($"Retrieved {response.Data.Count()} accounts");
    }

    [Test]
    public async Task CreateAccount_ShouldCreateAndReturnAccount()
    {
        // Arrange
        var createAccountDto = new CreateAccountDto
        {
            Code = "TEST001",
            Name = "Test Account for Integration",
            Type = Domain.Enums.AccountType.Asset,
            Category = Domain.Enums.AccountCategory.CurrentAssets,
            Description = "Test account created by integration test"
        };

        // Act
        var response = await PostAsync<CreateAccountDto, ApiResponse<AccountDto>>("/api/accounts", createAccountDto);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data.AccountCode, Is.EqualTo(createAccountDto.AccountCode));
        Assert.That(response.Data.AccountName, Is.EqualTo(createAccountDto.AccountName));
        Assert.That(response.Data.AccountType, Is.EqualTo(createAccountDto.AccountType));
        
        Console.WriteLine($"Created account with ID: {response.Data.Id}");
    }

    [Test]
    public async Task CreateAccount_ThenGetById_ShouldReturnSameAccount()
    {
        // Arrange
        var createAccountDto = new CreateAccountDto
        {
            AccountCode = "TEST002",
            AccountName = "Test Account for Get By ID",
            AccountType = Domain.Enums.AccountType.Liability,
            Description = "Test account for ID retrieval test"
        };

        // Act - Create
        var createResponse = await PostAsync<CreateAccountDto, ApiResponse<AccountDto>>("/api/accounts", createAccountDto);
        
        Assert.That(createResponse?.IsSuccess, Is.True);
        Assert.That(createResponse?.Data?.Id, Is.Not.Null);

        // Act - Get by ID
        var getResponse = await GetAsync<ApiResponse<AccountDto>>($"/api/accounts/{createResponse.Data.Id}");

        // Assert
        Assert.That(getResponse, Is.Not.Null);
        Assert.That(getResponse.IsSuccess, Is.True);
        Assert.That(getResponse.Data, Is.Not.Null);
        Assert.That(getResponse.Data.Id, Is.EqualTo(createResponse.Data.Id));
        Assert.That(getResponse.Data.AccountCode, Is.EqualTo(createAccountDto.AccountCode));
        Assert.That(getResponse.Data.AccountName, Is.EqualTo(createAccountDto.AccountName));
        
        Console.WriteLine($"Successfully retrieved account: {getResponse.Data.AccountName}");
    }

    [Test]
    public async Task CreateMultipleAccounts_ShouldAllAppearInAccountsList()
    {
        // Arrange
        var accounts = new[]
        {
            new CreateAccountDto
            {
                AccountCode = "ASSET001",
                AccountName = "Cash in Bank",
                AccountType = Domain.Enums.AccountType.Asset,
                Description = "Main bank account"
            },
            new CreateAccountDto
            {
                AccountCode = "LIAB001", 
                AccountName = "Accounts Payable",
                AccountType = Domain.Enums.AccountType.Liability,
                Description = "Trade creditors"
            },
            new CreateAccountDto
            {
                AccountCode = "REV001",
                AccountName = "Sales Revenue", 
                AccountType = Domain.Enums.AccountType.Revenue,
                Description = "Product sales"
            }
        };

        // Act - Create accounts
        var createdAccounts = new List<AccountDto>();
        foreach (var account in accounts)
        {
            var response = await PostAsync<CreateAccountDto, ApiResponse<AccountDto>>("/api/accounts", account);
            Assert.That(response?.IsSuccess, Is.True);
            createdAccounts.Add(response.Data);
        }

        // Act - Get all accounts
        var allAccountsResponse = await GetAsync<ApiResponse<IEnumerable<AccountSummaryDto>>>("/api/accounts");

        // Assert
        Assert.That(allAccountsResponse?.IsSuccess, Is.True);
        Assert.That(allAccountsResponse?.Data, Is.Not.Null);
        
        var accountsList = allAccountsResponse.Data.ToList();
        
        // Verify all created accounts are in the list
        foreach (var createdAccount in createdAccounts)
        {
            var foundAccount = accountsList.FirstOrDefault(a => a.Id == createdAccount.Id);
            Assert.That(foundAccount, Is.Not.Null, $"Account {createdAccount.AccountCode} not found in accounts list");
            Assert.That(foundAccount.AccountCode, Is.EqualTo(createdAccount.AccountCode));
        }
        
        Console.WriteLine($"Successfully verified {createdAccounts.Count} accounts in the accounts list");
    }

    [Test]
    public async Task GetNonExistentAccount_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var response = await Client.GetAsync($"/api/accounts/{nonExistentId}");
        
        // Should return 404 or a failure response
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound)
                    .Or.EqualTo(System.Net.HttpStatusCode.BadRequest));
        
        Console.WriteLine($"Correctly handled non-existent account request: {response.StatusCode}");
    }
}
