using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Application.DTOs.Sales;
using Sivar.Erp.Core.Domain.Entities.Sales;
using Sivar.Erp.Core.Domain.Enums;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.Sales;
using Sivar.Erp.Core.Domain.Interfaces;
using Sivar.Erp.Core.Shared.Responses;

namespace Sivar.Erp.Core.Application.Services.Sales;

/// <summary>
/// Customer service for managing customer operations
/// </summary>
public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(Guid companyId, CreateCustomerDto dto)
    {
        try
        {
            // Validate code uniqueness
            if (!await _customerRepository.IsCodeUniqueAsync(companyId, dto.Code))
            {
                return ApiResponse<CustomerDto>.Failure($"Customer code '{dto.Code}' already exists");
            }

            var customer = new Customer
            {
                CompanyId = companyId,
                Code = dto.Code,
                Name = dto.Name,
                ContactPerson = dto.ContactPerson,
                Email = dto.Email,
                Phone = dto.Phone,
                Mobile = dto.Mobile,
                CustomerType = dto.CustomerType,
                TaxId = dto.TaxId,
                BillingAddress = dto.BillingAddress,
                BillingCity = dto.BillingCity,
                BillingState = dto.BillingState,
                BillingPostalCode = dto.BillingPostalCode,
                BillingCountry = dto.BillingCountry,
                ShippingAddress = dto.ShippingAddress,
                ShippingCity = dto.ShippingCity,
                ShippingState = dto.ShippingState,
                ShippingPostalCode = dto.ShippingPostalCode,
                ShippingCountry = dto.ShippingCountry,
                PaymentTerms = dto.PaymentTerms,
                CreditLimit = dto.CreditLimit,
                Notes = dto.Notes,
                IsActive = true
            };

            await _customerRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Customer created successfully: {CustomerCode} - {CustomerName}", 
                customer.Code, customer.Name);

            var customerDto = MapToDto(customer);
            return ApiResponse<CustomerDto>.Success(customerDto, "Customer created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer with code: {CustomerCode}", dto.Code);
            return ApiResponse<CustomerDto>.Failure("Failed to create customer");
        }
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    public async Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(Guid companyId, Guid customerId)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            
            if (customer == null || customer.CompanyId != companyId)
            {
                return ApiResponse<CustomerDto>.Failure("Customer not found");
            }

            var customerDto = MapToDto(customer);
            return ApiResponse<CustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer: {CustomerId}", customerId);
            return ApiResponse<CustomerDto>.Failure("Failed to retrieve customer");
        }
    }

    /// <summary>
    /// Get customer by code
    /// </summary>
    public async Task<ApiResponse<CustomerDto>> GetCustomerByCodeAsync(Guid companyId, string code)
    {
        try
        {
            var customer = await _customerRepository.GetByCodeAsync(companyId, code);
            
            if (customer == null)
            {
                return ApiResponse<CustomerDto>.Failure("Customer not found");
            }

            var customerDto = MapToDto(customer);
            return ApiResponse<CustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer by code: {CustomerCode}", code);
            return ApiResponse<CustomerDto>.Failure("Failed to retrieve customer");
        }
    }

    /// <summary>
    /// Get all customers for a company
    /// </summary>
    public async Task<ApiResponse<List<CustomerSummaryDto>>> GetCustomersAsync(Guid companyId, bool activeOnly = true)
    {
        try
        {
            var customers = activeOnly 
                ? await _customerRepository.GetActiveCustomersAsync(companyId)
                : (await _customerRepository.GetByCompanyAsync(companyId)).Where(c => c.CompanyId == companyId);

            var customerDtos = customers.Select(MapToSummaryDto).ToList();

            return ApiResponse<List<CustomerSummaryDto>>.Success(customerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers for company: {CompanyId}", companyId);
            return ApiResponse<List<CustomerSummaryDto>>.Failure("Failed to retrieve customers");
        }
    }

    /// <summary>
    /// Get customers by type
    /// </summary>
    public async Task<ApiResponse<List<CustomerSummaryDto>>> GetCustomersByTypeAsync(Guid companyId, CustomerType customerType)
    {
        try
        {
            var customers = await _customerRepository.GetCustomersByTypeAsync(companyId, customerType);
            var customerDtos = customers.Select(MapToSummaryDto).ToList();

            return ApiResponse<List<CustomerSummaryDto>>.Success(customerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers by type: {CustomerType}", customerType);
            return ApiResponse<List<CustomerSummaryDto>>.Failure("Failed to retrieve customers");
        }
    }

    /// <summary>
    /// Update customer
    /// </summary>
    public async Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(Guid companyId, Guid customerId, CreateCustomerDto dto)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            
            if (customer == null || customer.CompanyId != companyId)
            {
                return ApiResponse<CustomerDto>.Failure("Customer not found");
            }

            // Validate code uniqueness (excluding current customer)
            if (!await _customerRepository.IsCodeUniqueAsync(companyId, dto.Code, customerId))
            {
                return ApiResponse<CustomerDto>.Failure($"Customer code '{dto.Code}' already exists");
            }

            // Update customer properties
            customer.Code = dto.Code;
            customer.Name = dto.Name;
            customer.ContactPerson = dto.ContactPerson;
            customer.Email = dto.Email;
            customer.Phone = dto.Phone;
            customer.Mobile = dto.Mobile;
            customer.CustomerType = dto.CustomerType;
            customer.TaxId = dto.TaxId;
            customer.BillingAddress = dto.BillingAddress;
            customer.BillingCity = dto.BillingCity;
            customer.BillingState = dto.BillingState;
            customer.BillingPostalCode = dto.BillingPostalCode;
            customer.BillingCountry = dto.BillingCountry;
            customer.ShippingAddress = dto.ShippingAddress;
            customer.ShippingCity = dto.ShippingCity;
            customer.ShippingState = dto.ShippingState;
            customer.ShippingPostalCode = dto.ShippingPostalCode;
            customer.ShippingCountry = dto.ShippingCountry;
            customer.PaymentTerms = dto.PaymentTerms;
            customer.CreditLimit = dto.CreditLimit;
            customer.Notes = dto.Notes;

            await _customerRepository.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Customer updated successfully: {CustomerCode} - {CustomerName}", 
                customer.Code, customer.Name);

            var customerDto = MapToDto(customer);
            return ApiResponse<CustomerDto>.Success(customerDto, "Customer updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer: {CustomerId}", customerId);
            return ApiResponse<CustomerDto>.Failure("Failed to update customer");
        }
    }

    /// <summary>
    /// Delete customer (soft delete)
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteCustomerAsync(Guid companyId, Guid customerId)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            
            if (customer == null || customer.CompanyId != companyId)
            {
                return ApiResponse<bool>.Failure("Customer not found");
            }

            // Check if customer has orders (business rule)
            var customerWithOrders = await _customerRepository.GetCustomerWithOrdersAsync(companyId, customerId);
            if (customerWithOrders?.SalesOrders.Any() == true)
            {
                return ApiResponse<bool>.Failure("Cannot delete customer with existing sales orders");
            }

            customer.IsActive = false;
            await _customerRepository.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Customer deactivated successfully: {CustomerCode}", customer.Code);

            return ApiResponse<bool>.Success(true, "Customer deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer: {CustomerId}", customerId);
            return ApiResponse<bool>.Failure("Failed to delete customer");
        }
    }

    /// <summary>
    /// Get customer balance
    /// </summary>
    public async Task<ApiResponse<decimal>> GetCustomerBalanceAsync(Guid companyId, Guid customerId)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            
            if (customer == null || customer.CompanyId != companyId)
            {
                return ApiResponse<decimal>.Failure("Customer not found");
            }

            var balance = await _customerRepository.GetCustomerBalanceAsync(companyId, customerId);
            return ApiResponse<decimal>.Success(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer balance: {CustomerId}", customerId);
            return ApiResponse<decimal>.Failure("Failed to retrieve customer balance");
        }
    }

    #region Private Methods

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            CompanyId = customer.CompanyId,
            Code = customer.Code,
            Name = customer.Name,
            ContactPerson = customer.ContactPerson,
            Email = customer.Email,
            Phone = customer.Phone,
            Mobile = customer.Mobile,
            CustomerType = customer.CustomerType,
            CustomerTypeText = customer.CustomerType.ToString(),
            TaxId = customer.TaxId,
            BillingAddress = customer.BillingAddress,
            BillingCity = customer.BillingCity,
            BillingState = customer.BillingState,
            BillingPostalCode = customer.BillingPostalCode,
            BillingCountry = customer.BillingCountry,
            ShippingAddress = customer.ShippingAddress,
            ShippingCity = customer.ShippingCity,
            ShippingState = customer.ShippingState,
            ShippingPostalCode = customer.ShippingPostalCode,
            ShippingCountry = customer.ShippingCountry,
            PaymentTerms = customer.PaymentTerms,
            PaymentTermsText = $"Net {(int)customer.PaymentTerms}",
            CreditLimit = customer.CreditLimit,
            CurrentBalance = customer.CurrentBalance,
            IsActive = customer.IsActive,
            Notes = customer.Notes,
            CreatedAt = customer.CreatedAt,
            ModifiedAt = customer.UpdatedAt
        };
    }

    private static CustomerSummaryDto MapToSummaryDto(Customer customer)
    {
        return new CustomerSummaryDto
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            ContactPerson = customer.ContactPerson,
            Email = customer.Email,
            Phone = customer.Phone,
            CustomerType = customer.CustomerType,
            CustomerTypeText = customer.CustomerType.ToString(),
            CreditLimit = customer.CreditLimit,
            CurrentBalance = customer.CurrentBalance,
            IsActive = customer.IsActive
        };
    }

    #endregion
}
